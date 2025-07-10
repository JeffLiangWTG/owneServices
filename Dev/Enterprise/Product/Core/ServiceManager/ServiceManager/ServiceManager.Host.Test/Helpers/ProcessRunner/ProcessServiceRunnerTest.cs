using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Host.Testing.Helpers;
using Enterprise.ServiceManager.Shared;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW1.Test;
using ServiceManagerProto;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ServiceManagerConstants))]

namespace Enterprise.ServiceManager.Host.Testing
{
	class ProcessServiceRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIdleResponse()
		{
			// Arrange
			using var processBackgroundTasks = new ManualResetEvent(false);
			var backgroundTasks = new List<Task>();
			const string taskCode = "ABC";
			var response = new ServiceTaskRunResponse
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.Idle,
				TaskCode = taskCode,
				TaskId = "1",
			};
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => backgroundTasks.Add(Task.Run(() =>
				{
					processBackgroundTasks.WaitOne();
					x.Invoke();
				})));

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, ProcessServiceRunnerForTesting.CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false), "state must be updated independently of background thread action queue");
			}

			processBackgroundTasks.Set();
			Task.WaitAll(backgroundTasks.ToArray());
			taskSchedulerMock.Verify(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()), Times.Never);
			Mock.Get(nudgeRequest).Verify(x => x.OnSuccessfulRun(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueResponse()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.Reenqueue,
					TaskCode = "ABC",
					TaskId = "1",
				},
				null);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueResponseServiceTaskLockFailure()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.Reenqueue,
					TaskCode = "ABC",
					TaskId = "1",
					FailureReason = FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock,
				},
				UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueResponseGroupLockFailure()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.Reenqueue,
					TaskCode = "ABC",
					TaskId = "1",
					FailureReason = FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock,
				},
				UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock);
		}
		[ExpectNoExceptions]
		public void TestSetRetryTimeResponse()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.ScheduleNextRunTime,
					TaskCode = "ABC",
					TaskId = "1",
					NextRunTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(5)),
				},
				null);
		}

		[ExpectNoExceptions]
		public void TestSetRetryTimeResponseWithServiceTaskLockFailureReason()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.ScheduleNextRunTime,
					TaskCode = "ABC",
					TaskId = "1",
					NextRunTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(5)),
					FailureReason = FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock,
				},
				UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock);
		}

		[ExpectNoExceptions]
		public void TestSetRetryTimeResponseWithMutualGroupLockFailureReason()
		{
			AssertReEnqueueResponse(
				"ABC",
				new ServiceTaskRunResponse()
				{
					Status = Status.ProcessFinished,
					Command = ResponseCommandType.ScheduleNextRunTime,
					TaskCode = "ABC",
					TaskId = "1",
					NextRunTime = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(5)),
					FailureReason = FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock,
				},
				UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock);
		}

		void AssertReEnqueueResponse(string taskCode, ServiceTaskRunResponse response, UnableToRunReason? expectedUnableToRunReason)
		{
			// Arrange
			using var processBackgroundTasks = new ManualResetEvent(false);
			var backgroundTasks = new List<Task>();
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => backgroundTasks.Add(AsyncHelper.RunTask(() =>
					{
						processBackgroundTasks.WaitOne();
						x.Invoke();
					},
					"background processing")));

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false), "state must be updated independently of background thread action queue");
				processBackgroundTasks.Set();
				Task.WaitAll(backgroundTasks.ToArray());
			}

			taskSchedulerMock.Verify(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()), Times.Once);
			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.Is<UnableToRunReason>(rr => rr == expectedUnableToRunReason.GetValueOrDefault()), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(expectedUnableToRunReason.HasValue ? 1 : 0));
		}

		[ExpectNoExceptions]
		public void TestRunnerExitingResponseWithoutCancellation() => AssertRunnerExitingResponse("ABC", false);
		[ExpectNoExceptions]
		public void TestRunnerExitingResponseWithCancellation() => AssertRunnerExitingResponse("ABC", true, UnableToRunReason.RunnerWasCancelled);

		void AssertRunnerExitingResponse(string taskCode, bool cancelled, UnableToRunReason? expectedUnableToRunReason = null)
		{
			// Arrange
			using var processBackgroundTasks = new ManualResetEvent(false);
			var backgroundTasks = new List<Task>();
			var response = cancelled
				? new ServiceTaskRunResponse() { Status = Status.RunnerExiting, FailureReason = FailureReasonType.RunnerWasCancelled }
				: new ServiceTaskRunResponse() { Status = Status.RunnerExiting, };
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => backgroundTasks.Add(Task.Run(() =>
				{
					processBackgroundTasks.WaitOne();
					x.Invoke();
				})));

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(!cancelled), "state must be updated independently of background thread action queue");
			}

			processBackgroundTasks.Set();
			Task.WaitAll(backgroundTasks.ToArray());
			taskSchedulerMock.Verify(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()), Times.Never);
			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.Is<UnableToRunReason>(rr => rr == expectedUnableToRunReason.GetValueOrDefault()), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(expectedUnableToRunReason.HasValue ? 1 : 0));
		}

		[ExpectNoExceptions]
		public void TestRunnerExitingResponseClosesGrpcStreamInTimelyManner()
		{
			// Arrange
			var response = new ServiceTaskRunResponse() { Status = Status.RunnerExiting, };
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == "ABC");
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			// Do nothing with the action queue, we must close the stream without it
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
			}

			runnerCommandQueueProvider.Verify(p => p.Close(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestIdleResponseWithDifferentId()
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var response = new ServiceTaskRunResponse
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.Idle,
				TaskCode = taskCode,
				TaskId = "1",
			};
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
			}

			taskSchedulerMock.Verify(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()), Times.Never);
			Mock.Get(nudgeRequest).Verify(x => x.OnSuccessfulRun(), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueResponseWithDifferentId()
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var response = new ServiceTaskRunResponse()
			{
				Status = Status.ProcessFinished, Command = ResponseCommandType.Reenqueue, TaskCode = taskCode, TaskId = "1",
			};

			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
			}

			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueResponseWithDifferentIdServiceTaskLockFailureReason() =>
			AssertReEnqueueResponseWithDifferentIdFailureReason(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock);

		[ExpectNoExceptions]
		public void TestReEnqueueResponseWithDifferentIdMutualGroupLockFailureReason() =>
			AssertReEnqueueResponseWithDifferentIdFailureReason(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock);

		void AssertReEnqueueResponseWithDifferentIdFailureReason(UnableToRunReason failureReason)
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var response = new ServiceTaskRunResponse()
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.Reenqueue,
				TaskCode = taskCode,
				TaskId = "1",
				FailureReason = UnableToRunReasonHelper.ConvertToFailureReasonType(failureReason),
			};
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
			}

			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestTaskRunRequestCompletedWithIdleResponse_ShouldInvokeEvent() => AssertInvokeTaskRunRequestCompletedAfterProcessFinished(new ServiceTaskRunResponse
		{
			Status = Status.ProcessFinished,
			Command = ResponseCommandType.Idle,
			TaskCode = "ABC",
			TaskId = "1",
		});

		[ExpectNoExceptions]
		public void TestTaskRunRequestCompletedWithReEnqueueResponse_ShouldInvokeEvent() => AssertInvokeTaskRunRequestCompletedAfterProcessFinished(new ServiceTaskRunResponse
		{
			Status = Status.ProcessFinished,
			Command = ResponseCommandType.Reenqueue,
			TaskCode = "ABC",
			TaskId = "1",
		});

		[ExpectNoExceptions]
		public void TestTaskRunRequestCompletedWithScheduleNextRunTimeResponse_ShouldInvokeEvent() => AssertInvokeTaskRunRequestCompletedAfterProcessFinished(new ServiceTaskRunResponse
		{
			Status = Status.ProcessFinished,
			Command = ResponseCommandType.ScheduleNextRunTime,
			TaskCode = "ABC",
			TaskId = "1",
		});

		[ExpectNoExceptions]
		public void TestTaskRunRequestCompletedWithFailureReasonResponse_ShouldInvokeEvent() => AssertInvokeTaskRunRequestCompletedAfterProcessFinished(new ServiceTaskRunResponse
		{
			Status = Status.RunnerExiting,
			FailureReason = FailureReasonType.RunnerWasCancelled,
			TaskCode = "ABC",
			TaskId = "1",
		});

		void AssertInvokeTaskRunRequestCompletedAfterProcessFinished(ServiceTaskRunResponse response)
		{
			// Arrange
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == response.TaskCode);

			IDirectTaskRunRequest taskRunRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(taskRunRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				var onTaskRunRequestCompletedHandler = new Mock<EventHandler>();
				runner.TaskRunRequestCompleted += onTaskRunRequestCompletedHandler.Object;

				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.NewGuid()));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
				onTaskRunRequestCompletedHandler.Verify(handler => handler(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestSetRetryTimeResponseWithDifferentId()
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var nextRunTime = DateTimeOffset.Now.AddSeconds(5);
			var response = new ServiceTaskRunResponse()
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.ScheduleNextRunTime,
				TaskCode = taskCode,
				TaskId = "1",
				NextRunTime = Timestamp.FromDateTimeOffset(nextRunTime),
			};

			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
			}

			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestSetRetryTimeResponseWithDifferentIdServiceTaskLockFailureReason() =>
			AssertSetRetryTimeResponseWithDifferentIdFailureReason(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock);

		[ExpectNoExceptions]
		public void TestSetRetryTimeResponseWithDifferentIdMutualGroupLockFailureReason() =>
			AssertSetRetryTimeResponseWithDifferentIdFailureReason(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock);

		void AssertSetRetryTimeResponseWithDifferentIdFailureReason(UnableToRunReason failureReason)
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var nextRunTime = DateTimeOffset.Now.AddSeconds(5);
			var response = new ServiceTaskRunResponse()
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.ScheduleNextRunTime,
				TaskCode = taskCode,
				TaskId = "1",
				NextRunTime = Timestamp.FromDateTimeOffset(nextRunTime),
				FailureReason = UnableToRunReasonHelper.ConvertToFailureReasonType(failureReason),
			};

			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false));
			}

			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestReEnqueueNewRequestWhenDifferentId()
		{
			// Arrange
			const string taskCode = "ABC";
			var notEmptyGuid = Guid.NewGuid();
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var newCreatedRequest = Mock.Of<IDirectTaskRunRequest>();
			var taskScheduler = new Mock<ITaskScheduler>();
			taskScheduler
				.Setup(x => x.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(newCreatedRequest);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);

			using (var runner = new ProcessServiceRunner(taskScheduler.Object, actionQueue.Object, processFactory.Object, Mock.Of<IProcessRunnerRemotingServices>(), Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == notEmptyGuid));
				process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostCommunicationReenqueueTaskCommandPrefix}:{taskCode}:{Guid.Empty}"));
			}

			// Assert
			Mock.Get(newCreatedRequest).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestLastSeenNotIdleTimeUpdatedByRunnerResponse_Queued() => AssertLastSeenNotIdleTimeUpdatedByRunnerResponse(Status.Queued);
		[ExpectNoExceptions]
		public void TestLastSeenNotIdleTimeUpdatedByRunnerResponse_ProcessStarted() => AssertLastSeenNotIdleTimeUpdatedByRunnerResponse(Status.ProcessStarted);
		[ExpectNoExceptions]
		public void TestLastSeenNotIdleTimeUpdatedByRunnerResponse_ProcessingFinished() => AssertLastSeenNotIdleTimeUpdatedByRunnerResponse(Status.ProcessFinished);

		void AssertLastSeenNotIdleTimeUpdatedByRunnerResponse(Status status)
		{
			// Arrange
			const string taskCode = "ABC";
			var response = new ServiceTaskRunResponse
			{
				Status = status,
			};
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => x.Invoke());

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, ProcessServiceRunnerForTesting.CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			dateTimeProviderMock
				.Setup(p => p.CurrentDateTimeUtc)
				.Returns(new DateTime(2020, 1, 2, 3, 4, 5, 6));

			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.LastSeenNotIdle, NUnit.Framework.Is.EqualTo(new DateTime(2020, 1, 2, 3, 4, 5, 6)));
			}
		}

		[ExpectNoExceptions]
		public void TestStreamClosedExceptionRequeuesRequestAndDoesNotKillRunner()
		{
			// Arrange
			var exception = new HostGrpcIsClosedException(new Exception("oh no!"));
			using var processBackgroundTasks = new ManualResetEvent(false);
			var backgroundTasks = new List<Task>();
			const string taskCode = "ABC";
			var response = new ServiceTaskRunResponse
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.Idle,
				TaskCode = taskCode,
				TaskId = "1",
			};
			var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
			var nudgeRequest = Mock.Of<IDirectTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
			var actionQueue = new Mock<IBackgroundThreadActionQueue>();
			actionQueue
				.Setup(x => x.Enqueue(It.IsAny<Action>()))
				.Callback<Action>(x => backgroundTasks.Add(Task.Run(() =>
				{
					processBackgroundTasks.WaitOne();
					x.Invoke();
				})));

			var process = new Mock<IProcess>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Returns(process.Object);
			process
				.Setup(p => p.Start())
				.Returns(true);
			process
				.SetupGet(p => p.Threads)
				.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
			process
				.Setup(p => p.WaitForExit(It.IsAny<int>()))
				.Returns(true);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Callback(() =>
				{
					process.Raise(x => x.OutputDataReceived += null, ProcessServiceRunnerForTesting.CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
				})
				.Returns(true);
			runnerCommandQueueProvider
				.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
				.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
			runnerCommandQueueProvider
				.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
				.Throws(exception);
			taskSchedulerMock
				.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
				.Returns(nudgeRequest);

			var errorReporter = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object))
			using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServices.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(nudgeRequest);

				// Assert
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				NUnit.Framework.Assert.That(runner.ExpectingTaskRunRequestCallback, NUnit.Framework.Is.EqualTo(false), "state must be updated independently of background thread action queue");
			}

			processBackgroundTasks.Set();
			Task.WaitAll(backgroundTasks.ToArray());
			Mock.Get(nudgeRequest).Verify(x => x.OnUnableToRun(UnableToRunReason.RunnerIsInProcessOfShuttingDown, true, true), Times.Once);
		}

		public void TestTrackServiceTaskErrorWhenOutputDataReceived()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test("TEL");
				Test("STD");
			});

			void Test(string taskCode)
			{
				// Arrange
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var task = Mock.Of<IRunnableServiceTask>(x => x.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>()) && x.Code == taskCode);
				var scheduledRequest = Mock.Of<IScheduledTaskRunRequest>(x => x.Task == task && x.Id == Guid.Empty);
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				actionQueue
					.Setup(x => x.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(x => x.Invoke());

				var process = new Mock<IProcess>();
				var processFactory = new Mock<IProcessFactory>();
				processFactory
					.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(process.Object);

				using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, Mock.Of<IProcessRunnerRemotingServices>(), Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, productRegistrationMock.Object))
				{
					// Act
					AssertNoExceptionThrown(() =>
					{
						runner.Run(scheduledRequest);
						process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostCommunicationServiceTaskErrorCommandPrefix}:{taskCode}"));
					});
				}

				// Assert
				taskSchedulerMock.Verify(scheduler => scheduler.TrackServiceTaskError(taskCode), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestNewProcessNotCreatedIfOldProcessExiting()
		{
			var repo = new MockRepository(MockBehavior.Default);
			var remotingServicesMock = repo.Create<IProcessRunnerRemotingServices>();
			var taskScheduler = repo.Create<ITaskScheduler>();
			var currentProcess = repo.Create<IProcess>();
			currentProcess.Setup(p => p.HasExited).Returns(true);
			var newProcess = repo.Create<IProcess>();
			var processFactory = repo.Create<IProcessFactory>();
			processFactory.Setup(pf => pf.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>())).Returns(newProcess.Object);

			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskScheduler.Object, actionQueue, remotingServicesMock.Object, hostLoggerMock.Object, dateTimeProviderMock.Object, currentProcess.Object, processFactory.Object, runnerCommandQueueProvider.Object, grpcClientSynchronizerFactoryMock.Object))
			{
				var runRequest = repo.Create<IDirectTaskRunRequest>();
				var task = repo.Create<IRunnableServiceTask>();
				var config = repo.Create<IHostedServiceAttribute>();

				config.Setup(c => c.Code).Returns("TST");
				task.Setup(t => t.Info).Returns(new ServiceTaskInfo(config.Object));
				runRequest.Setup(rr => rr.Task).Returns(task.Object);
				NUnit.Framework.Assert.That(!runner.Run(runRequest.Object), NUnit.Framework.Is.True);
				processFactory.Verify(pf => pf.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()), Times.Never);
				hostLoggerMock.Verify(x => x.Log(LogLevel.Debug, task.Object.Info.HostedServiceAttribute, It.IsAny<string>()));
			}
		}

		[ExpectNoExceptions]
		public void TestProcessCreatedWithNet48()
		{
			var hostRegistrySettings = Mock.Of<IHostRegistrySettings>(s => !s.SwitchRunnerToNetCore);
			AssertProcessCreatedWithCorrectFilePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServiceManagerConstants.ServiceManagerRunnerExe), hostRegistrySettings);
		}

		[ExpectNoExceptions]
		public void TestProcessCreatedWithNet8()
		{
			var hostRegistrySettings = Mock.Of<IHostRegistrySettings>(s => s.SwitchRunnerToNetCore);
			AssertProcessCreatedWithCorrectFilePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "net8.0", ServiceManagerConstants.ServiceManagerRunnerExe), hostRegistrySettings);
		}

		void AssertProcessCreatedWithCorrectFilePath(string expectedFilePath, IHostRegistrySettings registry)
		{
			// Arrange
			var result = string.Empty;
			var taskScheduler = Mock.Of<ITaskScheduler>();
			var actionQueue = Mock.Of<IBackgroundThreadActionQueue>();
			var processFactory = new Mock<IProcessFactory>();
			processFactory
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Callback<ProcessStartInfo, bool, ProcessPriorityClass>((info, _, _) =>
				{
					result = info.FileName;
				})
				.Returns(Mock.Of<IProcess>());
			var serviceProcessRunnerRemotingServices = Mock.Of<IProcessRunnerRemotingServices>();
			var hostLogger = new Mock<IHostLogger>();
			var taskRequest = Mock.Of<IDirectTaskRunRequest>(
				x => x.Task == Mock.Of<IRunnableServiceTask>(
					y => y.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(
						z => z.Code == "TestCode"
							&& z.Description == "TestDescription"))));

			using (var runner = new ProcessServiceRunner(taskScheduler, actionQueue, processFactory.Object, serviceProcessRunnerRemotingServices, hostLogger.Object, grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, registry, string.Empty, productRegistrationMock.Object))
			{
				// Act
				runner.Run(taskRequest);
			}

			// Assert
			processFactory.Verify(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()), Times.Once);
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(expectedFilePath));
		}

		[ExpectNoExceptions]
		public void TestProcessKilledOnDispose()
		{
			var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			var process = new Mock<IProcess>();

			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), new Mock<ITaskScheduler>().Object, actionQueue, remotingServicesMock.Object, hostLoggerMock.Object, dateTimeProviderMock.Object, process.Object, null, runnerCommandQueueProvider.Object, grpcClientSynchronizerFactoryMock.Object))
			{
			}

			process.Verify(p => p.Kill(), Times.AtLeastOnce);
		}

		[ExpectNoExceptions]
		public void TestStopNotSentToExitingRunner()
		{
			var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			var process = new Mock<IProcess>();

			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), new Mock<ITaskScheduler>().Object, actionQueue, remotingServicesMock.Object, hostLoggerMock.Object, dateTimeProviderMock.Object, process.Object, null, runnerCommandQueueProvider.Object, grpcClientSynchronizerFactoryMock.Object))
			{
				runner.OverrideTaskRunning(false);
				process.Setup(p => p.HasExited).Returns(true);
				runner.Stop();
				NUnit.Framework.Assert.That(runner.StopCalledCount, NUnit.Framework.Is.EqualTo(0), "Exiting runner should not have been sent a stop command");
			}
		}

		[ExpectNoExceptions]
		public void TestStopOnlySentOnce()
		{
			var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			var process = new Mock<IProcess>();

			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), new Mock<ITaskScheduler>().Object, actionQueue, remotingServicesMock.Object, hostLoggerMock.Object, dateTimeProviderMock.Object, process.Object, null, runnerCommandQueueProvider.Object, grpcClientSynchronizerFactoryMock.Object))
			{
				runner.OverrideTaskRunning(false);
				runner.Stop();
				runner.Stop();
				NUnit.Framework.Assert.That(runner.StopCalledCount, NUnit.Framework.Is.EqualTo(1), "Second stop call should have been ignored");
			}
		}

		[ExpectNoExceptions]
		public void TestRunnerProcessStartedWithPoolingOption()
		{
			var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			var taskScheduler = new Mock<ITaskScheduler>();
			var hostRegistryObject = new Mock<IHostRegistrySettings>();
			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			using (var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskScheduler.Object, actionQueue, remotingServicesMock.Object, hostLoggerMock.Object, dateTimeProviderMock.Object, null, null, runnerCommandQueueProvider.Object, grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryObject.Object))
			{
				runner.eventHandleNames = new GrpcEventHandleNames();

				hostRegistryObject.SetupGet(o => o.ServiceTaskRunnerConnectionPoolingEnabled).Returns(true);

				var result = runner.GetProcessStartInfo_Exposed(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
				NUnit.Framework.Assert.That(result.Arguments, NUnit.Framework.Does.Contain("-EnableConnectionPooling"));

				hostRegistryObject.SetupGet(o => o.ServiceTaskRunnerConnectionPoolingEnabled).Returns(false);

				result = runner.GetProcessStartInfo_Exposed(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
				NUnit.Framework.Assert.That(result.Arguments, NUnit.Framework.Does.Not.Contain("-EnableConnectionPooling"));
			}
		}

		[ExpectNoExceptions]
		public void TestCreateNewProcessWithParameters()
		{
			TestCreateNewProcessWithParameters(ProcessPriorityClass.Normal);
			TestCreateNewProcessWithParameters(ProcessPriorityClass.BelowNormal);
			TestCreateNewProcessWithParameters(ProcessPriorityClass.Idle);

			void TestCreateNewProcessWithParameters(ProcessPriorityClass priorityClass)
			{
				// Arrange
				var taskScheduler = Mock.Of<ITaskScheduler>();
				var actionQueue = Mock.Of<IBackgroundThreadActionQueue>();
				var processFactory = new Mock<IProcessFactory>();
				processFactory
					.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(Mock.Of<IProcess>());
				var serviceProcessRunnerRemotingServices = Mock.Of<IProcessRunnerRemotingServices>();
				var hostLogger = new Mock<IHostLogger>();
				var taskRequest = Mock.Of<IDirectTaskRunRequest>(
					x => x.Task == Mock.Of<IRunnableServiceTask>(
						y => y.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(
							z => z.Code == "TestCode"
							&& z.Description == "TestDescription"))));

				using (var runner = new ProcessServiceRunner(taskScheduler, actionQueue, processFactory.Object, serviceProcessRunnerRemotingServices, hostLogger.Object, grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(o => o.RunnerProcessPriorityValue == priorityClass), string.Empty, productRegistrationMock.Object))
				{
					// Act
					runner.Run(taskRequest);
				}

				// Assert
				processFactory
					.Verify(x => x.Create(It.IsAny<ProcessStartInfo>(), true, priorityClass), Times.Once);
				hostLogger
					.Verify(x => x.Log(LogLevel.Debug, It.Is<string>(y => y.Contains($"TestCode Starting a new Runner process with priority {priorityClass} for initial use (TestDescription)."))), Times.Once);
			}
		}

		public class RunProvidesCorrectCallToRunnerQueueTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();

				loggerMock = new Mock<IHostLogger>();
				taskSchedulerMock = new Mock<ITaskScheduler>();
				backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				backgroundThreadActionQueueMock
					.Setup(queue => queue.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(action => action.Invoke());
				processFactoryMock = new Mock<IProcessFactory>();
				runnerCommandQueueProviderMock = new Mock<IRunnerCommandQueueProvider>();
				processRunnerRemotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
				processRunnerRemotingServicesMock
					.Setup(services => services.CreateRunnerCommandQueueProxy(It.IsAny<int?>()))
					.Returns(runnerCommandQueueProviderMock.Object);
				grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
				grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
				grpcClientSynchronizerFactoryMock
					.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
					.Returns(grpcClientSynchronizerMock.Object);
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				processServiceRunner = new ProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, processRunnerRemotingServicesMock.Object, loggerMock.Object, grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")));

				processMock = new Mock<IProcess>();

				processFactoryMock
					.Setup(factory => factory.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(processMock.Object);
				processMock
					.Setup(process => process.Start())
					.Returns(true);
			}

			protected override void TearDown()
			{
				processServiceRunner.Dispose();

				base.TearDown();
			}

			public void TestRunRequest()
			{
				// Arrange
				var taskCode = "Code";
				var typeAssemblyName = "AssemblyName";
				var configString = "configString";
				var task = Mock.Of<IRunnableServiceTask>(
					x => x.Info == new ServiceTaskInfo(
							Mock.Of<IHostedServiceAttribute>(y => y.Code == taskCode && y.TypeAssemblyName == typeAssemblyName))
						&& x.Code == taskCode
						&& x.ConfigString == configString);
				var request = new Mock<IDirectTaskRunRequest>();
				request
					.SetupGet(runRequest => runRequest.Task)
					.Returns(task);
				var response = new ServiceTaskRunResponse { Status = Status.Queued, };
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				actionQueue
					.Setup(x => x.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(x => x.Invoke());

				var process = new Mock<IProcess>();
				var processFactory = new Mock<IProcessFactory>();
				processFactory
					.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(process.Object);
				process
					.Setup(p => p.Start())
					.Returns(true);
				process
					.SetupGet(p => p.Threads)
					.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
				process
					.Setup(p => p.WaitForExit(It.IsAny<int>()))
					.Returns(true);
				grpcClientSynchronizerMock
					.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
					.Callback(() =>
					{
						process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
					})
					.Returns(true);
				runnerCommandQueueProviderMock
					.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
					.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
				taskSchedulerMock
					.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
					.Returns(request.Object);

				using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServicesMock.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST"))))
				{
					// Act
					AssertNoExceptionThrown(() => runner.Run(request.Object));
				}

				// Assert
				runnerCommandQueueProviderMock.Verify(provider => provider.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
				runnerCommandQueueProviderMock.Verify(provider => provider.Run(typeAssemblyName, taskCode, It.IsAny<Guid>(), configString), Times.Once);
			}

			public void TestRunScheduledRequest()
			{
				// Arrange
				var taskCode = "Code";
				var typeAssemblyName = "AssemblyName";
				var configString = "configString";
				var expectedNextRunTime = new DateTime(2006, 12, 26, 13, 0, 0, DateTimeKind.Utc);
				var nextRunTime = new DateTime(2019, 12, 23, 20, 20, 0, DateTimeKind.Utc);
				var task = Mock.Of<IRunnableServiceTask>(
					x => x.Info == new ServiceTaskInfo(
							Mock.Of<IHostedServiceAttribute>(y => y.Code == taskCode && y.TypeAssemblyName == typeAssemblyName))
						&& x.Code == taskCode
						&& x.ConfigString == configString);
				var request = new Mock<IScheduledTaskRunRequest>();
				request
					.SetupGet(runRequest => runRequest.Task)
					.Returns(task);
				request
					.SetupGet(runRequest => runRequest.ExpectedNextRunTime)
					.Returns(expectedNextRunTime);
				request
					.SetupGet(runRequest => runRequest.NextRunTime)
					.Returns(nextRunTime);

				var response = new ServiceTaskRunResponse { Status = Status.Queued, };
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				actionQueue
					.Setup(x => x.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(x => x.Invoke());

				var process = new Mock<IProcess>();
				var processFactory = new Mock<IProcessFactory>();
				processFactory
					.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
					.Returns(process.Object);
				process
					.Setup(p => p.Start())
					.Returns(true);
				process
					.SetupGet(p => p.Threads)
					.Returns(new ProcessThreadCollection(Array.Empty<ProcessThread>()));
				process
					.Setup(p => p.WaitForExit(It.IsAny<int>()))
					.Returns(true);
				grpcClientSynchronizerMock
					.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
					.Callback(() =>
					{
						process.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs($"{ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix}:1"));
					})
					.Returns(true);
				runnerCommandQueueProviderMock
					.Setup(p => p.StartResponseTask(It.IsAny<Action<ServiceTaskRunResponse>>(), It.IsAny<CancellationToken>()))
					.Callback((Action<ServiceTaskRunResponse> action, CancellationToken ct) => action(response));
				taskSchedulerMock
					.Setup(s => s.ReconstructRequest(It.IsAny<Func<IRunnableServiceTask, ITaskRunRequest>>(), It.IsAny<string>()))
					.Returns(request.Object);

				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);

				errorReporterProxyMock = new Mock<IErrorReporterProxy>();

				using (var runner = new ProcessServiceRunner(taskSchedulerMock.Object, actionQueue.Object, processFactory.Object, processRunnerRemotingServicesMock.Object, Mock.Of<IHostLogger>(), grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), dateTimeProviderMock.Object, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST"))))
				{
					// Act
					AssertNoExceptionThrown(() => runner.Run(request.Object));
				}

				// Assert
				runnerCommandQueueProviderMock.Verify(provider => provider.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);
				runnerCommandQueueProviderMock.Verify(provider => provider.Run(typeAssemblyName, taskCode, Guid.Empty, expectedNextRunTime, nextRunTime, configString), Times.Once);
			}

			Mock<IBackgroundThreadActionQueue> backgroundThreadActionQueueMock;
			Mock<IProcessFactory> processFactoryMock;
			Mock<IProcess> processMock;
			Mock<IProcessRunnerRemotingServices> processRunnerRemotingServicesMock;
			Mock<IRunnerCommandQueueProvider> runnerCommandQueueProviderMock;
			Mock<ITaskScheduler> taskSchedulerMock;
			Mock<IHostLogger> loggerMock;
			Mock<IGrpcClientSynchronizerFactory> grpcClientSynchronizerFactoryMock;
			Mock<IGrpcClientSynchronizer> grpcClientSynchronizerMock;
			Mock<IDateTimeProvider> dateTimeProviderMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
			ProcessServiceRunner processServiceRunner;
		}

		protected override void SetUp()
		{
			base.SetUp();

			hostLoggerMock = new Mock<IHostLogger>();

			grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
			grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
			grpcClientSynchronizerFactoryMock
				.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
				.Returns(grpcClientSynchronizerMock.Object);
			runnerCommandQueueProvider = new Mock<IRunnerCommandQueueProvider>();
			processRunnerRemotingServices = new Mock<IProcessRunnerRemotingServices>();
			processRunnerRemotingServices
				.Setup(rs => rs.CreateRunnerCommandQueueProxy(It.IsAny<int?>()))
				.Returns(runnerCommandQueueProvider.Object);

			dateTimeProviderMock = new Mock<IDateTimeProvider>();
			dateTimeProviderMock
				.Setup(p => p.CurrentDateTimeUtc)
				.Returns(() => DateTime.UtcNow);

			taskSchedulerMock = new Mock<ITaskScheduler>();

			errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.SetupGet(o => o.Key)
				.Returns(Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST"));
		}

		Mock<IHostLogger> hostLoggerMock;
		Mock<IGrpcClientSynchronizerFactory> grpcClientSynchronizerFactoryMock;
		Mock<IGrpcClientSynchronizer> grpcClientSynchronizerMock;
		Mock<IRunnerCommandQueueProvider> runnerCommandQueueProvider;
		Mock<IProcessRunnerRemotingServices> processRunnerRemotingServices;
		Mock<ITaskScheduler> taskSchedulerMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IProductRegistration> productRegistrationMock;

		// http://stackoverflow.com/a/1354557/159926
		static DataReceivedEventArgs CreateMockDataReceivedEventArgs(string testData)
		{
			if (string.IsNullOrEmpty(testData))
			{
				throw new ArgumentException("Data is null or empty.", nameof(testData));
			}

			var mockEventArgs =
				(DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
					.GetUninitializedObject(typeof(DataReceivedEventArgs));

			var eventFields = typeof(DataReceivedEventArgs)
				.GetFields(
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.DeclaredOnly);

			if (eventFields.Length > 0)
			{
				eventFields[0].SetValue(mockEventArgs, testData);
			}
			else
			{
				throw new ApplicationException(
					"Failed to find _data field!");
			}

			return mockEventArgs;
		}
	}

	class ProcessServiceRunnerForTesting : ProcessServiceRunner
	{
		public ProcessServiceRunnerForTesting(
			Guid serviceHostPk,
			ITaskScheduler taskScheduler,
			IBackgroundThreadActionQueue actionQueue,
			IProcessRunnerRemotingServices remotingServices,
			IHostLogger hostLogger,
			IDateTimeProvider dateTimeProvider,
			IProcess currentProcess = null,
			IProcessFactory processFactory = null,
			IRunnerCommandQueueProvider runnerCommandQueueProvider = null,
			IGrpcClientSynchronizerFactory grpcClientSynchronizerFactory = null,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner = null,
			IHostRegistrySettings hostRegistrySettings = null)
			: base(
				taskScheduler,
				actionQueue,
				processFactory,
				remotingServices,
				hostLogger,
				grpcClientSynchronizerFactory,
				serviceTaskLocksCleaner ?? Mock.Of<IServiceTaskLocksCleaner>(),
				dateTimeProvider,
				Mock.Of<IErrorReporterProxy>(),
				hostRegistrySettings ?? Mock.Of<IHostRegistrySettings>(),
				string.Empty,
				Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")))
		{
			RunnerCommandQueueProviderProxy = runnerCommandQueueProvider ?? new Mock<IRunnerCommandQueueProvider>().Object;

			if (currentProcess != null)
			{
				process = currentProcess;
				InitHooks(process);
			}
		}

		public void OutputDataReceivedExposed(string text)
		{
			this.OutputDataReceived(null, CreateMockDataReceivedEventArgs(text));
		}

		// http://stackoverflow.com/a/1354557/159926
		public static DataReceivedEventArgs CreateMockDataReceivedEventArgs(string testData)
		{
			if (string.IsNullOrEmpty(testData))
			{
				throw new ArgumentException("Data is null or empty.", nameof(testData));
			}

			var mockEventArgs =
				(DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
					.GetUninitializedObject(typeof(DataReceivedEventArgs));

			var eventFields = typeof(DataReceivedEventArgs)
				.GetFields(
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.DeclaredOnly);

			if (eventFields.Length > 0)
			{
				eventFields[0].SetValue(mockEventArgs, testData);
			}
			else
			{
				throw new ApplicationException(
					"Failed to find _data field!");
			}

			return mockEventArgs;
		}

		public void OverrideTaskRunning(bool value)
		{
			overrideTaskRunning = true;
			taskRunning = value;
		}

		public ProcessStartInfo GetProcessStartInfo_Exposed(ServiceTaskInfo info)
		{
			return base.GetProcessStartInfo(info);
		}

		bool taskRunning;
		bool overrideTaskRunning;

		protected override bool CheckIsRunning()
		{
			if (overrideTaskRunning)
			{
				return taskRunning;
			}

			return base.CheckIsRunning();
		}

		public Exception ThrownByCreateRunnerCommandQueueProxy { get; set; }
		protected override IRunnerCommandQueueProvider CreateRunnerCommandQueueProxy()
		{
			if (ThrownByCreateRunnerCommandQueueProxy != null)
			{
				throw ThrownByCreateRunnerCommandQueueProxy;
			}
			return RunnerCommandQueueProviderProxy;
		}

		protected override bool SendCommand(Action<IRunnerCommandQueueProvider> command, ITaskRunRequest runRequest = null)
		{
			if (overrideTaskRunning)
			{
				return true;
			}

			return base.SendCommand(command, runRequest);
		}

		public bool StubOutRunCore { get; set; }

		protected override bool RunCore(ITaskRunRequest runRequest)
		{
			if (StubOutRunCore)
			{
				return true;
			}

			return base.RunCore(runRequest);
		}

		public override void Kill(bool withLogging = true)
		{
			if (StubOutRunCore)
			{
				return;
			}

			base.Kill(withLogging);
		}

		public int StopCalledCount { get; private set; }

		protected override void StopCore()
		{
			++StopCalledCount;
			base.StopCore();
		}

		public IProcess ProcessExposed
		{
			get
			{
				return process;
			}
			set
			{
				process = value;
			}
		}

		public IRunnerCommandQueueProvider RunnerCommandQueueProviderProxy { get; set; }
	}
}
