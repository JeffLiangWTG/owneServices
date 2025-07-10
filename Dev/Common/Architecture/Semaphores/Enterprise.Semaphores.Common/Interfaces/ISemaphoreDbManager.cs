using System;

namespace Enterprise.Semaphores.Common
{
	public interface ISemaphoreDbManager
	{
		/// <summary>
		/// GetActiveSemaphoreHandles for Semaphore Type
		/// </summary>
		/// <param name="semaphore"></param>
		/// <returns></returns>
		ISemaphoreInfo[] GetActiveSemaphoreHandles(ISemaphoreType semaphore);

		/// <summary>
		/// GetActiveSemaphoreHandles for Semaphore Type
		/// </summary>
		/// <param name="heartbeatId"></param>
		/// <returns></returns>
		ISemaphoreInfo[] GetActiveSemaphoreHandles(Guid heartbeatId);

		/// <summary>
		/// GetRemoteActiveSemaphoreHandles for Semaphore type
		/// </summary>
		/// <param name="semaphore"></param>
		/// <param name="userPk"></param>
		/// <param name="hostName"></param>
		/// <param name="clientIdentifier"></param>
		/// <returns></returns>
		ISemaphoreInfo[] GetRemoteActiveSemaphoreHandles(ISemaphoreType semaphore, Guid userPk, string hostName, string clientIdentifier);

		bool RefreshHeartbeatInDatabase(Guid heartbeatUniqueId, TimeSpan heartbeatDuration, bool isNewConnection);
		void UpdateUserContext(Guid heartbeatUniqueId, string hostName, Guid userPk, string userParentTableCode, string heartbeatType);
		bool UserPkInDatabase(Guid userPk);
		bool CheckHeartbeatExpiredTimeIsExpired(Guid heartbeatUniqueId);
		void DeleteHeartbeatFromDatabase(Guid heartbeatUniqueId);
		void CreateHeartbeatInDatabase(Guid heartbeatUniqueId, string hostName, int sessionProcessId, Guid userPk, string userParentTableCode, int pulseInSeconds, string heartbeatType, string clientIdentifier);
	}
}
