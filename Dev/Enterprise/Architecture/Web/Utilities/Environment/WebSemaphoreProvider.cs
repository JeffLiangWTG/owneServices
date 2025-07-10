using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	class WebDummySemaphoreProvider : ISemaphoreProvider
	{
		#region ISemaphoreProvider Members

		public ISemaphoreHandle CreateSemaphoreHandle(ISemaphoreType semaphore)
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			return new DummySemaphoreHandle();
		}

		public void DisposeHeartbeat()
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
		}

		public ISemaphoreInfo[] GetActiveSemaphoreHandles(ISemaphoreType semaphore)
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			return Array.Empty<ISemaphoreInfo>();
		}

		public ISemaphoreInfo[] GetActiveSemaphoreHandles(Guid heartbeatId)
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			return Array.Empty<ISemaphoreInfo>();
		}

		public HashSet<ZGuid> GetActiveSemaphoreLockInfo(string lockInfoPrefix)
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			return new HashSet<ZGuid>();
		}

		public ISemaphoreInfo[] GetRemoteActiveSemaphoreHandles(ISemaphoreType semaphore, Guid userPk, string clientIdentifier)
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			return Array.Empty<ISemaphoreInfo>();
		}

		public void RemoteLogoff(Guid userPk, string heartbeatType, string clientIdentifier)
		{
		}

		public void ReleaseLocks(string lockInfo, string heartbeatType, Guid userPk)
		{
		}

		public void RemoveSemaphore(ISemaphoreType semaphore)
		{
		}

		public IHeartbeat InternalHeartbeat
		{
			get
			{
				return null;
			}
		}

		#endregion
	}

	class DummySemaphoreHandle : ISemaphoreHandle
	{
		#region ISemaphoreHandle Members

		public Exception CreateException
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			get { return null; }
		}

		public ISemaphoreType Semaphore
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			get { return null; }
		}

		public bool Success
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
			get { return false; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			// SEMAPHORES NOT YET IMPLEMENTED FOR WEB
		}

		#endregion
	}
}
