using System;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class SqlMutexLocksCleaner : IServiceTaskLocksCleaner
	{
		public SqlMutexLocksCleaner(ISqlMutexLockProvider sqlMutexLockProvider)
		{
			this.sqlMutexLockProvider = sqlMutexLockProvider ?? throw new ArgumentNullException(nameof(sqlMutexLockProvider));
		}

		public void ReleaseLocksFromServiceTask(int processId, string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return;
			}

			var sqlMutexLockInfo = new SqlMutexLockInfo(Category, processId, code, System.Environment.MachineName);
			sqlMutexLockProvider.ReleaseLocks(sqlMutexLockInfo);
		}

		public void ReleaseLocksFromHost()
		{
			sqlMutexLockProvider.ReleaseLocks(new SqlMutexLockInfo(workStationName: System.Environment.MachineName));
		}

		readonly ISqlMutexLockProvider sqlMutexLockProvider;

		const string Category = "PRC";
	}
}
