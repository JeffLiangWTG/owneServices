using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Testing;
using Enterprise.ServiceManager.Host.Testing.Core.RunnableTask;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class SchedulerDispatcherTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			resourceThrottlerMock = new Mock<IResourceThrottler>();
			actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
			processRunnerPoolMock = new Mock<IProcessRunnerPool>();
			serviceRunnerMock = Mock.Of<IServiceRunner>();
			taskSchedulerMock = new Mock<ITaskScheduler>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			hostRegistryMock = new Mock<IHostRegistrySettings>();
			hostRegistryMock.Setup(o => o.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks).Returns(80);
			hostRegistryMock.Setup(o => o.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks).Returns(1);
			hostRegistryMock.Setup(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(1));
			hostRegistryMock.Setup(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(20);
			hostRegistryMock.Setup(o => o.ServiceTaskProcessingBatchSizeScalingFactor).Returns(1.2m);

			processRunnerPoolMock
				.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(serviceRunnerMock);
			actionQueueMock
				.Setup(q => q.InvokeActionsWhileWaiting(It.IsAny<TimeSpan>(), It.IsAny<Func<bool>>()))
				.Callback(() => Task.Delay(100).Wait());
			resourceThrottlerMock
				.Setup(t => t.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, TimeSpan.Zero, 1, 1, 1));
		}

		Mock<IResourceThrottler> resourceThrottlerMock;
		Mock<IProcessRunnerPool> processRunnerPoolMock;
		Mock<ITaskScheduler> taskSchedulerMock;
		Mock<IBackgroundThreadActionQueue> actionQueueMock;
		Mock<IHostRegistrySettings> hostRegistryMock;
		IServiceRunner serviceRunnerMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;

		public class MiscellaneousTest : SchedulerDispatcherTest
		{
			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_DelaysExecutionIfSecondaryProcessesMaxCountReached()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				var taskMock1 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock1.Object)).Returns(999);
				taskMock1.Setup(t => t.Code).Returns("A");
				taskMock1.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock1.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock2 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock2.Object)).Returns(0);
				taskMock2.Setup(t => t.Code).Returns("B");
				taskMock2.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock2.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				taskQueue.EnqueueTask(new ScheduledTaskRunRequest(taskMock1.Object));
				taskQueue.EnqueueTask(new ScheduledTaskRunRequest(taskMock2.Object));

				// Act
				NUnit.Framework.Assert.That(taskQueue.GetQueueSnapshot().First(), Is.EqualTo(taskMock1.Object));
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				taskMock2.Verify(tsu => tsu.SetNextRunTimeBasedOnRecurrence(), Times.Once());
				taskRunner.Verify(tr => tr.ProcessRunRequest(It.Is<ITaskRunRequest>(r => r.Task == taskMock2.Object), serviceRunnerMock), Times.Once());
				NUnit.Framework.Assert.That(taskQueue.GetQueueSnapshot().First(), Is.EqualTo(taskMock1.Object));
			}

			[ExpectNoExceptions]
			public void TestScheduleDispatchTaskLogsAllServiceTaskRequestsToRunner()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, new List<IRunnableServiceTask>(), hostLoggerMock, Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				var taskMock1 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock1.Object)).Returns(0);
				taskMock1.Setup(t => t.Code).Returns("A");
				taskMock1.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock1.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock2 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock2.Object)).Returns(0);
				taskMock2.Setup(t => t.Code).Returns("B");
				taskMock2.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock2.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock3 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock3.Object)).Returns(0);
				taskMock3.Setup(t => t.Code).Returns("C");
				taskMock3.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock3.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock3.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock3.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var request1 = new ScheduledTaskRunRequest(taskMock1.Object);
				var request2 = new ScheduledTaskRunRequest(taskMock2.Object);
				var request3 = new ScheduledTaskRunRequest(taskMock3.Object);

				taskQueue.EnqueueTask(request1);
				taskQueue.EnqueueTask(request2);
				taskQueue.EnqueueTask(request3);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Information, It.IsRegex($"Requests dispatched to runners: [A-C]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}, [A-C]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}, [A-C]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}.")), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestScheduleDispatchTaskLogsAllServiceTaskRequestsToRunnerWithDuplicates()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, new List<IRunnableServiceTask>(), hostLoggerMock, Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				var taskMock1 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock1.Object)).Returns(0);
				taskMock1.Setup(t => t.Code).Returns("A");
				taskMock1.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock1.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock1.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock2 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock2.Object)).Returns(0);
				taskMock2.Setup(t => t.Code).Returns("A");
				taskMock2.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock2.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock2.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock3 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock3.Object)).Returns(0);
				taskMock3.Setup(t => t.Code).Returns("B");
				taskMock3.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock3.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock3.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock3.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var taskMock4 = new Mock<IRunnableServiceTask>();
				processRunnerPool.Setup(prp => prp.RunningCount(taskMock3.Object)).Returns(0);
				taskMock4.Setup(t => t.Code).Returns("B");
				taskMock4.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				taskMock4.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				taskMock4.Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				taskMock4.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));

				var request1 = new ScheduledTaskRunRequest(taskMock1.Object);
				var request2 = new ScheduledTaskRunRequest(taskMock2.Object);
				var request3 = new ScheduledTaskRunRequest(taskMock3.Object);
				var request4 = new ScheduledTaskRunRequest(taskMock4.Object);

				taskQueue.EnqueueTask(request1);
				taskQueue.EnqueueTask(request2);
				taskQueue.EnqueueTask(request3);
				taskQueue.EnqueueTask(request4);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Information, It.IsRegex($"Requests dispatched to runners: [A-B]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}, [A-B]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}, [A-B]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}, [A-B]/{TaskRunRequestTest<DirectTaskRunRequest>.GuidRegExTemplate}.")), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestScheduleDispatchWithEmptyTaskQueueLog()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, new List<IRunnableServiceTask>(), hostLoggerMock, Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "No requests dispatched to runners."), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_DequeuesSetsNextRuntimeAndRunsTaskIfAvailable()
			{
				// Arrange
				var allTasks = new[]
					{
					"ONE",
					"TWO",
					"THR",
					"FOR",
					"FVE",
				}
					.Select(code =>
					{
						var runRequestMock = new Mock<IScheduledTaskRunRequest>();
						var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
						runRequestMock
							.SetupGet(request => request.Task)
							.Returns(runnableServiceTaskMock.Object);

						return (runRequestMock, runnableServiceTaskMock);
					})
					.ToList();

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks.Select(tuple => tuple.runnableServiceTaskMock).ToList(), taskRunner.Object, Mock.Of<IServiceHostsCache>());

				ITaskRunRequest dequeued = allTasks[0].runRequestMock.Object;
				var tryDequeueTaskCount = 0;
				var taskQueueMock = new Mock<ITaskQueue>();
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(allTasks.Select(tuple => tuple.runnableServiceTaskMock.Object));
				// Please make it more appropriate when Moq.Mock 4.8 will be available in Dev...
				taskQueueMock
					.Setup(queue => queue.TryDequeueTask(out dequeued))
					.Callback(() =>
					{
						dequeued = allTasks[++tryDequeueTaskCount].runRequestMock.Object;
						taskQueueMock
							.Setup(queue => queue.TryDequeueTask(out dequeued))
							.Callback(() =>
							{
								dequeued = allTasks[++tryDequeueTaskCount].runRequestMock.Object;
								taskQueueMock
									.Setup(queue => queue.TryDequeueTask(out dequeued))
									.Callback(() =>
									{
										dequeued = allTasks[++tryDequeueTaskCount].runRequestMock.Object;
										taskQueueMock
											.Setup(queue => queue.TryDequeueTask(out dequeued))
											.Callback(() =>
											{
												dequeued = allTasks[++tryDequeueTaskCount].runRequestMock.Object;
												taskQueueMock
													.Setup(queue => queue.TryDequeueTask(out dequeued))
													.Returns(true);
											})
											.Returns(true);
									})
									.Returns(true);
							})
							.Returns(true);
					})
					.Returns(true);

				// Act
				taskSchedulerDispatcher.Dispatch(taskQueueMock.Object);

				// Assert
				foreach (var (_, runnableServiceTaskMock) in allTasks)
				{
					runnableServiceTaskMock.Verify(task => task.SetNextRunTimeBasedOnRecurrence(), Times.Once);
				}

				taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Exactly(5));
			}

			public void TestScheduleAndDispatch_ForOverdueTask()
			{
				// Arrange
				var allTasks = CreateMockTasks(new string[1] { "TST" });
				allTasks[0].Setup(t => t.IsOverdue).Returns(true);
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, Mock.Of<IServiceHostsCache>());
				var taskQueue = CreateMockTaskQueue(allTasks, directRequest: false);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue.Object);
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);

				// Assert
				allTasks[0].Verify(tsu => tsu.SetNextRunTimeBasedOnRecurrence(), Times.Once());
				taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Never);
				Assert(true);
			}

			public void TestScheduleAndDispatch_ForOverdueDirectTask()
			{
				// Arrange
				var allTasks = CreateMockTasks(new string[1] { "TST" });
				allTasks[0].Setup(t => t.IsOverdue).Returns(true);
				var taskRunner = new Mock<ITaskRunRequestProcessor>();

				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, Mock.Of<IServiceHostsCache>());
				var taskQueue = CreateMockTaskQueue(allTasks, directRequest: true);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue.Object);
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);

				// Assert
				allTasks[0].Verify(tsu => tsu.SetNextRunTimeBasedOnRecurrence(), Times.Never);
				taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Once);
				Assert(true);
			}

			[ExpectNoExceptions]
			public void TestSchedule_EnqueuesTasks_InOrderAdded()
			{
				// Arrange
				var allTasks = CreateMockTasks(new string[5]
				{
					"ONE",
					"TWO",
					"THR",
					"FOR",
					"FVE",
				});
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);

				// Assert
				for (var i = 0; i < 5; i++)
				{
					NUnit.Framework.Assert.That(taskQueue.GetQueueSnapshot().ElementAt(i), Is.EqualTo(allTasks[i].Object));
				}
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_DispatchesAllTasksInQueueIfSmallerThanMaxBatchSize()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < 5; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);
				// Assert
				NUnit.Framework.Assert.That(!taskQueue.GetQueueSnapshot().Any(), Is.True);
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_DispatchesEntireQueue()
			{
				// Arrange
				var queueSize = 5;
				var batchSize = 2;
				hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(batchSize);
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < queueSize; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);
				// Assert
				NUnit.Framework.Assert.That(taskQueue.GetQueueSnapshot().Count(), Is.EqualTo(0));
			}

			[ExpectNoExceptions]
			public void TestDispatchLoopRampsUpBatchSize_ScalingFactor1() => AssertDispatchLoopRampsUpBatchSize(5, 1, 5, new[] { 5, 4, 3, 2, 1 });
			[ExpectNoExceptions]
			public void TestDispatchLoopRampsUpBatchSize_ScalingFactor2() => AssertDispatchLoopRampsUpBatchSize(32, 2, 20, new[] { 32, 31, 29, 25, 17, 1 });
			[ExpectNoExceptions]
			public void TestDispatchLoopRampsUpBatchSize_ScalingFactor1_5() => AssertDispatchLoopRampsUpBatchSize(12, 1.5m, 20, new[] { 12, 11, 10, 8, 5 });
			[ExpectNoExceptions]
			public void TestDispatchLoopRampsUpBatchSize_BatchSizeCappedAtMaximum() => AssertDispatchLoopRampsUpBatchSize(50, 3, 12, new[] { 50, 49, 46, 37, 25, 13, 1 });

			[ExpectNoExceptions]
			public void AssertDispatchLoopRampsUpBatchSize(int queueSize, decimal scalingFactor, int maximumBatchSize, IEnumerable<int> expectedRemainingQueueSizePerIteration)
			{
				// Arrange
				hostRegistryMock.SetupGet(h => h.ServiceTaskProcessingMaximumBatchSize).Returns(maximumBatchSize);
				hostRegistryMock.SetupGet(h => h.ServiceTaskProcessingBatchSizeScalingFactor).Returns(scalingFactor);
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < queueSize; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);
				var queueSizePerIteration = new List<int>();
				resourceThrottlerMock
					.Setup(r => r.WaitForResource())
					.Callback(() =>
					{
						queueSizePerIteration.Add(taskQueue.GetQueueSnapshot().Count());
					});

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				NUnit.Framework.Assert.That(expectedRemainingQueueSizePerIteration, Is.EqualTo(queueSizePerIteration));
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_WithDefaultBatchDelay()
			{
				// Arrange
				hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingBatchDelay).Returns(TimeSpan.FromMilliseconds(200));

				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < 2; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() =>
					{
						Task.Delay(1000).Wait();
						return Mock.Of<IServiceRunner>();
					});
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				actionQueueMock.Verify(a => a.InvokeActionsWhileWaiting(hostRegistryMock.Object.ServiceTaskProcessingBatchDelay, It.IsAny<Func<bool>>()), Times.AtLeastOnce);
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_WithCustomBatchDelay()
			{
				// Arrange
				hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingBatchDelay).Returns(TimeSpan.FromMilliseconds(200));

				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < 2; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() =>
					{
						Task.Delay(1000).Wait();
						return Mock.Of<IServiceRunner>();
					});
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				actionQueueMock.Verify(a => a.InvokeActionsWhileWaiting(hostRegistryMock.Object.ServiceTaskProcessingBatchDelay, It.IsAny<Func<bool>>()), Times.AtLeastOnce);
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_InitializedRunnersAreDispatchedFirst()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < 2; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var firstDelayedRunner = Mock.Of<IServiceRunner>();
				var secondNotDelayedRunner = Mock.Of<IServiceRunner>();

				var mockSequence = new MockSequence();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				taskRunner
					.InSequence(mockSequence)
					.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), secondNotDelayedRunner));
				taskRunner
					.InSequence(mockSequence)
					.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), firstDelayedRunner));
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				processRunnerPoolMock
					.SetupSequence(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() =>
					{
						Task.Delay(1000).Wait();
						return firstDelayedRunner;
					})
					.ReturnsAsync(() => secondNotDelayedRunner);
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				taskRunner.Verify(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), secondNotDelayedRunner), Times.Once);
				taskRunner.Verify(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), firstDelayedRunner), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_RunnersWhichFailToRetrieveReEnqueueTheirTask()
			{
				// Arrange
				var exception = new Exception("Bad Terrible thing");
				var numberOfTasks = 1;
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < numberOfTasks; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

				var runner = Mock.Of<IServiceRunner>();

				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				taskRunner
					.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), runner));
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ThrowsAsync(exception);
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				NUnit.Framework.Assert.That(taskQueue.GetQueueSnapshot().Count(), Is.EqualTo(numberOfTasks));
				errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(
					It.Is<string>(msg => msg.Contains("Failure to retrieve runner")),
					It.IsAny<AggregateException>()),
					Times.Once);
			}

			public void TestScheduleAndDispatch_TasksThrowsACriticalExceptionOnRequest()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(new DatabaseUpgradedException());
					Test(new HttpCriticalException());
					Test(new ExecuteScalarReturnedNullException());
					Test(new SqlLockLostException());
				});

				void Test(Exception exception)
				{
					// Arrange
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					var allTasks = new List<Mock<IRunnableServiceTask>>() { task };

					var serviceRunner = Mock.Of<IServiceRunner>();

					var taskRunner = new Mock<ITaskRunRequestProcessor>();
					taskRunner
						.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunner))
						.Throws(exception);
					var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
					var processRunnerPool = new Mock<IProcessRunnerPool>();
					processRunnerPoolMock
						.SetupSequence(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
						.ReturnsAsync(() =>
						{
							return serviceRunner;
						})
						.ReturnsAsync(() => serviceRunner);
					var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

					// Act
					taskSchedulerDispatcher.Schedule(taskQueue);

					// Assert
					var returnedException = AssertExceptionThrown<Exception>(() => taskSchedulerDispatcher.Dispatch(taskQueue));
					NUnit.Framework.Assert.That(returnedException is ICriticalException, Is.True);
				}
			}

			public void TestScheduleAndDispatch_TasksThrowsACriticalExceptionAfterRequest()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					Test(new DatabaseUpgradedException());
					Test(new HttpCriticalException());
					Test(new ExecuteScalarReturnedNullException());
					Test(new SqlLockLostException());
				});

				void Test(Exception exception)
				{
					// Arrange
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					var allTasks = new List<Mock<IRunnableServiceTask>>() { task };

					var serviceRunner = Mock.Of<IServiceRunner>();

					var mockSequence = new MockSequence();
					var taskRunner = new Mock<ITaskRunRequestProcessor>();
					var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
					var processRunnerPool = new Mock<IProcessRunnerPool>();
					processRunnerPoolMock
						.SetupSequence(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
						.Throws(exception);
					var taskQueue = CreateMockTaskQueue(allTasks, false);

					// Act
					taskSchedulerDispatcher.Schedule(taskQueue.Object);

					// Assert
					var returnedException = AssertExceptionThrown<Exception>(() => taskSchedulerDispatcher.Dispatch(taskQueue.Object));
					NUnit.Framework.Assert.That(returnedException.FlattenInnerExceptions().Any(x => x is ICriticalException), Is.True);
				}
			}

			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_TasksThatFailToRunDoNotBlockSubsequentTasks()
			{
				// Arrange
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				for (var i = 0; i < 2; i++)
				{
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(new Mock<IHostedServiceAttribute>().Object));
					allTasks.Add(task);
				}

				var firstDelayedRunner = Mock.Of<IServiceRunner>();
				var secondNotDelayedRunner = Mock.Of<IServiceRunner>();

				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

				var mockSequence = new MockSequence();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				taskRunner
					.InSequence(mockSequence)
					.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), secondNotDelayedRunner))
					.Throws(new Exception("Bad terrible things have happened"));
				taskRunner
					.InSequence(mockSequence)
					.Setup(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), firstDelayedRunner));
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, allTasks.Select(x => x.Object).ToList(), Mock.Of<IServiceHostsCache>());
				var processRunnerPool = new Mock<IProcessRunnerPool>();
				processRunnerPoolMock
					.SetupSequence(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(() =>
					{
						Task.Delay(1000).Wait();
						return firstDelayedRunner;
					})
					.ReturnsAsync(() => secondNotDelayedRunner);
				var taskQueue = new TaskQueue(processRunnerPool.Object, new Mock<IHostLogger>().Object);

				// Act
				taskSchedulerDispatcher.Schedule(taskQueue);
				taskSchedulerDispatcher.Dispatch(taskQueue);

				// Assert
				taskRunner.Verify(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), secondNotDelayedRunner), Times.Once);
				taskRunner.Verify(s => s.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), firstDelayedRunner), Times.Once);
				errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.Is<string>(msg => msg == "Task dispatch failures"), It.IsAny<Exception>()), Times.Once);
			}

			public void TestDispatch_ThrowsOperationCancelledExceptionIfCancellationRequested()
			{
				// Arrange
				var allTasks = CreateMockTasks(new string[5]
				{
					"ONE",
					"TWO",
					"THR",
					"FOR",
					"FVE",
				});
				var taskSelector = new Mock<ITaskSelector>();
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				using var tokenSource = new CancellationTokenSource();
				var logger = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = new SchedulerDispatcher(Mock.Of<IAllTasksConsumer>(a => a.GetAll() == allTasks.Select(x => x.Object)), taskSelector.Object, taskRunner.Object, logger.Object, Mock.Of<IServiceHostsCache>(), processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, tokenSource.Token, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>());
				var taskQueue = new Mock<ITaskQueue>();
				taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(allTasks.Select(x => x.Object));
				tokenSource.Cancel();

				// Act
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);

				// Assert
				var dequeued = (ITaskRunRequest)new ScheduledTaskRunRequest(allTasks[0].Object);
				taskQueue.Verify(tq => tq.TryDequeueTask(out dequeued), Times.Never);
				Assert(true);
			}

			public void TestDispatch_InvokesRunRequestIfAbleToSetNextRunTime()
			{
				// Arrange
				var allTasks = CreateMockTasks(new string[1] { "TST" });
				var taskRunner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, taskRunner.Object, Mock.Of<IServiceHostsCache>());
				var taskQueue = CreateMockTaskQueue(allTasks, directRequest: false);

				// Act
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);
				// Assert
				taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Once);
				Assert(true);
			}

			public void TestDispatchLockToUpdateNextRunTimeFailedOnSqlLockLostException()
			{
				// Arrange
				const string taskCode = "TST";
				var serviceConfigMock = new Mock<IHostedServiceAttribute>();
				var serviceConfig = serviceConfigMock.Object;
				serviceConfigMock.Setup(s => s.Code).Returns(taskCode);
				serviceConfigMock.Setup(s => s.AllowsMultipleInstances).Returns(false);

				var runnableServiceTask = Mock.Of<IRunnableServiceTask>(x =>
					x.Code == serviceConfig.Code
					&& x.Info == new ServiceTaskInfo(serviceConfig)
					&& x.TimeSinceLastEnqueued == new Stopwatch());

				var allTasks = new List<IRunnableServiceTask> { runnableServiceTask };

				var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTask);
				var taskRunRequestMock = new Mock<IScheduledTaskRunRequest>();
				taskRunRequestMock.Setup(x => x.TakeNextRunTimeFromTask()).Callback(() => scheduledTaskRunRequest.TakeNextRunTimeFromTask());
				taskRunRequestMock.Setup(x => x.Task).Returns(scheduledTaskRunRequest.Task);

				var scheduledTaskRunRequestMock = taskRunRequestMock.As<IScheduledTaskRunRequest>();
				var taskRunRequest = taskRunRequestMock.Object as ITaskRunRequest;
				var taskQueueMock = new Mock<ITaskQueue>();
				var taskQueue = taskQueueMock.Object;
				taskQueueMock
					.Setup(x => x.TryDequeueTask(out taskRunRequest))
					.Returns(true);
				taskQueueMock
					.Setup(x => x.GetQueueSnapshot())
					.Returns(allTasks);

				var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock
					.Setup(x => x.ConfiguredServiceHosts)
					.Returns(new[] { Mock.Of<IServiceHostClient>(), Mock.Of<IServiceHostClient>() });

				var taskRunnerMock = new Mock<ITaskRunRequestProcessor>();
				var taskRunner = taskRunnerMock.Object;
				_ = taskRunnerMock
					.SetupSequence(x => x.ProcessRunRequest(taskRunRequest, It.IsAny<IServiceRunner>()))
					.Returns(() => throw new SqlLockLostException())
					.Returns(TaskRunRequestResult.Success);

				var taskSchedulerDispatcher = new SchedulerDispatcher(Mock.Of<IAllTasksConsumer>(a => a.GetAll() == allTasks), Mock.Of<ITaskSelector>(), taskRunner, Mock.Of<IHostLogger>(), serviceHostsCacheMock.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, hostRegistryMock.Object, Mock.Of<IResourceThrottler>());

				// Act
				// Assert
				AssertNoExceptionThrown(() =>
				{
					// Act
					taskSchedulerDispatcher.Dispatch(taskQueue);

					// Assert
					scheduledTaskRunRequestMock.Verify(x => x.TakeNextRunTimeFromTask(), Times.Once);
					taskRunnerMock.Verify(x => x.ProcessRunRequest(taskRunRequest, serviceRunnerMock), Times.Once);
					scheduledTaskRunRequestMock.Verify(x => x.OnUnableToRun(UnableToRunReason.LockToUpdateNextRunTimeFailed, true, false), Times.Once());
				});
			}

			[TestDate(2016, 05, 26, 12, 00, 00)]
			[ExpectNoExceptions]
			public void TestDispatch_DoesNotRescheduleRun_WhenSuccess_Direct()
			{
				Dispatch_Success(true);
			}

			[TestDate(2016, 05, 26, 12, 00, 00)]
			[ExpectNoExceptions]
			public void TestDispatch_DoesNotRescheduleRun_WhenSuccess_Scheduled()
			{
				Dispatch_Success(false);
			}

			void Dispatch_Success(bool directRequest)
			{
				// Arrange
				var task = new Mock<IRunnableServiceTask>();
				task.Setup(t => t.Info).Returns(TaskSchedulerTest.CreateTaskInfo("TST"));
				var allTasks = new[] { task };
				var runner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, runner.Object, Mock.Of<IServiceHostsCache>());
				var request = CreateRequest(task, directRequest);
				var taskQueue = CreateMockTaskQueue(allTasks, request);

				// Act
				runner.Setup(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);

				// Assert
				Mock.Get(request).Verify(t => t.OnSuccessfulRunAttempt());
			}

			[ExpectNoExceptions]
			public void TestDispatch_DoesNotRescheduleRun_WhenConfigError()
			{
				Dispatch_ReschedulesFailedRun(TaskRunRequestResult.ConfigurationError, UnableToRunReason.ConfigurationError, false, false);
			}

			[ExpectNoExceptions]
			public void TestDispatch_DoesNotRescheduleRun_WhenTaskHasAlreadyCompletedRunning()
			{
				Dispatch_ReschedulesFailedRun(TaskRunRequestResult.TaskAlreadyCompleted, UnableToRunReason.TaskAlreadyCompleted, false, false);
			}

			[ExpectNoExceptions]
			public void TestDispatch_ReschedulesRun_WhenScheduledRequestFailsWithProcessDidNotStart()
			{
				Dispatch_ReschedulesFailedRun(TaskRunRequestResult.ProcessDidNotStart, UnableToRunReason.RunnerProcessDidNotStart, true, false);
			}

			[ExpectNoExceptions]
			public void TestDispatch_EnqueuesNewRun_WhenDirectRequestFailsWithProcessDidNotStart()
			{
				Dispatch_ReschedulesFailedRun(TaskRunRequestResult.ProcessDidNotStart, UnableToRunReason.RunnerProcessDidNotStart, true, true);
			}

			void Dispatch_ReschedulesFailedRun(TaskRunRequestResult result, UnableToRunReason expectedFailureReason, bool expectReschedule, bool directRequest)
			{
				// Arrange
				var logger = Mock.Of<IHostLogger>();
				var taskInfo = TaskSchedulerTest.CreateTaskInfo("TST");
				var task = Mock.Of<IRunnableServiceTask>(
					t => t.Info == taskInfo);

				var allTasks = new[] { Mock.Get(task) };
				var runner = new Mock<ITaskRunRequestProcessor>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, runner.Object, Mock.Of<IServiceHostsCache>());
				var request = CreateRequest(allTasks.First(), directRequest);
				var taskQueue = CreateMockTaskQueue(allTasks, request);

				// Act
				runner.Setup(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(result);
				taskSchedulerDispatcher.Dispatch(taskQueue.Object);

				// Assert
				Mock.Get(request).Verify(x => x.OnUnableToRun(expectedFailureReason, expectReschedule, true), Times.Once);
			}

			[TestDate(2016, 05, 26, 12, 00, 00)]
			[ExpectNoExceptions]
			public void TestMultipleDispatchersCannotDispatchTheSameTaskTwice_WhenAllowMultipleIsTrue()
			{
				AssertMultipleDispatchersCanDispatchTheSameTaskTwice(allowMultipleInstances: true);
			}

			[TestDate(2016, 05, 26, 12, 00, 00)]
			[ExpectNoExceptions]
			public void TestMultipleDispatchersCannotDispatchTheSameTaskTwice_WhenAllowMultipleIsFalse()
			{
				AssertMultipleDispatchersCanDispatchTheSameTaskTwice(allowMultipleInstances: false);
			}

			void AssertMultipleDispatchersCanDispatchTheSameTaskTwice(bool allowMultipleInstances)
			{
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory, active: true);
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { new Mock<IServiceHostClient>().Object, new Mock<IServiceHostClient>().Object });

				var task = new Mock<DelayedRunnableTask>(allowMultipleInstances, schedule);
				using (var task1 = task.Object)
				{
					var allTasks1 = new[] { task.As<IRunnableServiceTask>() };
					var runner1 = new Mock<ITaskRunRequestProcessor>();
					runner1.Setup(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);
					var taskSchedulerDispatcher1 = CreateSchedulerDispatcher(allTasks1, runner1.Object, serviceHostsCache.Object);
					var taskQueue1 = CreateMockTaskQueue(allTasks1, directRequest: false);

					var runner2 = new Mock<ITaskRunRequestProcessor>();
					var allTasks2 = CreateMockTasks(new string[1] { "TST" }, allowMultipleInstances);
					var taskSchedulerDispatcher2 = CreateSchedulerDispatcher(allTasks2, runner2.Object, serviceHostsCache.Object);
					var request2 = CreateRequest(allTasks2.First(), false);
					var taskQueue2 = CreateMockTaskQueue(allTasks2, request2);

					var dispatchTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							taskSchedulerDispatcher1.Dispatch(taskQueue1.Object);
						}
					});
					NUnit.Framework.Assert.That(task1.WaitUntilSetNextRunTimeBasedOnRecurrenceCalled(), Is.True);
					try
					{
						taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
						Mock.Get(request2).Verify(x => x.OnUnableToRun(UnableToRunReason.OtherHostIsSchedulingTheTask, true, false), Times.Once);
						runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never);
					}
					finally
					{
						task1.CompleteSetNextRunTimeBasedOnRecurrence();
						dispatchTask.Wait();
					}

					runner1.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));
				}
			}

			[UseSnapshotProtection(skipTransaction: true)] // we're doing loads in a different thread. with the transaction nothing is saved so we don't see it on the other thread
			[ExpectNoExceptions]
			public void TestScheduleDispatcherDoesNotRunIfAnotherHostHasRunButIsNowUnresponsive()
			{
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory, active: true, taskPeriod: "M");
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var host1 = new Mock<IServiceHostClient>();
				var host2 = new Mock<IServiceHostClient>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { host1.Object, host2.Object });
				serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(new[] { host1.Object });
				Factory.Save();
				var statusProvider = new Mock<IServiceTaskScheduleStatusProvider>();
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				using (ObjectFactory.Substitute(() => statusProvider.Object))
				{
					using var autoResetEvent1 = new AutoResetEvent(false);
					using var autoResetEvent2 = new AutoResetEvent(false);
					var runner1 = new Mock<ITaskRunRequestProcessor>();
					var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
					var allTasks1 = CreateTasks(new ServiceTaskSchedule[1] { schedule }, allowMultipleInstances: false, actionQueue: actionQueueMock.Object, transactionAdapter: transactionAdapter);
					runner1.Setup(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()))
						.Callback((ITaskRunRequest mi, IServiceRunner sr) =>
						{
							autoResetEvent1.Set();
							autoResetEvent2.WaitOne();
						})
						.Returns(TaskRunRequestResult.Success);
					var taskSchedulerDispatcher1 = CreateSchedulerDispatcher(allTasks1, runner1.Object, serviceHostsCache.Object);
					var taskQueue1 = CreateMockTaskQueue(allTasks1, directRequest: false);

					var runner2 = new Mock<ITaskRunRequestProcessor>();
					var allTasks2 = CreateTasks(new ServiceTaskSchedule[1] { new BusinessObjectFactory().Load<ServiceTaskSchedule>(schedule.PK) }, allowMultipleInstances: false, transactionAdapter: transactionAdapter);
					var taskSchedulerDispatcher2 = CreateSchedulerDispatcher(allTasks2, runner2.Object, serviceHostsCache.Object);
					var taskQueue2 = CreateMockTaskQueue(allTasks2, directRequest: false);
					schedule.FillWithValidTestData();

					schedule.Factory.ThreadSentry.RelinquishThreadOwnership();
					var dispatchTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							schedule.Factory.ThreadSentry.TakeThreadOwnership();
							taskSchedulerDispatcher1.Dispatch(taskQueue1.Object);
							schedule.Factory.ThreadSentry.RelinquishThreadOwnership();
						}
					});

					try
					{
						autoResetEvent1.WaitOne();
						taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
						autoResetEvent2.Set();

						dispatchTask.Wait();
						NUnit.Framework.Assert.That(dispatchTask.IsCompleted, Is.EqualTo(true));
						runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never);
					}
					finally
					{
					}

					runner1.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));
				}
			}

			[TestDate(2016, 05, 26, 12, 00, 00)]
			[ExpectNoExceptions]
			public void TestMultipleDispatchersCannotDispatchTheSameTaskTwice_WhenAllowMultipleIsFalse_ClearsFailedScheduleAttempt()
			{
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory, active: true, taskPeriod: "M");
				var repo = new MockRepository(MockBehavior.Default);
				var serviceHostsCache = repo.Create<IServiceHostsCache>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { repo.Create<IServiceHostClient>().Object, repo.Create<IServiceHostClient>().Object });
				serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var delayedTask = new Mock<DelayedRunnableTask>(false, schedule) { CallBase = true };
				using (var task1 = delayedTask.Object)
				{
					var allTasks1 = new[] { delayedTask.As<IRunnableServiceTask>() };
					var runner1 = repo.Create<ITaskRunRequestProcessor>();
					runner1.Setup(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);
					var taskSchedulerDispatcher1 = CreateSchedulerDispatcher(allTasks1, runner1.Object, serviceHostsCache.Object);
					var taskQueue1 = CreateMockTaskQueue(allTasks1, directRequest: false);

					var runner2 = repo.Create<ITaskRunRequestProcessor>();
					var allTasks2 = CreateTasks(new ServiceTaskSchedule[1] { schedule }, allowMultipleInstances: false);
					var taskSchedulerDispatcher2 = CreateSchedulerDispatcher(allTasks2, runner2.Object, serviceHostsCache.Object);
					var request2 = CreateRequest(allTasks2.First(), false);
					var taskQueue2 = CreateMockTaskQueue(allTasks2, request2);

					var dispatchTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							taskSchedulerDispatcher1.Dispatch(taskQueue1.Object);
						}
					});
					NUnit.Framework.Assert.That(task1.WaitUntilSetNextRunTimeBasedOnRecurrenceCalled(), Is.True);

					var retryScheduleTime = new ZDateTime(ZDateTime.UtcNow.ToDateTime().Add(TaskScheduler.FailedScheduleRetryTime));
					try
					{
						taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
						runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never);
						Mock.Get(request2).Verify(x => x.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
					}
					finally
					{
						task1.CompleteSetNextRunTimeBasedOnRecurrence();
						allTasks2[0].Object.SetNextRunTime(task1.NextScheduledRunTime, SetNextRuntimeReason.ScheduledToRun);
						dispatchTask.Wait();
					}

					runner1.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.AtLeastOnce);

					TestDateAttribute.Date = retryScheduleTime.ToDateTime();
					taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
					runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never);
					NUnit.Framework.Assert.That(allTasks2[0].Object.FailedScheduleRetryTime, Is.Null);
				}
			}

			[ExpectNoExceptions]
			public void TestUpdateNextRunTimeWhileSchedulingOtherTasks()
			{
				var schedule1 = TaskSchedulerTest.CreateSchedule("TT1", Factory, active: true);
				var task2 = TaskSchedulerTest.CreateMockTask(new SchedulerServiceTask(TaskSchedulerTest.CreateSchedule("TT2", Factory)), allowsMultiple: false, new BackgroundThreadActionQueue(CancellationToken.None));
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { new Mock<IServiceHostClient>().Object, new Mock<IServiceHostClient>().Object });
				serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var task = new Mock<DelayedRunnableTask>(true, schedule1) { CallBase = true };
				using (var task1 = task.Object)
				{
					var allTasks = new[] { task.As<IRunnableServiceTask>(), task2 };
					var runner = new Mock<ITaskRunRequestProcessor>();
					runner.Setup(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);
					var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, runner.Object, serviceHostsCache.Object);
					var taskQueue = new TaskQueue(new Mock<IProcessRunnerPool>().Object, new Mock<IHostLogger>().Object);
					taskQueue.EnqueueTask(new ScheduledTaskRunRequest(task1));
					taskQueue.EnqueueTask(new ScheduledTaskRunRequest(task2.Object));

					var dispatchTask = Task.Run(() =>
					{
						NUnit.Framework.Assert.That(task1.WaitUntilSetNextRunTimeBasedOnRecurrenceCalled(), Is.True);
						task2.Object.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(1), SetNextRuntimeReason.ScheduledToRun);
						task1.CompleteSetNextRunTimeBasedOnRecurrence(updateNextRunTime: false);
					});

					taskSchedulerDispatcher.Dispatch(taskQueue);
					dispatchTask.Wait();
					runner.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Once());
				}
			}

			[ExpectNoExceptions]
			public void TestDispatcherUpdatesStatusAndChecksNextRunTimeIsInFuture_WhenAllowMultipleIsTrue()
			{
				var runner = new Mock<ITaskRunRequestProcessor>();
				var allTasks = CreateMockTasks(new string[1] { "TST" }, allowMultipleInstances: true);
				allTasks[0].Setup(t => t.NextRunTimeIsInFuture).Returns(true);
				var logger = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, runner.Object, new List<IRunnableServiceTask>(), logger, Mock.Of<IServiceHostsCache>());

				//Schedule even if not ready, as this task may need backlog help.
				var taskQueue1 = CreateMockTaskQueue(allTasks, directRequest: false);
				taskSchedulerDispatcher.Dispatch(taskQueue1.Object);
				taskQueue1.Object.TryDequeueTask(out var taskRunReq1);
				Mock.Get(taskRunReq1.Task).Verify(rt => rt.UpdateStatus(), Times.Never());
				allTasks[0].Verify(ts => ts.SetNextRunTimeBasedOnRecurrence());
				runner.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));

				//Schedule if ready.
				allTasks = CreateMockTasks(new string[1] { "TST" }, allowMultipleInstances: true);
				allTasks[0].Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				var taskQueue2 = CreateMockTaskQueue(allTasks, directRequest: false);
				taskSchedulerDispatcher.Dispatch(taskQueue2.Object);
				taskQueue2.Object.TryDequeueTask(out var taskRunReq2);
				Mock.Get(taskRunReq1.Task).Verify(rt => rt.UpdateStatus(), Times.Never());
				allTasks[0].Verify(ts => ts.SetNextRunTimeBasedOnRecurrence());
				runner.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));
			}

			[ExpectNoExceptions]
			public void TestDispatcherUpdatesStatusAndChecksNextRunTimeIsInFuture_WhenAllowMultipleIsFalse()
			{
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { new Mock<IServiceHostClient>().Object, new Mock<IServiceHostClient>().Object });
				var runner = new Mock<ITaskRunRequestProcessor>();
				var allTasks = CreateMockTasks(new string[1] { "TST" }, allowMultipleInstances: false);
				allTasks[0].Setup(t => t.NextRunTimeIsInFuture).Returns(true);
				var logger = new Mock<IHostLogger>();
				var taskSchedulerDispatcher = CreateSchedulerDispatcher(allTasks, runner.Object, new List<IRunnableServiceTask>(), logger, serviceHostsCache.Object);

				//Don't schedule if not ready.
				var taskQueue1 = CreateMockTaskQueue(allTasks, directRequest: false);
				taskSchedulerDispatcher.Dispatch(taskQueue1.Object);
				taskQueue1.Object.TryDequeueTask(out var taskRunReq1);
				Mock.Get(taskRunReq1.Task).Verify(rt => rt.UpdateStatus(), Times.Once());
				allTasks[0].Verify(ts => ts.SetNextRunTimeBasedOnRecurrence(), Times.Never());
				runner.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never());

				//Schedule if ready.
				allTasks = CreateMockTasks(new string[1] { "TST" }, allowMultipleInstances: false);
				allTasks[0].Setup(t => t.NextRunTimeIsInFuture).Returns(false);
				var taskQueue2 = CreateMockTaskQueue(allTasks, directRequest: false);
				taskSchedulerDispatcher.Dispatch(taskQueue2.Object);
				taskQueue2.Object.TryDequeueTask(out var taskRunReq2);
				Mock.Get(taskRunReq2.Task).Verify(rt => rt.UpdateStatus(), Times.Once());
				allTasks[0].Verify(ts => ts.SetNextRunTimeBasedOnRecurrence());
				runner.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));
			}

			[ExpectNoExceptions]
			[TestDate(2014, 8, 18, 13, 56, 0)]
			public void TestScheduleAndDispatch_ReschedulesTaskThatFailedToRun_EvenIfNextRunTimeIsOverwritten()
			{
				using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
				{
					var transactionAdapterMock = Mock.Of<ITransactionAdapter>();
					var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory, active: true);
					schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddHours(1);
					var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1), allowsMultiple: false, actionQueue, transactionAdapter: transactionAdapterMock);
					var allTasks = new Mock<IAllTasksConsumer>();
					allTasks.Setup(a => a.GetAll()).Returns(new[] { task1, });
					allTasks.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task1)).Returns(true);

					var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
					var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
					var loggerMock = new Mock<IHostLogger>();
					var dataSaverFactory = ControllerTest.CreateMockDataSaverFactory();
					var grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
					var grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
					grpcClientSynchronizerFactoryMock
						.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
						.Returns(grpcClientSynchronizerMock.Object);
					grpcClientSynchronizerMock
						.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
						.Returns(true);

					using (var taskScheduler = new TaskScheduler(transactionAdapterMock, allTasks.Object, loggerMock.Object, regChecker.Object, dataSaverFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
					{
						taskScheduler
							.ReconstructRequest((x) => new ScheduledTaskRunRequest(x), "AAA")
							?.OnUnableToRun(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, true, true);

						schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddHours(1);

						var taskSelector = new TaskSelector(new Mock<IProcessRunnerPool>().Object, Mock.Of<IHostLogger>());
						var taskRunner = new Mock<ITaskRunRequestProcessor>();
						taskRunner.Setup(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);

						var schedulerDispatcher = new SchedulerDispatcher(allTasks.Object, taskSelector, taskRunner.Object, loggerMock.Object, Mock.Of<IServiceHostsCache>(), processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueue, new CancellationToken(), errorReporterProxyMock.Object, hostRegistryMock.Object, Mock.Of<IResourceThrottler>());
						var runnerPool = new Mock<IProcessRunnerPool>();
						runnerPool.Setup(rp => rp.RunningCount(It.IsAny<IRunnableServiceTask>())).Returns(0);

						var taskQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);

						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never());

						TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime().Add(TaskScheduler.FailedScheduleRetryTime);
						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						taskRunner.Verify(tr => tr.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));
					}
				}
			}

			[TestDate(2014, 8, 18, 13, 56, 0)]
			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_TaskRunsOncePerPeriod_WhenMultipleDispatchers()
			{
				var schedule = TaskSchedulerTest.CreateSchedule("TST", Factory, active: true, taskPeriod: "M");
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(new[] { new Mock<IServiceHostClient>().Object, new Mock<IServiceHostClient>().Object });
				serviceHostsCache.Setup(sc => sc.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var task = new Mock<DelayedRunnableTask>(false, schedule);
				using (var actionQueue2 = new BackgroundThreadActionQueue(CancellationToken.None))
				using (var task1 = task.Object)
				{
					var allTasks1 = new[] { task.As<IRunnableServiceTask>() };
					var runner1 = new Mock<ITaskRunRequestProcessor>();
					runner1.Setup(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>())).Returns(TaskRunRequestResult.Success);
					var taskSchedulerDispatcher1 = CreateSchedulerDispatcher(allTasks1, runner1.Object, serviceHostsCache.Object);
					var taskQueue1 = CreateMockTaskQueue(allTasks1, directRequest: false);

					var runner2 = new Mock<ITaskRunRequestProcessor>();
					var task2 = TaskSchedulerTest.CreateMockTask(new SchedulerServiceTask(schedule), allowsMultiple: false, actionQueue2);
					var allTasks2 = new[] { task2 };
					var taskSchedulerDispatcher2 = CreateSchedulerDispatcher(allTasks2, runner2.Object, serviceHostsCache.Object);
					var taskQueue2 = CreateMockTaskQueue(allTasks2, directRequest: false);

					var dispatchTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							taskSchedulerDispatcher1.Dispatch(taskQueue1.Object);
						}
					});
					NUnit.Framework.Assert.That(task1.WaitUntilSetNextRunTimeBasedOnRecurrenceCalled(), Is.True);
					try
					{
						taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
						runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never());
					}
					finally
					{
						task1.CompleteSetNextRunTimeBasedOnRecurrence();
						task2.Object.SetNextRunTime(task1.NextScheduledRunTime, SetNextRuntimeReason.ScheduledToRun);
						dispatchTask.Wait();
					}

					runner1.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));

					TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime().Add(TaskScheduler.FailedScheduleRetryTime);
					taskSchedulerDispatcher2.Schedule(taskQueue2.Object);
					taskSchedulerDispatcher2.Dispatch(taskQueue2.Object);
					runner2.Verify(r => r.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()), Times.Never());
				}
			}

			[TestDate(2014, 8, 18, 13, 56, 0)]
			[ExpectNoExceptions]
			public void TestScheduleAndDispatch_NudgeAfterNextRunTimeUpdated()
			{
				hostRegistryMock.Setup(o => o.ForcefullyDisabledTasks).Returns(string.Empty);
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None, Mock.Of<IAsyncDelayProvider>()))
				{
					var loggerMock = new Mock<IHostLogger>();
					var runnerPool = new Mock<IProcessRunnerPool>();
					runnerPool.Setup(rp => rp.RunningCount(It.IsAny<IRunnableServiceTask>())).Returns(0);
					var taskQueue = new TaskQueue(runnerPool.Object, loggerMock.Object);
					var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory, active: true, taskPeriod: "D");
					Factory.Save();
					schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddDays(-2).AddHours(-2);
					var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1), allowsMultiple: false, actionQueue, taskQueue, hostRegistry: hostRegistryMock.Object, transactionAdapter: transactionAdapter);
					var allTasks = new Mock<IAllTasksConsumer>();
					allTasks.Setup(a => a.GetAll()).Returns(new[] { task1, });
					allTasks.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task1)).Returns(true);

					task1.SetNextRunTimeBasedOnRecurrence();
					NUnit.Framework.Assert.That(task1.NextRunTime.Value, Is.GreaterThanOrEqualTo(ZDateTime.UtcNow.ToNullableDateTimeOffset().Value));

					var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
					regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
					var dataSaverFactory = ControllerTest.CreateMockDataSaverFactory();
					using (var taskScheduler = new TaskScheduler(transactionAdapter, allTasks.Object, loggerMock.Object, regChecker.Object, dataSaverFactory, errorReporterProxyMock.Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
					{
						var taskSelector = new TaskSelector(new Mock<IProcessRunnerPool>().Object, Mock.Of<IHostLogger>());
						var resThrottle = new Mock<IResourceThrottler>();
						resThrottle.Setup(rt => rt.WaitForResource()).Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1), new decimal(0), new decimal(0), new decimal(0)));
						var processRunner = new Mock<IServiceRunner>();
						processRunner.Setup(pr => pr.Run(It.IsAny<ITaskRunRequest>())).Returns(true);
						var processRunnerPool = new Mock<IProcessRunnerPool>();
						processRunnerPool
							.Setup(pp => pp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
							.ReturnsAsync(processRunner.Object);
						var taskRunner = new TaskRunner(processRunnerPool.Object, loggerMock.Object);
						var schedulerDispatcher = new SchedulerDispatcher(allTasks.Object, taskSelector, taskRunner, loggerMock.Object, Mock.Of<IServiceHostsCache>(), processRunnerPool.Object, taskSchedulerMock.Object, actionQueue, new CancellationToken(), errorReporterProxyMock.Object, hostRegistryMock.Object, Mock.Of<IResourceThrottler>());

						var result = taskScheduler.ScheduleTasks(new[] { new TaskCodeDTO("AAA") });
						NUnit.Framework.Assert.That(result.Results.First().Value.Outcome, Is.EqualTo(TaskActionOutcomeDTO.EnqueuedNow));

						schedulerDispatcher.Schedule(taskQueue);
						schedulerDispatcher.Dispatch(taskQueue);
						processRunner.Verify(pr => pr.Run(It.IsAny<ITaskRunRequest>()));
					}
				}
			}

			[ExpectNoExceptions]
			public void TestTaskIsReEnqueuedAfterProcessStartFailure()
			{
				// Arrange
				const string taskCode = "xxx";
				const string taskDescription = nameof(TestTaskIsReEnqueuedAfterProcessStartFailure);

				var remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var taskSchedulerMock = new Mock<ITaskScheduler> { Name = taskCode };
				processRunnerPoolMock = new Mock<IProcessRunnerPool> { Name = taskCode };
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue> { Name = taskCode };
				var processFactoryMock = new Mock<IProcessFactory> { Name = taskCode };
				var processMock = new Mock<IProcess> { Name = taskCode };
				var taskQueueMock = new Mock<ITaskQueue> { Name = taskCode };
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask> { Name = taskCode };
				var hostedServiceConfigMock = new Mock<IHostedServiceAttribute> { Name = taskCode };
				hostedServiceConfigMock
					.SetupGet(c => c.TypeName)
					.Returns("someType");
				var taskRunRequestMock = new Mock<IDirectTaskRunRequest> { Name = taskCode };
				var taskSelectorMock = new Mock<ITaskSelector> { Name = taskCode };
				var serviceTaskLocksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
				var grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
				var grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
				grpcClientSynchronizerFactoryMock
					.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
					.Returns(grpcClientSynchronizerMock.Object);

				processMock.Setup(x => x.Start()).Returns(false);
				processFactoryMock.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>())).Returns(processMock.Object);

				var serviceTaskInfo = new ServiceTaskInfo(hostedServiceConfigMock.Object);

				hostedServiceConfigMock.Setup(x => x.Code).Returns(taskCode);
				hostedServiceConfigMock.Setup(x => x.Description).Returns(taskDescription);
				runnableServiceTaskMock.Setup(task => task.Code).Returns(taskCode);
				runnableServiceTaskMock.Setup(task => task.Info).Returns(serviceTaskInfo);
				taskQueueMock.Setup(queue => queue.GetQueueSnapshot()).Returns(new[] { runnableServiceTaskMock.Object });

				var runnableServiceTask = new RunnableServiceTask(serviceTaskInfo, null, backgroundThreadActionQueueMock.Object, taskQueueMock.Object, hostLoggerMock.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

				taskRunRequestMock.Setup(request => request.Task).Returns(runnableServiceTaskMock.Object);
				taskRunRequestMock.Setup(request => request.OnUnableToRun(It.IsAny<UnableToRunReason>(), It.IsAny<bool>(), It.IsAny<bool>()))
					.Callback((UnableToRunReason failureReason, bool retry, bool failedPostScheduleUpdate) => runnableServiceTask.HandleUnableToRun(failureReason, taskRunRequestMock.Object, retry, failedPostScheduleUpdate));
				var taskRunRequest = taskRunRequestMock.Object as ITaskRunRequest;
				taskQueueMock.Setup(queue => queue.TryDequeueTask(out taskRunRequest)).Returns(true);

				var allTasks = Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>());
				using var processServiceRunner = new ProcessServiceRunner(taskSchedulerMock.Object, backgroundThreadActionQueueMock.Object, processFactoryMock.Object, remotingServicesMock.Object, hostLoggerMock.Object, grpcClientSynchronizerFactoryMock.Object, serviceTaskLocksCleanerMock.Object, Mock.Of<IDateTimeProvider>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), string.Empty, Mock.Of<IProductRegistration>());
				processRunnerPoolMock
					.Setup(pool => pool.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(processServiceRunner);
				var taskRunRequestProcessorMock = new Mock<ITaskRunRequestProcessor>();
				taskRunRequestProcessorMock
					.Setup(p => p.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()))
					.Returns(TaskRunRequestResult.ProcessDidNotStart);

				var schedulerDispatcher = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunRequestProcessorMock.Object, hostLoggerMock.Object, Mock.Of<IServiceHostsCache>(), processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, new CancellationToken(), errorReporterProxyMock.Object, hostRegistryMock.Object, Mock.Of<IResourceThrottler>());

				// Act
				schedulerDispatcher.Dispatch(taskQueueMock.Object);

				// Assert
				taskRunRequestMock.Verify(x => x.OnUnableToRun(UnableToRunReason.RunnerProcessDidNotStart, true, true), Times.Once);
			}

			public void TestWrongParamsCall()
			{
				NUnit.Framework.Assert.Multiple(() =>
				{
					var allTasks = Mock.Of<IAllTasksConsumer>();
					var taskSelectorMock = new Mock<ITaskSelector>();
					var taskRunnerMock = new Mock<ITaskRunRequestProcessor>();
					var loggerMock = new Mock<IHostLogger>();
					var serviceHostsCache = new Mock<IServiceHostsCache>();

					var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(null, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("allTasks"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, null, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskSelector"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, null, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("runner"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, null, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("logger"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, null, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceHostsCache"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, null, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("processRunnerPool"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, null, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskScheduler"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, null, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("actionQueue"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, null, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, null, Mock.Of<IResourceThrottler>()));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostRegistry"));

					result = AssertExceptionThrown<ArgumentNullException>(() => _ = new SchedulerDispatcher(allTasks, taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCache.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), null));
					NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("resourceThrottler"));
				});
			}

			static ITaskRunRequest CreateRequest(Mock<IRunnableServiceTask> task, bool directRequest)
			{
				return directRequest
					? Mock.Of<IDirectTaskRunRequest>(x => x.Task == task.Object)
					: Mock.Of<IScheduledTaskRunRequest>(x => x.Task == task.Object);
			}

			static Mock<ITaskQueue> CreateMockTaskQueue(ICollection<Mock<IRunnableServiceTask>> tasks, bool directRequest)
			{
				var request = CreateRequest(tasks.Single(), directRequest);
				return CreateMockTaskQueue(tasks, request);
			}

			static Mock<ITaskQueue> CreateMockTaskQueue(ICollection<Mock<IRunnableServiceTask>> tasks, ITaskRunRequest taskRequest)
			{
				var taskQueue = new Mock<ITaskQueue>();
				taskQueue.Setup(tq => tq.TryDequeueTask(out taskRequest)).Returns(true);
				taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(tasks.Select(x => x.Object));

				return taskQueue;
			}

			SchedulerDispatcher CreateSchedulerDispatcher(ICollection<Mock<IRunnableServiceTask>> allTasks, ITaskRunRequestProcessor runner, IServiceHostsCache serviceHostsCache)
			{
				return CreateSchedulerDispatcher(allTasks, runner, new List<IRunnableServiceTask>(), serviceHostsCache);
			}

			SchedulerDispatcher CreateSchedulerDispatcher(ICollection<Mock<IRunnableServiceTask>> allTasks, ITaskRunRequestProcessor runner, ICollection<IRunnableServiceTask> tasksToSchedule, IServiceHostsCache serviceHostsCache)
			{
				return CreateSchedulerDispatcher(allTasks, runner, tasksToSchedule, new Mock<IHostLogger>(), serviceHostsCache);
			}

			SchedulerDispatcher CreateSchedulerDispatcher(ICollection<Mock<IRunnableServiceTask>> allTasks, ITaskRunRequestProcessor runner, ICollection<IRunnableServiceTask> tasksToSchedule, Mock<IHostLogger> logger, IServiceHostsCache serviceHostsCache)
			{
				var taskSelector = new Mock<ITaskSelector>();
				taskSelector.Setup(ts => ts.SelectTasksToRun(It.IsAny<IEnumerable<IRunnableServiceTask>>())).Returns(tasksToSchedule.Select(rt => new ScheduledTaskRunRequest(rt)));
				return new SchedulerDispatcher(Mock.Of<IAllTasksConsumer>(a => a.GetAll() == allTasks.Select(x => x.Object)), taskSelector.Object, runner, logger.Object, serviceHostsCache, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, new CancellationToken(), errorReporterProxyMock.Object, hostRegistryMock.Object, resourceThrottlerMock.Object);
			}

			static List<Mock<IRunnableServiceTask>> CreateMockTasks(string[] taskCodes, bool allowMultipleInstances = false)
			{
				var taskConfigs = new List<Mock<IHostedServiceAttribute>>();
				foreach (var taskCode in taskCodes)
				{
					var serviceConfig = new Mock<IHostedServiceAttribute>();
					serviceConfig.Setup(s => s.Code).Returns(taskCode);
					serviceConfig.Setup(s => s.AllowsMultipleInstances).Returns(allowMultipleInstances);
					taskConfigs.Add(serviceConfig);
				}

				return CreateMockTasks(taskConfigs.ToArray());
			}

			static List<Mock<IRunnableServiceTask>> CreateMockTasks(Mock<IHostedServiceAttribute>[] taskConfigs)
			{
				var allTasks = new List<Mock<IRunnableServiceTask>>();
				foreach (var taskConfig in taskConfigs)
				{
					var logger = Mock.Of<IHostLogger>();
					var task = new Mock<IRunnableServiceTask>();
					task.Setup(t => t.Code).Returns(taskConfig.Object.Code);
					task.Setup(t => t.Info).Returns(new ServiceTaskInfo(taskConfig.Object));
					task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
					allTasks.Add(task);
				}

				return allTasks;
			}

			List<IRunnableServiceTask> CreateTasks(string[] taskCodes, bool allowMultipleInstances = false)
			{
				var tasks = new List<IRunnableServiceTask>();
				foreach (var taskCode in taskCodes)
				{
					tasks.Add(TaskSchedulerTest.CreateTask(taskCode, Factory, allowMultipleInstances));
				}

				return tasks;
			}

			static List<Mock<IRunnableServiceTask>> CreateTasks(ServiceTaskSchedule[] taskSchedules, bool allowMultipleInstances = false, IBackgroundThreadActionQueue actionQueue = null, ITransactionAdapter transactionAdapter = null)
			{
				var tasks = new List<Mock<IRunnableServiceTask>>();
				foreach (var taskSchedule in taskSchedules)
				{
					tasks.Add(TaskSchedulerTest.CreateMockTask(new SchedulerServiceTask(taskSchedule), allowMultipleInstances, actionQueue, transactionAdapter: transactionAdapter));
				}

				return tasks;
			}

			internal class DelayedRunnableTask : IRunnableServiceTask, IDisposable
			{
				readonly AutoResetEvent SetNextRunTimeBasedOnRecurrenceCalled;
				readonly AutoResetEvent SetNextRunTimeBasedOnRecurrenceComplete;

				public IServiceTaskInfo Info { get; }
				public bool NextRunTimeIsInFuture => false;
				public DateTimeOffset? LastRunTime { get; set; }
				public bool IsOverdue => false;
				public string Code => Info.Code;
				public string Branch => throw new NotImplementedException();

				public bool HasSchedule => true;
				public bool IsDisabled => throw new NotImplementedException();
				public bool IsLastRunFailed => throw new NotImplementedException();
				public int MaxSecondaryRunningCount => 10;
				public bool IsActive => throw new NotImplementedException();
				public Stopwatch TimeSinceLastEnqueued { get; set; }
				public Stopwatch TimeSinceLastDequeued { get; set; }
				public Stopwatch TimeSinceLastStarted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
				public Stopwatch TimeRunning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
				public TimeSpan OverdueDuration => throw new NotImplementedException();
				public DateTimeOffset? NextScheduledRunTime { get; set; }
				public string ConfigString => throw new NotImplementedException();
				public string ScheduleDescription => throw new NotImplementedException();
				public string TypeOfDocument => throw new NotImplementedException();
				public TimeSpan SchedulePeriodDuration => throw new NotImplementedException();
				public DateTimeOffset? NextRunTimeAllowingForLocalSchedulingFailures => throw new NotImplementedException();
				public DateTimeOffset? NextRunTime => ZDateTime.UtcNow.ToNullableDateTimeOffset();
				public DateTimeOffset? FailedScheduleRetryTime => null;
				public DateTimeOffset? LastErrorTime => throw new NotImplementedException();

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
				public int ErrorCountLast24Hours => throw new NotImplementedException();
				public IHostLogger HostLogger => _hostLogger.Object;

				public Mock<IHostLogger> _hostLogger { get; private set; }
				public IDirectTaskRunRequest LastDirectRunRequest => throw new NotImplementedException();

				readonly ServiceTaskSchedule schedule;

				public DelayedRunnableTask(bool allowMultipleInstances, ServiceTaskSchedule schedule)
				{
					this.schedule = schedule;
					_hostLogger = new Mock<IHostLogger>();

					var serviceConfig = new Mock<IHostedServiceAttribute>();
					serviceConfig.Setup(s => s.Code).Returns(schedule.S5_ScheduleType);
					serviceConfig.Setup(s => s.AllowsMultipleInstances).Returns(allowMultipleInstances);
					Info = new ServiceTaskInfo(serviceConfig.Object);

					SetNextRunTimeBasedOnRecurrenceCalled = new AutoResetEvent(false);
					SetNextRunTimeBasedOnRecurrenceComplete = new AutoResetEvent(false);
					TimeSinceLastEnqueued = new Stopwatch();
					TimeSinceLastDequeued = new Stopwatch();
				}

				public void Dispose()
				{
					SetNextRunTimeBasedOnRecurrenceCalled.Dispose();
					SetNextRunTimeBasedOnRecurrenceComplete.Dispose();
				}

				public void SetNextRunTimeBasedOnRecurrence()
				{
					SetNextRunTimeBasedOnRecurrenceCalled.Set();
					SetNextRunTimeBasedOnRecurrenceComplete.WaitOne();
				}

				public void UpdateStatus() { }

				public bool WaitUntilSetNextRunTimeBasedOnRecurrenceCalled()
				{
					return SetNextRunTimeBasedOnRecurrenceCalled.WaitOne();
				}

				public void CompleteSetNextRunTimeBasedOnRecurrence(bool updateNextRunTime = true)
				{
					if (updateNextRunTime)
					{
						NextScheduledRunTime = schedule.CalculateNextRunTime(HostLogger).ToNullableDateTimeOffset();
					}

					SetNextRunTimeBasedOnRecurrenceComplete.Set();
				}

				public void OnSuccessfulRunAttempt(ITaskRunRequest request) => LastRunTime = ZDateTimeOffset.UtcNow.ToDateTimeOffset();

				public TaskRunRequestResult ValidateForRun() => throw new NotImplementedException();
				public void UpdateSchedule(IEnumerable<IServiceTask> schedules, bool reEnableMandatory) => throw new NotImplementedException();
				public void ReloadConfigurationAsync() => throw new NotImplementedException();
				public void ClearFailedRunAttempt() => throw new NotImplementedException();
				public void HandleUnableToRun(UnableToRunReason failureReason, ITaskRunRequest request, bool retry, bool failedPostScheduleUpdate) => throw new NotImplementedException();
				public void OnErrorReported() => throw new NotImplementedException();
				public void RecordLastRunError() => throw new NotImplementedException();
				public void SetNextRunTime(DateTimeOffset? value, SetNextRuntimeReason operation, bool? revertingAfterFailedRun = null) => throw new NotImplementedException();
				public void EnqueueDelayed(TimeSpan delaySpan, bool echoes) => throw new NotImplementedException();
				public void ReEnqueueDelayed(TimeSpan delaySpan, ITaskRunRequest request) => throw new NotImplementedException();
				public bool Enqueue(bool echoes) => throw new NotImplementedException();
				public void UpdateUnderlyingScheduleNextRunTime() => throw new NotImplementedException();
				public bool HasScheduleUpdates(IServiceTask serviceTaskSchedule) => throw new NotImplementedException();
				public void Log(LogLevel logLevel, string message) { }
				public void OnQueuedResponse(IServiceRunner serviceRunner, ITaskRunRequest request) => throw new NotImplementedException();
				public void OnSuccessfulRun(ITaskRunRequest request) { }
			}
		}

		public class ResourceThrottlerUsageTest : SchedulerDispatcherTest
		{
			protected override void SetUp()
			{
				base.SetUp();

				taskRunnerMock = new Mock<ITaskRunRequestProcessor>();
				loggerMock = new Mock<IHostLogger>();
				resourceThrottlerMock = new Mock<IResourceThrottler>();

				var hostRegistryMock = new Mock<IHostRegistrySettings>();
				hostRegistryMock.Setup(o => o.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks).Returns(80);
				hostRegistryMock.Setup(o => o.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks).Returns(1);
				hostRegistryMock.Setup(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(1));

				taskQueueMock = new Mock<ITaskQueue>();
			}

			ISchedulerDispatcher CreateSchedulerDispatcher()
			{
				return new SchedulerDispatcher(Mock.Of<IAllTasksConsumer>(a => a.GetAll() == Enumerable.Empty<IRunnableServiceTask>()), Mock.Of<ITaskSelector>(), taskRunnerMock.Object, loggerMock.Object, Mock.Of<IServiceHostsCache>(), processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, new Mock<IErrorReporterProxy>().Object, hostRegistryMock.Object, resourceThrottlerMock.Object);
			}

			[ExpectNoExceptions]
			public void TestDispatchCallsResourceCheckForEachBatch()
			{
				// Arrange
				var queueSize = 5;
				var batchSize = 1;
				hostRegistryMock.SetupGet(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(batchSize);

				var taskMocks = Enumerable
					.Range(0, queueSize)
					.Select(i =>
					{
						var directTaskRunRequestMock = new Mock<IDirectTaskRunRequest>();
						var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
						directTaskRunRequestMock
							.SetupGet(request => request.Task)
							.Returns(runnableServiceTaskMock.Object);

						return (directTaskRunRequestMock, runnableServiceTaskMock);
					})
					.ToList();

				ITaskRunRequest dequeued = taskMocks[0].directTaskRunRequestMock.Object;
				var tryDequeueTaskCount = 0;
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(taskMocks.Select(tuple => tuple.runnableServiceTaskMock.Object));
				taskQueueMock
					.Setup(queue => queue.TryDequeueTask(out dequeued))
					.Callback(() =>
					{
						dequeued = taskMocks[++tryDequeueTaskCount].directTaskRunRequestMock.Object;
						taskQueueMock
							.Setup(queue => queue.TryDequeueTask(out dequeued))
							.Returns(true);
					})
					.Returns(true);

				// Act
				CreateSchedulerDispatcher().Dispatch(taskQueueMock.Object);

				// Assert
				resourceThrottlerMock.Verify(throttler => throttler.WaitForResource(), Times.Exactly(queueSize));
			}

			[ExpectNoExceptions]
			public void TestDispatchExitsLoopOnNoResources()
			{
				// Arrange
				var directTaskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				var taskRunRequest = (ITaskRunRequest)directTaskRunRequestMock.Object;
				directTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);

				var serviceRunnerMock = Mock.Of<IServiceRunner>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(serviceRunnerMock);

				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new[] { runnableServiceTaskMock.Object });
				taskQueueMock
					.Setup(q => q.TryDequeueTask(out taskRunRequest))
					.Returns(true);

				resourceThrottlerMock
					.Setup(throttler => throttler.WaitForResource())
					.Returns(new ResourceThrottlerResult(true, default, default, default, default, default));

				// Act
				CreateSchedulerDispatcher().Dispatch(taskQueueMock.Object);

				// Assert
				resourceThrottlerMock.Verify(throttler => throttler.WaitForResource(), Times.Once);
				taskRunnerMock.Verify(processor => processor.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Never);
			}

			[ExpectNoExceptions]
			public void TestDispatchLogsOnNoResources()
			{
				// Arrange
				var directTaskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				var taskRunRequest = (ITaskRunRequest)directTaskRunRequestMock.Object;
				directTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);

				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new[] { runnableServiceTaskMock.Object });
				taskQueueMock
					.Setup(q => q.TryDequeueTask(out taskRunRequest))
					.Returns(true);

				resourceThrottlerMock
					.Setup(throttler => throttler.WaitForResource())
					.Returns(new ResourceThrottlerResult(true, default, default, default, default, default));

				// Act
				CreateSchedulerDispatcher().Dispatch(taskQueueMock.Object);

				// Assert
				resourceThrottlerMock.Verify(throttler => throttler.WaitForResource(), Times.Once);
				loggerMock.Verify(
					logger => logger.Log(
						LogLevel.Warning,
						It.Is<string>(s => s.StartsWith("Starting dispatch round delayed due to resource contention. Waited for ", StringComparison.OrdinalIgnoreCase))),
					Times.Once);
			}

			[ExpectNoExceptions]
			public void TestDispatchDoesNotDequeueOnNoResources()
			{
				// Arrange
				var directTaskRunRequestMock = new Mock<IDirectTaskRunRequest>();
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				var taskRunRequest = (ITaskRunRequest)directTaskRunRequestMock.Object;
				directTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);

				var dequeued = (ITaskRunRequest)directTaskRunRequestMock.Object;
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new[] { runnableServiceTaskMock.Object });
				taskQueueMock
					.Setup(q => q.TryDequeueTask(out taskRunRequest))
					.Returns(true);

				resourceThrottlerMock
					.Setup(throttler => throttler.WaitForResource())
					.Returns(new ResourceThrottlerResult(true, default, default, default, default, default));

				// Act
				CreateSchedulerDispatcher().Dispatch(taskQueueMock.Object);

				// Assert
				resourceThrottlerMock.Verify(throttler => throttler.WaitForResource(), Times.Once);
				taskQueueMock.Verify(queue => queue.TryDequeueTask(out dequeued), Times.Never);
			}

			Mock<IHostLogger> loggerMock;
			Mock<ITaskQueue> taskQueueMock;
			Mock<ITaskRunRequestProcessor> taskRunnerMock;
		}

		public class DispatchTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				var taskSelectorMock = new Mock<ITaskSelector>();
				taskRunnerMock = new Mock<ITaskRunRequestProcessor>();
				var loggerMock = new Mock<IHostLogger>();
				serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				var resourceThrottlerMock = new Mock<IResourceThrottler>();
				processRunnerPoolMock = new Mock<IProcessRunnerPool>();
				taskSchedulerMock = new Mock<ITaskScheduler>();
				actionQueueMock = new Mock<IBackgroundThreadActionQueue>();

				var hostRegistryMock = new Mock<IHostRegistrySettings>();
				hostRegistryMock.Setup(o => o.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks).Returns(80);
				hostRegistryMock.Setup(o => o.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks).Returns(1);
				hostRegistryMock.Setup(o => o.ServiceTaskMaxWaitForResourceAvailability).Returns(TimeSpan.FromSeconds(1));
				hostRegistryMock.Setup(o => o.ServiceTaskProcessingMaximumBatchSize).Returns(20);

				schedulerDispatcher = new SchedulerDispatcher(Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>()), taskSelectorMock.Object, taskRunnerMock.Object, loggerMock.Object, serviceHostsCacheMock.Object, processRunnerPoolMock.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, new Mock<IErrorReporterProxy>().Object, hostRegistryMock.Object, resourceThrottlerMock.Object);

				taskQueueMock = new Mock<ITaskQueue>();
			}

			[ExpectNoExceptions]
			public void TestDispatchScheduledRequestSetsNextRunTime_OneHost()
			{
				// Arrange
				taskRunnerMock.Reset();

				var serviceRunnerMock = Mock.Of<IServiceRunner>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(serviceRunnerMock);
				var scheduledTaskRunRequestMock = new Mock<IScheduledTaskRunRequest>();
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				scheduledTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);

				var dequeued = (ITaskRunRequest)scheduledTaskRunRequestMock.Object;
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new[] { runnableServiceTaskMock.Object });
				taskQueueMock
					.Setup(queue => queue.TryDequeueTask(out dequeued))
					.Returns(true);

				var sequence = new MockSequence();
				scheduledTaskRunRequestMock
					.InSequence(sequence)
					.Setup(request => request.TakeNextRunTimeFromTask());
				taskRunnerMock
					.InSequence(sequence)
					.Setup(processor => processor.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));

				// Act
				schedulerDispatcher.Dispatch(taskQueueMock.Object);

				// Assert
				scheduledTaskRunRequestMock.Verify(request => request.TakeNextRunTimeFromTask(), Times.Once);
				taskRunnerMock.Verify(processor => processor.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestDispatchScheduledRequestSetsNextRunTime_ManyHosts()
			{
				// Arrange
				taskRunnerMock.Reset();

				var scheduledTaskRunRequestMock = new Mock<IScheduledTaskRunRequest>();
				var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
				scheduledTaskRunRequestMock
					.SetupGet(request => request.Task)
					.Returns(runnableServiceTaskMock.Object);

				var serviceRunnerMock = Mock.Of<IServiceRunner>();
				processRunnerPoolMock
					.Setup(p => p.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(serviceRunnerMock);
				var dequeued = (ITaskRunRequest)scheduledTaskRunRequestMock.Object;
				taskQueueMock
					.Setup(queue => queue.GetQueueSnapshot())
					.Returns(new[] { runnableServiceTaskMock.Object });
				taskQueueMock
					.Setup(queue => queue.TryDequeueTask(out dequeued))
					.Returns(true);

				serviceHostsCacheMock
					.SetupGet(cache => cache.ConfiguredServiceHosts)
					.Returns(new[] { new Mock<IServiceHostClient>().Object, new Mock<IServiceHostClient>().Object, });

				var sequence = new MockSequence();
				scheduledTaskRunRequestMock
					.InSequence(sequence)
					.Setup(request => request.TakeNextRunTimeFromTask());
				taskRunnerMock
					.InSequence(sequence)
					.Setup(processor => processor.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), It.IsAny<IServiceRunner>()));

				// Act
				schedulerDispatcher.Dispatch(taskQueueMock.Object);

				// Assert
				scheduledTaskRunRequestMock.Verify(request => request.TakeNextRunTimeFromTask(), Times.Once);
				taskRunnerMock.Verify(processor => processor.ProcessRunRequest(It.IsAny<ITaskRunRequest>(), serviceRunnerMock), Times.Once);
			}

			SchedulerDispatcher schedulerDispatcher;
			Mock<ITaskQueue> taskQueueMock;
			Mock<ITaskRunRequestProcessor> taskRunnerMock;
			Mock<IServiceHostsCache> serviceHostsCacheMock;
			Mock<IProcessRunnerPool> processRunnerPoolMock;
			Mock<ITaskScheduler> taskSchedulerMock;
			Mock<IBackgroundThreadActionQueue> actionQueueMock;
		}
	}
}
