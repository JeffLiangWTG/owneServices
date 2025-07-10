using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(CommandRequestHandler))]
	class CommandRequestHandlerTest : RequestHandlerBaseTest
	{
		[ExpectNoExceptions]
		public void TestCorrectActionIsCalledForSchedule()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test("schedule", "a,b");
				Test("schedule", "a,,b");
				Test("schedule", "a,b,");
				Test("schedule", ",a,b");
				Test("Schedule", "a,b");
				Test("Schedule", "a,,b");
				Test("Schedule", "a,b,");
				Test("Schedule", ",a,b");
			});

			void Test(string action, string tasks)
			{
				taskSchedulerMock.Invocations.Clear();

				var queryStringCollection = new NameValueCollection
				{
					{ "action", action },
					{ "tasks", tasks },
				};

				TestAssertResults(action, queryStringCollection);
				taskSchedulerMock.Verify(x => x
					.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestCorrectActionIsCalledForRequestConfigReload()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test("requestconfigreload", "a,b");
				Test("requestconfigreload", "a,,b");
				Test("requestconfigreload", "a,b,");
				Test("requestconfigreload", ",a,b");
				Test("RequestConfigReload", "a,b");
				Test("RequestConfigReload", "a,,b");
				Test("RequestConfigReload", "a,b,");
				Test("RequestConfigReload", ",a,b");
			});

			void Test(string requestConfigReload, string tasks)
			{
				taskSchedulerMock.Invocations.Clear();

				var queryStringCollection = new NameValueCollection
				{
					{ "action", requestConfigReload },
					{ "tasks", tasks },
				};

				TestAssertResults(requestConfigReload, queryStringCollection);
				taskSchedulerMock.Verify(x => x
					.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestCorrectActionIsCalledForSetNextRuntime()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test("setnextruntime", "a,b");
				Test("setnextruntime", "a,,b");
				Test("setnextruntime", "a,b,");
				Test("setnextruntime", ",a,b");
				Test("setNextRuntime", "a,b");
				Test("setNextRuntime", "a,,b");
				Test("setNextRuntime", "a,b,");
				Test("setNextRuntime", ",a,b");
			});

			void Test(string setNextRuntime, string tasks)
			{
				taskSchedulerMock.Invocations.Clear();

				var queryStringCollection = new NameValueCollection
				{
					{ "action", setNextRuntime },
					{ "tasks", tasks },
					{ "nextruntime", DateTime.UtcNow.AddDays(1).ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture) },
					{ "reverting", "true" },
				};

				TestAssertResults(setNextRuntime, queryStringCollection);
				taskSchedulerMock.Verify(x => x
					.SetNextRuntime(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.Is<DateTimeOffset?>(dt => dt.Value.Offset == TimeSpan.Zero), It.IsAny<bool?>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestGetCommandResult_ScheduleActionDetails()
		{
			foreach (var tuple in GetCommandResult_ScheduleActionDetailsSource())
			{
				Test(tuple.queryStringCollection, tuple.expected);
			}

			void Test(NameValueCollection queryStringCollection, (TaskCodeDTO[] tasks, bool echoes, TimeSpan? delay) expected)
			{
				// Arrange
				IEnumerable<TaskCodeDTO> taskCodeDTOs = null;
				var scheduleDetails = new List<(IEnumerable<TaskCodeDTO> code, bool echoes, TimeSpan? delay)>();
				taskSchedulerMock.Setup(x => x
						.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
					.Callback<IEnumerable<TaskCodeDTO>, bool, TimeSpan?, string>((taskCodes, echoes, delay, userCode) =>
					{
						taskCodeDTOs = taskCodes;
						scheduleDetails.Add((taskCodes, echoes, delay));
					})
					.Returns(() => GetActionResultDTO(taskCodeDTOs));

				httpRequestInfoMock.Setup(x => x.QueryString).Returns(queryStringCollection);

				// Act
				_ = handler.Handle(httpRequestInfoMock.Object);

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(scheduleDetails.Count, Is.EqualTo(1));
					var result = scheduleDetails.Single();
					NUnit.Framework.Assert.That(result.code, Is.EquivalentTo(expected.tasks));
					NUnit.Framework.Assert.That(result.echoes, Is.EqualTo(expected.echoes));
					NUnit.Framework.Assert.That(result.delay, Is.EqualTo(expected.delay));
				});
			}
		}

		static IEnumerable<(NameValueCollection queryStringCollection, (TaskCodeDTO[] tasks, bool echoes, TimeSpan? delay) expected)> GetCommandResult_ScheduleActionDetailsSource()
		{
			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "b" },
				},
				(
					new[] { new TaskCodeDTO("b") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
					{ "echoes", "0" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					false,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
					{ "echoes", "1" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
					{ "echoes", "5" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					null
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
					{ "delayinseconds", "10" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					TimeSpan.FromSeconds(10)
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a" },
					{ "delayinseconds", "100" },
				},
				(
					new[] { new TaskCodeDTO("a") },
					true,
					TimeSpan.FromSeconds(100)
				));

			yield return (
				new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "a,b," },
					{ "echoes", "1" },
					{ "delayinseconds", "10" },
				},
				(
					new[] { new TaskCodeDTO("a"), new TaskCodeDTO("b") },
					true,
					TimeSpan.FromSeconds(10)
				));
		}

		[ExpectNoExceptions]
		public void TestGetResultError_EmptyTasks()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test("schedule", "a,b,");
				Test("requestconfigreload", "a,b,");
				Test("setnextruntime", "a,b,");
				Test("trackservicetaskerror", "a,b,");
			});

			void Test(string action, string tasks)
			{
				IEnumerable<TaskCodeDTO> taskCodeDTOs = null;
				var scheduleDetails = new List<(IEnumerable<TaskCodeDTO>, bool, TimeSpan?)>();
				taskSchedulerMock.Setup(x => x
						.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
					.Callback<IEnumerable<TaskCodeDTO>, bool, TimeSpan?, string>((taskCodes, echoes, delay, userCode) =>
					{
						taskCodeDTOs = taskCodes;
						scheduleDetails.Add((taskCodes, echoes, delay));
					})
					.Returns(() => GetActionResultDTO(taskCodeDTOs));

				var queryStringCollection = new NameValueCollection
				{
					{ "action", action },
					{ "tasks", "," },
				};

				httpRequestInfoMock.Setup(x => x.QueryString).Returns(queryStringCollection);

				var result = handler.Handle(httpRequestInfoMock.Object);

				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(string)));
				NUnit.Framework.Assert.That(result, Is.EqualTo($"Error: You need to provide at least one task for command: {action}"));
			}
		}

		[ExpectNoExceptions]
		public void TestGetResultError_UnknownCommand()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(Guid.NewGuid().ToString("N"), "a,b,");
				Test(null, "a,b,");
				Test(string.Empty, "a,b,");
				Test("blahblah", "a,b,");
				Test("foo", "a,b,");
				Test("bar", "a,b,");
			});

			void Test(string action, string tasks)
			{
				IEnumerable<TaskCodeDTO> taskCodeDTOs = null;
				var scheduleDetails = new List<(IEnumerable<TaskCodeDTO>, bool, TimeSpan?)>();
				taskSchedulerMock.Setup(x => x
						.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
					.Callback<IEnumerable<TaskCodeDTO>, bool, TimeSpan?, string>((taskCodes, echoes, delay, userCode) =>
					{
						taskCodeDTOs = taskCodes;
						scheduleDetails.Add((taskCodes, echoes, delay));
					})
					.Returns(() => GetActionResultDTO(taskCodeDTOs));

				var queryStringCollection = new NameValueCollection
				{
					{ "action", action },
					{ "tasks", tasks },
				};

				httpRequestInfoMock.Setup(x => x.QueryString).Returns(queryStringCollection);
				httpRequestInfoMock.Setup(x => x.Uri).Returns(new Uri($"http://localhost:7070/query?{action}"));

				var result = handler.Handle(httpRequestInfoMock.Object);

				NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(string)));
				NUnit.Framework.Assert.That(result, Is.EqualTo($"Error: Unknown command: ?{action}"));
			}
		}

		void TestAssertResults(string action, NameValueCollection queryStringCollection)
		{
			httpRequestInfoMock.Setup(x => x.QueryString).Returns(queryStringCollection);

			var result = handler.Handle(httpRequestInfoMock.Object);

			NUnit.Framework.Assert.That(!string.IsNullOrEmpty(result), Is.True, $"{action} was handled with no exceptions");

			var actionResults = result.FromJSON<TasksActionResultDTO>();

			NUnit.Framework.Assert.That(actionResults, Is.Not.EqualTo(default(TasksActionResultDTO)));
			NUnit.Framework.Assert.That(actionResults.Results, Is.Not.EqualTo(default(Dictionary<TaskCodeDTO, TaskActionResultDTO>)));

			var rasks = queryStringCollection["tasks"]?
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(x => !string.IsNullOrEmpty(x)).ToArray() ?? Array.Empty<string>();

			foreach (var taskCode in rasks)
			{
				NUnit.Framework.Assert.That(actionResults.Results.ContainsKey(taskCode), Is.True);
				NUnit.Framework.Assert.That(actionResults.Results.ContainsKey(taskCode), Is.True);
			}
		}

		static TasksActionResultDTO GetActionResultDTO(IEnumerable<TaskCodeDTO> dtos)
		{
			var results = new Dictionary<TaskCodeDTO, TaskActionResultDTO>();
			foreach (var dto in dtos)
			{
				results.Add(dto, new TaskActionResultDTO(TaskActionOutcomeDTO.Succeeded));
			}

			return new TasksActionResultDTO(results);
		}

		protected override void SetUp()
		{
			base.SetUp();

			taskSchedulerMock = new Mock<ITaskScheduler>();
			httpRequestInfoMock = new Mock<IHttpRequestInfo>();

			taskSchedulerMock.Setup(x => x
					.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
				.Callback<IEnumerable<TaskCodeDTO>, bool, TimeSpan?, string>((dtos, a, b, userCode) => taskCodeDTOs = dtos)
				.Returns(() => GetActionResultDTO(taskCodeDTOs))
				.Verifiable();

			taskSchedulerMock.Setup(x => x
					.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Callback<IEnumerable<TaskCodeDTO>>(dtos => taskCodeDTOs = dtos)
				.Returns(() => GetActionResultDTO(taskCodeDTOs))
				.Verifiable();

			taskSchedulerMock.Setup(x => x
					.SetNextRuntime(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<DateTimeOffset?>(), It.IsAny<bool?>()))
				.Callback<IEnumerable<TaskCodeDTO>, DateTimeOffset?, bool?>((dtos, a, b) => taskCodeDTOs = dtos)
				.Returns(() => GetActionResultDTO(taskCodeDTOs))
				.Verifiable();

			handler = new CommandRequestHandler(taskSchedulerMock.Object, new JsonNetConverter(), string.Empty);
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/command");
		}

		Mock<IHttpRequestInfo> httpRequestInfoMock;
		IEnumerable<TaskCodeDTO> taskCodeDTOs;
		Mock<ITaskScheduler> taskSchedulerMock;

		public class IntegrationTest : TestCaseWithFactory
		{
			[TestDate(2006, 12, 26)]
			[ExpectNoExceptions]
			public void TestRequestConfigReloadDoesNotSaveSchedule()
			{
				// Arrange
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				backgroundThreadActionQueueMock
					.Setup(queue => queue.Enqueue(It.IsAny<Action>()))
					.Callback<Action>(action => action.Invoke());

				var culpritTask = CreateTaskWithSchedule("CUL");
				var innocentTask = CreateTaskWithSchedule("INN");
				Factory.Save();

				UpdateLocalScheduleForInnocentTask();

				var backgroundDataSaverFactory = new Mock<IBackgroundDataSaverFactory>();
				backgroundDataSaverFactory.Setup(saver => saver.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>()))
					.Returns(Mock.Of<IBackgroundDataSaver>());

				allTasksConsumerMock = new Mock<IAllTasksConsumer>();
				allTasksConsumerMock.Setup(a => a.GetAll())
					.Returns(new[] { culpritTask.task, innocentTask.task });

				using var taskScheduler = new TaskScheduler(transactionAdapter, allTasksConsumerMock.Object, new Mock<IHostLogger>().Object, new Mock<IProductRegistrationPeriodicChecker>().Object, backgroundDataSaverFactory.Object, new Mock<IErrorReporterProxy>().Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());
				var handler = new CommandRequestHandler(taskScheduler, Mock.Of<IJsonConverter>(), string.Empty);

				var queryStringCollection = new NameValueCollection
				{
					{ "action", "requestConfigReload" },
					{ "tasks", "cul" },
				};
				var httpRequestInfoMock = Mock.Of<IHttpRequestInfo>(info => info.QueryString == queryStringCollection);

				// Act
				_ = handler.Handle(httpRequestInfoMock);

				// Assert
				var result = Db.Connection.ExecuteScalar<DateTime>("SELECT S5_NextScheduledPrintRunTimeUtc FROM dbo.StmScheduleTask WHERE S5_PK=@pk",
					command => command.AddParameter("@pk", SqlDbType.UniqueIdentifier, innocentTask.schedule.PK.ToGuid()));
				NUnit.Framework.Assert.That(result, Is.EqualTo(ZDateTime.Now.ToDateTime()));

				(RunnableServiceTask task, ServiceTaskSchedule schedule) CreateTaskWithSchedule(string code)
				{
					var serviceTaskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(config => config.Code == code));
					var schedule = TaskSchedulerTest.CreateSchedule(code, Factory);
					var task = new RunnableServiceTask(serviceTaskInfo, new SchedulerServiceTask(schedule), backgroundThreadActionQueueMock.Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapter, new LoggerFactory(), Mock.Of<IServiceTaskScheduleStatusProvider>());
					return (task, schedule);
				}

				void UpdateLocalScheduleForInnocentTask()
				{
					var nextRuntime = new DateTime(2019, 12, 23);
					innocentTask.task.SetNextRunTime(nextRuntime, SetNextRuntimeReason.UpdateReceivedFromPeerController);
					innocentTask.task.UpdateUnderlyingScheduleNextRunTime();
				}
			}

			[TestDate(2019, 12, 23)]
			[ExpectNoExceptions]
			public void TestNudgeHasCorrectDefaultValue()
			{
				// Arrange
				var transactionAdapterMock = Mock.Of<ITransactionAdapter>();
				var backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				ITaskRunRequest actualTaskRunRequest = null;
				var taskQueueMock = new Mock<ITaskQueue>();
				taskQueueMock
					.Setup(queue => queue.EnqueueTask(It.IsAny<ITaskRunRequest>()))
					.Returns<ITaskRunRequest>(request =>
					{
						actualTaskRunRequest = request;
						return true;
					});

				IRunnableServiceTask runnableServiceTask = CreateTaskWithSchedule("TSK");
				Factory.Save();

				var expectedTaskRunRequest = new DirectTaskRunRequest(runnableServiceTask, true);

				var productRegistrationPeriodicCheckerMock = Mock.Of<IProductRegistrationPeriodicChecker>(
					checker => checker.IsProductRegisteredAsNonTrialSystemOrUnknown());
				var backgroundDataSaverFactory = new Mock<IBackgroundDataSaverFactory>();
				backgroundDataSaverFactory.Setup(saver => saver.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>()))
					.Returns(Mock.Of<IBackgroundDataSaver>());

				allTasksConsumerMock = new Mock<IAllTasksConsumer>();
				allTasksConsumerMock.Setup(a => a.GetAll())
					.Returns(new[] { runnableServiceTask });
				allTasksConsumerMock.Setup(a => a.TryGetByCode(It.IsAny<string>(), out runnableServiceTask)).Returns(true);

				using var taskScheduler = new TaskScheduler(transactionAdapterMock, allTasksConsumerMock.Object, new Mock<IHostLogger>().Object, productRegistrationPeriodicCheckerMock, backgroundDataSaverFactory.Object, new Mock<IErrorReporterProxy>().Object, Mock.Of<IDatabaseAspectVersions>(),
					Mock.Of<IClientHostedServiceAttributeProvider>());
				var handler = new CommandRequestHandler(taskScheduler, Mock.Of<IJsonConverter>(), string.Empty);

				var queryStringCollection = new NameValueCollection
				{
					{ "action", "schedule" },
					{ "tasks", "tsk" },
				};
				var httpRequestInfoMock = Mock.Of<IHttpRequestInfo>(info => info.QueryString == queryStringCollection);

				// Act
				_ = handler.Handle(httpRequestInfoMock);

				// Assert
				taskQueueMock
					.Verify(queue => queue.EnqueueTask(It.IsAny<IDirectTaskRunRequest>()), Times.Once);
				taskQueueMock
					.VerifyNoOtherCalls();
				NUnit.Framework.Assert.That(actualTaskRunRequest, Is.EqualTo(expectedTaskRunRequest).Using(CustomComparers.TypeComparison));

				RunnableServiceTask CreateTaskWithSchedule(string code)
				{
					var serviceTaskInfo = new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(config => config.Code == code));
					var schedule = TaskSchedulerTest.CreateSchedule(code, Factory);
					var task = new RunnableServiceTask(serviceTaskInfo, new SchedulerServiceTask(schedule), backgroundThreadActionQueueMock.Object, taskQueueMock.Object, Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, new LoggerFactory(), Mock.Of<IServiceTaskScheduleStatusProvider>());
					return task;
				}
			}

			Mock<IAllTasksConsumer> allTasksConsumerMock;
		}
	}
}
