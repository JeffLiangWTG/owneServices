using System;
using CargoWise.Types;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	public class ZGlobalMutex : IZGlobalMutex
	{
		/// <summary>
		/// Create a critical section for all users for the whole system. Only one
		/// user can run the critical section at once. All users on other machines
		/// will not be able to run the critical section.
		/// </summary>
		/// <param name="mutexID">To create one, edit MutexRegistration in ZModules project.</param>
		public ZGlobalMutex(MutexID mutexID) : this(mutexID, ZString.Empty)
		{
		}

		/// <summary>
		/// Create a critical section for all users for the whole system. Only one
		/// user can run the critical section at once. All users on other machines
		/// will not be able to run the critical section.
		/// </summary>
		/// <param name="mutexID">To create one, edit MutexRegistration in ZModules project.</param>
		/// <param name="recordIdentifier">Allows multiple locks of the same type for different records (eg, different shipments).</param>
		public ZGlobalMutex(MutexID mutexID, ZString recordIdentifier)
		{
			this.MutexID = mutexID;
			this.RecordIdentifier = recordIdentifier;
			this.semaphore = new ZGlobalMutexSemaphore(mutexID.Name, recordIdentifier);
		}

		/// <summary>
		/// If the process is not running, stamp the process as started.
		/// </summary>
		/// <returns>
		/// FALSE: If it's already running or
		///        If can't stamp as started (cannot create semaphore)
		/// TRUE : If lock was succesfully acquired
		/// </returns>
		public bool Lock()
		{
			if (!HasLock)
			{
				mutexHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(semaphore);
			}

			return mutexHandle.Success;
		}

		/// <summary>
		/// Ends the lock.
		/// </summary>
		public void Unlock()
		{
			if (HasLock)
			{
				mutexHandle.Dispose();
				mutexHandle = null;
			}
			else
			{
				throw new MutexNotLockedException(string.Format("Unable to unlock object with ID = [{0}] since it was not locked by {1} mutex.", RecordIdentifier, MutexID.Name));
			}
		}

		/// <summary>
		/// Is this mutex locked?
		/// </summary>
		public ZBool IsLocked
		{
			get
			{
				return (GetLockInfo() != null);
			}
		}

		/// <summary>
		/// Is this mutex locked by current?
		/// </summary>
		public ZBool HasLock
		{
			get
			{
				return (mutexHandle != null && mutexHandle.Success);
			}
		}

		/// <summary>
		/// Tells you about the lock and the lock holder. Returns null if there is no lock.
		/// </summary>
		public LockInfo GetLockInfo()
		{
			LockInfo result = null;
			ISemaphoreInfo[] infos = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphore);

			if (infos.Length > 0)
			{
				result = new LockInfo(infos[0].CreateTimeUtc, MutexID, RecordIdentifier, infos[0].OwnerSession.LogonIdentificationCode, infos[0].OwnerSession.HostName, infos[0].OwnerSession.ProcessId);
			}

			return result;
		}

		public void ReleaseLocks(Guid userPk, string heartbeatType = HeartbeatTypes.Enterprise)
		{
			EnvProxy.Instance.SemaphoreProvider.ReleaseLocks(semaphore.LockInfo, heartbeatType, userPk);
		}

		void IDisposable.Dispose()
		{
			if (HasLock)
			{
				Unlock();
			}
		}

		public MutexID MutexID { get; private set; }
		public ZString RecordIdentifier { get; private set; }
		readonly ISemaphoreType semaphore;
		ISemaphoreHandle mutexHandle;
	}
}
