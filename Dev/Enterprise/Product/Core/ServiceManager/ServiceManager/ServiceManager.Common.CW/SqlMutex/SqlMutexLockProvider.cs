using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common.CW
{
	public class SqlMutexLockProvider : ISqlMutexLockProvider
	{
		public SqlMutexLockProvider() : this(new SqlMutexLockDbManager())
		{
		}

		internal SqlMutexLockProvider(ISqlMutexLockDbManager sqlMutexLockDbManager)
		{
			this.sqlMutexLockDbManager = sqlMutexLockDbManager;
		}

		bool TryGetLock(
			out ISqlMutexLock? mutexLock,
			out int sqlErrorNumber,
			out string returnMessage,
			string lockInfo,
			string category,
			TimeSpan timeout,
			string workStationName,
			int processId,
			string userCode,
			bool throwOnDispose)
		{
			var sqlMutexLockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(
				category,
				lockInfo,
				timeout,
				workStationName,
				processId,
				userCode);
			returnMessage = sqlMutexLockResult.ReturnMessage;

			if (!sqlMutexLockResult.HasAcquiredLock)
			{
				mutexLock = null;
				sqlErrorNumber = sqlMutexLockResult.ReturnValue;
				return false;
			}

			mutexLock = new SqlMutexLock(
				sqlMutexLockDbManager,
				lockInfo,
				category,
				timeout,
				workStationName,
				processId,
				userCode,
				throwOnDispose,
				sqlMutexLockResult);

			sqlErrorNumber = SqlMutexLockResult.LockReturnSuccess;
			return true;
		}

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "out mutexLock is externally disposed in SqlMutexLocker")]
		public bool TryGetLock(
			out ISqlMutexLock? mutexLock,
			out int sqlErrorNumber,
			out string returnMessage,
			string lockInfo,
			string category,
			TimeSpan timeout,
			string userCode,
			string? machineName,
			int? processId,
			bool throwOnLockDispose)
		{
			Argument.NotNullOrEmpty(lockInfo, nameof(lockInfo));
			Argument.NotNullOrEmpty(category, nameof(category));
			Argument.NotNullOrEmpty(userCode, nameof(userCode));

			return TryGetLock(
				out mutexLock,
				out sqlErrorNumber,
				out returnMessage,
				lockInfo,
				category,
				timeout,
				machineName ?? Environment.MachineName,
				processId ?? Process.GetCurrentProcess().Id,
				userCode,
				throwOnLockDispose);
		}

		public bool GetUnobservedLock(string lockInfo, string category, TimeSpan timeout, string userCode, string? machineName = null, int? processId = null)
		{
			Argument.NotNullOrEmpty(lockInfo, nameof(lockInfo));
			Argument.NotNullOrEmpty(category, nameof(category));
			Argument.NotNullOrEmpty(userCode, nameof(userCode));

			return sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(
					lockInfo,
					category,
					timeout,
					machineName ?? Environment.MachineName,
					processId ?? Process.GetCurrentProcess().Id,
					userCode)
				.HasAcquiredLock;
		}

		public void ReleaseLocks(SqlMutexLockInfo sqlMutexLockInfo)
		{
			sqlMutexLockDbManager.ReleaseLocks(sqlMutexLockInfo);
		}

		readonly ISqlMutexLockDbManager sqlMutexLockDbManager;
	}
}
