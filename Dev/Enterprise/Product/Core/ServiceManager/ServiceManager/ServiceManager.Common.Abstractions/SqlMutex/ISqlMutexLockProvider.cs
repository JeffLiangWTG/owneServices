using System;

namespace ServiceManager.Common.Abstractions
{
	public interface ISqlMutexLockProvider
	{
		bool TryGetLock(
			out ISqlMutexLock? mutexLock,
			out int sqlErrorNumber,
			out string returnMessage,
			string lockInfo,
			string category,
			TimeSpan timeout,
			string userCode,
			string? machineName = null,
			int? processId = null,
			bool throwOnLockDispose = false);

		bool GetUnobservedLock(
			string lockInfo,
			string category,
			TimeSpan timeout,
			string userCode,
			string? machineName = null,
			int? processId = null);

		void ReleaseLocks(SqlMutexLockInfo sqlMutexLockInfo);
	}
}
