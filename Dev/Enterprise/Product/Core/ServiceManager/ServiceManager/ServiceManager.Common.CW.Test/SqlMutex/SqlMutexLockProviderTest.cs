using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Semaphores.Common.Test;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using static System.FormattableString;

namespace Enterprise.Semaphores.Common.Testing
{
	[UseSnapshotProtection]
	class SqlMutexLockProviderTest : TestCase
	{
		public class MiscellaneousTest : SqlMutexLockProviderTest
		{
			public void TestTryGetLock_Success()
			{
				var lockInfo = nameof(TestTryGetLock_Success);
				var result = lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode);

				Assert(returnMessage, result);
				using (mutexLock)
				{
					var sqlMutexLock = mutexLock as SqlMutexLock;

					Assert(mutexLock is SqlMutexLock);
					AssertNotNull(sqlMutexLock);
					Assert(sqlMutexLock.LatestSqlMutexLockResult.HasAcquiredLock);
					Assert(sqlMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc >= sqlMutexLock.LatestSqlMutexLockResult.AcquiredDateTimeUtc + testTimeOut);

					Thread.Sleep(testTimeOut);

					result = lockProvider.TryGetLock(out var mutexLock1, out _, out returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode);
					Assert(returnMessage, result);
					var sqlMutexLock1 = mutexLock1 as SqlMutexLock;

					AssertNotNull(sqlMutexLock1);
					Assert(sqlMutexLock1.LatestSqlMutexLockResult.HasAcquiredLock);
					Assert(sqlMutexLock1.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc > sqlMutexLock.LatestSqlMutexLockResult.ExpiresAtDateTimeUtc);
					mutexLock1.Dispose();
				}
			}

			public void TestTryGetLock_Failed_FromOtherWorkStation()
			{
				var lockInfo = nameof(TestTryGetLock_Failed_FromOtherWorkStation);
				var result = lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode);
				Assert(returnMessage, result);

				using (mutexLock)
				{
					var result1 = lockProvider.TryGetLock(out var mutexLock1, out var sqlErrorNumber, out returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode, machineName: "Other machine");
					Assert(returnMessage, !result1);
					AssertNull(mutexLock1);
					AssertEquals(2601, sqlErrorNumber);
					mutexLock1?.Dispose();
				}
			}

			public void TestTryGetLock_Failed_FromOtherProcess()
			{
				var lockInfo = nameof(TestTryGetLock_Failed_FromOtherProcess);
				var result = lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode);
				Assert(returnMessage, result);

				using (mutexLock)
				{
					var process1 = (mutexLock as SqlMutexLock).ProcessId + 1;
					var result1 = lockProvider.TryGetLock(out var mutexLock1, out var sqlErrorNumber, out returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode, processId: process1);

					Assert(returnMessage, !result1);
					AssertNull(mutexLock1);
					mutexLock1?.Dispose();
				}
			}
		}

		public class ThrowOnDisposeTest : SqlMutexLockProviderTest
		{
			public void TestTryGetLock_ThrowOnDispose_Default()
			{
				// Arrange
				var lockInfo = nameof(TestTryGetLock_ThrowOnDispose_Default);
				lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode);
				using (mutexLock)
				{
					var sqlMutexLock = (SqlMutexLock)mutexLock;

					// Act
					var result = sqlMutexLock.ThrowOnDispose;

					// Assert
					AssertEquals(returnMessage, false, result);
				}
			}

			public void TestTryGetLock_ThrowOnDispose_False()
			{
				// Arrange
				var lockInfo = nameof(TestTryGetLock_ThrowOnDispose_False);
				lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode, throwOnLockDispose: false);
				using (mutexLock)
				{
					var sqlMutexLock = (SqlMutexLock)mutexLock;

					// Act
					var result = sqlMutexLock.ThrowOnDispose;

					// Assert
					AssertEquals(returnMessage, false, result);
				}
			}

			public void TestTryGetLock_ThrowOnDispose_True()
			{
				// Arrange
				var lockInfo = nameof(TestTryGetLock_ThrowOnDispose_True);
				lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, TestConstants.Category, testTimeOut, TestConstants.UserCode, throwOnLockDispose: true);
				using (mutexLock)
				{
					var sqlMutexLock = (SqlMutexLock)mutexLock;

					// Act
					var result = sqlMutexLock.ThrowOnDispose;

					// Assert
					AssertEquals(returnMessage, true, result);
				}
			}
		}

		public class GetUnobservedLockTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				mutexLockDbManagerMock = new Mock<ISqlMutexLockDbManager>();
				lockProvider = new SqlMutexLockProvider(mutexLockDbManagerMock.Object);
			}

			public void TestGetUnobservedLock()
			{
				CombineAssertions(() =>
				{
					Test("category",
						"lockInfo",
						TimeSpan.FromSeconds(1),
						"userCode",
						"machineName",
						1,
						new SqlMutexLockResult(0, DateTime.UtcNow.AddDays(1), DateTime.UtcNow, string.Empty),
						true);
					Test("category2",
						"lockInfo2",
						TimeSpan.FromSeconds(2),
						"userCode2",
						"machineName2",
						2,
						new SqlMutexLockResult(1, new DateTime(), new DateTime(), string.Empty),
						false);
				});

				void Test(string category, string lockInfo, TimeSpan timeout, string userCode, string machineName, int processId, SqlMutexLockResult sqlMutexLockResult, bool expected)
				{
					// Arrange
					mutexLockDbManagerMock.Invocations.Clear();
					mutexLockDbManagerMock
						.Setup(manager => manager.AcquireOrUpdateSqlMutexLock(
							It.IsAny<string>(),
							It.IsAny<string>(),
							It.IsAny<TimeSpan>(),
							It.IsAny<string>(),
							It.IsAny<int>(),
							It.IsAny<string>()))
						.Returns(sqlMutexLockResult);

					// Act
					var result = lockProvider.GetUnobservedLock(category,
						lockInfo,
						timeout,
						userCode,
						machineName,
						processId);

					// Assert
					mutexLockDbManagerMock.Verify(
						manager => manager.AcquireOrUpdateSqlMutexLock(category,
							lockInfo,
							timeout,
							machineName,
							processId,
							userCode),
						Times.Once);
					mutexLockDbManagerMock.VerifyNoOtherCalls();
					AssertEquals(expected, result);
				}
			}

			[ExpectNoExceptions]
			public void TestDefaultValues()
			{
				// Arrange
				mutexLockDbManagerMock.Invocations.Clear();
				mutexLockDbManagerMock
					.Setup(manager => manager.AcquireOrUpdateSqlMutexLock(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<TimeSpan>(),
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<string>()))
					.Returns(new SqlMutexLockResult(0, new DateTime(), new DateTime(), string.Empty));

				// Act
				var result = lockProvider.GetUnobservedLock("category", "lockInfo", TimeSpan.Zero, "userCode");

				// Assert
				mutexLockDbManagerMock.Verify(
					manager => manager.AcquireOrUpdateSqlMutexLock(It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<TimeSpan>(),
						System.Environment.MachineName,
						Process.GetCurrentProcess().Id,
						It.IsAny<string>()),
					Times.Once);
				mutexLockDbManagerMock.VerifyNoOtherCalls();
			}

			public void TestWrongParamsCall()
			{
				var result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock(null, "category", TimeSpan.Zero, "userCode"));
				AssertEquals("lockInfo", result.ParamName);

				result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock(string.Empty, "category", TimeSpan.Zero, "userCode"));
				AssertEquals("lockInfo", result.ParamName);

				result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock("lockInfo", null, TimeSpan.Zero, "userCode"));
				AssertEquals("category", result.ParamName);

				result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock("lockInfo", string.Empty, TimeSpan.Zero, "userCode"));
				AssertEquals("category", result.ParamName);

				result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock("lockInfo", "category", TimeSpan.Zero, null));
				AssertEquals("userCode", result.ParamName);

				result = AssertExceptionThrown<ArgumentException>(() => _ = lockProvider.GetUnobservedLock("lockInfo", "category", TimeSpan.Zero, string.Empty));
				AssertEquals("userCode", result.ParamName);
			}

			SqlMutexLockProvider lockProvider;
			Mock<ISqlMutexLockDbManager> mutexLockDbManagerMock;
		}

		protected override void SetUp()
		{
			base.SetUp();

			mutexLockDbManager = new SqlMutexLockDbManager();
			lockProvider = new SqlMutexLockProvider(mutexLockDbManager);
		}

		ISqlMutexLockProvider lockProvider;
		ISqlMutexLockDbManager mutexLockDbManager;
		readonly TimeSpan testTimeOut = TimeSpan.FromSeconds(1);

		public class SqlMutexLockPerformanceTest : TestCase
		{
			[ExpectNoExceptions]
			[DeveloperOnlyTest]
			public void TestSqlMutexLockPerformance()
			{
				try
				{
					var threads = new List<Thread>();

					for (var i = 0; i < 365; i++)
					{
						var x = i + 1;
						threads.Add(new Thread(() => RunClient("BBB", $"Lock BBB - {nameof(TestSqlMutexLockPerformance)}")));
						threads.Add(new Thread(() => RunClient("AAA", Invariant($"Lock {x:d3}- {nameof(TestSqlMutexLockPerformance)}"))));
						threads.Add(new Thread(() => RunClient("CCC", $"Lock CCC - {nameof(TestSqlMutexLockPerformance)}")));
					}

					foreach (var thread in threads)
					{
						thread.Start();
					}

					foreach (var thread in threads)
					{
						thread.Join();
					}

					if (exceptionThrown != null)
					{
						throw exceptionThrown;
					}
				}
				finally
				{
					var datRootFolder = Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "CargoWise edi"), "DAT");
					if (!Directory.Exists(datRootFolder))
					{
						Directory.CreateDirectory(datRootFolder);
					}

					var logfile = Path.Combine(datRootFolder, nameof(SqlMutexLockPerformanceTest));
					var logfilePath = Path.ChangeExtension(logfile, ".txt");
					if (File.Exists(logfilePath))
					{
						File.Delete(logfilePath);
					}

					using (var writer = new StreamWriter(logfilePath, true, Encoding.UTF8))
					{
						while (logs.TryDequeue(out var line))
						{
							writer.WriteLine(line);
						}
					}

					Db.Connection.ExecuteNonQuery("delete from dbo.StmServiceMutex");
					DbCommitTracker.Reset();
				}
			}

			public void RunClient(string category, string lockInfo)
			{
				const string userCode = "~TS";
				var testTimeOut = TimeSpan.FromSeconds(1);

				using (Db.DisposableActionForDbConnection())
				{
					try
					{
						const string attemptTo = "attempting to acquire lock";
						const string success = "lock acquired";
						const string failed = "failed to acquire lock";
						const string released = "lock released";

						var threadId = Thread.CurrentThread.ManagedThreadId;
						var acquisition = Invariant($"({lockInfo})\t{category}");
						var random = new Random();
						var timeStart = DateTime.Now;

						logs.Enqueue(Invariant($"{timeStart:HH:mm:ss.fff}\t[Thread {threadId:d5}]\t{attemptTo} {acquisition}"));
						var lockResult = lockProvider.TryGetLock(out var mutexLock, out _, out var returnMessage, lockInfo, category, testTimeOut, userCode, Guid.NewGuid().ToString("N"), random.Next(1000, 100_1000), false);

						var resultTime = DateTime.Now;
						var elapsed = resultTime - timeStart;
						logs.Enqueue(Invariant($"{resultTime:HH:mm:ss.fff}\t[Thread {threadId:d5}]\twait={elapsed.TotalMilliseconds}ms, {(lockResult ? success : failed)} {acquisition}, {returnMessage}"));

						if (lockResult)
						{
							using (mutexLock)
							{
								Thread.Sleep(TimeSpan.FromSeconds(5));
							}

							var releaseTime = DateTime.Now;
							elapsed = releaseTime - timeStart;
							logs.Enqueue(Invariant($"{releaseTime:HH:mm:ss.fff}\t[Thread {threadId:d5}]\treleased={elapsed.TotalSeconds}s, {released} {acquisition}"));
						}
						mutexLock?.Dispose();
					}
					catch (Exception ex)
					{
						if (exceptionThrown == null)
						{
							exceptionThrown = ex;
						}
					}
				}
			}

			Exception exceptionThrown;
			readonly ConcurrentQueue<string> logs = new ConcurrentQueue<string>();
			readonly SqlMutexLockProvider lockProvider = new SqlMutexLockProvider(new SqlMutexLockDbManager());
		}
	}
}
