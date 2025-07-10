using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Host.Testing.Core.Http;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;
using WTG.NUnit;
using static CargoWise.Data.Testing.DbConnectionTest;
using Async = System.Threading.Tasks;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRunTaskDispatcherInitializeHttpListenerWithoutWaitingForServiceHostsCache()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var timeStampOfInvocations = new Dictionary<string, TimeSpan>();
			var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			var invocations = new List<string>();
			var serviceTasksInitializerMock = new Mock<IServiceTasksInitializer>();
			serviceTasksInitializerMock
				.Setup(x => x.InitializeServiceTasks())
				.Callback(() =>
				{
					invocations.Add("InitializeServiceTasks");
					cancellationTokenSource.Cancel();
				})
				.Returns(false);

			var httpListenerInitialiserMock = new Mock<IHttpRequestProcessorInitialiser>();
			httpListenerInitialiserMock.Setup(x => x.ConfigureHttpRequestProcessor(It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>(), It.IsAny<IActionQueue>())).Callback(() =>
			{
				invocations.Add("ConfigureHttpListener");
				Thread.Sleep(10);
			});

			var serviceTasksInitializerFactoryMock = new Mock<IServiceTasksInitializerFactory>();
			serviceTasksInitializerFactoryMock
				.Setup(x => x.Create(
					It.IsAny<IProductRegistrationPeriodicChecker>(),
					It.IsAny<ITransactionAdapter>(),
					It.IsAny<IServiceTaskCollectionGovernor>(),
					It.IsAny<ITaskScheduler>(),
					It.IsAny<IAllTasksCollection>(),
					It.IsAny<IInitializationTaskRunner>(),
					It.IsAny<IHostLogger>(),
					It.IsAny<IEventLogger>(),
					It.IsAny<ITaskInitializationRequirementsChecker>(),
					It.IsAny<ITaskQueue>(),
					It.IsAny<IBackgroundThreadActionQueue>(),
					It.IsAny<IErrorReporterProxy>(),
					It.IsAny<IHostRegistrySettings>(),
					It.IsAny<IServiceTaskScheduleManager>(),
					It.IsAny<IServiceTaskScheduleStatusProvider>(),
					It.IsAny<IClientHostedServiceAttributeProvider>(),
					It.IsAny<IServiceHostsCache>()))
				.Returns(serviceTasksInitializerMock.Object);

			using (ObjectFactory.Substitute(serviceHostsCacheMock.Object))
			{
				using var controller = new ControllerForTest(
					Mock.Of<IControllerService>(),
					Mock.Of<IControllerUpgrade>(),
					Mock.Of<IAsyncDelayProvider>(),
					httpListenerInitialiserMock.Object,
					new ProcessRunnerPoolFactory(provider),
					serviceTasksInitializerFactory: serviceTasksInitializerFactoryMock.Object);

				// Act
				controller.RunTaskDispatcher(cancellationTokenSource.Token);

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					serviceTasksInitializerMock.Verify(x => x.InitializeServiceTasks(), Times.AtLeastOnce);
					httpListenerInitialiserMock.Verify(x => x.ConfigureHttpRequestProcessor(It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>(), It.IsAny<IActionQueue>()), Times.Once);
					NUnit.Framework.Assert.That(invocations, Is.EqualTo(new[] { "ConfigureHttpListener", "InitializeServiceTasks" }));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestRunTaskDispatcherConfigureQueueMonitorAfterServiceTasksInitialized()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var invocations = new List<string>();
			var serviceTasksInitializerMock = new Mock<IServiceTasksInitializer>();
			serviceTasksInitializerMock
				.Setup(x => x.InitializeServiceTasks())
				.Callback(() =>
				{
					invocations.Add("InitializeServiceTasks");
					cancellationTokenSource.Cancel();
				})
				.Returns(true);

			var queueMonitorInitializerMock = new Mock<IQueueMonitorInitializer>();
			queueMonitorInitializerMock
				.Setup(x => x.ConfigureQueueMonitor(It.IsAny<ITaskStatusProvider>()))
				.Callback(() =>
				{
					invocations.Add("ConfigureQueueMonitor");
				});
			var serviceTasksInitializerFactoryMock = new Mock<IServiceTasksInitializerFactory>();
			serviceTasksInitializerFactoryMock
				.Setup(x => x.Create(
					It.IsAny<IProductRegistrationPeriodicChecker>(),
					It.IsAny<ITransactionAdapter>(),
					It.IsAny<IServiceTaskCollectionGovernor>(),
					It.IsAny<ITaskScheduler>(),
					It.IsAny<IAllTasksCollection>(),
					It.IsAny<IInitializationTaskRunner>(),
					It.IsAny<IHostLogger>(),
					It.IsAny<IEventLogger>(),
					It.IsAny<ITaskInitializationRequirementsChecker>(),
					It.IsAny<ITaskQueue>(),
					It.IsAny<IBackgroundThreadActionQueue>(),
					It.IsAny<IErrorReporterProxy>(),
					It.IsAny<IHostRegistrySettings>(),
					It.IsAny<IServiceTaskScheduleManager>(),
					It.IsAny<IServiceTaskScheduleStatusProvider>(),
					It.IsAny<IClientHostedServiceAttributeProvider>(),
					It.IsAny<IServiceHostsCache>()))
				.Returns(serviceTasksInitializerMock.Object);

			using var controller = new ControllerForTest(
				Mock.Of<IControllerService>(),
				Mock.Of<IControllerUpgrade>(),
				processRunnerPoolFactory: new ProcessRunnerPoolFactory(provider),
				queueMonitorInitializer: queueMonitorInitializerMock.Object,
				serviceTasksInitializerFactory: serviceTasksInitializerFactoryMock.Object);

			// Act
			controller.RunTaskDispatcher(cancellationTokenSource.Token);

			// Assert
			serviceTasksInitializerMock.Verify(x => x.InitializeServiceTasks(), Times.AtLeastOnce);
			queueMonitorInitializerMock.Verify(x => x.ConfigureQueueMonitor(It.IsAny<ITaskStatusProvider>()), Times.Once);
			NUnit.Framework.Assert.That(invocations, Is.EqualTo(new[] { "InitializeServiceTasks", "ConfigureQueueMonitor" }));
		}

		public void TestDispatchingLoopDoesNotCatchCriticalExceptions()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				TestDispatchingLoopDoesNotCatchCriticalExceptions(new OutOfMemoryException());
				TestDispatchingLoopDoesNotCatchCriticalExceptions(new AppDomainUnloadedException());
			});

			void TestDispatchingLoopDoesNotCatchCriticalExceptions<T>(T criticalException) where T : Exception
			{
				// Arrange
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

				var productRegistrationPeriodicChecker = new Mock<IProductRegistrationPeriodicChecker>();
				productRegistrationPeriodicChecker
					.Setup(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown())
					.Throws(criticalException);

				var hostLoggerMock = new Mock<IHostLogger>();
				var taskScheduler = new Mock<ITaskScheduler>();

				var serviceTasksInitializer = new Mock<IServiceTasksInitializer>();
				serviceTasksInitializer
					.Setup(x => x.InitializeServiceTasks())
					.Returns(true);

				var dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);

				var serviceTasksInitializerFactory = new Mock<IServiceTasksInitializerFactory>();
				serviceTasksInitializerFactory
					.Setup(x => x.Create(
						It.IsAny<IProductRegistrationPeriodicChecker>(),
						It.IsAny<ITransactionAdapter>(),
						It.IsAny<IServiceTaskCollectionGovernor>(),
						It.IsAny<ITaskScheduler>(),
						It.IsAny<IAllTasksCollection>(),
						It.IsAny<IInitializationTaskRunner>(),
						It.IsAny<IHostLogger>(),
						It.IsAny<IEventLogger>(),
						It.IsAny<ITaskInitializationRequirementsChecker>(),
						It.IsAny<ITaskQueue>(),
						It.IsAny<IBackgroundThreadActionQueue>(),
						It.IsAny<IErrorReporterProxy>(),
						It.IsAny<IHostRegistrySettings>(),
						It.IsAny<IServiceTaskScheduleManager>(),
						It.IsAny<IServiceTaskScheduleStatusProvider>(),
						It.IsAny<IClientHostedServiceAttributeProvider>(),
						It.IsAny<IServiceHostsCache>()))
					.Returns(serviceTasksInitializer.Object);

				var processRunnerPoolFactory = new Mock<IProcessRunnerPoolFactory>();
				processRunnerPoolFactory
					.Setup(x => x.GetOrCreate())
					.Returns(new Mock<IProcessRunnerPool>().Object);

				var controller = new Controller(
					Mock.Of<IControllerService>(),
					Mock.Of<IEventLogger>(),
					hostLoggerMock.Object,
					taskScheduler.Object,
					productRegistrationPeriodicChecker.Object,
					new Mock<IControllerUpgrade>().Object,
					new Mock<IHttpRequestProcessorInitialiser>().Object,
					Mock.Of<IProcessRunnerPoolFactory>(f => f.GetOrCreate() == Mock.Of<IProcessRunnerPool>()),
					serviceTasksInitializerFactory.Object,
					new ServiceHostProviderFactory(Mock.Of<IHostRegistrySettings>()),
					Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
					Mock.Of<IQueueMonitorInitializer>(),
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTasksReloaderFactory>(x => x.CreateServiceTasksReloader() == Mock.Of<IServiceTaskLoader>()),
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IResourceThrottler>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>(),
					Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
					Mock.Of<ITaskRunRequestProcessor>(),
					Mock.Of<IAllTasksCollection>());

				using (ObjectFactory.Substitute(() => Mock.Of<IServiceHostsCache>()))
				{
					// Act
					var exception = AssertExceptionThrown<T>(() => controller.Run(CancellationToken.None));

					// Assert
					NUnit.Framework.Assert.That(criticalException, Is.EqualTo(exception));
					NUnit.Framework.Assert.That(exception.IsCriticalException(), Is.True);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDispatchingLoopInvokesControllerUpgradeOnDatabaseUpgradeExceptions()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				TestDispatchingLoopInvokesControllerUpgradeOnDatabaseUpgradeExceptions(new DatabaseUpgradedException());
				TestDispatchingLoopInvokesControllerUpgradeOnDatabaseUpgradeExceptions(new Mock<DatabaseUpgradeException>(string.Empty).Object);
				TestDispatchingLoopInvokesControllerUpgradeOnDatabaseUpgradeExceptions(new DatabaseUpgradeInProgressException());
			});

			void TestDispatchingLoopInvokesControllerUpgradeOnDatabaseUpgradeExceptions<T>(T databaseUpgradeException) where T : DatabaseUpgradeException
			{
				// Arrange
				const int cancellationTimeOut = 30;
				var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(cancellationTimeOut));

				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

				var productRegistrationPeriodicChecker = new Mock<IProductRegistrationPeriodicChecker>();
				productRegistrationPeriodicChecker
					.SetupSequence(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown())
					.Throws(databaseUpgradeException)
					.Returns(true);

				var hostLoggerMock = new Mock<IHostLogger>();
				var taskScheduler = new Mock<ITaskScheduler>();
				var controllerUpgrade = new Mock<IControllerUpgrade>();

				var serviceTasksInitializer = new Mock<IServiceTasksInitializer>();
				serviceTasksInitializer
					.Setup(x => x.InitializeServiceTasks())
					.Returns(true);

				var serviceTasksInitializerFactory = new Mock<IServiceTasksInitializerFactory>();
				serviceTasksInitializerFactory
					.Setup(x => x.Create(
						It.IsAny<IProductRegistrationPeriodicChecker>(),
						It.IsAny<ITransactionAdapter>(),
						It.IsAny<IServiceTaskCollectionGovernor>(),
						It.IsAny<ITaskScheduler>(),
						It.IsAny<IAllTasksCollection>(),
						It.IsAny<IInitializationTaskRunner>(),
						It.IsAny<IHostLogger>(),
						It.IsAny<IEventLogger>(),
						It.IsAny<ITaskInitializationRequirementsChecker>(),
						It.IsAny<ITaskQueue>(),
						It.IsAny<IBackgroundThreadActionQueue>(),
						It.IsAny<IErrorReporterProxy>(),
						It.IsAny<IHostRegistrySettings>(),
						It.IsAny<IServiceTaskScheduleManager>(),
						It.IsAny<IServiceTaskScheduleStatusProvider>(),
						It.IsAny<IClientHostedServiceAttributeProvider>(),
						It.IsAny<IServiceHostsCache>()))
					.Returns(serviceTasksInitializer.Object);

				var dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);

				var processRunnerPoolFactory = new Mock<IProcessRunnerPoolFactory>();
				processRunnerPoolFactory
					.Setup(x => x.GetOrCreate())
					.Returns(new Mock<IProcessRunnerPool>().Object);

				var transactionAdapterMock = new Mock<ITransactionAdapter>();
				var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
				var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
				serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);

				var controller = new Controller(
					Mock.Of<IControllerService>(),
					Mock.Of<IEventLogger>(),
					hostLoggerMock.Object,
					taskScheduler.Object,
					productRegistrationPeriodicChecker.Object,
					controllerUpgrade.Object,
					new Mock<IHttpRequestProcessorInitialiser>().Object,
					Mock.Of<IProcessRunnerPoolFactory>(f => f.GetOrCreate() == Mock.Of<IProcessRunnerPool>()),
					serviceTasksInitializerFactory.Object,
					new ServiceHostProviderFactory(Mock.Of<IHostRegistrySettings>()),
					Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
					Mock.Of<IQueueMonitorInitializer>(),
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					transactionAdapterMock.Object,
					serviceTasksReloaderFactoryMock.Object,
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IResourceThrottler>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>(),
					Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
					Mock.Of<ITaskRunRequestProcessor>(),
					Mock.Of<IAllTasksCollection>());

				// Act
				using (ObjectFactory.Substitute(Mock.Of<IServiceHostsCache>()))
				{
					controller.Run(cancellationToken.Token);
				}

				// Assert
				controllerUpgrade
					.Verify(x => x.UpgradeSoftwareIfNeeded(), Times.Once);
				errorReporterProxyMock
					.VerifyNoOtherCalls();
			}
		}

		[ExpectNoExceptions]
		public void TestDispatchingLoopReportNonCriticalExceptions()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				TestControllerRun_ReportNonCriticalExceptions(new Exception());
				TestControllerRun_ReportNonCriticalExceptions(new ArgumentException());
			});

			void TestControllerRun_ReportNonCriticalExceptions<T>(T exception) where T : Exception
			{
				// Arrange
				const int cancellationTimeOut = 30;
				var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(cancellationTimeOut));

				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

				var productRegistrationPeriodicChecker = new Mock<IProductRegistrationPeriodicChecker>();
				productRegistrationPeriodicChecker
					.SetupSequence(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown())
					.Throws(exception)
					.Returns(true);

				var hostLoggerMock = new Mock<IHostLogger>();
				var taskScheduler = new Mock<ITaskScheduler>();
				var controllerUpgrade = new Mock<IControllerUpgrade>();

				var serviceTasksInitializer = new Mock<IServiceTasksInitializer>();
				serviceTasksInitializer
					.Setup(x => x.InitializeServiceTasks())
					.Returns(true);

				var serviceTasksInitializerFactory = new Mock<IServiceTasksInitializerFactory>();
				serviceTasksInitializerFactory
					.Setup(x => x.Create(
						It.IsAny<IProductRegistrationPeriodicChecker>(),
						It.IsAny<ITransactionAdapter>(),
						It.IsAny<IServiceTaskCollectionGovernor>(),
						It.IsAny<ITaskScheduler>(),
						It.IsAny<IAllTasksCollection>(),
						It.IsAny<IInitializationTaskRunner>(),
						It.IsAny<IHostLogger>(),
						It.IsAny<IEventLogger>(),
						It.IsAny<ITaskInitializationRequirementsChecker>(),
						It.IsAny<ITaskQueue>(),
						It.IsAny<IBackgroundThreadActionQueue>(),
						It.IsAny<IErrorReporterProxy>(),
						It.IsAny<IHostRegistrySettings>(),
						It.IsAny<IServiceTaskScheduleManager>(),
						It.IsAny<IServiceTaskScheduleStatusProvider>(),
						It.IsAny<IClientHostedServiceAttributeProvider>(),
						It.IsAny<IServiceHostsCache>()))
					.Returns(serviceTasksInitializer.Object);

				var dateTimeProviderMock = new Mock<IDateTimeProvider>();
				dateTimeProviderMock
					.Setup(p => p.CurrentDateTimeUtc)
					.Returns(() => DateTime.UtcNow);

				var processRunnerPoolFactory = new Mock<IProcessRunnerPoolFactory>();
				processRunnerPoolFactory
					.Setup(x => x.GetOrCreate())
					.Returns(new Mock<IProcessRunnerPool>().Object);

				var transactionAdapterMock = new Mock<ITransactionAdapter>();
				var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
				var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
				serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);

				var controller = new Controller(
					Mock.Of<IControllerService>(),
					Mock.Of<IEventLogger>(),
					hostLoggerMock.Object,
					taskScheduler.Object,
					productRegistrationPeriodicChecker.Object,
					controllerUpgrade.Object,
					new Mock<IHttpRequestProcessorInitialiser>().Object,
					Mock.Of<IProcessRunnerPoolFactory>(f => f.GetOrCreate() == Mock.Of<IProcessRunnerPool>()),
					serviceTasksInitializerFactory.Object,
					new ServiceHostProviderFactory(Mock.Of<IHostRegistrySettings>()),
					Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
					Mock.Of<IQueueMonitorInitializer>(),
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					transactionAdapterMock.Object,
					serviceTasksReloaderFactoryMock.Object,
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IResourceThrottler>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>(),
					Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
					Mock.Of<ITaskRunRequestProcessor>(),
					Mock.Of<IAllTasksCollection>());

				// Act
				using (ObjectFactory.Substitute(() => Mock.Of<IServiceHostsCache>()))
				{
					controller.Run(cancellationToken.Token);
				}

				// Assert
				controllerUpgrade
					.Verify(x => x.UpgradeSoftwareIfNeeded(), Times.Never);
				errorReporterProxyMock
					.Verify(x => x.ReportOnce(It.IsAny<string>(), It.Is<Exception>(throwedException => throwedException == exception)), Times.Once);
				errorReporterProxyMock
					.VerifyNoOtherCalls();
				NUnit.Framework.Assert.That(!exception.IsCriticalException(), Is.True);
			}
		}

		public void TestControllerWillDispatchOnlyIfRegistrationIsValid()
		{
			// Arrange
			using var controller = new ControllerForTest(service: null);
			var taskQueue = new Mock<ITaskQueue>();
			taskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			var schedulerDispatcher = new Mock<ISchedulerDispatcher>();
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			// Act
			controller.RunTaskDispatchingRoundExposed(taskQueue.Object, schedulerDispatcher.Object);

			// Assert
			schedulerDispatcher.Verify(sd => sd.Schedule(taskQueue.Object), Times.AtLeastOnce);
			schedulerDispatcher.Verify(sd => sd.Dispatch(taskQueue.Object), Times.AtLeastOnce);
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestIfDBLoginTimeHasChangedRunDSATask()
		{
			// Arrange
			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);
			using var controller = new ControllerForTest(null, transactionAdapter: transactionAdapterMock.Object, serviceTasksReloaderFactory: serviceTasksReloaderFactoryMock.Object);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			// Act
			var assertionsAction = new Action(() =>
			{
				controller.DsaTaskRunner.Verify(dsa => dsa.RunDsaTaskIfDbServerRestarts(), Times.AtLeastOnce);
			});
			controller.RunTaskDispatchingLoopExposed(assertionsAction);
		}

		[ExpectNoExceptions]
		[TestDate(2016, 05, 26, 12, 00, 00)]
		public void TestController_ChecksRegistration_And_EmptiesQueue_IfTheSystemIsUnregistered()
		{
			// Arrange
			using var controller = new ControllerForTest(null);
			var taskMock = new Mock<IRunnableServiceTask>();
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>() { taskMock.Object });
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(false);

			// Act
			var assertionsAction = new Action(() =>
			{
				controller.RegChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.AtLeastOnce);
				controller.TaskQueue.Verify(q => q.EmptyQueue(), Times.AtLeastOnce);
			});
			controller.RunTaskDispatchingLoopExposed(assertionsAction);
		}

		[ExpectNoExceptions]
		[TestDate(2016, 05, 26, 12, 00, 00)]
		public void TestControllerInvokesUpgradeCheck_IfThereWasA15MinuteDifferenceFromTheLastCheck()
		{
			// Arrange
			using var controller = new ControllerForTest(null);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			controller.Upgrader.Setup(u => u.TimeSinceLastUpgradeCheck).Returns(TimeSpan.FromMinutes(15.0001));

			// Act
			var assertionsAction = new Action(() => controller.Upgrader.Verify(u => u.UpgradeSoftwareIfNeeded(), Times.AtLeastOnce));
			controller.RunTaskDispatchingLoopExposed(assertionsAction, loginTime: Db.Connection.LoginTime);
		}

		[ExpectNoExceptions]
		public void TestControllerCallsForDispatchingThrottler_AndSleeps()
		{
			// Arrange
			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);
			using var controller = new ControllerForTest(null, transactionAdapter: transactionAdapterMock.Object, serviceTasksReloaderFactory: serviceTasksReloaderFactoryMock.Object);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			using var ctsSource = new CancellationTokenSource();

			// Act
			var assertionsAction = new Action(() => controller.Throttler.Verify(tsu => tsu.GetTimeToSleep(It.IsAny<ICollection<IRunnableServiceTask>>(), It.IsAny<ICollection<IRunnableServiceTask>>(), It.IsAny<bool>()), Times.AtLeastOnce));
			controller.RunTaskDispatchingLoopExposed(assertionsAction);
		}

		[ExpectNoExceptions]
		public void TestControllerConstructorDoesNotTouchSql()
		{
			// Arrange
			var initialCommandCount = Db.Connection.ExecutedCommandCount;

			// Act
			using var controller = new Controller(Mock.Of<IControllerService>(),
				Mock.Of<IEventLogger>(),
				new Mock<IHostLogger>().Object,
				new Mock<ITaskScheduler>().Object,
				new Mock<IProductRegistrationPeriodicChecker>().Object,
				Mock.Of<IControllerUpgrade>(),
				new Mock<IHttpRequestProcessorInitialiser>().Object,
				Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
				Mock.Of<IProcessRunnerPoolFactory>(f => f.GetOrCreate() == Mock.Of<IProcessRunnerPool>()),
				Mock.Of<IServiceTasksInitializerFactory>(),
				Mock.Of<IServiceHostProviderFactory>(),
				Mock.Of<IQueueMonitorInitializer>(),
				new Mock<IErrorReporterProxy>().Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ITransactionAdapter>(),
				Mock.Of<IServiceTasksReloaderFactory>(),
				Mock.Of<IServiceTaskScheduleManager>(),
				Mock.Of<IResourceThrottler>(),
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				Mock.Of<IClientHostedServiceAttributeProvider>(),
				Mock.Of<IServiceHostsCache>(),
				Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
				Mock.Of<ITaskRunRequestProcessor>(),
				Mock.Of<IAllTasksCollection>());

			// Assert
			NUnit.Framework.Assert.That(initialCommandCount, Is.EqualTo(Db.Connection.ExecutedCommandCount));
		}

		[ExpectNoExceptions]
		public void TestControllerRecordActualDotNetVersion()
		{
			// Arrange
			using var controller = new ControllerForTest(null);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			// Act
			var assertionsAction = new Action(() => NUnit.Framework.Assert.That(controller.IsDotNetRecorded, Is.True));
			controller.RunTaskDispatchingLoopExposed(assertionsAction, lastDotNetRecord: DateTime.UtcNow.AddHours(-25));
		}

		[ExpectNoExceptions]
		public void TestControllerRecordActualDotNetVersionWithDBConnectionClosedDuringUpgrade()
		{
			// Arrange
			var closedConnection = false;
			try
			{
				using var controller = new ControllerForTest(null);
				controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
				controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
				controller.Upgrader.Setup(u => u.TimeSinceLastUpgradeCheck).Returns(TimeSpan.FromMinutes(15.0001));
				controller.Upgrader.Setup(u => u.UpgradeSoftwareIfNeeded())
					.Callback(() =>
					{
						Db.Connection.CloseConnection();
						closedConnection = true;
						Db.Connection.RollbackTransaction();
					});

				// Act
				var assertionsAction = new Action(() =>
				{
					controller.Upgrader.Verify(u => u.UpgradeSoftwareIfNeeded(), Times.AtLeastOnce);
					NUnit.Framework.Assert.That(controller.IsDotNetRecorded, Is.True);
				});
				controller.RunTaskDispatchingLoopExposed(assertionsAction, loginTime: Db.Connection.LoginTime, lastDotNetRecord: DateTime.UtcNow.AddHours(-25));
			}
			finally
			{
				if (closedConnection)
				{
					Db.Connection.BeginTransaction();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDsaTaskExists()
		{
			NUnit.Framework.Assert.That(ObjectFactory.Get<IClientHostedServiceAttributeProvider>().GetClientHostedServiceAttribute("DSA"), Is.Not.EqualTo(default(IHostedServiceAttribute)));
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		[ExpectNoExceptions]
		public void TestSchedulesAreSavedOnExit()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var delayProvider = new Mock<IAsyncDelayProvider>();
			var httpListenerInitialiser = new Mock<IHttpRequestProcessorInitialiser>();
			var serviceTaskInitFactory = new Mock<IServiceTasksInitializerFactory>();
			var serviceTaskInit = new Mock<IServiceTasksInitializer>();
			var controllerService = new Mock<IControllerService>();
			serviceTaskInit.Setup(si => si.InitializeServiceTasks()).Returns(true);
			var dsaTaskRunner = new Mock<IDSATaskRunner>();

			serviceTaskInit.Setup(si => si.CreateDSARunner()).Returns(dsaTaskRunner.Object);
			serviceTaskInitFactory
				.Setup(stf => stf.Create(
					It.IsAny<IProductRegistrationPeriodicChecker>(),
					It.IsAny<ITransactionAdapter>(),
					It.IsAny<IServiceTaskCollectionGovernor>(),
					It.IsAny<ITaskScheduler>(),
					It.IsAny<IAllTasksCollection>(),
					It.IsAny<IInitializationTaskRunner>(),
					It.IsAny<IHostLogger>(),
					It.IsAny<IEventLogger>(),
					It.IsAny<ITaskInitializationRequirementsChecker>(),
					It.IsAny<ITaskQueue>(),
					It.IsAny<IBackgroundThreadActionQueue>(),
					It.IsAny<IErrorReporterProxy>(),
					It.IsAny<IHostRegistrySettings>(),
					It.IsAny<IServiceTaskScheduleManager>(),
					It.IsAny<IServiceTaskScheduleStatusProvider>(),
					It.IsAny<IClientHostedServiceAttributeProvider>(),
					It.IsAny<IServiceHostsCache>()))
				.Returns(serviceTaskInit.Object);

			using var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None, delayProvider.Object);
			var actionQueueFactoryMock = new Mock<IBackgroundThreadActionQueueFactory>();
			actionQueueFactoryMock
				.Setup(f => f.BackgroundThreadActionQueue)
				.Returns(actionQueue);

			var runnerPoolFactory = new ProcessRunnerPoolFactory(provider);
			using var controller = new ControllerForTest(controllerService.Object,
				Mock.Of<IControllerUpgrade>(),
				delayProvider.Object,
				httpListenerInitialiser.Object,
				runnerPoolFactory,
				serviceTaskInitFactory.Object,
				actionQueueFactory: actionQueueFactoryMock.Object);

			var schedule = TaskSchedulerTest.CreateSchedule("TST", controller.ReferenceFactoryExposed);
			using var immediateActionQueue = new ImmediateActionQueue();
			var task = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), allowsMultiple: true, actionQueue: immediateActionQueue, transactionAdapter: transactionAdapter);

			controller.ReferenceFactoryExposed.Save();

			using var cancellationTokenSource = new CancellationTokenSource();
			var newNextRuntime = (DateTimeOffset?)null;

			runnerPoolFactory.Created += (f, runnerPool) => controller.ActionQueue.Enqueue(() =>
			{
				task.SetNextRunTimeBasedOnRecurrence();
				newNextRuntime = task.NextScheduledRunTime;
				cancellationTokenSource.Cancel();
				controller.ActionQueue.Wake();
			});

			controller.RunTaskDispatcher(cancellationTokenSource.Token);
			schedule.Reload();

			NUnit.Framework.Assert.That(task.NextScheduledRunTime, Is.EqualTo(newNextRuntime));
		}

		public void TestTaskScheduleIsSaved()
		{
			// Arrange
			var delayProvider = new Mock<IAsyncDelayProvider>();
			var httpListenerInitialiser = new Mock<IHttpRequestProcessorInitialiser>();
			var serviceTaskInitFactory = new Mock<IServiceTasksInitializerFactory>();
			var serviceTaskInit = new Mock<IServiceTasksInitializer>();
			serviceTaskInitFactory
				.Setup(stf => stf.Create(
					It.IsAny<IProductRegistrationPeriodicChecker>(),
					It.IsAny<ITransactionAdapter>(),
					It.IsAny<IServiceTaskCollectionGovernor>(),
					It.IsAny<ITaskScheduler>(),
					It.IsAny<IAllTasksCollection>(),
					It.IsAny<IInitializationTaskRunner>(),
					It.IsAny<IHostLogger>(),
					It.IsAny<IEventLogger>(),
					It.IsAny<ITaskInitializationRequirementsChecker>(),
					It.IsAny<ITaskQueue>(),
					It.IsAny<IBackgroundThreadActionQueue>(),
					It.IsAny<IErrorReporterProxy>(),
					It.IsAny<IHostRegistrySettings>(),
					It.IsAny<IServiceTaskScheduleManager>(),
					It.IsAny<IServiceTaskScheduleStatusProvider>(),
					It.IsAny<IClientHostedServiceAttributeProvider>(),
					It.IsAny<IServiceHostsCache>()))
				.Returns(serviceTaskInit.Object);
			serviceTaskInit.Setup(si => si.InitializeServiceTasks()).Returns(false);
			var runnerPoolFactory = new Mock<IProcessRunnerPoolFactory>();
			runnerPoolFactory
				.Setup(x => x.GetOrCreate())
				.Returns(Mock.Of<IProcessRunnerPool>());

			var taskScheduler = new Mock<ITaskScheduler>(MockBehavior.Strict);
			taskScheduler.Setup(scheduler => scheduler.Save());

			var logger = new Mock<IHostLogger>().Object;
			var transactionAdapterMock = Mock.Of<ITransactionAdapter>();

			using var controller = new ControllerForTest(Mock.Of<IControllerService>(),
				Mock.Of<IControllerUpgrade>(),
				delayProvider.Object,
				httpListenerInitialiser.Object,
				runnerPoolFactory.Object,
				serviceTaskInitFactory.Object,
				taskScheduler: taskScheduler.Object,
				logger: logger,
				transactionAdapter: transactionAdapterMock);

			// Act
			controller.RunTaskDispatcher(CancellationToken.None);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				taskScheduler.VerifyAll();
			});
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestRequestSwitchOffTimeOnExit()
		{
			// Arrange
			var delayProvider = new Mock<IAsyncDelayProvider>();
			var httpListenerInitialiser = new Mock<IHttpRequestProcessorInitialiser>();
			var serviceTaskInitFactory = new Mock<IServiceTasksInitializerFactory>();
			var serviceTaskInit = new Mock<IServiceTasksInitializer>();
			serviceTaskInit.Setup(si => si.InitializeServiceTasks()).Returns(true);
			var dsaTaskRunner = new Mock<IDSATaskRunner>();
			serviceTaskInit.Setup(si => si.CreateDSARunner()).Returns(dsaTaskRunner.Object);
			serviceTaskInitFactory
				.Setup(stf => stf.Create(
					It.IsAny<IProductRegistrationPeriodicChecker>(),
					It.IsAny<ITransactionAdapter>(),
					It.IsAny<IServiceTaskCollectionGovernor>(),
					It.IsAny<ITaskScheduler>(),
					It.IsAny<IAllTasksCollection>(),
					It.IsAny<IInitializationTaskRunner>(),
					It.IsAny<IHostLogger>(),
					It.IsAny<IEventLogger>(),
					It.IsAny<ITaskInitializationRequirementsChecker>(),
					It.IsAny<ITaskQueue>(),
					It.IsAny<IBackgroundThreadActionQueue>(),
					It.IsAny<IErrorReporterProxy>(),
					It.IsAny<IHostRegistrySettings>(),
					It.IsAny<IServiceTaskScheduleManager>(),
					It.IsAny<IServiceTaskScheduleStatusProvider>(),
					It.IsAny<IClientHostedServiceAttributeProvider>(),
					It.IsAny<IServiceHostsCache>()))
				.Returns(serviceTaskInit.Object);

			using var cancellationTokenSource = new CancellationTokenSource();
			var runnerPoolFactoryMock = new Mock<IProcessRunnerPoolFactory>();
			var controllerServiceMock = new Mock<IControllerService>();
			var switchOffTimeSpan = TimeSpan.FromSeconds(35.8);
			controllerServiceMock.Setup(service => service.RequireSwitchOffTime()).Returns(switchOffTimeSpan);
			var processRunnerPoolMock = new Mock<IProcessRunnerPool>();

			using var controller = new ControllerForTest(controllerServiceMock.Object,
				Mock.Of<IControllerUpgrade>(),
				delayProvider.Object,
				httpListenerInitialiser.Object,
				runnerPoolFactoryMock.Object,
				serviceTaskInitFactory.Object);

			runnerPoolFactoryMock.Setup(factory => factory.GetOrCreate())
				.Returns(processRunnerPoolMock.Object)
				.Callback(() =>
				{
					cancellationTokenSource.Cancel();
					controller.ActionQueue.Wake();
				});

			// Act
			controller.RunTaskDispatcher(cancellationTokenSource.Token);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				controllerServiceMock.Verify(service => service.RequireSwitchOffTime(), Times.Once);
				processRunnerPoolMock.Verify(pool => pool.Stop(It.IsAny<TimeSpan>()), Times.Once);
				processRunnerPoolMock.Verify(pool => pool.Stop(switchOffTimeSpan), Times.Once);
			});
		}

		public static IBackgroundDataSaverFactory CreateMockDataSaverFactory()
		{
			var mockBackupFactory = new Mock<IBackgroundDataSaverFactory>();
			var mockBackup = new Mock<IBackgroundDataSaver>();
			mockBackupFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<BackgroundDataFileAction>(), It.IsAny<Action<Exception>>())).Returns(mockBackup.Object);
			return mockBackupFactory.Object;
		}

		[ExpectNoExceptions]
		public void TestRunTaskDispatchingLoopUpdateTaskSchedules()
		{
			// Arrange
			var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
			var allTasks = new List<IRunnableServiceTask> { runnableServiceTaskMock.Object };
			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);

			var runnableServiceTasksScheduleUpdaterMock = new Mock<IRunnableServiceTasksScheduleUpdater>();
			using var controller = new ControllerForTest(Mock.Of<IControllerService>(), runnableServiceTasksScheduleUpdater: runnableServiceTasksScheduleUpdaterMock.Object, transactionAdapter: transactionAdapterMock.Object, serviceTasksReloaderFactory: serviceTasksReloaderFactoryMock.Object);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			// Act
			controller.RunTaskDispatchingLoopExposed(() =>
			{
				// Assert
				runnableServiceTasksScheduleUpdaterMock.Verify(
					x => x.ReloadUpdatedFromDatabase(allTasks, It.IsAny<IServiceTasksReloader>(), It.IsAny<ITaskScheduler>()),
					Times.AtLeastOnce);
			},
			loginTime: Db.Connection.LoginTime, lastDotNetRecord: DateTime.UtcNow.AddHours(-25), allTasks: allTasks);
		}

		[ExpectNoExceptions]
		public void TestRunTaskDispatchingLoopWithInnerExceptionIsDatabaseUpgradeException()
		{
			var controllerService = new Mock<IControllerService>();
			using var controller = new ControllerForTest(controllerService.Object);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			controller.Upgrader.Setup(u => u.UpgradeSoftwareIfNeeded());

			Async.Task.Run(() => controller.ActionQueue.Enqueue(() => throw new ApplicationException("Loading Error", new DatabaseUpgradeInProgressException()))).Wait();
			var assertionsAction = new Action(() => controller.Upgrader.Verify(u => u.UpgradeSoftwareIfNeeded(), Times.AtLeastOnce));
			controller.RunTaskDispatchingLoopExposed(assertionsAction, loginTime: Db.Connection.LoginTime, lastDotNetRecord: DateTime.UtcNow.AddHours(-25));
		}

		[ExpectNoExceptions]
		public void TestRunTaskDispatchingLoopWithSqlException()
		{
			AssertExceptionIsLoggedByRunLoop(SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired"));
		}

		[ExpectNoExceptions]
		public void TestRunTaskDispatchingLoopWithZSaveException()
		{
			AssertExceptionIsLoggedByRunLoop(new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired"), null, null), null));
		}

		void AssertExceptionIsLoggedByRunLoop(Exception ex)
		{
			var controllerService = new Mock<IControllerService>();
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			using var controller = new ControllerForTest(controllerService.Object, errorReporterProxy: errorReporterProxyMock.Object);
			controller.TaskQueue.Setup(tq => tq.GetQueueSnapshot()).Returns(new List<IRunnableServiceTask>());
			controller.RegChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			Async.Task.Run(() => controller.ActionQueue.Enqueue(() => throw ex));

			controller.RunTaskDispatchingLoopExposed(() =>
			{
				errorReporterProxyMock.Verify(x => x.ReportOnce("Controller Dispatching Loop", ex), Times.AtLeastOnce);
			});
		}

		[ExpectNoExceptions]
		public void TestServiceStartSetsServiceHostActiveStatus()
		{
			var activeHostname = "active.host";
			var inactiveHostname = "inactive.host";
			var newHostname = "new.host";

			var activeHost = Factory.New<StmServiceHost>();
			activeHost.SH_HostName = activeHostname;
			activeHost.SH_IsActive = true;
			var inactiveHost = Factory.New<StmServiceHost>();
			inactiveHost.SH_HostName = inactiveHostname;
			inactiveHost.SH_IsActive = false;
			Factory.Save();

			var serviceHostProvider = new ServiceHostProvider(Factory, Mock.Of<IHostRegistrySettings>());
			var loadedActiveHost = serviceHostProvider.LoadServiceHost(activeHostname);
			var loadedInactiveHost = serviceHostProvider.LoadServiceHost(inactiveHostname);
			var loadedNewHost = serviceHostProvider.LoadServiceHost(newHostname);
			var createdNewHost = serviceHostProvider.CreateServiceHost(newHostname);

			NUnit.Framework.Assert.That(Factory.Load<StmServiceHost>(activeHost.PK).SH_IsActive, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Service host IsActive should be true for loaded host");
			NUnit.Framework.Assert.That(Factory.Load<StmServiceHost>(inactiveHost.PK).SH_IsActive, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Service host IsActive should be true for loaded host");
			NUnit.Framework.Assert.That(loadedNewHost, Is.EqualTo(default(StmServiceHost)), "New host should be hull");
			NUnit.Framework.Assert.That(Factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, newHostname)).SH_IsActive, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Service host IsActive should be true for new host when created");
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName })
				.BuildServiceProvider();
		}
		protected override void TearDown()
		{
			disposeContainerAction?.Dispose();
			provider?.Dispose();

			// Contructing a ControllerService will disable this
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();

			base.TearDown();
		}

		readonly IDisposable disposeContainerAction;
		ServiceProvider provider;
	}

	class ControllerTestWithoutTransaction : TestCase
	{
		public void TestWrongConstructorParams()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var controllerServiceMock = new Mock<IControllerService>();
				var hostLoggerMock = new Mock<IHostLogger>();
				var taskSchedulerMock = new Mock<ITaskScheduler>();
				var productRegistrationPeriodicCheckerMock = new Mock<IProductRegistrationPeriodicChecker>();
				var httpListenerInitialiserMock = new Mock<IHttpRequestProcessorInitialiser>();
				var runnableServiceTasksScheduleUpdaterMock = new Mock<IRunnableServiceTasksScheduleUpdater>();
				var controllerUpgrader = Mock.Of<IControllerUpgrade>();
				var queueMonitorInitializer = Mock.Of<IQueueMonitorInitializer>();
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				var transactionAdapterMock = Mock.Of<ITransactionAdapter>();
				var serviceTasksReloaderFactoryMock = Mock.Of<IServiceTasksReloaderFactory>();
				var serviceTaskScheduleManagerMock = Mock.Of<IServiceTaskScheduleManager>();
				var resourceThrottlerMock = Mock.Of<IResourceThrottler>();
				var serviceTaskScheduleStatusProvider = Mock.Of<IServiceTaskScheduleStatusProvider>();
				var hostedServiceAttributeProvider = Mock.Of<IClientHostedServiceAttributeProvider>();
				var serviceHostsCache = Mock.Of<IServiceHostsCache>();
				var backgroundThreadActionQueueFactoryMock = Mock.Of<IBackgroundThreadActionQueueFactory>();
				var taskRunnerMock = Mock.Of<ITaskRunRequestProcessor>();
				var allTasksMock = Mock.Of<IAllTasksCollection>();

				var result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(null, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader,  httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("service"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, null, hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("eventLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), null, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader,  httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, null, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader,  httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskScheduler"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, null, controllerUpgrader,  httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("productRegistrationPeriodicChecker"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, null, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("httpRequestProcessorInitialiser"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader,  httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), null, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("runnableServiceTasksScheduleUpdater"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, null, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("controllerUpgrade"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, null, Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("processRunnerPoolFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), null, Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTasksInitializerFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), null, runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostProviderFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, null, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("queueMonitorInitializer"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, null, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, null, transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostRegistry"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), null, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("transactionAdapter"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, null, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTasksReloaderFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, null, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTaskScheduleManager"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, null, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("resourceThrottler"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, null, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceTaskScheduleStatusProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, null, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostedServiceAttributeProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, null, backgroundThreadActionQueueFactoryMock, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceHostsCache"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, null, taskRunnerMock, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("backgroundThreadActionQueueFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, null, allTasksMock));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("taskRunner"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new Controller(controllerServiceMock.Object, Mock.Of<IEventLogger>(), hostLoggerMock.Object, taskSchedulerMock.Object, productRegistrationPeriodicCheckerMock.Object, controllerUpgrader, httpListenerInitialiserMock.Object, Mock.Of<IProcessRunnerPoolFactory>(), Mock.Of<IServiceTasksInitializerFactory>(), Mock.Of<IServiceHostProviderFactory>(), runnableServiceTasksScheduleUpdaterMock.Object, queueMonitorInitializer, errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), transactionAdapterMock, serviceTasksReloaderFactoryMock, serviceTaskScheduleManagerMock, resourceThrottlerMock, serviceTaskScheduleStatusProvider, hostedServiceAttributeProvider, serviceHostsCache, backgroundThreadActionQueueFactoryMock, taskRunnerMock, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("allTasks"));
			});
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestControllerDoesNotThrowDatabaseUpgradeExceptionTwice()
		{
			var loggerMock = new Mock<IHostLogger>();
			var runnerPoolFactory = new ProcessRunnerPoolFactory(provider);
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			regChecker.Setup(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			var taskScheduler = new Mock<ITaskScheduler>();
			runnerPoolFactory.Created += (f, runnerPool) =>
			{
				var runner = new ProcessServiceRunnerCoreForTesting(new Mock<IBackgroundThreadActionQueue>().Object, "TestRunner", loggerMock.Object, Mock.Of<IDateTimeProvider>());
				var config = new HostedServiceAttribute() { TypeName = "foo", Code = "", TypeAssemblyName = "foo.dll" };
				var taskInfo = new ServiceTaskInfo(config);
				var runnableTask = new Mock<IRunnableServiceTask>();
				runnableTask.Setup(rt => rt.Info).Returns(taskInfo);
				var request = new ScheduledTaskRunRequest(runnableTask.Object);
				runner.Run(request);
				Thread.Sleep(TimeSpan.FromSeconds(1));
			};

			using (var upgradedConnection = new UpgradedDbConnectionForTest(Db.ServerName, Db.DatabaseName) { shouldSayDbSchemaHasChanged = false })
			{
				var requestListenerFactory = new RequestProcessorInitialiserThatSimulatesUpgrade(upgradedConnection);
				using var ctsSource = new CancellationTokenSource();
				using var controller = new ControllerForTest(service: null, upgrader: Mock.Of<IControllerUpgrade>(), delayProvider: null, httpListenerInitialiser: requestListenerFactory, processRunnerPoolFactory: runnerPoolFactory, taskScheduler: taskScheduler.Object);
				var dbEnv = new DbEnvironmentWithMockGuiPluginForTest();

				var existingEnv = DbEnv.Instance;
				try
				{
					DbEnv.SetDbEnvironment(dbEnv);
					Db.ConnectionOverrideForTest = upgradedConnection;
					NUnit.Framework.Assert.That(Db.IsSchemaVersionCheckDisabled, Is.EqualTo(false), "PRE");

					AssertExceptionThrown(typeof(DatabaseUpgradedException), () => controller.RunTaskDispatcher(ctsSource.Token));
				}
				finally
				{
					DbEnv.SetDbEnvironment(existingEnv);
					Db.ConnectionOverrideForTest = null;
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestControllerHandlesAggregateDatabaseUpgradeException()
		{
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			using var ctsSource = new CancellationTokenSource();
			var httpListenerFactory = new Mock<IHttpListenerFactory>();
			using (var httpListener = new HttpRequestListenerTaskTest.LimitedRequestHttpListener(0))
			{
				httpListenerFactory.Setup(lf => lf.Create(It.IsAny<string>())).Returns(httpListener);
				var httpListenerInitialiser = new Mock<IHttpRequestProcessorInitialiser>();
				var serviceTasksInitializerFactory = new Mock<IServiceTasksInitializerFactory>();
				serviceTasksInitializerFactory.Setup(tif => tif.Create(
						It.IsAny<IProductRegistrationPeriodicChecker>(),
						It.IsAny<ITransactionAdapter>(),
						It.IsAny<IServiceTaskCollectionGovernor>(),
						It.IsAny<ITaskScheduler>(),
						It.IsAny<IAllTasksCollection>(),
						It.IsAny<IInitializationTaskRunner>(),
						It.IsAny<IHostLogger>(),
						It.IsAny<IEventLogger>(),
						It.IsAny<ITaskInitializationRequirementsChecker>(),
						It.IsAny<ITaskQueue>(),
						It.IsAny<IBackgroundThreadActionQueue>(),
						It.IsAny<IErrorReporterProxy>(),
						It.IsAny<IHostRegistrySettings>(),
						It.IsAny<IServiceTaskScheduleManager>(),
						It.IsAny<IServiceTaskScheduleStatusProvider>(),
						It.IsAny<IClientHostedServiceAttributeProvider>(),
						It.IsAny<IServiceHostsCache>()))
					.Throws(new DatabaseUpgradedException());
				using var controller = new ControllerForTest(Mock.Of<IControllerService>(),
					Mock.Of<IControllerUpgrade>(),
					null,
					httpListenerInitialiser.Object,
					new ProcessRunnerPoolFactory(provider),
					serviceTasksInitializerFactory.Object,
					errorReporterProxy: errorReporterProxyMock.Object);
				var dbEnv = new DbEnvironmentWithMockGuiPluginForTest();

				var existingEnv = DbEnv.Instance;
				try
				{
					DbEnv.SetDbEnvironment(dbEnv);
					AssertExceptionThrown<DatabaseUpgradeException>(() => controller.RunTaskDispatcher(CancellationToken.None));
					serviceTasksInitializerFactory.Verify(
						expression: tif => tif.Create(
							It.IsAny<IProductRegistrationPeriodicChecker>(),
							It.IsAny<ITransactionAdapter>(),
							It.IsAny<IServiceTaskCollectionGovernor>(),
							It.IsAny<ITaskScheduler>(),
							It.IsAny<IAllTasksCollection>(),
							It.IsAny<IInitializationTaskRunner>(),
							It.IsAny<IHostLogger>(),
							It.IsAny<IEventLogger>(),
							It.IsAny<ITaskInitializationRequirementsChecker>(),
							It.IsAny<ITaskQueue>(),
							It.IsAny<IBackgroundThreadActionQueue>(),
							It.IsAny<IErrorReporterProxy>(),
							It.IsAny<IHostRegistrySettings>(),
							It.IsAny<IServiceTaskScheduleManager>(),
							It.IsAny<IServiceTaskScheduleStatusProvider>(),
							It.IsAny<IClientHostedServiceAttributeProvider>(),
							It.IsAny<IServiceHostsCache>()),
						times: Times.AtLeastOnce);
					Mock.Get(dbEnv.ConnectionGuiPlugin).Verify(gp => gp.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradedException>()), Times.Never);
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				}
				finally
				{
					DbEnv.SetDbEnvironment(existingEnv);
				}
				httpListenerFactory.Verify(lf => lf.Create(It.IsAny<string>()), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestLogMessageForDispatchingLoop()
		{
			// Arrange
			using var cancellation = new CancellationTokenSource();
			var sequence = new MockSequence();
			var logger = new Mock<IHostLogger>();
			logger
				.Setup(x => x.LogSection(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback(() => cancellation.Cancel());
			var throttling = new Mock<IDispatchingLoopThrottling>();
			throttling
				.Setup(x => x.GetTimeToSleep(It.IsAny<IEnumerable<IRunnableServiceTask>>(), It.IsAny<ICollection<IRunnableServiceTask>>(), It.IsAny<bool>()))
				.Returns(TimeSpan.Zero);
			var transactionAdapterMock = new Mock<ITransactionAdapter>();
			var serviceTasksReloaderFactoryMock = new Mock<IServiceTasksReloaderFactory>();
			var serviceTasksReloaderMock = new Mock<IServiceTasksReloader>();
			serviceTasksReloaderFactoryMock.Setup(x => x.CreateServiceTasksReloader()).Returns(serviceTasksReloaderMock.Object);

			using var controller = new ControllerForTest(
				Mock.Of<IControllerService>(),
				Mock.Of<IControllerUpgrade>(),
				Mock.Of<IAsyncDelayProvider>(),
				Mock.Of<IHttpRequestProcessorInitialiser>(),
				Mock.Of<IProcessRunnerPoolFactory>(),
				Mock.Of<IServiceTasksInitializerFactory>(),
				Mock.Of<ISchedulerDispatcher>(),
				Mock.Of<IBackgroundThreadActionQueueFactory>(),
				Mock.Of<IDispatchingLoopThrottling>(),
				Mock.Of<ITaskScheduler>(),
				logger.Object,
				transactionAdapter: transactionAdapterMock.Object,
				serviceTasksReloaderFactory: serviceTasksReloaderFactoryMock.Object);

			// Act
			controller.RunTaskDispatchingLoopExposed(
				Mock.Of<ITaskScheduler>(),
				Mock.Of<ITaskQueue>(),
				Array.Empty<IRunnableServiceTask>(),
				Mock.Of<ISchedulerDispatcher>(),
				Mock.Of<IProductRegistrationPeriodicChecker>(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown()),
				Mock.Of<IServiceHostTerminator>(),
				Mock.Of<IDispatchingLoopThrottling>(),
				Mock.Of<IControllerUpgrade>(),
				Mock.Of<IDSATaskRunner>(),
				Db.Connection.LoginTime,
				DateTime.MaxValue,
				cancellation.Token);

			// Assert
			logger.Verify(x => x.LogSection(LogLevel.Debug, "Starting dispatch round.", "Dispatch round is finished."));
			logger.VerifyNoOtherCalls();
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName })
				.BuildServiceProvider();
		}

		protected override void TearDown()
		{
			provider?.Dispose();

			base.TearDown();
		}

		ServiceProvider provider;
	}

	class DbEnvironmentWithMockGuiPluginForTest : BaseDbEnvironment
	{
		readonly IDbConnectionGuiPlugin connectionGuiPlugin = new Mock<IDbConnectionGuiPlugin>().Object;

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
	}

	class RequestProcessorInitialiserThatSimulatesUpgrade : IHttpRequestProcessorInitialiser
	{
		readonly UpgradedDbConnectionForTest upgradedDbConnection;

		public RequestProcessorInitialiserThatSimulatesUpgrade(UpgradedDbConnectionForTest upgradedDbConnection)
		{
			this.upgradedDbConnection = upgradedDbConnection;
		}

		public void ConfigureHttpRequestProcessor(ITaskScheduler scheduler, ITaskStatusProvider statusProvider, IActionQueue actionQueue)
		{
			upgradedDbConnection.shouldSayDbSchemaHasChanged = true;
			upgradedDbConnection.CloseConnection();
			upgradedDbConnection.EnsureIsOpen(); //Should throw upgraded exception

			throw new InvalidOperationException("Should not arrive here. EnsureIsOpen should have thrown DatabaseUpgradedException.");
		}
	}

	class ControllerForTest : Controller
	{
		public Mock<IDSATaskRunner> DsaTaskRunner { get; private set; }
		public Mock<ITaskQueue> TaskQueue { get; set; }
		public Mock<ISchedulerDispatcher> SchedulerDispatcher { get; private set; }
		public Mock<IProductRegistrationPeriodicChecker> RegChecker { get; private set; }
		public Mock<IServiceHostTerminator> HostTerminator { get; private set; }
		public Mock<IDispatchingLoopThrottling> Throttler { get; private set; }
		public Mock<IControllerUpgrade> Upgrader { get; private set; }

		public ControllerForTest(
			IControllerService service,
			IControllerUpgrade upgrader,
			IAsyncDelayProvider delayProvider = null,
			IHttpRequestProcessorInitialiser httpListenerInitialiser = null,
			IProcessRunnerPoolFactory processRunnerPoolFactory = null,
			IServiceTasksInitializerFactory serviceTasksInitializerFactory = null,
			ISchedulerDispatcher schedulerDispatcher = null,
			IBackgroundThreadActionQueueFactory actionQueueFactory = null,
			IDispatchingLoopThrottling throttler = null,
			ITaskScheduler taskScheduler = null,
			IHostLogger logger = null,
			IRunnableServiceTasksScheduleUpdater runnableServiceTasksScheduleUpdater = null,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner = null,
			IQueueMonitorInitializer queueMonitorInitializer = null,
			IDateTimeProvider dateTimeProvider = null,
			IErrorReporterProxy errorReporterProxy = null,
			ITransactionAdapter transactionAdapter = null,
			IServiceTasksReloaderFactory serviceTasksReloaderFactory = null
		)
			: base(
				service ?? Mock.Of<IControllerService>(),
				Mock.Of<IEventLogger>(),
				logger ?? new Mock<IHostLogger>().Object,
				taskScheduler ?? Mock.Of<ITaskScheduler>(),
				new Mock<IProductRegistrationPeriodicChecker>().Object,
				upgrader ?? Mock.Of<IControllerUpgrade>(),
				httpListenerInitialiser ?? new Mock<IHttpRequestProcessorInitialiser>().Object,
				processRunnerPoolFactory ?? Mock.Of<IProcessRunnerPoolFactory>(),
				serviceTasksInitializerFactory ?? Mock.Of<IServiceTasksInitializerFactory>(),
				Mock.Of<IServiceHostProviderFactory>(),
				runnableServiceTasksScheduleUpdater ?? Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
				queueMonitorInitializer ?? Mock.Of<IQueueMonitorInitializer>(),
				errorReporterProxy ?? new Mock<IErrorReporterProxy>().Object,
				Mock.Of<IHostRegistrySettings>(),
				transactionAdapter ?? Mock.Of<ITransactionAdapter>(),
				serviceTasksReloaderFactory ?? Mock.Of<IServiceTasksReloaderFactory>(x => x.CreateServiceTasksReloader() == Mock.Of<IServiceTaskLoader>()),
				Mock.Of<IServiceTaskScheduleManager>(),
				Mock.Of<IResourceThrottler>(),
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				Mock.Of<IClientHostedServiceAttributeProvider>(),
				Mock.Of<IServiceHostsCache>(),
				actionQueueFactory ?? Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
				Mock.Of<ITaskRunRequestProcessor>(),
				Mock.Of<IAllTasksCollection>())
		{
			InitialiseMocks(delayProvider, schedulerDispatcher, actionQueue, throttler);
		}

		public ControllerForTest(
			IControllerService service = null,
			BusinessObjectFactory referenceFactory = null,
			IHostLogger logger = null,
			IRunnableServiceTasksScheduleUpdater runnableServiceTasksScheduleUpdater = null,
			IErrorReporterProxy errorReporterProxy = null,
			ITransactionAdapter transactionAdapter = null,
			IServiceTasksReloaderFactory serviceTasksReloaderFactory = null)
			: base(
				service ?? Mock.Of<IControllerService>(),
				Mock.Of<IEventLogger>(),
				logger ?? new Mock<IHostLogger>().Object,
				new Mock<ITaskScheduler>(MockBehavior.Strict).Object,
				new Mock<IProductRegistrationPeriodicChecker>().Object,
				Mock.Of<IControllerUpgrade>(),
				new Mock<IHttpRequestProcessorInitialiser>().Object,
				runnableServiceTasksScheduleUpdater ?? Mock.Of<IRunnableServiceTasksScheduleUpdater>(),
				Mock.Of<IProcessRunnerPoolFactory>(),
				Mock.Of<IServiceTasksInitializerFactory>(),
				Mock.Of<IServiceHostProviderFactory>(),
				Mock.Of<IQueueMonitorInitializer>(),
				errorReporterProxy ?? new Mock<IErrorReporterProxy>().Object,
				Mock.Of<IHostRegistrySettings>(),
				transactionAdapter ?? Mock.Of<ITransactionAdapter>(),
				serviceTasksReloaderFactory ?? Mock.Of<IServiceTasksReloaderFactory>(x => x.CreateServiceTasksReloader() == Mock.Of<IServiceTaskLoader>()),
				Mock.Of<IServiceTaskScheduleManager>(),
				Mock.Of<IResourceThrottler>(),
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				Mock.Of<IClientHostedServiceAttributeProvider>(),
				Mock.Of<IServiceHostsCache>(),
				Mock.Of<IBackgroundThreadActionQueueFactory>(f => f.BackgroundThreadActionQueue == Mock.Of<IBackgroundThreadActionQueue>()),
				Mock.Of<ITaskRunRequestProcessor>(),
				Mock.Of<IAllTasksCollection>())
		{
			_ReferenceFactory = referenceFactory;
			InitialiseMocks();
		}

		void InitialiseMocks(IAsyncDelayProvider delayProvider = null, ISchedulerDispatcher schedulerDispatcher = null, IBackgroundThreadActionQueue actionQueue = null, IDispatchingLoopThrottling throttler = null)
		{
			DsaTaskRunner = new Mock<IDSATaskRunner>();
			TaskQueue = new Mock<ITaskQueue>();
			SchedulerDispatcher = schedulerDispatcher == null ? new Mock<ISchedulerDispatcher>() : Mock.Get(schedulerDispatcher);
			RegChecker = new Mock<IProductRegistrationPeriodicChecker>();
			HostTerminator = new Mock<IServiceHostTerminator>();
			Throttler = throttler == null ? new Mock<IDispatchingLoopThrottling>() : Mock.Get(throttler);
			Upgrader = new Mock<IControllerUpgrade>();
			this.actionQueue = actionQueue ?? new BackgroundThreadActionQueue(CancellationToken.None, delayProvider);
			hostProviderFactoryMock = new Mock<IServiceHostProviderFactory>();
			var hostProvider = new Mock<IServiceHostProvider>();
			hostProviderFactoryMock.Setup(hf => hf.Create(It.IsAny<BusinessObjectFactory>())).Returns(hostProvider.Object);
			hostProviderFactory = hostProviderFactoryMock.Object;
		}

		public void RunTaskDispatchingLoopExposed(
			ITaskScheduler taskScheduler,
			ITaskQueue taskQueue,
			IEnumerable<IRunnableServiceTask> allTasks,
			ISchedulerDispatcher schedulerDispatcher,
			IProductRegistrationPeriodicChecker regChecker,
			IServiceHostTerminator serviceHostTerminator,
			IDispatchingLoopThrottling throttlingPeriodProvider,
			IControllerUpgrade upgrader,
			IDSATaskRunner dsaTaskRunner,
			DateTime loginTime,
			DateTime lastDotNetRecord,
			CancellationToken cancellationToken)
		{
			RunTaskDispatchingLoop(
				taskScheduler,
				taskQueue,
				allTasks,
				schedulerDispatcher,
				regChecker,
				serviceHostTerminator,
				throttlingPeriodProvider,
				upgrader,
				dsaTaskRunner,
				loginTime,
				lastDotNetRecord,
				cancellationToken);
		}

		public void RunTaskDispatchingRoundExposed(ITaskQueue taskQueue, ISchedulerDispatcher schedulerDispatcher) => RunTaskDispatchingRound(taskQueue, schedulerDispatcher);

		public void RunTaskDispatchingLoopExposed(Action assertionsAction, DateTime? loginTime = null, DateTime? lastDotNetRecord = null, List<IRunnableServiceTask> allTasks = null, ITaskScheduler taskScheduler = null)
		{
			using var ctsSource = new CancellationTokenSource();
			taskScheduler = taskScheduler ?? Mock.Of<ITaskScheduler>();

			var thread = new Thread(() =>
			{
				for (var i = 0; !CheckAssertions(assertionsAction) && i < 25; i++)
				{
					Thread.Sleep(200);
				}
				ctsSource.Cancel();
			});
			thread.Start();
			try
			{
				RunTaskDispatchingLoop(
					taskScheduler,
					TaskQueue.Object,
					allTasks ?? new List<IRunnableServiceTask>(),
					SchedulerDispatcher.Object,
					RegChecker.Object,
					HostTerminator.Object,
					Throttler.Object,
					Upgrader.Object,
					DsaTaskRunner.Object,
					loginTime ?? DateTime.MinValue,
					lastDotNetRecord ?? DateTime.UtcNow,
					ctsSource.Token);
			}
			finally
			{
				thread.Join();
			}

			assertionsAction();
		}

		static bool CheckAssertions(Action expectationAction)
		{
			try
			{
				expectationAction();
				return true;
			}
			catch
			{
				return false;
			}
		}

		Mock<IServiceHostProviderFactory> hostProviderFactoryMock;

		readonly BusinessObjectFactory _ReferenceFactory;

		public BusinessObjectFactory ReferenceFactoryExposed => ReferenceFactory;

		protected override BusinessObjectFactory ReferenceFactory => _ReferenceFactory ?? base.ReferenceFactory;

		public bool WaitForDispatchingThreadResumeRequest(TimeSpan timeout)
		{
			return base.WaitForDispatchingThreadResumeRequest(timeout, CancellationToken.None);
		}
	}

	public class ImmediateActionQueue : IBackgroundThreadActionQueue
	{
		public void SetMainThreadId() => throw new NotImplementedException();
		public void Dispose() { }
		public void InvokeActions() => throw new NotImplementedException();
		public void InvokeActionsWhileWaiting(TimeSpan upToTimeout, Func<bool> exitCondition) => throw new NotImplementedException();
		public bool WaitForEnqueue(TimeSpan timeout, CancellationToken cancellationToken) => throw new NotImplementedException();
		public void Wake() { }
		public IDisposable CreateTimer(Action timerAction, TimeSpan dueTime, TimeSpan period) => throw new NotImplementedException();

		public void Enqueue(Action action) => action();

		public void Enqueue(TimeSpan delaySpan, Action action)
		{
			if (delaySpan != TimeSpan.Zero)
			{
				throw new NotImplementedException();
			}

			Enqueue(action);
		}
	}
}
