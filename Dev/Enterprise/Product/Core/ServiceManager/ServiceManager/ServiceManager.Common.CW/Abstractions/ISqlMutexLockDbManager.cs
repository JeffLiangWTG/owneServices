using System;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common.CW
{
	interface ISqlMutexLockDbManager
	{
		SqlMutexLockResult ReleaseSqlMutexLock(SqlMutexLock mutexLockInfo);
		SqlMutexLockResult AcquireOrUpdateSqlMutexLock(SqlMutexLock mutexLockInfo);
		SqlMutexLockResult AcquireOrUpdateSqlMutexLock(string category, string lockInfo, TimeSpan timeout, string workStationName, int processId, string userCode);
		void ReleaseLocks(SqlMutexLockInfo sqlMutexLockInfo);
	}
}
