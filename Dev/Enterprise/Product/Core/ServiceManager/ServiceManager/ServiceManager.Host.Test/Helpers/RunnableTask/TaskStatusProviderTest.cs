using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class TaskStatusProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
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
		}

		[ExpectNoExceptions]
		public void TestGetTasksStatus()
		{
			var repo = new MockRepository(MockBehavior.Default);
			var taskMock = repo.Create<IRunnableServiceTask>();
			var taskMockObject = taskMock.Object;
			taskMock.Setup(tm => tm.Code).Returns("TST");
			var taskQueue = repo.Create<ITaskQueue>();
			taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			var runnerPool = repo.Create<IProcessRunnerPool>();
			runnerPool.Setup(rp => rp.GetRunnersSnapshot()).Returns(new List<ServiceTaskCodeWithRunnerProcessId>());
			var regChecker = repo.Create<IProductRegistrationPeriodicChecker>();
			var allTasks = repo.Create<IAllTasksConsumer>();
			allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[1] { taskMockObject });
			allTasks.Setup(a => a.TryGetByCode("TST", out taskMockObject)).Returns(true);
			var bindings = new List<HostedServiceBusinessObjectBindingAttribute>();
			var provider = new TaskStatusProvider(allTasks.Object, taskQueue.Object, runnerPool.Object, regChecker.Object, bindings);

			var status = provider.GetTaskStatus("TST");
			NUnit.Framework.Assert.That(status, Is.Not.EqualTo(default(ServiceTaskStatus)));
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo("TST"));

			NUnit.Framework.Assert.That(provider.GetTaskStatus("BLA"), Is.EqualTo(default(ServiceTaskStatus)));
		}

		[ExpectNoExceptions]
		public void TestGetTaskStatusFromBackgroundThread()
		{
			var repo = new MockRepository(MockBehavior.Default);
			var (schedule, governor) = CreateSchedule("TST");
			var task = CreateRunnableTask(schedule);
			var taskQueue = repo.Create<ITaskQueue>();
			taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			var runnerPool = repo.Create<IProcessRunnerPool>();
			runnerPool.Setup(rp => rp.GetRunnersSnapshot()).Returns(new List<ServiceTaskCodeWithRunnerProcessId>());
			var regChecker = repo.Create<IProductRegistrationPeriodicChecker>();
			var allTasks = repo.Create<IAllTasksConsumer>();
			allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[1] { task });
			allTasks.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task)).Returns(true);
			var bindings = new List<HostedServiceBusinessObjectBindingAttribute>();
			var provider = new TaskStatusProvider(allTasks.Object, taskQueue.Object, runnerPool.Object, regChecker.Object, bindings);

			var stopWatch = new Stopwatch();
			var timeout = TimeSpan.FromSeconds(1);
			var originalNextRunTime = schedule.NextRunTime;
			using var taskEvent = new AutoResetEvent(false);

			var getStatusTask = Task.Run(() =>
			{
				stopWatch.Restart();
				taskEvent.Set();
				while (stopWatch.Elapsed < timeout)
				{
					var status = provider.GetTaskStatus("TST");
					NUnit.Framework.Assert.That(status, Is.Not.EqualTo(default(ServiceTaskStatus)));
					NUnit.Framework.Assert.That(status.Code, Is.EqualTo("TST"));
					NUnit.Framework.Assert.That(status.Description, Is.EqualTo("Desc 1"));
					NUnit.Framework.Assert.That(status.Category, Is.EqualTo("T1"));
					NUnit.Framework.Assert.That(status.NextRunTime, Is.GreaterThanOrEqualTo(originalNextRunTime.DateTime));
					NUnit.Framework.Assert.That(status.SchedulePeriod, Is.EqualTo(TimeSpan.FromSeconds(10)));
				}
			});

			taskEvent.WaitOne();
			while (!getStatusTask.IsCompleted && stopWatch.Elapsed < timeout)
			{
				governor.SetNextRunTime(schedule.NextRunTime + timeout);
				task.UpdateSchedule(new[] { governor.GovernedTask }, false);
			}

			getStatusTask.Wait();

			IRunnableServiceTask CreateRunnableTask(IServiceTask schedule, bool allowsMultiple = false, IBackgroundThreadActionQueue actionQueue = null, ITaskQueue taskQueue = null, IHostLogger logger = null, IErrorReporterProxy errorReporterProxy = null, IHostRegistrySettings hostRegistry = null, ITransactionAdapter transactionAdapter = null, ILoggerFactory loggerFactory = null)
			{
				var taskInfo = new ServiceTaskInfo(attributeMock);
				IRunnableServiceTask task = new RunnableServiceTask(taskInfo, schedule, actionQueue ?? new Mock<IBackgroundThreadActionQueue>().Object, taskQueue ?? Mock.Of<ITaskQueue>(), logger ?? Mock.Of<IHostLogger>(), errorReporterProxy ?? Mock.Of<IErrorReporterProxy>(), hostRegistry ?? Mock.Of<IHostRegistrySettings>(), transactionAdapter ?? Mock.Of<ITransactionAdapter>(), loggerFactory ?? Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());
				return task;
			}

			(IServiceTask, IServiceTaskGovernor) CreateSchedule(string code)
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

				governor.SetSchedule(10, "S");
				governor.SetActive(true);
				governor.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset());
				governor.Save();

				return (governor.GovernedTask, governor);
			}
		}

		[ExpectNoExceptions]
		public void TestGetTaskStatusConsistencyOfRuntimeStatus()
		{
			var repo = new MockRepository(MockBehavior.Default);
			var taskMock = repo.Create<IRunnableServiceTask>();
			var taskMockObject = taskMock.Object;
			taskMock.Setup(tm => tm.Code).Returns("TST");
			taskMock.Setup(tm => tm.IsActive).Returns(true);
			var taskQueue = repo.Create<ITaskQueue>();
			taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			var runnerPool = repo.Create<IProcessRunnerPool>();
			runnerPool.Setup(rp => rp.GetRunnersSnapshot()).Returns(new List<ServiceTaskCodeWithRunnerProcessId>() { new ServiceTaskCodeWithRunnerProcessId("TST", 1234) });
			runnerPool.Setup(rp => rp.RunningCount(taskMock.Object)).Returns(0);
			var regChecker = repo.Create<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			var allTasks = repo.Create<IAllTasksConsumer>();
			allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[1] { taskMockObject });
			allTasks.Setup(a => a.TryGetByCode(It.IsAny<string>(), out taskMockObject)).Returns(true);
			var provider = new TaskStatusProvider(allTasks.Object, taskQueue.Object, runnerPool.Object, regChecker.Object, new List<HostedServiceBusinessObjectBindingAttribute>());

			var status = provider.GetTaskStatus("TST");
			NUnit.Framework.Assert.That(status, Is.Not.EqualTo(default(ServiceTaskStatus)));
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo("TST"));
			NUnit.Framework.Assert.That(status.RunnerPids.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(status.RunnerPids.FirstOrDefault(), Is.EqualTo(1234));
			NUnit.Framework.Assert.That(status.Status, Is.EqualTo(ServiceManagerHelper.IsInitializedMask | ServiceManagerHelper.IsActiveMask | ServiceManagerHelper.IsRunningMask));
		}

		IHostedServiceAttribute attributeMock;
		IClientHostedServiceAttributeProvider attributeProviderMock;
		IServiceTaskScheduleStatusProvider statusProviderMock;
		IHostedServiceBusinessObjectBindingsProvider bindingsProviderMock;
	}
}
