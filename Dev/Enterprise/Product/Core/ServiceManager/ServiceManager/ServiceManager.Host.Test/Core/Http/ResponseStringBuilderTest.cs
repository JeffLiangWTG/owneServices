using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Host.Testing;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class ResponseStringBuilderTest : TestCaseWithFactory
	{
		const string server = "SERVER";
		const string dbname = "DBNAME";
		Mock<IServiceHostRequestProvider> serviceHostRequestProviderMock;

		protected override void SetUp()
		{
			base.SetUp();
			serviceHostRequestProviderMock = new Mock<IServiceHostRequestProvider>();
		}

		public void TestResponseStringBuilderWhenArgumentsAreNull()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var scheduler = new Mock<ITaskScheduler>();
				var statusProvider = new Mock<ITaskStatusProvider>();
				var queueStatusProviderFactory = new Mock<IQueueStatusProviderFactory>();
				var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
				var jsonConverter = Mock.Of<IJsonConverter>();

				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(null, dbname, scheduler.Object, statusProvider.Object, queueStatusProviderFactory.Object, hostStatusProvider.Object, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("dbServer"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, null, scheduler.Object, statusProvider.Object, queueStatusProviderFactory.Object, hostStatusProvider.Object, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("dbName"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, null, statusProvider.Object, queueStatusProviderFactory.Object, hostStatusProvider.Object, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskScheduler"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, scheduler.Object, null, queueStatusProviderFactory.Object, hostStatusProvider.Object, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskStatusProvider"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object, null, hostStatusProvider.Object, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("statusProviderFactory"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object, queueStatusProviderFactory.Object, null, jsonConverter, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostServiceStatusProvider"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object, queueStatusProviderFactory.Object, hostStatusProvider.Object, null, serviceHostRequestProviderMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("jsonConverter"));
				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object, queueStatusProviderFactory.Object, hostStatusProvider.Object, jsonConverter, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceHostRequestProvider"));
			});
		}

		[ExpectNoExceptions]
		public void TestLogFilesRequestHandler()
		{
			var request = new WebRequestInfo
			{
				Uri = new Uri(ServiceManagerHelper.GetLogFilesUri("localhost") + "/LWK"),
			};

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, Mock.Of<IJsonConverter>(), serviceHostRequestProviderMock.Object);

			NUnit.Framework.Assert.That(rsb.TryGetRequestHandler(request, out _), Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetUnknownResponse()
		{
			// Arrange
			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/UnknownRequestURL", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
			};

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, Mock.Of<IJsonConverter>(), serviceHostRequestProviderMock.Object);

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo("The page cannot be found."));
		}

		[TestDate(2015, 2, 23)]
		[ExpectNoExceptions]
		public void TestGetTasksStatusResponseWithEmptyNextRunTime()
		{
			AssertGetTaskStatusResponse(ZDateTime.Empty);
		}

		[TestDate(2015, 2, 23)]
		[ExpectNoExceptions]
		public void TestGetTasksStatusResponse()
		{
			AssertGetTaskStatusResponse(ZDateTime.UtcNow);
		}

		[ExpectNoExceptions]
		void AssertGetTaskStatusResponse(ZDateTime nextRunTime)
		{
			// Arrange
			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/status", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
			};

			var schedule = Factory.New<NullBranchServiceTaskSchedule>();
			schedule.S5_ScheduleType = "C01";
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = nextRunTime;
			schedule.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute() { DefaultScheduleRunEvery = "0seconds" });
			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
			var serviceTask = new RunnableServiceTask(RunnableServiceTaskTest.DummyTaskInfo, new SchedulerServiceTask(schedule), new Mock<IBackgroundThreadActionQueue>().Object, Mock.Of<ITaskQueue>(), Mock.Of<IHostLogger>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), Mock.Of<ITransactionAdapter>(), Mock.Of<ILoggerFactory>(), Mock.Of<IServiceTaskScheduleStatusProvider>());

			var sw = new Stopwatch();
			sw.Start();

			statusProvider.Setup(c => c.GetTasksStatus())
				.Returns(new List<ServiceTaskStatus>()
				{
					new ServiceTaskStatus(serviceTask, new List<IRunnableServiceTask>(), new List<ServiceTaskCodeWithRunnerProcessId>(), new List<HostedServiceBusinessObjectBindingAttribute>(), false)
				});

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			if (nextRunTime.IsEmpty)
			{
				NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""StatusList"":[{""BindingTypes"":[],""Branch"":"""",""Category"":"""",""Code"":"""",""Description"":""Desc 1"",""ErrorCountLast24Hours"":0,""IsActive"":true,""LastErrorTime"":null,""LastRunTime"":null,""NextRunTime"":null,""PlaceInQueue"":-1,""RunnerPids"":[],""RunningCount"":0,""SchedulePeriod"":""PT0S"",""Status"":3}]}"), "Response is valid");
			}
			else
			{
				NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""StatusList"":[{""BindingTypes"":[],""Branch"":"""",""Category"":"""",""Code"":"""",""Description"":""Desc 1"",""ErrorCountLast24Hours"":0,""IsActive"":true,""LastErrorTime"":null,""LastRunTime"":null,""NextRunTime"":""\/Date(1424649600000)\/"",""PlaceInQueue"":-1,""RunnerPids"":[],""RunningCount"":0,""SchedulePeriod"":""PT0S"",""Status"":3}]}"), "Response is valid");
			}
			statusProvider.VerifyAll();
		}

		[TestDate(2016, 11, 1)]
		[ExpectNoExceptions]
		public void TestGetTaskStatusResponse()
		{
			var qs = new NameValueCollection { ["task"] = "TST" };
			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/taskstatus?task=TST", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
				QueryString = qs,
			};

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var serviceTask = new Mock<IRunnableServiceTask>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
			serviceTask.Setup(st => st.Code).Returns("TST");
			serviceTask.Setup(st => st.NextScheduledRunTime).Returns(ZDateTime.UtcNow.ToNullableDateTimeOffset());

			statusProvider.Setup(c => c.GetTaskStatus(serviceTask.Object.Code))
				.Returns(new ServiceTaskStatus(serviceTask.Object, new List<IRunnableServiceTask>(), new List<ServiceTaskCodeWithRunnerProcessId>(), new List<HostedServiceBusinessObjectBindingAttribute>(), false));

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""BindingTypes"":[],""Branch"":null,""Category"":null,""Code"":""TST"",""Description"":null,""ErrorCountLast24Hours"":0,""IsActive"":false,""LastErrorTime"":null,""LastRunTime"":null,""NextRunTime"":""\/Date(1477958400000)\/"",""PlaceInQueue"":-1,""RunnerPids"":[],""RunningCount"":0,""SchedulePeriod"":""PT0S"",""Status"":1}"), "Response is valid");
			statusProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestGetTaskStatusResponseWhenTaskNotFound()
		{
			var qs = new NameValueCollection { ["task"] = "TST" };
			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/taskstatus?task=TST", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
				QueryString = qs,
			};

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
			statusProvider.Setup(c => c.GetTaskStatus("TST")).Returns((ServiceTaskStatus)null);

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(@"null"));
			statusProvider.VerifyAll();
		}

		[UseSnapshotProtection]
		[TestDate(2012, 4, 11, 9, 0, 0)]
		[ExpectNoExceptions]
		public void TestScheduleCommandResponseNoEchoes()
		{
			AssertScheduleCommandResponse(0, Array.Empty<int>());
		}

		[UseSnapshotProtection]
		[TestDate(2012, 4, 11, 9, 0, 0)]
		[ExpectNoExceptions]
		public void TestScheduleCommandResponseWithEchoes()
		{
			AssertScheduleCommandResponse(1, new int[] { 30 });
		}

		void AssertScheduleCommandResponse(uint echoes, IEnumerable<int> expectedAdditionalNudges)
		{
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var taskSelector = new TaskSelector(runnerPool.Object, logger.Object);
			var taskRunProcessor = new Mock<ITaskRunRequestProcessor>();
			var allTasks = new Mock<IAllTasksConsumer>();
			var taskSchedulerMock = new Mock<ITaskScheduler>();
			var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var schedulerDispatcher = new Mock<SchedulerDispatcher>(allTasks.Object, taskSelector, taskRunProcessor.Object, logger.Object, serviceHostsCache.Object, runnerPool.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>());
			var delayProvider = new Mock<IAsyncDelayProvider>();
			var taskQueue = new Mock<TaskQueue>(runnerPool.Object, logger.Object) { CallBase = true };
			using var controller = new ControllerForTest(null, null, delayProvider.Object, null, null, null, schedulerDispatcher.Object)
			{
				TaskQueue = taskQueue.As<ITaskQueue>()
			};
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var task1 = TaskSchedulerTest.CreateTask("ABC", Factory, allowsMultiple: false, actionQueue: controller.ActionQueue, taskQueue: controller.TaskQueue.Object, transactionAdapter: transactionAdapterMock.Object);
			task1.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddDays(2), SetNextRuntimeReason.ScheduledToRun); //Ensure the task isn't scheduled to run, just let it be nudged.
			var task2 = TaskSchedulerTest.CreateTask("BCD", Factory, allowsMultiple: false, actionQueue: controller.ActionQueue, taskQueue: controller.TaskQueue.Object, transactionAdapter: transactionAdapterMock.Object);
			task2.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddDays(2), SetNextRuntimeReason.ScheduledToRun); //Ensure the task isn't scheduled to run, just let it be nudged.
			allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[] { task1, task2 });
			allTasks.Setup(a => a.TryGetByCode("ABC", out task1)).Returns(true);
			allTasks.Setup(a => a.TryGetByCode("BCD", out task2)).Returns(true);

			using (var scheduler = new TaskScheduler(transactionAdapterMock.Object, allTasks.Object, logger.Object, controller.RegChecker.Object, ControllerTest.CreateMockDataSaverFactory() , new Mock<IErrorReporterProxy>().Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var statusProvider = new Mock<ITaskStatusProvider>();
				var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
				var rsb = new ResponseStringBuilder(server, dbname, scheduler, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

				var result = rsb.GetResponseString(CreateScheduleWebRequest("ABC,BCD", echoes));
				NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":1},""BCD"":{""Description"":null,""Outcome"":1}}}"));
			}
		}

		[ExpectNoExceptions]
		public void TestScheduleCommandResponse_Delayed()
		{
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var taskSelector = new TaskSelector(runnerPool.Object, logger.Object);
			var taskRunProcessor = new Mock<ITaskRunRequestProcessor>();
			var allTasks = new Mock<IAllTasksConsumer>();
			var taskSchedulerMock = new Mock<ITaskScheduler>();
			var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			var serviceHostsCache = new Mock<IServiceHostsCache>();
			var schedulerDispatcher = new Mock<SchedulerDispatcher>(allTasks.Object, taskSelector, taskRunProcessor.Object, logger.Object, serviceHostsCache.Object, runnerPool.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>());
			var delayProvider = new Mock<IAsyncDelayProvider>();
			delayProvider.Setup(dp => dp.DelayAsync(TimeSpan.FromSeconds(5), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
			using var controller = new ControllerForTest(null, null, delayProvider.Object, null, null, null, schedulerDispatcher.Object)
			{
				TaskQueue = new Mock<ITaskQueue>()
			};
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var task1 = TaskSchedulerTest.CreateTask("ABC", Factory, allowsMultiple: false, actionQueue: controller.ActionQueue, taskQueue: controller.TaskQueue.Object, transactionAdapter: transactionAdapterMock.Object);
			task1.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddDays(2), SetNextRuntimeReason.ScheduledToRun); //Ensure the task isn't scheduled to run, just let it be nudged.
			allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[] { task1 });
			allTasks.Setup(a => a.TryGetByCode(It.IsAny<string>(), out task1)).Returns(true);

			using (var scheduler = new TaskScheduler(transactionAdapterMock.Object, allTasks.Object, logger.Object, controller.RegChecker.Object, ControllerTest.CreateMockDataSaverFactory(), new Mock<IErrorReporterProxy>().Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
			{
				var statusProvider = new Mock<ITaskStatusProvider>();
				var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
				var rsb = new ResponseStringBuilder(server, dbname, scheduler, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

				var result = rsb.GetResponseString(CreateScheduleWebRequest("ABC", delay: TimeSpan.FromSeconds(5)));
				NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":2}}}"));
				delayProvider.Verify(dp => dp.DelayAsync(TimeSpan.FromSeconds(5), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
			}
		}

		WebRequestInfo CreateScheduleWebRequest(string tasks, uint? echoes = null, TimeSpan? delay = null)
		{
			var qs = new NameValueCollection
			{
				["action"] = "schedule",
				["tasks"] = tasks
			};
			AddToOptionsList(qs, "echoes", echoes);
			AddToOptionsList(qs, "delayinseconds", TimeSpanToSeconds(delay));

			return new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/command?action=schedule&tasks={2}", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID, tasks)),
				QueryString = qs,
			};
		}

		static uint? TimeSpanToSeconds(TimeSpan? span) => span != null ? (uint?)span.Value.TotalSeconds : null;

		static void AddToOptionsList<T>(NameValueCollection qs, string name, T? value) where T : struct
		{
			if (value.HasValue)
			{
				qs.Add(name, value.Value.ToString().ToLowerInvariant());
			}
		}

		[UseSnapshotProtection]
		[TestDate(2012, 4, 11, 9, 0, 0)]
		[ExpectNoExceptions]
		public void TestScheduleCommandResponseLastCommandIsEchoed()
		{
			using (var runRequestProcessor = new RunRequestProcessorThatCanPause())
			{
				var logger = new Mock<IHostLogger>();
				var runnerPool = new Mock<IProcessRunnerPool>();
				var taskSelector = new TaskSelector(runnerPool.Object, logger.Object);
				var allTasks = new Mock<IAllTasksConsumer>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var actionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var schedulerDispatcher = new Mock<SchedulerDispatcher>(allTasks.Object, taskSelector, runRequestProcessor, logger.Object, serviceHostsCache.Object, runnerPool.Object, taskSchedulerMock.Object, actionQueueMock.Object, CancellationToken.None, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), Mock.Of<IResourceThrottler>());
				var taskQueue = new Mock<TaskQueue>(runnerPool.Object, logger.Object) { CallBase = true };
				using var controller = new ControllerForTest(null, null, new Mock<IAsyncDelayProvider>().Object, null, null, null, schedulerDispatcher.Object)
				{
					TaskQueue = taskQueue.As<ITaskQueue>()
				};
				controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

				var transactionAdapterMock = new Mock<ITransactionAdapter>();
				var task1 = TaskSchedulerTest.CreateTask("ABC", Factory, allowsMultiple: false, actionQueue: controller.ActionQueue, taskQueue: controller.TaskQueue.Object, transactionAdapter: transactionAdapterMock.Object);
				task1.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddDays(2), SetNextRuntimeReason.ScheduledToRun); //Ensure the task isn't scheduled to run, just let it be nudged.
				var task2 = TaskSchedulerTest.CreateTask("BCD", Factory, allowsMultiple: false, actionQueue: controller.ActionQueue, taskQueue: controller.TaskQueue.Object, transactionAdapter: transactionAdapterMock.Object);
				task2.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddDays(2), SetNextRuntimeReason.ScheduledToRun); //Ensure the task isn't scheduled to run, just let it be nudged.
				allTasks.Setup(a => a.GetAll()).Returns(new IRunnableServiceTask[] { task1, task2 });
				allTasks.Setup(a => a.TryGetByCode("ABC", out task1)).Returns(true);
				allTasks.Setup(a => a.TryGetByCode("BCD", out task2)).Returns(true);

				using (var scheduler = new TaskScheduler(transactionAdapterMock.Object, allTasks.Object, logger.Object, controller.RegChecker.Object, ControllerTest.CreateMockDataSaverFactory(), new Mock<IErrorReporterProxy>().Object, Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>()))
				{
					var statusProvider = new Mock<ITaskStatusProvider>();
					var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
					var rsb = new ResponseStringBuilder(server, dbname, scheduler, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

					var requestABC = CreateScheduleWebRequest("ABC", 1);
					var result = rsb.GetResponseString(requestABC);
					NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":1}}}"));

					result = rsb.GetResponseString(CreateScheduleWebRequest("BCD", 0));
					NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""BCD"":{""Description"":null,""Outcome"":1}}}"));

					result = rsb.GetResponseString(requestABC);
					NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":3}}}"));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestRequestReloadOfTaskConfigurationCommandResponse()
		{
			// Arrange

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);

			scheduler.Setup(c => c.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()
			{
				{ new TaskCodeDTO("ABC"), TaskActionResultDTO.Succeeded },
				{ new TaskCodeDTO("BCD"), TaskActionResultDTO.Succeeded }
			}));

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object, CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			var qs = new NameValueCollection
			{
				["action"] = "requestconfigreload",
				["tasks"] = "ABC,BCD"
			};

			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/command?action=requestconfigreload&tasks=ABC,BCD", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
				QueryString = qs,
			};

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			scheduler.Verify(c => c.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()), Times.Once);
			NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":6},""BCD"":{""Description"":null,""Outcome"":6}}}"));
		}

		[TestDate(2016, 05, 26, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestSetNextRunTimeCommandResponse()
		{
			// Arrange
			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
			var currentTime = ZDateTime.UtcNow.ToNullableDateTimeOffset();
			var reverting = true;
			scheduler.Setup(c => c.SetNextRuntime(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<DateTimeOffset?>(), It.IsAny<bool?>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()
				{
					{ new TaskCodeDTO("ABC"), TaskActionResultDTO.Succeeded },
					{ new TaskCodeDTO("BCD"), TaskActionResultDTO.Succeeded }
				}));

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			var qs = new NameValueCollection
			{
				["action"] = "setnextruntime",
				["tasks"] = "ABC,BCD",
				["nextruntime"] = currentTime?.ToString(ServiceManagerConstants.JsonDateTimeFormat, CultureInfo.InvariantCulture),
				["reverting"] = reverting.ToString()
			};

			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/command?action=setisactive&tasks=ABC,BCD&active=true", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
				QueryString = qs,
			};

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			scheduler.Verify(c => c.SetNextRuntime(It.Is<IEnumerable<TaskCodeDTO>>(cl => cl.Contains(new TaskCodeDTO("ABC")) && cl.Contains(new TaskCodeDTO("BCD"))), currentTime, reverting), Times.Once);
			NUnit.Framework.Assert.That(result, Is.EqualTo(@"{""Results"":{""ABC"":{""Description"":null,""Outcome"":6},""BCD"":{""Description"":null,""Outcome"":6}}}"));
		}

		[ExpectNoExceptions]
		public void TestUnknownGetCommandResponse()
		{
			// Arrange

			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);

			scheduler.Setup(c => c.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()
				{
					{ new TaskCodeDTO("ABC"), TaskActionResultDTO.EnqueuedNow },
					{ new TaskCodeDTO("BCD"), TaskActionResultDTO.EnqueuedNow }
				}));

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object);

			var qs = new NameValueCollection
			{
				["action"] = "somethingElse",
				["tasks"] = "ABC,BCD"
			};

			var request = new WebRequestInfo
			{
				Uri = new Uri(string.Format("http://localhost:7070/cargowise/processcontroller/{0}{1}/command?action=somethingElse&tasks=ABC,BCD", GlbCompany.CurrentCompany.LicenceEnterpriseCode, GlbCompany.CurrentCompany.LicenceServerID)),
				QueryString = qs,
			};

			// Act
			var result = rsb.GetResponseString(request);

			// Assert
			scheduler.Verify(c => c.ScheduleTasks(It.IsAny<IEnumerable<TaskCodeDTO>>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<string>()), Times.Never);
			NUnit.Framework.Assert.That(result, Is.EqualTo(@"Error: Unknown command: ?action=somethingElse&tasks=ABC,BCD"));
		}

		[ExpectNoExceptions]
		public void TestAllServiceHostRequestProvidersCanBeAccessed()
		{
			AssertDependencyInjectedProvidersAreAccessible();
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestAssertDummyDependencyInejectedProviders()
		{
			AssertDependencyInjectedProvidersAreAccessible(new DummyServiceHostRequestProvider(new DummyServiceHostGetRequest(), new DummyServiceHostPostRequest()));
		}

		void AssertDependencyInjectedProvidersAreAccessible(IServiceHostRequestProvider serviceHostRequestProvider = null)
		{
			var scheduler = new Mock<ITaskScheduler>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			var hostStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);

			var rsb = new ResponseStringBuilder(server, dbname, scheduler.Object, statusProvider.Object,  CreateMockQueueStatusProviderFactory(), hostStatusProvider.Object, Mock.Of<IJsonConverter>(), serviceHostRequestProvider ?? serviceHostRequestProviderMock.Object);

			foreach (var requestProvider in ObjectFactory.Get<IServiceHostRequestProvider>()?.ServiceHostRequests ?? Enumerable.Empty<IServiceHostRequest>())
			{
				var request = new WebRequestInfo { Uri = requestProvider.GetUri("localhost", server, dbname) };
				NUnit.Framework.Assert.That(rsb.TryGetRequestHandler(request, out _), Is.True);
			}
		}

		static IQueueStatusProviderFactory CreateMockQueueStatusProviderFactory()
		{
			var queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());

			return queueStatusProviderFactoryMock.Object;
		}

		class DummyServiceHostRequestProvider : IServiceHostRequestProvider
		{
			public DummyServiceHostRequestProvider(params IServiceHostRequest[] requests)
			{
				ServiceHostRequests = requests;
			}
			public IEnumerable<IServiceHostRequest> ServiceHostRequests { get; }
		}

		class DummyServiceHostPostRequest : IServiceHostPostRequest
		{
			public Uri GetUri(string hostName, string server, string dbName)
			{
				return new Uri("herpderpderp:7070/derp/derp/derp");
			}

			public Stream RunRequest(IHttpRequestInfo info, Stream stream)
			{
				return stream;
			}
		}

		class DummyServiceHostGetRequest : IServiceHostGetRequest
		{
			public Uri GetUri(string hostName, string server, string dbName)
			{
				return new Uri("herpderpderp:7070/nang/nang/nang/nang");
			}

			public Stream RunRequest(IHttpRequestInfo info)
			{
				return new MemoryStream();
			}
		}

		class RunRequestProcessorThatCanPause : ITaskRunRequestProcessor, IDisposable
		{
			readonly ManualResetEvent enterEvent = new ManualResetEvent(false);
			readonly ManualResetEvent exitEvent = new ManualResetEvent(false);

			public bool PauseEnabled { get; set; }
			public int RunRequests { get; private set; }

			public TaskRunRequestResult ProcessRunRequest(ITaskRunRequest request, IServiceRunner serviceRunner)
			{
				if (PauseEnabled)
				{
					enterEvent.Set();
					NUnit.Framework.Assert.That(exitEvent.WaitOne(TimeSpan.FromSeconds(30)), Is.True);
				}
				++RunRequests;
				return TaskRunRequestResult.Success;
			}

			public bool WaitForRunRequest() => enterEvent.WaitOne(TimeSpan.FromSeconds(30));
			public void ExitRunRequest() => exitEvent.Set();

			#region IDisposable

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					enterEvent.Dispose();
					exitEvent.Dispose();
				}
			}

			#endregion
		}
	}
}
