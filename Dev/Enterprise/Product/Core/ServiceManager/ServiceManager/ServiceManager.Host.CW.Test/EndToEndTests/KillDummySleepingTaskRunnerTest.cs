using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.DummySleepingService;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	[UseSnapshotProtection(true)]
	class KillDummySleepingTaskRunnerTest : TestCase
	{
		public void TestServiceTaskCanBeRestartedAfterRunnerProcessWasKilled()
		{
			RunDummySleepingServiceTask(
				runner =>
				{
					try
					{
						Process.GetProcessById(runner.ProcessId)?.Kill();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						// ignored
					}
				});
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestServiceTaskCanBeRestartedAfterRunnerProcessWasStopped()
		{
			RunDummySleepingServiceTask(runner => runner.Stop());
		}

		void RunDummySleepingServiceTask(Action<IServiceRunner> runnerAction)
		{
			var hostOptionsMock = new Mock<IServiceManagerHostOptions>();
			hostOptionsMock.Setup(o => o.ServerName).Returns(Db.ServerName);
			hostOptionsMock.Setup(o => o.DatabaseName).Returns(Db.DatabaseName);

			using var cancellationTokenSource = new CancellationTokenSource();
			using var backgroundThreadActionQueue = new BackgroundThreadActionQueue(cancellationTokenSource.Token);

			var backgroundThreadActionQueueFactoryMock = new Mock<IBackgroundThreadActionQueueFactory>();
			backgroundThreadActionQueueFactoryMock
				.Setup(f => f.BackgroundThreadActionQueue)
				.Returns(backgroundThreadActionQueue);

			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			var attributeMock = Mock.Of<IHostedServiceAttribute>(config =>
				config.Code == DummySleepingTask.ServiceConfig.Code
				&& config.TypeAssemblyName == Path.GetFileNameWithoutExtension(typeof(DummySleepingTask).Assembly.Location)
				&& config.Description == DummySleepingTask.ServiceConfig.Description
				&& config.Category == DummySleepingTask.ServiceConfig.Category
				&& !config.AllowsMultipleInstances
				&& config.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.Upgrade
				&& config.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			var attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			attributeProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(attributeMock);

			using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);

			using (var provider = new ServiceCollection()
						.AddRegistrations(Array.Empty<string>())
						.RegisterSharedServices()
						.RegisterRunnerServices()
						.AddSingleton<ITransactionAdapterFactory, HostTransactionAdapterFactory>()
						.BuildServiceProvider())
			{
				var hostLogger = Mock.Of<IHostLogger>();
				var processServiceRunnerFactory = new ProcessServiceRunnerFactory(new ProcessWrapperFactory(),
					new GrpcClientSynchronizerFactory(),
					new ProcessRunnerRemotingServices(hostLogger, errorReporterProxyMock.Object),
					backgroundThreadActionQueueFactoryMock.Object, hostLogger, provider.GetRequiredService<IServiceTaskLocksCleaner>(),
					new ServiceManagerDateTimeProvider(), errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(
						o => o.RunnerProcessPriorityValue == ProcessPriorityClass.BelowNormal),
					Mock.Of<IProductRegistration>(o =>
						o.Key == Mock.Of<IProductRegistrationKey>(x =>
							x.EnterpriseCode == "TST" && x.ServerCode == "TST")));

				using var transactionAdapter = provider.GetRequiredService<ITransactionAdapterFactory>().CreateTransactionAdapter();
				using var runner = processServiceRunnerFactory.Create(new Mock<ITaskScheduler>(MockBehavior.Strict).Object, string.Empty);

				var serviceTaskInfo = new ServiceTaskInfo(attributeMock);
				var runnableServiceTask = new RunnableServiceTask(
					serviceTaskInfo,
					null,
					backgroundThreadActionQueue,
					new Mock<ITaskQueue>().Object,
					hostLogger,
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == string.Empty),
					transactionAdapter,
					new LoggerFactory(),
					Mock.Of<IServiceTaskScheduleStatusProvider>());

				// Initialization
				var factory = new BusinessObjectFactory();
				var schedule = factory.New<StmServiceTask>();
				schedule.SST_Active = true;
				schedule.SST_ServiceTaskCode = serviceTaskInfo.Code;
				schedule.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" /></ScheduleConfig>";
				factory.Save();

				// First run
				var runRequest = new DirectTaskRunRequest(runnableServiceTask);
				runner.Run(runRequest);
				var runnerProcessId = runner.ProcessId;
				AssertEquals("First start is performed", true, ServiceTaskShouldBeStarted(TimeSpan.FromMinutes(1)));

				// Arrange
				runnerAction?.Invoke(runner);
				WaitForIdleOrExited(runner, runnerProcessId, TimeSpan.FromMinutes(3));
				backgroundThreadActionQueue.InvokeActions();

				// Act
				runner.Run(new DirectTaskRunRequest(runnableServiceTask));
				backgroundThreadActionQueue.InvokeActions();

				// Assert
				var result = ServiceTaskShouldBeStarted(TimeSpan.FromSeconds(60));

				runner.Stop();
				cancellationTokenSource.Cancel();
				WaitForIdleOrExited(runner, runnerProcessId, TimeSpan.FromMinutes(3));

				backgroundThreadActionQueue.InvokeActions();
				AssertEquals("Second start after killed process is performed", true, result);
			}

			void WaitForIdleOrExited(IServiceRunner runner, int processId, TimeSpan timeout)
			{
				var stopwatch = Stopwatch.StartNew();
				while (!runner.IsIdle)
				{
					if (stopwatch.Elapsed >= timeout)
					{
						throw new AssertionFailedError("Process did not exit in time");
					}
					Thread.Sleep(TimeSpan.FromMilliseconds(50));

					try
					{
						Process.GetProcessById(processId).WaitForExit(1000);
					}
					catch (ArgumentException) // The process specified by the processId parameter is not running. The identifier might be expired.
					{
						return;
					}
				}
			}

			bool ServiceTaskShouldBeStarted(TimeSpan timeout)
			{
				Mutex mutex;
				var stopwatch = Stopwatch.StartNew();
				while (!Mutex.TryOpenExisting(DummySleepingTask.MutexLock, out mutex))
				{
					if (stopwatch.Elapsed >= timeout)
					{
						return false;
					}

					Thread.Sleep(TimeSpan.FromMilliseconds(50));
				}

				mutex.Dispose();
				return true;
			}
		}
	}
}
