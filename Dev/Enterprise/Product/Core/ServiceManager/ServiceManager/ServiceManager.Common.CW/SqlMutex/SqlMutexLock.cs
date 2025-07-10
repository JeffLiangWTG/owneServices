using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using ServiceManager.Common.Abstractions;
using static System.FormattableString;

namespace ServiceManager.Common.CW
{
	sealed class SqlMutexLock : Disposable, ISqlMutexLock
	{
		internal SqlMutexLock(ISqlMutexLockDbManager sqlMutexLockDbProvider,
			string lockInfo,
			string category,
			TimeSpan timeout,
			string workstationName,
			int processId,
			string userCode,
			bool throwOnDispose,
			SqlMutexLockResult sqlMutexLockResult)
		{
			Argument.NotNull(sqlMutexLockDbProvider, nameof(sqlMutexLockDbProvider));
			Argument.NotNullOrEmpty(lockInfo, nameof(lockInfo));
			Argument.NotNullOrEmpty(category, nameof(category));
			Argument.NotNullOrEmpty(workstationName, nameof(workstationName));
			Argument.NotNullOrEmpty(userCode, nameof(userCode));

			mutexLockDbProvider = sqlMutexLockDbProvider;
			LockInfo = lockInfo;
			Category = category;
			WorkStationName = workstationName;
			ProcessId = processId;
			UserCode = userCode;
			LatestSqlMutexLockResult = sqlMutexLockResult;
			ReturnMessage = sqlMutexLockResult?.ReturnMessage;
			ThrowOnDispose = throwOnDispose;
			LockTimeout = timeout;
			TimerTickInterval = TimeSpan.FromMilliseconds(LockTimeout.TotalMilliseconds * TimerTickIntervalFactor);

			if (timeout != Timeout.InfiniteTimeSpan)
			{
				keepAliveTimer = new Timer(KeepAliveTimerCallback, null, TimerTickInterval, TimerTickInterval);
			}
		}

		public TimeSpan TimerTickInterval { get; }
		public string LockInfo { get; }
		public string Category { get; }
		public string WorkStationName { get; }
		public int ProcessId { get; }
		public TimeSpan LockTimeout { get; }
		public string UserCode { get; }
		public bool ThrowOnDispose { get; }
		public SqlMutexLockResult LatestSqlMutexLockResult { get; private set; }
		public string? ReturnMessage { get; private set; }

		void KeepAliveTimerCallback(object? state)
		{
			if (isDoingCallBack || IsDisposed)
			{
				return;
			}

			isDoingCallBack = true;

			try
			{
				if (DbEnv.Instance.IsTimerDisabledDuringDbUpgrade)
				{
					lockLostCount++;
				}
				else
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							if (Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
							{
								lockLostCount++;
							}
							else
							{
								LatestSqlMutexLockResult = mutexLockDbProvider.AcquireOrUpdateSqlMutexLock(this);

								if (LatestSqlMutexLockResult.HasAcquiredLock)
								{
									lockLostCount = 0;
								}
								else
								{
									lockLostCount++;
								}
							}
						}
					}
					catch
					{
						lockLostCount++;
					}
				}

				if (lockLostCount >= LockLostMaxCount)
				{
					var lockResult = LatestSqlMutexLockResult?.ReturnValue ?? -1;
					OnLockLost?.Invoke(this, new SqlMutexLockEventArgs(LockInfo, Category, lockResult));
				}
			}
			finally
			{
				isDoingCallBack = false;
			}
		}

		public override string ToString()
		{
			return Invariant($"LockInfo={LockInfo};Category:{Category};Timeout={LockTimeout};UserCode={UserCode}");
		}

		public event EventHandler<SqlMutexLockEventArgs>? OnLockLost;

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				if (keepAliveTimer != null)
				{
					// prevent the timer from restarting and disable periodic signaling
					keepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);

					using (var waitHandle = new ManualResetEvent(false))
					{
						keepAliveTimer?.Dispose(waitHandle);
						waitHandle.WaitOne();
					}

					keepAliveTimer = null;
				}

				try
				{
					using var cts = new CancellationTokenSource(ReleaseLockTimeout.Value);
					SqlMutexLockResult? result = null;

					do
					{
						result = mutexLockDbProvider.ReleaseSqlMutexLock(this);
					} while (
						result.ReturnValue != SqlMutexLockResult.LockReturnSuccess
						&& !cts.Token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(10)));

					if (result?.ReturnValue != SqlMutexLockResult.LockReturnSuccess && ThrowOnDispose)
					{
						throw new SqlMutexLockReleaseException($"{result?.ReturnValue}{Environment.NewLine}{result?.ReturnMessage}");
					}
				}
				catch (SqlMutexLockReleaseException)
				{
					throw;
				}
				catch (Exception exception)
				{
					if (ThrowOnDispose)
					{
						throw new SqlMutexLockReleaseException(exception);
					}
				}
			}
		}

		internal const double TimerTickIntervalFactor = 0.4;
		internal const int LockLostMaxCount = (int)(1 / TimerTickIntervalFactor);

		Timer? keepAliveTimer;
		readonly ISqlMutexLockDbManager mutexLockDbProvider;

		internal int lockLostCount;
		bool isDoingCallBack;

		internal static Overridable<TimeSpan> ReleaseLockTimeout = new Overridable<TimeSpan>(TimeSpan.FromMinutes(1));
	}
}
