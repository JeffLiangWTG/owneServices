using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Semaphores.Common.Test;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;

namespace Enterprise.Semaphores.Common.Testing
{
	[UseSnapshotProtection]
	class SqlMutexLockTest : TestCase
	{
		public void TestMutexLockConstructor()
		{
			var lockInfo = nameof(TestMutexLockConstructor);
			var lockResult = new SqlMutexLockResult(0, DateTime.UtcNow, DateTime.UtcNow, lockInfo);
			using (var mutexLock = new SqlMutexLock(sqlMutexLockProvider, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, lockResult))
			{
				CombineAssertions(() =>
				{
					AssertEquals(lockInfo, mutexLock.LockInfo);
					AssertEquals(TestConstants.Category, mutexLock.Category);
					AssertEquals(testTimeOut, mutexLock.LockTimeout);
					AssertEquals(TestConstants.UserCode, mutexLock.UserCode);
					AssertEquals(workStation, mutexLock.WorkStationName);
					AssertEquals(processId, mutexLock.ProcessId);
					AssertEquals(lockResult, mutexLock.LatestSqlMutexLockResult);
					AssertEquals(lockResult.ReturnMessage, mutexLock.ReturnMessage);
				});
			}
		}

		public void TestLockAcquiredAndUpdated()
		{
			var lockInfo = nameof(TestLockAcquiredAndUpdated);
			using (var mutexLock = new SqlMutexLock(sqlMutexLockProvider, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, default))
			{
				AssertNull("We haven't acquired the lock yet", mutexLock.LatestSqlMutexLockResult);

				var lastExpiryDateTimeUtc = DateTime.UtcNow;
				var repeat = -1;

				while (++repeat < 3)
				{
					Thread.Sleep(mutexLock.TimerTickInterval.Add(TimeSpan.FromMilliseconds(100)));

					AssertNotNull(mutexLock.LatestSqlMutexLockResult);
					Assert("We have successfully acquired the lock now", mutexLock.LatestSqlMutexLockResult.HasAcquiredLock);
					Assert("We have successfully updated the lock expiry time", mutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc > lastExpiryDateTimeUtc);
					Assert("Lock result contains return message", !string.IsNullOrEmpty(mutexLock.LatestSqlMutexLockResult.ReturnMessage));
				}
			}
		}

		public void TestLockLostAfterMaxLostCount()
		{
			var lockInfo = nameof(TestLockLostAfterMaxLostCount);
			using (var mutexLock = new SqlMutexLock(sqlMutexLockProvider, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, default))
			{
				var lostLock = string.Empty;
				var lockResult = int.MinValue;
				mutexLock.OnLockLost += (sender, args) =>
				{
					lostLock = args.LockInfo;
					lockResult = args.LockResult;
				};

				Thread.Sleep(mutexLock.TimerTickInterval.Add(TimeSpan.FromMilliseconds(100)));

				Assert("We have successfully acquired the lock already", mutexLock.LatestSqlMutexLockResult.HasAcquiredLock);
				Assert(string.IsNullOrEmpty(lostLock));

				using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
				{
					while (mutexLock.lockLostCount < SqlMutexLock.LockLostMaxCount)
					{
						Thread.Sleep(100);
					}
				}

				AssertEquals(lockInfo, lostLock);
				AssertEquals("The result was from the last operation before lock was lost", 0, lockResult);
			}
		}

		public void TestLockLostDatabaseUpgradedExceptionHasBeenThrown()
		{
			var lockInfo = nameof(TestLockLostDatabaseUpgradedExceptionHasBeenThrown);
			using (var mutexLock = new SqlMutexLock(sqlMutexLockProvider, lockInfo, TestConstants.Category, testTimeOut, workStation, processId, TestConstants.UserCode, false, default))
			{
				var lostLock = string.Empty;
				var lockResult = 0;
				mutexLock.OnLockLost += (sender, args) =>
				{
					lostLock = args.LockInfo;
					lockResult = args.LockResult;
				};

				Thread.Sleep(mutexLock.TimerTickInterval.Add(TimeSpan.FromMilliseconds(100)));

				Assert("We have successfully acquired the lock already", mutexLock.LatestSqlMutexLockResult.HasAcquiredLock);
				Assert(string.IsNullOrEmpty(lostLock));

				try
				{
					// Arrange
					Db.Connection.EnsureIsOpen();
					Db.Connection.ExecuteNonQuery("UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(8000), N'-1') WHERE SD_Name = 'DATABASE_MINOR_SCHEMA_VERSION';");

					while (mutexLock.lockLostCount < SqlMutexLock.LockLostMaxCount)
					{
						Thread.Sleep(100);
					}

					AssertEquals(lockInfo, lostLock);
					AssertEquals(0, lockResult);
				}
				finally
				{
					Db.Connection.ResetDatabaseUpgradedExceptionHasBeenThrown();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			sqlMutexLockProvider = new SqlMutexLockDbManager();
			workStation = System.Environment.MachineName;
			processId = Process.GetCurrentProcess().Id;
		}

		string workStation;
		int processId;

		ISqlMutexLockDbManager sqlMutexLockProvider;
		readonly TimeSpan testTimeOut = TimeSpan.FromSeconds(1);

		public class ThrowOnDisposeTest : TestCase
		{
			public void TestMutexLockConstructor()
			{
				sqlMutexLockProviderMock
					.Setup(manager => manager.ReleaseSqlMutexLock(It.IsAny<SqlMutexLock>()))
					.Returns(new SqlMutexLockResult(default, default, default, string.Empty));

				CombineAssertions(() =>
				{
					Test(true);
					Test(false);
				});

				void Test(bool throwOnDispose)
				{
					using (var mutexLock = NewSqlMutexLock(nameof(TestMutexLockConstructor), throwOnDispose))
					{
						AssertEquals(throwOnDispose, mutexLock.ThrowOnDispose);
					}
				}
			}

			public void TestDisposeDoesNotThrow()
			{
				// Arrange
				using (var mutexLock = NewSqlMutexLock(nameof(TestDisposeDoesNotThrow), false))
				{
					sqlMutexLockProviderMock
						.Setup(manager => manager.ReleaseSqlMutexLock(It.IsAny<SqlMutexLock>()))
						.Throws<InvalidOperationException>();

					// Act
					// Assert
					AssertNoExceptionThrown(() => mutexLock.Dispose());
				}
			}

			public void TestDisposeThrowsExceptionsFromRelease()
			{
				CombineAssertions(() =>
				{
					Test<InvalidOperationException>();
					Test<OverflowException>();
					Test<Exception>();
				});

				void Test<T>() where T : Exception, new()
				{
					// Arrange
					var mutexLock = NewSqlMutexLock(nameof(TestDisposeThrowsExceptionsFromRelease), true);
					try
					{
						sqlMutexLockProviderMock
							.Setup(manager => manager.ReleaseSqlMutexLock(It.IsAny<SqlMutexLock>()))
							.Throws<T>();

						// Act
						// Assert
						var result = AssertExceptionThrown<SqlMutexLockReleaseException>(() => mutexLock.Dispose());
						mutexLock = null;
						AssertType<T>(result?.InnerException);
					}
					finally
					{
						mutexLock?.Dispose();
					}
				}
			}

			public void TestDisposeThrowsLockReleaseExceptionIfSqlResultIsNotZero()
			{
				CombineAssertions(() =>
				{
					Test(1);
					Test(10);
					Test(100);
				});

				void Test(int returnValue)
				{
					// Arrange
					var mutexLock = NewSqlMutexLock(nameof(TestDisposeThrowsLockReleaseExceptionIfSqlResultIsNotZero), true);
					try
					{
						var returnMessage = nameof(TestDisposeThrowsLockReleaseExceptionIfSqlResultIsNotZero);
						var expectedExceptionMessage = $"{returnValue}{System.Environment.NewLine}{returnMessage}";
						sqlMutexLockProviderMock
							.Setup(manager => manager.ReleaseSqlMutexLock(It.IsAny<SqlMutexLock>()))
							.Returns(new SqlMutexLockResult(returnValue, default, default, returnMessage));

						// Act
						// Assert
						var result = AssertExceptionThrown<SqlMutexLockReleaseException>(() =>
						{
							using (new DisposableAction(() => { SqlMutexLock.ReleaseLockTimeout.ResetValue(); }))
							{
								SqlMutexLock.ReleaseLockTimeout.Value = TimeSpan.FromMilliseconds(100);
								mutexLock.Dispose();
							}
						});

						AssertNotNull(result);
						AssertEquals(expectedExceptionMessage, result.Message);
					}
					finally
					{
						mutexLock?.Dispose();
					}
				}
			}

			SqlMutexLock NewSqlMutexLock(string lockInfo, bool throwOnDispose)
			{
				return new SqlMutexLock(sqlMutexLockProviderMock.Object, lockInfo, TestConstants.Category, Timeout.InfiniteTimeSpan, "workstation", 0, "TestConstants.UserCode", throwOnDispose, default);
			}

			protected override void SetUp()
			{
				base.SetUp();
				sqlMutexLockProviderMock = new Mock<ISqlMutexLockDbManager>();
			}

			Mock<ISqlMutexLockDbManager> sqlMutexLockProviderMock;
		}
	}
}
