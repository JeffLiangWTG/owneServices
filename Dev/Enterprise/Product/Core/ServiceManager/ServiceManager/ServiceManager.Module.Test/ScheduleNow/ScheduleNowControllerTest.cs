using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Module.ScheduleNow;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;

namespace Enterprise.ServiceManager.Module.Testing.ScheduleNow
{
	class ScheduleNowControllerTest : TestCase
	{
		public void TestWrongParamsCall()
		{
			var serviceHostClientMock = new Mock<IServiceHostClient>();

			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new ScheduleNowController().ScheduleNow(null, s => { }, It.IsAny<bool>()));
				AssertEquals("taskCodes", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ScheduleNowController().ScheduleNow(Enumerable.Empty<string>(), null, It.IsAny<bool>()));
				AssertEquals("callbackAction", result.ParamName);
			});
		}

		public class ScheduleNowTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				scheduleNowController = new ScheduleNowController();
				serviceHostClientMock = new Mock<IServiceHostClient>();
			}

			public void TestNoAvailableHostsCallsCallback()
			{
				// Arrange
				var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock
					.SetupGet(cache => cache.ConfiguredServiceHosts)
					.Returns(Enumerable.Empty<IServiceHostClient>());

				using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
				{
					// Act
					string result = null;
					scheduleNowController.ScheduleNow(Enumerable.Empty<string>(), s => result = s, It.IsAny<bool>());

					// Assert
					AssertContains("There are no Hosts available.", result, true);
				}
			}

			public void TestScheduleNowIgnoresErrorResults()
			{
				// Arrange
				var taskCodes = new[] { "TST" };

				var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock.Setup(cache => cache.ConfiguredServiceHosts).Returns(new[] { Mock.Of<IServiceHostClient>(), Mock.Of<IServiceHostClient>() });
				serviceHostsCacheMock
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient,TasksActionResultDTO>>()))
					.Returns(new AggregateResult<TasksActionResultDTO>(new[]
					{
						new AggregateResultEntry<TasksActionResultDTO>(
							new ServiceHostName("faulty.host"), new ServiceHostCommunicationException("Communication error.")),
						new AggregateResultEntry<TasksActionResultDTO>(
							new ServiceHostName("fastest.host"), new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()))
					}));

				var messages = new List<string>();
				using var countdownEvent = new CountdownEvent(initialCount: 5);

				using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
				{
					// Act
					scheduleNowController.ScheduleNow(taskCodes, AddMessageAndSignal, It.IsAny<bool>());
					scheduleNowController.ScheduleNow(taskCodes, AddMessageAndSignal, It.IsAny<bool>());
					scheduleNowController.ScheduleNow(taskCodes, AddMessageAndSignal, It.IsAny<bool>());
					scheduleNowController.ScheduleNow(taskCodes, AddMessageAndSignal, It.IsAny<bool>());
					scheduleNowController.ScheduleNow(taskCodes, AddMessageAndSignal, It.IsAny<bool>());
				}

				// Assert
				while (!countdownEvent.Wait(TimeSpan.FromSeconds(1)))
				{
					Application.DoEvents();
				}
				var expectedMessages = Enumerable.Repeat($"Host: fastest.host{System.Environment.NewLine}", 5).ToArray();
				var actualMessages = messages.ToArray();

				AssertArrayEqualsByElements(expectedMessages, actualMessages);

				void AddMessageAndSignal(string message)
				{
					messages.Add(message);
					countdownEvent.Signal();
				}
			}

			[ExpectNoExceptions]
			public void TestCallsServiceHostsCache()
			{
				CombineAssertions(() =>
				{
					Test(forceRestart: true, "task");
					Test(forceRestart: false, "task");
					Test(forceRestart: true, "task1", "task2");
					Test(forceRestart: false, "task1", "task2");
				});

				void Test(bool forceRestart, params string[] taskCodes)
				{
					// Arrange
					var resetEvent = new ManualResetEvent(false);
					serviceHostClientMock.Reset();

					var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
					serviceHostsCacheMock
						.SetupGet(cache => cache.ConfiguredServiceHosts)
						.Returns(new[] { serviceHostClientMock.Object });

					var schedulingOutcome = new TasksActionResultDTO(taskCodes
						.Select(taskCode => (dto: new TaskCodeDTO(taskCode), outcome: TaskActionResultDTO.Succeeded))
						.ToDictionary(tuple => tuple.dto, tuple => tuple.outcome));

					serviceHostsCacheMock
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TasksActionResultDTO>>()))
						.Returns(() => new AggregateResult<TasksActionResultDTO>(new[]
						{
							new AggregateResultEntry<TasksActionResultDTO>("hostName", schedulingOutcome)
						}))
						.Callback((Func<IServiceHostClient, TasksActionResultDTO>  invocationFunc) =>
						{
							invocationFunc(serviceHostClientMock.Object);
							resetEvent.Set();
						});

					using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
					{
						// Act
						scheduleNowController.ScheduleNow(taskCodes, s => { }, forceRestart: forceRestart);
					}

					// Assert
					resetEvent.WaitOne();
					serviceHostClientMock.Verify(client => client.InvokeActionOnTasks("schedule", taskCodes, "echoes=0", $"force={forceRestart}", $"user={GlbStaff.CurrentUser.GS_Code}"), Times.Once);
				}
			}

			public void TestReturnsExpectedErrorResult()
			{
				CombineAssertions(() =>
				{
					Test(new Exception("message"));
					Test(new InvalidOperationException("some other message"));
				});

				void Test(Exception exception)
				{
					// Arrange
					var resetEvent = new ManualResetEvent(false);
					serviceHostClientMock.Reset();

					var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
					serviceHostsCacheMock
						.SetupGet(cache => cache.ConfiguredServiceHosts)
						.Returns(new[] { serviceHostClientMock.Object });

					serviceHostsCacheMock
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TasksActionResultDTO>>()))
						.Returns(new AggregateResult<TasksActionResultDTO>(new[] { new AggregateResultEntry<TasksActionResultDTO>("hostName", exception) }));

					var result = new List<string>();

					using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
					{
						// Act
						scheduleNowController.ScheduleNow(Enumerable.Empty<string>(), s =>
						{
							result.Add(s);
							resetEvent.Set();
						},
						It.IsAny<bool>());
					}
					// Assert
					while (!resetEvent.WaitOne(100))
					{
						Application.DoEvents();
					}
					AssertContains(exception.Message, result.Single());
				}
			}

			public void TestReturnsExpectedResult()
			{
				CombineAssertions(() =>
				{
					Test("hostName1", ("task", TaskActionResultDTO.EnqueuedAlready));
					Test("hostName2", ("secondInResult", TaskActionResultDTO.UnknownTask), ("firstInResult", TaskActionResultDTO.TaskDisabledOrInactive));
				});

				void Test(string hostName, params (string taskCode, TaskActionResultDTO outcome)[] requests)
				{
					// Arrange
					var resetEvent = new ManualResetEvent(false);
					serviceHostClientMock.Reset();

					var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
					serviceHostsCacheMock
						.SetupGet(cache => cache.ConfiguredServiceHosts)
						.Returns(new[] { serviceHostClientMock.Object });

					var schedulingOutcome = new TasksActionResultDTO(requests
						.Select(tuple => (dto: new TaskCodeDTO(tuple.taskCode), tuple.outcome))
						.ToDictionary(tuple => tuple.dto, tuple => tuple.outcome));

					serviceHostsCacheMock
						.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TasksActionResultDTO>>()))
						.Returns(new AggregateResult<TasksActionResultDTO>(new[] { new AggregateResultEntry<TasksActionResultDTO>(hostName, schedulingOutcome) }));

					var result = new List<string>();
					var expectedResult = $"Host: {hostName}{System.Environment.NewLine}" +
										string.Join(
											System.Environment.NewLine,
											requests
												.OrderBy(tuple => tuple.taskCode)
												.Select(tuple => $"{tuple.taskCode}: {tuple.outcome}"));

					using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
					{
						// Act
						scheduleNowController.ScheduleNow(Enumerable.Empty<string>(), s =>
						{
							result.Add(s);
							resetEvent.Set();
						},
						It.IsAny<bool>());
					}

					// Assert
					while (!resetEvent.WaitOne(100))
					{
						Application.DoEvents();
					}
					AssertEquals(expectedResult, result.Single());
				}
			}

			[ExpectNoExceptions]
			public void TestInvokeActionOnTasksIsCalledWithUserCode()
			{
				// Arrange
				var taskCodes = new[] { "task" };
				var forceRestart = true;
				using var resetEvent = new ManualResetEvent(false);
				serviceHostClientMock.Reset();

				var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock
					.SetupGet(cache => cache.ConfiguredServiceHosts)
					.Returns(new[] { serviceHostClientMock.Object });

				var schedulingOutcome = new TasksActionResultDTO(taskCodes
					.Select(taskCode => (dto: new TaskCodeDTO(taskCode), outcome: TaskActionResultDTO.Succeeded))
					.ToDictionary(tuple => tuple.dto, tuple => tuple.outcome));

				serviceHostsCacheMock
					.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TasksActionResultDTO>>()))
					.Returns(() => new AggregateResult<TasksActionResultDTO>(new[]
					{
						new AggregateResultEntry<TasksActionResultDTO>("hostName", schedulingOutcome)
					}))
					.Callback((Func<IServiceHostClient, TasksActionResultDTO>  invocationFunc) =>
					{
						invocationFunc(serviceHostClientMock.Object);
						resetEvent.Set();
					});

				using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
				{
					// Act
					scheduleNowController.ScheduleNow(taskCodes, s => { }, forceRestart: forceRestart);
				}

				// Assert
				resetEvent.WaitOne();
				serviceHostClientMock.Verify(client => client.InvokeActionOnTasks("schedule", taskCodes, "echoes=0", $"force={forceRestart}", $"user={GlbStaff.CurrentUser.GS_Code}"), Times.Once);
			}

			ScheduleNowController scheduleNowController;
			Mock<IServiceHostClient> serviceHostClientMock;
		}
	}
}
