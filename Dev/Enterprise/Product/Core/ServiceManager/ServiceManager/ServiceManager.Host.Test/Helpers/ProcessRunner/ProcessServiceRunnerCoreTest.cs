using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if NET
using System.Runtime.InteropServices;
#endif
using System.Text;
#if NET
using System.Text.Json;
#endif
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ServiceManager.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ProcessServiceRunnerCoreTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLogMessageWhenSuccessfullyProcess()
		{
			// Arrange
			const int id = 1234;

			var logger = new Mock<IHostLogger>();
			var factory = new Mock<IProcessFactory>();
			factory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(Mock.Of<IProcess>(y => y.Id == id));

			var runner = new Mock<ProcessServiceRunnerCore>(
				Mock.Of<ITaskScheduler>(),
				Mock.Of<IBackgroundThreadActionQueue>(),
				factory.Object,
				logger.Object,
				Mock.Of<IServiceTaskLocksCleaner>(),
				new ServiceManagerDateTimeProvider(),
				new Mock<IErrorReporterProxy>().Object,
				Mock.Of<IHostRegistrySettings>(),
				string.Empty);
			runner
				.Protected()
				.Setup<bool>("RunCore", ItExpr.IsAny<ITaskRunRequest>())
				.Returns(true);

			var request = Mock.Of<ITaskRunRequest>(
				x => x.Task == Mock.Of<IRunnableServiceTask>(
					y => y.Code == "LWM"));

			// Act
			runner.Object.Run(request);

			// Assert
			logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<string>()), Times.Once);
			Mock.Get(request).Verify(x => x.FormatRequestToLogMessage(LogMessageStage.RequestIsSentToRunner, runner.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcessAlreadyDisposed()
		{
			var loggerMock = new Mock<IHostLogger>();
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerCoreForTesting(actionQueue, "Key", loggerMock.Object, dateTimeProviderMock.Object))
			{
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll" };
				var taskInfo = new ServiceTaskInfo(config);
				var runnableTask = new Mock<IRunnableServiceTask>();
				runnableTask.Setup(rt => rt.Info).Returns(taskInfo);

				runner.Run(new ScheduledTaskRunRequest(runnableTask.Object));
				Thread.Sleep(TimeSpan.FromSeconds(1));

				runner.Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestDateFormatIsCorrectInLogMessage()
		{
			var mockTask = new Mock<ITaskScheduler>();
			var mockBackground = new Mock<IBackgroundThreadActionQueue>();
			var mockProcess = new Mock<IProcessFactory>();
			var mockHost = new Mock<IHostLogger>();
			var mockService = new Mock<IServiceTaskLocksCleaner>();
			var processServiceRunnerCoreMock = new Mock<ProcessServiceRunnerCore>(mockTask.Object, mockBackground.Object, mockProcess.Object, mockHost.Object, mockService.Object, new ServiceManagerDateTimeProvider(), new Mock<IErrorReporterProxy>().Object, Mock.Of<IHostRegistrySettings>(), string.Empty)
			{
				CallBase = true,
			};
			processServiceRunnerCoreMock
				.Setup(r => r.TaskRunning)
				.Returns(true);

			var expectedMessage = @"^\d{2}-[a-zA-Z]{3}-\d{2}$";
			var capturedLogMessage = "test one";

			var loggerMock = new Mock<IHostLogger>();
			loggerMock
				.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
				.Callback((LogLevel t, string s) =>
				{
					capturedLogMessage = s;
				});

			processServiceRunnerCoreMock.Object.CheckIdleStatus(loggerMock.Object, new TimeSpan(1));
			var result = capturedLogMessage.Split(' ');

			NUnit.Framework.Assert.That(result.Any(x => Regex.IsMatch(x, expectedMessage)), Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestElapsedFromLastRunIsSet()
		{
			var loggerMock = new Mock<IHostLogger>();
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerCoreForTesting(actionQueue, "Key", loggerMock.Object, dateTimeProviderMock.Object))
			{
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll" };
				var taskInfo = new ServiceTaskInfo(config);
				var runnableTask = new Mock<IRunnableServiceTask>();
				runnableTask.Setup(rt => rt.Info).Returns(taskInfo);

				NUnit.Framework.Assert.That(runner.ElapsedFromLastRun, Is.EqualTo(TimeSpan.Zero));

				runner.Run(new ScheduledTaskRunRequest(runnableTask.Object));

				NUnit.Framework.Assert.That(runner.ElapsedFromLastRun, Is.Not.EqualTo(DateTime.MinValue).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestIsAllocatingTaskSetToFalseAfterRunCore()
		{
			var loggerMock = new Mock<IHostLogger>();
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerCoreForTesting(actionQueue, "Key", loggerMock.Object, dateTimeProviderMock.Object))
			{
				var isAllocatingDuringRun = true;
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll" };
				var taskInfo = new ServiceTaskInfo(config);
				var runnableTask = new Mock<IRunnableServiceTask>();
				runnableTask.Setup(rt => rt.Info).Returns(taskInfo);
				runner.IsAllocatingTask = true;
				loggerMock
					.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
					.Callback(() => isAllocatingDuringRun = runner.IsAllocatingTask);

				runner.Run(new ScheduledTaskRunRequest(runnableTask.Object));

				NUnit.Framework.Assert.That(isAllocatingDuringRun, Is.EqualTo(true));
				NUnit.Framework.Assert.That(runner.IsAllocatingTask, Is.EqualTo(false));
			}
		}

		public class RunMethodTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestSetsFlagExpectingTaskRunRequestCallback()
			{
				// Arrange
				const int id = 1234;

				var logger = Mock.Of<IHostLogger>();
				var factory = Mock.Of<IProcessFactory>(
					processFactory =>
						processFactory.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()) == Mock.Of<IProcess>(y => y.Id == id));

				var runner = new Mock<ProcessServiceRunnerCore>(
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					factory,
					logger,
					Mock.Of<IServiceTaskLocksCleaner>(),
					new ServiceManagerDateTimeProvider(),
					new Mock<IErrorReporterProxy>().Object,
					Mock.Of<IHostRegistrySettings>(),
					string.Empty);
				runner
					.Protected()
					.Setup<bool>("RunCore", ItExpr.IsAny<ITaskRunRequest>())
					.Returns(true);

				var request = Mock.Of<ITaskRunRequest>(
					x => x.Task == Mock.Of<IRunnableServiceTask>(
						y => y.Code == "LWM"));

				// Act
				runner.Object.Run(request);

				// Assert
				NUnit.Framework.Assert.That(runner.Object.ExpectingTaskRunRequestCallback, Is.EqualTo(true));
			}
		}

		public class KillTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);
			}

			Mock<IDateTimeProvider> dateTimeProviderMock;

			[ExpectNoExceptions]
			public void TestExpectingTaskRunRequestFlagNotClearedSoRequestIsReprocessed()
			{
				// Arrange
				const int id = 1234;

				var logger = Mock.Of<IHostLogger>();

				var runner = new Mock<ProcessServiceRunnerCore>(
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<IProcessFactory>(
						processFactory => processFactory.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()) ==
										Mock.Of<IProcess>(y => y.Id == id)),
					logger,
					Mock.Of<IServiceTaskLocksCleaner>(),
					new ServiceManagerDateTimeProvider(),
					new Mock<IErrorReporterProxy>().Object,
					Mock.Of<IHostRegistrySettings>(),
					string.Empty);
				runner
					.Protected()
					.Setup<bool>("RunCore", ItExpr.IsAny<ITaskRunRequest>())
					.Returns(true);

				var request = Mock.Of<ITaskRunRequest>(
					x => x.Task == Mock.Of<IRunnableServiceTask>(
						y => y.Code == "LWM"));

				runner.Object.Run(request);

				// Act
				runner.Object.KillInBackground();

				// Assert
				NUnit.Framework.Assert.That(runner.Object.ExpectingTaskRunRequestCallback, Is.EqualTo(true));
			}

			[ExpectNoExceptions]
			public void TestKillInBackgroundDoesNotClearProcessUntilOnExitCallbackIsRaised()
			{
				// Arrange
				var taskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				processMock
					.SetupGet(p => p.Id)
					.Returns(10);
				processMock
					.Setup(p => p.Start())
					.Returns(true);
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, new Mock<IErrorReporterProxy>().Object);

				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));
				serviceRunner.Run(taskRunRequestMock.Object);
				NUnit.Framework.Assert.That(serviceRunner.ProcessId, Is.EqualTo(10));

				// Act
				serviceRunner.KillInBackground();

				// Assert
				AsyncHelper.WaitAllActiveTasksForTest();
				NUnit.Framework.Assert.That(serviceRunner.ProcessId, Is.EqualTo(10));

				backgroundThreadActionQueueMock.Verify(r => r.Enqueue(It.IsAny<Action>()), Times.Never);
				processMock.Raise(p => p.Exited += null, processMock.Object, EventArgs.Empty);
				backgroundThreadActionQueueMock.Verify(r => r.Enqueue(It.IsAny<Action>()), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestKillDoesNotClearProcessUntilOnExitCallbackIsRaised()
			{
				// Arrange
				var taskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				processMock
					.SetupGet(p => p.Id)
					.Returns(10);
				processMock
					.Setup(p => p.Start())
					.Returns(true);
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, new Mock<IErrorReporterProxy>().Object);

				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));
				serviceRunner.Run(taskRunRequestMock.Object);
				NUnit.Framework.Assert.That(serviceRunner.ProcessId, Is.EqualTo(10));

				// Act
				serviceRunner.Kill();

				// Assert
				NUnit.Framework.Assert.That(serviceRunner.ProcessId, Is.EqualTo(10));

				backgroundThreadActionQueueMock.Verify(r => r.Enqueue(It.IsAny<Action>()), Times.Never);
				processMock.Raise(p => p.Exited += null, processMock.Object, EventArgs.Empty);
				backgroundThreadActionQueueMock.Verify(r => r.Enqueue(It.IsAny<Action>()), Times.Once);
			}
		}

		public class OnExitTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestOnExitCleansUpLocks()
			{
				// Arrange
				var serviceTaskLocks = new List<(int ProcessId, string Code)>();

				// Act
				TestOnExit((runner, data, logs) => { }, (runner, serviceTaskInfo) =>
				{
					// Arrange
					serviceTaskLocks.Add((runner.ProcessId, serviceTaskInfo.Code));
				});

				// Assert
				serviceTaskLocks.ForEach(lockInfo =>
				{
					serviceTaskLocksCleanerMock.Verify(x => x
						.ReleaseLocksFromServiceTask(lockInfo.ProcessId, lockInfo.Code), Times.Once);
				});
			}

			[ExpectNoExceptions]
			public void TestOnExitWithErrorLogsToTheRightTask()
			{
				TestOnExit((runner, data, logs) =>
				{
					NUnit.Framework.Assert.That(data.Count, Is.EqualTo(2));

					for (var i = 0; i < data.Count; ++i)
					{
						AssertNoExceptionThrown(() => data[i].runnableTaskMock.Verify(
							t => t.Log(It.IsAny<LogLevel>(), It.Is<string>(message => message.Contains("Runner StdErr [:(]"))),
							Times.Once));
						AssertNoExceptionThrown(() => data[i].runnableTaskMock.Verify(
							t => t.Log(It.IsAny<LogLevel>(), It.Is<string>(message => message.Contains($"Runner StdErr [{i - 1}]"))),
							Times.Once));
					}
				});
			}

			[ExpectNoExceptions]
			public void TestOnExitWithErrorReportsErrorToTheRightTask()
			{
				TestOnExit((runner, data, logs) =>
				{
					NUnit.Framework.Assert.That(data.Count, Is.EqualTo(2));

					AssertNoExceptionThrown(
						() => data[0].runnableTaskMock.Verify(
							task => task.HandleUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<ITaskRunRequest>(), It.IsAny<bool>(), It.IsAny<bool>()),
							Times.Once));
					AssertNoExceptionThrown(
						() => data[0].runnableTaskMock.Verify(
							task => task.HandleUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<ITaskRunRequest>(), true, true),
							Times.Once));
					NUnit.Framework.Assert.That(data[0].taskInfo.ErrorOnLastRun, Is.EqualTo(true));

					AssertNoExceptionThrown(
						() => data[1].runnableTaskMock.Verify(
							task => task.HandleUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<ITaskRunRequest>(), It.IsAny<bool>(), It.IsAny<bool>()),
							Times.Never));
					NUnit.Framework.Assert.That(data[1].taskInfo.ErrorOnLastRun, Is.EqualTo(false));
				});
			}

			[ExpectNoExceptions]
			public void TestOnExitWithErrorLogsToHostRightTasks()
			{
				TestOnExit((runner, data, logs) =>
				{
					NUnit.Framework.Assert.That(data.Count, Is.EqualTo(2));

					var hostLogs = logs
						.ToString()
						.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
						.Where(s => s.Contains("Warning|PID="))
						.Where(s => s.Contains($"Terminated with ExitCode={RunnerExitCode.ServiceTaskUnhandledException}"))
						.Select(s => s.Split(new[] { $"ExitCode={RunnerExitCode.ServiceTaskUnhandledException}. " }, StringSplitOptions.RemoveEmptyEntries)[1])
						.ToList();

					NUnit.Framework.Assert.That(hostLogs, Is.EquivalentTo(data.Take(1).Select(tuple => $"{tuple.taskInfo.Code} - {tuple.taskInfo.Description}")));
				});
			}

			[ExpectNoExceptions]
			public void TestClearsFlagExpectingTaskRunRequestCallback()
			{
				// Arrange
				var result = true;

				// Act
				TestOnExit(
					(runner, data, logs) =>
					{
						result = runner.ExpectingTaskRunRequestCallback;
					},
					(runner, serviceTaskInfo) =>
					{
					});

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(false));
			}

			void TestOnExit(
				Action<ProcessServiceRunnerCoreWithDynamicAssembly, List<(ServiceTaskInfo taskInfo, Mock<IRunnableServiceTask> runnableTaskMock, Mock<IHostLogger> loggerMock)>, StringBuilder> assertAction,
				Action<IServiceRunner, ServiceTaskInfo> serviceTaskStartedAction = null)
			{
				// Arrange
				const string code = @"
using System;

namespace HelloWorld
{
	class HelloWorldClass
	{
		[STAThread]
		static int Main(string[] args)
		{
			Console.Error.WriteLine("":("");
			Console.Error.WriteLine(string.Join(""\n"", args));
			var exitCode = Convert.ToInt32(args[0]);
			return exitCode;
		}
	}
}";

				using (var tempDir = new TempDirectory())
				using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
				{
					var hostLogs = new StringBuilder();
					var hostLoggerMock = new Mock<IHostLogger>();
					hostLoggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<string>())).Callback((LogLevel logLevel, string message) =>
					{
						hostLogs.AppendLine($"{logLevel}|{message}");
					});

					var data = Enumerable
						.Range(0, 2)
						.Select(i => new ServiceTaskInfo(new HostedServiceAttribute
						{
							TypeName = "foo",
							Code = $"Code{i:D3}",
							Description = $"Description {i:D5}",
							ProcessArguments = $"{i - 1}",
						}))
						.Select(taskInfo => (
							taskInfo,
							hostLoggerMock
						))
						.Select(tuple =>
						{
							var runnableTaskMock = new Mock<IRunnableServiceTask>();
							runnableTaskMock
								.SetupGet(task => task.Info)
								.Returns(tuple.taskInfo);
							runnableTaskMock
								.SetupGet(task => task.Code)
								.Returns(tuple.taskInfo.Code);
							return (
								tuple.taskInfo,
								runnableTaskMock,
								tuple.hostLoggerMock);
						})
						.ToList();

					var delayedActions = new List<Action>();
					var finishedProcesses = new HashSet<ServiceTaskInfo>();

					using var processFactory = new WeirdProcessFactory((s, e) =>
					{
						var process = data
							.Select(tuple => tuple.taskInfo)
							.FirstOrDefault(info => !finishedProcesses.Contains(info));
						if (process != null)
						{
							finishedProcesses.Add(process);
						}
					});

					var actionQueueMock = new Mock<IBackgroundThreadActionQueue>(MockBehavior.Strict);
					actionQueueMock
						.Setup(queue => queue.Enqueue(It.IsAny<Action>()))
						.Callback<Action>(action => delayedActions.Add(action));

					using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, code, actionQueueMock.Object, hostLoggerMock.Object, serviceTaskLocksCleanerMock.Object, dateTimeProviderMock.Object, processFactory, hostRegistry: Mock.Of<IHostRegistrySettings>(o => o.RunnerProcessPriorityValue == ProcessPriorityClass.BelowNormal)))
					{
						var exitedRunners = 0;
						runner.Exited += (s, e) => Interlocked.Increment(ref exitedRunners);

						// Act
						StartServiceTaskAndWaitForExit(runner, data[0].runnableTaskMock.Object, data[0].taskInfo);
						StartServiceTaskAndWaitForExit(runner, data[1].runnableTaskMock.Object, data[1].taskInfo);

						foreach (var value in delayedActions)
						{
							actionQueue.Enqueue(value);
						}

						var stopwatch = Stopwatch.StartNew();
						while (exitedRunners < data.Count && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
						{
							actionQueue.WaitForEnqueue(TimeSpan.FromSeconds(1), CancellationToken.None);
							actionQueue.InvokeActions();
						}

						// Assert
						NUnit.Framework.Assert.Multiple(() =>
						{
							NUnit.Framework.Assert.That(exitedRunners, Is.EqualTo(data.Count));
							assertAction(runner, data, hostLogs);
						});
					}

					void StartServiceTaskAndWaitForExit(IServiceRunner runner, IRunnableServiceTask runnableServiceTask, ServiceTaskInfo serviceTaskInfo)
					{
						runner.Run(new ScheduledTaskRunRequest(runnableServiceTask));
						serviceTaskStartedAction?.Invoke(runner, serviceTaskInfo);

						var stopwatch = Stopwatch.StartNew();
						while (stopwatch.Elapsed < TimeSpan.FromSeconds(5) && !finishedProcesses.Contains(serviceTaskInfo))
						{
							actionQueue.WaitForEnqueue(TimeSpan.FromSeconds(1), CancellationToken.None);
							actionQueue.InvokeActions();
						}

						NUnit.Framework.Assert.That(finishedProcesses.Contains(serviceTaskInfo), Is.EqualTo(true));
					}
				}
			}

			[ExpectNoExceptions]
			public void TestOnExitWithWithIncompleteRequestWillRequeueTaskRequest_DirectTaskRequest()
			{
				AssertOnExitWithWithIncompleteRequestWillRequeueTaskRequest(new Mock<IDirectTaskRunRequest>(), true);
			}

			void AssertOnExitWithWithIncompleteRequestWillRequeueTaskRequest<T>(Mock<T> taskRunRequestMock, bool expectedRetry)
				where T : class, ITaskRunRequest
			{
				// Arrange
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, new Mock<IErrorReporterProxy>().Object);
				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));

				backgroundThreadActionQueueMock
					.Setup(q => q.Enqueue(It.IsAny<Action>()))
					.Callback((Action action) => action());

				processMock.SetupAdd(m => m.Exited += It.IsAny<EventHandler>());
				processMock.SetupRemove(m => m.Exited -= It.IsAny<EventHandler>());
				processMock.Setup(p => p.Start()).Returns(true);

				// Act
				serviceRunner.Run(taskRunRequestMock.Object);
				processMock.Raise(p => p.Exited += null, processMock.Object, EventArgs.Empty);

				// Assert
				taskRunRequestMock.Verify(r => r.OnUnableToRun(UnableToRunReason.HostDidNotReceiveRunnerProcessingFinishedCallback, expectedRetry, true), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestOnExitWithServiceTaskLockCouldNotBeReleasedDoesNotRequeueTask_DirectTaskRequest()
			{
				AssertOnExitWithServiceTaskLockCouldNotBeReleasedDoesNotRequeueTask(new Mock<IDirectTaskRunRequest>(), 1, Guid.Parse("A8C64E91-CE5C-48C3-A6FF-EA13F60DBE84"));
			}

			void AssertOnExitWithServiceTaskLockCouldNotBeReleasedDoesNotRequeueTask<T>(Mock<T> taskRunRequestMock, int processId, Guid guid)
				where T : class, ITaskRunRequest
			{
				// Arrange
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, new Mock<IErrorReporterProxy>().Object);
				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskRunRequestMock
					.SetupGet(t => t.Id)
					.Returns(guid);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));

				backgroundThreadActionQueueMock
					.Setup(q => q.Enqueue(It.IsAny<Action>()))
					.Callback((Action action) => action());

				processMock.SetupAdd(m => m.Exited += It.IsAny<EventHandler>());
				processMock.SetupRemove(m => m.Exited -= It.IsAny<EventHandler>());
				processMock.Setup(p => p.Start()).Returns(true);
				processMock.SetupGet(p => p.Id).Returns(processId);
				processMock.SetupGet(p => p.ExitCode).Returns(RunnerExitCode.ServiceTaskLockNotReleased);

				// Act
				serviceRunner.Run(taskRunRequestMock.Object);
				processMock.Raise(p => p.Exited += null, processMock.Object, EventArgs.Empty);

				// Assert
				taskRunRequestMock.Verify(r => r.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
				hostLoggerMock.Verify(l => l.Log(LogLevel.Warning, $"PID={processId}: Terminated with ExitCode=ServiceTaskLockNotReleased. ASD - "), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestErrorReportedIfExitRaisedFromNotAProcess_Object()
			{
				AssertErrorReportedIfExitRaisedFromNotAProcess(new object(), nameof(ProcessServiceRunnerCore), $"Exit raised with sender of type [{typeof(object)}] must be [{typeof(IProcess)}]");
			}

			[ExpectNoExceptions]
			public void TestErrorReportedIfExitRaisedFromNotAProcess_null()
			{
				AssertErrorReportedIfExitRaisedFromNotAProcess(null, nameof(ProcessServiceRunnerCore), $"Exit raised with sender of type [] must be [{typeof(IProcess)}]");
			}

			void AssertErrorReportedIfExitRaisedFromNotAProcess(object sender, string expectedMessage, string expectedErrorMessage)
			{
				// Arrange
				var taskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, errorReporterProxyMock.Object);
				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));

				backgroundThreadActionQueueMock
					.Setup(q => q.Enqueue(It.IsAny<Action>()))
					.Callback((Action action) => action());

				processMock.SetupAdd(m => m.Exited += It.IsAny<EventHandler>());
				processMock.SetupRemove(m => m.Exited -= It.IsAny<EventHandler>());
				processMock.Setup(p => p.Start()).Returns(true);

				// Act
				serviceRunner.Run(taskRunRequestMock.Object);
				processMock.Raise(p => p.Exited += null, sender, EventArgs.Empty);

				// Assert
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(expectedMessage, It.Is<InvalidOperationException>(e => e.Message == expectedErrorMessage)), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestStartProcessThrowsExceptionIfProcessIsNotNull()
			{
				// Arrange
				var taskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var processMock = new Mock<IProcess>();
				processMock
					.SetupGet(p => p.Id)
					.Returns(10);
				processMock
					.Setup(p => p.Start())
					.Returns(true);
				var processFactoryMock = new Mock<IProcessFactory>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var locksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				processFactoryMock
					.Setup(f => f.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
				using var serviceRunner = new BasicProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, hostLoggerMock.Object, locksCleanerMock.Object, dateTimeProviderMock.Object, errorReporterProxyMock.Object);

				var taskMock = new Mock<IRunnableServiceTask>();
				taskRunRequestMock
					.SetupGet(r => r.Task)
					.Returns(taskMock.Object);
				taskMock
					.SetupGet(t => t.Info)
					.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.Code == "ASD")));

				// Act
				var firstRunResult = serviceRunner.Run(taskRunRequestMock.Object);
				var secondRunResult = serviceRunner.Run(taskRunRequestMock.Object);

				// Assert
				NUnit.Framework.Assert.That(firstRunResult, Is.EqualTo(true));
				NUnit.Framework.Assert.That(secondRunResult, Is.EqualTo(false));
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce("Attempted to start a process with a process already running, PID:[10]", It.IsAny<Exception>()), Times.Once);
			}

			class WeirdProcessFactory : IProcessFactory, IDisposable
			{
				public WeirdProcessFactory(Action<object, EventArgs> onExitAction)
				{
					this.onExitAction = onExitAction;
				}

				public IProcess Create(ProcessStartInfo startInfo, bool enableRaisingEvents = false, ProcessPriorityClass priority = ProcessPriorityClass.Normal)
				{
					process = new Process()
					{
						StartInfo = startInfo,
						EnableRaisingEvents = enableRaisingEvents,
					};
					var result = new ProcessAdapter(process, priority);
					result.Exited += (sender, args) => onExitAction(sender, args);
					return result;
				}

				public void Dispose()
				{
					process?.Dispose();
				}

				readonly Action<object, EventArgs> onExitAction;
				Process process;
			}

			protected override void SetUp()
			{
				base.SetUp();
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);
				serviceTaskLocksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
			}

			Mock<IServiceTaskLocksCleaner> serviceTaskLocksCleanerMock;
			Mock<IDateTimeProvider> dateTimeProviderMock;
		}

		public class ProcessRunningTest : TestCaseWithFactory
		{
			[ExpectNoExceptions]
			public void TestProcessRunning()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(true);
					Test(false);
				});

				void Test(bool value)
				{
					// Arrange
					using (var runner = new ProcessServiceRunnerCoreForTesting(new Mock<IBackgroundThreadActionQueue>().Object, "poolKey", loggerMock.Object, dateTimeProviderMock.Object))
					{
						var processMock = new Mock<IProcess>();
						processMock
							.SetupGet(process => process.HasExited)
							.Returns(value);
						runner.Process = processMock.Object;

						// Act
						var result = runner.ProcessRunning_Exposed;

						// Assert
						NUnit.Framework.Assert.That(result, Is.EqualTo(!value));
					}
				}
			}

			[ExpectNoExceptions]
			public void TestProcessRunningObjectDisposed()
			{
				// Arrange
				using (var runner = new ProcessServiceRunnerCoreForTesting(new Mock<IBackgroundThreadActionQueue>().Object, "poolKey", loggerMock.Object, dateTimeProviderMock.Object))
				{
					var processMock = new Mock<IProcess>();
					processMock
						.SetupGet(process => process.HasExited)
						.Throws(new ObjectDisposedException("process"));
					runner.Process = processMock.Object;

					// Act
					var result = runner.ProcessRunning_Exposed;

					// Assert
					NUnit.Framework.Assert.That(result, Is.EqualTo(false));
				}
			}

			protected override void SetUp()
			{
				base.SetUp();
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);
				loggerMock = new Mock<IHostLogger>();
			}

			Mock<IHostLogger> loggerMock;
			Mock<IDateTimeProvider> dateTimeProviderMock;
		}

		public class ExceptionTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				const string taskCode = "ZZZ";
				hostLoggerMock = new Mock<IHostLogger>();
				var taskQueueMock = new Mock<ITaskQueue>();
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new List<IRunnableServiceTask>());

				var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(config => config.TypeName)
					.Returns("foo");
				hostedServiceConfigMock
					.SetupGet(config => config.Code)
					.Returns(taskCode);
				hostedServiceConfigMock
					.SetupGet(config => config.Description)
					.Returns("Description");
				hostedServiceConfigMock
					.SetupGet(config => config.TypeAssemblyName)
					.Returns("foo.dll");
				hostedServiceConfigMock
					.SetupGet(config => config.ProcessArguments)
					.Returns("22 55");
				taskInfo = new ServiceTaskInfo(hostedServiceConfigMock.Object);

				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				runnableServiceTaskMock
					.SetupGet(task => task.Info)
					.Returns(taskInfo);
				runnableServiceTaskMock
					.SetupGet(task => task.Code)
					.Returns(taskCode);
				runnableServiceTaskMock.Setup(x => x.RecordLastRunError()).Callback(() =>
				{
					taskInfo.ErrorOnLastRun = true;
				});
				loggerMock = new Mock<ILogger>();
				runnableServiceTaskMock
					.Setup(task => task.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
					.Callback((LogLevel lvl, string msg) => loggerMock.Object.Log(lvl, msg));

				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);

				scheduledTaskRunRequestMock = new Mock<IScheduledTaskRunRequest>();
				scheduledTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);
				scheduledTaskRunRequestMock
						.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
						.Returns<LogMessageStage, object[]>((x, _) => x.ToString());

				var controllerService = new Mock<IControllerService>();
				controller = new ControllerForTest(service: controllerService.Object, upgrader: Mock.Of<IControllerUpgrade>())
				{
					TaskQueue = taskQueueMock,
				};
			}

			[ExpectNoExceptions]
			public void TestMinorExitCodeFailureTriggersErrorOnLastRun()
			{
				// Arrange
				using (var tempDir = new TempDirectory())
				using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, CodeWithMinorExitError, controller.ActionQueue, hostLoggerMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object))
				{
					// Act
					runner.Run(scheduledTaskRunRequestMock.Object);
					NUnit.Framework.Assert.That(controller.WaitForDispatchingThreadResumeRequest(TimeSpan.FromSeconds(5)), Is.True);
					controller.RunTaskDispatchingLoopExposed(() =>
					{
						NUnit.Framework.Assert.That(taskInfo.ErrorOnLastRun, Is.True);
					});

					// Assert
					NUnit.Framework.Assert.That(taskInfo.ErrorOnLastRun, Is.True);
				}
			}

			[ExpectNoExceptions]
			public void TestExceptionExitCodeTriggersErrorOnLastRun()
			{
				// Arrange
				using (var tempDir = new TempDirectory())
				using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, CodeWithServiceTaskUnhandledExceptionExitError, controller.ActionQueue, hostLoggerMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object))
				{
					// Act
					runner.Run(scheduledTaskRunRequestMock.Object);
					NUnit.Framework.Assert.That(controller.WaitForDispatchingThreadResumeRequest(TimeSpan.FromSeconds(5)), Is.True);
					controller.RunTaskDispatchingLoopExposed(() =>
					{
						NUnit.Framework.Assert.That(taskInfo.ErrorOnLastRun, Is.True);
					});

					// Assert
					NUnit.Framework.Assert.That(taskInfo.ErrorOnLastRun, Is.True);
				}
			}

			[ExpectNoExceptions]
			public void TestExceptionExitCodeFailureLogsException()
			{
				// Arrange
				using (var tempDir = new TempDirectory())
				using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, CodeWithServiceTaskUnhandledExceptionExitError, controller.ActionQueue, hostLoggerMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object))
				using (var processExitErrorIsLoggedEvent = new ManualResetEvent(false))
				{
					hostLoggerMock
						.Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.Is<string>(s => s.IndexOf("exception added", StringComparison.OrdinalIgnoreCase) >= 0)))
						.Callback(() => processExitErrorIsLoggedEvent.Set());

					// Act
					runner.Run(scheduledTaskRunRequestMock.Object);

					NUnit.Framework.Assert.That(controller.WaitForDispatchingThreadResumeRequest(TimeSpan.FromSeconds(5)), Is.True);
					controller.RunTaskDispatchingLoopExposed(() => NUnit.Framework.Assert.That(processExitErrorIsLoggedEvent.WaitOne(TimeSpan.FromSeconds(15)), Is.True));

					// Assert
					hostLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, "ZZZ:Description exception added."), Times.Once);
				}
			}

			Mock<IHostLogger> hostLoggerMock;
			Mock<IDateTimeProvider> dateTimeProviderMock;
			ServiceTaskInfo taskInfo;
			Mock<IScheduledTaskRunRequest> scheduledTaskRunRequestMock;
			ControllerForTest controller;
			Mock<ILogger> loggerMock;
		}

		const string ErrorCode = @"
using System;
using System.Threading;

namespace HelloWorld
{
	class HelloWorldClass
	{
		[STAThread]
		static void Main(string[] args)
		{
			Console.Error.WriteLine(""I am the gorkle-sporkle!"");
			Console.Error.WriteLine(""Multiple lines shouldn't be aggregated."");
		}
	}
}
";

		[ExpectNoExceptions]
		public void TestLogsStdErr()
		{
			using (var tempDir = new TempDirectory())
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, ErrorCode, actionQueue, loggerMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, hostRegistry: Mock.Of<IHostRegistrySettings>(o => o.RunnerProcessPriorityValue == ProcessPriorityClass.BelowNormal)))
			{
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll" };
				var taskInfo = new ServiceTaskInfo(config);
				var runnableTask = new Mock<IRunnableServiceTask>();
				runnableTask.Setup(rt => rt.Info).Returns(taskInfo);

				var exited = false;
				runner.Exited += (s, e) => exited = true;
				runner.Run(new ScheduledTaskRunRequest(runnableTask.Object));
				for (var i = 0; !exited && i < 3; i++)
				{
					actionQueue.WaitForEnqueue(TimeSpan.FromSeconds(5), CancellationToken.None);
					actionQueue.InvokeActions();
				}

				NUnit.Framework.Assert.Multiple(() =>
				{
					runnableTask.Verify(l => l.Log(LogLevel.Warning, It.Is<string>(message => message.Contains("Runner StdErr [I am the gorkle-sporkle!]"))));
					runnableTask.Verify(l => l.Log(LogLevel.Warning, It.Is<string>(message => message.Contains("Runner StdErr [Multiple lines shouldn't be aggregated.]"))));
				});
			}
		}

		static readonly string CodeWithServiceTaskUnhandledExceptionExitError = $@"
using System;
using System.Threading;

namespace HelloWorld
{{
	class HelloWorldClass
	{{
		[STAThread]
		static int Main(string[] args)
		{{
			return {(int)RunnerExitCode.ServiceTaskUnhandledException};
		}}
	}}
}}
";

		static readonly string CodeWithMinorExitError = $@"
using System;
using System.Threading;

namespace HelloWorld
{{
	class HelloWorldClass
	{{
		[STAThread]
		static int Main(string[] args)
		{{
			return {(int)RunnerExitCode.ServiceTaskCorruptedTheEnvironment};
		}}
	}}
}}
";

		[ExpectNoExceptions]
		public void TestErrorExitCodeWithDirectRequestCausesRetry()
		{
			AssertErrorExitCodeCausesRetry(new DirectTaskRunRequest(new Mock<IRunnableServiceTask>().Object), expectRetry: true, codeWithExitError: CodeWithServiceTaskUnhandledExceptionExitError);
		}

		[ExpectNoExceptions]
		public void TestErrorExitCodeWithScheduledRequestCausesRetry()
		{
			AssertErrorExitCodeCausesRetry(new ScheduledTaskRunRequest(new Mock<IRunnableServiceTask>().Object), expectRetry: true, codeWithExitError: CodeWithServiceTaskUnhandledExceptionExitError);
		}

		static void AssertErrorExitCodeCausesRetry(ITaskRunRequest request, bool expectRetry, string codeWithExitError)
		{
			var dateProviderMock = new Mock<IDateTimeProvider>();
			dateProviderMock
				.Setup(p => p.CurrentDateTimeUtc)
				.Returns(() => DateTime.UtcNow);
			var controllerService = new Mock<IControllerService>();
			using var controller = new ControllerForTest(service: controllerService.Object, upgrader: Mock.Of<IControllerUpgrade>())
			{
				TaskQueue = new Mock<ITaskQueue>(),
			};
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());

			using (var tempDir = new TempDirectory())
			using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, codeWithExitError, controller.ActionQueue, new Mock<IHostLogger>().Object, Mock.Of<IServiceTaskLocksCleaner>(), dateProviderMock.Object))
			{
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "ZZZ", TypeAssemblyName = "foo.dll" };
				config.ProcessArguments = "22 55";
				var taskInfo = new ServiceTaskInfo(config);
				Mock.Get(request.Task).Setup(rt => rt.Info).Returns(taskInfo);
				Mock.Get(request.Task).Setup(rt => rt.Code).Returns(taskInfo.Code);

				runner.Run(request);
				NUnit.Framework.Assert.That(controller.WaitForDispatchingThreadResumeRequest(TimeSpan.FromSeconds(5)), Is.True);
				var assertionsAction = new Action(() =>
				{
					NUnit.Framework.Assert.That(taskInfo.ErrorOnLastRun, Is.True);
					Mock.Get(request.Task).Verify(t => t.HandleUnableToRun(UnableToRunReason.RunnerProcessExited, request, expectRetry, true), Times.AtLeastOnce);
				});
				controller.RunTaskDispatchingLoopExposed(assertionsAction);
			}
		}

		[ExpectNoExceptions]
		public void TestSqlExceptionDuringRetryOnErrorExitCodeIsHandled()
		{
			AssertExceptionDuringRetryOnErrorExitCodeIsHandled(SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired"), errorLogged: true);
		}

		[ExpectNoExceptions]
		public void TestDatabaseUpgradingExceptionDuringRetryOnErrorExitCodeIsHandled()
		{
			AssertExceptionDuringRetryOnErrorExitCodeIsHandled(new DatabaseUpgradeInProgressException(), errorLogged: false);
		}

		void AssertExceptionDuringRetryOnErrorExitCodeIsHandled(Exception ex, bool errorLogged)
		{
			var controllerService = new Mock<IControllerService>();
			var delayProvider = new Mock<IAsyncDelayProvider>();
			delayProvider.Setup(dp => dp.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			using var controller = new ControllerForTest(service: controllerService.Object, upgrader: Mock.Of<IControllerUpgrade>(), delayProvider: delayProvider.Object, errorReporterProxy: errorReporterProxyMock.Object)
			{
				TaskQueue = new Mock<ITaskQueue>(),
			};
			controller.TaskQueue.Setup(tq => tq.EnqueueTask(It.IsAny<ITaskRunRequest>())).Throws(ex);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());

			using (var tempDir = new TempDirectory())
			using (var runner = new ProcessServiceRunnerCoreWithDynamicAssembly(tempDir, CodeWithServiceTaskUnhandledExceptionExitError, controller.ActionQueue, loggerMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object))
			{
				var runnableTask = TaskSchedulerTest.CreateTask("ZZZ", Factory, false, controller.ActionQueue, controller.TaskQueue.Object);
				runner.Run(new DirectTaskRunRequest(runnableTask));
				NUnit.Framework.Assert.That(controller.WaitForDispatchingThreadResumeRequest(TimeSpan.FromSeconds(5)), Is.True);
				var assertionsAction = new Action(() =>
				{
					NUnit.Framework.Assert.That(runnableTask.Info.ErrorOnLastRun, Is.True);
					controller.TaskQueue.Verify(tq => tq.EnqueueTask(It.IsAny<ITaskRunRequest>()), Times.AtLeastOnce);
					if (errorLogged)
					{
						errorReporterProxyMock.Verify(x => x.ReportOnce("Controller Dispatching Loop", ex));
					}
					else
					{
						errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					}
				});
				controller.RunTaskDispatchingLoopExposed(assertionsAction);
			}
		}

		[ExpectNoExceptions]
		public void TestRun_ReturnsPassOrFail()
		{
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerCoreForTesting(actionQueue, "Key", loggerMock.Object, dateTimeProviderMock.Object))
			{
				runner.StubOutRunCore = true;

				runner.StubbedRunCoreResult = true;
				NUnit.Framework.Assert.That(runner.Run(Mock.Of<ITaskRunRequest>()), Is.EqualTo(true));

				runner.StubbedRunCoreResult = false;
				NUnit.Framework.Assert.That(runner.Run(Mock.Of<ITaskRunRequest>()), Is.EqualTo(false));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<IHostLogger>();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock
				.Setup(p => p.CurrentDateTimeUtc)
				.Returns(() => DateTime.UtcNow);
			actionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
		}

		protected override void TearDown()
		{
			actionQueue?.Dispose();

			base.TearDown();
		}

		Mock<IHostLogger> loggerMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;
		BackgroundThreadActionQueue actionQueue;
	}

	class BasicProcessServiceRunner : ProcessServiceRunnerCore
	{
		public BasicProcessServiceRunner(
			ITaskScheduler taskScheduler,
			IBackgroundThreadActionQueue actionQueue,
			IProcessFactory processFactory,
			IHostLogger hostLogger,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner,
			IDateTimeProvider dateTimeProvider,
			IErrorReporterProxy errorReporterProxy)
			: base(
				taskScheduler,
				actionQueue,
				processFactory,
				hostLogger,
				serviceTaskLocksCleaner,
				dateTimeProvider,
				errorReporterProxy, 
				Mock.Of<IHostRegistrySettings>(),
				string.Empty)
		{
			startInfo = new ProcessStartInfo();
		}

		protected override bool RunCore(ITaskRunRequest runRequest)
		{
			return StartProcess(runRequest);
		}

		protected override void StopCore()
		{
		}

		public override bool TaskRunning => false;

		protected override ProcessStartInfo GetProcessStartInfo(IServiceTaskInfo info) => startInfo;

		readonly ProcessStartInfo startInfo;
	}

	class ProcessServiceRunnerCoreForTesting : ProcessServiceRunnerCore
	{
		public ProcessServiceRunnerCoreForTesting(IBackgroundThreadActionQueue actionQueue, string poolKey, IHostLogger hostLogger, IDateTimeProvider dateTimeProvider)
			: base(null, actionQueue, null, hostLogger, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProvider, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), string.Empty)
		{
		}

		protected override void StopCore()
		{
		}

		protected override bool RunCore(ITaskRunRequest runRequest)
		{
			if (StubOutRunCore)
			{
				return StubbedRunCoreResult;
			}
			return StartProcess(runRequest);
		}

		protected override ProcessStartInfo GetProcessStartInfo(IServiceTaskInfo info)
		{
			var pinfo = new ProcessStartInfo();
			pinfo.FileName = @"calc.exe";
			return pinfo;
		}

		public override bool TaskRunning { get; }

		public bool StubOutRunCore { get; set; }

		public bool StubbedRunCoreResult { get; set; }

		public IProcess Process
		{
			get => process;
			set => process = value;
		}

		public bool ProcessRunning_Exposed => ProcessRunning;
	}

	class ProcessServiceRunnerCoreWithDynamicAssembly : ProcessServiceRunnerCore
	{
		public ProcessServiceRunnerCoreWithDynamicAssembly(TempDirectory dir, string assemblyCode, IBackgroundThreadActionQueue actionQueue, IHostLogger hostLogger, IServiceTaskLocksCleaner serviceTaskLocksCleaner, IDateTimeProvider dateTimeProvider, IProcessFactory processFactory = null, IHostRegistrySettings hostRegistry = null)
			: base(new Mock<ITaskScheduler>().Object, actionQueue, processFactory ?? new ProcessWrapperFactory(), hostLogger, serviceTaskLocksCleaner, dateTimeProvider, Mock.Of<IErrorReporterProxy>(), hostRegistry ?? Mock.Of<IHostRegistrySettings>(), string.Empty)
		{
			using (var provider = new CSharpCodeProvider())
			{
				var applicationName = "crasha.exe";
				output = Path.Combine(dir.DirectoryName, applicationName);
				GenerateAssembly(dir.DirectoryName, applicationName, assemblyCode);
			}

			static string GenerateAssembly(string dirPath, string applicationName, string code)
			{
				var filePath = Path.Combine(dirPath, applicationName);

				var systemDllLocation = Path.GetDirectoryName(typeof(object).Assembly.Location);

				var systemDlls = new[]
				{
					"mscorlib.dll",
					"System.Runtime.dll",
					"System.Console.dll",
					"System.Core.dll",
					"System.Linq.Expressions.dll",
				}.Select(dll => MetadataReference.CreateFromFile(Path.Combine(systemDllLocation, dll))).ToArray();
				MetadataReference runtimeDll = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

				var compilation = CSharpCompilation
					.Create
					(
						assemblyName: applicationName,
						syntaxTrees: new[]
						{
							CSharpSyntaxTree.ParseText(code),
						},
						references:
						[
							.. systemDlls,
							runtimeDll
						]
					)
					.WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));

				var result = compilation.Emit(filePath);
#if NET
				File.WriteAllText(Path.ChangeExtension(filePath, "runtimeconfig.json"), GenerateRuntimeConfig());
#endif

				return filePath;
			}

#if NET
			static string GenerateRuntimeConfig()
			{
				var runtimeConfig = new RuntimeConfig()
				{
					RuntimeOptions = new()
					{
						Tfm = $"net{RuntimeInformation.FrameworkDescription.Split(" ")[1][..3]}",
						Framework = new()
						{
							Name = "Microsoft.NETCore.App",
							Version = RuntimeInformation.FrameworkDescription.Replace(".NET ", "")
						}
					}
				};
				var options = new JsonSerializerOptions()
				{
					WriteIndented = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				};
				return JsonSerializer.Serialize(runtimeConfig, options);
			}
#endif
		}
		readonly string output;

		protected override void StopCore()
		{
		}

		protected override bool RunCore(ITaskRunRequest runRequest)
		{
			var result = StartProcess(runRequest);
			ExpectingTaskRunRequestCallback = false;
			return result;
		}

		protected override ProcessStartInfo GetProcessStartInfo(IServiceTaskInfo info)
		{
#if NET
			var processInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"{output} {info.HostedServiceAttribute.ProcessArguments}",
				UseShellExecute = true,
			};
#else
			var processInfo = new ProcessStartInfo
			{
				FileName = output,
				Arguments = info.HostedServiceAttribute.ProcessArguments,
			};
#endif
			return processInfo;
		}

		protected override void DisposeCore(bool disposing)
		{
			try
			{
				process?.WaitForExit(10000);
			}
			finally
			{
				base.DisposeCore(disposing);
			}
		}

		public override bool TaskRunning { get; }
	}
#if NET
	class RuntimeConfig
	{
		public RuntimeOptions RuntimeOptions { get; set; } = new();
	}

	class RuntimeOptions
	{
		public string Tfm { get; set; } = string.Empty;
		public Framework Framework { get; set; } = new();
	}

	class Framework
	{
		public string Name { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
	}
#endif
}
