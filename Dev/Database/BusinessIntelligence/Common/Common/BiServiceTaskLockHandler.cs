namespace CargoWise.Bi.Common
{
	using System;
	using CargoWise.Data;
	using CargoWise.Data.Utils;

	#region SuppressResourceStringsCheckRegion

	public class BiServiceTaskLockHandler
	{
		protected virtual DbConnection MainDbConnection
		{
			get
			{
				return Db.Connection;
			}
		}

		public const string LockKey = "BI-SERVICE-TASK-LOCK";

		public IDisposable TryGetLock(string dbType, TimeSpan waitTimeout, out bool isLockAcquired)
		{
			return TryGetLock(waitTimeout, LockKey + dbType, out isLockAcquired);
		}

		public IDisposable TryGetLock(TimeSpan waitTimeout, string lockKey, out bool isLockAcquired)
		{
			SqlApplicationLock biLock;

			if (MainDbConnection.TryGetLock(lockKey, waitTimeout, out biLock))
			{
				isLockAcquired = true;
				return biLock;
			}
			else
			{
				isLockAcquired = false;
				return null;
			}
		}

		public IDisposable AcquireLock(string dbType, TimeSpan waitTimeout)
		{
			return AcquireLock(waitTimeout, LockKey + dbType);
		}

		public IDisposable AcquireLock(TimeSpan waitTimeout, string lockKey)
		{
			SqlApplicationLock biLock;

			if (MainDbConnection.TryGetLock(lockKey, waitTimeout, out biLock))
			{
				return biLock;
			}
			else
			{
				var errorMessage = "Could not acquire BI service task lock. It is currently held by another session.";
				throw new BiServiceLockException(errorMessage);
			}
		}
	}
	#endregion
}
