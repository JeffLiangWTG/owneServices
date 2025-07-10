using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace ServiceManager.Runner.CW1.Test;

class NativeServiceTaskRunnerWithNextRunTimeCheckTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		base.SetUp();
		serviceTaskRunnerMock = new Mock<IServiceTaskRunner>();
		runnerLoggerMock = new Mock<IRunnerLogger>();
		serviceTaskRunnerWithNextRunTimeCheck = new NativeServiceTaskRunnerWithNextRunTimeCheck(serviceTaskRunnerMock.Object, runnerLoggerMock.Object);
		serviceTaskHandlerMock = new Mock<IDisposableServiceTaskHandler>();
		serviceTaskHandlerInitializerMock = new Mock<IServiceTaskHandlerInitializer>();
		serviceTaskHandlerInitializerMock
			.Setup(x => x.CreateServiceTaskHandler(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.Returns(serviceTaskHandlerMock.Object);
	}

	Mock<IRunnerLogger> runnerLoggerMock;
	Mock<IDisposableServiceTaskHandler> serviceTaskHandlerMock;
	Mock<IServiceTaskRunner> serviceTaskRunnerMock;
	Mock<IServiceTaskHandlerInitializer> serviceTaskHandlerInitializerMock;
	NativeServiceTaskRunnerWithNextRunTimeCheck serviceTaskRunnerWithNextRunTimeCheck;

	public class MiscellaneousTest : NativeServiceTaskRunnerWithNextRunTimeCheckTest
	{
		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NativeServiceTaskRunnerWithNextRunTimeCheck(null, runnerLoggerMock.Object));
				AssertEquals("serviceTaskRunner", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new NativeServiceTaskRunnerWithNextRunTimeCheck(serviceTaskRunnerMock.Object, null));
				AssertEquals("runnerLogger", result.ParamName);
			});
		}
	}

	public class NotScheduledRunTest : NativeServiceTaskRunnerWithNextRunTimeCheckTest
	{
		[ExpectNoExceptions]
		public void TestProvidesRightParameters()
		{
			CombineAssertions(() =>
			{
				Test<IRunCommandInfo>();
				Test<IDirectRunCommandInfo>();
			});

			void Test<T>() where T : class, IRunCommandInfo
			{
				// Arrange
				serviceTaskRunnerMock.Reset();
				var runCommandInfoMock = new Mock<T>();
				var cancellationTokenSource = new CancellationTokenSource();

				// Act
				serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(runCommandInfoMock.Object, cancellationTokenSource);

				// Assert
				serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
				serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(runCommandInfoMock.Object, cancellationTokenSource), Times.Once);
			}
		}
	}

	public class ScheduledRunTest : NativeServiceTaskRunnerWithNextRunTimeCheckTest
	{
		[UseSnapshotProtection(true)]
		public void TestBlockedRow_ThrowsRunnerInternalException()
		{
			// Arrange
			using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			{
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				CreateTask(new DateTimeOffset(nextRunTime, TimeSpan.Zero));
				Factory.Save();

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns("TST");
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				using (blockingConnection.BeginTransactionWithManager())
				{
					blockingConnection.ExecuteNonQuery($@"
UPDATE
	{StmServiceTaskSchema.Constants.TableName}
SET
	{StmServiceTaskSchema.Constants.SST_NextRunTime}={StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc}=GetUtcDate(),
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}='~BP'
WHERE
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode}='TST'
");

					// Act
					// Assert
					AssertExceptionThrown<RunnerInternalException>(() => serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource()));
				}
			}
		}

		[UseSnapshotProtection(true)]
		public void TestBlockedRow_DoesNotRunTask()
		{
			// Arrange
			using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			{
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				CreateTask(new DateTimeOffset(nextRunTime, TimeSpan.Zero));
				Factory.Save();

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns("TST");
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				using (blockingConnection.BeginTransactionWithManager())
				{
					blockingConnection.ExecuteNonQuery($@"
UPDATE
	{StmServiceTaskSchema.Constants.TableName}
SET
	{StmServiceTaskSchema.Constants.SST_NextRunTime}={StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc}=GetUtcDate(),
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}='~BP'
WHERE
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode}='TST'
");

					// Act
					AssertExceptionThrown<Exception>(() => serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource()));

					// Assert
					serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Never);
				}
			}
		}

		[UseSnapshotProtection(true)]
		public void TestBlockedRow_Logs()
		{
			// Arrange
			using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			{
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				CreateTask(new DateTimeOffset(nextRunTime, TimeSpan.Zero));
				Factory.Save();

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns("TST");
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				using (var blockingConnection = Db.NewExtraConnectionToMainDb())
				using (blockingConnection.BeginTransactionWithManager())
				{
					blockingConnection.ExecuteNonQuery($@"
UPDATE
	{StmServiceTaskSchema.Constants.TableName} 
SET
	{StmServiceTaskSchema.Constants.SST_NextRunTime} = {StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc}=GetUtcDate(),
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}='~BP'
WHERE
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode}='TST'
");

					// Act
					AssertExceptionThrown<Exception>(() => serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource()));

					// Assert
						runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsRegex(@"^Could not read next run time. Attempt \d+ out of 10."), scheduledRunCommandInfo.Object), Times.Exactly(10));
					runnerLoggerMock.VerifyNoOtherCalls();
				}
			}
		}

		[UseSnapshotProtection(true)]
		public void TestBlockedRow_LockReleased_DoesNotThrowException()
		{
			// Arrange
			using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			{
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				CreateTask(new DateTimeOffset(nextRunTime, TimeSpan.Zero));
				Factory.Save();

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns("TST");
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				using var timeToRelease = new ManualResetEvent(false);
				runnerLoggerMock
					.Setup(logger => logger.Log(LogLevel.Debug, It.IsAny<string>(), It.IsAny<ICommandInfo>()))
					.Callback(() => timeToRelease.Set());

				using var timeToAct = new ManualResetEvent(false);
				var blockingThread = new Thread(() =>
				{
					using (var blockingConnection = Db.NewExtraConnectionToMainDb())
					using (blockingConnection.BeginTransactionWithManager())
					{
						blockingConnection.ExecuteNonQuery($@"
UPDATE
	{StmServiceTaskSchema.Constants.TableName} 
SET
	{StmServiceTaskSchema.Constants.SST_NextRunTime} = {StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc}=GetUtcDate(),
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}='~BP'
WHERE
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode}='TST'
");
						timeToAct.Set();

						timeToRelease.WaitOne();
					}
				});

				blockingThread.Start();
				timeToAct.WaitOne();

				try
				{
					// Act
					// Assert
						AssertNoExceptionThrown(() => serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource()));
				}
				finally
				{
					timeToRelease.Set();
					blockingThread.Join();
				}
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(true)]
		public void TestBlockedRow_LockReleased_RunsTask()
		{
			// Arrange
			using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(5)))
			{
				using var cancellationTokenSource = new CancellationTokenSource();
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
				Factory.Save();

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns("TST");
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				using var timeToRelease = new ManualResetEvent(false);
				runnerLoggerMock
					.Setup(logger => logger.Log(LogLevel.Debug, It.IsAny<string>(), It.IsAny<ICommandInfo>()))
					.Callback(() => timeToRelease.Set());

				using var timeToAct = new ManualResetEvent(false);
				var blockingThread = new Thread(() =>
				{
					using (var blockingConnection = Db.NewExtraConnectionToMainDb())
					using (blockingConnection.BeginTransactionWithManager())
					{
						blockingConnection.ExecuteNonQuery($@"
UPDATE
	{StmServiceTaskSchema.Constants.TableName}
SET
	{StmServiceTaskSchema.Constants.SST_NextRunTime}={StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc}=GetUtcDate(),
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}='~BP'
WHERE
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode}='TST'
");
						timeToAct.Set();

						timeToRelease.WaitOne();
					}
				});

				blockingThread.Start();
				timeToAct.WaitOne();

				try
				{
					// Act
					serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

					// Assert
					serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
				}
				finally
				{
					timeToRelease.Set();
					blockingThread.Join();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestNextRunDoesMatchExpectations_ProvidesRightParameters()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);

			CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(expectedNextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource), Times.Once);
		}

		public void TestNextRunDoesMatchExpectations_DoesNotUpdateOtherTasks()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
			var expectedNextRunTimeOffset = new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);

			_ = CreateTask(expectedNextRunTimeOffset);
			var otherSchedules = Enumerable.Range(0, 10)
				.Select(i => CreateTask(expectedNextRunTime, $"{i:D3}"))
				.ToList();
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(expectedNextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			var results = otherSchedules
				.Select(otherSchedule => Db.Connection.ExecuteScalar<DateTimeOffset>(
					$"SELECT {StmServiceTaskSchema.Constants.SST_NextRunTime} FROM {StmServiceTaskSchema.Constants.SqlSchemaName}.{StmServiceTaskSchema.Constants.TableName} WHERE {StmServiceTaskSchema.Constants.PK}=@pk",
					command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, otherSchedule.PK.ToGuid())));
			CombineAssertions(() =>
			{
				foreach (var result in results)
				{
					AssertEquals(expectedNextRunTimeOffset, result);
				}
			});
		}

		public void TestNextRunDoesMatchExpectations_TimeIsEqualToExpected_UpdatesNextRunTimeAndRunsTask()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);

			var schedule = CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(expectedNextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			var result = Db.Connection.ExecuteScalar<DateTimeOffset>(
				$"SELECT {StmServiceTaskSchema.Constants.SST_NextRunTime} FROM {StmServiceTaskSchema.Constants.SqlSchemaName}.{StmServiceTaskSchema.Constants.TableName} WHERE {StmServiceTaskSchema.Constants.PK}=@pk",
				command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, schedule.PK.ToGuid()));
			AssertEquals(new DateTimeOffset(nextRunTime, TimeSpan.Zero), result);
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
		}

		public void TestNextRunDoesMatchExpectations_TimeIsEqualToExpected_UpdatesNextRunTimeAndRunsTask_RoundsDownNextRunTime()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0, 999), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0, 999), DateTimeKind.Utc);
			var nextRunTimeRoundDown = DateTime.SpecifyKind(new DateTime(nextRunTime.Year, nextRunTime.Month, nextRunTime.Day, nextRunTime.Hour, nextRunTime.Minute, nextRunTime.Second, 0), DateTimeKind.Utc);

			var schedule = CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(expectedNextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			var result = Db.Connection.ExecuteScalar<DateTimeOffset>(
				$"SELECT {StmServiceTaskSchema.Constants.SST_NextRunTime} FROM {StmServiceTaskSchema.Constants.SqlSchemaName}.{StmServiceTaskSchema.Constants.TableName} WHERE {StmServiceTaskSchema.Constants.PK}=@pk",
				command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, schedule.PK.ToGuid()));
			AssertEquals(new DateTimeOffset(nextRunTimeRoundDown, TimeSpan.Zero), result);
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
		}

		public void TestNextRunDoesMatchExpectations_TimeIsEqualToExpected_ReturnsRunResult()
		{
			CombineAssertions(() =>
			{
				foreach (var serviceTaskRunResult in Enum.GetValues(typeof(ServiceTaskRunResult)).Cast<ServiceTaskRunResult>())
				{
					Test(serviceTaskRunResult);
				}
			});

			void Test(ServiceTaskRunResult serviceTaskRunResult)
			{
				// Arrange
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
				var code = "TST";
				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns(code);
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				serviceTaskRunnerMock
					.Setup(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()))
					.Returns(serviceTaskRunResult);

				Insert(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero), code);

				using var action = new DisposableAction(() =>
				{
					Delete(code);
				});

				// Act
				var result = serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource());

				// Assert
				AssertEquals(serviceTaskRunResult, result);
			}
		}

		public void TestNextRunDoesMatchExpectations_TimeIsOlderThanExpected_UpdatesNextRunTimeAndRunsTask()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);

			var schedule = CreateTask(new DateTimeOffset(expectedNextRunTime.AddMinutes(-5), TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(expectedNextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			var result = Db.Connection.ExecuteScalar<DateTimeOffset>(
				$"SELECT {StmServiceTaskSchema.Constants.SST_NextRunTime} FROM {StmServiceTaskSchema.Constants.SqlSchemaName}.{StmServiceTaskSchema.Constants.TableName} WHERE {StmServiceTaskSchema.Constants.PK}=@pk",
				command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, schedule.PK.ToGuid()));
			AssertEquals(new DateTimeOffset(nextRunTime, TimeSpan.Zero), result);
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Once);
		}

		public void TestNextRunDoesMatchExpectations_TimeIsOlderThanExpected_ReturnsRunResult()
		{
			CombineAssertions(() =>
			{
				foreach (var serviceTaskRunResult in Enum.GetValues(typeof(ServiceTaskRunResult)).Cast<ServiceTaskRunResult>())
				{
					Test(serviceTaskRunResult);
				}
			});

			void Test(ServiceTaskRunResult serviceTaskRunResult)
			{
				// Arrange
				var code = "TST";
				var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);
				var nextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);

				var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
				scheduledRunCommandInfo
					.SetupGet(info => info.Code)
					.Returns(code);
				scheduledRunCommandInfo
					.SetupGet(info => info.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				scheduledRunCommandInfo
					.SetupGet(info => info.NextRunTime)
					.Returns(nextRunTime);

				serviceTaskRunnerMock
					.Setup(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()))
					.Returns(serviceTaskRunResult);

				Insert(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero), code);

				using var action = new DisposableAction(() =>
				{
					Delete(code);
				});

				// Act
				var result = serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, new CancellationTokenSource());

				// Assert
				AssertEquals(serviceTaskRunResult, result);
			}
		}

		public void TestNextRunDoesNotMatchExpectations_TimeIsNewerThanExpected_DoesNotRunTask()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);

			var schedule = CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(nextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			var result = Db.Connection.ExecuteScalar<DateTimeOffset>(
				$"SELECT {StmServiceTaskSchema.Constants.SST_NextRunTime} FROM {StmServiceTaskSchema.Constants.SqlSchemaName}.{StmServiceTaskSchema.Constants.TableName} WHERE {StmServiceTaskSchema.Constants.PK}=@pk",
				command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, schedule.PK.ToGuid()));
			AssertEquals(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero), result);
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Never);
		}

		public void TestNextRunDoesNotMatchExpectations_TimeIsNewerThanExpected_ReturnsResult()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);

			CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(nextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			var result = serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			AssertEquals(ServiceTaskRunResult.Success, result);
		}

		[ExpectNoExceptions]
		public void TestNextRunDoesNotMatchExpectations_TimeIsNewerThanExpected_Logs()
		{
			// Arrange
			var expectedNextRunTime = DateTime.SpecifyKind(new DateTime(2019, 12, 23, 20, 20, 0), DateTimeKind.Utc);
			var nextRunTime = DateTime.SpecifyKind(new DateTime(2006, 12, 26, 13, 0, 0), DateTimeKind.Utc);

			CreateTask(new DateTimeOffset(expectedNextRunTime, TimeSpan.Zero));
			Factory.Save();

			var scheduledRunCommandInfo = new Mock<IScheduledRunCommandInfo>();
			scheduledRunCommandInfo
				.SetupGet(info => info.Code)
				.Returns("TST");
			scheduledRunCommandInfo
				.SetupGet(info => info.ExpectedNextRunTime)
				.Returns(nextRunTime);
			scheduledRunCommandInfo
				.SetupGet(info => info.NextRunTime)
				.Returns(nextRunTime);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunnerWithNextRunTimeCheck.RunServiceTask(scheduledRunCommandInfo.Object, cancellationTokenSource);

			// Assert
			serviceTaskRunnerMock.Verify(runner => runner.RunServiceTask(It.IsAny<IRunCommandInfo>(), It.IsAny<CancellationTokenSource>()), Times.Never);
			runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Information, "Task run is skipped, due to current next run time [2019-12-23 20:20:00] does not match to an expected one", scheduledRunCommandInfo.Object));
		}

		StmServiceTask CreateTask(DateTimeOffset nextRunTime, string taskCode = "TST")
		{
			var task = Factory.New<StmServiceTask>();
			task.SST_ServiceTaskCode = taskCode;
			var localNextRunTime = new DateTimeOffset(nextRunTime.Year, nextRunTime.Month, nextRunTime.Day, nextRunTime.Hour, nextRunTime.Minute, nextRunTime.Second, nextRunTime.Offset);
			task.SST_NextRunTime = localNextRunTime;
			return task;
		}

		void Insert(DateTimeOffset nextRunTime, string taskCode)
		{
			var sqlText = $@"
INSERT INTO dbo.{StmServiceTaskSchema.Constants.TableName}
(
	{StmServiceTaskSchema.Constants.PK},
	{StmServiceTaskSchema.Constants.SST_ServiceTaskCode},
	{StmServiceTaskSchema.Constants.SST_NextRunTime},
	{StmServiceTaskSchema.Constants.SST_Configuration},
	{StmServiceTaskSchema.Constants.SST_SystemCreateTimeUtc},
	{StmServiceTaskSchema.Constants.SST_SystemCreateUser},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditTimeUtc},
	{StmServiceTaskSchema.Constants.SST_SystemLastEditUser}
)
VALUES
(
	NEWID(),
	@taskCode,
	@nextRunTime,
	'',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)
";
			Db.Connection.ExecuteNonQuery(sqlText, cmd =>
			{
				cmd.AddParameter("@taskCode", SqlDbType.VarChar, taskCode);
				cmd.AddParameter("@nextRunTime", SqlDbType.DateTimeOffset, nextRunTime);
			});
		}

		void Delete(string taskCode)
		{
			var sqlText = $@"
DELETE FROM dbo.{StmServiceTaskSchema.Constants.TableName}
WHERE {StmServiceTaskSchema.Constants.SST_ServiceTaskCode} = @taskCode
";
			Db.Connection.ExecuteNonQuery(sqlText, cmd => cmd.AddParameter("@taskCode", SqlDbType.VarChar, taskCode));
		}
	}
}
