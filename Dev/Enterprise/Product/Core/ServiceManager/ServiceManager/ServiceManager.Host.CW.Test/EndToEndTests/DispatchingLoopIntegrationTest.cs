using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.DummyServiceTasks.Test;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManagerProto;
using WTG.ApplicationLogging.Abstractions;
using CompositionRoot = Enterprise.ServiceManager.Runner.CompositionRoot;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;
using LoggerFactory = ServiceManager.Logging.CW.LoggerFactory;
using TaskRunner = Enterprise.ServiceManager.Host.TaskRunner;
using TaskScheduler = Enterprise.ServiceManager.Host.TaskScheduler;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	[UseSnapshotProtection(true)]
	class DispatchingLoopIntegrationTest : TestCaseWithFactory
	{
		Mock<IHostRegistrySettings> hostRegistryMock;

		protected override void SetUp()
		{
			base.SetUp();

			hostRegistryMock = new Mock<IHostRegistrySettings>();
			hostRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(120);
			hostRegistryMock.SetupGet(o => o.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks).Returns(80);
			hostRegistryMock.SetupGet(o => o.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks).Returns(1);
			hostRegistryMock.SetupGet(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(30));
			hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(20);
			hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingBatchSizeScalingFactor).Returns(1.2m);
			hostRegistryMock.SetupGet(o => o.RunnerProcessPriorityValue).Returns(ProcessPriorityClass.BelowNormal);
			hostRegistryMock.SetupGet(o => o.ForcefullyDisabledTasks).Returns(string.Empty);
			hostRegistryMock.SetupGet(o => o.ServiceTaskRunnerSpecificGroup).Returns(new Dictionary<string, string>());
			hostRegistryMock.SetupGet(o => o.SwitchToNewServiceTasksModule).Returns(false);

			cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(3));
		}

		public void TestReenqueuedNudgeRequestDoesNotRepeatAfterSuccess_TaskNotInitialized()
		{
			// Arrange
			hostRegistryMock.SetupGet(o => o.SwitchToNewServiceTasksModule).Returns(true);
			var serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();

			Func<IServiceCollection, IServiceCollection> registrations = services =>
				services
					.AddSingleton(serviceRunnerFactoryMock.Object)
					.AddSingleton(hostRegistryMock.Object);

			using (var provider = CreateServiceProvider(registrations, false))
			using (var serviceRunnerCreatedEvent = new ManualResetEventSlim(false))
			{
				var backgroundThreadActionQueueFactory = provider.GetRequiredService<IBackgroundThreadActionQueueFactory>();
				using var backgroundThreadActionQueue = backgroundThreadActionQueueFactory.BackgroundThreadActionQueue;
				var hostLogger = provider.GetRequiredService<IHostLogger>();
				var errorReporterProxy = provider.GetRequiredService<IErrorReporterProxy>();
				var taskLoaderFactory = provider.GetRequiredService<IServiceTaskLoaderFactory>();
				var transactionAdapterFactory = provider.GetRequiredService<ITransactionAdapterFactory>();
				using var transactionAdapter = transactionAdapterFactory.CreateTransactionAdapter();
				var processRunnerRemotingServices = new ProcessRunnerRemotingServices(hostLogger, errorReporterProxy);
				IServiceRunner serviceRunner = null;
				serviceRunnerFactoryMock
					.Setup(factory => factory.Create(It.IsAny<ITaskScheduler>(), string.Empty))
					.Returns<ITaskScheduler, string>(
						(scheduler, s) =>
						{
							serviceRunner = new ProcessServiceRunnerFactory(new ProcessWrapperFactory(), new GrpcClientSynchronizerFactory(), processRunnerRemotingServices, backgroundThreadActionQueueFactory, hostLogger, provider.GetRequiredService<IServiceTaskLocksCleaner>(), new ServiceManagerDateTimeProvider(), errorReporterProxy, hostRegistryMock.Object, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")))
								.Create(scheduler, string.Empty);
							serviceRunnerCreatedEvent.Set();
							return serviceRunner;
						});

				var servicTaskInfo = ConfigureServiceTaskInfo(DoingNothingServiceTask.ServiceConfig.Code);
				var serviceTaskScheduleManager = new ServiceTaskScheduleManager(taskLoaderFactory, transactionAdapterFactory, new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider()), new ServiceTaskRequirementsChecker());
				var serviceTask = serviceTaskScheduleManager.ConfigureSchedules(new[] { servicTaskInfo.HostedServiceAttribute })
					.Single(t => t.Code == servicTaskInfo.HostedServiceAttribute.Code);
				var governer = transactionAdapter.GetServiceTaskGovernor(serviceTask.Pk);
				governer.SetActive(true);
				transactionAdapter.Commit();

				var runnableServiceTask = new RunnableServiceTask(
					servicTaskInfo,
					serviceTask,
					backgroundThreadActionQueue,
					provider.GetRequiredService<ITaskQueue>(),
					provider.GetRequiredService<IHostLogger>(),
					errorReporterProxy,
					hostRegistryMock.Object,
					transactionAdapter,
					provider.GetRequiredService<ILoggerFactory>(),
					provider.GetRequiredService<IServiceTaskScheduleStatusProvider>());
				var taskRunRequest = new DirectTaskRunRequest(runnableServiceTask);

				var tasksCollection = provider.GetRequiredService<IAllTasksCollection>();
				tasksCollection.Add(runnableServiceTask);

				var processRunnerPool = provider.GetRequiredService<IProcessRunnerPool>();
				var taskScheduler = provider.GetRequiredService<ITaskScheduler>();
				processRunnerPool.GetOrCreateRunnerAsync(taskRunRequest, taskScheduler, CancellationToken.None);

				var taskRunner = provider.GetRequiredService<ITaskRunRequestProcessor>();
				var result = taskRunner.ProcessRunRequest(taskRunRequest, serviceRunner);
				AssertEquals(TaskRunRequestResult.Success, result);
				serviceRunnerCreatedEvent.Wait(TimeSpan.FromSeconds(60));
				WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));

				var schedulerDispatcher = new SchedulerDispatcher(
					tasksCollection,
					Mock.Of<ITaskSelector>(),
					provider.GetRequiredService<ITaskRunRequestProcessor>(),
					provider.GetRequiredService<IHostLogger>(),
					Mock.Of<IServiceHostsCache>(),
					provider.GetRequiredService<IProcessRunnerPool>(),
					taskScheduler,
					backgroundThreadActionQueue,
					cancellationTokenSource.Token,
					errorReporterProxy,
					hostRegistryMock.Object,
					Mock.Of<IResourceThrottler>());

				var directTaskRunRequest = new DirectTaskRunRequest(runnableServiceTask)
				{
					OverrideNextRunDelay_ForTest = TimeSpan.FromSeconds(2)
				};

				var taskQueue = provider.GetRequiredService<ITaskQueue>();
				taskQueue.EnqueueTask(directTaskRunRequest);

				using (GetTaskLock(runnableServiceTask.Code))
				{
					// Act
					for (var i = 0; i < 2; i++)
					{
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));
						WaitForTaskQueue(backgroundThreadActionQueue, taskQueue, TimeSpan.FromSeconds(30));
					}
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					var i = 0;
					var timeout = TimeSpan.FromSeconds(30);
					var stopwatch = Stopwatch.StartNew();
					while (stopwatch.Elapsed <= timeout)
					{
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(30));
						backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
						if (i++ == 0)
						{
							taskQueue.EnqueueTask(new DirectTaskRunRequest(runnableServiceTask)
							{
								OverrideNextRunDelay_ForTest = TimeSpan.FromSeconds(2)
							});
						}

						Thread.Sleep(TimeSpan.FromMilliseconds(50));
					}
				});

				AssertEquals($"{nameof(TaskQueue)} is empty", 0, taskQueue.GetQueueSnapshot().Count());
				serviceRunnerFactoryMock.Verify(factory =>
						factory.Create(It.IsAny<ITaskScheduler>(), string.Empty),
					Times.AtLeastOnce);
				AssertNotNull("Runner is initialized", serviceRunner);

				WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));
			}

			void WaitForIdle(IServiceRunner runner, IBackgroundThreadActionQueue threadActionQueue, TimeSpan timeout)
			{
				var processId = runner.ProcessId;
				WaitFor(
					() => runner.IsIdle,
					() =>
					{
						Process.GetProcessById(processId); // The process specified by the processId parameter is not running. The identifier might be expired.
						threadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
					},
					timeout);
			}

			void WaitForTaskQueue(IBackgroundThreadActionQueue actions, ITaskQueue tasks, TimeSpan timeout)
			{
				WaitFor(
					() => tasks.GetQueueSnapshot().Any(),
					() => actions.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false),
					timeout);
			}

			void WaitFor(Func<bool> condition, Action action, TimeSpan timeout)
			{
				var stopwatch = Stopwatch.StartNew();
				while (!condition())
				{
					if (stopwatch.Elapsed >= timeout)
					{
						throw new TimeoutException("Time's up");
					}

					action();

					Thread.Sleep(TimeSpan.FromMilliseconds(50));
				}
			}

			IDisposable GetTaskLock(string taskCode)
			{
				using (var runnerProvider = CompositionRoot.AddRegistrations(new ServiceCollection(), Mock.Of<IApplicationLoggerFactory>())
							.RegisterRunnerServices()
							.BuildServiceProvider())
				{
					var locker = runnerProvider.GetRequiredService<ISqlMutexLocker>();
					AssertEquals("Task is running somewhere else", true, locker.TryAcquireLock(taskCode, out var taskLock));
					return taskLock;
				}
			}
		}

		public void TestReenqueuedNudgeRequestDoesNotRepeatAfterSuccess_TaskAlreadyInitialized()
		{
			// Arrange
			hostRegistryMock.SetupGet(o => o.SwitchToNewServiceTasksModule).Returns(true);
			var serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();

			Func<IServiceCollection, IServiceCollection> registrations = services =>
				services
					.AddSingleton(serviceRunnerFactoryMock.Object)
					.AddSingleton(hostRegistryMock.Object);

			using (var provider = CreateServiceProvider(registrations, false))
			using (var serviceRunnerCreatedEvent = new ManualResetEventSlim(false))
			{
				var backgroundThreadActionQueueFactory = provider.GetRequiredService<IBackgroundThreadActionQueueFactory>();
				using var backgroundThreadActionQueue = backgroundThreadActionQueueFactory.BackgroundThreadActionQueue;
				var hostLogger = provider.GetRequiredService<IHostLogger>();
				var errorReporterProxy = provider.GetRequiredService<IErrorReporterProxy>();
				var pk = Guid.NewGuid();
				Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.StmServiceTask (SST_PK, SST_ServiceTaskCode, SST_Active, SST_Configuration, SST_SystemCreateTimeUtc, SST_SystemCreateUser, SST_SystemLastEditTimeUtc, SST_SystemLastEditUser)
VALUES ('{pk}', '~DN', 1, '<ScheduleConfig><NextRunTimeCalculatorMinutes Period=""1"" /></ScheduleConfig>',GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
				var transactionAdapterFactory = provider.GetRequiredService<ITransactionAdapterFactory>();
				using var transactionAdapter = transactionAdapterFactory.CreateTransactionAdapter();
				var processRunnerRemotingServices = new ProcessRunnerRemotingServices(hostLogger, errorReporterProxy);
				IServiceRunner serviceRunner = null;
				serviceRunnerFactoryMock
					.Setup(factory => factory.Create(It.IsAny<ITaskScheduler>(), string.Empty))
					.Returns<ITaskScheduler, string>(
						(scheduler, s) =>
						{
							serviceRunner = new ProcessServiceRunnerFactory(new ProcessWrapperFactory(), new GrpcClientSynchronizerFactory(), processRunnerRemotingServices, backgroundThreadActionQueueFactory, hostLogger, provider.GetRequiredService<IServiceTaskLocksCleaner>(), new ServiceManagerDateTimeProvider(), errorReporterProxy, hostRegistryMock.Object, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")))
								.Create(scheduler, string.Empty);
							serviceRunnerCreatedEvent.Set();
							return serviceRunner;
						});

				var governer = transactionAdapter.GetServiceTaskGovernor(pk);
				governer.SetActive(true);
				transactionAdapter.Commit();

				var runnableServiceTask = new RunnableServiceTask(
					ConfigureServiceTaskInfo(DoingNothingServiceTask.ServiceConfig.Code),
					governer.GovernedTask,
					backgroundThreadActionQueue,
					provider.GetRequiredService<ITaskQueue>(),
					provider.GetRequiredService<IHostLogger>(),
					errorReporterProxy,
					hostRegistryMock.Object,
					transactionAdapter,
					provider.GetRequiredService<ILoggerFactory>(),
					provider.GetRequiredService<IServiceTaskScheduleStatusProvider>());
				var taskRunRequest = new DirectTaskRunRequest(runnableServiceTask);

				var tasksCollection = provider.GetRequiredService<IAllTasksCollection>();
				tasksCollection.Add(runnableServiceTask);

				var processRunnerPool = provider.GetRequiredService<IProcessRunnerPool>();
				var taskScheduler = provider.GetRequiredService<ITaskScheduler>();
				processRunnerPool.GetOrCreateRunnerAsync(taskRunRequest, taskScheduler, CancellationToken.None);

				var taskRunner = provider.GetRequiredService<ITaskRunRequestProcessor>();
				var result = taskRunner.ProcessRunRequest(taskRunRequest, serviceRunner);
				AssertEquals(TaskRunRequestResult.Success, result);
				serviceRunnerCreatedEvent.Wait(TimeSpan.FromSeconds(60));
				WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));

				var schedulerDispatcher = new SchedulerDispatcher(
					tasksCollection,
					Mock.Of<ITaskSelector>(),
					provider.GetRequiredService<ITaskRunRequestProcessor>(),
					provider.GetRequiredService<IHostLogger>(),
					Mock.Of<IServiceHostsCache>(),
					provider.GetRequiredService<IProcessRunnerPool>(),
					taskScheduler,
					backgroundThreadActionQueue,
					cancellationTokenSource.Token,
					errorReporterProxy,
					hostRegistryMock.Object,
					Mock.Of<IResourceThrottler>());

				var directTaskRunRequest = new DirectTaskRunRequest(runnableServiceTask)
				{
					OverrideNextRunDelay_ForTest = TimeSpan.FromSeconds(2)
				};

				var taskQueue = provider.GetRequiredService<ITaskQueue>();
				taskQueue.EnqueueTask(directTaskRunRequest);

				using (GetTaskLock(runnableServiceTask.Code))
				{
					// Act
					for (var i = 0; i < 2; i++)
					{
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));
						WaitForTaskQueue(backgroundThreadActionQueue, taskQueue, TimeSpan.FromSeconds(30));
					}
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					var i = 0;
					var timeout = TimeSpan.FromSeconds(30);
					var stopwatch = Stopwatch.StartNew();
					while (stopwatch.Elapsed <= timeout)
					{
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(30));
						backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
						if (i++ == 0)
						{
							taskQueue.EnqueueTask(new DirectTaskRunRequest(runnableServiceTask)
							{
								OverrideNextRunDelay_ForTest = TimeSpan.FromSeconds(2)
							});
						}

						Thread.Sleep(TimeSpan.FromMilliseconds(50));
					}
				});

				AssertEquals($"{nameof(TaskQueue)} is empty", 0, taskQueue.GetQueueSnapshot().Count());
				serviceRunnerFactoryMock.Verify(factory =>
						factory.Create(It.IsAny<ITaskScheduler>(), string.Empty),
					Times.AtLeastOnce);
				AssertNotNull("Runner is initialized", serviceRunner);

				WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(60));
			}

			void WaitForIdle(IServiceRunner runner, IBackgroundThreadActionQueue threadActionQueue, TimeSpan timeout)
			{
				var processId = runner.ProcessId;
				WaitFor(
					() => runner.IsIdle,
					() =>
					{
						Process.GetProcessById(processId); // The process specified by the processId parameter is not running. The identifier might be expired.
						threadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
					},
					timeout);
			}

			void WaitForTaskQueue(IBackgroundThreadActionQueue actions, ITaskQueue tasks, TimeSpan timeout)
			{
				WaitFor(
					() => tasks.GetQueueSnapshot().Any(),
					() => actions.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false),
					timeout);
			}

			void WaitFor(Func<bool> condition, Action action, TimeSpan timeout)
			{
				var stopwatch = Stopwatch.StartNew();
				while (!condition())
				{
					if (stopwatch.Elapsed >= timeout)
					{
						throw new TimeoutException("Time's up");
					}

					action();

					Thread.Sleep(TimeSpan.FromMilliseconds(50));
				}
			}

			IDisposable GetTaskLock(string taskCode)
			{
				using (var runnerProvider = CompositionRoot.AddRegistrations(new ServiceCollection(), Mock.Of<IApplicationLoggerFactory>())
							.RegisterRunnerServices()
							.BuildServiceProvider())
				{
					var locker = runnerProvider.GetRequiredService<ISqlMutexLocker>();
					AssertEquals("Task is running somewhere else", true, locker.TryAcquireLock(taskCode, out var taskLock));
					return taskLock;
				}
			}
		}

		public void TestReenqueuedNudgeRequestDoesRepeat()
		{
			// Arrange
			hostRegistryMock.SetupGet(o => o.SwitchToNewServiceTasksModule).Returns(true);
			var loggerMock = new Mock<IHostLogger>();
			var serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();
			Func<IServiceCollection, IServiceCollection> registrations = services =>
				services
					.AddSingleton<ILogger>(loggerMock.Object)
					.AddSingleton(loggerMock.Object)
					.AddSingleton<IAsyncDelayProvider, DelayProvider>()
					.AddSingleton(serviceRunnerFactoryMock.Object)
					.AddSingleton(hostRegistryMock.Object);

			using (var provider = CreateServiceProvider(registrations, false))
			{
				// Arrange
				var backgroundThreadActionQueueFactory = provider.GetRequiredService<IBackgroundThreadActionQueueFactory>();
				using var backgroundThreadActionQueue = backgroundThreadActionQueueFactory.BackgroundThreadActionQueue;
				var errorReporterProxy = provider.GetRequiredService<IErrorReporterProxy>();
				var transactionAdapterFactory = provider.GetRequiredService<ITransactionAdapterFactory>();
				using var transactionAdapter = transactionAdapterFactory.CreateTransactionAdapter();
				var processRunnerRemotingServices = new ProcessRunnerRemotingServices(loggerMock.Object, errorReporterProxy);

				IServiceRunner serviceRunner = null;
				serviceRunnerFactoryMock
					.Setup(factory => factory.Create(It.IsAny<ITaskScheduler>(), string.Empty))
					.Returns<ITaskScheduler, string>(
						(scheduler, s) =>
							serviceRunner ??
							(serviceRunner = new ProcessServiceRunnerFactory(new ProcessWrapperFactory(), new GrpcClientSynchronizerFactory(), processRunnerRemotingServices, backgroundThreadActionQueueFactory, loggerMock.Object, provider.GetRequiredService<IServiceTaskLocksCleaner>(), new ServiceManagerDateTimeProvider(), errorReporterProxy, hostRegistryMock.Object, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")))
								.Create(scheduler, string.Empty)));

				var runnableServiceTask = new RunnableServiceTask(
					ConfigureServiceTaskInfo(DoingNothingServiceTask.ServiceConfig.Code),
					null,
					backgroundThreadActionQueue,
					provider.GetRequiredService<ITaskQueue>(),
					loggerMock.Object,
					errorReporterProxy,
					hostRegistryMock.Object,
					transactionAdapter,
					new LoggerFactory(),
					provider.GetRequiredService<IServiceTaskScheduleStatusProvider>());

				var pk = Guid.NewGuid();
				Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.StmServiceTask (SST_PK, SST_ServiceTaskCode, SST_Active, SST_Configuration, SST_SystemCreateTimeUtc, SST_SystemCreateUser, SST_SystemLastEditTimeUtc, SST_SystemLastEditUser)
VALUES ('{pk}', '~DN', 1, '<ScheduleConfig><NextRunTimeCalculatorMinutes Period=""1"" /></ScheduleConfig>',GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
				var governor = transactionAdapter.GetServiceTaskGovernor(pk);
				runnableServiceTask.UpdateSchedule(new[] { governor.GovernedTask }, reEnableMandatory: false);

				var tasksCollection = provider.GetRequiredService<IAllTasksCollection>();
				tasksCollection.Add(runnableServiceTask);

				var taskRunRequest = new DirectTaskRunRequest(runnableServiceTask);
				var processRunnerPool = provider.GetRequiredService<IProcessRunnerPool>();
				var taskScheduler = provider.GetRequiredService<ITaskScheduler>();
				using var schedulerDispatcherCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(3));
				processRunnerPool.GetOrCreateRunnerAsync(taskRunRequest, taskScheduler, CancellationToken.None);

				var schedulerDispatcher = new SchedulerDispatcher(
					tasksCollection,
					Mock.Of<ITaskSelector>(),
					provider.GetRequiredService<ITaskRunRequestProcessor>(),
					loggerMock.Object,
					Mock.Of<IServiceHostsCache>(),
					provider.GetRequiredService<IProcessRunnerPool>(),
					taskScheduler,
					backgroundThreadActionQueue,
					schedulerDispatcherCancellationTokenSource.Token,
					errorReporterProxy,
					hostRegistryMock.Object,
					Mock.Of<IResourceThrottler>());

				var directTaskRunRequest = new DirectTaskRunRequest(runnableServiceTask, false);
				var taskQueue = provider.GetRequiredService<ITaskQueue>();
				taskQueue.EnqueueTask(directTaskRunRequest);

				using (GetTaskLock(runnableServiceTask.Code))
				using (var requestQueuedAgainEvent = new ManualResetEventSlim(false))
				{
					loggerMock
						.Setup(logger => logger.Log(
							It.IsAny<LogLevel>(),
							It.Is<string>(s => s.Contains("Requeueing Nudge run request after an attempt failed due to reason: Service task lock for single instance service task could not be obtained."))))
						.Callback<LogLevel, string>((type, message) => requestQueuedAgainEvent.Set());

					FirstBlockedRun();

					void FirstBlockedRun()
					{
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						WaitForIdle(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(30));
					}

					WaitFor(
						() => requestQueuedAgainEvent.IsSet,
						() => backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false),
						serviceRunner,
						TimeSpan.FromSeconds(10));
				}

				cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1));
				{
					loggerMock
						.Setup(logger => logger.Log(
							It.IsAny<LogLevel>(),
							It.Is<string>(s => s.Contains("Nudge run request is sent to Runner with PID="))))
						.Callback<LogLevel, string>((type, message) => cancellationTokenSource.Cancel());

					// Act
					SecondRun();

					void SecondRun()
					{
						while (!cancellationTokenSource.IsCancellationRequested)
						{
							schedulerDispatcher.Schedule(taskQueue);
							schedulerDispatcher.Dispatch(taskQueue);
							backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
						}

						WaitForTaskNotRunning(serviceRunner, backgroundThreadActionQueue, TimeSpan.FromSeconds(30));
					}
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock
						.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.Is<string>(s => s.Contains("Nudge run request is sent to Runner with PID="))),
							Times.Exactly(2));
				});
				AssertEquals(false, directTaskRunRequest.HasRunsRemaining);
			}

			void WaitForTaskNotRunning(IServiceRunner runner, IBackgroundThreadActionQueue threadActionQueue, TimeSpan timeout)
			{
				var processId = runner.ProcessId;
				WaitFor(
					() => !runner.TaskRunning,
					() =>
					{
						Process.GetProcessById(processId); // The process specified by the processId parameter is not running. The identifier might be expired.
						threadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
					},
					runner,
					timeout);
			}

			void WaitForIdle(IServiceRunner runner, IBackgroundThreadActionQueue threadActionQueue, TimeSpan timeout)
			{
				var processId = runner.ProcessId;
				WaitFor(
					() => runner.IsIdle,
					() =>
					{
						Process.GetProcessById(processId); // The process specified by the processId parameter is not running. The identifier might be expired.
						threadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
					},
					runner,
					timeout);
			}

			void WaitFor(Func<bool> condition, Action action, IServiceRunner runner, TimeSpan timeout)
			{
				var stopwatch = Stopwatch.StartNew();
				while (!condition())
				{
					if (stopwatch.Elapsed >= timeout)
					{
						throw new TimeoutException($"Time's up - Runner State: TaskRunning[{runner.TaskRunning}] IsIdle[{runner.IsIdle}] IsAllocatingTask[{runner.IsAllocatingTask}] ElapsedFromLastRun[{runner.ElapsedFromLastRun}]");
					}

					action();

					Thread.Sleep(TimeSpan.FromMilliseconds(50));
				}
			}

			IDisposable GetTaskLock(string taskCode)
			{
				using (var runnerProvider = CompositionRoot.AddRegistrations(new ServiceCollection(), Mock.Of<IApplicationLoggerFactory>())
							.RegisterRunnerServices()
							.BuildServiceProvider())
				{
					var locker = runnerProvider.GetRequiredService<ISqlMutexLocker>();
					Assert("Task is running somewhere else", locker.TryAcquireLock(taskCode, out var taskLock));
					return taskLock;
				}
			}
		}

		public void TestGetRunnerCompletesImmediatelyWhenBusyRunnerBecomesIdle()
		{
			// Arrange
			hostRegistryMock.SetupGet(o => o.SwitchToNewServiceTasksModule).Returns(true);

			var serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();

			Func<IServiceCollection, IServiceCollection> registerHostRegistry = services =>
				services.AddSingleton(hostRegistryMock.Object);

			Func<IServiceCollection, IServiceCollection> registerServiceRunnerFactory = services =>
				services.AddSingleton(serviceRunnerFactoryMock.Object);

			Func<IServiceCollection, IServiceCollection> registerTask =
				services => services
					.AddSingleton(provider =>
						new RunnableServiceTask(
							ConfigureServiceTaskInfo(DoingNothingServiceTask.ServiceConfig.Code, true),
							null,
							provider.GetRequiredService<IBackgroundThreadActionQueueFactory>().BackgroundThreadActionQueue,
							provider.GetRequiredService<ITaskQueue>(),
							provider.GetRequiredService<IHostLogger>(),
							provider.GetRequiredService<IErrorReporterProxy>(),
							hostRegistryMock.Object,
							provider.GetRequiredService<ITransactionAdapterFactory>().CreateTransactionAdapter(),
							provider.GetRequiredService<ILoggerFactory>(),
							provider.GetRequiredService<IServiceTaskScheduleStatusProvider>()));

			Func<IServiceCollection, IServiceCollection> configureTaskSelectorToReturnRunRequests =
				services =>
					services
						.AddSingleton(provider =>
						{
							var taskSelectorMock = new Mock<ITaskSelector>();
							var task = provider.GetRequiredService<RunnableServiceTask>();
							taskSelectorMock
								.SetupSequence(selector => selector.SelectTasksToRun(It.IsAny<IEnumerable<IRunnableServiceTask>>()))
								.Returns(
									new ITaskRunRequest[]
									{
										new ScheduledTaskRunRequest(task),
										new DirectTaskRunRequest(task)
									})
								.Throws<InvalidOperationException>();
							return taskSelectorMock.Object;
						});

			var registrations = Functions.ComposeAll(registerHostRegistry, registerServiceRunnerFactory, registerTask, configureTaskSelectorToReturnRunRequests);

			var timeToWait = TimeSpan.FromSeconds(30);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)timeToWait.TotalSeconds);
			hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(1);
			hostRegistryMock.SetupGet(o => o.ForcefullyDisabledTasks).Returns(string.Empty);

			using (var provider = CreateServiceProvider(registrations, false))
			using (var serviceTaskEndedEvent = new ManualResetEvent(false))
			using (var dispatchFinishedEvent = new ManualResetEvent(false))
			{
				// Instantiate the task selector so it populates the right values from the task, as the task is mutable
				var runnableServiceTask = provider.GetRequiredService<RunnableServiceTask>();
				using var transactionAdapter = provider.GetRequiredService<ITransactionAdapterFactory>().CreateTransactionAdapter();

				var hostLogger = provider.GetRequiredService<IHostLogger>();
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				var processRunnerRemotingServices = new ProcessRunnerRemotingServices(hostLogger, errorReporterProxyMock.Object);
				var backgroundThreadActionQueue = provider.GetRequiredService<IBackgroundThreadActionQueueFactory>().BackgroundThreadActionQueue;
				InitServiceTask(runnableServiceTask, transactionAdapter);
				Mock<ProcessServiceRunner> serviceRunnerMock = null;
				serviceRunnerFactoryMock
					.Setup(factory => factory.Create(It.IsAny<ITaskScheduler>(), string.Empty))
					.Returns<ITaskScheduler, string>(
						(scheduler, taskGroup) =>
						{
							if (serviceRunnerMock == null)
							{
								serviceRunnerMock = new Mock<ProcessServiceRunner>(
										scheduler,
										backgroundThreadActionQueue,
										new ProcessWrapperFactory(),
										processRunnerRemotingServices,
										hostLogger,
										new GrpcClientSynchronizerFactory(),
										provider.GetRequiredService<IServiceTaskLocksCleaner>(),
										new ServiceManagerDateTimeProvider(),
										errorReporterProxyMock.Object,
										hostRegistryMock.Object,
										taskGroup,
										Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")))
								{ CallBase = true };
								serviceRunnerMock
									.Protected()
									.Setup("ProcessResponseMessage", ItExpr.IsAny<ServiceTaskRunResponse>())
									.Callback<ServiceTaskRunResponse>((response) =>
									{
										if (response.Status != Status.Queued)
										{
											serviceTaskEndedEvent.Set();
										}
									})
									.CallBase();
							}

							return serviceRunnerMock.Object;
						});

				var taskQueue = provider.GetRequiredService<ITaskQueue>();
				var taskQueueMock = new Mock<ITaskQueue>(MockBehavior.Strict);
				taskQueueMock
					.Setup(queue => queue.EnqueueTask(It.IsAny<ITaskRunRequest>()))
					.Returns<ITaskRunRequest>(request => taskQueue.EnqueueTask(request));
				ITaskRunRequest dequeued = null;
				taskQueueMock
					.Setup(queue => queue.TryDequeueTask(out dequeued))
					.Returns(new TryDequeueTaskCallback((out ITaskRunRequest request) =>
					{
						Assert($"Previous task should complete without error ({serviceRunnerMock})", IsFirstTaskOrPreviousTaskCompleted());
						var tryDequeueTask = taskQueue.TryDequeueTask(out request);
						Assert("Queue should dequeue successfully", tryDequeueTask);
						return true;

						bool IsFirstTaskOrPreviousTaskCompleted()
						{
							if (serviceRunnerMock == null)
							{
								return true;
							}

							serviceTaskEndedEvent.WaitOne(timeToWait);
							backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(1), () => false);
							var stopwatch = Stopwatch.StartNew();
							while (serviceRunnerMock.Object.TaskRunning
									|| stopwatch.Elapsed < TimeSpan.FromSeconds(1))
							{
								Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
							}
							return !serviceRunnerMock.Object.TaskRunning;
						}
					}));
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(() => taskQueue.GetQueueSnapshot());

				var tasksCollection = provider.GetRequiredService<IAllTasksCollection>();
				tasksCollection.Add(runnableServiceTask);

				var schedulerDispatcher = new SchedulerDispatcher(
					tasksCollection,
					provider.GetRequiredService<ITaskSelector>(),
					provider.GetRequiredService<ITaskRunRequestProcessor>(),
					provider.GetRequiredService<IHostLogger>(),
					Mock.Of<IServiceHostsCache>(),
					provider.GetRequiredService<IProcessRunnerPool>(),
					provider.GetRequiredService<ITaskScheduler>(),
					backgroundThreadActionQueue,
					cancellationTokenSource.Token,
					errorReporterProxyMock.Object,
					hostRegistryMock.Object,
					Mock.Of<IResourceThrottler>());

				// Act
				RunTaskDispatchingRound(schedulerDispatcher, taskQueueMock.Object, dispatchFinishedEvent);

				// Assert
				serviceTaskEndedEvent.WaitOne(timeToWait);
				Assert("Dispatch round is finished", dispatchFinishedEvent.WaitOne(timeToWait));
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);

				taskQueueMock
					.Verify(queue => queue.EnqueueTask(It.IsAny<ITaskRunRequest>()), Times.Exactly(2));
				serviceRunnerMock
					.Protected()
					.Verify("RunCore", Times.Exactly(2), ItExpr.IsAny<ITaskRunRequest>());
			}

			void InitServiceTask(RunnableServiceTask serviceTask, ITransactionAdapter transactionAdapter)
			{
				var businessObjectFactory = new BusinessObjectFactory();
				var schedule = businessObjectFactory.New<StmServiceTask>();
				schedule.SST_ServiceTaskCode = serviceTask.Code;
				schedule.SST_Active = true;
				schedule.SST_NextRunTime = new ZDateTimeOffset(DateTimeOffset.MinValue + TimeSpan.FromDays(1));
				schedule.SST_GB_Branch = GlbBranch.CurrentBranch.PK;
				schedule.SST_Configuration =
					"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"15\"  /><SecondaryProcessesMaxCount>10</SecondaryProcessesMaxCount></ScheduleConfig>";
				businessObjectFactory.Save();
				var governor = transactionAdapter.GetServiceTaskGovernor(schedule.PK.ToGuid());
				serviceTask.UpdateSchedule(new[] { governor.GovernedTask }, false);
			}

			void RunTaskDispatchingRound(SchedulerDispatcher schedulerDispatcher, ITaskQueue queue, EventWaitHandle dispatchFinishedEvent)
			{
				schedulerDispatcher.Schedule(queue);
				schedulerDispatcher.Dispatch(queue);
				dispatchFinishedEvent.Set();
			}
		}

		static ServiceTaskInfo ConfigureServiceTaskInfo(string serviceTaskCode, bool allowsMultipleInstances = false)
		{
			var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
				config =>
					config.Code == serviceTaskCode
					&& config.TypeAssemblyName == Path.GetFileNameWithoutExtension(typeof(DoingNothingServiceTask).Assembly.Location)
					&& config.Description == DoingNothingServiceTask.ServiceConfig.Description
					&& config.Category == DoingNothingServiceTask.ServiceConfig.Category
					&& config.TypeName == typeof(DoingNothingServiceTask).FullName
					&& config.AllowsMultipleInstances == allowsMultipleInstances
					&& config.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.Upgrade
					&& config.CanRunInAnyBranch
					&& config.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes")
					&& config.ActiveByDefault);
			return new ServiceTaskInfo(hostedServiceConfigMock);
		}

		static void UpdateSchedule(IRunnableServiceTask serviceTask)
		{
			var collectionGovernor = new SchedulerServiceTasksLoader(new BusinessObjectFactory()).Load();
			serviceTask.UpdateSchedule(collectionGovernor.GovernedTasks, false);
		}

		ServiceProvider CreateServiceProvider(Func<IServiceCollection, IServiceCollection> customRegistrations, bool useOldTable) =>
			new ServiceCollection()
				.AddRegistrations(Array.Empty<string>())
				.AddSingleton(Mock.Of<IDelayProvider>())
				.AddSingleton(Mock.Of<IAsyncDelayProvider>())
				.AddSingleton(
					provider =>
					{
						var actionQueue = new BackgroundThreadActionQueue(cancellationTokenSource.Token, provider.GetRequiredService<IAsyncDelayProvider>());
						var mockFactory = new Mock<IBackgroundThreadActionQueueFactory>();
						mockFactory
							.Setup(f => f.BackgroundThreadActionQueue)
							.Returns(actionQueue);
						return mockFactory.Object;
					})
				.AddSingleton(Mock.Of<INudgingController>())
				.AddSingleton(Mock.Of<IJobObject>())
				.AddSingleton(Mock.Of<IProductRegistrationPeriodicChecker>())
				.AddSingleton(Mock.Of<IBackgroundDataSaverFactory>(factory =>
					factory.Create(
						It.IsAny<string>(),
						It.IsAny<TimeSpan>(),
						It.IsAny<BackgroundDataFileAction>(),
						It.IsAny<Action<Exception>>()) == Mock.Of<IBackgroundDataSaver>()))
				.AddSingleton(Mock.Of<ITaskSelector>())
				.AddSingleton(Mock.Of<IServiceHostsCache>())
				.AddSingleton(Mock.Of<IResourceThrottler>())
				.AddSingleton(Mock.Of<IHttpListenerTask>())
				.AddSingleton(Mock.Of<IHttpListenerFactory>())
				.AddSingleton(new BusinessObjectFactory())
				.AddSingleton<IProcessRunnerPool, ProcessRunnerPool>()
				.AddSingleton<ITaskQueue, TaskQueue>()
				.AddSingleton<ITaskRunRequestProcessor, TaskRunner>()
				.AddSingleton(Mock.Of<IHostLogger>())
				.AddSingleton(Mock.Of<IErrorReporterProxy>())
				.AddSingleton(Mock.Of<IServiceTaskScheduleStatusProvider>())
				.AddSingleton(
					provider =>
					{
						var mock = new Mock<ILogger>();
						var hostLogger = provider.GetRequiredService<IHostLogger>();
						mock
							.Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
							.Callback((LogLevel type, string message) => hostLogger.Log(type, message));
						return mock.Object;
					})
				.AddSingleton<ITaskScheduler>(
					provider =>
					{
						var runnableServiceTask = new RunnableServiceTask(
							ConfigureServiceTaskInfo(DoingNothingServiceTask.ServiceConfig.Code),
							null,
							provider.GetRequiredService<IBackgroundThreadActionQueueFactory>().BackgroundThreadActionQueue,
							provider.GetService<ITaskQueue>(),
							provider.GetService<IHostLogger>(),
							provider.GetService<IErrorReporterProxy>(),
							provider.GetService<IHostRegistrySettings>(),
							provider.GetService<ITransactionAdapterFactory>().CreateTransactionAdapter(),
							provider.GetRequiredService<ILoggerFactory>(),
							provider.GetRequiredService<IServiceTaskScheduleStatusProvider>());

						provider.GetRequiredService<IAllTasksCollection>().Add(runnableServiceTask);

						return new TaskScheduler(
							provider.GetService<ITransactionAdapterFactory>().CreateTransactionAdapter(),
							provider.GetRequiredService<IAllTasksConsumer>(),
							provider.GetRequiredService<IHostLogger>(),
							provider.GetRequiredService<IProductRegistrationPeriodicChecker>(),
							provider.GetRequiredService<IBackgroundDataSaverFactory>(),
							provider.GetRequiredService<IErrorReporterProxy>(),
							provider.GetRequiredService<IDatabaseAspectVersions>(),
							provider.GetRequiredService<IClientHostedServiceAttributeProvider>());
					})
				.AddSingleton(Mock.Of<IServiceRunnerFactory>())
				.AddSingleton(Mock.Of<IRequestQueue>())
				.AddSingleton(Mock.Of<IRequestQueueProcessor>())
				.AddCustom(customRegistrations)
				.BuildServiceProvider();

		CancellationTokenSource cancellationTokenSource;

		delegate bool TryDequeueTaskCallback(out ITaskRunRequest taskRunRequest);
	}
}

internal static class Functions
{
	internal static A Identity<A>(A a) => a;

	internal static Func<A, C> AndThen<A, B, C>(this Func<A, B> f, Func<B, C> g) => a => g(f(a));

	internal static Func<A, A> ComposeAll<A>(Func<A, A> f, params Func<A, A>[] gs) => gs.Aggregate(f, (f1, f2) => f1.AndThen(f2));
}
