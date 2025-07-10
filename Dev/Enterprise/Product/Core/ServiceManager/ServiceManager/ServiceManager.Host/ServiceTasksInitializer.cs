using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ServiceManager.Business;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Host
{
	class ServiceTasksInitializerFactory : IServiceTasksInitializerFactory
	{
		public ServiceTasksInitializerFactory(ILoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1082:DoNotUseTooManyArguments")]
		public IServiceTasksInitializer Create(
			IProductRegistrationPeriodicChecker regChecker,
			ITransactionAdapter transactionAdapter,
			IServiceTaskCollectionGovernor taskCollectionGovernor,
			ITaskScheduler taskScheduler,
			IAllTasksCollection allTasks,
			IInitializationTaskRunner taskInitializationRunner,
			IHostLogger hostLogger,
			IEventLogger eventLogger,
			ITaskInitializationRequirementsChecker taskInitializationRequirements,
			ITaskQueue taskQueue,
			IBackgroundThreadActionQueue actionQueue,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			IServiceTaskScheduleManager serviceTaskScheduleManager,
			IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IServiceHostsCache serviceHostsCache)
		{
			return new ServiceTasksInitializer(
				regChecker,
				transactionAdapter,
				taskCollectionGovernor,
				taskScheduler,
				allTasks,
				taskInitializationRunner,
				hostLogger,
				eventLogger,
				taskInitializationRequirements,
				taskQueue,
				actionQueue,
				errorReporterProxy,
				hostRegistry,
				loggerFactory,
				serviceTaskScheduleManager,
				serviceTaskScheduleStatusProvider,
				hostedServiceAttributeProvider,
				serviceHostsCache);
		}

		readonly ILoggerFactory loggerFactory;
	}

	class ServiceTasksInitializer : IServiceTasksInitializer
	{
		readonly ITransactionAdapter transactionAdapter;
		readonly IServiceTaskCollectionGovernor taskCollectionGovernor;
		readonly IAllTasksCollection allTasks;
		readonly IProductRegistrationPeriodicChecker regChecker;
		readonly ITaskScheduler taskScheduler;
		readonly IInitializationTaskRunner taskInitializationRunner;
		readonly IHostLogger hostLogger;
		readonly IEventLogger eventLogger;
		readonly ITaskInitializationRequirementsChecker taskInitializationRequirements;
		readonly ITaskQueue taskQueue;
		readonly IBackgroundThreadActionQueue actionQueue;
		readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;
		readonly IServiceHostsCache serviceHostsCache;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IHostRegistrySettings hostRegistry;
		readonly IServiceTaskScheduleManager serviceTaskScheduleManager;
		readonly ILoggerFactory loggerFactory;
		readonly IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider;

		const string initializationLockKey = "CrossHostSvcTaskSchedulesInit";

		public ServiceTasksInitializer(
			IProductRegistrationPeriodicChecker regChecker,
			ITransactionAdapter transactionAdapter,
			IServiceTaskCollectionGovernor taskCollectionGovernor,
			ITaskScheduler taskScheduler,
			IAllTasksCollection allTasks,
			IInitializationTaskRunner taskInitializationRunner,
			IHostLogger hostLogger,
			IEventLogger eventLogger,
			ITaskInitializationRequirementsChecker taskInitializationRequirements,
			ITaskQueue taskQueue,
			IBackgroundThreadActionQueue actionQueue,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			ILoggerFactory loggerFactory,
			IServiceTaskScheduleManager serviceTaskScheduleManager,
			IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IServiceHostsCache serviceHostsCache)
		{
			this.regChecker = regChecker;
			this.transactionAdapter = transactionAdapter;
			this.taskCollectionGovernor = taskCollectionGovernor;
			this.taskScheduler = taskScheduler;
			this.allTasks = allTasks;
			this.taskInitializationRunner = taskInitializationRunner;
			this.hostLogger = hostLogger;
			this.eventLogger = eventLogger;
			this.taskInitializationRequirements = taskInitializationRequirements;
			this.taskQueue = taskQueue;
			this.actionQueue = actionQueue;
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
			this.serviceHostsCache = serviceHostsCache ?? throw new ArgumentNullException(nameof(serviceHostsCache));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.serviceTaskScheduleManager = serviceTaskScheduleManager ?? throw new ArgumentNullException(nameof(serviceTaskScheduleManager));
			this.serviceTaskScheduleStatusProvider = serviceTaskScheduleStatusProvider ?? throw new ArgumentNullException(nameof(serviceTaskScheduleStatusProvider));
		}

		internal TimeSpan InitializationLockTimeout { get; set; } = TimeSpan.FromMinutes(5);

		public bool InitializeServiceTasks()
		{
			using (hostLogger.LogSection(LogLevel.Information,
				"Initializing service tasks.",
				"Service tasks are initialized."))
			{
				if (!Enterprise.ZArchitecture.AssemblyMetaDataReader.FilesExist)
				{
					var missingAssemblyMetaData = string.Format(CultureInfo.InvariantCulture, "{0} are missing, likely caused by an error at installation. Please contact your System Administrator.", nameof(Enterprise.ZArchitecture.AssemblyMetaDataReader.AssemblyMetaDataFiles));
					eventLogger.Log(LogLevel.Error, missingAssemblyMetaData);
					return false;
				}

				// Run tasks that need initialization (have no schedule) or have Business Object Binding attributes first.
				// Tasks will have no schedule if a software upgrade has added a new task.

				var result = Db.Connection.RunLocked(initializationLockKey, (isFirstAttempt) =>
				{
					hostLogger.Log(LogLevel.Information, "Initializing service task schedules");
					var serviceTasksFromMetadata = GetServiceTasksFromMetadata();
					var taskAttributes = ResolveServiceTasksForInitialization(serviceTasksFromMetadata).Select(t => t.Info.HostedServiceAttribute).ToArray();
					var serviceTasks = serviceTaskScheduleManager.ConfigureSchedules(taskAttributes);
					ConfigureServiceTasks(serviceTasksFromMetadata, serviceTasks);
				});

				switch (result)
				{
					case LockedProcessResult.AlreadyBeingProcessed:
						hostLogger.Log(LogLevel.Debug, "Waiting up to 5 minutes for another host to initialize the service task schedules");

						if (!Db.Connection.TryGetLock(initializationLockKey, InitializationLockTimeout, out var sqlAppLock))
						{
							throw new InitializationLockTimeoutException(InitializationLockTimeout);
						}
						sqlAppLock.Dispose();

						taskCollectionGovernor.Reload();
						ResolveServiceTasks();
						UpdateTaskSchedules(reEnableMandatory: false);
						break;
					case LockedProcessResult.Completed:
						break;
					case LockedProcessResult.Error:
						throw new InitializationLockTimeoutException();
					default:
						throw new InitializationLockTimeoutException(result);
				}

				taskScheduler.InitialiseTasks();

				hostLogger.Log(LogLevel.Debug, string.Format(CultureInfo.InvariantCulture, "Dispatcher initializaton - formed list of all runnable tasks: {0}", string.Join(",", allTasks.GetAll().Select(task => task.Code))));

				if (!regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown())
				{
					hostLogger.Log(LogLevel.Debug, string.Format(CultureInfo.InvariantCulture, "{0}@{1} system is unregistered. Nudgeable tasks were not run.", Db.DatabaseName, Db.ServerName));
					return true;
				}

				taskInitializationRunner.EnqueueTasksThatAlwaysRunOnStartup(allTasks.GetAll());

				var anyServiceHostsAvailableAtStartup = serviceHostsCache.AvailableServiceHosts.Any();
				if (anyServiceHostsAvailableAtStartup || result == LockedProcessResult.AlreadyBeingProcessed)
				{
					hostLogger.Log(LogLevel.Debug, "Nudgeable tasks were not run as there was already another process controller running.");
				}
				else
				{
					taskInitializationRunner.EnqueueNudgeableTasks(allTasks.GetAll(), HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings);
				}

				return true;
			}
		}

		public IDSATaskRunner CreateDSARunner()
		{
			if (!allTasks.TryGetByCode("DSA", out var dsaTask))
			{
				throw new InvalidOperationException("Unable to proceed with execution as the mandatory 'Database Security Admin Task' (DSA) is missing after initializer returned success");
			}

			return new DSATaskRunner(dsaTask, taskQueue, hostLogger);
		}

		static IEnumerable<ScheduleWithInfo> ResolveServiceTasksForInitialization(IEnumerable<IHostedServiceAttribute> serviceTasksFromMetadata)
		{
			return serviceTasksFromMetadata
				.Select(attribute =>
					new ScheduleWithInfo(
						new ServiceTaskInfo(attribute),
						null));
		}

		void ConfigureServiceTasks(IEnumerable<IHostedServiceAttribute> serviceTasksFromMetadata, IEnumerable<IServiceTask> serviceTasks)
		{
			var codeToServiceTask = serviceTasks.ToDictionary(t => t.Code, t => t);
			var configuredTasks = serviceTasksFromMetadata
				.Select(attribute =>
					new ScheduleWithInfo(
						new ServiceTaskInfo(attribute),
						codeToServiceTask.TryGetValue(attribute.Code, out var task) ? task : null));

			var tasksThatSatisfyRequirements = taskInitializationRequirements.EnsureTaskRequirementsSatisfied(configuredTasks, taskCollectionGovernor);
			PopulateAllTasksRunnable(tasksThatSatisfyRequirements);
		}

		void ResolveServiceTasks()
		{
			var serviceTasksFromMetadata = GetServiceTasksFromMetadata();

			var codeToSchedule = taskCollectionGovernor
				.GovernedTasks
				.GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key, y => y.First(), StringComparer.OrdinalIgnoreCase);

			var configuredTasks = serviceTasksFromMetadata.Select(serviceConfig =>
				new ScheduleWithInfo(
					new ServiceTaskInfo(serviceConfig),
					codeToSchedule.GetValueSafe(serviceConfig.Code)
				));

			var tasksThatSatisfyRequirements = taskInitializationRequirements.EnsureTaskRequirementsSatisfied(configuredTasks, taskCollectionGovernor);
			PopulateAllTasksRunnable(tasksThatSatisfyRequirements);
		}

		IEnumerable<IHostedServiceAttribute> GetServiceTasksFromMetadata()
		{
			if (hostedServiceAttributeProvider.GetClientHostedServiceAttribute("DSA") == null)
			{
				throw new InvalidOperationException("Unable to proceed with execution as the mandatory 'Database Security Admin Task' (DSA) is missing from AssemblyMetaDataFile");
			}

			return hostedServiceAttributeProvider.GetClientHostedServiceAttributes();
		}

		void UpdateTaskSchedules(bool reEnableMandatory)
		{
			taskCollectionGovernor.Reload();
			foreach (var serviceTask in allTasks.GetAll())
			{
				serviceTask.UpdateSchedule(taskCollectionGovernor.GovernedTasks, reEnableMandatory);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		void PopulateAllTasksRunnable(IEnumerable<ScheduleWithInfo> tasksThatSatisfyRequirements)
		{
			allTasks.Clear();

			var isProductivityWiseModeEnabled = hostRegistry.ProductivityWiseModeEnabled;

			var runnableTaskCategories = ServiceTaskCategoryDescriptors
				.Get(x => !isProductivityWiseModeEnabled || x.IsShownInProductivityWiseMode)
				.Select(x => x.Code)
				.ToHashSet(StringComparer.Ordinal);

			foreach (var taskInfo in tasksThatSatisfyRequirements)
			{
				if (runnableTaskCategories.Contains(taskInfo.Info.Category))
				{
					allTasks.Add(
						new RunnableServiceTask(
							taskInfo.Info,
							taskInfo.Task,
							actionQueue,
							taskQueue,
							hostLogger,
							errorReporterProxy,
							hostRegistry,
							transactionAdapter,
							loggerFactory,
							serviceTaskScheduleStatusProvider));
				}
			}
		}
	}
}
