using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Testing.Core.RunnableTask;
using Enterprise.ServiceManager.Host.Testing.Helpers;
using Enterprise.ServiceManager.Host.Testing.Helpers.RunnableTask;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Shared.Abstractions;
using WTG.NUnit;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host.Testing
{
	class RunnableServiceTaskTest : TestCaseWithFactory
	{
		public static ServiceTaskInfo DummyTaskInfo
		{
			get
			{
				var config = new HostedServiceAttribute { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll", DefaultScheduleRunEvery = "1day" };
				return new ServiceTaskInfo(config);
			}
		}

		public void TestConstructorWithNullArgumentsThrowsException()
		{
			const string testCode = "TST";
			var taskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));

			var exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(null, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("info"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), null, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("actionQueue"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), null, Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("taskQueue"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), null, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("hostLogger"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), null, Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("errorReporterProxy"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), null, Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("hostRegistry"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), null, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("transactionAdapter"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), null, Mock.Of<IServiceTaskScheduleStatusProvider>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("loggerFactory"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new RunnableServiceTask(taskInfo, Mock.Of<IServiceTask>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), null));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("serviceTaskScheduleStatusProvider"));
		}

		[ExpectNoExceptions]
		public void TestIsDisabledOnEmptyMandatorilyDisabledTasksRegistry()
		{
			const string testCode = "TST";
			NUnit.Framework.Assert.Multiple(() =>
			{
				TestIsDisabledOnEmptyMandatorilyDisabledTasksRegistry(testCode, true);
				TestIsDisabledOnEmptyMandatorilyDisabledTasksRegistry("", false);
				TestIsDisabledOnEmptyMandatorilyDisabledTasksRegistry($"{testCode}|ABC", true);
			});

			void TestIsDisabledOnEmptyMandatorilyDisabledTasksRegistry(string value, bool expectedResult)
			{
				// Arrange
				var taskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));
				var schedule = Factory.New<NullBranchServiceTaskSchedule>();
				schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

				var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

				var task = new RunnableServiceTask(taskInfo, governor.GovernedTask, Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == value), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				// Act
				// Assert
				NUnit.Framework.Assert.That(task.IsDisabled, Is.EqualTo(expectedResult), $"Expected task disable state: {expectedResult} for value {value}");
			}
		}

		[ExpectNoExceptions]
		public void TestValidateForRunReturnTaskIsDisabledOnMandatorilyDisabledTask()
		{
			// Arrange
			const string testCode = "TST";
			var taskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));
			var schedule = Factory.New<NullBranchServiceTaskSchedule>();
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var logger = new Mock<IHostLogger>();

			var task = new RunnableServiceTask(taskInfo, governor.GovernedTask, Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<ITaskQueue>(), logger.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == testCode), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			// Act
			var result = task.ValidateForRun();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(TaskRunRequestResult.TaskIsDisabled));
			logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<IHostedServiceAttribute>(), "Skipped running task - is disabled"), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestNullBranchInConstructor()
		{
			var schedule = Factory.New<NullBranchServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			NUnit.Framework.Assert.That(schedule.Branch, Is.EqualTo(default(GlbBranch)), "Test integrity check - branch must be null for this test to be valid.");
			try
			{
				IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			}
			catch (NullReferenceException)
			{
				Fail("Failed to handle valid case where schedule.Branch is null.");
			}
		}

		public void TestBranchWhenAccessedByAnotherThreadShouldNotThrowThreadSentryError()
		{
			// Arrange
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_GB = branch.PK;
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Action
			var task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			Thread readerThread = new Thread(() =>
			{
				// Assert
				AssertNoExceptionThrown(() =>
				{
					_ = task.Branch;
				});
			});

			readerThread.Start();
			readerThread.Join();
		}

		[ExpectNoExceptions]
		public void TestBranchWhenScheduleBranchIsNull()
		{
			// Arrange
			var schedule = Factory.New<NullBranchServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });
			NUnit.Framework.Assert.That(schedule.Branch, Is.EqualTo(default(GlbBranch)));

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			// Assert
			NUnit.Framework.Assert.That(task.Branch, Is.EqualTo(string.Empty));
		}

		[ExpectNoExceptions]
		public void TestBranchWhenScheduleBranchIsNotNull()
		{
			// Arrange
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var schedule = Factory.New<NullBranchServiceTaskSchedule>();
			schedule.S5_GB = branch.PK;
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			// Assert
			NUnit.Framework.Assert.That(task.Branch, Is.EqualTo(branch.GB_Code.ToString()));
		}

		[ExpectNoExceptions]
		public void TestMaxSecondaryProcessesReturnsZeroIfNullSchedule()
		{
			//Arrange
			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			//Act
			var maxProcesses = task.MaxSecondaryRunningCount;

			//Assert
			NUnit.Framework.Assert.That(maxProcesses, Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestIsActiveCalculatedIsSetInConstructor()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.S5_IsActive = false;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(task.IsActive, Is.EqualTo(false));

			var schedule2 = Factory.New<ServiceTaskSchedule>();
			schedule2.S5_ScheduleType = "C01";
			schedule2.S5_ScheduleDescription = "Desc 1";
			schedule2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule2.S5_IsActive = true;

			var governor2 = new SchedulerServiceTaskGovernor(Factory, schedule2);

			IRunnableServiceTask task2 = new RunnableServiceTask(DummyTaskInfo, governor2.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(task2.IsActive, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestCachedPropertiesFollowUpdates()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			var originalNextRunTime = ZDateTime.UtcNow;
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = originalNextRunTime;
			schedule.S5_TypeOfDocument = "T1";
			schedule.S5_IsActive = false;
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_TaskPeriod = "S";
			schedule.S5_OverdueDurationInSeconds = 10;
			var settings = new HostedServiceSerializableSettings { ConfigString = "Original" };
			schedule.S5_ScheduleState = Encoding.ASCII.GetBytes(settings.AsXml());
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "1second" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(task.IsActive, Is.EqualTo(false));
			NUnit.Framework.Assert.That(task.ScheduleDescription, Is.EqualTo("Desc 1"));
			NUnit.Framework.Assert.That(task.TypeOfDocument, Is.EqualTo("T1"));
			NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(originalNextRunTime.ToNullableDateTimeOffset()));
			NUnit.Framework.Assert.That(task.SchedulePeriodDuration, Is.EqualTo(TimeSpan.FromSeconds(1)));
			NUnit.Framework.Assert.That(task.OverdueDuration.Seconds, Is.EqualTo(10));
			NUnit.Framework.Assert.That(task.ConfigString, Is.EqualTo("Original"));

			governor.SetActive(true);
			governor.SetOverdueDuration(TimeSpan.FromSeconds(20));
			governor.SetSchedule(5, "S");
			NUnit.Framework.Assert.That(task.IsActive, Is.EqualTo(true));
			NUnit.Framework.Assert.That(task.SchedulePeriodDuration, Is.EqualTo(TimeSpan.FromSeconds(5)));
			NUnit.Framework.Assert.That(task.OverdueDuration.Seconds, Is.EqualTo(20));
		}

		[ExpectNoExceptions]
		public void TestIsOverdue()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			schedule.S5_IsActive = true;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(task.IsActive, Is.EqualTo(true));
			NUnit.Framework.Assert.That(task.IsOverdue, Is.EqualTo(false));

			task.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(-2), SetNextRuntimeReason.ScheduledToRun);
			NUnit.Framework.Assert.That(task.IsOverdue, Is.EqualTo(false));

			governor.SetOverdueDuration(TimeSpan.FromSeconds(60));
			NUnit.Framework.Assert.That(task.IsOverdue, Is.EqualTo(true));

			schedule.S5_OverdueDurationInSeconds = 150;
			governor.SetOverdueDuration(TimeSpan.FromSeconds(150));
			NUnit.Framework.Assert.That(task.IsOverdue, Is.EqualTo(false));
		}

		[TestDate(2016, 05, 26, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestNextRunTimeIsInFuture()
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-2);
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, governor.GovernedTask, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(task.NextRunTimeIsInFuture, Is.EqualTo(false));

			task.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset(), SetNextRuntimeReason.ScheduledToRun);
			NUnit.Framework.Assert.That(task.NextRunTimeIsInFuture, Is.EqualTo(false));

			task.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(2), SetNextRuntimeReason.ScheduledToRun);
			NUnit.Framework.Assert.That(task.NextRunTimeIsInFuture, Is.EqualTo(true));

			var laterTime = ZDateTime.UtcNow.AddMinutes(3).ToDateTime();
			TestDateAttribute.Date = laterTime;
			NUnit.Framework.Assert.That(task.NextRunTimeIsInFuture, Is.EqualTo(false));

			IRunnableServiceTask taskNoSchedule = new RunnableServiceTask(DummyTaskInfo, null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			NUnit.Framework.Assert.That(taskNoSchedule.NextRunTimeIsInFuture, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestUpdateStatus()
		{
			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			IRunnableServiceTask taskNoSchedule = new RunnableServiceTask(DummyTaskInfo, null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			taskNoSchedule.UpdateStatus(); //Check does not throw if no schedule.

			var schedule = Factory.New<NullUpdateStatusServiceTaskSchedule>();
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });

			IRunnableServiceTask task = new RunnableServiceTask(DummyTaskInfo, new SchedulerServiceTask(schedule), new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			task.UpdateStatus();
			NUnit.Framework.Assert.That(schedule.UpdateStatusCalled, Is.True);
		}

		[ExpectNoExceptions]
		public void TestSetNextRunTimeBasedOnRecurrence_SetsNextRuntime()
		{
			// Arrange
			var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			using var immediateActionQueue = new ImmediateActionQueue();
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1), false, immediateActionQueue, transactionAdapter: Mock.Of<ITransactionAdapter>());
			var reference = DateTime.UtcNow;
			schedule1.S5_NextScheduledPrintRunTimeUtc = reference;

			//Act
			task1.SetNextRunTimeBasedOnRecurrence();

			//Assert
			NUnit.Framework.Assert.That(task1.NextScheduledRunTime, Is.Not.EqualTo(reference).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestSetNextRunTimeBasedOnRecurrence_BumpsNextRunTimeBy1Minute_IfScheduleWasInvalid()
		{
			// Arrange
			var schedule = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), false, immediateActionQueue, errorReporterProxy: errorReporterProxyMock.Object, transactionAdapter: Mock.Of<ITransactionAdapter>());
			schedule.Recurrence.WeeklyRange = true;
			schedule.Recurrence.DayList = "NNNNNNN";
			var reference = ZDateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = reference;

			// Act
			task.SetNextRunTimeBasedOnRecurrence();

			// Assert
			errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.Is<string>(msg => msg.StartsWith("SetNextRunTimeBasedOnRecurrence: ")), It.IsAny<Exception>()), Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(reference.AddMinutes(1).ToNullableDateTimeOffset()));

			schedule.Recurrence.DayList = "YNNNNNN"; // Avoid exception when calling Factory.Save() in Dispose method of taskSchedule
		}

		[ExpectNoExceptions]
		public void TestUpdateScheduleWillReInitialiseScheduleThreadSafeReaderCache()
		{
			// Arrange
			var schedule = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			schedule.S5_IsActive = true;
			schedule.S5_ScheduleDescription = "Description 1";
			Factory.Save();

			var collectionGovernor = new SchedulerServiceTasksLoader(Factory).Load();

			var allTasks = collectionGovernor.GovernedTasks
				.Select(_ => TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: false, new ImmediateActionQueue(), transactionAdapter: Mock.Of<ITransactionAdapter>()))
				.ToList();

			// Act
			RunnableServiceTaskTestHelper.DbUpdateServiceTaskSchedule(
				pk: schedule.PK.ToGuid(),
				isActive: false,
				description: "Description 2"
			);
			collectionGovernor.Reload();

			foreach (var task in allTasks)
			{
				task.UpdateSchedule(collectionGovernor.GovernedTasks, false);

				// Assert
				NUnit.Framework.Assert.That(task.ScheduleDescription, Is.EqualTo("Description 2"));
				NUnit.Framework.Assert.That(task.IsActive, Is.EqualTo(false));
			}
		}

		[ExpectNoExceptions]
		public void TestOnErrorReportedIncrementsErrorCountLast24Hours()
		{
			// Arrange
			var task = new RunnableServiceTask(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
				Mock.Of<IServiceTask>(),
				Mock.Of<IBackgroundThreadActionQueue>(),
				Mock.Of<ITaskQueue>(),
				Mock.Of<IHostLogger>(),
				Mock.Of<IErrorReporterProxy>(),
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ITransactionAdapter>(),
				Mock.Of<ILoggerFactory>(),
				Mock.Of<IServiceTaskScheduleStatusProvider>());

			var errorCount = task.ErrorCountLast24Hours;

			// Act
			task.OnErrorReported();

			// Assert
			NUnit.Framework.Assert.That(task.ErrorCountLast24Hours, Is.EqualTo(errorCount + 1));
		}

		[ExpectNoExceptions]
		public void TestOnErrorReportedUpdatesLastErrorTime()
		{
			// Arrange
			var task = new RunnableServiceTask(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
				Mock.Of<IServiceTask>(),
				Mock.Of<IBackgroundThreadActionQueue>(),
				Mock.Of<ITaskQueue>(),
				Mock.Of<IHostLogger>(),
				Mock.Of<IErrorReporterProxy>(),
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ITransactionAdapter>(),
				Mock.Of<ILoggerFactory>(),
				Mock.Of<IServiceTaskScheduleStatusProvider>());

			task.OnErrorReported();
			var previousLastErrorTime = task.LastErrorTime;
			Task.Delay(TimeSpan.FromSeconds(0.2)).Wait();

			// Act
			task.OnErrorReported();

			// Assert
			NUnit.Framework.Assert.That(task.LastErrorTime, Is.GreaterThan(previousLastErrorTime));
		}

		[UseSnapshotProtection]
		public class UpdateScheduleAdjustPeriodTest : TestCaseWithFactory
		{
			public class ValidValuesTest : UpdateScheduleAdjustPeriodTest
			{
				[ExpectNoExceptions]
				public void TestMinWithMax()
				{
					const int min = 10;
					const int max = min + 20;
					var value = min + 10;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute() { MinimumPeriod = $"{min}s", MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between max: {max}s and min: {min}s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}

				[ExpectNoExceptions]
				public void TestMinWithMaxAndValueEqualToMin()
				{
					const int min = 10;
					const int max = min + 20;
					var value = min;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute() { MinimumPeriod = $"{min}s", MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between max: {max}s and min: {min}s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}

				[ExpectNoExceptions]
				public void TestMinWithMaxAndValueEqualToMax()
				{
					const int min = 10;
					const int max = min + 20;
					var value = max;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute() { MinimumPeriod = $"{min}s", MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between max: {max}s and min: {min}s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}

				[ExpectNoExceptions]
				public void TestMinWithNoMax()
				{
					const int min = 10;
					var value = min + 10;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute() { MinimumPeriod = $"{min}s", DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between infinite and min: {min}s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}

				[ExpectNoExceptions]
				public void TestDefaultMinWithMax()
				{
					const int min = 10;
					const int max = min + 20;
					var value = min + 10;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute { MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between max: {max}s and  default min: 1s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}

				[ExpectNoExceptions]
				public void TestDefaultMinWithNoMax()
				{
					const int min = 10;
					const int max = min + 20;
					var value = min + 10;
					var description = "Test Task";
					var attribute = new HostedServiceAttribute { DefaultScheduleRunEvery = $"{value}s" };

					var failureMessage = $"Expected value: {value} remains same as it's between max: {max}s and  deafult min: 1s";
					TestUpdateScheduleAdjustPeriod(value, description, attribute, value, failureMessage, Times.Never());
				}
			}

			public class AboveMaxTest : UpdateScheduleAdjustPeriodTest
			{
				[ExpectNoExceptions]
				public void TestWithMin_SetToMax()
				{
					const int min = 10;
					const int max = min + 20;
					const int value = max + 10;
					var description = "Test Task";
					var failureMessage = $"Expected to cap the set expected value: {value} to max: {max}s as it's above the max.";
					var attribute = new HostedServiceAttribute { MinimumPeriod = $"{min}s", MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					TestUpdateScheduleAdjustPeriod(value, description, attribute, max, failureMessage, Times.Once(), loggerMock =>
					{
						loggerMock
							.As<ILogger>()
							.VerifyLog(
								LogLevel.Warning,
								message => message.StartsWith($"Service task period [00:00:40] was updated to [00:00:30] to be in allowed interval for service task {description}."), Times.Once);
					});
				}

				[ExpectNoExceptions]
				public void TestWithNoMin_SetToMax()
				{
					const int min = 10;
					const int max = min + 20;
					const int value = max + 10;
					var description = "Test Task";
					var failureMessage = $"Expected to cap the set expected value: {value} to max: {max}s as it's above the max.";
					var attribute = new HostedServiceAttribute { MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					TestUpdateScheduleAdjustPeriod(value, description, attribute, max, failureMessage, Times.Once());
				}
			}

			public class BelowMinTest : UpdateScheduleAdjustPeriodTest
			{
				[ExpectNoExceptions]
				public void TestWithMax_SetToMin()
				{
					const int min = 10;
					const int max = min + 20;
					const int value = min - 10;
					var description = "Test Task";
					var failureMessage = $"Expected to floor the set expected value: {value} to min: {min}s as it's below the min.";
					var attribute = new HostedServiceAttribute { MinimumPeriod = $"{min}s", MaximumPeriod = $"{max}s", DefaultScheduleRunEvery = $"{value}s" };

					TestUpdateScheduleAdjustPeriod(value, description, attribute, min, failureMessage, Times.Once());
				}

				[ExpectNoExceptions]
				public void TestWithNoMax_SetToMin()
				{
					const int min = 10;
					const int value = min - 10;
					var description = "Test Task";
					var failureMessage = $"Expected to floor the set expected value: {value} to min: {min}s as it's below the min.";
					var attribute = new HostedServiceAttribute { MinimumPeriod = $"{min}s", DefaultScheduleRunEvery = $"{value}s" };

					TestUpdateScheduleAdjustPeriod(value, description, attribute, min, failureMessage, Times.Once());
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				factory = Factory;
			}

			protected override void TearDown()
			{
				factory = null;
				base.TearDown();
			}

			[ExpectNoExceptions]
			static void TestUpdateScheduleAdjustPeriod(int value, string taskDescription, HostedServiceAttribute attribute, int expectedValue, string failureMessage, Times expectedLogCount, Action<Mock<IHostLogger>> loggerAssertions = null)
			{
				// Arrange
				var schedule = factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = "~~T";
				schedule.S5_TaskPeriodCount = value;
				schedule.S5_TaskPeriod = "S";
				schedule.S5_ScheduleDescription = taskDescription;
				schedule.SetStaticServiceAttributesDebugOnly(attribute);
				var scheduleTask = new SchedulerServiceTask(schedule);

				var expectedLogMessage = $"Service task period [{TimeSpan.FromSeconds(value)}] was updated to [{TimeSpan.FromSeconds(expectedValue)}] to be in allowed interval for service task {taskDescription}.";
				var loggerMock = new Mock<IHostLogger>();

				using (ObjectFactory.Substitute(Mock.Of<IServiceHostsCache>(x => x.AvailableServiceHosts == Enumerable.Empty<IServiceHostClient>())))
				{
					factory.Save();
					using var immediateActionQueue = new ImmediateActionQueue();
					var task = TaskSchedulerTest.CreateTask(scheduleTask, false, immediateActionQueue, logger: loggerMock.Object, transactionAdapter: Mock.Of<ITransactionAdapter>());

					// Act
					task.UpdateSchedule(new[] { scheduleTask }, false);

					// Assert
					var query = new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, schedule.S5_ScheduleType);
					var serviceTask = factory.Load<ServiceTaskSchedule>(query).First();
					NUnit.Framework.Assert.That(serviceTask.S5_TaskPeriodCount, Is.EqualTo(expectedValue).Using(CustomComparers.TypeComparison), failureMessage);
					loggerMock
						.As<ILogger>()
						.VerifyLog(LogLevel.Warning, expectedLogMessage, () => expectedLogCount);

					if (value != expectedValue)
					{
						loggerMock
							.As<ILogger>()
							.VerifyLog(LogLevel.Warning,
								message => message.StartsWith($"Service task {taskDescription} - \"next runtime\" has been corrected from"), Times.Once);
						loggerMock
							.As<ILogger>()
							.VerifyLog(LogLevel.Debug,
								message => message.StartsWith("Next runtime was out of the expected range, so it was recalculated based off 'now'"), Times.Once);
					}

					loggerAssertions?.Invoke(loggerMock);
					loggerMock.VerifyNoOtherCalls();
				}
			}

			static BusinessObjectFactory factory;
		}

		public class OnSuccessfulRunAttemptTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestDirectRequestIsAddedToActionQueueWithDelay()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(TimeSpan.FromSeconds(1));
					Test(TimeSpan.FromSeconds(10));
					Test(TimeSpan.FromSeconds(100));
				});

				void Test(TimeSpan delay)
				{
					// Arrange
					var hostLoggerMock = new Mock<IHostLogger>();
					var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
					var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();

					var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, transactionAdapter: Mock.Of<ITransactionAdapter>(), taskQueue: Mock.Of<ITaskQueue>());
					var directTaskRunRequestMock = Mock.Of<IDirectTaskRunRequest>(
						request => request.HasRunsRemaining
						&& request.NextRunDelay == delay
						&& request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD"));
					actionQueueMock.Invocations.Clear();

					// Act
					task.OnSuccessfulRunAttempt(directTaskRunRequestMock);

					// Assert
					actionQueueMock.Verify(queue => queue.Enqueue(delay, It.IsAny<Action>()), Times.Once);
					actionQueueMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestDelayedDirectRequestWaitingForTaskCompletionBailsIfTaskIsUnableToRun_SuccessfulRun()
			{
				AssertDirectRequestIsAddedToActionQueueWithDelaysUntilTaskFinishesRunning((IRunnableServiceTask task, ITaskRunRequest request) => task.OnSuccessfulRun(request));
			}

			[ExpectNoExceptions]
			public void TestDelayedDirectRequestWaitingForTaskCompletionBailsIfTaskIsUnableToRun_UnableToRun()
			{
				AssertDirectRequestIsAddedToActionQueueWithDelaysUntilTaskFinishesRunning((IRunnableServiceTask task, ITaskRunRequest request) => task.HandleUnableToRun(UnableToRunReason.ConfigurationError, request, false, false));
			}

			public void AssertDirectRequestIsAddedToActionQueueWithDelaysUntilTaskFinishesRunning(Action<IRunnableServiceTask, ITaskRunRequest> taskCompletionAction)
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(TimeSpan.FromSeconds(1), 1);
					Test(TimeSpan.FromSeconds(10), 5);
					Test(TimeSpan.FromSeconds(100), 10);
				});

				void Test(TimeSpan delay, int numberOfRuns)
				{
					// Arrange
					var hostLoggerMock = new Mock<IHostLogger>();
					var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
					var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
					var taskQueueMock = new Mock<ITaskQueue>();

					var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, transactionAdapter: Mock.Of<ITransactionAdapter>(), taskQueue: taskQueueMock.Object);
					var directTaskRunRequestMock = Mock.Of<IDirectTaskRunRequest>(
						request => request.HasRunsRemaining
									&& request.NextRunDelay == delay
									&& request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD"));
					actionQueueMock.Invocations.Clear();

					var actionQueue = new Queue<Action>();
					var numberOfInvocations = 0;
					actionQueueMock
						.Setup(q => q.Enqueue(It.IsAny<TimeSpan>(), It.IsAny<Action>()))
						.Callback((TimeSpan delay, Action action) =>
						{
							numberOfInvocations++;
							if (numberOfInvocations >= numberOfRuns)
							{
								taskCompletionAction(task, directTaskRunRequestMock);
								task.OnSuccessfulRun(directTaskRunRequestMock);
							}
							actionQueue.Enqueue(action);
						});

					// Act
					task.OnSuccessfulRunAttempt(directTaskRunRequestMock);
					var j = 0;
					while (actionQueue.Count > 0 && j <= numberOfRuns)
					{
						j++;
						var action = actionQueue.Dequeue();
						action();
					}

					// Assert
					hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, It.Is<string>(s => s.Contains("Re-enqueue with delay"))), Times.Exactly(numberOfRuns));
					hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, It.Is<string>(s => s.Contains($"Completed after [{numberOfRuns}] requeue attempts, enqueue now"))), Times.Once);
					actionQueueMock.Verify(queue => queue.Enqueue(delay, It.IsAny<Action>()), Times.Exactly(numberOfRuns));
					actionQueueMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestDirectRequestIsAddedToTaskQueue()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(TimeSpan.FromSeconds(1));
					Test(TimeSpan.FromSeconds(10));
					Test(TimeSpan.FromSeconds(100));
				});

				void Test(TimeSpan delay)
				{
					// Arrange
					var hostLoggerMock = new Mock<IHostLogger>();
					var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
					Action queueAction = null;
					var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
					var taskQueueMock = new Mock<ITaskQueue>();

					var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: new SchedulerServiceTaskTransactionAdapter());
					var directTaskRunRequestMock = Mock.Of<IDirectTaskRunRequest>(
						request => request.HasRunsRemaining
						&& request.NextRunDelay == delay
						&& request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD"));
					taskQueueMock.Invocations.Clear();
					actionQueueMock
						.Setup(q => q.Enqueue(It.IsAny<TimeSpan>(), It.IsAny<Action>()))
						.Callback((TimeSpan delay, Action action) =>
						{
							task.OnSuccessfulRun(directTaskRunRequestMock);
							queueAction = action;
						});

					// Act
					task.OnSuccessfulRunAttempt(directTaskRunRequestMock);
					queueAction.Invoke();

					// Assert
					taskQueueMock.Verify(queue => queue.EnqueueTask(directTaskRunRequestMock), Times.Once);
					taskQueueMock.VerifyNoOtherCalls();
				}
			}

			[ExpectNoExceptions]
			public void TestOnSuccessfulRunLogsForDirectTaskRunRequest_1() => AssertOnSuccessfulRunLogsForDirectTaskRunRequest(
				"ASD",
				"874bd6ac-8d51-4f21-9aae-d85e4bfa16fd",
				"[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Run succeeded, releasing lock");
			[ExpectNoExceptions]
			public void TestOnSuccessfulRunLogsForDirectTaskRunRequest_2() => AssertOnSuccessfulRunLogsForDirectTaskRunRequest(
				"DSA",
				"e077eb3d-6300-4c72-978c-96c65a80c996",
				"[DSA/e077eb3d-6300-4c72-978c-96c65a80c996] - Requeue Lock: Run succeeded, releasing lock");

			void AssertOnSuccessfulRunLogsForDirectTaskRunRequest(string code, string guid, string expectedLog)
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var taskQueueMock = new Mock<ITaskQueue>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: transactionAdapter);

				// Act
				task.OnSuccessfulRun(Mock.Of<IDirectTaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == code) && t.Id == Guid.Parse(guid)));

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, expectedLog), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestOOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest_1() => AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest("ASD", "874bd6ac-8d51-4f21-9aae-d85e4bfa16fd", "[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Run succeeded, releasing lock");
			[ExpectNoExceptions]
			public void TestOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest_2() => AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest("DSA", "e077eb3d-6300-4c72-978c-96c65a80c996", "[ASD/e077eb3d-6300-4c72-978c-96c65a80c996] - Requeue Lock: Run succeeded, releasing lock");

			[ExpectNoExceptions]
			void AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest(string code, string guid, string expectedLog)
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var taskQueueMock = new Mock<ITaskQueue>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: transactionAdapter);

				// Act
				task.OnSuccessfulRun(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == code) && t.Id == Guid.Parse(guid)));

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, expectedLog), Times.Never);
			}

			[ExpectNoExceptions]
			public void TestOnSuccessfulRunAttemptLogsWarningForDirectTaskRunRequestIfTaskAlreadyRunning()
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var taskQueueMock = new Mock<ITaskQueue>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: transactionAdapter);

				// Act

				task.OnSuccessfulRunAttempt(Mock.Of<IDirectTaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD") && t.Id == Guid.Parse("874BD6AC-8D51-4F21-9AAE-D85E4BFA16FD")));
				task.OnSuccessfulRunAttempt(Mock.Of<IDirectTaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD") && t.Id == Guid.Parse("874BD6AC-8D51-4F21-9AAE-D85E4BFA16FD")));

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Acquire lock on task to prevent unnecessary retries while task has not returned from runner"), Times.Exactly(2));
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Separate request has overridden current run, release lock to prevent blocking of new service task run"), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestOnSuccessfulRunAttemptUpdatesLastRunTime()
			{
				// Arrange
				var governor = new Mock<IServiceTaskGovernor>();

				var schedule = new Mock<IServiceTask>();
				schedule.Setup(o => o.AssignedGovernor).Returns(governor.Object);

				var task = new RunnableServiceTask(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
					schedule.Object,
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>());

				task.OnSuccessfulRunAttempt(Mock.Of<ITaskRunRequest>());
				var previousLastRunTime = task.LastRunTime;
				Task.Delay(TimeSpan.FromSeconds(0.2)).Wait();

				// Act
				task.OnSuccessfulRunAttempt(Mock.Of<ITaskRunRequest>());

				// Assert
				NUnit.Framework.Assert.That(task.LastRunTime, Is.GreaterThan(previousLastRunTime));
			}
		}

		public class OnUnableToRunTest : TestCaseWithFactory
		{
			public void TestLogsAction()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(Mock.Of<ITaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), false, false, LogMessageStage.IgnoreRequest);
					Test(Mock.Of<ITaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), false, true, LogMessageStage.IgnoreRequest);
					Test(Mock.Of<ITaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), true, false, LogMessageStage.ReprocessRequest);
					Test(Mock.Of<ITaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), true, true, LogMessageStage.ReprocessRequest);
					Test(Mock.Of<IDirectTaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), false, false, LogMessageStage.IgnoreRequest);
					Test(Mock.Of<IDirectTaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), false, true, LogMessageStage.IgnoreRequest);
					Test(Mock.Of<IDirectTaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), true, false, LogMessageStage.ReprocessRequest);
					Test(Mock.Of<IDirectTaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD")), true, true, LogMessageStage.ReprocessRequest);
				});

				void Test(ITaskRunRequest request, bool retry, bool failedPostScheduleUpdate, LogMessageStage expectedLogMessageStage)
				{
					// Arrange
					var hostLoggerMock = new Mock<IHostLogger>();
					var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
					var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object);

					// Act
					task.HandleUnableToRun(UnableToRunReason.ConfigurationError, request, retry, failedPostScheduleUpdate);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						Mock.Get(request).Verify(x => x.FormatRequestToLogMessage(expectedLogMessageStage, UnableToRunReason.ConfigurationError), Times.Once);
						hostLoggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Exactly(request is IDirectTaskRunRequest ? 2 : 1));
					});
				}
			}

			public void TestLogsReason()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					foreach (var (reason, logLevel, message) in UnableToRunReasonHelper.Source)
					{
						Test(reason, logLevel, LogMessageStage.IgnoreRequest);
					}
				});

				void Test(UnableToRunReason failureReason, LogLevel logLevel, LogMessageStage expectedLogMessageStage)
				{
					// Arrange
					var hostLoggerMock = new Mock<IHostLogger>();
					var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
					var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object);
					var request = Mock.Of<ITaskRunRequest>(request => request.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD"));

					// Act
					task.HandleUnableToRun(failureReason, request, false, true);

					// Assert
					AssertNoExceptionThrown(() =>
					{
						Mock.Get(request).Verify(x => x.FormatRequestToLogMessage(expectedLogMessageStage, failureReason), Times.Once);
						hostLoggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Once);
					});
				}
			}

			[ExpectNoExceptions]
			public void TestOnFailedRunLogsForDirectTaskRunRequest_Retry() => AssertOnFailedRunLogsForDirectTaskRunRequest(true);
			[ExpectNoExceptions]
			public void TestOnFailedRunLogsForDirectTaskRunRequest_NoRetry() => AssertOnFailedRunLogsForDirectTaskRunRequest(false);

			void AssertOnFailedRunLogsForDirectTaskRunRequest(bool retry)
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var taskQueueMock = new Mock<ITaskQueue>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: transactionAdapter);

				// Act
				task.HandleUnableToRun(UnableToRunReason.OtherHostIsSchedulingTheTask, Mock.Of<IDirectTaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD") && t.Id == Guid.Parse("874BD6AC-8D51-4F21-9AAE-D85E4BFA16FD")), retry, false);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Run failed, releasing lock"), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest_Retry() => AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest(true);
			[ExpectNoExceptions]
			public void TestOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest_NoRetry() => AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest(false);

			void AssertOnSuccessfulRunDoesNotLogForNonDirectTaskRunRequest(bool retry)
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var taskQueueMock = new Mock<ITaskQueue>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object, actionQueue: actionQueueMock.Object, taskQueue: taskQueueMock.Object, transactionAdapter: transactionAdapter);

				// Act
				task.HandleUnableToRun(UnableToRunReason.OtherHostIsSchedulingTheTask, Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD") && t.Id == Guid.Parse("874BD6AC-8D51-4F21-9AAE-D85E4BFA16FD")), retry, false);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "[ASD/874bd6ac-8d51-4f21-9aae-d85e4bfa16fd] - Requeue Lock: Run failed, releasing lock"), Times.Never);
			}
		}

		public class OnQueuedResponseTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestLogsQueuedResponse()
			{
				// Arrange
				var hostLoggerMock = new Mock<IHostLogger>();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), logger: hostLoggerMock.As<IHostLogger>().Object);
				var taskRunRequest = Mock.Of<ITaskRunRequest>();
				var serviceRunner = Mock.Of<IServiceRunner>();

				// Act
				task.OnQueuedResponse(serviceRunner, taskRunRequest);

				// Assert
				Mock.Get(taskRunRequest).Verify(x => x.FormatRequestToLogMessage(LogMessageStage.RequestIsQueuedByRunner, serviceRunner), Times.Once);
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<string>()), Times.Once);
			}
		}

		public class ReloadConfigurationAsyncTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				serviceTaskInfoMock = new Mock<ServiceTaskInfo>(Mock.Of<IHostedServiceAttribute>());

				backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				backgroundThreadActionQueueMock
					.Setup(queue => queue.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(action => action.Invoke());

				attributeMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Code == "C01" &&
					a.Description == "Desc 1" &&
					a.Category == "T1" &&
					a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d =>
						d.DoNotRunTillNextDueTimeIfOverdue == "10seconds" &&
						d.RunEvery == "1minute"));

				attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(p =>
					p.GetClientHostedServiceAttribute(It.IsAny<string>()) == attributeMock);

				statusProviderMock = Mock.Of<IServiceTaskScheduleStatusProvider>();
				bindingsProviderMock = Mock.Of<IHostedServiceBusinessObjectBindingsProvider>();
			}

			[TestDate(2006, 12, 26)]
			[ExpectNoExceptions]
			public void TestDoesNotSaveSchedules()
			{
				// Arrange
				var timeInDatabase = ZDateTime.Now.ToDateTime();
				var nextRuntime = new DateTime(2019, 12, 23);

				var (miscellaneousSchedules, schedule, task) = CreateAndFillSchedules();

				SetNextRuntimeInMemory(miscellaneousSchedules, nextRuntime, task);

				// Act
				task.ReloadConfigurationAsync();

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					var result = GetNextRunTime(schedule);
					NUnit.Framework.Assert.That(result, Is.EqualTo(timeInDatabase), schedule.S5_ScheduleType.ToString());

					foreach (var (taskSchedule, _) in miscellaneousSchedules)
					{
						result = GetNextRunTime(taskSchedule);
						NUnit.Framework.Assert.That(result, Is.EqualTo(timeInDatabase), taskSchedule.S5_ScheduleType.ToString());
					}

					DateTime GetNextRunTime(BusinessObject taskSchedule)
					{
						return Db.Connection.ExecuteScalar<DateTime>("SELECT S5_NextScheduledPrintRunTimeUtc FROM dbo.StmScheduleTask WHERE S5_PK=@pk",
							command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, taskSchedule.PK.ToGuid()));
					}
				});
			}

			[TestDate(2022, 02, 01)]
			[ExpectNoExceptions]
			public void TestFutureRunTimeCheckingDoesNotProduceMemoryLeaks()
			{
				// Arrange
				const int accessCount = 5000;
				const int increasedMemoryTolerance = 10_000;
				var nextRuntime = ZDateTime
					.UtcNow
					.AddDays(-1)
					.ToDateTime();

				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var task = new RunnableServiceTask(serviceTaskInfoMock.Object, new SchedulerServiceTask(schedule), backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				task.SetNextRunTime(nextRuntime, SetNextRuntimeReason.ScheduledToRun);
				task.UpdateUnderlyingScheduleNextRunTime();
				Factory.Save();

				var beforeMem = GC.GetTotalMemory(true);

				// Act
				for (int i = 0; i < accessCount; i++)
				{
					_ = task.NextRunTimeIsInFuture;
				}
				var afterMem = GC.GetTotalMemory(true);

				// Assert
				NUnit.Framework.Assert.That(afterMem, Is.LessThanOrEqualTo(beforeMem).Within(increasedMemoryTolerance),
					$"Memory should not leak, but {(afterMem - beforeMem)} bytes are not released.");
			}

			[TestDate(2006, 12, 26)]
			[ExpectNoExceptions]
			public void TestReloadsOnlyRequestedSchedule()
			{
				// Arrange
				var timeInDatabase = ZDateTime.Now.ToDateTime();
				var nextRuntime = new DateTime(2019, 12, 23, 0, 0, 0, DateTimeKind.Utc);

				var (miscellaneousSchedules, schedule, task) = CreateAndFillSchedules();

				SetNextRuntimeInMemory(miscellaneousSchedules, nextRuntime, task);

				// Act
				task.ReloadConfigurationAsync();

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(schedule.S5_NextScheduledPrintRunTimeUtc.ToDateTime(), Is.EqualTo(timeInDatabase), schedule.S5_ScheduleType.ToString());

					foreach (var (taskSchedule, _) in miscellaneousSchedules)
					{
						NUnit.Framework.Assert.That(taskSchedule.S5_NextScheduledPrintRunTimeUtc.ToDateTime(), Is.EqualTo(nextRuntime), taskSchedule.S5_ScheduleType.ToString());
					}
				});
			}

			[TestDate(2006, 12, 26)]
			[ExpectNoExceptions]
			public void TestReloadsStmServiceTaskSchedule()
			{
				// Arrange
				var timeInDatabase = ZDateTimeOffset.UtcNow.ToDateTimeOffset();
				var nextRuntime = new DateTimeOffset(2019, 12, 23, 0, 0, 0, TimeSpan.Zero);

				var (miscellaneousSchedules, schedule, task) = CreateAndFillStmSchedules();

				SetStmNextRuntimeInMemory(miscellaneousSchedules, nextRuntime, task);

				// Act
				task.ReloadConfigurationAsync();

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(task.NextRunTime, Is.EqualTo(timeInDatabase), schedule.Code);

					foreach (var (taskSchedule, runnableTask) in miscellaneousSchedules)
					{
						NUnit.Framework.Assert.That(runnableTask.NextRunTime, Is.EqualTo(nextRuntime), taskSchedule.Code);
					}
				});

				static void SetStmNextRuntimeInMemory(IEnumerable<(IServiceTask taskSchedule, RunnableServiceTask runnableServiceTask)> miscellaneousSchedules, DateTimeOffset nextRuntime, IRunnableServiceTask task)
				{
					foreach (var (_, runnableServiceTask) in miscellaneousSchedules)
					{
						runnableServiceTask.SetNextRunTime(nextRuntime, SetNextRuntimeReason.ScheduledToRun);
						runnableServiceTask.UpdateUnderlyingScheduleNextRunTime();
					}

					task.SetNextRunTime(nextRuntime, SetNextRuntimeReason.ScheduledToRun);
					task.UpdateUnderlyingScheduleNextRunTime();
				}
			}

			[ExpectNoExceptions]
			public void TestReloadsStmServiceTaskSchedule_ActiveStatus()
			{
				// Arrange
				var timeInDatabase = ZDateTimeOffset.UtcNow.ToDateTimeOffset();
				var nextRuntime = new DateTimeOffset(2019, 12, 23, 0, 0, 0, TimeSpan.Zero);

				var schedule = CreateStmServiceTask("~~T");
				var task = new RunnableServiceTask(serviceTaskInfoMock.Object, schedule, backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				NUnit.Framework.Assert.That(task.IsActive, Is.True, "Precondition");

				SetTaskInactiveInDb();

				// Act
				task.ReloadConfigurationAsync();

				// Assert
				NUnit.Framework.Assert.That(task.IsActive, Is.False);

				static void SetTaskInactiveInDb()
				{
					using var cmd = Db.Connection.Command("update dbo.StmServiceTask set SST_Active = 0, SST_SystemLastEditTimeUtc = GETUTCDATE(), SST_SystemLastEditUser = '~BP' where SST_ServiceTaskCode = '~~T'");
					cmd.ExecuteNonQuery();
				}
			}

			[ExpectNoExceptions]
			public void TestLogMessageWhenEnqueue()
			{
				// Arrange
				const string testCode = "TES";
				var schedule = TaskSchedulerTest.CreateSchedule(testCode, Factory);
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				var logger = new Mock<IHostLogger>();
				var taskConfig = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));
				var task = new RunnableServiceTask(taskConfig, new SchedulerServiceTask(schedule), actionQueue.Object, Mock.Of<ITaskQueue>(), logger.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				// Act
				task.Enqueue(false);

				// Assert
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[[0-9A-Z]{{3}}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Nudge run request is created.")), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestEnqueueDelayedRequest()
			{
				// Arrange
				const string testCode = "TES";
				var delayTime = TimeSpan.FromSeconds(5);
				var schedule = TaskSchedulerTest.CreateSchedule(testCode, Factory);
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				actionQueue
					.Setup(x => x.Enqueue(It.IsAny<TimeSpan>(), It.IsAny<Action>()))
					.Callback<TimeSpan, Action>((x, y) => y.Invoke());
				var logger = new Mock<IHostLogger>();
				var taskQueue = Mock.Of<ITaskQueue>();
				var taskConfig = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));
				var task = new RunnableServiceTask(taskConfig, new SchedulerServiceTask(schedule), actionQueue.Object, taskQueue, logger.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var request = Mock.Of<IDirectTaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "ASD") && t.Id == Guid.Parse("874BD6AC-8D51-4F21-9AAE-D85E4BFA16FD"));

				// Act
				task.HandleUnableToRun(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, request, true, false);

				// Assert
				actionQueue.Verify(x => x.Enqueue(It.IsAny<TimeSpan>(), It.IsAny<Action>()), Times.Once);
				Mock.Get(taskQueue).Verify(x => x.EnqueueTask(request), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestLogMessageWhenEnqueueDelayed()
			{
				// Arrange
				const string testCode = "TES";
				var delayTime = TimeSpan.FromSeconds(5);
				var schedule = TaskSchedulerTest.CreateSchedule(testCode, Factory);
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				actionQueue
					.Setup(x => x.Enqueue(It.IsAny<TimeSpan>(), It.IsAny<Action>()))
					.Callback<TimeSpan, Action>((x, y) => y.Invoke());
				var logger = new Mock<IHostLogger>();
				var taskConfig = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(x => x.Code == testCode));
				var task = new RunnableServiceTask(taskConfig, new SchedulerServiceTask(schedule), actionQueue.Object, Mock.Of<ITaskQueue>(), logger.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				// Act
				task.EnqueueDelayed(delayTime, false);

				// Assert
				logger.Verify(x => x.Log(LogLevel.Debug, "Delay 00:00:05 before creating Nudge run request for task [TES]."), Times.Once);
				logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[[0-9A-Z]{{3}}/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}\\] Delayed Nudge run request is created.")), Times.Once);
			}

			(List<(ServiceTaskSchedule taskSchedule, RunnableServiceTask runnableServiceTask)> miscellaneousSchedules, ServiceTaskSchedule schedule, RunnableServiceTask task) CreateAndFillSchedules()
			{
				var transactionAdapterMock = new Mock<ITransactionAdapter>();

				var miscellaneousSchedules = Enumerable.Range(0, 10)
					.Select(i => TaskSchedulerTest.CreateSchedule($"TS{i}", Factory))
					.Select(taskSchedule => (taskSchedule, runnableServiceTask: new RunnableServiceTask(serviceTaskInfoMock.Object, new SchedulerServiceTask(taskSchedule), backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>())))
					.ToList();
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory);
				var task = new RunnableServiceTask(serviceTaskInfoMock.Object, new SchedulerServiceTask(schedule), backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				Factory.Save();
				return (miscellaneousSchedules, schedule, task);
			}

			static void SetNextRuntimeInMemory(IEnumerable<(ServiceTaskSchedule taskSchedule, RunnableServiceTask runnableServiceTask)> miscellaneousSchedules, DateTime nextRuntime, IRunnableServiceTask task)
			{
				foreach (var (_, runnableServiceTask) in miscellaneousSchedules)
				{
					runnableServiceTask.SetNextRunTime(nextRuntime, SetNextRuntimeReason.ScheduledToRun);
					runnableServiceTask.UpdateUnderlyingScheduleNextRunTime();
				}

				task.SetNextRunTime(nextRuntime, SetNextRuntimeReason.ScheduledToRun);
				task.UpdateUnderlyingScheduleNextRunTime();
			}

			(List<(IServiceTask taskSchedule, RunnableServiceTask runnableServiceTask)> miscellaneousSchedules, IServiceTask schedule, RunnableServiceTask task) CreateAndFillStmSchedules()
			{
				var transactionAdapterMock = new Mock<ITransactionAdapter>();

				var miscellaneousSchedules = Enumerable.Range(0, 10)
					.Select(i => CreateStmServiceTask($"TS{i}"))
					.Select(serviceTask =>
						(serviceTask, runnableServiceTask: new RunnableServiceTask(
							serviceTaskInfoMock.Object,
							serviceTask,
							backgroundThreadActionQueueMock.Object,
							Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>())))
					.ToList();

				var serviceTask = CreateStmServiceTask("TST");
				var task = new RunnableServiceTask(serviceTaskInfoMock.Object, serviceTask, backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock.Object, Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				return (miscellaneousSchedules, serviceTask, task);
			}

			IServiceTask CreateStmServiceTask(string code, bool active = true, string taskPeriod = "S", int taskPeriodCount = 10, Mock<IHostedServiceAttribute> config = null)
			{
				var dto = new NativeServiceTaskDTO(
					code,
					Guid.Empty,
					false,
					ZDateTimeOffset.UtcNow.ToDateTimeOffset(),
					null,
					null,
					null,
					null,
					"");

				var governor = new NativeServiceTaskGovernor(dto, attributeProviderMock, statusProviderMock, bindingsProviderMock, new ServiceManagerDateTimeProvider());

				governor.SetSchedule(taskPeriodCount, taskPeriod);
				governor.SetActive(active);
				governor.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset());
				governor.Save();

				return governor.GovernedTask;
			}

			Mock<IBackgroundThreadActionQueue> backgroundThreadActionQueueMock;
			Mock<ServiceTaskInfo> serviceTaskInfoMock;

			IHostedServiceAttribute attributeMock;
			IClientHostedServiceAttributeProvider attributeProviderMock;
			IServiceTaskScheduleStatusProvider statusProviderMock;
			IHostedServiceBusinessObjectBindingsProvider bindingsProviderMock;
		}

		public class HasScheduleUpdatesTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestS5_TaskPeriod()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_TaskPeriod = "asd", schedule => schedule.S5_TaskPeriod = "asd");
					Test(true, schedule => schedule.S5_TaskPeriod = "asd", schedule => schedule.S5_TaskPeriod = "zxc");
				});
			}

			[ExpectNoExceptions]
			public void TestS5_TaskPeriodCount()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_TaskPeriodCount = 10, schedule => schedule.S5_TaskPeriodCount = 10);
					Test(true, schedule => schedule.S5_TaskPeriodCount = 10, schedule => schedule.S5_TaskPeriodCount = 11);
				});
			}

			[ExpectNoExceptions]
			public void TestRecurrence_DayOfMonth()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.Recurrence.DayOfMonth = 10, schedule => schedule.Recurrence.DayOfMonth = 10);
					Test(true, schedule => schedule.Recurrence.DayOfMonth = 10, schedule => schedule.Recurrence.DayOfMonth = 11);
				});
			}

			[ExpectNoExceptions]
			public void TestS5_DayList()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_DayList = "YNNNNNN", schedule => schedule.S5_DayList = "YNNNNNN");
					Test(true, schedule => schedule.S5_DayList = "YNNNNNN", schedule => schedule.S5_DayList = "NNNNNNN");
				});
			}

			[ExpectNoExceptions]
			public void TestS5_MonthNumber()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_MonthNumber = 1, schedule => schedule.S5_MonthNumber = 1);
					Test(true, schedule => schedule.S5_MonthNumber = 1, schedule => schedule.S5_MonthNumber = 2);
				});
			}

			[ExpectNoExceptions]
			public void TestS5_NextScheduledPrintRunTimeUtc()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(
						false,
						schedule => schedule.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2019, 12, 23, 20, 20, 20));
					Test(
						true,
						schedule => schedule.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2022, 7, 21, 8, 30, 20));
				});
			}

			[ExpectNoExceptions]
			public void TestS5_ScheduleState()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_ScheduleState = Encoding.ASCII.GetBytes("<xml></xml>"), schedule => schedule.S5_ScheduleState = Encoding.ASCII.GetBytes("<xml></xml>"));
					Test(true, schedule => schedule.S5_ScheduleState = Encoding.ASCII.GetBytes("<xml></xml>"), schedule => schedule.S5_ScheduleState = Encoding.ASCII.GetBytes("<xml><a/></xml>"));
				});
			}

			[ExpectNoExceptions]
			public void TestS5_GB()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_GB = new Guid("00000000-0000-0000-0000-000000000001"), schedule => schedule.S5_GB = new Guid("00000000-0000-0000-0000-000000000001"));
					Test(true, schedule => schedule.S5_GB = new Guid("00000000-0000-0000-0000-000000000001"), schedule => schedule.S5_GB = new Guid("00000000-0000-0000-0000-000000000002"));
				});
			}

			[ExpectNoExceptions]
			public void TestS5_DailyStartTime()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(
						false,
						schedule => schedule.S5_DailyStartTime = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_DailyStartTime = new ZDateTime(2019, 12, 23, 20, 20, 20));
					Test(
						true,
						schedule => schedule.S5_DailyStartTime = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_DailyStartTime = new ZDateTime(2022, 7, 21, 8, 30, 20));
				});
			}

			[ExpectNoExceptions]
			public void TestS5_DailyEndTime()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(
						false,
						schedule => schedule.S5_DailyEndTime = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_DailyEndTime = new ZDateTime(2019, 12, 23, 20, 20, 20));
					Test(
						true,
						schedule => schedule.S5_DailyEndTime = new ZDateTime(2019, 12, 23, 20, 20, 20),
						schedule => schedule.S5_DailyEndTime = new ZDateTime(2022, 7, 21, 8, 30, 20));
				});
			}

			[ExpectNoExceptions]
			public void TestS5_WeekDaysOnly()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_WeekDaysOnly = true, schedule => schedule.S5_WeekDaysOnly = true);
					Test(true, schedule => schedule.S5_WeekDaysOnly = true, schedule => schedule.S5_WeekDaysOnly = false);
				});
			}

			[ExpectNoExceptions]
			public void TestS5_IsActive()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(false, schedule => schedule.S5_IsActive = true, schedule => schedule.S5_IsActive = true);
					Test(true, schedule => schedule.S5_IsActive = true, schedule => schedule.S5_IsActive = false);
				});
			}

			[ExpectNoExceptions]
			public void TestS5_StartDate()
			{
				Test(
					false,
					schedule => schedule.S5_StartDate = new ZDateTime(2019, 12, 23, 20, 20, 20),
					schedule => schedule.S5_StartDate = new ZDateTime(2019, 12, 23, 20, 20, 20));
				Test(
					true,
					schedule => schedule.S5_StartDate = new ZDateTime(2019, 12, 23, 20, 20, 20),
					schedule => schedule.S5_StartDate = new ZDateTime(2022, 7, 21, 8, 30, 20));
			}

			[ExpectNoExceptions]
			public void TestNullScheduleReturnsTrue()
			{
				// Arrange
				var runnableServiceTask = new RunnableServiceTask(
					new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
					null,
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>());
				var otherServiceTaskSchedule = Factory.New<ServiceTaskSchedule>();

				// Act
				var result = runnableServiceTask.HasScheduleUpdates(new SchedulerServiceTask(otherServiceTaskSchedule));

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(true));
			}

			public void TestWrongParamsCall()
			{
				// Arrange
				var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();
				serviceTaskSchedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });
				var runnableServiceTask = new RunnableServiceTask(
					new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
					new SchedulerServiceTask(serviceTaskSchedule),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<ITaskQueue>(MockBehavior.Strict),
					Mock.Of<IHostLogger>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>());

				// Act
				// Assert
				var result = AssertExceptionThrown<ArgumentNullException>(() => runnableServiceTask.HasScheduleUpdates(null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTask"));
			}

			[ExpectNoExceptions]
			void Test(bool expected, Action<ServiceTaskSchedule> configureOriginalSchedule, Action<ServiceTaskSchedule> configureOtherSchedule)
			{
				// Arrange
				var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();
				serviceTaskSchedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "15minutes" });
				configureOriginalSchedule(serviceTaskSchedule);
				var runnableServiceTask = new RunnableServiceTask(
					new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()),
					new SchedulerServiceTask(serviceTaskSchedule),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>());

				var otherServiceTaskSchedule = Factory.New<ServiceTaskSchedule>();
				configureOtherSchedule(otherServiceTaskSchedule);

				// Act
				var result = runnableServiceTask.HasScheduleUpdates(new SchedulerServiceTask(otherServiceTaskSchedule));

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
			}
		}
	}

	class NullUpdateStatusServiceTaskSchedule : ServiceTaskSchedule
	{
		public NullUpdateStatusServiceTaskSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool UpdateStatusCalled { get; private set; }

		public override TaskInstanceStatus UpdateStatus()
		{
			UpdateStatusCalled = true;
			return null;
		}
	}

	public class NullBranchServiceTaskSchedule : ServiceTaskSchedule
	{
		public NullBranchServiceTaskSchedule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			S5_GB = new ZGuid();
		}
	}
}
