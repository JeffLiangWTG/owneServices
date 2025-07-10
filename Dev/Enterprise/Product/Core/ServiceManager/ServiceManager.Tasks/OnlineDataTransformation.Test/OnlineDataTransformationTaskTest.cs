using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Logging;
using CargoWise.Types;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using WTG.ApplicationLogging.Abstractions;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	[TestedType(typeof(OnlineDataTransformationTask))]
	sealed class OnlineDataTransformationTaskTest : ServiceTaskTestCase<OnlineDataTransformationTask>
	{
		public void TestODTServiceTaskExceptionsAreReportedToIssueManager()
		{
			// Arrange
			const string odtServiceTaskCode = "ODT";
			const string mockedTransformationUserDescription = nameof(TestODTServiceTaskExceptionsAreReportedToIssueManager);
			const string errorMessage = "Cannot insert duplicate key row in object 'dbo.StmModuleFilter' with unique index 'NR_UX_S9_ModuleID_S9_FilterName_S9_GC";

			var cannotInsertDuplicateUniqueIndexKeySqlException = SqlExceptionBuilder.CreateSqlException(2601, errorMessage);
			var serviceTaskTypeName = "Enterprise.ServiceManager.Tasks.OnlineDataTransformation.OnlineDataTransformationTask";
			var serviceTaskAssemblyName = Assembly.GetExecutingAssembly().FullName;

			var serviceLoggerMock = new Mock<ILogger>();
			var onlineTransformationTaskMock = new Mock<IOnlineTransformation>();
			var onlineTransformationProviderMock = new Mock<IOnlineTransformationProvider>();
			var onlineTransformationTasks = new IOnlineTransformation[] { onlineTransformationTaskMock.Object };
			var odtServiceTask = new OnlineDataTransformationTask(onlineTransformationProviderMock.Object, TimeSpan.FromSeconds(2));
			_ = onlineTransformationProviderMock.Setup(x => x.GetRunningTasks()).Returns(onlineTransformationTasks);
			_ = onlineTransformationTaskMock.Setup(x => x
				.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()))
				.Throws(cannotInsertDuplicateUniqueIndexKeySqlException);
			_ = onlineTransformationTaskMock.SetupGet(x => x
				.UserDescription)
				.Returns(mockedTransformationUserDescription);
			odtServiceTask.ServiceLogger = serviceLoggerMock.Object;

			var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
			_ = hostedServiceConfigMock.Setup(x => x.Code).Returns(odtServiceTaskCode);
			_ = hostedServiceConfigMock.Setup(x => x.TypeName).Returns(serviceTaskTypeName);
			_ = hostedServiceConfigMock.Setup(x => x.TypeAssemblyName).Returns(serviceTaskAssemblyName);

			var serviceTaskMock = new Mock<IDisposableServiceTaskHandler>();
			_ = serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			_ = serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback((CancellationToken cancellationToken) =>
				{
					odtServiceTask.RunTask(cancellationToken);
				});
			_ = serviceTaskMock.Setup(task => task.HandleException(It.IsAny<Exception>(), It.IsAny<string>()))
				.Callback((Exception exception, string message) =>
				{
					ErrorReporter.ReportOnce(ExceptionConstants.RunnerExceptionLocation, exception);
				});

			var loggerFactoryMock = new Mock<ICategorizedApplicationLoggerFactory>();

			loggerFactoryMock
				.Setup(o => o.CreateCategorizedLogger(It.IsAny<LoggerCategory>(), It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>()))
				.Returns((LoggerCategory category, string name, IEnumerable<KeyValuePair<string, object>> properties) =>
				{
					var activitySource = new ActivitySource(name);

					return Mock.Of<IApplicationLogger>(o => o.ActivitySource == activitySource);
				});

			using var listener = new ActivityListener();
			listener.Sample = (ref ActivityCreationOptions<ActivityContext> o) => ActivitySamplingResult.AllData;
			listener.ShouldListenTo = o => true;
			ActivitySource.AddActivityListener(listener);

			var serviceTaskHandlerInitializerMock = new Mock<IServiceTaskHandlerInitializer>();
			serviceTaskHandlerInitializerMock
				.Setup(s => s.CreateServiceTaskHandler(serviceTaskMock.Object.HostedServiceAttribute.TypeAssemblyName, serviceTaskMock.Object.HostedServiceAttribute.Code, string.Empty))
				.Returns(serviceTaskMock.Object);

			var serviceTaskRunner = new ServiceTaskRunner(
				Mock.Of<IServiceTaskLogger>(),
				Mock.Of<IRunnerLogger>(),
				Mock.Of<IProcessEnvironmentRecorder>(),
				Mock.Of<IEnvironmentCheckerStrategy>(),
				new ErrorReporterProxy(),
				serviceTaskHandlerInitializerMock.Object,
				loggerFactoryMock.Object);

			try
			{
				var errorReporterMock = new Mock<IErrorReporter>();
				var serviceTaskRunResult = ServiceTaskRunResult.Success;
				var numOfReportsBeforeTest = CountStmErrorReport();

				// Act
				using (Globals.SetIsUnitTestingProductionFunctionality()) // to skip those ugly #if DEBUG sections
				using (Globals.SetIsUserInteractiveForTest(false)) // background process
				{
					ExceptionReporter.Instance.TestingDoReportException.Value = true; // please do the report from test

					serviceTaskRunResult = serviceTaskRunner.RunServiceTask(
						new DirectRunCommandInfo(serviceTaskAssemblyName, odtServiceTaskCode, Guid.Empty),
						new CancellationTokenSource());
				}

				// Assert
				CombineAssertions(() =>
				{
					onlineTransformationTaskMock.Verify(x => x
						.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Once);
					onlineTransformationTaskMock.VerifyAll();

					AssertEquals(ServiceTaskRunResult.UnhandledException, serviceTaskRunResult);
					AssertGreaterThan("ODT SqlExceptions should have been reported", CountStmErrorReport(), numOfReportsBeforeTest);

					var reportXml = Db.Connection.ExecuteScalar<string>("SELECT TOP 1 QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
					AssertContains(errorMessage, reportXml);
					AssertContains(typeof(OnlineDataTransformationException).FullName, reportXml);
					AssertContains(mockedTransformationUserDescription, reportXml);
				});
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
				ExceptionReporter.Instance.TestingDoReportException.Value = false;
			}

			static int CountStmErrorReport()
			{
				return Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM [StmErrorReport]");
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestTasksAreInterleavedUntilCompletion()
		{
			var mockProvider = new Mock<IOnlineTransformationProvider>();
			var tasks = new IOnlineTransformation[] { new Transformation01(), new DecrementingFromFiveSecondsTransformation(), new Transformation03() };
			mockProvider.Setup(x => x.GetRunningTasks()).Returns(tasks);
			var task = new OnlineDataTransformationTask(mockProvider.Object, TimeSpan.FromSeconds(2));
			var logger = new DummyLogger();
			var logs = new List<string>();
			logger.OnLog += (sender, eventArgs) => { logs.Add(eventArgs.Message); };
			task.ServiceLogger = logger;
			task.RunTask(CancellationToken.None); // service task injects token here
			AssertContainsExactElementsInExactOrder("Logs should be like, but are not", new[]
			{
"[01] - Start",
"[01] - Finish",
"[Decrementing transform] - Start",
"[Decrementing transform] - Yielded to allow other tasks to complete",
"[Description] - Start",
"[Description] - Finish",
"[Decrementing transform] - Start",
"[Decrementing transform] - Yielded to allow other tasks to complete",
}, logs.Take(8));
			AssertEquals("Last log should be", "[Decrementing transform] - Finish", logs.Last());
		}

		public void TestServiceTaskTerminationInterruptsPromptly()
		{
			var mockProvider = new Mock<IOnlineTransformationProvider>();
			var tasks = new IOnlineTransformation[] { new DecrementingFromFiveSecondsTransformation() };
			mockProvider.Setup(x => x.GetRunningTasks()).Returns(tasks);
			var task = new OnlineDataTransformationTask(mockProvider.Object, TimeSpan.FromSeconds(2));
			var logger = new DummyLogger();
			var logs = new List<string>();
			logger.OnLog += (sender, eventArgs) => { logs.Add(eventArgs.Message); };
			task.ServiceLogger = logger;
			using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6)))
			{
				task.RunTask(cts.Token); // service task injects token here
				AssertEquals("Last log", "[Decrementing transform] - Yielded as service host requested STOP", logs.Last());
			}
		}

		public void TestAlwaysRunAtStartupIsTrue()
		{
			var serviceAttribute = AssemblyMetaDataReader.GetAttributes<HostedServiceAttribute>().Single(a => a.Code == OnlineDataTransformationTask.ServiceTaskCode);
			AssertEquals(true, serviceAttribute.AlwaysRunAtStartup);
		}

		public void TestWhenNoCurrentSavedState_AllTransformationsThatShouldRunAreExecuted()
		{
			var repository = new MockRepository(MockBehavior.Loose);

			var newTask1 = repository.Create<Transformation01>();
			newTask1.SetupGet(_ => _.UserDescription).Returns("Online Transformation 1");
			var newTask2 = repository.Create<Transformation02>();
			newTask2.SetupGet(_ => _.UserDescription).Returns("Online Transformation 2");
			var newTask3 = repository.Create<Transformation03>();
			newTask3.SetupGet(_ => _.UserDescription).Returns("Online Transformation 3");

			var allTasks = new List<IOnlineTransformation>() { newTask1.Object, newTask2.Object, newTask3.Object };
			allTasks.ForEach(task => Mock.Get(task).Setup(t => t.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())));

			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(p => p.GetRunningTasks()).Returns(allTasks);

			var transformation = CreateOnlineDataTransformationTaskMock(provider.Object);
			transformation.RunTask();

			allTasks.ForEach(task => Mock.Get(task).VerifyAll());
			Assert(true);
		}

		public void TestMultipleRuns_MustExecuteTasksOnce()
		{
			var repository = new MockRepository(MockBehavior.Loose);   //Init the repository first

			var newTask = repository.Create<Transformation01>();
			newTask.SetupGet(_ => _.UserDescription).Returns("Online Transformation 1");

			var completedTask = repository.Create<Transformation02>();
			completedTask.SetupGet(_ => _.UserDescription).Returns("Online Transformation 2");

			var pendingTask = repository.Create<Transformation03>();
			pendingTask.SetupGet(_ => _.UserDescription).Returns("Online Transformation 3");

			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(p => p.GetRunningTasks()).Returns(new IOnlineTransformation[] { newTask.Object, pendingTask.Object });

			var transformation = CreateOnlineDataTransformationTaskMock(provider.Object);
			transformation.RunTask();

			newTask.Verify(t => t.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
			completedTask.Verify(m => m.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Never());
			pendingTask.Verify(t => t.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
			provider.Verify(p => p.OnTaskCompleted(It.Is<IOnlineTransformation>(t => t == newTask.Object)), Times.Exactly(1));
			provider.Verify(p => p.OnTaskCompleted(It.Is<IOnlineTransformation>(t => t == pendingTask.Object)), Times.Exactly(1));

			provider.VerifyAll();

			provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(p => p.GetRunningTasks()).Returns(Array.Empty<IOnlineTransformation>());
			provider.Verify(p => p.OnTaskCompleted(It.IsAny<IOnlineTransformation>()), Times.Never());
			transformation = CreateOnlineDataTransformationTaskMock(provider.Object);
			transformation.RunTask();

			Assert(true);
		}

		public void TestAfterATaskIsRunSuccessfully_StateWillBeUpdated()
		{
			var newTask = new Transformation01();
			var completedTask = new Transformation02();
			var pendingTask = new Transformation03();
			var suspendedTask = new TransformationSuspended();

			var allTasks = new List<IOnlineTransformation>() { newTask, completedTask, pendingTask };

			var repository = new MockRepository(MockBehavior.Loose);   //Init the repository first

			var provider = repository.Create<IOnlineTransformationProvider>();

			provider.Setup(p => p.GetRunningTasks()).Returns(allTasks);

			Expression<Predicate<OnlineTransformationTaskStatus>> matchFirstSave = (status) =>
				status.Pending.Count() == 1
				&& status.Pending.Contains(pendingTask.GetType().FullName)
				&& status.Pending.Contains(pendingTask.GetType().FullName)
				&& status.Completed.Count() == 2
				&& status.Completed.Contains(completedTask.GetType().FullName)
				&& status.Completed.Contains(newTask.GetType().FullName)
				&& status.Pending.Contains(suspendedTask.GetType().FullName);

			Expression<Predicate<OnlineTransformationTaskStatus>> matchSecondSave = (status) =>
				status.Pending.Count() == 0
				&& status.Pending.Contains(suspendedTask.GetType().FullName)
				&& status.Completed.Count() == 3
				&& status.Completed.Contains(completedTask.GetType().FullName)
				&& status.Completed.Contains(newTask.GetType().FullName)
				&& status.Completed.Contains(pendingTask.GetType().FullName);

			Expression<Predicate<OnlineTransformationTaskStatus>> matchThirdSave = (status) =>
				status.Pending.Count() == 0
				&& status.Completed.Contains(suspendedTask.GetType().FullName)
				&& status.Completed.Count() == 3
				&& status.Completed.Contains(completedTask.GetType().FullName)
				&& status.Completed.Contains(newTask.GetType().FullName)
				&& status.Completed.Contains(pendingTask.GetType().FullName);

			var transformation = CreateOnlineDataTransformationTaskMock(provider.Object);
			transformation.RunTask();

			provider.Verify(p => p.GetRunningTasks(), Times.Exactly(1));
			provider.VerifyAll();
			Assert(true);
		}

		public void TestWhenCompletedTaskIsDeleted_ItShouldNotBeSavedToState()
		{
			var completedButDeletedTask = new Transformation01();
			var completedTask = new Transformation02();
			var pendingTask = new Transformation03();

			var allTasks = new List<IOnlineTransformation>() { completedTask, pendingTask };

			var repository = new MockRepository(MockBehavior.Loose);   //Init the repository first
			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(p => p.GetRunningTasks()).Returns(allTasks);

			Expression<Predicate<OnlineTransformationTaskStatus>> matchStatus = (status) =>
				 status.Pending.Count() == 1
				 && status.Pending.Contains(pendingTask.GetType().FullName)
				 && status.Completed.Count() == 1
				 && status.Completed.Contains(completedTask.GetType().FullName);

			Expression<Predicate<OnlineTransformationTaskStatus>> nonMatchStatus = (status) =>
				status.Pending.Contains(completedButDeletedTask.GetType().FullName)
				|| status.Completed.Contains(completedButDeletedTask.GetType().FullName);

			var transformation = CreateOnlineDataTransformationTaskMock(provider.Object);
			transformation.RunTask();

			provider.VerifyAll();
			Assert(true);
		}

		[TestDate(2024, 7, 1, 12, 30, 0)]
		public void TestSetTaskNextRunTime_NoRunningTransformations()
		{
			var repository = new MockRepository(MockBehavior.Strict);   //Init the repository first
			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(p => p.GetRunningTasks()).Returns(Enumerable.Empty<IOnlineTransformation>());
			var mockGovernor = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(mockGovernor.Object))
			{
				var task = new OnlineDataTransformationTask(provider.Object);
				InitialiseTaskSchedule(task, out StmServiceTask taskSchedule);

				var firstRunTime = ZDateTimeOffset.UtcNow;

				AssertEquals("[INITIAL] IsActive", true, taskSchedule.SST_Active);
				AssertEquals("[INITIAL] NextRunTime", firstRunTime, taskSchedule.SST_NextRunTime);

				// Runs service task. Since there are no running transformations => next schedule run time should be 1 day from now
				task.RunTask();
				taskSchedule.Reload();

				AssertEquals("[AFTER RUN WITH NO TRANSFORMATIONS] IsActive", true, taskSchedule.SST_Active);
				mockGovernor.Verify(g => g.SetServiceTaskNextRuntime(OnlineDataTransformationTask.ServiceTaskCode, ZDateTime.UtcNow.AddDays(1).ToNullableDateTimeOffset()), Times.Once);
				provider.VerifyAll();
			}
		}

		[TestDate(2024, 7, 1, 12, 30, 0)]
		public void TestSetTaskNextRunTime()
		{
			var mockProvider = new Mock<IOnlineTransformationProvider>();
			var tasks = new IOnlineTransformation[] { new Transformation01() };
			mockProvider.Setup(x => x.GetRunningTasks()).Returns(tasks);
			var mockGovernor = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(mockGovernor.Object))
			{
				var task = new OnlineDataTransformationTask(mockProvider.Object);
				InitialiseTaskSchedule(task, out StmServiceTask taskSchedule);

				var firstRunTime = ZDateTimeOffset.UtcNow;

				AssertEquals("[INITIAL] IsActive", true, taskSchedule.SST_Active);
				AssertEquals("[INITIAL] NextRunTime", firstRunTime, taskSchedule.SST_NextRunTime);

				// Runs service task. Since there are is a transformation running => next scheduled run time should be in 1 minute
				task.RunTask();
				taskSchedule.Reload();

				AssertEquals("[AFTER RUN WITH TRANSFORMATIONS] IsActive", true, taskSchedule.SST_Active);
				mockGovernor.Verify(g => g.SetServiceTaskNextRuntime(OnlineDataTransformationTask.ServiceTaskCode, ZDateTime.UtcNow.AddMinutes(1).ToNullableDateTimeOffset()), Times.Once);
				mockProvider.VerifyAll();
			}
		}

		[TestDate(2024, 7, 1, 12, 30, 0)]
		public void TestTransformationsExceptionsAreNotSwallowed()
		{
			var strictRepository = new MockRepository(MockBehavior.Strict);   //Init the repository first
			var transformation1 = strictRepository.Create<Transformation01>();
			transformation1.SetupGet(_ => _.UserDescription).Returns("Online Transformation 1");
			transformation1.Setup(_ => _.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()));

			var transformation2 = strictRepository.Create<Transformation02>();
			transformation2.SetupGet(_ => _.UserDescription).Returns("Online Transformation 2");
			transformation2.Setup(_ => _.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()));

			const string errorTransformationTaskDescription = "Online Transformation 3";
			var exception3 = new Exception("Exception in Online Transformation 3");
			var transformation3 = strictRepository.Create<Transformation03>();
			transformation3.SetupGet(_ => _.UserDescription).Returns(errorTransformationTaskDescription);
			transformation3.Setup(_ => _.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).Throws(exception3);

			var provider = strictRepository.Create<IOnlineTransformationProvider>();
			provider.Setup(_ => _.GetRunningTasks()).Returns(new IOnlineTransformation[] { transformation1.Object, transformation2.Object, transformation3.Object });
			provider.Setup(_ => _.OnTaskCompleted(It.IsAny<IOnlineTransformation>()));

			var task = new OnlineDataTransformationTask(provider.Object);
			var exception = AssertExceptionThrown<AggregateException>(() => InitialiseAndRunTaskSchedule(task));

			AssertEquals(1, exception.InnerExceptions.Count);
			AssertType<OnlineDataTransformationException>(exception.InnerExceptions[0]);
			AssertNotNull(exception.InnerExceptions[0].InnerException);
			AssertEquals(exception3, exception.InnerExceptions[0].InnerException);
			AssertStartsWith("Service Task Log", @"
Information|[Online Transformation 1] - Start
Information|[Online Transformation 1] - Finish
Information|[Online Transformation 2] - Start
Information|[Online Transformation 2] - Finish
Information|[Online Transformation 3] - Start
Information|[Online Transformation 3] - Exception in Online Transformation 3".TrimStart(),
				task.ServiceLogger.ToString());

			transformation1.VerifyAll();
			transformation2.VerifyAll();
			transformation3.VerifyAll();
		}

		public void TestDeadlockPriority()
		{
			var repository = new MockRepository(MockBehavior.Strict);   //Init the repository first
			var transform = repository.Create<Transformation01>();

			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(_ => _.GetRunningTasks()).Returns(new IOnlineTransformation[] { transform.Object });

			var deadlockPriority = -100;
			var task = new OnlineDataTransformationTask(provider.Object);
			task.UserAction_ForTest = () =>
			{
				deadlockPriority = Db.Connection.DeadlockPriority;

				throw new OperationCanceledException("Cancelled by user");
			};

			AssertExceptionThrown<OperationCanceledException>(() => task.RunTask(CancellationToken.None));
			AssertEquals("Deadlock priority", (int)DeadlockPriority.Min, deadlockPriority);
		}

		[TestDate(2024, 7, 1, 12, 30, 0)]
		public void TestRequeuedDueToDeadlock()
		{
			var deadlockException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1205, 51, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0)));

			var repository = new MockRepository(MockBehavior.Loose);   //Init the repository first
			var transform1 = new Mock<Transformation01>(MockBehavior.Strict);
			transform1.SetupGet(_ => _.UserDescription).Returns("Online Transformation 1");
			transform1.SetupSequence(m => m.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).Throws(deadlockException);

			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(_ => _.GetRunningTasks()).Returns(new IOnlineTransformation[] { transform1.Object, });

			var task = new OnlineDataTransformationTask(provider.Object);
			using (task.TemporarySetTryCountDueToException_ForTest(2))
			{
				InitialiseAndRunTaskSchedule(task);
			}

			transform1.Verify(_ => _.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
			provider.Verify(_ => _.OnTaskCompleted(It.IsAny<IOnlineTransformation>()), Times.Exactly(1));

			AssertStartsWith("Service Task Log", @"
Information|[Online Transformation 1] - Start
Information|[Online Transformation 1] - Requeued due to deadlock
Information|[Online Transformation 1] - Start
Information|[Online Transformation 1] - Finish".TrimStart(),
				task.ServiceLogger.ToString());

			transform1.VerifyAll();
		}

		[TestDate(2024, 7, 1, 12, 30, 0)]
		public void TestRequeuedDueToLockTimeout()
		{
			var lockTimeoutException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(1222, 51, 13, Db.ServerName, "Lock request time out period exceeded.", string.Empty, 0)));

			var repository = new MockRepository(MockBehavior.Loose);   //Init the repository first
			var transform1 = repository.Create<Transformation01>();
			transform1.SetupGet(_ => _.UserDescription).Returns("Online Transformation 1");
			transform1.SetupSequence(m => m.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>())).Throws(lockTimeoutException);

			var provider = repository.Create<IOnlineTransformationProvider>();
			provider.Setup(_ => _.GetRunningTasks()).Returns(new IOnlineTransformation[] { transform1.Object, });

			var task = new OnlineDataTransformationTask(provider.Object);
			using (task.TemporarySetTryCountDueToException_ForTest(2))
			{
				InitialiseAndRunTaskSchedule(task);
			}
			transform1.Verify(_ => _.Run(It.IsAny<Action<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

			AssertStartsWith("Service Task Log", @"
Information|[Online Transformation 1] - Start
Information|[Online Transformation 1] - Requeued due to lock timeout
Information|[Online Transformation 1] - Start
Information|[Online Transformation 1] - Finish".TrimStart(),
				task.ServiceLogger.ToString());

			transform1.VerifyAll();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		static OnlineDataTransformationTask CreateOnlineDataTransformationTaskMock(IOnlineTransformationProvider provider)
		{
			var transformation = new OnlineDataTransformationTask(provider);
			transformation.ServiceLogger = new Mock<ILogger>().Object;
			return transformation;
		}

		#endregion
	}
}
