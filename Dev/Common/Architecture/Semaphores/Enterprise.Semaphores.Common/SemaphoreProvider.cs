using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Semaphores.Common
{
	public abstract class SemaphoreProvider : ISemaphoreProvider
	{
		readonly int attemptsToHandleDeadlocks;

		protected SemaphoreProvider(int? attemptsToHandleDeadlocks = null)
		{
			this.attemptsToHandleDeadlocks = attemptsToHandleDeadlocks ?? AttemptCountToHandleDeadLocks;
		}

		const int AttemptCountToHandleDeadLocks = 3;

		#region ISemaphoreProvider Members

		ISemaphoreHandle ISemaphoreProvider.CreateSemaphoreHandle(ISemaphoreType semaphore)
		{
			return CreateSemaphoreHandleCore(semaphore);
		}

		ISemaphoreInfo[] ISemaphoreProvider.GetActiveSemaphoreHandles(ISemaphoreType semaphore)
		{
			return dbManager.GetActiveSemaphoreHandles(semaphore);
		}

		void ISemaphoreProvider.RemoveSemaphore(ISemaphoreType semaphore)
		{
			dbManager.RemoveSemaphore(semaphore);
		}

		ISemaphoreInfo[] ISemaphoreProvider.GetActiveSemaphoreHandles(Guid heartbeatId)
		{
			return dbManager.GetActiveSemaphoreHandles(heartbeatId);
		}

		public HashSet<ZGuid> GetActiveSemaphoreLockInfo(string lockInfoPrefix)
		{
			return dbManager.GetActiveSemaphoreLockInfo(lockInfoPrefix);
		}

		ISemaphoreInfo[] ISemaphoreProvider.GetRemoteActiveSemaphoreHandles(ISemaphoreType semaphore, Guid userPk, string clientIdentifier)
		{
			return dbManager.GetRemoteActiveSemaphoreHandles(semaphore, userPk, LocalMachineName, clientIdentifier);
		}

		void ISemaphoreProvider.RemoteLogoff(Guid userPk, string heartbeatType, string clientIdentifier)
		{
			dbManager.RemoteLogoff(userPk, LocalMachineName, heartbeatType, clientIdentifier);
		}

		void ISemaphoreProvider.ReleaseLocks(string lockInfo, string heartbeatType, Guid userPk)
		{
			dbManager.ReleaseLocks(lockInfo, heartbeatType, userPk);
		}

		string LocalMachineName
		{
			get
			{
				return HeartbeatSessionInfoFactory.New().HostName;
			}
		}

		#endregion

		#region Internal Heartbeat

		public
#if DEBUG
		 virtual
#endif
		IHeartbeat InternalHeartbeat
		{
			get
			{
				if (internalHeartbeat == null)
				{
					internalHeartbeat = Heartbeat.New(HeartbeatSessionInfoFactory, HeartbeatDuration, LogoffHandler, HeartBeatWindowsTimer);
				}

				return internalHeartbeat;
			}
		}

		protected void DisposeInternalHeartbeat()
		{
			if (internalHeartbeat != null)
			{
				internalHeartbeat.Dispose();
				internalHeartbeat = null;
			}
		}

		protected IHeartbeat internalHeartbeat;

		#endregion

		#region Create Semaphore

		ISemaphoreHandle CreateSemaphoreHandleCore(ISemaphoreType semaphore)
		{
			Argument.NotNull(semaphore, nameof(semaphore));

			ISemaphoreHandle result = null;

			lock (heartbeatLock)
			{
				if (semaphore.Category == "LGN" && internalHeartbeat != null)
				{
					internalHeartbeat.UpdateUserContext(HeartbeatSessionInfoFactory);
				}

				// Must use lazy instantiated heartbeat property here
				result = SemaphoreHandle.New(InternalHeartbeat.HeartbeatId, semaphore, attemptsToHandleDeadlocks);
				RegisterSemaphorePendingDisposal(result);
			}

			return result;
		}

		void RegisterSemaphorePendingDisposal(ISemaphoreHandle semaphoreHandle)
		{
			Argument.NotNull(semaphoreHandle, nameof(semaphoreHandle));

			((ISemaphoreDisposal)semaphoreHandle).OnSemaphoreDisposed = DisposeHeartbeatIfNoActiveSemaphores;

			if (semaphoreHandle.Success)
			{
				activeSemaphores.Add(new WeakReference(semaphoreHandle));
				RegisterDisposableSemaphore(semaphoreHandle);
			}
		}

		void DisposeHeartbeatIfNoActiveSemaphores(ISemaphoreHandle disposedSemaphore)
		{
			lock (heartbeatLock)
			{
				UnregisterDisposableSemaphore(disposedSemaphore);
				int activeSemaphoreCount = RemoveDisposedSemaphoresAndReturnAliveCount(disposedSemaphore);

				if (activeSemaphoreCount == 0)
				{
					DisposeInternalHeartbeat();
				}
			}
		}

		int RemoveDisposedSemaphoresAndReturnAliveCount(ISemaphoreHandle disposedSemaphore)
		{
			if (activeSemaphores.Count > 0)
			{
				for (int i = activeSemaphores.Count - 1; i >= 0; i--)
				{
					object activeTarget;
					if ((activeTarget = activeSemaphores[i].Target) == null || Object.ReferenceEquals(activeTarget, disposedSemaphore))
					{
						activeSemaphores.RemoveAt(i);
					}
				}
			}

			return activeSemaphores.Count;
		}

		protected internal readonly List<WeakReference> activeSemaphores = new List<WeakReference>();

		#region Disposable Leak - Semaphore

		[Conditional("DEBUG")]
		void RegisterDisposableSemaphore(IDisposable semaphoreHandle)
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(semaphoreHandle, forceStackTrace: true);
		}

		[Conditional("DEBUG")]
		protected void UnregisterDisposableSemaphore(IDisposable semaphoreHandle)
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(semaphoreHandle);
		}

		#endregion

		#endregion

		readonly SemaphoreDbManager dbManager = new SemaphoreDbManager();
		readonly object heartbeatLock = new object();

		protected abstract IHeartbeatInfoFactory HeartbeatSessionInfoFactory { get; }
		protected abstract TimeSpan HeartbeatDuration { get; }
		protected abstract IHeartBeatRemoteLogoff LogoffHandler { get; }
		protected virtual IWindowsTimer HeartBeatWindowsTimer
		{
			get { return null; }
		}

		#region Statics

		public static ISemaphoreHandle CreateSemaphoreHandle(ISemaphoreProvider semaphoreProvider, ISemaphoreType semaphoreType)
		{
			Argument.NotNull(semaphoreProvider, nameof(semaphoreProvider));
			Argument.NotNull(semaphoreType, nameof(semaphoreType));

			return semaphoreProvider.CreateSemaphoreHandle(semaphoreType);
		}

		public static void RemoteLogoff(Guid userPK, string machineName, string heartbeatType, string clientIdentifier)
		{
			Argument.NotNull(machineName, nameof(machineName));

			new SemaphoreDbManager().RemoteLogoff(userPK, machineName, heartbeatType, clientIdentifier);
		}

		public static void ReleaseLocks(string lockInfo, string heartbeatType, Guid userPk)
		{
			Argument.NotNull(lockInfo, nameof(lockInfo));

			new SemaphoreDbManager().ReleaseLocks(lockInfo, heartbeatType, userPk);
		}

		#endregion
	}
}

namespace Enterprise.Semaphores.Common.Contracts
{
}
