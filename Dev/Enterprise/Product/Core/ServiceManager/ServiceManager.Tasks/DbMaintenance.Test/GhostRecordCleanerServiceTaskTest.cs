using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(GhostRecordCleanerServiceTask))]
	sealed class GhostRecordCleanerServiceTaskTest : ServiceTaskTestCase<GhostRecordCleanerServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		[TestUtcOffset(2, 0, 0)]
		public void TestInitialiseSchedule()
		{
			var ghostRecordCleanerTask = new GhostRecordCleanerServiceTask();

			InitialiseTaskSchedule(ghostRecordCleanerTask, out StmServiceTask taskSchedule);
			CombineAssertions(() =>
			{
				Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("ScheduleTask - TaskPeriodCount", 5, taskSchedule.Recurrence.Period);
				AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			});
		}

		public void TestActiveByDefault_SelfHosted()
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("");

			try
			{
				var ghostRecordCleanerTask = new GhostRecordCleanerServiceTask();
				InitialiseTaskSchedule(ghostRecordCleanerTask, out StmServiceTask taskSchedule);
				AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestActiveByDefault()
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");

			try
			{
				var ghostRecordCleanerTask = new GhostRecordCleanerServiceTask();
				InitialiseTaskSchedule(ghostRecordCleanerTask, out StmServiceTask taskSchedule);
				AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestLockTimeoutDoesNotChangeAfterServiceRun()
		{
			var defaultLockTimeout = (int)Db.Connection.ExecuteScalar("select @@LOCK_TIMEOUT");
			try
			{
				AssertNoExceptionThrown(() =>
				{
					new GhostRecordCleanerServiceTask() { ServiceLogger = Mock.Of<ILogger>() }.RunTask();
				});
			}
			finally
			{
				AssertEquals(defaultLockTimeout, (int)Db.Connection.ExecuteScalar("select @@LOCK_TIMEOUT"));
			}
		}

		public void TestSqlExceptionIndexOperationAlreadyInProgress_IsHandled()
		{
			var task = new Mock<GhostRecordCleanerServiceTask>(-1, 1, 1, 1, -1, -1, true, 15000) { CallBase = true };
			var sqlException = SqlExceptionBuilder.CreateSqlException(1912, "Could not proceed with index DDL operation on blah blah because it conflicts with another concurrent operation that is already in progress on the object");
			var loggerMock = new Mock<ILogger>();
			loggerMock.Setup(x => x.Log(LogType.Warning, It.IsAny<string>())).Verifiable();
			task.Object.ServiceLogger = loggerMock.Object;

			task.Setup(x => x.RebuildIndividualIndex(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(sqlException);

			//Act
			AssertNoExceptionThrown(task.Object.RunTask);

			loggerMock.Verify(x =>
					x.Log(LogType.Warning, It.Is<string>(message => message.Contains("due to another process working on the index"))),
				Times.Exactly(task.Object.GhostRecordCleanerIndexes.Count()));
		}

		[UseSnapshotProtection(true)]
		public void TestAppliesLowPriorityWait()
		{
			// Arrange
			var task = new GhostRecordCleanerServiceTask(-1, 2, 1 ,1)
			{
				ServiceLogger = Mock.Of<ILogger>()
			};

			var indexCount = task.GhostRecordCleanerIndexes.Count();
			AssertGreaterThan(
				$"Pre-condition: {nameof(GhostRecordCleanerServiceTask)}.{nameof(task.GhostRecordCleanerIndexes)} returns some indexes",
				indexCount,
				0);

			const string expectedPart = "REBUILD WITH (ONLINE = ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 2 MINUTES, ABORT_AFTER_WAIT = SELF)))";

			using (Db.Connection.TrackExecutedCommands())
			{
				// Act
				task.RunTask();

				// Assert
				var correctRebuildCommands = Db.Connection
					.ExecutedCommands
					.Count(c => c.Contains(expectedPart));

				AssertEquals(
					$@"Commands applied the low priority wait correctly.
Correct command should contain:
{expectedPart}

A sample executed ALTER INDEX is:
{Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("ALTER INDEX"))}",
					indexCount,
					correctRebuildCommands);
			}
		}

		public void TestDefaultValuesAreReadFromRegistry()
		{
			// Arrange
			using (RawDataRegistry.Instance.GhostRecordMaxProcessingThreadCount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (RawDataRegistry.Instance.GhostRecordProcessingThreadInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (RawDataRegistry.Instance.GhostRecordCleanUpThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			using (RawDataRegistry.Instance.GhostRecordCleanUpRebuildWaitMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				// Act
				var task = new GhostRecordCleanerServiceTask();

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(nameof(task.MaxProcessingThreadCount), 5, task.MaxProcessingThreadCount);
					AssertEquals(nameof(task.ProcessingThreadInterval), 10, task.ProcessingThreadInterval);
					AssertEquals(nameof(task.GhostCountThreshold), 50, task.GhostCountThreshold);
					AssertEquals(nameof(task.RebuildLowPriorityWaitMinutes), 10, task.RebuildLowPriorityWaitMinutes);
				});
			}
		}

		[UseSnapshotProtection(true)]
		public void TestMoreThanOneIndexReturnedInSubquery()
		{
			// Arrange
			const string tempTableName = nameof(TestMoreThanOneIndexReturnedInSubquery);
			var serviceTask = new GhostRecordCleanerServiceTask(-1) { ServiceLogger = Mock.Of<ILogger>() };
			var indexName = serviceTask.GhostRecordCleanerIndexes.First().indexName;

			Db.Connection.ExecuteNonQuery($"CREATE TABLE {tempTableName}(Id Int)");
			Db.Connection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [{indexName}] On {tempTableName}(Id)");

			// Act
			// Assert
			AssertNoExceptionThrown(() => serviceTask.RunTask(CancellationToken.None));
		}

		[UseSnapshotProtection(true)]
		public void TestMoreThanOneIndexReturnedInSubqueryDifferentSchema()
		{
			// Arrange
			const string schemaName = "StrangeSchema";
			var serviceTask = new GhostRecordCleanerServiceTask(-1) { ServiceLogger = Mock.Of<ILogger>() };
			var (_, tableName, indexName) = serviceTask.GhostRecordCleanerIndexes.First();

			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA [{schemaName}]");
			Db.Connection.ExecuteNonQuery($"CREATE TABLE [{schemaName}].{tableName}(Id Int)");
			Db.Connection.ExecuteNonQuery($"CREATE CLUSTERED INDEX [{indexName}] On [{schemaName}].{tableName}(Id)");

			// Act
			// Assert
			AssertNoExceptionThrown(() => serviceTask.RunTask(CancellationToken.None));
		}

		public void TestGetsIndexesToRebuildReturnsExpectedIndexes()
		{
			//Arrange
			const int threshold = 10;
			var serviceTask = new Mock<GhostRecordCleanerServiceTask>(threshold, 1, 1, 5, -1, -1, true, 15000) { CallBase = true, Object = { ServiceLogger = Mock.Of<ILogger>() } };
			var indexesToRebuild = new List<(string schema, string table, string index)>
			{
				("dbo", "dummyTable", "dummyIndex1"),
				("dbo", "dummyTable2", "dummyIndex2"),
				("dbo", "dummyTable3", "dummyIndex3"),
				("dbo", "dummyTable4", "dummyIndex4"),
				("dbo", "dummyTable5", "dummyIndex5"),
				("dbo", "dummyTable6", "dummyIndex6"),
			};
			for (int i = 0; i < 3; i++)
			{
				var grcIndex = indexesToRebuild[i];
				serviceTask.Setup(x => x.GetGhostRecordCountForIndex(grcIndex)).Returns(threshold + 1);
			}

			serviceTask.SetupGet(x => x.GhostRecordCleanerIndexes).Returns(indexesToRebuild);

			//Act
			var foundIndexes = serviceTask.Object.GetIndexesToRebuild(CancellationToken.None);

			//Assert
			CombineAssertions(() =>
			{
				AssertEquals(3, foundIndexes.Count());
				AssertContainsExactElementsInAnyOrder(indexesToRebuild.Take(3), foundIndexes);
			});
		}

		[UseSnapshotProtection(true)]
		public void TestProcessesAllIndexesForTablesWithThreadOptimization()
		{
			//Arrange
			const int threadSleepSeconds = 3;

			var serviceTask = new Mock<GhostRecordCleanerServiceTask>(-1, 1, 1, 5, -1, -1, true, 15000) { CallBase = true, Object = { ServiceLogger = Mock.Of<ILogger>() } };
			var indexesToRebuild = GhostRecordCleanerIndexGroup.GhostRecordCleanerIndexes.Take(10).ToList();
			var individualTableCount = indexesToRebuild.GroupBy(x => (x.schemaName, x.tableName)).Count();
			var processedCount = 0;

			serviceTask.SetupGet(x => x.GhostRecordCleanerIndexes).Returns(indexesToRebuild);
			serviceTask.Setup(x => x.RebuildIndividualIndex(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback(() => Interlocked.Increment(ref processedCount));
			var stopwatch = Stopwatch.StartNew();

			//Act
			serviceTask.Object.RunTask();

			//Assert
			stopwatch.Stop();
			CombineAssertions(() =>
			{
				AssertEquals($"All indexes should have been processed, expected {indexesToRebuild.Count}", indexesToRebuild.Count, processedCount);
				Assert("Should have processed quicker than combined sleep duration due to thread optimization", stopwatch.Elapsed < TimeSpan.FromSeconds(individualTableCount * threadSleepSeconds));
			});
		}

		[UseSnapshotProtection(true)]
		public void TestProcessingRebuildCreatesLogs()
		{
			//Arrange
			var logger = new Mock<ILogger>();
			var logs = new ConcurrentStack<string>();
			logger.Setup(x => x.Log(LogType.Debug, It.IsAny<string>())).Callback<LogType, string>((_, msg) => logs.Push(msg));
			var serviceTask = new Mock<GhostRecordCleanerServiceTask>(-1, 1, 1, 5, -1, -1, true, 15000) { CallBase = true, Object = { ServiceLogger = logger.Object } };
			serviceTask.Setup(x => x.RebuildIndividualIndex(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback<string, string, string>((_, _, _) => Thread.Sleep(1000));

			//Act
			serviceTask.Object.RunTask();

			//Assert
			var rebuildLogs = logs.Where(x => x.Contains("Rebuilding index"));
			var checkingIndexLogs = logs.Where(x => x.Contains("Checking index"));
			var addLogs = logs.Where(x => x.Contains("Adding index"));

			CombineAssertions(() =>
			{
				AssertEquals(serviceTask.Object.GhostRecordCleanerIndexes.Count(), rebuildLogs.Count());
				AssertEquals(serviceTask.Object.GhostRecordCleanerIndexes.Count(), addLogs.Count());
				AssertEquals(serviceTask.Object.GhostRecordCleanerIndexes.Count(), checkingIndexLogs.Count());
			});
		}

		public void TestGetCommandTimeout()
		{
			CombineAssertions(() =>
			{
				TestGetCommandTimeout(10, 10000, 1);
				TestGetCommandTimeout(0, 10000, 0);
				TestGetCommandTimeout(10, 0, 0);
				TestGetCommandTimeout(10, DbConnection.LockTimeout.Infinite, DbCommand.Timeout.Infinite);
			});

			void TestGetCommandTimeout(int commandTimeOutThreshold, int connectionLockTimeout, int expectedTimeout)
			{
				// Arrange
				// Act
				var result = GhostRecordCleanerServiceTask.GetCommandTimeout(commandTimeOutThreshold, connectionLockTimeout);

				// Assert
				AssertEquals(expectedTimeout, result);
			}
		}

		public void TestGetKillBlockersWaitTime()
		{
			CombineAssertions(() =>
			{
				TestGetKillBlockersWaitTime(10, 10000, TimeSpan.FromSeconds(1));
				TestGetKillBlockersWaitTime(0, 10000, TimeSpan.FromSeconds(0));
				TestGetKillBlockersWaitTime(10, 0, TimeSpan.FromSeconds(0));
				TestGetKillBlockersWaitTime(10, DbConnection.LockTimeout.Infinite, TimeSpan.MaxValue);
				TestGetKillBlockersWaitTime(-1, 10000, TimeSpan.MaxValue);
			});

			void TestGetKillBlockersWaitTime(int killBlockersThreshold, int connectionLockTimeout, TimeSpan expectedTime)
			{
				// Arrange
				// Act
				var result = GhostRecordCleanerServiceTask.GetKillBlockersWaitTime(killBlockersThreshold, connectionLockTimeout);

				// Assert
				AssertEquals(expectedTime, result);
			}
		}

		public void TestGetOnlineOption()
		{
			CombineAssertions(() =>
			{
				TestGetOnlineOption(true, 2, "ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = 2 MINUTES, ABORT_AFTER_WAIT = SELF))");
				TestGetOnlineOption(false, 2, "OFF");
			});

			void TestGetOnlineOption(bool onlineRebuild, int rebuildLowPriorityWait, string expectedTime)
			{
				// Arrange
				// Act
				var result = GhostRecordCleanerServiceTask.GetOnlineOption(onlineRebuild, rebuildLowPriorityWait);

				// Assert
				AssertEquals(expectedTime, result);
			}
		}

		public class RebuildIndividualIndexTest : TestCase
		{
			[UseSnapshotProtection]
			public void TestRebuildIndividualIndexWithOfflineOption()
			{
				// Arrange
				CreateTestTableAndIndex(testTableName, testIndexName);

				using (RawDataRegistry.Instance.GhostRecordCleanupKillBlockersThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
				using (RawDataRegistry.Instance.GhostRecordCleanupOnlineRebuild.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (Db.Connection.TrackExecutedCommands())
				{
					var grcTask = new GhostRecordCleanerServiceTask();

					// Act
					grcTask.RebuildIndividualIndex("dbo", testTableName, testIndexName);

					// Assert
					AssertEquals(1, Db.Connection.ExecutedCommands.Count(p => p.Contains("REBUILD WITH (ONLINE = OFF)")));
				}
			}

			[UseSnapshotProtection]
			public void TestRebuildIndividualIndexKillBlockerConnection()
			{
				// Arrange
				const int timeoutInSec = 100;
				const int killBlockersThresholdInPercentage = 10;
				var originalLockTimeout = DataRegistry.Instance.LockTimeout;

				using (new DisposableAction(() => DataRegistry.Instance.LockTimeout = timeoutInSec * 1000, () => DataRegistry.Instance.LockTimeout = originalLockTimeout))
				using (RawDataRegistry.Instance.GhostRecordCleanupOnlineRebuild.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (RawDataRegistry.Instance.GhostRecordCleanupKillBlockersThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, killBlockersThresholdInPercentage))
				using (var blockerStartedSignal = new ManualResetEvent(false))
				using (var grcFinishedSignal = new ManualResetEvent(false))
				{
					CreateTestTableAndIndex(testTableName, testIndexName);
					var grcTask = new GhostRecordCleanerServiceTask() { ServiceLogger = Mock.Of<ILogger>() };
					SqlException blockerConnectionLostException = null;
					var blockerTask = Task.Run(() =>
					{
						using (var blockerConnection = Db.NewExtraConnectionToMainDb())
						{
							try
							{
								blockerConnection.BeginTransaction();
								blockerConnection.ExecuteNonQuery($"UPDATE {testTableName} SET RandomField = RandomField");
								blockerStartedSignal.Set();
								grcFinishedSignal.WaitOne();
								blockerConnection.RollbackTransaction();
							}
							catch (SqlException sqlEx) when(new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.GeneralNetworkError)
							{
								blockerConnectionLostException = sqlEx;
							}
						}
					});
					blockerStartedSignal.WaitOne();

					// Act
					var stopWatch = Stopwatch.StartNew();
					grcTask.RebuildIndividualIndex("dbo", testTableName, testIndexName);
					stopWatch.Stop();

					// Assert
					grcFinishedSignal.Set();
					var startKillingTime = timeoutInSec * killBlockersThresholdInPercentage / 100;

					AssertGreaterThanOrEqualTo(stopWatch.ElapsedMilliseconds, startKillingTime * 1000);
					AssertLessThan(stopWatch.ElapsedMilliseconds, (startKillingTime + 10) * 1000);
					Assert(blockerTask.Wait(2000));
					AssertNotNull(blockerConnectionLostException);
				}
			}

			[UseSnapshotProtection]
			public void TestRebuildIndividualIndexIsTerminatedWhenReachingTimeout()
			{
				// Arrange
				const int timeoutInSec = 100;
				const int grcTimeoutThresholdInPercentage = 50;
				var originalLockTimeout = DataRegistry.Instance.LockTimeout;

				using (new DisposableAction(() => DataRegistry.Instance.LockTimeout = timeoutInSec * 1000, () => DataRegistry.Instance.LockTimeout = originalLockTimeout))
				using (RawDataRegistry.Instance.GhostRecordCleanupKillBlockersThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -1))
				using (RawDataRegistry.Instance.GhostRecordCleanupOnlineRebuild.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (RawDataRegistry.Instance.GhostRecordCleanupCommandTimeOutThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, grcTimeoutThresholdInPercentage))
				using (var blockerStartedSignal = new ManualResetEvent(false))
				using (var grcFinishedSignal = new ManualResetEvent(false))
				{
					CreateTestTableAndIndex(testTableName, testIndexName);
					var grcTask = new GhostRecordCleanerServiceTask() { ServiceLogger = Mock.Of<ILogger>() };
					var blockerTask = Task.Run(() =>
					{
						using (var blockerConnection = Db.NewExtraConnectionToMainDb())
						{
							blockerConnection.BeginTransaction();
							blockerConnection.ExecuteNonQuery($"UPDATE {testTableName} SET RandomField = RandomField");
							blockerStartedSignal.Set();
							grcFinishedSignal.WaitOne();
							blockerConnection.RollbackTransaction();
						}
					});
					blockerStartedSignal.WaitOne();

					// Act
					var stopWatch = Stopwatch.StartNew();
					var ex = AssertExceptionThrown<SqlException>(() => grcTask.RebuildIndividualIndex("dbo", testTableName, testIndexName));
					stopWatch.Stop();

					// Assert
					grcFinishedSignal.Set();
					var grcTimeout = timeoutInSec * grcTimeoutThresholdInPercentage / 100;
					var uncertainty = 16;

					AssertEquals(DbErrorType.TimeoutExpired, new DbErrorMatch(ex).ExceptionType);
					AssertGreaterThanOrEqualTo(stopWatch.ElapsedMilliseconds + uncertainty, grcTimeout * 1000);
					AssertLessThan(stopWatch.ElapsedMilliseconds, (grcTimeout + 10) * 1000);
					Assert(blockerTask.Wait(1000));
				}
			}

			void CreateTestTableAndIndex(string tableName, string indexName)
			{
				var sql = $@"
CREATE TABLE {Db.DatabaseName}.dbo.{testTableName}
(
	Id int					NOT NULL PRIMARY KEY,
	RandomField int         NOT NULL, 
	x  uniqueidentifier     NULL,
)
CREATE INDEX {testIndexName} on {Db.DatabaseName}.dbo.{testTableName}(RandomField);
";
				Db.Connection.ExecuteNonQuery(sql);
			}

			const string testTableName = "RebuildIndividualIndexTestTable";
			const string testIndexName = "RebuildIndividualIndexTestIndex";
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
