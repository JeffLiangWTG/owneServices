using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;

namespace Enterprise.Semaphores.Common.Test
{
	[UseSnapshotProtection]
	public class SqlMutexLockProviderUseCasesTest : TestCase
	{
		public class AcquireLockTest : SqlMutexLockProviderUseCasesTest
		{
			public void TestAcquireSuccessFromCleanEnvironment()
			{
				// Arrange
				TestHelper.CleanupSqlMutexLocks();

				// Act
				var result = lockProvider.TryGetLock(
					out var sqlMutexLock,
					out var sqlReturn,
					out var returnMessage,
					nameof(TestAcquireSuccessFromCleanEnvironment),
					TestConstants.Category,
					TestMutexLockTimeout,
					TestConstants.UserCode);

				try
				{
					// Assert
					Assert(returnMessage, result);
					CombineAssertions(() =>
					{
						AssertEquals(returnMessage, true, result);
						AssertNotNull(sqlMutexLock);
						AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlReturn);
					});
				}
				finally
				{
					sqlMutexLock?.Dispose();
				}
			}

			public void TestAcquireSuccessUpdateExpiredLockFromSameHostAndProccess()
			{
				var lockInfo = nameof(TestAcquireSuccessUpdateExpiredLockFromSameHostAndProccess);
				SetupCallbackAfterLockHadBeenAcquiredByHostAndProccess(
					lockInfo,
					setLockHasExpired: true,
					TestMutexLockTimeout,
					System.Environment.MachineName,
					Process.GetCurrentProcess().Id,
					otherResult =>
				{
					var acquiredDateTimeUtc = otherResult.AcquiredDateTimeUtc;
					var lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category);

					AssertNotNull(lockExpiryTime);
					AssertLessThanOrEqualTo("The lock acquired by current process is expired.", lockExpiryTime.Value + TestMutexLockTimeout, acquiredDateTimeUtc);

					// Act
					var result = lockProvider.TryGetLock(
						out var sqlMutexLock1,
						out var sqlReturn,
						out var returnMessage,
						lockInfo,
						TestConstants.Category,
						TestMutexLockTimeout,
						TestConstants.UserCode);

					// Assert
					Assert(returnMessage, result);
					CombineAssertions(() =>
					{
						AssertNotNull(returnMessage, sqlMutexLock1);
						sqlMutexLock1?.Dispose();
						AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlReturn);
					});
				});
			}

			public void TestAcquireSuccessUpdateActiveLockFromSameHostAndProccess()
			{
				var lockInfo = nameof(TestAcquireSuccessUpdateActiveLockFromSameHostAndProccess);
				SetupCallbackAfterLockHadBeenAcquiredByHostAndProccess(
					lockInfo,
					setLockHasExpired: false,
					TestMutexLockTimeout,
					System.Environment.MachineName,
					Process.GetCurrentProcess().Id,
					otherResult =>
					{
						var acquiredDateTimeUtc = otherResult.AcquiredDateTimeUtc;
						var lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category);

						AssertNotNull(lockExpiryTime);
						AssertGreaterThanOrEqualTo("The lock acquired by current process is still active.", lockExpiryTime.Value, acquiredDateTimeUtc);

						// Act
						var result = lockProvider.TryGetLock(
							out var sqlMutexLock1,
							out var sqlReturn,
							out var returnMessage,
							lockInfo,
							TestConstants.Category,
							TestMutexLockTimeout,
							TestConstants.UserCode);

						// Assert
						Assert(returnMessage, result);
						CombineAssertions(() =>
						{
							AssertNotNull(returnMessage, sqlMutexLock1);
							sqlMutexLock1?.Dispose();
							AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlReturn);
						});
					});
			}

			public void TestAcquireFailureLockAcquiredToAnotherProcess()
			{
				// Arrange
				// Act
				// Assert
				var otherProcessId = Process.GetCurrentProcess().Id + 1;
				AcquireFailureLockAcquiredToAnotherProcessOrHost(otherProcessId, System.Environment.MachineName);
			}

			public void TestAcquireFailureLockAcquiredToAnotherHost()
			{
				// Arrange
				// Act
				// Assert
				AcquireFailureLockAcquiredToAnotherProcessOrHost(Process.GetCurrentProcess().Id, $"{Guid.NewGuid()}");
			}

			void AcquireFailureLockAcquiredToAnotherProcessOrHost(int otherProcessId, string otherHost)
			{
				var lockInfo = nameof(AcquireFailureLockAcquiredToAnotherProcessOrHost);
				SetupCallbackAfterLockHadBeenAcquiredByHostAndProccess(
					lockInfo,
					setLockHasExpired: false,
					TestMutexLockTimeout,
					otherHost,
					otherProcessId,
					otherResult =>
					{
						var acquiredDateTimeUtc = otherResult.AcquiredDateTimeUtc;
						var lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category, otherProcessId, otherHost);

						AssertNotNull(lockExpiryTime);
						AssertGreaterThanOrEqualTo("The lock acquired by current process is still active.", lockExpiryTime.Value, acquiredDateTimeUtc);

						// Act
						// Assert
						var counter = -1;
						while (++counter < 1000)
						{
							var result = lockProvider.TryGetLock(
								out var sqlMutexLock1,
								out var sqlReturn,
								out var returnMessage,
								lockInfo,
								TestConstants.Category,
								TimeSpan.FromMilliseconds(TestMutexLockTimeout.TotalMilliseconds / 3),
								TestConstants.UserCode);

							// Assert
							CombineAssertions($"Failed at repeat: {counter}\r\n{sqlMutexLock1}", () =>
							{
								AssertEquals(returnMessage, false, result);
								AssertNull(sqlMutexLock1);
								AssertNotEquals(SqlMutexLockResult.LockReturnSuccess, sqlReturn);
							});
						}
					});
			}
		}

		public class KeepAliveLockTest : SqlMutexLockProviderUseCasesTest
		{
			public void TestKeepAliveImmediatelyRetryOnFailures()
			{
				// Arrange
				var lockInfo = nameof(TestKeepAliveImmediatelyRetryOnFailures);
				ISqlMutexLock sqlMutexLock = null;
				DateTime? lockExpiryTime;

				try
				{
					var result = lockProvider.TryGetLock(out sqlMutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, TestMutexLockTimeout, TestConstants.UserCode);
					Assert(returnMessage, result);

					if (sqlMutexLock is SqlMutexLock mutexLock)
					{
						var initialLockExpiriesTimeUtc = mutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

						using (var extraConnection = Db.NewExtraConnectionToMainDb())
						using (TestHelper.AcquireStmServiceMutexRowLocks(extraConnection, commit: false))
						{
							Thread.Sleep(mutexLock.TimerTickInterval);
							Thread.Sleep(mutexLock.TimerTickInterval);
						}

						// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
						Thread.Sleep(mutexLock.TimerTickInterval);
						Thread.Sleep(mutexLock.TimerTickInterval);

						// Assert
						lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category);
						AssertGreaterThan(lockExpiryTime.Value, initialLockExpiriesTimeUtc);
					}
				}
				finally
				{
					sqlMutexLock?.Dispose();
				}
			}

			public void TestKeepAliveSuccessFromCleanEnvironment()
			{
				var lockInfo = nameof(TestKeepAliveImmediatelyRetryOnFailures);
				var result = lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, TestMutexLockTimeout, TestConstants.UserCode);
				Assert(returnMessage, result);
				var sqlMutexLock = mutexLock as SqlMutexLock;
				AssertNotNull(sqlMutexLock);
				var lockExpiriesTimeUtc = sqlMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

				// Arrange
				TestHelper.CleanupSqlMutexLocks();

				try
				{
					// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
					Thread.Sleep(sqlMutexLock.TimerTickInterval.Add(TimeSpan.FromSeconds(1)));

					// Assert
					var lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category);
					AssertNotNull(lockExpiryTime);
					AssertGreaterThan(lockExpiryTime.Value, lockExpiriesTimeUtc);
					AssertGreaterThan(sqlMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc, lockExpiriesTimeUtc);
				}
				finally
				{
					sqlMutexLock?.Dispose();
				}
			}

			public void TestKeepAliveSuccessUpdateExpiredLockFromSameHostAndProccess()
			{
				var lockInfo = nameof(TestKeepAliveSuccessUpdateExpiredLockFromSameHostAndProccess);
				var hostName = System.Environment.MachineName;
				var processId = Process.GetCurrentProcess().Id;
				DateTime? lockExpiryTime;

				KeepAliveUpdateLockFromHostAndProccess(lockInfo, TestMutexLockTimeout, hostName, processId, keepAliveMutexLock =>
				{
					var initialLockExpiriesTimeUtc = keepAliveMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

					// Force the lock to be expired
					TestHelper.UpdateLockExpiresAfterTimeoutInSeconds(Db.Connection, lockInfo, TestConstants.Category, timeoutInSeconds: -10);

					// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

					// Assert
					lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(Db.Connection, lockInfo, TestConstants.Category, processId, hostName);
					AssertNotNull(lockExpiryTime);
					AssertGreaterThan(lockExpiryTime.Value, initialLockExpiriesTimeUtc);
				});
			}

			public void TestKeepAliveSuccessUpdateActiveLockFromSameHostAndProccess()
			{
				var lockInfo = nameof(TestKeepAliveSuccessUpdateActiveLockFromSameHostAndProccess);
				var hostName = System.Environment.MachineName;
				var processId = Process.GetCurrentProcess().Id;
				DateTime? lockExpiryTime;

				KeepAliveUpdateLockFromHostAndProccess(lockInfo, TestMutexLockTimeout, hostName, processId, keepAliveMutexLock =>
				{
					var lockExpiriesTimeUtc = keepAliveMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

					// block the keepAliveThread keepAlive calls
					using (var extraConnection = Db.NewExtraConnectionToMainDb())
					using (TestHelper.AcquireStmServiceMutexRowLocks(extraConnection, commit: true))
					{
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

						// Arrange - set the lock as active
						TestHelper.UpdateLockExpiresAfterTimeoutInSeconds(extraConnection, lockInfo, TestConstants.Category, (int)TestMutexLockTimeout.TotalSeconds);
					}

					// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

					// Assert
					lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category);
					AssertNotNull(lockExpiryTime);
					AssertGreaterThan(lockExpiryTime.Value, lockExpiriesTimeUtc);
				});
			}

			public void TestKeepAliveSuccessTakeOverExpiredLockFromOtherHostProcess()
			{
				var lockInfo = nameof(TestKeepAliveSuccessTakeOverExpiredLockFromOtherHostProcess);
				var otherHostName = $"{Guid.NewGuid()}";
				var otherProcessId = Process.GetCurrentProcess().Id + 1;
				DateTime? lockExpiryTime;

				KeepAliveUpdateLockFromHostAndProccess(lockInfo, TestMutexLockTimeout, otherHostName, otherProcessId, keepAliveMutexLock =>
				{
					var lockExpiriesTimeUtc = keepAliveMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

					// block the keepAliveThread keepAlive calls
					using (var extraConnection = Db.NewExtraConnectionToMainDb())
					using (TestHelper.AcquireStmServiceMutexRowLocks(extraConnection, commit: true))
					{
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

						// Arrange - set the lock as expired
						TestHelper.UpdateLockExpiresAfterTimeoutInSeconds(extraConnection, lockInfo, TestConstants.Category, timeoutInSeconds: -10, otherProcessId, otherHostName);
					}

					// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

					// Assert
					lockExpiryTime = TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category, otherProcessId, otherHostName);
					AssertNotNull(lockExpiryTime);
					AssertGreaterThan(lockExpiryTime.Value, lockExpiriesTimeUtc);
				});
			}

			public void TestKeepAliveLockLostWhenLockIsAcquireToAnotherHostProcess()
			{
				var lockInfo = nameof(TestKeepAliveLockLostWhenLockIsAcquireToAnotherHostProcess);
				var otherHostName = $"{Guid.NewGuid()}";
				var otherProcessId = Process.GetCurrentProcess().Id + 1;

				KeepAliveUpdateLockFromHostAndProccess(lockInfo, TestMutexLockTimeout, System.Environment.MachineName, Process.GetCurrentProcess().Id, keepAliveMutexLock =>
				{
					var lockExpiriesTimeUtc = keepAliveMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;

					// block the keepAliveThread keepAlive calls
					using (var extraConnection = Db.NewExtraConnectionToMainDb())
					using (TestHelper.AcquireStmServiceMutexRowLocks(extraConnection, commit: true))
					{
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
						Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

						// Arrange - set the lock as acquired by another host process
						_ = TestHelper.UpdateLockAsAcquiredToOtherHostProcess(extraConnection, lockInfo, TestConstants.Category, otherProcessId, otherHostName);
					}

					// Act - the acquired sqlMutexLock has a background timer tick, let the timer run
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);
					Thread.Sleep(keepAliveMutexLock.TimerTickInterval);

					// Assert
					AssertNull(TestHelper.GetLockExpiryTimeUtc(lockInfo, TestConstants.Category));
				});
			}

			void KeepAliveUpdateLockFromHostAndProccess(string lockInfo, TimeSpan lockTimeout, string otherHost, int otherProcessId, Action<SqlMutexLock> callBackAction)
			{
				// Arrange
				var result = lockProvider.TryGetLock(
					out var sqlMutexLock,
					out _,
					out var returnMessage,
					lockInfo,
					TestConstants.Category,
					lockTimeout,
					TestConstants.UserCode,
					otherHost,
					otherProcessId);

				try
				{
					Assert(returnMessage, result);

					if (sqlMutexLock is SqlMutexLock mutexLock)
					{
						callBackAction?.Invoke(mutexLock);
					}
					else
					{
						Fail($"{sqlMutexLock} is not type of: {typeof(SqlMutexLock)}, {returnMessage}.");
					}
				}
				finally
				{
					sqlMutexLock?.Dispose();
				}
			}
		}

		public class ReleaseLockTest : SqlMutexLockProviderUseCasesTest
		{
			public void TestReleaseRetryOnFailures()
			{
				// Arrange
				var numberOfRetries = 0;
				var sqlMutexLockDbManagerMock = new Mock<ISqlMutexLockDbManager>();
				sqlMutexLockDbManagerMock.Setup(x => x
					.AcquireOrUpdateSqlMutexLock(It.IsAny<SqlMutexLock>()))
					.Returns(new SqlMutexLockResult(SuccessAcquireOrUpdateSqlMutex, DateTime.UtcNow.Add(TestMutexLockTimeout), DateTime.UtcNow, string.Empty));
				sqlMutexLockDbManagerMock.Setup(x => x
					.ReleaseSqlMutexLock(It.IsAny<SqlMutexLock>()))
					.Returns(() =>
					{
						numberOfRetries++;
						return new SqlMutexLockResult(FailedAcquireOrUpdateSqlMutex, DateTime.UtcNow.Add(TestMutexLockTimeout), DateTime.UtcNow, string.Empty);
					});

				using var sqlMutexLock = new SqlMutexLock(
					sqlMutexLockDbManagerMock.Object,
					nameof(TestReleaseRetryOnFailures),
					TestConstants.Category,
					TestMutexLockTimeout,
					System.Environment.MachineName,
					Process.GetCurrentProcess().Id,
					TestConstants.UserCode,
					throwOnDispose: true,
					new SqlMutexLockResult(SqlMutexLockResult.LockReturnSuccess, DateTime.UtcNow.Add(TestMutexLockTimeout), DateTime.UtcNow, string.Empty));

				var stopWatch = new Stopwatch();
				var releaseLockTimeout = TimeSpan.FromMilliseconds(100);

				var exception = AssertExceptionThrown<SqlMutexLockReleaseException>(() =>
				{
					using (new DisposableAction(() => { SqlMutexLock.ReleaseLockTimeout.ResetValue(); }))
					{
						SqlMutexLock.ReleaseLockTimeout.Value = releaseLockTimeout;
						stopWatch.Start();

						// Act
						sqlMutexLock?.Dispose();
					}
				});

				stopWatch.Stop();

				// Assert
				AssertNull(exception.InnerException);
				AssertGreaterThanOrEqualTo(stopWatch.Elapsed.TotalMilliseconds, releaseLockTimeout.TotalMilliseconds * 0.9d);
				AssertGreaterThan(numberOfRetries, 2);
			}

			public void TestReleaseSuccessFromCleanEnvironment()
			{
				var sqlMutexLock = GetSqlMutexLock(nameof(TestReleaseSuccessFromCleanEnvironment));
				sqlMutexLock?.Dispose();

				AssertNotNull(sqlMutexLock);
				TestHelper.CleanupSqlMutexLocks();
				AssertNull(TestHelper.GetLockExpiryTimeUtc(sqlMutexLock.LockInfo, sqlMutexLock.Category));
			}

			public void TestReleaseSuccessExpiredLock()
			{
				var sqlMutexLock = GetSqlMutexLock(nameof(TestReleaseSuccessExpiredLock));
				sqlMutexLock?.Dispose();

				// Arrange - set the lock as expired
				TestHelper.UpdateLockExpiresAfterTimeoutInSeconds(Db.Connection, sqlMutexLock.LockInfo, sqlMutexLock.Category, timeoutInSeconds: -10);

				// Assert
				AssertNotNull(sqlMutexLock);
				var lockExpiryTimeUtc = TestHelper.GetLockExpiryTimeUtc(sqlMutexLock.LockInfo, sqlMutexLock.Category);
				AssertNotNull(lockExpiryTimeUtc);
				AssertGreaterThan(DateTime.UtcNow, lockExpiryTimeUtc.Value);
			}

			public void TestReleaseSuccessAsNoopWhenLockIsAcquiredToAnotherHostProcess()
			{
				var sqlMutexLock = GetSqlMutexLock(nameof(TestReleaseSuccessAsNoopWhenLockIsAcquiredToAnotherHostProcess));
				sqlMutexLock?.Dispose();

				// Arrange - set the lock as acquired by another host process
				var otherProcessId = Process.GetCurrentProcess().Id;
				var otherHostName = $"{Guid.NewGuid()}";
				_ = TestHelper.UpdateLockAsAcquiredToOtherHostProcess(Db.Connection, sqlMutexLock.LockInfo, sqlMutexLock.Category, otherProcessId, otherHostName);

				// Assert
				AssertNotNull(sqlMutexLock);
				AssertNull(TestHelper.GetLockExpiryTimeUtc(sqlMutexLock.LockInfo, sqlMutexLock.Category));
			}

			SqlMutexLock GetSqlMutexLock(string lockInfo)
			{
				ISqlMutexLock mutexLock;
				var result = lockProvider.TryGetLock(
					out mutexLock,
					out _,
					out var returnMessage,
					lockInfo,
					TestConstants.Category,
					TestMutexLockTimeout,
					TestConstants.UserCode,
					System.Environment.MachineName,
					Process.GetCurrentProcess().Id,
					throwOnLockDispose: true);
				Assert(returnMessage, result);
				return mutexLock as SqlMutexLock;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestHelper.CleanupSqlMutexLocks();

			mutexLockDbManager = new SqlMutexLockDbManager();
			lockProvider = new SqlMutexLockProvider(mutexLockDbManager);
		}

		protected void SetupCallbackAfterLockHadBeenAcquiredByHostAndProccess(
			string lockInfo,
			bool setLockHasExpired,
			TimeSpan lockTimeout,
			string otherHost,
			int otherProcessId,
			Action<(DateTime ExpiresAtDateTimeUtc, DateTime AcquiredDateTimeUtc)> callBackAction)
		{
			// Arrange
			var readyEventHandle = $"{Guid.NewGuid()}";
			using var readyEvent = new AutoResetEvent(false);
			using var exitEvent = new AutoResetEvent(false);
			DateTime expiresAtDateTimeUtc = DateTime.MinValue;
			DateTime ecquiredDateTimeUtc = DateTime.MaxValue;
			var eventWaitTimeout = TimeSpan.FromSeconds(5);
			ISqlMutexLock sqlMutexLock = null;

			var thread = new Thread(() =>
			{
				// Acquire the lock from another thread
				lockProvider.TryGetLock(
					out sqlMutexLock,
					out _,
					out var returnMessage,
					lockInfo,
					TestConstants.Category,
					lockTimeout,
					"OTH",
					machineName: otherHost,
					processId: otherProcessId);

				if (sqlMutexLock is SqlMutexLock mutexLock)
				{
					expiresAtDateTimeUtc = mutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc;
					ecquiredDateTimeUtc = mutexLock.LatestSqlMutexLockResult.AcquiredDateTimeUtc;
				}

				// dispose to set lock as expired as per test setup
				if (setLockHasExpired)
				{
					sqlMutexLock?.Dispose();
				}

				readyEvent.Set();
				exitEvent.WaitOne(eventWaitTimeout);
			});

			try
			{
				thread.Start();

				readyEvent.WaitOne(eventWaitTimeout);
				callBackAction?.Invoke((expiresAtDateTimeUtc, ecquiredDateTimeUtc));
			}
			finally
			{
				exitEvent.Set();
				thread.Join();
				sqlMutexLock?.Dispose();
			}
		}

		ISqlMutexLockProvider lockProvider;
		ISqlMutexLockDbManager mutexLockDbManager;

		readonly TimeSpan TestMutexLockTimeout = TimeSpan.FromSeconds(3);

		const int SuccessAcquireOrUpdateSqlMutex = SqlMutexLockResult.LockReturnSuccess;
		const int FailedAcquireOrUpdateSqlMutex = -1;
	}
}
