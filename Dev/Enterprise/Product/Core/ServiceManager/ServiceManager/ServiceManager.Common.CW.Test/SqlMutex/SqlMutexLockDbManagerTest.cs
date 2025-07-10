using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Semaphores.Common.Test;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;

namespace Enterprise.Semaphores.Common.Testing
{
	[UseSnapshotProtection]
	abstract class SqlMutexLockDbManagerTestBase : TestCase
	{
		[UseSnapshotProtection]
		class StoredProcReturnMessageTest : SqlMutexLockDbManagerTestBase
		{
			public void TestAcquireSuccess()
			{
				// Arrange
				var expectedReturnMessages = new[]
				{
					"started",
					"exist count(0)",
					"updated(0)",
					"inserted(1)"
				};

				// Act
				var sqlMutexLockResult = AcquireOrUpdateSqlMutexLock();

				// Assert
				AssertNotNull(sqlMutexLockResult);
				CombineAssertions(sqlMutexLockResult.ReturnMessage, () =>
				{
					AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlMutexLockResult.ReturnValue);

					var actualMessages = sqlMutexLockResult.ReturnMessage
						.Split(new[] { '\r', '\n' })
						.Where(x => !string.IsNullOrEmpty(x.Trim()))
						.Select(x => Regex.Replace(x, @"^.*\:\s+", string.Empty));

					AssertContainsExactElementsInAnyOrder(expectedReturnMessages, actualMessages);
				});
			}

			public void TestReleaseSuccess()
			{
				// Arrange
				var expectedReturnMessages = new[]
				{
					"started",
					"released(1)",
				};

				var lockInfo = nameof(TestReleaseSuccess);
				var lockResult = AcquireOrUpdateSqlMutexLock(lockInfo);
				Assert(lockResult.HasAcquiredLock);
				var mutexLock = NewSqlMutexLock(lockInfo, 1, default, false);
				using (mutexLock)
				{
					// Act
					var sqlMutexLockResult = sqlMutexLockDbManager.ReleaseSqlMutexLock(mutexLock);

					// Assert
					AssertNotNull(sqlMutexLockResult);
					CombineAssertions(sqlMutexLockResult.ReturnMessage, () =>
					{
						AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlMutexLockResult.ReturnValue);

						var actualMessages = sqlMutexLockResult.ReturnMessage
							.Split(new[] { '\r', '\n' })
							.Where(x => !string.IsNullOrEmpty(x.Trim()))
							.Select(x => Regex.Replace(x, @"^.*\:\s+", string.Empty));

						AssertContainsExactElementsInAnyOrder(actualMessages, expectedReturnMessages);
					});
				}
			}

			public void TestKeepAliveSuccess()
			{
				// Arrange
				var expectedReturnMessages = new[]
				{
					"started",
					"exist count(1)",
					"kept alive(1)",
				};

				var lockInfo = nameof(TestKeepAliveSuccess);
				var lockResult = AcquireOrUpdateSqlMutexLock(lockInfo);
				Assert(lockResult.HasAcquiredLock);
				var mutexLock = NewSqlMutexLock(lockInfo, 1, default, false);

				try
				{
					// Act
					var sqlMutexLockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

					// Assert
					AssertNotNull(sqlMutexLockResult);
					CombineAssertions(sqlMutexLockResult.ReturnMessage, () =>
					{
						AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlMutexLockResult.ReturnValue);

						var actualMessages = sqlMutexLockResult.ReturnMessage
							.Split(new[] { '\r', '\n' })
							.Where(x => !string.IsNullOrEmpty(x.Trim()))
							.Select(x => Regex.Replace(x, @"^.*\:\s+", string.Empty));

						AssertContainsExactElementsInAnyOrder(expectedReturnMessages, actualMessages);
					});
				}
				finally
				{
					mutexLock.Dispose();
				}
			}

			public void TestUpdateExpiredSuccess()
			{
				// Arrange
				var expectedReturnMessages = new[]
				{
					"started",
					"exist count(0)",
					"updated(1)",
				};

				var lockInfo = nameof(TestUpdateExpiredSuccess);
				var lockResult = AcquireOrUpdateSqlMutexLock(lockInfo);
				Assert(lockResult.HasAcquiredLock);
				var mutexLock = NewSqlMutexLock(lockInfo, 1, default, false);
				_ = TestHelper.UpdateLockAsExpiredToOtherHostProcess(Db.Connection, lockInfo, TestConstants.Category);

				try
				{
					// Act
					var sqlMutexLockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

					// Assert
					AssertNotNull(sqlMutexLockResult);
					CombineAssertions(sqlMutexLockResult.ReturnMessage, () =>
					{
						AssertEquals(SqlMutexLockResult.LockReturnSuccess, sqlMutexLockResult.ReturnValue);

						var actualMessages = sqlMutexLockResult.ReturnMessage
							.Split(new[] { '\r', '\n' })
							.Where(x => !string.IsNullOrEmpty(x.Trim()))
							.Select(x => Regex.Replace(x, @"^.*\:\s+", string.Empty));

						AssertContainsExactElementsInAnyOrder(expectedReturnMessages, actualMessages);
					});
				}
				finally
				{
					mutexLock.Dispose();
				}
			}

			SqlMutexLockResult AcquireOrUpdateSqlMutexLock(string lockInfo = null)
			{
				return sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(
					TestConstants.Category,
					lockInfo ?? nameof(AcquireOrUpdateSqlMutexLock),
					TimeSpan.FromSeconds(900),
					System.Environment.MachineName,
					Process.GetCurrentProcess().Id,
					TestConstants.UserCode);
			}
		}

		class SqlMutexLockDbManagerTest : SqlMutexLockDbManagerTestBase
		{
			public void TestUnrecoverableSqlErrorsAreHandled()
			{
				// Arrange
				const string conversionFromVarcharToInt = "SELECT number = CONVERT(int, 'ABC')";
				var expectedException = AssertExceptionThrown<SqlException>(() => Db.Connection.ExecuteScalar<int>(conversionFromVarcharToInt));

				var lockInfo = nameof(TestUnrecoverableSqlErrorsAreHandled);

				using var createTestProcDisposable = CreateAcquireOrUpdateSqlMutexForTest(TestProc, conversionFromVarcharToInt);
				using var mutexLock = NewSqlMutexLock(lockInfo, default, default, false);

				// Act
				var lockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals($"Should fail to acquire the lock due to sql error: {expectedException}", false, lockResult.HasAcquiredLock);
					AssertEquals("Original sql error number is returned", expectedException.Number, lockResult.ReturnValue);
					AssertEquals(true, lockResult.ReturnMessage.Contains("Conversion failed when converting the varchar value 'ABC' to data type int"));
				});
			}

			public void TestAcquireOrUpdateInMultiOpenTransactions()
			{
				// Arrange
				Db.Connection.BeginTransaction();
				Db.Connection.ExecuteNonQuery("DELETE [StmServiceMutex]");

				var mutexLocks = new SqlMutexLock[] { NewSqlMutexLock($"{nameof(TestAcquireSqlMutex_Success)}_1"), NewSqlMutexLock($"{nameof(TestAcquireSqlMutex_Success)}_2") };

				try
				{
					mutexLocks.ForEach(mutexLock =>
					{
						var dateTimeUtcNow = ZDateTime.UtcNow;

						// Act
						var exception = AssertExceptionThrown<SqlException>(() => _ = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock));

						// Assert
						Assert(exception.Message.Contains("AcquireOrUpdateSqlMutex must NOT be run in transaction"));
					});
				}
				finally
				{
					mutexLocks.ForEach(mutexLock =>
					{
						try
						{
							mutexLock.Dispose();
						}
						catch (SqlMutexLockReleaseException)
						{
							// ignore
						}
					});

					if (Db.Connection.IsInTransaction)
					{
						Db.Connection.RollbackTransaction();
					}
				}
			}

			[ExpectNoExceptions]
			public void TestAcquireSqlMutex_WithSqlErrors()
			{
				const int errorNumber = 15600;
				var raiseErrorAction = $"RAISERROR ({errorNumber}, 16, 1, 'This is an error to cater for test');";

				SqlMutexLockDbManager.OverridableAcquireOrUpdateSqlMutexProc.Value = TestProc;

				using var createTestProcDisposable = CreateAcquireOrUpdateSqlMutexForTest(TestProc, raiseErrorAction);

				var lockInfo = nameof(TestAcquireSqlMutex_WithSqlErrors);
				using (new DisposableAction(() => SqlMutexLockDbManager.OverridableAcquireOrUpdateSqlMutexProc.ResetValue()))
				using (var mutexLock = NewSqlMutexLock(lockInfo, default, default, false))
				{
					var lockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

					Assert("Failed due to sql error", !lockResult.HasAcquiredLock);
					AssertEquals(errorNumber, lockResult.ReturnValue);
				}
			}

			public void TestAcquireSqlMutex_Success()
			{
				var lockInfo = nameof(TestAcquireSqlMutex_Success);
				using (var mutexLock = NewSqlMutexLock(lockInfo))
				{
					var dateTimeUtcNow = ZDateTime.UtcNow;
					var lockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

					Assert("Acquire sql mutex succeeded", lockResult.HasAcquiredLock);
					Assert("Mutex expires at utc later than now", lockResult.ExpiresAtDateTimeUtc > dateTimeUtcNow);
					AssertNotEquals(DateTime.MinValue, lockResult.AcquiredDateTimeUtc);
				}
			}

			public void TestAcquireSqlMutex_Deny_ToOtherProcesses()
			{
				var lockInfo = nameof(TestAcquireSqlMutex_Deny_ToOtherProcesses);

				using (var mutexLock = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, default))
				using (var mutexLock_2 = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, testTimeOut, workStation, mutexLock.ProcessId + 1, TestConstants.UserCode, false, default))
				using (var mutexLock_3 = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, testTimeOut, Guid.NewGuid().ToString("N"), processId, TestConstants.UserCode, false, default))
				{
					var dateTimeUtcNow = ZDateTime.UtcNow;

					var lock1Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);
					Assert("Acquire sql mutex succeeded", lock1Result.HasAcquiredLock);
					Assert("Mutex expires at utc later than now", lock1Result.ExpiresAtDateTimeUtc > dateTimeUtcNow);

					var lock2Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock_2);

					Assert("Second attempt to another process should NOT return success", !lock2Result.HasAcquiredLock);
					AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, lock2Result.ReturnValue);
					Assert("Second returned expiry time reset to year 1900", lock2Result.ExpiresAtDateTimeUtc < dateTimeUtcNow);
					AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), lock2Result.ExpiresAtDateTimeUtc);

					var lock3Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock_3);

					Assert("Second attempt to another process should NOT return success", !lock3Result.HasAcquiredLock);
					AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, lock3Result.ReturnValue);
					Assert("Second returned expiry time reset to year 1900", lock3Result.ExpiresAtDateTimeUtc < dateTimeUtcNow);
					AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), lock3Result.ExpiresAtDateTimeUtc);
				}
			}

			public void TestUpdateSqlMutex_AfterExpiryTime_LostToOtherProcess()
			{
				var lockInfo = nameof(TestUpdateSqlMutex_AfterExpiryTime_LostToOtherProcess);
				var lockTimeout = TimeSpan.FromSeconds(1);

				using (var mutexLock = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, lockTimeout, workStation, processId, TestConstants.UserCode, false, default))
				using (var mutexLock_3 = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, lockTimeout, workStation, mutexLock.ProcessId + 1, TestConstants.UserCode, false, default))
				{
					var dateTimeUtcNow = ZDateTime.UtcNow;

					var lock1Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);
					CombineAssertions(lock1Result.ReturnMessage, () =>
					{
						AssertEquals($"Acquire sql mutex [{mutexLock.LockInfo}] failed: {lock1Result.ReturnMessage}.", true, lock1Result.HasAcquiredLock);
						AssertGreaterThan("Mutex expires at utc later than now", lock1Result.ExpiresAtDateTimeUtc, dateTimeUtcNow);
					});

					Thread.Sleep(TimeSpan.FromSeconds(mutexLock.LockTimeout.TotalSeconds / 2));
					var lock1Result2 = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);
					AssertEquals($"Sql mutex [{mutexLock.LockInfo}] has been kept alive", true, lock1Result2.HasAcquiredLock);
					AssertGreaterThan("Mutex expiry time has been updated to later", lock1Result2.ExpiresAtDateTimeUtc, lock1Result.ExpiresAtDateTimeUtc);

					using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
					{
						while (ZDateTime.UtcNow <= lock1Result2.ExpiresAtDateTimeUtc)
						{
							Thread.Sleep(100);
						}
						Thread.Sleep(100);
					}

					var lock3Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock_3);
					var lock1Result3 = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);

					CombineAssertions(() =>
					{
						AssertEquals("Acquire sql mutex failed", true, lock3Result.HasAcquiredLock);
						AssertGreaterThan("Mutex expiry time has not updated", lock3Result.ExpiresAtDateTimeUtc, DateTime.UtcNow);

						AssertEquals("1st process has lost the lock since failed to update before expiry", false, lock1Result3.HasAcquiredLock);
						AssertEquals("SQL error number 2601 should be returned as duplicate should exist", 2601, lock1Result3.ReturnValue);
						AssertEquals("Expires time should be set to year 1900", new DateTime(1900, 1, 1), lock1Result3.ExpiresAtDateTimeUtc);
					});
				}
			}

			public void TestReleasedLock_Available_ToOtherProcesses()
			{
				var lockInfo = nameof(TestReleasedLock_Available_ToOtherProcesses);

				using (var mutexLock = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, default))
				using (var mutexLock_2 = new SqlMutexLock(sqlMutexLockDbManager, lockInfo, TestConstants.Category, testTimeOut, Guid.NewGuid().ToString("N"), mutexLock.ProcessId + 1, TestConstants.UserCode, false, default))
				{
					var dateTimeUtcNow = ZDateTime.UtcNow;

					var lockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);
					Assert("Acquire sql mutex succeeded", lockResult.HasAcquiredLock);
					Assert("Mutex expires at utc later than now", lockResult.ExpiresAtDateTimeUtc > dateTimeUtcNow);

					var lock2Result = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock_2);

					Assert("Second attempt to another process should NOT return success", !lock2Result.HasAcquiredLock);
					AssertEquals("SQL error number 2601 returned as duplicate exist", 2601, lock2Result.ReturnValue);
					Assert("Second returned expiry time reset to year 1900", lock2Result.ExpiresAtDateTimeUtc < dateTimeUtcNow);
					AssertEquals("Expires time set to year 1900", new DateTime(1900, 1, 1), lock2Result.ExpiresAtDateTimeUtc);

					sqlMutexLockDbManager.ReleaseSqlMutexLock(mutexLock);
					var lock2Result2 = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock_2);

					Assert("Second attempt to another workstation should return success after other released", lock2Result2.HasAcquiredLock);
					Assert("Acquire sql mutex succeeded", lock2Result2.HasAcquiredLock);
					Assert("Mutex expires at utc later than now", lock2Result2.ExpiresAtDateTimeUtc > dateTimeUtcNow);
				}
			}
		}

		class AcquireOrUpdateTest : SqlMutexLockDbManagerTestBase
		{
			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestReleaseAcquiredLock_LockWasLost_ResultInNoErrors()
			{
				TestActionOnAcquiredLock_LockWasLost(
					mutexLock =>
					{
						mutexLock.Dispose();
					},
					$"{nameof(TestReleaseAcquiredLock_LockWasLost_ResultInNoErrors)}");
			}

			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestReleaseExpiredLock_LockWasLost_ResultInNoErrors()
			{
				TestActionOnAcquiredLock_LockWasLost(mutexLock =>
					{
						Thread.Sleep(mutexLock.LockTimeout);

						EnsureLockLost(mutexLock, () => mutexLock?.Dispose());
					},
					$"{nameof(TestReleaseExpiredLock_LockWasLost_ResultInNoErrors)}");
			}

			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestReleaseExpiredLock_LockWasLostToAnotherThread_ResultInNoErrors()
			{
				using var anotherThreadAcquiredTheLockEvent = new AutoResetEvent(false);
				TestActionOnAcquiredLock_LockWasLost(mutexLock =>
					{
						var anotherThread = new Thread(() =>
						{
							EnsureLockLost(mutexLock, () => anotherThreadAcquiredTheLockEvent.Set());
						});

						anotherThread.Start();

						anotherThreadAcquiredTheLockEvent.WaitOne();
						mutexLock.Dispose();
						anotherThreadAcquiredTheLockEvent.Set();

						anotherThread.Join();
					},
					$"{nameof(TestReleaseExpiredLock_LockWasLostToAnotherThread_ResultInNoErrors)}");
			}

			[ExpectNoExceptions]
			[UseSnapshotProtection]
			public void TestReleaseExpiredLockFromAnotherThread_LockWasLostToAnotherThread_ResultInNoErrors()
			{
				TestActionOnAcquiredLock_LockWasLost(mutexLock =>
					{
						var anotherThread = new Thread(() =>
						{
							EnsureLockLost(mutexLock, () => mutexLock?.Dispose());
						});

						anotherThread.Start();
						anotherThread.Join();
					},
					$"{nameof(TestReleaseExpiredLockFromAnotherThread_LockWasLostToAnotherThread_ResultInNoErrors)}");
			}

			void EnsureLockLost(SqlMutexLock mutexLock, Action lockAcquiredByAnotherProcessAction)
			{
				using (Db.DisposableActionForDbConnection())
				using (var newSqlMutexLock = NewSqlMutexLock(mutexLock.LockInfo, mutexLock.LockTimeout.Seconds, mutexLock.ProcessId + 1))
				{
					while (true)
					{
						try
						{
							// Lock was lost somehow
							Db.Connection.ExecuteNonQuery("DELETE dbo.StmServiceMutex");

							if (sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(newSqlMutexLock).HasAcquiredLock)
							{
								lockAcquiredByAnotherProcessAction?.Invoke();

								break;
							}
						}
						catch (SqlException e)
							when (new DbErrorMatch(e).ExceptionType == DbErrorType.DeadlockError)
						{
							// Deadlocked on resource and chosen as victim, retry
						}
					}
				}
			}

			void TestActionOnAcquiredLock_LockWasLost(Action<SqlMutexLock> actionOnLockLost, string code)
			{
				// Arrange
				const int timeOutInSeconds = 3;

				using var actionCompleteEvent = new AutoResetEvent(false);
				using var lockLostEvent = new AutoResetEvent(false);
				using var mutexLock = NewSqlMutexLock(code, timeOutInSeconds);
				var lockResult = sqlMutexLockDbManager.AcquireOrUpdateSqlMutexLock(mutexLock);
				Assert(lockResult.HasAcquiredLock);

				void OnLockLost(object sender, SqlMutexLockEventArgs args)
				{
					mutexLock.OnLockLost -= OnLockLost;
					lockLostEvent.Set();
				}

				mutexLock.OnLockLost += OnLockLost;

				var lockLostMonitorThread = new Thread(() =>
				{
					while (!lockLostEvent.WaitOne(10))
					{
						Thread.Sleep(10);
					}

					try
					{
						// Act
						actionOnLockLost?.Invoke(mutexLock);
					}
					finally
					{
						actionCompleteEvent.Set();
					}
				});
				lockLostMonitorThread.Start();
				while (!lockLostMonitorThread.IsAlive)
				{
					Thread.Sleep(10);
				}

				EnsureLockLost(mutexLock, () =>
				{
					while (!actionCompleteEvent.WaitOne(10))
					{
						Thread.Sleep(10);
					}
				});

				lockLostMonitorThread.Join();
			}
		}

		#region Implementations

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected SqlMutexLock NewSqlMutexLock(string lockInfo, int timeOutInSeconds = default, int lockProcessId = default, bool throwOnDispose = true)
		{
			return new SqlMutexLock(
				sqlMutexLockDbManager,
				lockInfo,
				TestConstants.Category,
				timeOutInSeconds == default ? testTimeOut : TimeSpan.FromSeconds(timeOutInSeconds),
				workStation,
				lockProcessId == default ? processId : lockProcessId,
				TestConstants.UserCode,
				throwOnDispose,
				default);
		}

		static IDisposable CreateAcquireOrUpdateSqlMutexForTest(string testProcName, string action)
		{
			if (string.Equals(testProcName, SqlMutexLockDbManager.AcquireOrUpdateSqlMutexProc, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("Please use a different name for the test procedure", nameof(testProcName));
			}

			var script = @"
SELECT DEFINITION
FROM sys.sql_modules as m
JOIN sys.procedures as p
    on m.object_id = p.object_id
WHERE 1 = 1
    AND p.name = @name
";
			var originalProc = Db.Connection.ExecuteScalar<string>(script, cmd =>
			{
				cmd.AddParameter("@name", System.Data.SqlDbType.VarChar, 128, SqlMutexLockDbManager.AcquireOrUpdateSqlMutexProc);
			});

			var regex = new Regex("BEGIN TRY", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			var testProc = regex.Replace(
				originalProc.Replace(
					$"CREATE PROCEDURE {SqlMutexLockDbManager.AcquireOrUpdateSqlMutexProc}",
					$"CREATE OR ALTER PROCEDURE {testProcName}"),
				new MatchEvaluator((match) => $@"
		BEGIN TRY
			{action}"), 1);

			Db.Connection.ExecuteNonQuery(testProc);
			SqlMutexLockDbManager.OverridableAcquireOrUpdateSqlMutexProc.Value = testProcName;

			return new DisposableAction(() =>
			{
				SqlMutexLockDbManager.OverridableAcquireOrUpdateSqlMutexProc.ResetValue();
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			sqlMutexLockDbManager = new SqlMutexLockDbManager();
			workStation = System.Environment.MachineName;
			processId = Process.GetCurrentProcess().Id;

			Db.Connection.ExecuteNonQuery("DELETE [StmServiceMutex]");
		}

		SqlMutexLockDbManager sqlMutexLockDbManager;
		readonly TimeSpan testTimeOut = TimeSpan.FromSeconds(TestConstants.TenSecondsLockTimeOut);

		string workStation;
		int processId;

		const string TestProc = "AcquireOrUpdateSqlMutexForTest";

		#endregion
	}
}
