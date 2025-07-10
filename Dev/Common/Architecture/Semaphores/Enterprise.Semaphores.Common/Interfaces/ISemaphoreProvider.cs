using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Semaphores.Common
{
	public interface ISemaphoreProvider
	{
		/// <summary>
		/// Tries to create a semaphore handle for a given semaphore type.
		/// </summary>
		/// <param name="semaphore">Identifier of the semaphore to be created.</param>
		/// <returns>The handle if successfully created or null if not.</returns>
		ISemaphoreHandle CreateSemaphoreHandle(ISemaphoreType semaphore);

		/// <summary>
		/// Gets an array of active semaphore handles for a given semaphore type.
		/// </summary>
		/// <param name="semaphore">Semaphore type identifier.</param>
		/// <returns>Array of active semaphore handles.</returns>
		ISemaphoreInfo[] GetActiveSemaphoreHandles(ISemaphoreType semaphore);

		/// <summary>
		/// Gets an array of active semaphore handles for a given user.
		/// </summary>
		/// <param name="heartbeatId">Session heartbeat identifier.</param>
		/// <returns>Array of active semaphore handles.</returns>
		ISemaphoreInfo[] GetActiveSemaphoreHandles(Guid heartbeatId);

		/// <summary>
		/// Removes any semaphore matching the given semaphore type.
		/// </summary>
		/// <param name="semaphore">Semaphore type identifier.</param>
		void RemoveSemaphore(ISemaphoreType semaphore);

		/// <summary>
		/// Gets an array of active semaphore lockinfo data for a given semaphore type.
		/// </summary>
		/// <param name="lockInfoPrefix">The prefix that preceeds the guid data in the lockinfo column</param>
		/// <returns>Array of ZGuid from the lockinfo column of matching rows</returns>
		HashSet<ZGuid> GetActiveSemaphoreLockInfo(string lockInfoPrefix);

		/// <summary>
		/// Gets an array of active semaphore handles for a given semaphore type and user ID
		/// for remote machines (i.e. machines other than the local machine)
		/// </summary>
		/// <param name="semaphore">Semaphore type identifier.</param>
		/// <param name="userPk">user primary key</param>
		/// <param name="clientIdentifier"> A unique identifier for the client machine.</param>
		/// <returns>Array of active semaphore handles.</returns>
		ISemaphoreInfo[] GetRemoteActiveSemaphoreHandles(ISemaphoreType semaphore, Guid userPk, string clientIdentifier);

		/// <summary>
		/// Logoff remote users
		/// </summary>
		/// <param name="userPk">user primary key</param>
		/// <param name="heartbeatType">type of heartbeat</param>
		void RemoteLogoff(Guid userPk, string heartbeatType, string clientIdentifier);

		/// <summary>
		/// Release all locks on the semaphore lockInfo
		/// </summary>
		/// <param name="lockInfo">value of lockInfo column</param>
		/// <param name="heartbeatType">type of heartbeat</param>
		/// <param name="userPk">user primary key</param>
		void ReleaseLocks(string lockInfo, string heartbeatType, Guid userPk);

		IHeartbeat InternalHeartbeat { get; }
	}
}
