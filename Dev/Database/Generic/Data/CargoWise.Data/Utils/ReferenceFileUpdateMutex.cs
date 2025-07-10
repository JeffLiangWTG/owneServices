namespace CargoWise.Data
{
	using System;
	using System.Data;
	using System.Globalization;

	public static class ReferenceFileUpdateMutex
	{
		/// <summary>
		/// CMR AU Reference Database can be shared amonst different Enterprise systems in the same server.
		/// So to control concurrency, an application lock is used as a semaphore control.
		/// </summary>
		public static bool AcquireUpdateLock(DbConnection connection, TimeSpan timeout, RefDbTypeEnum dbType, string country, string resourceNameForLock)
		{
			var result = false;
			if (connection != null && !string.IsNullOrWhiteSpace(country) && !string.IsNullOrWhiteSpace(resourceNameForLock))
			{
				result = AcquireUpdateLock(connection, timeout, ((IPhysicalRefDbLocation)connection).GetReferenceDatabaseName(dbType, country), resourceNameForLock);
			}
			return result;
		}

		/// <summary>
		/// CMR AU Reference Database can be shared amonst different Enterprise systems in the same server.
		/// So to control concurrency, an application lock is used as a semaphore control.
		/// </summary>
		public static bool AcquireUpdateLock(DbConnection connection, TimeSpan timeout, string referenceDbName, string resourceNameForLock)
		{
			var result = false;
			if (connection != null && !string.IsNullOrWhiteSpace(referenceDbName) && !string.IsNullOrWhiteSpace(resourceNameForLock))
			{
				var timeOutInMs = (timeout <= TimeSpan.Zero) ? 0 : (int)timeout.TotalMilliseconds;

				var sqlText = string.Format(CultureInfo.InvariantCulture, "[{0}]..sp_getapplock", referenceDbName); // This is an SQL expression

				using (var cmd = connection.Command(sqlText))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@Resource", SqlDbType.VarChar, resourceNameForLock);
					cmd.AddParameter("@LockMode", SqlDbType.VarChar, "Exclusive"); // This is an SQL expression
					cmd.AddParameter("@LockOwner", SqlDbType.VarChar, "Transaction"); // This is an SQL expression
					cmd.AddParameter("@LockTimeout", SqlDbType.Int, timeOutInMs);
					int returnCode = cmd.ExecuteProcedureWithReturnValue();

					result = (returnCode >= 0);
				}
			}
			return result;
		}

		/// <summary>
		/// Returns information about whether or not a lock can be granted on a particular application resource without acquiring the lock.
		/// </summary>
		public static bool CanAquireLock(DbConnection connection, RefDbTypeEnum dbType, string country, string resourceNameForLock)
		{
			var result = false;
			if (connection != null && !string.IsNullOrWhiteSpace(country) && !string.IsNullOrWhiteSpace(resourceNameForLock))
			{
				result = CanAquireLock(connection, ((IPhysicalRefDbLocation)connection).GetReferenceDatabaseName(dbType, country), resourceNameForLock);
			}
			return result;
		}

		/// <summary>
		/// Returns information about whether or not a lock can be granted on a particular application resource without acquiring the lock.
		/// </summary>
		public static bool CanAquireLock(DbConnection connection, string referenceDbName, string resourceNameForLock)
		{
			var result = false;
			if (connection != null && !string.IsNullOrWhiteSpace(referenceDbName) && !string.IsNullOrWhiteSpace(resourceNameForLock))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT APPLOCK_TEST('public', '{0}', 'Exclusive', 'Transaction')", resourceNameForLock); // This is an SQL expression
				using (((ICurrentDbControl)connection).UseDatabase(referenceDbName))
				{
					var lockValue = connection.ExecuteScalar(sqlText);
					result = lockValue != null && (int)lockValue == 1;
				}
			}
			return result;
		}

		public static bool AcquireUpdateLockForSharedDatabase(DbConnection connection, string lockType, string countryCode)
		{
			var result = false;
			if (connection != null && !string.IsNullOrWhiteSpace(lockType) && !string.IsNullOrWhiteSpace(countryCode))
			{
				var currentRefDbName = ((IPhysicalRefDbLocation)connection).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, countryCode);
				if (RefDbTableNameResolver.IsSharedDatabase(currentRefDbName))
				{
					result = CanAquireLock(connection, currentRefDbName, currentRefDbName) // check to ensure the db is not locked for upgrade
						&& AcquireUpdateLock(connection, TimeSpan.Zero, currentRefDbName, lockType);
				}
				else
				{
					result = true;
				}
			}
			return result;
		}
	}
}
