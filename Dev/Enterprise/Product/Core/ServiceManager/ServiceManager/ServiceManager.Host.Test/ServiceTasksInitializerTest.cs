using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class ServiceTasksInitializerTest : TestCaseWithFactory
	{
		public void TestServiceTasksInitializerCtorArgumentNullExceptions()
		{
			var argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					Mock.Of<IInitializationTaskRunner>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					null,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>()));
			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("errorReporterProxy"));

			argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					Mock.Of<IInitializationTaskRunner>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<IErrorReporterProxy>(),
					null,
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>()));
			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("hostRegistry"));

			argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					Mock.Of<IInitializationTaskRunner>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					null,
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>()));

			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("loggerFactory"));

			argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					Mock.Of<IInitializationTaskRunner>(),
					Mock.Of<IHostLogger>(),
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					Mock.Of<IErrorReporterProxy>(),
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					null,
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					Mock.Of<IServiceHostsCache>()));

			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("serviceTaskScheduleManager"));

			argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				_ = new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown()),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					initializationTaskRunner.Object,
					hostLogger.Object,
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					null,
					Mock.Of<IServiceHostsCache>()));

			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("hostedServiceAttributeProvider"));

			argumentNullException = AssertExceptionThrown<ArgumentNullException>(() =>
				_ = new ServiceTasksInitializer(
					Mock.Of<IProductRegistrationPeriodicChecker>(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown()),
					Mock.Of<ITransactionAdapter>(),
					Mock.Of<IServiceTaskCollectionGovernor>(),
					Mock.Of<ITaskScheduler>(),
					Mock.Of<IAllTasksCollection>(),
					initializationTaskRunner.Object,
					hostLogger.Object,
					Mock.Of<IEventLogger>(),
					Mock.Of<ITaskInitializationRequirementsChecker>(),
					Mock.Of<ITaskQueue>(),
					Mock.Of<IBackgroundThreadActionQueue>(),
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					Mock.Of<IServiceTaskScheduleManager>(),
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					Mock.Of<IClientHostedServiceAttributeProvider>(),
					null));

			NUnit.Framework.Assert.That(argumentNullException.ParamName, Is.EqualTo("serviceHostsCache"));
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasksDoesNotEnqueueNudgeableTasksWhenOtherServiceHostsAvailableAtStartup()
		{
			var initializationTaskRunnerMock = new Mock<IInitializationTaskRunner>();
			var hostLoggerMock = new Mock<IHostLogger>();

			// Arrange
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(new[] { Mock.Of<IServiceHostClient>(), Mock.Of<IServiceHostClient>() });
			NUnit.Framework.Assert.That(serviceHostsCacheMock.Object.AvailableServiceHosts.Any(), Is.EqualTo(true));

			// Act
			TestInitializeServiceTasks(() =>
			{
				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					initializationTaskRunnerMock.Verify(x => x.EnqueueNudgeableTasks(It.IsAny<List<IRunnableServiceTask>>(), It.IsAny<IEnumerable<HostedServiceBusinessObjectBindingAttribute>>()), Times.Never);
				});
			}, serviceHostsCacheMock.Object, hostLoggerMock.Object, initializationTaskRunnerMock.Object);
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasksEnqueueNudgeableTasksWhenNoOtherServiceHostsAvailableAtStartup()
		{
			var initializationTaskRunnerMock = new Mock<IInitializationTaskRunner>();
			var hostLoggerMock = new Mock<IHostLogger>();

			// Arrange
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());
			NUnit.Framework.Assert.That(serviceHostsCacheMock.Object.AvailableServiceHosts.Any(), Is.EqualTo(false));

			// Act
			TestInitializeServiceTasks(() =>
			{
				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					initializationTaskRunnerMock.Verify(x => x.EnqueueNudgeableTasks(It.IsAny<IEnumerable<IRunnableServiceTask>>(), It.IsAny<IEnumerable<IHostedServiceBusinessObjectBinding>>()), Times.Once);
				});
			}, serviceHostsCacheMock.Object, hostLoggerMock.Object, initializationTaskRunnerMock.Object);
		}

		void TestInitializeServiceTasks(Action assertions, IServiceHostsCache serviceHostsCache, IHostLogger hostLogger, IInitializationTaskRunner initializationTaskRunner)
		{
			// Arrange
			var businessObjectBindings = HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings;
			var allTasks = new AllTasksCollection();
			var scheduledTasks = new SchedulerServiceTasksLoader(new BusinessObjectFactory()).Load().GovernedTasks;

			var serviceTasksInitializer = new ServiceTasksInitializer(
				Mock.Of<IProductRegistrationPeriodicChecker>(x => x.IsProductRegisteredAsNonTrialSystemOrUnknown()),
				Mock.Of<ITransactionAdapter>(),
				Mock.Of<IServiceTaskCollectionGovernor>(x => x.GovernedTasks == scheduledTasks),
				Mock.Of<ITaskScheduler>(),
				allTasks,
				initializationTaskRunner,
				hostLogger,
				Mock.Of<IEventLogger>(),
				Mock.Of<ITaskInitializationRequirementsChecker>(),
				Mock.Of<ITaskQueue>(),
				Mock.Of<IBackgroundThreadActionQueue>(),
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCache);

			// Act
			serviceTasksInitializer.InitializeServiceTasks();

			// Assert
			assertions.Invoke();
		}

		[TestDate(2016, 05, 24, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_InitializesTaskScheduler()
		{
			// Arrange
			var testStartTime = ZDateTime.UtcNow;
			schedule.S5_ScheduleType = "DSA";
			schedule.S5_NextScheduledPrintRunTimeUtc = testStartTime;
			Factory.Save();

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var schedules = new List<ServiceTaskSchedule>() { schedule };
			var memoryBackupFactory = new MemoryBackupFactory();
			var allTasks = new AllTasksCollection();
			var aspectVersions = new Mock<IDatabaseAspectVersions>().Object;
			using var taskScheduler = new TaskScheduler(transactionAdapter, allTasks, hostLogger.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, aspectVersions, attributeProvider.Object);

			var taskCodeAttr = new XAttribute(TaskScheduler.Constants.TaskCodeAttribute, "DSA");
			var taskNextRunAttr = new XAttribute(TaskScheduler.Constants.TaskNextRunTimeAttribute, testStartTime.AddHours(2).ToString(TaskScheduler.Constants.XMLDateTimeFormat));
			var taskNode = new XElement(TaskScheduler.Constants.TaskNode, taskCodeAttr, taskNextRunAttr);
			var allTasksAttr = new XAttribute("version", TaskScheduler.Constants.GetLatestXMLFileVersion(aspectVersions));
			var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode, allTasksAttr, taskNode);
			memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
			serviceTaskScheduleManagerMock
				.Setup(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Returns(schedules.Select(s => new SchedulerServiceTask(s)));

			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());
			using var immediateActionQueue = new ImmediateActionQueue();

			// Act
			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler,
				allTasks,
				initializationTaskRunner.Object,
				hostLogger.Object,
				eventLogger.Object,
				new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider()), taskQueue.Object, immediateActionQueue,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);
			initializer.InitializeServiceTasks();

			// Assert
			var dsaTask = allTasks.GetAll().FirstOrDefault(t => t.Code == "DSA");
			NUnit.Framework.Assert.That(dsaTask.NextScheduledRunTime, Is.EqualTo(testStartTime.AddHours(2).ToNullableDateTimeOffset()));
		}

		[ExpectNoExceptions]
		[TestDate(2016, 05, 24, 12, 00, 00)]
		public void TestInitializeServiceTasks_InitializesTaskSchedulerRequirementsCheckedAfterScheduleInitialization()
		{
			// Arrange
			var testStartTime = ZDateTime.UtcNow;
			schedule.S5_ScheduleType = "DSA";
			schedule.S5_NextScheduledPrintRunTimeUtc = testStartTime;
			Factory.Save();

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var schedules = new List<ServiceTaskSchedule>() { schedule };
			var memoryBackupFactory = new MemoryBackupFactory();
			var allTasks = new AllTasksCollection();
			var aspectVersions = new Mock<IDatabaseAspectVersions>();
			using var taskScheduler = new TaskScheduler(transactionAdapter, allTasks, hostLogger.Object, null, memoryBackupFactory, errorReporterProxyMock.Object, aspectVersions.Object, attributeProvider.Object);

			var taskCodeAttr = new XAttribute(TaskScheduler.Constants.TaskCodeAttribute, "DSA");
			var taskNextRunAttr = new XAttribute(TaskScheduler.Constants.TaskNextRunTimeAttribute, testStartTime.AddHours(2).ToString(TaskScheduler.Constants.XMLDateTimeFormat));
			var taskNode = new XElement(TaskScheduler.Constants.TaskNode, taskCodeAttr, taskNextRunAttr);
			var allTasksNode = new XElement(TaskScheduler.Constants.AllTasksNode, taskNode);
			memoryBackupFactory.Backup.Document = new XDocument(new XElement(TaskScheduler.Constants.RootNode, allTasksNode));
			var mockSequence = new MockSequence();
			serviceTaskScheduleManagerMock
				.InSequence(mockSequence)
				.Setup(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Returns(schedules.Select(s => new SchedulerServiceTask(s)));
			requirementsChecker
				.InSequence(mockSequence)
				.Setup(c => c.EnsureTaskRequirementsSatisfied(It.IsAny<IEnumerable<ScheduleWithInfo>>(), It.IsAny<IServiceTaskCollectionGovernor>()))
				.Returns((IEnumerable<ScheduleWithInfo> s, IServiceTaskCollectionGovernor c) => s);

			var configs = new List<IHostedServiceAttribute>();
			// Note, "DSA" task is mandatory. There's an exception if it's not added.
			AddTask(configs, "DSA", "DBM", DSAType, DSAAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "ODT", "SYS", ODTType, ODTAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "UPG", "SYS", UPGType, UPGAssemblyName, alwaysRunAtStartup: true);
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();
			configProvider
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);
			configProvider
				.Setup(x => x.GetClientHostedServiceAttributes())
				.Returns(configs);

			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			// Act
			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler,
				Mock.Of<IAllTasksCollection>(),
				initializationTaskRunner.Object,
				hostLogger.Object, eventLogger.Object,
				requirementsChecker.Object,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				configProvider.Object,
				serviceHostsCache: serviceHostsCacheMock.Object);
			initializer.InitializeServiceTasks();

			// Assert
			serviceTaskScheduleManagerMock.Verify(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()), Times.Once);
			requirementsChecker.Verify(c => c.EnsureTaskRequirementsSatisfied(It.IsAny<IEnumerable<ScheduleWithInfo>>(), It.IsAny<IServiceTaskCollectionGovernor>()), Times.Once);
		}

		public void TestInitializeServiceTasks_FailsWithExceptionIfDSAIsNotInAssemblyMetaData()
		{
			// Arrange
			attributeProvider.Setup(c => c.GetClientHostedServiceAttributes()).Returns(Enumerable.Empty<IHostedServiceAttribute>());
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				Mock.Of<IAllTasksCollection>(),
				initializationTaskRunner.Object,
				hostLogger.Object, eventLogger.Object,
				requirementsChecker.Object,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				attributeProvider.Object,
				serviceHostsCache: serviceHostsCacheMock.Object);

			// Act
			AssertExceptionThrown(typeof(InvalidOperationException), () => initializer.InitializeServiceTasks());
		}

		public void TestInitializeServiceTasks_FailsWithExceptionIfDSADidNotSatisfyRequirements()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider());
			var config = new Mock<IHostedServiceAttribute>();
			config.Setup(c => c.Code).Returns("DSA");
			config.Setup(c => c.TypeName).Returns("Bla");
			var configs = new []
			{
				config.Object,
			};
			attributeProvider.Setup(c => c.GetClientHostedServiceAttributes()).Returns(configs);
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				Mock.Of<IAllTasksCollection>(),
				initializationTaskRunner.Object,
				hostLogger.Object,
				eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				attributeProvider.Object,
				serviceHostsCache: serviceHostsCacheMock.Object);

			// Act
			AssertExceptionThrown(typeof(InvalidOperationException), () => initializer.InitializeServiceTasks());
		}

		public void TestInitializeServiceTasks_FailsWithExceptionIfNoActiveCompanies()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var companyProvider = new Mock<IGlbCompanyProvider>();
			companyProvider.Setup(p => p.GetActiveCompanies()).Returns(Array.Empty<IGlbCompany>());
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, companyProvider.Object);
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				Mock.Of<IAllTasksCollection>(),
				initializationTaskRunner.Object,
				hostLogger.Object, eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);

			// Act
			AssertExceptionThrown(typeof(ProcessControllerConfigurationException), "Unable to proceed with execution as there are no active companies that contain an active branch.", () => initializer.InitializeServiceTasks());
		}

		public void TestInitializeServiceTasks_FailsWithExceptionIfNoActiveBranches()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var companyProvider = new Mock<IGlbCompanyProvider>();
			var company = new Mock<IGlbCompany>();
			company.Setup(c => c.GetActiveBranches()).Returns(Array.Empty<IGlbBranch>());
			companyProvider.Setup(p => p.GetActiveCompanies()).Returns(new IGlbCompany[1] { company.Object });
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, companyProvider.Object);
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				Mock.Of<IAllTasksCollection>(),
				initializationTaskRunner.Object,
				hostLogger.Object,
				eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);

			// Act
			AssertExceptionThrown(typeof(ProcessControllerConfigurationException), "Unable to proceed with execution as there are no active companies that contain an active branch.", () => initializer.InitializeServiceTasks());
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_Initializes_ButDoesNotRunNudgeableTasks_IfProductIsUnregistered()
		{
			// Arrange
			var allTasks = new AllTasksCollection();
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider());
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				allTasks,
				initializationTaskRunner.Object,
				hostLogger.Object,
				eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);

			regChecker.SetupSequence(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(false);
			serviceTaskScheduleManagerMock
				.SetupSequence(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Returns(allTasks.GetAll().ToArray().Select(t => Mock.Of<IServiceTask>(s => s.Code == t.Code)));

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			regChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.Once);
			serviceTaskScheduleManagerMock.Verify(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()));
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_DoesNotRunNudgeableTasks_IfAnotherProcessControllerIsRunning()
		{
			// Arrange
			var allTasks = new AllTasksCollection();
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider());

			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(new[] { Mock.Of<IServiceHostClient>(), Mock.Of<IServiceHostClient>() });

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				allTasks,
				initializationTaskRunner.Object,
				hostLogger.Object, eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);

			regChecker.SetupSequence(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			serviceTaskScheduleManagerMock
				.SetupSequence(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Returns(allTasks.GetAll().ToArray().Select(t => Mock.Of<IServiceTask>(s => s.Code == t.Code)));

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			regChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.Once);
			serviceTaskScheduleManagerMock.Verify(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()));
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_RunsNudgeableTasksOnce()
		{
			// Arrange
			var allTasks = new AllTasksCollection();
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider());
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				allTasks,
				initializationTaskRunner.Object,
				hostLogger.Object,
				eventLogger.Object,
				requirementsChecker,
				taskQueue.Object,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
				serviceHostsCacheMock.Object);

			regChecker.SetupSequence(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
			initializationTaskRunner.SetupSequence(itr => itr.EnqueueNudgeableTasks(It.IsAny<ICollection<IRunnableServiceTask>>(), It.IsAny<IEnumerable<HostedServiceBusinessObjectBindingAttribute>>()));
			serviceTaskScheduleManagerMock
				.SetupSequence(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Returns(allTasks.GetAll().ToArray().Select(t => Mock.Of<IServiceTask>(s => s.Code == t.Code)));

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			regChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_WithProductivityWiseModeEnabled_ShouldExcludeTasksForCategoriesNotIncludedInProductivityWise()
		{
			ObjectFactory.Get<IBMTestHelper>().EnableBMSInRegistry();
			ServiceTasksInitializer initializer;
			Mock<IProductRegistrationPeriodicChecker> regChecker;
			AllTasksCollection allTasks;
			Mock<IHostRegistrySettings> hostRegistryMock;

			void Setup()
			{
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter(new Lazy<BusinessObjectFactory>(() => Factory));
				var taskScheduler = new Mock<ITaskScheduler>();
				regChecker = new Mock<IProductRegistrationPeriodicChecker>();
				var initializationTaskRunner = new Mock<IInitializationTaskRunner>();
				var logger = new Mock<IHostLogger>();
				var eventLogger = new Mock<IEventLogger>();
				var actionQueue = new Mock<IBackgroundThreadActionQueue>();
				var taskQueue = new Mock<ITaskQueue>();
				var requirementsChecker = new TaskInitializationRequirementsChecker(logger.Object, new GlbCompanyProvider());
				allTasks = new AllTasksCollection();
				hostRegistryMock = new Mock<IHostRegistrySettings>();

				serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				initializer = new ServiceTasksInitializer(
					regChecker.Object,
					transactionAdapter,
					transactionAdapter.GetCollectionGovernorForAllTasks(),
					taskScheduler.Object,
					allTasks,
					initializationTaskRunner.Object,
					logger.Object,
					eventLogger.Object,
					requirementsChecker,
					taskQueue.Object,
					actionQueue.Object,
					errorReporterProxyMock.Object,
					hostRegistryMock.Object,
					Mock.Of<ILoggerFactory>(),
					serviceTaskScheduleManagerMock.Object,
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
					serviceHostsCacheMock.Object);

				regChecker.SetupSequence(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);
				initializationTaskRunner.SetupSequence(itr => itr.EnqueueNudgeableTasks(It.IsAny<ICollection<IRunnableServiceTask>>(), It.IsAny<IEnumerable<HostedServiceBusinessObjectBindingAttribute>>()));
				serviceTaskScheduleManagerMock
					.SetupSequence(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
					.Returns(allTasks.GetAll().ToArray().Select(t => Mock.Of<IServiceTask>(s => s.Code == t.Code)));
			}

			Setup();
			initializer.InitializeServiceTasks();
			regChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.Once);

			var includedCodes = allTasks.GetAll().Select(x => x.Code).ToArray();
			NUnit.Framework.Assert.That(includedCodes, Has.Some.EqualTo("CCV"));
			NUnit.Framework.Assert.That(includedCodes, Has.Some.EqualTo("TAG"));

			Setup();
			hostRegistryMock.SetupGet(o => o.ProductivityWiseModeEnabled).Returns(true);
			initializer.InitializeServiceTasks();
			regChecker.Verify(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown(), Times.Once);

			includedCodes = allTasks.GetAll().Select(x => x.Code).ToArray();
			NUnit.Framework.Assert.That(includedCodes, Has.None.EqualTo("CCV"), "ProductivityWise mode should hide many service tasks, like this one. SAD!");
			NUnit.Framework.Assert.That(includedCodes, Has.Some.EqualTo("TAG"));
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_MultipleHostsAtTheSameTime()
		{
			var allTasks1 = new AllTasksCollection();
			var allTasks2 = new AllTasksCollection();
			var serviceTaskScheduleManagerMock1 = new Mock<IServiceTaskScheduleManager>();
			var serviceTaskScheduleManagerMock2 = new Mock<IServiceTaskScheduleManager>();
			var initTaskRunner1 = new Mock<IInitializationTaskRunner>();
			var initTaskRunner2 = new Mock<IInitializationTaskRunner>();
			var taskRequirementsChecker2 = new Mock<ITaskInitializationRequirementsChecker>();
			var timeout = TimeSpan.FromSeconds(300);
			taskRequirementsChecker2
				.Setup(c => c.EnsureTaskRequirementsSatisfied(It.IsAny<IEnumerable<ScheduleWithInfo>>(), It.IsAny<IServiceTaskCollectionGovernor>()))
				.Returns((IEnumerable<ScheduleWithInfo> s, IServiceTaskCollectionGovernor c) => s);

			using (var taskRequirementsChecker1 = new ManualRequirementsChecker(timeout))
			using (var secondInitcompletedEvent = new AutoResetEvent(false))
			{
				using var transactionAdapter2 = new NativeServiceTaskTransactionAdapter();
				ServiceTasksInitializer initializer1 = null;
				var serviceHostsCacheMock2 = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock2.Setup(x => x.AvailableServiceHosts).Returns(new[] { Mock.Of<IServiceHostClient>(), Mock.Of<IServiceHostClient>() });

				var initializer2 = new ServiceTasksInitializer(
					regChecker.Object,
					transactionAdapter2,
					transactionAdapter2.GetCollectionGovernorForAllTasks(),
					taskScheduler.Object,
					allTasks2,
					initTaskRunner2.Object,
					hostLogger.Object,
					eventLogger.Object,
					taskRequirementsChecker2.Object,
					taskQueue.Object,
					actionQueue.Object,
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					serviceTaskScheduleManagerMock2.Object,
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
					serviceHostsCacheMock2.Object);

				var initTask = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						using var transactionAdapter1 = new NativeServiceTaskTransactionAdapter();
						serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

						initializer1 = new ServiceTasksInitializer(
							regChecker.Object,
							transactionAdapter1,
							transactionAdapter1.GetCollectionGovernorForAllTasks(),
							taskScheduler.Object,
							allTasks1,
							initTaskRunner1.Object,
							hostLogger.Object,
							eventLogger.Object,
							taskRequirementsChecker1,
							taskQueue.Object,
							actionQueue.Object,
							errorReporterProxyMock.Object,
							Mock.Of<IHostRegistrySettings>(),
							Mock.Of<ILoggerFactory>(),
							serviceTaskScheduleManagerMock1.Object,
							Mock.Of<IServiceTaskScheduleStatusProvider>(),
							ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
							serviceHostsCacheMock.Object);

						initializer1.InitializeServiceTasks();
					}
				});

				NUnit.Framework.Assert.That(taskRequirementsChecker1.WaitUntilCheckStarted(), Is.True);
				var completeCheckTask = Task.Run(() =>
				{
					NUnit.Framework.Assert.That(!secondInitcompletedEvent.WaitOne(TimeSpan.FromSeconds(1)), Is.True);
					taskRequirementsChecker1.CompleteCheck();
				});
				initializer2.InitializeServiceTasks();
				taskRequirementsChecker2.Verify(trc => trc.EnsureTaskRequirementsSatisfied(It.IsAny<IEnumerable<ScheduleWithInfo>>(), It.IsAny<IServiceTaskCollectionGovernor>()), Times.Once);
				serviceTaskScheduleManagerMock2.Verify(m => m.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()), Times.Never);
				secondInitcompletedEvent.Set();
				NUnit.Framework.Assert.That(initTask.Wait(timeout), Is.True);
				NUnit.Framework.Assert.That(completeCheckTask.Wait(timeout), Is.True);
				serviceTaskScheduleManagerMock1.Verify(s => s.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()), Times.Once);
				serviceTaskScheduleManagerMock2.VerifyNoOtherCalls();
				allTasks1.GetAll().ForEach(t => t.ReloadConfigurationAsync());
				NUnit.Framework.Assert.That(allTasks1.Count, Is.GreaterThan(0));
				NUnit.Framework.Assert.That(allTasks2.Count, Is.EqualTo(allTasks1.Count));
				NUnit.Framework.Assert.That(allTasks2.GetAll().Count(t => t.IsActive), Is.EqualTo(allTasks1.GetAll().Count(t => t.IsActive)));
			}
		}

		const string DSAType = "Enterprise.ServiceManager.Tasks.DbSecurityAdmin.DbSecurityAdminTask";
		const string DSAAssemblyName = "Enterprise.ServiceManager.Tasks.DbSecurityAdmin";
		const string ODTType = "Enterprise.ServiceManager.Tasks.OnlineDataTransformation.OnlineDataTransformationTask";
		const string ODTAssemblyName = "Enterprise.ServiceManager.Tasks.OnlineDataTransformation";
		const string UPGType = "Enterprise.ServiceManager.Tasks.ScheduledUpgrader.UpgraderServiceTask";
		const string UPGAssemblyName = "Enterprise.ServiceManager.Tasks.ScheduledUpgrader";

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_MultipleHostsAtTheSameTime_SecondHasTheSameAmountOfActiveTasks()
		{
			// Arrange
			var allTasks1 = new AllTasksCollection();
			var allTasks2 = new AllTasksCollection();
			var serviceTaskScheduleManagerMock1 = new Mock<IServiceTaskScheduleManager>();
			var serviceTaskScheduleManagerMock2 = new Mock<IServiceTaskScheduleManager>();
			var initTaskRunner1 = new Mock<IInitializationTaskRunner>();
			var initTaskRunner2 = new Mock<IInitializationTaskRunner>();
			var timeout = TimeSpan.FromSeconds(30);

			var configs = new List<IHostedServiceAttribute>();
			AddTask(configs, "DSA", "DBM", DSAType, DSAAssemblyName, alwaysRunAtStartup: true, true, true);
			AddTask(configs, "ODT", "SYS", ODTType, ODTAssemblyName, alwaysRunAtStartup: true, true, true);
			Factory.Save();

			var configProvider1 = new Mock<IClientHostedServiceAttributeProvider>();
			configProvider1.Setup(c => c.GetClientHostedServiceAttributes())
				.Returns(configs);
			configProvider1
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);
			var configProvider2 = new Mock<IClientHostedServiceAttributeProvider>();
			configProvider2.Setup(c => c.GetClientHostedServiceAttributes())
				.Returns(configs);
			configProvider2
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);

			using (var taskRequirementsChecker = new ManualRequirementsChecker(timeout, tuples =>
			{
				return tuples
					.Where(tuple => tuple.Info.Code == "DSA")
					.ToList();
			}))
			using (var secondInitcompletedEvent = new AutoResetEvent(false))
			{
				// Act
				ServiceTasksInitializer initializer1 = null;
				using var transactionAdapter2 = new SchedulerServiceTaskTransactionAdapter();
				var serviceHostsCacheMock2 = new Mock<IServiceHostsCache>();
				serviceHostsCacheMock2.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var initializer2 = new ServiceTasksInitializer(
					regChecker.Object,
					transactionAdapter2,
					transactionAdapter2.GetCollectionGovernorForAllTasks(),
					taskScheduler.Object,
					allTasks2,
					initTaskRunner2.Object,
					hostLogger.Object,
					eventLogger.Object,
					taskRequirementsChecker,
					taskQueue.Object,
					actionQueue.Object,
					errorReporterProxyMock.Object,
					Mock.Of<IHostRegistrySettings>(),
					Mock.Of<ILoggerFactory>(),
					serviceTaskScheduleManagerMock2.Object,
					Mock.Of<IServiceTaskScheduleStatusProvider>(),
					configProvider2.Object,
					serviceHostsCache: serviceHostsCacheMock2.Object);

				var initTask = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						using var transactionAdapter1 = new SchedulerServiceTaskTransactionAdapter();
						serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

						initializer1 = new ServiceTasksInitializer(
							regChecker.Object,
							transactionAdapter1,
							transactionAdapter1.GetCollectionGovernorForAllTasks(),
							taskScheduler.Object,
							allTasks1,
							initTaskRunner1.Object,
							hostLogger.Object,
							eventLogger.Object,
							taskRequirementsChecker,
							taskQueue.Object,
							actionQueue.Object,
							errorReporterProxyMock.Object,
							Mock.Of<IHostRegistrySettings>(),
							Mock.Of<ILoggerFactory>(),
							serviceTaskScheduleManagerMock1.Object,
							Mock.Of<IServiceTaskScheduleStatusProvider>(),
							configProvider1.Object,
							serviceHostsCache: serviceHostsCacheMock.Object);

						initializer1.InitializeServiceTasks();
					}
				});

				NUnit.Framework.Assert.That(taskRequirementsChecker.WaitUntilCheckStarted(), Is.True);
				var completeCheckTask = Task.Run(() =>
				{
					NUnit.Framework.Assert.That(!secondInitcompletedEvent.WaitOne(TimeSpan.FromSeconds(1)), Is.True);
					taskRequirementsChecker.CompleteCheck();
				});
				initializer2.InitializeServiceTasks();
				secondInitcompletedEvent.Set();

				// Assert
				NUnit.Framework.Assert.That(initTask.Wait(timeout), Is.True);
				NUnit.Framework.Assert.That(completeCheckTask.Wait(timeout), Is.True);
				NUnit.Framework.Assert.That(allTasks1.Count, Is.GreaterThan(0));
				NUnit.Framework.Assert.That(allTasks2.GetAll().Count(t => t.IsActive), Is.EqualTo(allTasks1.Count));
				serviceTaskScheduleManagerMock1.Verify(s => s.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()), Times.Once);
				serviceTaskScheduleManagerMock2.VerifyNoOtherCalls();
			}
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_EnqueuesAllAlwaysRunAtStartupTasks()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var taskQueue = new TaskQueue(runnerPool.Object, hostLogger.Object);
			var initializationTaskRunnerMock = new Mock<IInitializationTaskRunner>();
			var requirementsChecker = new TaskInitializationRequirementsChecker(hostLogger.Object, new GlbCompanyProvider());
			var configs = new List<IHostedServiceAttribute>();
			AddTask(configs, "DSA", "DBM", DSAType, DSAAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "ODT", "SYS", ODTType, ODTAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "UPG", "SYS", UPGType, UPGAssemblyName, alwaysRunAtStartup: false);
			Factory.Save();

			attributeProvider
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);

			attributeProvider.Setup(c => c.GetClientHostedServiceAttributes()).Returns(configs);
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler.Object,
				new AllTasksCollection(),
				initializationTaskRunnerMock.Object,
				hostLogger.Object,
				eventLogger.Object,
				requirementsChecker,
				taskQueue,
				actionQueue.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				attributeProvider.Object,
				serviceHostsCache: serviceHostsCacheMock.Object);

			regChecker.SetupSequence(rc => rc.IsProductRegisteredAsNonTrialSystemOrUnknown()).Returns(true);

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			initializationTaskRunnerMock.Verify(itr => itr.EnqueueTasksThatAlwaysRunOnStartup(It.IsAny<IEnumerable<IRunnableServiceTask>>()));
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_RunLockedRetry_AllTasksHasNoDuplicates()
		{
			// Arrange
			var settings = new HostedServiceAttribute { CanRunInAnyBranch = true };
			var task1 = schedule;
			task1.S5_ScheduleType = "TT1";
			task1.SetStaticServiceAttributesDebugOnly(settings);

			var task2 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			task2.S5_ScheduleType = "TT2";
			task2.SetStaticServiceAttributesDebugOnly(settings);
			Factory.Save();

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var collectionGovernor = transactionAdapter.GetCollectionGovernorForAllTasks();
			NUnit.Framework.Assert.That(collectionGovernor.GovernedTasks.Count(), Is.EqualTo(2), "PRE");

			var allTasks = new AllTasksCollection();
			var regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			var scheduler = new Mock<ITaskScheduler>();
			var logger = new Mock<IHostLogger>();
			var eventLogger = new Mock<IEventLogger>();
			var actions = new Mock<IBackgroundThreadActionQueue>();
			var hostsCache = new Mock<IServiceHostsCache>();
			var taskQueue = new Mock<ITaskQueue>();

			var requirementsChecker = new Mock<ITaskInitializationRequirementsChecker>();
			requirementsChecker
				.Setup(x => x.EnsureTaskRequirementsSatisfied(It.IsAny<IEnumerable<ScheduleWithInfo>>(), It.IsAny<IServiceTaskCollectionGovernor>()))
				.Returns((IEnumerable<ScheduleWithInfo> s, IServiceTaskCollectionGovernor g) => s);

			var configs = new List<IHostedServiceAttribute>();
			// Note, "DSA" task is mandatory. There's an exception if it's not added.
			AddTask(configs, "DSA", "DBM", DSAType, DSAAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "ODT", "SYS", ODTType, ODTAssemblyName, alwaysRunAtStartup: true);
			AddTask(configs, "UPG", "SYS", UPGType, UPGAssemblyName, alwaysRunAtStartup: true);
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();
			configProvider
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);
			configProvider
				.Setup(x => x.GetClientHostedServiceAttributes())
				.Returns(configs);

			var initTaskRunner = new Mock<IInitializationTaskRunner>();
			var tryCount = 0;
			serviceTaskScheduleManagerMock.Setup(x => x.ConfigureSchedules(It.IsAny<IEnumerable<IHostedServiceAttribute>>()))
				.Callback(() =>
				{
					tryCount++;
					if (tryCount == 1)
					{
						throw new SqlLockLostException();
					}
				});

			var initializer = new ServiceTasksInitializer(
				regChecker.Object,
				transactionAdapter,
				collectionGovernor,
				scheduler.Object,
				allTasks,
				initTaskRunner.Object,
				logger.Object,
				eventLogger.Object,
				requirementsChecker.Object,
				taskQueue.Object,
				actions.Object,
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				configProvider.Object,
				serviceHostsCache: hostsCache.Object);

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			NUnit.Framework.Assert.That(string.Join(", ", allTasks.GetAll().Select(x => x.Code).OrderBy(code => code)), Is.EqualTo("DSA, ODT, UPG"), "3 unique tasks");
		}

		[ExpectNoExceptions]
		public void TestLogMessageForInitialization()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var logger = new Mock<IHostLogger>();
			var configs = new List<IHostedServiceAttribute>();
			AddTask(configs, "DSA", "DBM", DSAType, DSAAssemblyName, alwaysRunAtStartup: true);
			var configProvider = new Mock<IClientHostedServiceAttributeProvider>();
			configProvider
				.Setup(c => c.GetClientHostedServiceAttribute("DSA"))
				.Returns(configs[0]);
			configProvider
				.Setup(x => x.GetClientHostedServiceAttributes())
				.Returns(configs);
			serviceHostsCacheMock.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			var initializer = new ServiceTasksInitializer(
				Mock.Of<IProductRegistrationPeriodicChecker>(),
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				Mock.Of<ITaskScheduler>(),
				new AllTasksCollection(),
				Mock.Of<IInitializationTaskRunner>(),
				logger.Object,
				Mock.Of<IEventLogger>(),
				Mock.Of<ITaskInitializationRequirementsChecker>(),
				Mock.Of<ITaskQueue>(),
				Mock.Of<IBackgroundThreadActionQueue>(),
				errorReporterProxyMock.Object,
				Mock.Of<IHostRegistrySettings>(),
				Mock.Of<ILoggerFactory>(),
				serviceTaskScheduleManagerMock.Object,
				Mock.Of<IServiceTaskScheduleStatusProvider>(),
				configProvider.Object,
				serviceHostsCache: serviceHostsCacheMock.Object);

			// Act
			initializer.InitializeServiceTasks();

			// Assert
			logger.Verify(x => x.LogSection(LogLevel.Information, "Initializing service tasks.", "Service tasks are initialized."), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestInitializeServiceTasks_InitializationLockTimeout()
		{
			// Arrange
			using (var anotherDbConnection = Db.NewExtraConnectionToMainDb())
			{
				using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();

				NUnit.Framework.Assert.That(anotherDbConnection.TryGetLock("CrossHostSvcTaskSchedulesInit", out SqlApplicationLock mutex), Is.True);
				using (mutex)
				{
					var initializer = new ServiceTasksInitializer(
						regChecker.Object,
						transactionAdapter,
						transactionAdapter.GetCollectionGovernorForAllTasks(),
						taskScheduler.Object,
						new AllTasksCollection(),
						initializationTaskRunner.Object,
						hostLogger.Object,
						eventLogger.Object,
						requirementsChecker.Object,
						taskQueue.Object,
						actionQueue.Object,
						errorReporterProxyMock.Object,
						Mock.Of<IHostRegistrySettings>(),
						Mock.Of<ILoggerFactory>(),
						serviceTaskScheduleManagerMock.Object,
						Mock.Of<IServiceTaskScheduleStatusProvider>(),
						ObjectFactory.Get<IClientHostedServiceAttributeProvider>(),
						serviceHostsCacheMock.Object);

					initializer.InitializationLockTimeout = TimeSpan.Zero;

					// Act
					Exception actualException = null;
					try
					{
						initializer.InitializeServiceTasks();
					}
					catch (Exception ex)
					{
						actualException = ex;
					}

					// Assert
					NUnit.Framework.Assert.That(actualException, Is.TypeOf<InitializationLockTimeoutException>());
					NUnit.Framework.Assert.That(actualException.Message, Is.EqualTo("Unable to acquire Cross Host Service Task Schedules initialization lock after 00:00:00. Exiting to retry later."));
				}
			}
		}

		void AddTask(List<IHostedServiceAttribute> attributes, string code, string category, string typeName, string assemblyName, bool alwaysRunAtStartup, bool isActive = true, bool isMandatory = false)
		{
			var config = new Mock<IHostedServiceAttribute>();
			config.SetupGet(c => c.Code).Returns(code);
			config.SetupGet(c => c.Category).Returns(category);
			config.SetupGet(c => c.AlwaysRunAtStartup).Returns(alwaysRunAtStartup);
			config.SetupGet(c => c.IsMandatory).Returns(isMandatory);
			config.SetupGet(c => c.TypeName).Returns(typeName);
			config.SetupGet(c => c.TypeAssemblyName).Returns(assemblyName);
			TaskSchedulerTest.CreateSchedule(code, Factory, isActive, config: config);
			attributes.Add(config.Object);
		}

		protected override void SetUp()
		{
			base.SetUp();
			regChecker = new Mock<IProductRegistrationPeriodicChecker>();
			taskScheduler = new Mock<ITaskScheduler>();
			initializationTaskRunner = new Mock<IInitializationTaskRunner>();
			hostLogger = new Mock<IHostLogger>();
			eventLogger = new Mock<IEventLogger>();
			schedule = Factory.New<ServiceTaskSchedule>();
			requirementsChecker = new Mock<ITaskInitializationRequirementsChecker>();
			attributeProvider = new Mock<IClientHostedServiceAttributeProvider>();
			actionQueue = new Mock<IBackgroundThreadActionQueue>();
			serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			taskQueue = new Mock<ITaskQueue>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			serviceTaskScheduleManagerMock = new Mock<IServiceTaskScheduleManager>();
		}

		Mock<IProductRegistrationPeriodicChecker> regChecker;
		Mock<IInitializationTaskRunner> initializationTaskRunner;
		Mock<IHostLogger> hostLogger;
		Mock<IEventLogger> eventLogger;
		ServiceTaskSchedule schedule;
		Mock<ITaskScheduler> taskScheduler;
		Mock<ITaskInitializationRequirementsChecker> requirementsChecker;
		Mock<IClientHostedServiceAttributeProvider> attributeProvider;
		Mock<IBackgroundThreadActionQueue> actionQueue;
		Mock<IServiceHostsCache> serviceHostsCacheMock;
		Mock<ITaskQueue> taskQueue;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IServiceTaskScheduleManager> serviceTaskScheduleManagerMock;
	}

	class ManualRequirementsChecker : ITaskInitializationRequirementsChecker, IDisposable
	{
		readonly AutoResetEvent checkStarted = new AutoResetEvent(false);
		readonly AutoResetEvent checkCompleted = new AutoResetEvent(false);
		readonly TimeSpan timeout;
		readonly Func<IEnumerable<ScheduleWithInfo>, IEnumerable<ScheduleWithInfo>> satisfyFilter;

		public ManualRequirementsChecker(TimeSpan timeout, Func<IEnumerable<ScheduleWithInfo>, IEnumerable<ScheduleWithInfo>> satisfyFilter = null)
		{
			this.timeout = timeout;
			this.satisfyFilter = satisfyFilter;
		}

		public void Dispose()
		{
			checkStarted.Dispose();
			checkCompleted.Dispose();
		}

		public IEnumerable<ScheduleWithInfo> EnsureTaskRequirementsSatisfied(IEnumerable<ScheduleWithInfo> allTasks, IServiceTaskCollectionGovernor governor)
		{
			checkStarted.Set();
			checkCompleted.WaitOne(timeout);
			return satisfyFilter == null
				? allTasks
				: satisfyFilter(allTasks);
		}

		public bool WaitUntilCheckStarted() => checkStarted.WaitOne(timeout);
		public void CompleteCheck() => checkCompleted.Set();
	}
}
