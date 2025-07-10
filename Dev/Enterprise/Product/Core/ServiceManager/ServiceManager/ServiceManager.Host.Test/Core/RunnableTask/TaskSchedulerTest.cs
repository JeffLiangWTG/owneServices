using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;
using WTG.NUnit;
using Async = System.Threading.Tasks;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class MemoryBackup : IBackgroundDataSaver
	{
		public XDocument Document { get; set; }
		public bool DeleteFileOnDispose { get; set; }

		readonly BackgroundDataFileAction persistAction;

		public MemoryBackup(BackgroundDataFileAction persistAction)
		{
			this.persistAction = persistAction;
		}

		public void Dispose()
		{
		}

		public void Load(BackgroundDataFileAction loadAction)
		{
			using (var memoryStream = new MemoryStream())
			{
				if (Document != null)
				{
					Document.Save(memoryStream);
				}
				memoryStream.Position = 0;
				loadAction(memoryStream);
			}
		}

		public void Persist()
		{
			using (var memoryStream = new MemoryStream())
			{
				persistAction(memoryStream);
				memoryStream.Position = 0;
				Document = XDocument.Load(memoryStream);
			}
		}
	}

	public class MemoryBackupFactory : IBackgroundDataSaverFactory
	{
		public MemoryBackup Backup { get; private set; }

		public MemoryBackupFactory()
		{
		}

		public IBackgroundDataSaver Create(string persistFilePath, TimeSpan persistFrequency, BackgroundDataFileAction persistAction, Action<Exception> backgroundExceptionHandler)
		{
			Backup = new MemoryBackup(persistAction);
			return Backup;
		}
	}

	class TaskSchedulerTest : TestCaseWithFactory
	{
		ITransactionAdapter nativeTransactionAdapter;
		TaskScheduler taskScheduler;
		Mock<IHostLogger> loggerMock;
		Mock<IBackgroundDataSaverFactory> mockBackupFactory;
		Mock<IBackgroundDataSaver> mockBackup;
		Mock<IErrorReporterProxy> errorReporterProxyMock;

		protected override void SetUp()
		{
			loggerMock = new Mock<IHostLogger>();
			mockBackupFactory = new Mock<IBackgroundDataSaverFactory>();
			mockBackup = new Mock<IBackgroundDataSaver>();
			mockBackupFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>())).Returns(mockBackup.Object);
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			nativeTransactionAdapter = new NativeServiceTaskTransactionAdapter();

			attributeMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "TST" &&
				a.Description == "Desc 1" &&
				a.Category == "T1" &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d =>
					d.DoNotRunTillNextDueTimeIfOverdue == "10seconds" &&
					d.RunEvery == "1minute"));

			attributeProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(p =>
				p.GetClientHostedServiceAttribute(It.IsAny<string>()) == attributeMock);

			statusProviderMock = Mock.Of<IServiceTaskScheduleStatusProvider>();
			bindingsProviderMock = Mock.Of<IHostedServiceBusinessObjectBindingsProvider>();

			nativeTransactionAdapter = new NativeServiceTaskTransactionAdapter(
				attributeProviderMock,
				statusProviderMock,
				bindingsProviderMock,
				new ServiceManagerDateTimeProvider());
		}

		protected override void TearDown()
		{
			if (taskScheduler != null)
			{
				taskScheduler.Dispose();
			}
			nativeTransactionAdapter?.Dispose();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksEnqueuedNow()
		{
			//Arrange
			var runnerPool = new Mock<IProcessRunnerPool>();
			var taskMock1 = CreateRunnableServiceTaskMock("AAA");
			var taskMock2 = CreateRunnableServiceTaskMock("BBB");
			var taskMock3 = CreateRunnableServiceTaskMock("CCC");
			var taskMockObject1 = taskMock1.Object;
			var taskMockObject2 = taskMock2.Object;
			var taskMockObject3 = taskMock3.Object;
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { taskMock1.Object, taskMock2.Object, taskMock3.Object });
			allTasksMock.Setup(a => a.TryGetByCode("AAA", out taskMockObject1)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("BBB", out taskMockObject2)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("CCC", out taskMockObject3)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("BBB") }, true);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			NUnit.Framework.Assert.That(result.Results["BBB"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			taskMock1.Verify(x => x.Enqueue(It.IsAny<bool>()), Times.Once);
			taskMock2.Verify(x => x.Enqueue(It.IsAny<bool>()), Times.Once);
			taskMock3.Verify(x => x.Enqueue(It.IsAny<bool>()), Times.Never);
			loggerMock.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Never);
			regChecker.VerifyAll();

			Mock<IRunnableServiceTask> CreateRunnableServiceTaskMock(string code)
			{
				var taskMock = new Mock<IRunnableServiceTask>();
				taskMock
					.Setup(x => x.Code)
					.Returns(code);
				taskMock
					.Setup(x => x.IsActive)
					.Returns(true);
				taskMock
					.Setup(x => x.HasSchedule)
					.Returns(true);
				taskMock
					.Setup(x => x.Enqueue(It.IsAny<bool>()))
					.Returns(true);

				return taskMock;
			}
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksEnqueuedDelayed()
		{
			//Arrange
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			using var actionQueue = new ImmediateActionQueue();
			var task = Mock.Of<IRunnableServiceTask>(x
				=> x.Code == "AAA"
				&& x.IsActive
				&& x.HasSchedule);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }, true, delay: new TimeSpan());

			//Assert
			loggerMock.Verify(l => l.Log(LogLevel.Warning, It.IsAny<string>()), Times.Never);
			loggerMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksNotLicensed()
		{
			//Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			using var immediateActionQueue1 = new ImmediateActionQueue();
			using var immediateActionQueue2 = new ImmediateActionQueue();
			using var immediateActionQueue3 = new ImmediateActionQueue();
			var task1 = CreateTask("AAA", allowsMultiple: false, actionQueue: immediateActionQueue1, taskQueue: runnableQueue);
			var task2 = CreateTask("BBB", allowsMultiple: false, actionQueue: immediateActionQueue2, taskQueue: runnableQueue);
			var task3 = CreateTask("CCC", allowsMultiple: false, actionQueue: immediateActionQueue3, taskQueue: runnableQueue);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task1, task2, task3 });
			allTasksMock.Setup(a => a.TryGetByCode("AAA", out task1)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("BBB", out task2)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("CCC", out task3)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(false);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("BBB") }, false);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Results["AAA"].ToString(), Is.EqualTo(nameof(TaskActionOutcomeDTO.RequiresProductRegistration)));
			NUnit.Framework.Assert.That(result.Results["BBB"].ToString(), Is.EqualTo(nameof(TaskActionOutcomeDTO.RequiresProductRegistration)));
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(0), "Tasks were not enqueued");
			loggerMock.Verify(l => l.Log(LogLevel.Warning, "Failed to Nudge task [AAA] due to reason: RequiresProductRegistration"));
			loggerMock.Verify(l => l.Log(LogLevel.Warning, "Failed to Nudge task [BBB] due to reason: RequiresProductRegistration"));
			regChecker.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksEnqueuedAlready()
		{
			//Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			using var immediateActionQueue1 = new ImmediateActionQueue();
			using var immediateActionQueue2 = new ImmediateActionQueue();
			using var immediateActionQueue3 = new ImmediateActionQueue();
			var task1 = CreateTask("AAA", allowsMultiple: false, actionQueue: immediateActionQueue1, taskQueue: runnableQueue);
			var task2 = CreateTask("BBB", allowsMultiple: false, actionQueue: immediateActionQueue2, taskQueue: runnableQueue);
			var task3 = CreateTask("CCC", allowsMultiple: false, actionQueue: immediateActionQueue3, taskQueue: runnableQueue);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task1, task2, task3 });
			allTasksMock.Setup(a => a.TryGetByCode("AAA", out task1)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("BBB", out task2)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("CCC", out task3)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			runnableQueue.EnqueueTask(new DirectTaskRunRequest(task2));

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("BBB") }, false);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			NUnit.Framework.Assert.That(result.Results["BBB"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedAlready));
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(2), "Tasks were enqueued");
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot(), Has.Some.EqualTo(task1), "Tasks were enqueued");
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot(), Has.Some.EqualTo(task2), "Tasks were enqueued");
			loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<string>()), Times.Never);
			regChecker.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksUnknown()
		{
			//Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			using var immediateActionQueue1 = new ImmediateActionQueue();
			using var immediateActionQueue2 = new ImmediateActionQueue();
			using var immediateActionQueue3 = new ImmediateActionQueue();
			var task1 = CreateTask("AAA", allowsMultiple: false, actionQueue: immediateActionQueue1, taskQueue: runnableQueue);
			var task2 = CreateTask("BBB", allowsMultiple: false, actionQueue: immediateActionQueue2, taskQueue: runnableQueue);
			var task3 = CreateTask("CCC", allowsMultiple: false, actionQueue: immediateActionQueue3, taskQueue: runnableQueue);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task1, task2, task3 });
			allTasksMock.Setup(a => a.TryGetByCode("AAA", out task1)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("BBB", out task2)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("CCC", out task3)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("ZZZ") }, false);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			NUnit.Framework.Assert.That(result.Results["ZZZ"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.UnknownTask));
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(1), "Tasks were enqueued");
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot(), Has.Some.EqualTo(task1), "Tasks were enqueued");
			loggerMock.Verify(x => x.Log(LogLevel.Warning, "Failed to Nudge task [ZZZ] due to reason: UnknownTask"));
			regChecker.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksInAssemblyBindingsButNotInDb()
		{
			//Arrange
			var allTasks = Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { });
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			var hostedServiceAttributeProvider = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceAttributeProvider.Setup(hs => hs.GetClientHostedServiceAttribute(It.IsAny<string>())).Returns(new HostedServiceAttribute());
			hostedServiceAttributeProvider.Setup(hs => hs.GetClientHostedServiceAttributes()).Returns(new List<HostedServiceAttribute> { new HostedServiceAttribute() });
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasks, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), hostedServiceAttributeProvider.Object);

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("DSA") }, false);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.Results["DSA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.TaskDisabledOrInactive));
			loggerMock.Verify(x => x.Log(LogLevel.Debug, "Failed to Nudge task [DSA] due to reason: TaskDisabledOrInactive"));
			loggerMock.Verify(x => x.Log(LogLevel.Debug, "Remotely enqueued tasks: "), Times.Never());
			regChecker.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestScheduleTasksDisabledOrInactive()
		{
			//Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			using var immediateActionQueue2 = new ImmediateActionQueue();
			using var immediateActionQueue3 = new ImmediateActionQueue();
			var task1 = CreateTaskWithNoSchedule("AAA");
			var task2 = CreateTask("BBB", allowsMultiple: false, actionQueue: immediateActionQueue2, taskQueue: runnableQueue);
			var task3 = CreateTask("CCC", allowsMultiple: false, actionQueue: immediateActionQueue3, taskQueue: runnableQueue);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task1, task2, task3 });
			allTasksMock.Setup(a => a.TryGetByCode("AAA", out task1)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("BBB", out task2)).Returns(true);
			allTasksMock.Setup(a => a.TryGetByCode("CCC", out task3)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			//Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("BBB") }, false);

			//Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(2));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.TaskDisabledOrInactive));
			NUnit.Framework.Assert.That(result.Results["BBB"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(1), "Tasks were enqueued");
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot(), Has.Some.EqualTo(task2), "Tasks were enqueued");
			loggerMock.Verify(x => x.Log(LogLevel.Debug, "Failed to Nudge task [AAA] due to reason: TaskDisabledOrInactive"));
			regChecker.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestManualScheduleNowIsLoggedIfEnqueued()
		{
			// Arrange
			var hostLoggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			var loggerFactoryMock = new Mock<ILoggerFactory>();
			var serviceTaskLoggerMock = new Mock<Integration.ILogger>();
			loggerFactoryMock
				.Setup(lf => lf.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(serviceTaskLoggerMock.Object);
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = CreateTask("AAA", actionQueue: immediateActionQueue, taskQueue: runnableQueue, logger: hostLoggerMock.Object, loggerFactory: loggerFactoryMock.Object);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, hostLoggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());
			var userCode = "TU1";

			// Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }, false, userCode: userCode);

			// Assert
			NUnit.Framework.Assert.That(result, Is.Not.Null);
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));
			hostLoggerMock.Verify(l => l.Log(LogLevel.Information, It.Is<string>(s => s.Contains(userCode))), Times.Never);
			serviceTaskLoggerMock.Verify(l => l.Log(LogType.Information, It.Is<string>(s => s.Contains(userCode))), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestManualScheduleNowIsNotLoggedIfNotEnqueued()
		{
			// Arrange
			var hostLoggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
			var loggerFactoryMock = new Mock<ILoggerFactory>();
			var serviceTaskLoggerMock = new Mock<Integration.ILogger>();
			loggerFactoryMock
				.Setup(lf => lf.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(serviceTaskLoggerMock.Object);
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = CreateTask("AAA", actionQueue: immediateActionQueue, taskQueue: runnableQueue, logger: hostLoggerMock.Object, loggerFactory: loggerFactoryMock.Object);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, hostLoggerMock.Object, regChecker.Object, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());
			var userCode = "TU1";
			runnableQueue.EnqueueTask(new DirectTaskRunRequest(task));

			// Act
			var result = taskScheduler.ScheduleTasks(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }, false, userCode: userCode);

			// Assert
			NUnit.Framework.Assert.That(result, Is.Not.Null);
			NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
			NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedAlready));
			hostLoggerMock.Verify(l => l.Log(LogLevel.Information, It.Is<string>(s => s.Contains(userCode))), Times.Never);
			serviceTaskLoggerMock.Verify(l => l.Log(LogType.Information, It.Is<string>(s => s.Contains(userCode))), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestRequestReloadOfTaskConfiguration()
		{
			//Arrange
			attributeMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "AAA" &&
				a.Description == "Desc 1" &&
				a.Category == "T1" &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "1minute"));

			var (scheduleAAA, governor) = CreateNativeSchedule(attributeMock, nativeTransactionAdapter);
			using var controller = new ControllerForTest(service: null);
			var taskAAA = CreateRunnableTask(scheduleAAA, allowsMultiple: false, actionQueue: controller.ActionQueue, transactionAdapter: nativeTransactionAdapter);
			nativeTransactionAdapter.Commit();

			using var differentTransactionAdapter = new NativeServiceTaskTransactionAdapter(
				attributeProviderMock,
				statusProviderMock,
				bindingsProviderMock,
				new ServiceManagerDateTimeProvider());
			var governorFromOtherFactory = differentTransactionAdapter.GetServiceTaskGovernor(scheduleAAA.Pk);
			var allTasks = Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA });
			var loggerMock = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(false);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasks, loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());
			NUnit.Framework.Assert.That(taskAAA.IsActive, Is.EqualTo(true), "Task was not active by default");
			governorFromOtherFactory.SetActive(false);
			differentTransactionAdapter.Commit();
			NUnit.Framework.Assert.That(taskAAA.IsActive, Is.EqualTo(true), "Task was not active after modification by different factory");

			//Act
			var task = Async.Task.Run(() => taskScheduler.RequestReloadOfTaskConfiguration(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }));
			task.Wait();
			var assertionsAction = new Action(() =>
			{
				NUnit.Framework.Assert.That(taskAAA.IsActive, Is.EqualTo(false), "Task was not inactive after reload");
				NUnit.Framework.Assert.That(task.Result, Is.Not.EqualTo(default(TasksActionResultDTO)));
				NUnit.Framework.Assert.That(task.Result.Results.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(task.Result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.Succeeded));
			});
			controller.RunTaskDispatchingLoopExposed(assertionsAction);
		}

		[TestDate(2016, 05, 26, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestSetNextRuntimeFromAnotherThread()
		{
			AssertSetNextRuntimeFromAnotherThread(revertingAfterFailedRun: false);
		}

		[TestDate(2016, 05, 26, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestSetNextRuntimeRevertingAfterFailedRun()
		{
			AssertSetNextRuntimeFromAnotherThread(revertingAfterFailedRun: true);
		}

		void AssertSetNextRuntimeFromAnotherThread(bool revertingAfterFailedRun)
		{
			string prevAppName = DbConnection.ApplicationName;
			DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
			try
			{
				//Arrange
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
				var nextRunTime = ZDateTime.UtcNow;
				var scheduleAAA = CreateSchedule("AAA");
				scheduleAAA.S5_NextScheduledPrintRunTimeUtc = nextRunTime;
				var scheduleBBB = CreateSchedule("BBB");
				scheduleBBB.S5_NextScheduledPrintRunTimeUtc = nextRunTime;
				using var controller = new ControllerForTest(service: null);
				var taskAAA = CreateTask(new SchedulerServiceTask(scheduleAAA), allowsMultiple: false, actionQueue: controller.ActionQueue, transactionAdapter: transactionAdapter);
				var taskBBB = CreateTask(new SchedulerServiceTask(scheduleBBB), allowsMultiple: false, actionQueue: controller.ActionQueue, transactionAdapter: transactionAdapter);
				var allTasks = Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA, taskBBB });
				var loggerMock = new Mock<IHostLogger>();
				var runnerPool = new Mock<IProcessRunnerPool>();
				taskScheduler = new TaskScheduler(transactionAdapter, allTasks, loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				var task = Async.Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						return taskScheduler.SetNextRuntime(new List<TaskCodeDTO> { new TaskCodeDTO("BBB"), new TaskCodeDTO("CCC") }, nextRunTime.AddHours(5).ToNullableDateTime(), revertingAfterFailedRun);
					}
				});
				task.Wait();

				controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
				controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(false);
				var assertionsAction = new Action(() =>
				{
					NUnit.Framework.Assert.That(task.Result, Is.Not.EqualTo(default(TasksActionResultDTO)));
					NUnit.Framework.Assert.That(task.Result.Results.Count, Is.EqualTo(2));
					NUnit.Framework.Assert.That(task.Result.Results["BBB"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.Succeeded));
					NUnit.Framework.Assert.That(task.Result.Results["CCC"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.UnknownTask));
					NUnit.Framework.Assert.That(taskAAA.NextScheduledRunTime, Is.EqualTo(nextRunTime.ToNullableDateTimeOffset()));
					NUnit.Framework.Assert.That(taskBBB.NextScheduledRunTime, Is.EqualTo(nextRunTime.AddHours(5).ToNullableDateTimeOffset()));
					NUnit.Framework.Assert.That(taskBBB.NextRunTimeAllowingForLocalSchedulingFailures, Is.EqualTo(revertingAfterFailedRun ? ZDateTime.UtcNow.Add(TaskScheduler.FailedScheduleRetryTime).ToNullableDateTimeOffset() : taskBBB.NextScheduledRunTime));
				});
				controller.RunTaskDispatchingLoopExposed(assertionsAction);
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		[ExpectNoExceptions]
		public void TestSetNextRuntimeOnTaskWithNoSchedule()
		{
			string prevAppName = DbConnection.ApplicationName;
			DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
			try
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				var result = taskScheduler.SetNextRuntime(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }, ZDateTimeOffset.UtcNow.ToDateTimeOffset(), revertingAfterFailedRun: false);

				//Assert
				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
				NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.TaskDisabledOrInactive));
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		class DuplicateTaskCodesTest : TaskSchedulerTest
		{
			[ExpectNoExceptions]
			public void TestRequestReloadOfTaskConfigurationOnTaskWithDuplicateTaskCodePassedInShouldNotThrowException()
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				var result = taskScheduler.RequestReloadOfTaskConfiguration(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("AAA") });

				//Assert
				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
				NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.TaskDisabledOrInactive));
			}

			public void TestRequestReloadOfTaskConfigurationOnTaskWithDuplicateTaskCodeInAllTasksShouldThrowException()
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var taskAAADuplicate = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA, taskAAADuplicate }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				AssertExceptionThrown<ArgumentException>(() => taskScheduler.RequestReloadOfTaskConfiguration(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }));
			}

			[ExpectNoExceptions]
			public void TestSetNextRuntimeOnTaskWithDuplicateTaskCodePassedInShouldNotThrowException()
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				var result = taskScheduler.SetNextRuntime(new List<TaskCodeDTO> { new TaskCodeDTO("AAA"), new TaskCodeDTO("AAA") }, ZDateTimeOffset.UtcNow.ToDateTimeOffset(), revertingAfterFailedRun: false);

				//Assert
				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
				NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(result.Results["AAA"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.TaskDisabledOrInactive));
			}

			public void TestSetNextRuntimeOnTaskWithDuplicateTaskCodeInAllTasksShouldThrowException()
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var taskAAADuplicate = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA, taskAAADuplicate }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				AssertExceptionThrown<ArgumentException>(() => taskScheduler.SetNextRuntime(new List<TaskCodeDTO> { new TaskCodeDTO("AAA") }, ZDateTimeOffset.UtcNow.ToDateTimeOffset(), revertingAfterFailedRun: false));
			}

			[ExpectNoExceptions]
			public void TestSetNextRuntimeOnTaskWithDuplicateUnknownTaskCodePassedInShouldNotThrowException()
			{
				//Arrange
				var taskAAA = new RunnableServiceTask(CreateTaskInfo("AAA"), null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				var loggerMock = new Mock<IHostLogger>();
				var runnableQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, loggerMock.Object);
				taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask> { taskAAA }), loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

				//Act
				var result = taskScheduler.SetNextRuntime(new List<TaskCodeDTO> { new TaskCodeDTO("BBB"), new TaskCodeDTO("BBB") }, ZDateTimeOffset.UtcNow.ToDateTimeOffset(), revertingAfterFailedRun: false);

				//Assert
				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(TasksActionResultDTO)));
				NUnit.Framework.Assert.That(result.Results.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(result.Results["BBB"].Outcome, Is.EqualTo(TaskActionOutcomeDTO.UnknownTask));
			}
		}

		[ExpectNoExceptions]
		public void TestBackFileIsDisposed()
		{
			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, null, loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{ }
			mockBackup.Verify(b => b.Dispose(), Times.AtLeastOnce);
		}

		[ExpectNoExceptions]
		public void TestBackFileIsDeletedIfScheduleWasSaved()
		{
			//using var transactionAdapter = new NativeServiceTaskTransactionAdapter();
			var (schedule, governor) = CreateNativeSchedule(attributeMock, nativeTransactionAdapter);
			var task = CreateRunnableTask(schedule, true, transactionAdapter: nativeTransactionAdapter);
			var updatedNextRunTime = DateTimeOffset.UtcNow.AddMinutes(5);
			updatedNextRunTime = updatedNextRunTime.AddTicks(-(updatedNextRunTime.Ticks % TimeSpan.TicksPerSecond));
			task.SetNextRunTime(updatedNextRunTime, SetNextRuntimeReason.ScheduledToRun);

			using var tempDir = new TempDirectory();
			var saveFilePath = Path.Combine(tempDir.DirectoryName, "TaskSchedulerTest.txt");
			using (var scheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, null, saveFilePath, TimeSpan.FromSeconds(15), errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				scheduler.Save();
			}

			NUnit.Framework.Assert.That(!File.Exists(saveFilePath), Is.True);
			var newFactory = new BusinessObjectFactory();
			var reloadedSchedule = newFactory.Load<StmServiceTask>(schedule.Pk);
			NUnit.Framework.Assert.That(reloadedSchedule.SST_NextRunTime.ToDateTimeOffset, Is.EqualTo(updatedNextRunTime));
		}

		[ExpectNoExceptions]
		public void TestBackFileIsNotDeletedIfScheduleWasNotSaved()
		{
			var schedule = CreateSchedule("TST");
			var task = CreateTask(new SchedulerServiceTask(schedule), true);

			using (var tempDir = new TempDirectory())
			{
				var saveFilePath = Path.Combine(tempDir.DirectoryName, "TaskSchedulerTest.txt");
				using (var scheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, null, saveFilePath, TimeSpan.FromSeconds(15), errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
				{
				}

				NUnit.Framework.Assert.That(File.Exists(saveFilePath), Is.True);
			}
		}

		[ExpectNoExceptions]
		public void TestBackgroundSaveExceptionIsHandled()
		{
			var task = new Mock<IRunnableServiceTask>();
			var exception = new InvalidOperationException();
			task.Setup(t => t.Code).Throws(exception);
			task.Setup(t => t.HasSchedule).Returns(true);

			var isCalled = false;

			errorReporterProxyMock
				.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback(() => isCalled = true);

			using (var tempDir = new TempDirectory())
			{
				var saveFilePath = Path.Combine(tempDir.DirectoryName, "TaskSchedulerTest.txt");
				using (var scheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task.Object }), loggerMock.Object, null, null, saveFilePath, TimeSpan.FromMilliseconds(100), errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
				{
					scheduler.Save();

					for (int i = 0; i < 50 && !isCalled; i++)
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(100));
					}
					errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), exception), Times.AtLeastOnce);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFileCorrectDefaults()
		{
			var expectedFilePath = Path.Combine(ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName), TaskScheduler.Constants.PersistenceFileName);
			mockBackupFactory.Setup(f => f.Create(expectedFilePath, TimeSpan.FromMinutes(5), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>())).Returns(new Mock<IBackgroundDataSaver>().Object);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, null, loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());
			mockBackupFactory.Verify(f => f.Create(expectedFilePath, TimeSpan.FromMinutes(5), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>()), Times.AtLeastOnce);
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestBackupFileIsLoadedDuringInitialise_NewerRuntime()
		{
			BackupFileIsLoadedDuringInitialise(ZDateTime.UtcNow, ZDateTime.UtcNow.AddHours(5));
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestBackupFileIsLoadedDuringInitialise_OlderRuntime()
		{
			BackupFileIsLoadedDuringInitialise(ZDateTime.UtcNow.AddHours(5), ZDateTime.UtcNow);
		}

		void BackupFileIsLoadedDuringInitialise(ZDateTime originalTime, ZDateTime newTime)
		{
			var memoryBackupFactory = new MemoryBackupFactory();
			var schedule = CreateSchedule("TST");
			schedule.S5_NextScheduledPrintRunTimeUtc = originalTime;
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: true, actionQueue: immediateActionQueue);
			var aspectVersions = CreateAspectVersions();
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, aspectVersions, Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var taskCodeAttr = new XAttribute(TaskScheduler.Constants.TaskCodeAttribute, "TST");
				var taskNextRunAttr = new XAttribute(TaskScheduler.Constants.TaskNextRunTimeAttribute, newTime.ToString(TaskScheduler.Constants.XMLDateTimeFormat));
				var taskNode = new XElement(TaskScheduler.Constants.TaskNode, taskCodeAttr, taskNextRunAttr);
				var allTasksAttr = new XAttribute("version", TaskScheduler.Constants.GetLatestXMLFileVersion(aspectVersions));
				var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode, allTasksAttr, taskNode);
				memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
				mytaskScheduler.InitialiseTasks();
				NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo((newTime > originalTime) ? newTime.ToNullableDateTimeOffset() : originalTime.ToNullableDateTimeOffset()));
			}
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestBackupFileDateTimeKindIsUtc()
		{
			var originalTime = ZDateTime.UtcNow;
			var newTime = ZDateTime.UtcNow.AddHours(5);
			var memoryBackupFactory = new MemoryBackupFactory();
			var schedule = CreateSchedule("TST");
			var aspectVersions = CreateAspectVersions();
			schedule.S5_NextScheduledPrintRunTimeUtc = originalTime;
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: true, actionQueue: immediateActionQueue);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new List<IRunnableServiceTask>() { task });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, aspectVersions, Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var taskCodeAttr = new XAttribute(TaskScheduler.Constants.TaskCodeAttribute, "TST");
				var taskNextRunAttr = new XAttribute(TaskScheduler.Constants.TaskNextRunTimeAttribute, newTime.ToString(TaskScheduler.Constants.XMLDateTimeFormat));
				var taskNode = new XElement(TaskScheduler.Constants.TaskNode, taskCodeAttr, taskNextRunAttr);
				var allTasksAttr = new XAttribute("version", TaskScheduler.Constants.GetLatestXMLFileVersion(aspectVersions));
				var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode, allTasksAttr, taskNode);
				memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
				mytaskScheduler.InitialiseTasks();
				NUnit.Framework.Assert.That(task.NextRunTime, Is.EqualTo(newTime.ToNullableDateTimeOffset()));
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFileIsLoadedDuringInitialise_DoesNotUpdateDeletedTasks()
		{
			var memoryBackupFactory = new MemoryBackupFactory();
			var schedule = CreateSchedule("TST");
			var originalTime = ZDateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = originalTime;
			var task = CreateTask(new SchedulerServiceTask(schedule), true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>()), loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var taskCodeAttr = new XAttribute(TaskScheduler.Constants.TaskCodeAttribute, "TST");
				var taskNextRunAttr = new XAttribute(TaskScheduler.Constants.TaskNextRunTimeAttribute, originalTime.AddHours(5).ToString(TaskScheduler.Constants.XMLDateTimeFormat));
				var taskNode = new XElement(TaskScheduler.Constants.TaskNode, taskCodeAttr, taskNextRunAttr);
				var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode, taskNode);
				memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
				mytaskScheduler.InitialiseTasks();
				NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(originalTime.ToNullableDateTimeOffset()));
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFileWithMissingTaskIsLoadedDuringInitialise()
		{
			var memoryBackupFactory = new MemoryBackupFactory();
			var task = CreateTask("TST", true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode);
				memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
				mytaskScheduler.InitialiseTasks();
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFileIsEmptyOnInitialise()
		{
			var task = CreateTask("TST", true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, new MemoryBackupFactory(), errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				mytaskScheduler.InitialiseTasks();
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFileIsMissingAllTasksNodeOnInitialise()
		{
			var memoryBackupFactory = new MemoryBackupFactory();
			var task = CreateTask("TST", true);

			using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode));
				mytaskScheduler.InitialiseTasks();
			}
		}

		[ExpectNoExceptions]
		public void TestBackupFilePersist()
		{
			var memoryBackupFactory = new MemoryBackupFactory();
			var schedule = CreateSchedule("TST");
			var originalTime = ZDateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = originalTime;
			var task = CreateTask(new SchedulerServiceTask(schedule), true);
			taskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			memoryBackupFactory.Backup.Persist();

			var allTasksNode = memoryBackupFactory.Backup.Document.Root.Elements().First((e) => e.Name == TaskScheduler.Constants.AllTasksNode);
			NUnit.Framework.Assert.That(allTasksNode.Elements().Count(), Is.EqualTo(1));
			var taskNode = allTasksNode.Elements().First();
			var taskCode = taskNode.Attributes().First((a) => a.Name == TaskScheduler.Constants.TaskCodeAttribute)?.Value;
			NUnit.Framework.Assert.That(taskCode, Is.EqualTo("TST"));
			var nextRunTimeString = taskNode.Attributes().FirstOrDefault((a) => a.Name == TaskScheduler.Constants.TaskNextRunTimeAttribute)?.Value;
			NUnit.Framework.Assert.That(nextRunTimeString, Is.EqualTo(originalTime.ToString(TaskScheduler.Constants.XMLDateTimeFormat)));
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestDbVersionIsDifferentThanXmlVersion()
		{
			foreach (var versionOffset in new[] { 10, -10 })
			{
				Test(versionOffset);
			}

			void Test(int dbVersionOffset)
			{
				// Arrange
				var originalTime = ZDateTime.UtcNow.AddDays(-5);
				var newTime = ZDateTime.UtcNow;
				var memoryBackupFactory = new MemoryBackupFactory();
				var schedule = CreateSchedule("TST");

				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + dbVersionOffset;
				var bumpedTransformationVersion = ObjectFactory.Get<IDatabaseAspectVersions>().TransformationVersion.Major + dbVersionOffset;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));
				versionMock
					.Setup(x => x.TransformationVersion)
					.Returns(new VersionLabel(bumpedTransformationVersion, 0));

				schedule.S5_NextScheduledPrintRunTimeUtc = originalTime;
				using var immediateActionQueue = new ImmediateActionQueue();
				var task = CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: true, actionQueue: immediateActionQueue);
				using (var mytaskScheduler = new TaskScheduler(nativeTransactionAdapter, Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>() { task }), loggerMock.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
				{
					//Act
					memoryBackupFactory.Backup.Persist();
					task.SetNextRunTime(newTime.ToNullableDateTimeOffset(), SetNextRuntimeReason.LoadingInitialValue);

					using (ObjectFactory.Substitute(versionMock.Object))
					{
						mytaskScheduler.InitialiseTasks();
						//Assert
						NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(newTime.ToNullableDateTimeOffset()), $"Task should still have new run time when XML file version is different to DB by {dbVersionOffset} amount");
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestSetNextRunTimeBasedOnRecurrence_SetsNextRuntime()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var schedule1 = CreateSchedule("AAA");
			using var immediateActionQueue = new ImmediateActionQueue();
			var task1 = CreateTask(new SchedulerServiceTask(schedule1), allowsMultiple: false, actionQueue: immediateActionQueue, transactionAdapter: transactionAdapter);
			var reference = DateTime.UtcNow;
			schedule1.S5_NextScheduledPrintRunTimeUtc = reference;
			Factory.Save();

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
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var schedule = CreateSchedule("AAA");
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: false, actionQueue: immediateActionQueue, errorReporterProxy: errorReporterProxyMock.Object, transactionAdapter: transactionAdapter);
			schedule.Recurrence.WeeklyRange = true;
			schedule.Recurrence.DayList = "NNNNNNN";
			var reference = ZDateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = reference;

			// Act
			task.SetNextRunTimeBasedOnRecurrence();

			// Assert
			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.Is<string>(msg => msg.StartsWith("SetNextRunTimeBasedOnRecurrence: ")), It.IsAny<Exception>()),
				Times.Once);
			NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(reference.AddMinutes(1).ToNullableDateTimeOffset()));

			schedule.Recurrence.DayList = "YNNNNNN"; // Avoid exception when calling Factory.Save() in Dispose method of taskSchedule
		}

		[ExpectNoExceptions]
		public void TestConstructRequest()
		{
			// Arrange
			const string taskCode = "AAA";
			var taskMock = Mock.Of<IRunnableServiceTask>(x => x.Code == taskCode);
			var allTasksMock = new Mock<IAllTasksConsumer>();
			allTasksMock.Setup(a => a.GetAll()).Returns(new[] { taskMock });
			allTasksMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out taskMock)).Returns(true);

			taskScheduler = new TaskScheduler(nativeTransactionAdapter, allTasksMock.Object, loggerMock.Object, null, mockBackupFactory.Object, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			// Act
			var request = taskScheduler.ReconstructRequest(x => Mock.Of<ITaskRunRequest>(y => y.Task == x), taskCode);

			// Assert
			NUnit.Framework.Assert.That(taskMock, Is.EqualTo(request.Task), "Task should be the same");
		}

		IRunnableServiceTask CreateTask(string code, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null)
		{
			return CreateTask(code, Factory, allowsMultiple, actionQueue, taskQueue, logger ?? loggerMock.Object, transactionAdapter: transactionAdapter, loggerFactory: loggerFactory);
		}

		public static IRunnableServiceTask CreateTask(string code, BusinessObjectFactory factory, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null)
		{
			return CreateTask(new SchedulerServiceTask(CreateSchedule(code, factory)), allowsMultiple, actionQueue, taskQueue, logger, transactionAdapter: transactionAdapter, loggerFactory: loggerFactory);
		}

		public static IRunnableServiceTask CreateTask(IServiceTask schedule, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, IErrorReporterProxy errorReporterProxy = null, IHostRegistrySettings hostRegistry = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null)
		{
			var taskInfo = CreateTaskInfo(schedule.Code, allowsMultiple);
			IRunnableServiceTask task = new RunnableServiceTask(taskInfo, schedule, actionQueue ?? new Mock<IBackgroundThreadActionQueue>().Object, taskQueue ?? Mock.Of<ITaskQueue>(), logger ?? Mock.Of<IHostLogger>(), errorReporterProxy ?? Mock.Of<IErrorReporterProxy>(), hostRegistry ?? Mock.Of<IHostRegistrySettings>(), transactionAdapter ?? Mock.Of<ITransactionAdapter>(), loggerFactory ?? Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			return task;
		}

		public static Mock<IRunnableServiceTask> CreateMockTask(IServiceTask schedule, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, IErrorReporterProxy errorReporterProxy = null, IHostRegistrySettings hostRegistry = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null, IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider = null)
		{
			var taskInfo = CreateTaskInfo(schedule.Code, allowsMultiple);
			var mockTask = new Mock<RunnableServiceTask>(
				taskInfo, 
				schedule, 
				actionQueue ?? new Mock<IBackgroundThreadActionQueue>().Object, 
				taskQueue ?? Mock.Of<ITaskQueue>(), 
				logger ?? Mock.Of<IHostLogger>(), 
				errorReporterProxy ?? Mock.Of<IErrorReporterProxy>(), 
				hostRegistry ?? Mock.Of<IHostRegistrySettings>(), 
				transactionAdapter ?? Mock.Of<ITransactionAdapter>(),
				loggerFactory ?? Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleStatusProvider ?? Mock.Of<IServiceTaskScheduleStatusProvider>())
			{
				CallBase = true,
			};
			return mockTask.As<IRunnableServiceTask>();
		}

		public static ServiceTaskSchedule CreateSchedule(string code, BusinessObjectFactory factory, bool active = true, string taskPeriod = "S", int taskPeriodCount = 10, Mock<IHostedServiceAttribute> config = null)
		{
			var schedule = factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = code;
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			schedule.S5_IsActive = active;
			schedule.S5_TaskPeriod = taskPeriod;
			schedule.S5_TaskPeriodCount = taskPeriodCount;
			schedule.S5_TypeOfDocument = "T1";

			if (config is not null)
			{
				config
					.SetupGet(c => c.DefaultSchedule.RunEvery)
					.Returns(CalculateDefaultScheduleRunEvery(taskPeriod, taskPeriodCount));
				config
					.SetupGet(c => c.DefaultSchedule.DayOfMonth)
					.Returns(taskPeriod == ScheduleRecurrenceType.Monthly ? 1 : 0);
				config
					.SetupGet(c => c.DefaultSchedule.DaysOfWeek)
					.Returns(taskPeriod == ScheduleRecurrenceType.Weekly ? new[] { DayOfWeek.Saturday } : null);
			}

			schedule.SetStaticServiceAttributesDebugOnly(config?.Object ??
				new HostedServiceAttribute()
				{
					DefaultScheduleRunEvery = CalculateDefaultScheduleRunEvery(taskPeriod, taskPeriodCount),
					DefaultScheduleDayOfMonth = taskPeriod == ScheduleRecurrenceType.Monthly ? 1 : 0,
					DefaultScheduleDaysOfWeek = taskPeriod == ScheduleRecurrenceType.Weekly ? new[] { DayOfWeek.Saturday } : null,
				});

			return schedule;
		}

		ServiceTaskSchedule CreateSchedule(string code) => CreateSchedule(code, Factory);

		public static IRunnableServiceTask CreateTaskWithNoSchedule(string code, bool allowsMultiple = false)
		{
			var taskInfo = CreateTaskInfo(code, allowsMultiple);
			IRunnableServiceTask task = new RunnableServiceTask(taskInfo, null, new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			return task;
		}

		public static ServiceTaskInfo CreateTaskInfo(string code, bool allowsMultiple = false)
		{
			var config = new HostedServiceAttribute() { TypeName = "foo", Code = code, TypeAssemblyName = "foo.dll", AllowsMultipleInstances = allowsMultiple, DefaultScheduleRunEvery = "1day" };
			return new ServiceTaskInfo(config);
		}

		IRunnableServiceTask CreateRunnableTask(IServiceTask schedule, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, IErrorReporterProxy errorReporterProxy = null, IHostRegistrySettings hostRegistry = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null)
		{
			var taskInfo = new ServiceTaskInfo(attributeMock);
			IRunnableServiceTask task = new RunnableServiceTask(taskInfo, schedule, actionQueue ?? new Mock<IBackgroundThreadActionQueue>().Object, taskQueue ?? Mock.Of<ITaskQueue>(), logger ?? Mock.Of<IHostLogger>(), errorReporterProxy ?? Mock.Of<IErrorReporterProxy>(), hostRegistry ?? Mock.Of<IHostRegistrySettings>(), transactionAdapter ?? Mock.Of<ITransactionAdapter>(), loggerFactory ?? Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
			return task;
		}

		(IServiceTask, IServiceTaskGovernor) CreateNativeSchedule(IHostedServiceAttribute serviceTaskAttribute, ITransactionAdapter transactionAdapter)
		{
			var governor = transactionAdapter.GetNewServiceTaskGovernor(serviceTaskAttribute);

			governor.SetSchedule(10, "S");
			governor.SetActive(true);
			governor.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset());
			transactionAdapter.Commit();

			return (governor.GovernedTask, governor);
		}

		static string CalculateDefaultScheduleRunEvery(string taskPeriod, int taskPeriodCount)
		{
			switch (taskPeriod)
			{
				case ScheduleRecurrenceType.Second:
					return $"{taskPeriodCount}seconds";

				case ScheduleRecurrenceType.Minute:
					return $"{taskPeriodCount}minutes";

				case ScheduleRecurrenceType.Hourly:
					return $"{taskPeriodCount}hours";

				case ScheduleRecurrenceType.Daily:
					return $"{taskPeriodCount}days";

				case ScheduleRecurrenceType.Weekly:
					return $"{taskPeriodCount}weeks";

				case ScheduleRecurrenceType.Monthly:
					return $"{taskPeriodCount}months";

				case ScheduleRecurrenceType.Yearly:
					return $"{taskPeriodCount}years";

				default:
					return "15minutes";
			}
		}

		static IDatabaseAspectVersions CreateAspectVersions()
		{
			var aspectVersions = new Mock<IDatabaseAspectVersions>();
			aspectVersions.Setup(x => x.SchemaVersion).Returns(new VersionLabel(1, 0));
			aspectVersions.Setup(x => x.TransformationVersion).Returns(new VersionLabel(1, 0));
			aspectVersions.Setup(x => x.ScriptVersion).Returns(new VersionLabel(1, 0));
			return aspectVersions.Object;
		}

		IHostedServiceAttribute attributeMock;
		IClientHostedServiceAttributeProvider attributeProviderMock;
		IServiceTaskScheduleStatusProvider statusProviderMock;
		IHostedServiceBusinessObjectBindingsProvider bindingsProviderMock;
	}
}
