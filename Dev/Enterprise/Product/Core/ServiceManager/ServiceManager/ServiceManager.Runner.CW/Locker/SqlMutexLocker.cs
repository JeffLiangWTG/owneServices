using System.Diagnostics;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Runner
{
	class SqlMutexLocker : ISqlMutexLocker
	{
		public SqlMutexLocker(ISqlMutexLockProvider sqlMutexLockProvider, IRunnerLogger runnerLogger)
		{
			this.sqlMutexLockProvider = sqlMutexLockProvider ?? throw new ArgumentNullException(nameof(sqlMutexLockProvider));
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
		}

		static bool IsReportableError(int sqlErrorNumber)
		{
			switch (sqlErrorNumber)
			{
				case 0:
				case SqlErrorCannotInsertDuplicateKey:
					return false;
				default:
					return true;
			}
		}

		public bool TryAcquireLock(string code, out IDisposable? disposableLock)
		{
			disposableLock = null;

			if (sqlMutexLockProvider.TryGetLock(
				out var mutexLock,
				out var sqlErrorNumber,
				out var returnMessage,
				code,
				Category,
				TimeSpan.FromSeconds(Env.Registry.ServiceTaskHeartbeatDurationSeconds),
				((GlbStaff)Env.CurrentUser)?.GS_Code ?? User.ServiceUserCode,
				System.Environment.MachineName,
				Process.GetCurrentProcess().Id,
				true))
			{
				disposableLock = mutexLock;
				mutexLock!.OnLockLost += (sender, args) =>
				{
					runnerLogger.Log(LogLevel.Error, Invariant($"Mutex lock lost to {Category}:{code}, reason: {args.LockResult}."));
				};

				runnerLogger.Log(LogLevel.Debug, Invariant($"Mutex lock acquired {Category}:{code}, ReturnMessage: {returnMessage}."));

				return true;
			}

			if (IsReportableError(sqlErrorNumber))
			{
				runnerLogger.Log(LogLevel.Information, Invariant($"Failed to acquire a lock on {Category}:{code}, reason: {sqlErrorNumber}.{System.Environment.NewLine}{returnMessage}"));
			}

			return false;
		}

		const string Category = "PRC";
		const int SqlErrorCannotInsertDuplicateKey = 2601; // Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'. The duplicate key value is %ls.
		readonly IRunnerLogger runnerLogger;
		readonly ISqlMutexLockProvider sqlMutexLockProvider;
	}
}
