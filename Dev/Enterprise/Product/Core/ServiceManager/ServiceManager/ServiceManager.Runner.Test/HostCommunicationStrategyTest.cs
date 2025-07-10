using System;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManagerProto;

namespace CargoWise.ServiceManager.Runner.Test
{
	class HostCommunicationStrategyTest
	{
		[SetUp]
		public void SetUp()
		{
			serviceHostMessageDispatcherMock = new Mock<IServiceHostMessageDispatcher>();
			serviceTaskRunnerQueueMock = new Mock<IServiceTaskRunnerQueue>();
			hostCommunicationStrategy = new HostCommunicationStrategy(serviceHostMessageDispatcherMock.Object);
		}

		HostCommunicationStrategy hostCommunicationStrategy;
		Mock<IServiceHostMessageDispatcher> serviceHostMessageDispatcherMock;
		Mock<IServiceTaskRunnerQueue> serviceTaskRunnerQueueMock;

		public class CompletedTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestCompleted()
			{
				// Arrange
				// Act
				Assert.DoesNotThrow(() => hostCommunicationStrategy.Completed(serviceTaskRunnerQueueMock.Object));

				// Assert
				serviceTaskRunnerQueueMock.Verify(dispatcher => dispatcher.EnqueueResponse(
					It.Is<ServiceTaskRunResponse>(
						r =>
							r.Command == ResponseCommandType.Idle
							&& r.Status == Status.ProcessFinished)), Times.Once);
			}
		}

		public class RunnerCancelledTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestWrongParamsCall()
			{
				var result = Assert.Throws<ArgumentNullException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, null));
				Assert.That(result, Is.InstanceOf<ArgumentException>());
				Assert.That(result.ParamName, Is.EqualTo("commandInfo"));

				var result2 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<IStopCommandInfo>()));
				Assert.That(result2, Is.InstanceOf<ArgumentException>());
				Assert.That(result2.ParamName, Is.EqualTo("commandInfo"));

				var result3 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<ICommandInfo>()));
				Assert.That(result3, Is.InstanceOf<ArgumentException>());
				Assert.That(result3.ParamName, Is.EqualTo("commandInfo"));
			}
		}

		public class ServiceTaskLockNotAcquiredTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestDirectCommandIsReQueued()
			{
				Assert.Multiple(() =>
				{
					Test("TST", Guid.Parse("00000000-1111-0000-0000-000000000000"));
					Test("TSK", Guid.Parse("00000000-0000-1111-0000-000000000000"));
				});

				void Test(string taskCode, Guid id)
				{
					// Arrange
					var commandMock = Mock.Of<IDirectRunCommandInfo>(
						info => info.Code == taskCode
								&& info.Id == id);
					serviceTaskRunnerQueueMock.Invocations.Clear();

					// Act
					Assert.DoesNotThrow(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, commandMock));

					// Assert
					serviceTaskRunnerQueueMock.Verify(dispatcher => dispatcher.EnqueueResponse(
						It.Is<ServiceTaskRunResponse>(
							r =>
								r.Command == ResponseCommandType.Reenqueue
								&& r.Status == Status.ProcessFinished
								&& r.TaskCode == taskCode
								&& r.TaskId == id.ToString()
								&& r.FailureReason == FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock)), Times.Once);
				}
			}

			[Test]
			public void TestScheduledCommandIsCompleted()
			{
				Assert.Multiple(() =>
				{
					Test("TST", Guid.Parse("00000000-1111-0000-0000-000000000000"));
					Test("TSK", Guid.Parse("00000000-0000-1111-0000-000000000000"));
				});

				void Test(string taskCode, Guid id)
				{
					// Arrange
					var commandMock = Mock.Of<IScheduledRunCommandInfo>(
						info => info.Code == taskCode
								&& info.Id == id);
					serviceTaskRunnerQueueMock.Invocations.Clear();

					// Act
					Assert.DoesNotThrow(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, commandMock));

					// Assert
					serviceTaskRunnerQueueMock.Verify(dispatcher => dispatcher.EnqueueResponse(
						It.Is<ServiceTaskRunResponse>(
							r =>
								r.Command == ResponseCommandType.Idle
								&& r.Status == Status.ProcessFinished
								&& r.FailureReason == FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock)), Times.Once);
				}
			}

			[Test]
			public void TestWrongParamsCall()
			{
				var result = Assert.Throws<ArgumentNullException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, null));
				Assert.That(result, Is.InstanceOf<ArgumentException>());
				Assert.That(result.ParamName, Is.EqualTo("commandInfo"));

				var result2 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<IStopCommandInfo>()));
				Assert.That(result2, Is.InstanceOf<ArgumentException>());
				Assert.That(result2.ParamName, Is.EqualTo("commandInfo"));

				var result3 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.ServiceTaskLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<ICommandInfo>()));
				Assert.That(result3, Is.InstanceOf<ArgumentException>());
				Assert.That(result3.ParamName, Is.EqualTo("commandInfo"));
			}
		}

		public class GroupLockNotAcquiredTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestDirectCommandIsReQueued()
			{
				Assert.Multiple(() =>
				{
					Test("TST", Guid.Parse("00000000-1111-0000-0000-000000000000"));
					Test("TSK", Guid.Parse("00000000-0000-1111-0000-000000000000"));
				});

				void Test(string taskCode, Guid id)
				{
					// Arrange
					var commandMock = Mock.Of<IDirectRunCommandInfo>(
						info => info.Code == taskCode
								&& info.Id == id);
					serviceTaskRunnerQueueMock.Invocations.Clear();

					// Act
					Assert.DoesNotThrow(() => hostCommunicationStrategy.GroupLockNotAcquired(serviceTaskRunnerQueueMock.Object, commandMock));

					// Assert
					serviceTaskRunnerQueueMock.Verify(dispatcher => dispatcher.EnqueueResponse(
						It.Is<ServiceTaskRunResponse>(
							r =>
								r.Command == ResponseCommandType.Reenqueue
								&& r.Status == Status.ProcessFinished
								&& r.TaskCode == taskCode
								&& r.TaskId == id.ToString()
								&& r.FailureReason == FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock)), Times.Once);
				}
			}

			[Test]
			public void TestScheduledCommandIsReScheduled()
			{
				Assert.Multiple(() =>
				{
					Test("TST", Guid.Parse("00000000-1111-0000-0000-000000000000"));
					Test("TSK", Guid.Parse("00000000-0000-1111-0000-000000000000"));
				});

				void Test(string taskCode, Guid id)
				{
					// Arrange
					var commandMock = Mock.Of<IScheduledRunCommandInfo>(
						info => info.Code == taskCode
								&& info.Id == id);
					serviceTaskRunnerQueueMock.Invocations.Clear();

					// Act
					Assert.DoesNotThrow(() => hostCommunicationStrategy.GroupLockNotAcquired(serviceTaskRunnerQueueMock.Object, commandMock));

					// Assert
					serviceTaskRunnerQueueMock.Verify(dispatcher => dispatcher.EnqueueResponse(
						It.Is<ServiceTaskRunResponse>(
							r =>
								r.Command == ResponseCommandType.ScheduleNextRunTime
								&& r.Status == Status.ProcessFinished
								&& r.TaskCode == taskCode
								&& r.TaskId == id.ToString()
								&& r.FailureReason == FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock)), Times.Once);
				}
			}

			[Test]
			public void TestWrongParamsCall()
			{
				var result = Assert.Throws<ArgumentNullException>(() => hostCommunicationStrategy.GroupLockNotAcquired(serviceTaskRunnerQueueMock.Object, null));
				Assert.That(result, Is.InstanceOf<ArgumentException>());
				Assert.That(result.ParamName, Is.EqualTo("commandInfo"));

				var result2 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.GroupLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<IStopCommandInfo>()));
				Assert.That(result2, Is.InstanceOf<ArgumentException>());
				Assert.That(result2.ParamName, Is.EqualTo("commandInfo"));

				var result3 = Assert.Throws<ArgumentOutOfRangeException>(() => hostCommunicationStrategy.GroupLockNotAcquired(serviceTaskRunnerQueueMock.Object, Mock.Of<ICommandInfo>()));
				Assert.That(result3, Is.InstanceOf<ArgumentException>());
				Assert.That(result3.ParamName, Is.EqualTo("commandInfo"));
			}
		}

		public class GrpcPortOpenedTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestSendPort()
			{
				Assert.Multiple(() =>
				{
					Test(10);
					Test(20);
				});

				void Test(int port)
				{
					// Arrange
					serviceHostMessageDispatcherMock.Invocations.Clear();

					// Act
					hostCommunicationStrategy.GrpcPortOpened(port);

					// Assert
					Assert.DoesNotThrow(() =>
						serviceHostMessageDispatcherMock.Verify(dispatcher => dispatcher.SendGrpcPortLockAcquired(port), Times.Once));
					Assert.DoesNotThrow(() =>
						serviceHostMessageDispatcherMock.VerifyNoOtherCalls());
				}
			}
		}

		public class MiscellaneousTest : HostCommunicationStrategyTest
		{
			[Test]
			public void TestWrongParamsCall()
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new HostCommunicationStrategy(null));
				Assert.That(result.ParamName, Is.EqualTo("serviceHostMessageDispatcher"));
			}
		}
	}
}
