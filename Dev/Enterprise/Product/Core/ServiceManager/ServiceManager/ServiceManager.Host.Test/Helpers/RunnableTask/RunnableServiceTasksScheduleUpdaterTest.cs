using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.RunnableTask
{
	class RunnableServiceTasksScheduleUpdaterTest : TestCaseWithFactory
	{
		public void TestServiceTaskConfigurationPollingIntervalZeroDisablesUpdater()
		{
			// Arrange
			var allTasks = Mock.Of<IAllTasksConsumer>(a => a.GetAll() == new List<IRunnableServiceTask>());
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderMock.Setup(x => x
				.Reload(It.IsAny<DateTimeOffset>()))
				.Verifiable();
			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));
			var backgroundDataSaverFactory = new Mock<IBackgroundDataSaverFactory>();
			backgroundDataSaverFactory.Setup(saver => saver.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>()))
				.Returns(Mock.Of<IBackgroundDataSaver>());
			using var taskScheduler = new TaskScheduler(Mock.Of<ITransactionAdapter>(), allTasks, Mock.Of<IHostLogger>(), Mock.Of<IProductRegistrationPeriodicChecker>(), backgroundDataSaverFactory.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			// Act
			new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.Zero))
				.ReloadUpdatedFromDatabase(
					allTasks.GetAll(),
					serviceTasksReloaderMock.Object,
					taskScheduler);

			// Assert
			AssertNoExceptionThrown(() =>
				serviceTasksReloaderMock.Verify(x => x.Reload(It.IsAny<DateTimeOffset>()), Times.Never()));
		}

		[ExpectNoExceptions]
		public void TestReloadUpdatedFromDatabaseAndRequestReloadHavingIsActiveChanged()
		{
			// Arrange
			var allTasks = new List<IRunnableServiceTask>
			{
				Mock.Of<IRunnableServiceTask>(task => task.Code == "111" && task.IsActive && task.HasSchedule),
				Mock.Of<IRunnableServiceTask>(task => task.Code == "222" && task.IsActive && task.HasSchedule && task.HasScheduleUpdates(It.IsAny<IServiceTask>())),
				Mock.Of<IRunnableServiceTask>(task => task.Code == "333" && !task.IsActive && task.HasSchedule && task.HasScheduleUpdates(It.IsAny<IServiceTask>())),
				Mock.Of<IRunnableServiceTask>(task => task.Code == "444" && !task.IsActive && task.HasSchedule),
			};

			var allTasksConsumerMock = new Mock<IAllTasksConsumer>();
			allTasksConsumerMock.Setup(a => a.GetAll()).Returns(allTasks);

			var configureUpdatedSchedules = ConfigureUpdatedSchedules();
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderMock
				.Setup(x => x.Reload(It.IsAny<DateTimeOffset>()))
				.Returns(Mock.Of<IServiceTaskCollectionGovernor>(collection => collection.GovernedTasks == configureUpdatedSchedules));
			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));
			var backgroundDataSaverFactory = new Mock<IBackgroundDataSaverFactory>();
			backgroundDataSaverFactory.Setup(saver => saver.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>()))
				.Returns(Mock.Of<IBackgroundDataSaver>());
			using var taskScheduler = new TaskScheduler(Mock.Of<ITransactionAdapter>(), allTasksConsumerMock.Object, Mock.Of<IHostLogger>(), Mock.Of<IProductRegistrationPeriodicChecker>(), backgroundDataSaverFactory.Object, Mock.Of<IErrorReporterProxy>(), Mock.Of<IDatabaseAspectVersions>(), Mock.Of<IClientHostedServiceAttributeProvider>());

			// Act
			new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromSeconds(1)))
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloaderMock.Object,
					taskScheduler);

			// Assert
			Mock.Get(allTasks[0]).Verify(x => x.ReloadConfigurationAsync(), Times.Never);
			Mock.Get(allTasks[1]).Verify(x => x.ReloadConfigurationAsync(), Times.Once);
			Mock.Get(allTasks[2]).Verify(x => x.ReloadConfigurationAsync(), Times.Once);
			Mock.Get(allTasks[3]).Verify(x => x.ReloadConfigurationAsync(), Times.Never);

			IEnumerable<IServiceTask> ConfigureUpdatedSchedules()
			{
				var taskSchedulesMock = new List<IServiceTask>
				{
					NewFunction(1),
					NewFunction(2),
					NewFunction(3)
				};
				return taskSchedulesMock;

				IServiceTask NewFunction(int index)
				{
					var schedule = Factory.New<ServiceTaskSchedule>();
					schedule.S5_ScheduleType = allTasks[index].Code;
					return new SchedulerServiceTask(schedule);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestReloadsUpdatedFromDatabase()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "TST";
			var allTasks = new List<IRunnableServiceTask>
			{
				Mock.Of<IRunnableServiceTask>(task => task.Code == schedule.S5_ScheduleType.ToString() && task.IsActive && task.HasSchedule && task.HasScheduleUpdates(It.IsAny<IServiceTask>())),
			};

			Factory.Save();

			var serviceTasksReloader = new SchedulerServiceTasksReloader();
			var taskSchedulerMock = new Mock<ITaskScheduler>();
			taskSchedulerMock
				.Setup(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()));

			var memoryCacheMock = new Mock<IMemoryCache>();
			memoryCacheMock
					.Setup(cache => cache.AddOrGetExisting(nameof(RunnableServiceTasksScheduleUpdater), It.IsAny<Func<object>>(), It.IsAny<TimeSpan>()))
					.Returns<string, Func<object>, TimeSpan>((key, func, expiration) => func.Invoke());

			var runnableServiceTasksScheduleUpdater = new RunnableServiceTasksScheduleUpdater(memoryCacheMock.Object, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromSeconds(1)));
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloader,
					taskSchedulerMock.Object);
			taskSchedulerMock.Invocations.Clear();

			RunnableServiceTaskTestHelper.DbUpdateServiceTaskSchedule(
				pk: schedule.PK.ToGuid(),
				isActive: false,
				lastEditTimeUtc: schedule.S5_SystemLastEditTimeUtc.AddHours(1)
			);

			// Act
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloader,
					taskSchedulerMock.Object);

			// Assert
			taskSchedulerMock.Verify(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.Is<IEnumerable<TaskCodeDTO>>(dtos => dtos.Single().Code == schedule.S5_ScheduleType)), Times.Once);
			taskSchedulerMock.VerifyNoOtherCalls();
		}

		[TestDate(2024, 1, 27, 5, 10, 45)] // 2024-01-27 05:10:45; Still in the first minute of the initialisation, next minute is 5:11:00
		[ExpectNoExceptions]
		public void TestReloadsFromDatabaseDuringTheFirstMinuteOfInitialisation()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "TST";
			schedule.S5_IsActive = true;
			Factory.Save();

			var allTasks = new List<IRunnableServiceTask>
			{
				Mock.Of<IRunnableServiceTask>(task => task.Code == schedule.S5_ScheduleType.ToString() && task.IsActive && task.HasSchedule && task.HasScheduleUpdates(It.IsAny<IServiceTask>())),
			};

			var serviceTasksReloader = new SchedulerServiceTasksReloader();

			var taskSchedulerMock = new Mock<ITaskScheduler>();
			taskSchedulerMock
				.Setup(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()));
			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

			// Act
			var runnableServiceTasksScheduleUpdater = new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromSeconds(1)));
			RunnableServiceTaskTestHelper.DbUpdateServiceTaskSchedule(
				pk: schedule.PK.ToGuid(),
				isActive: false,
				lastEditTimeUtc: ZDateTime.TruncateSeconds(new ZDateTime(2024, 1, 27, 5, 10, 45).AddSeconds(10))
			);
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloader,
					taskSchedulerMock.Object);

			// Assert
			schedule.Reload();
			NUnit.Framework.Assert.That(new ZDateTime(2024, 1, 27, 5, 10, 00), Is.EqualTo(schedule.S5_SystemLastEditTimeUtc)); // 2024-01-27 05:10:00, seconds are 00 due to SmallDateTime type
			taskSchedulerMock.Verify(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.Is<IEnumerable<TaskCodeDTO>>(dtos => dtos.Single().Code == schedule.S5_ScheduleType)), Times.Once);
		}

		public void TestArgumentNullExceptions()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(
				() => _ = new RunnableServiceTasksScheduleUpdater(null, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>()));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("memoryCache"));

			result = AssertExceptionThrown<ArgumentNullException>(
				() => _ = new RunnableServiceTasksScheduleUpdater(Mock.Of<IMemoryCache>(), null, Mock.Of<IHostRegistrySettings>()));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

			var taskSchedulesUpdater = new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromSeconds(1)));

			result = AssertExceptionThrown<ArgumentNullException>(() =>
				taskSchedulesUpdater.ReloadUpdatedFromDatabase(
					null,
					Mock.Of<IServiceTasksReloader>(),
					Mock.Of<ITaskScheduler>()));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("allTasks"));

			result = AssertExceptionThrown<ArgumentNullException>(() =>
					taskSchedulesUpdater.ReloadUpdatedFromDatabase(
						Mock.Of<ICollection<IRunnableServiceTask>>(),
						null,
						Mock.Of<ITaskScheduler>()));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("updatedTasksReloader"));

			result = AssertExceptionThrown<ArgumentNullException>(() =>
					taskSchedulesUpdater.ReloadUpdatedFromDatabase(
						Mock.Of<ICollection<IRunnableServiceTask>>(),
						Mock.Of<IServiceTasksReloader>(),
						null));
			NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskScheduler"));
		}

		[ExpectNoExceptions]
		public void TestDoesNotReloadOnNotExpiredInterval()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "TST";
			var allTasks = new[]
			{
				Mock.Of<IRunnableServiceTask>(task => task.Code == schedule.S5_ScheduleType.ToString() && task.IsActive && task.HasSchedule),
			};

			Factory.Save();

			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderMock
				.Setup(provider => provider.Reload(It.IsAny<DateTimeOffset>()))
				.Returns(Mock.Of<IServiceTaskCollectionGovernor>(
					collection => collection.GovernedTasks == new List<IServiceTask> { Mock.Of<IServiceTask>(t => t.Code == "TST") }));
			var taskSchedulerMock = new Mock<ITaskScheduler>();

			taskSchedulerMock
				.Setup(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()));

			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

			var runnableServiceTasksScheduleUpdater = new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromMinutes(100)));
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloaderMock.Object,
					taskSchedulerMock.Object);
			serviceTasksReloaderMock.Invocations.Clear();

			// Act
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloaderMock.Object,
					taskSchedulerMock.Object);

			// Assert
			serviceTasksReloaderMock
				.Verify(
					provider => provider.Reload(It.IsAny<DateTimeOffset>()),
					Times.Never);
		}

		[ExpectNoExceptions]
		public void TestReloadsOnExpiredInterval()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "TST";
			var allTasks = new[]
			{
				Mock.Of<IRunnableServiceTask>(task => task.Code == schedule.S5_ScheduleType.ToString() && task.IsActive && task.HasSchedule),
			};

			Factory.Save();

			var serviceTaskConfigurationPollingInterval = TimeSpan.FromSeconds(5);
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderMock
				.Setup(provider => provider.Reload(It.IsAny<DateTimeOffset>()))
				.Returns(Mock.Of<IServiceTaskCollectionGovernor>(
					collection => collection.GovernedTasks == new List<IServiceTask> { Mock.Of<IServiceTask>(t => t.Code == "TST") }));
			var taskSchedulerMock = new Mock<ITaskScheduler>();
			taskSchedulerMock
				.Setup(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
				.Returns(new TasksActionResultDTO(new Dictionary<TaskCodeDTO, TaskActionResultDTO>()));

			using var memoryCache = new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None));

			var runnableServiceTasksScheduleUpdater = new RunnableServiceTasksScheduleUpdater(memoryCache, Mock.Of<IHostLogger>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == serviceTaskConfigurationPollingInterval));
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloaderMock.Object,
					taskSchedulerMock.Object);
			serviceTasksReloaderMock.Invocations.Clear();

			Thread.Sleep(serviceTaskConfigurationPollingInterval.Add(TimeSpan.FromSeconds(5)));

			// Act
			runnableServiceTasksScheduleUpdater
				.ReloadUpdatedFromDatabase(
					allTasks,
					serviceTasksReloaderMock.Object,
					taskSchedulerMock.Object);

		// Assert
		serviceTasksReloaderMock.Verify(provider => provider.Reload(It.IsAny<DateTimeOffset>()), Times.Once);
		}

		public void TestLogsReloadedTasks()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(
					new TasksActionResultDTO(
						new Dictionary<TaskCodeDTO, TaskActionResultDTO>()),
					loggerMock => loggerMock.VerifyNoOtherCalls);

				Test(
					new TasksActionResultDTO(
						new Dictionary<TaskCodeDTO, TaskActionResultDTO>
						{
							{ new TaskCodeDTO("ABC"), TaskActionResultDTO.Succeeded },
							{ new TaskCodeDTO("BCD"), TaskActionResultDTO.Succeeded },
						}),
					loggerMock => () =>
					{
						loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s.IndexOf("ABC", StringComparison.OrdinalIgnoreCase) >= 0)), Times.Once);
						loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s.IndexOf("BCD", StringComparison.OrdinalIgnoreCase) >= 0)), Times.Once);
						loggerMock.VerifyNoOtherCalls();
					});

				Test(
					new TasksActionResultDTO(
						new Dictionary<TaskCodeDTO, TaskActionResultDTO>
						{
							{ new TaskCodeDTO("QWE"), TaskActionResultDTO.Succeeded },
							{ new TaskCodeDTO("ASD"), TaskActionResultDTO.Succeeded },
							{ new TaskCodeDTO("ZXC"), TaskActionResultDTO.Succeeded },
						}),
					loggerMock => () =>
					{
						loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s.IndexOf("QWE", StringComparison.OrdinalIgnoreCase) >= 0)), Times.Once);
						loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s.IndexOf("ASD", StringComparison.OrdinalIgnoreCase) >= 0)), Times.Once);
						loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s.IndexOf("ZXC", StringComparison.OrdinalIgnoreCase) >= 0)), Times.Once);
						loggerMock.VerifyNoOtherCalls();
					});
			});

			void Test(TasksActionResultDTO tasksActionResultDto, Func<Mock<IHostLogger>, AnonymousMethod> assertLogger)
			{
				// Arrange
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var loggerMock = new Mock<IHostLogger>();

				var runnableServiceTasksScheduleUpdater = new RunnableServiceTasksScheduleUpdater(new MemoryCache(Mock.Of<ICancellationTokenProvider>(p => p.Token == CancellationToken.None)), loggerMock.Object, Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskConfigurationPollingInterval == TimeSpan.FromSeconds(1)));
				taskSchedulerMock
					.Setup(scheduler => scheduler.RequestReloadOfTaskConfiguration(It.IsAny<IEnumerable<TaskCodeDTO>>()))
					.Returns(tasksActionResultDto);

				// Act
				runnableServiceTasksScheduleUpdater
					.ReloadUpdatedFromDatabase(
						Enumerable.Empty<IRunnableServiceTask>(),
						new SchedulerServiceTasksReloader(),
						taskSchedulerMock.Object);

				// Assert
				AssertNoExceptionThrown(assertLogger(loggerMock));
			}
		}
	}
}
