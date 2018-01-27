﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {
	
	[SerializeField] ObjectPool[] chunkPools;
	private bool play = false;

	[Header("Spawner parameters")]
	[SerializeField] float chunkSize;
	[SerializeField] float chunkSpawnDistance;
	[SerializeField] float chunkDespawnDistance;
	[SerializeField] int simultaneousChunks;

	#region Variables
	private LinkedList<LevelChunk> activeChunks = new LinkedList<LevelChunk>();
	[SerializeField][Range(0.1f,4f)] private float moveSpeed = 1f;

	private Coroutine playCoroutine;

	#endregion

	public void TogglePlay() {
		play = !play;
	}

	#region MonoBehaviour messages

	private void Update(){

#if UNITY_EDITOR
/// FOR DEBUG PURPOSES
		if(Input.GetKeyDown(KeyCode.P)){
			TogglePlay();
		}
#endif
		if(play){
			ClearUnusedChunks();
			GenerateNewChunks();
			MoveActiveChunks();
		}
	}

	#endregion

	private void MoveActiveChunks(){
		foreach(var chunk in activeChunks){
			chunk.transform.position -= transform.forward * moveSpeed * Time.deltaTime;
		}
	}

	private void GenerateNewChunks(){
		if(activeChunks.Count < simultaneousChunks) {
			if(activeChunks.Count == 0) {
				GenerateChunk(transform.position + transform.forward * chunkSpawnDistance, Quaternion.identity);
			} else if (activeChunks.First.Value.transform.position.z + chunkSize < chunkSpawnDistance) 
			{
				GenerateChunk(transform.position + transform.forward * (activeChunks.First.Value.transform.position.z + chunkSize), Quaternion.identity);
			}
		}
	}

	// Clear the chunks that went past the chunkDespawnDistance
	private void ClearUnusedChunks(){
		if(
			activeChunks.Count > 0 &&
			activeChunks.Last.Value.transform.position.z < chunkDespawnDistance
		)
		{
			ClearChunk(activeChunks.Last.Value);
		}
	}

	#region Chunk methods

	private void GenerateChunk(Vector3 position, Quaternion rotation) {
		var newChunk = 
			chunkPools[Random.Range(0, chunkPools.Length)].GetObject().GetComponent<LevelChunk>();

		newChunk.transform.position = position;
		newChunk.transform.rotation = rotation;
		
		newChunk.OnSpawn();
		activeChunks.AddFirst(newChunk);
	}

	private void ClearChunk(LevelChunk chunk){
		activeChunks.Remove(chunk);
		chunk.OnDespawn();

		chunk.ReturnToPool();
	}

	#endregion
	
	#region Editor Gizmos
	public void OnDrawGizmos(){
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.color = Color.green;
		Gizmos.DrawLine(Vector3.forward * chunkSpawnDistance + Vector3.right * -2f, Vector3.forward * chunkSpawnDistance + Vector3.right * 2f);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(Vector3.forward * chunkDespawnDistance + Vector3.right * -2f, Vector3.forward * chunkDespawnDistance + Vector3.right * 2f);

		Gizmos.color = Color.yellow;
		for(int i = 0; i < simultaneousChunks; i++){
			var pos = Vector3.forward * (chunkDespawnDistance + chunkSize * i + chunkSize / 2f);
			Gizmos.DrawWireCube(pos, new Vector3(2f, 1f, chunkSize));
		}
	}

	#endregion
}