using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Shared;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Host
{
	class Controller : IDisposable, IController
	{
		public Controller(
			IControllerService service,
			IEventLogger eventLogger,
			IHostLogger hostLogger,
			ITaskScheduler taskScheduler,
			IProductRegistrationPeriodicChecker productRegistrationPeriodicChecker,
			IControllerUpgrade controllerUpgrade,
			IHttpRequestProcessorInitialiser httpRequestProcessorInitialiser,
			IRunnableServiceTasksScheduleUpdater runnableServiceTasksScheduleUpdater,
			IProcessRunnerPoolFactory processRunnerPoolFactory,
			IServiceTasksInitializerFactory serviceTasksInitializerFactory,
			IServiceHostProviderFactory serviceHostProviderFactory,
			IQueueMonitorInitializer queueMonitorInitializer,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			ITransactionAdapter transactionAdapter,
			IServiceTasksReloaderFactory serviceTasksReloaderFactory,
			IServiceTaskScheduleManager serviceTaskScheduleManager,
			IResourceThrottler resourceThrottler,
			IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IServiceHostsCache serviceHostsCache,
			IBackgroundThreadActionQueueFactory backgroundThreadActionQueueFactory,
			ITaskRunRequestProcessor taskRunner,
			IAllTasksCollection allTasks)
			: this(
				service,
				eventLogger,
				hostLogger,
				taskScheduler,
				productRegistrationPeriodicChecker,
				controllerUpgrade,
				httpRequestProcessorInitialiser,
				processRunnerPoolFactory,
				serviceTasksInitializerFactory,
				serviceHostProviderFactory,
				runnableServiceTasksScheduleUpdater,
				queueMonitorInitializer,
				errorReporterProxy,
				hostRegistry,
				transactionAdapter,
				serviceTasksReloaderFactory,
				serviceTaskScheduleManager,
				resourceThrottler,
				serviceTaskScheduleStatusProvider,
				hostedServiceAttributeProvider,
				serviceHostsCache,
				backgroundThreadActionQueueFactory,
				taskRunner,
				allTasks
			)
		{
		}

		internal Controller(
			IControllerService service,
			IEventLogger eventLogger,
			IHostLogger hostLogger,
			ITaskScheduler taskScheduler,
			IProductRegistrationPeriodicChecker productRegistrationPeriodicChecker,
			IControllerUpgrade controllerUpgrade,
			IHttpRequestProcessorInitialiser httpRequestProcessorInitialiser,
			IProcessRunnerPoolFactory processRunnerPoolFactory,
			IServiceTasksInitializerFactory serviceTasksInitializerFactory,
			IServiceHostProviderFactory hostProviderFactory,
			IRunnableServiceTasksScheduleUpdater runnableServiceTasksScheduleUpdater,
			IQueueMonitorInitializer queueMonitorInitializer,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			ITransactionAdapter transactionAdapter,
			IServiceTasksReloaderFactory serviceTasksReloaderFactory,
			IServiceTaskScheduleManager serviceTaskScheduleManager,
			IResourceThrottler resourceThrottler,
			IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IServiceHostsCache serviceHostsCache,
			IBackgroundThreadActionQueueFactory backgroundThreadActionQueueFactory,
			ITaskRunRequestProcessor taskRunner,
			IAllTasksCollection allTasks)
		{
			// general application environment initialization (no DB hits)
			Service = service ?? throw new ArgumentNullException(nameof(service));
			this.eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
			this.productRegistrationPeriodicChecker = productRegistrationPeriodicChecker ?? throw new ArgumentNullException(nameof(productRegistrationPeriodicChecker));
			this.controllerUpgrade = controllerUpgrade ?? throw new ArgumentNullException(nameof(controllerUpgrade));
			this.httpRequestProcessorInitialiser = httpRequestProcessorInitialiser ?? throw new ArgumentNullException(nameof(httpRequestProcessorInitialiser));
			this.processRunnerPoolFactory = processRunnerPoolFactory ?? throw new ArgumentNullException(nameof(processRunnerPoolFactory));
			this.serviceTasksInitializerFactory = serviceTasksInitializerFactory ?? throw new ArgumentNullException(nameof(serviceTasksInitializerFactory));
			this.hostProviderFactory = hostProviderFactory ?? throw new ArgumentNullException(nameof(hostProviderFactory));
			this.runnableServiceTasksScheduleUpdater = runnableServiceTasksScheduleUpdater ?? throw new ArgumentNullException(nameof(runnableServiceTasksScheduleUpdater));
			this.queueMonitorInitializer = queueMonitorInitializer ?? throw new ArgumentNullException(nameof(queueMonitorInitializer));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.serviceTaskScheduleManager = serviceTaskScheduleManager ?? throw new ArgumentNullException(nameof(serviceTaskScheduleManager));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			this.transactionAdapter = transactionAdapter ?? throw new ArgumentNullException(nameof(transactionAdapter));
			this.serviceTasksReloaderFactory = serviceTasksReloaderFactory ?? throw new ArgumentNullException(nameof(serviceTasksReloaderFactory));
			this.resourceThrottler = resourceThrottler ?? throw new ArgumentNullException(nameof(resourceThrottler));
			this.serviceTaskScheduleStatusProvider = serviceTaskScheduleStatusProvider ?? throw new ArgumentNullException(nameof(serviceTaskScheduleStatusProvider));
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
			this.serviceHostsCache = serviceHostsCache ?? throw new ArgumentNullException(nameof(serviceHostsCache));
			this.backgroundThreadActionQueueFactory = backgroundThreadActionQueueFactory ?? throw new ArgumentNullException(nameof(backgroundThreadActionQueueFactory));
			this.taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
			this.allTasks = allTasks ?? throw new ArgumentNullException(nameof(allTasks));
		}

		public IControllerService Service { get; }
		public IBackgroundThreadActionQueue ActionQueue => actionQueue;

#if DEBUG
		internal bool IsDotNetRecorded { get; set; }
#endif

#if DEBUG
		protected virtual
#endif
			BusinessObjectFactory ReferenceFactory
		{
			get { return referenceFactory ??= new BusinessObjectFactory { RefreshEnabled = false }; }
		}

#if DEBUG
		internal
#endif
			void RunTaskDispatcher(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			var currentHostName = ServiceManagerHelper.GetHostName();
			var serviceHostProvider = hostProviderFactory.Create(ReferenceFactory);
			var currentHost = serviceHostProvider.LoadServiceHost(currentHostName) ?? serviceHostProvider.CreateServiceHost(currentHostName);
			serviceHostProvider.InitializeController(currentHost, hostLogger);

			actionQueue = backgroundThreadActionQueueFactory.BackgroundThreadActionQueue;
			actionQueue.SetMainThreadId();

			var taskQueue = new TaskQueue(processRunnerPoolFactory.GetOrCreate(), hostLogger);
			var statusProvider = new TaskStatusProvider(allTasks, taskQueue, processRunnerPoolFactory.GetOrCreate(), productRegistrationPeriodicChecker, HostedServiceBusinessObjectBindingsProvider.Instance.BusinessObjectBindings);
			httpRequestProcessorInitialiser.ConfigureHttpRequestProcessor(taskScheduler, statusProvider, actionQueue);
			var serviceHostTerminator = new ServiceHostTerminator(hostRegistry);

			var taskInitializerRunner = new InitializationTaskRunner(hostLogger, taskQueue);
			var taskInitializer = serviceTasksInitializerFactory.Create(
				productRegistrationPeriodicChecker,
				transactionAdapter,
				transactionAdapter.GetCollectionGovernorForAllTasks(),
				taskScheduler,
				allTasks,
				taskInitializerRunner,
				hostLogger,
				eventLogger,
				new TaskInitializationRequirementsChecker(hostLogger, new GlbCompanyProvider()),
				taskQueue,
				actionQueue,
				errorReporterProxy,
				hostRegistry,
				serviceTaskScheduleManager,
				serviceTaskScheduleStatusProvider,
				hostedServiceAttributeProvider,
				serviceHostsCache);

			if (taskInitializer.InitializeServiceTasks())
			{
				queueMonitorInitializer.ConfigureQueueMonitor(statusProvider);

				var secondaryProcessSpinUpDelay = TimeSpan.FromSeconds(hostRegistry.SecondaryProcessSpinUpDelayInSeconds);
				var taskSelector = new TaskSelector(processRunnerPoolFactory.GetOrCreate(), hostLogger);
				var schedulerDispatcher = new SchedulerDispatcher(allTasks, taskSelector, taskRunner, hostLogger, serviceHostsCache, processRunnerPoolFactory.GetOrCreate(), taskScheduler, actionQueue, cancellationToken, errorReporterProxy, hostRegistry, resourceThrottler);
				var throttlingPeriodCalculator = new DispatchingLoopThrottling(taskSelector, processRunnerPoolFactory.GetOrCreate(), secondaryProcessSpinUpDelay, hostRegistry);
				var dsaTaskRunner = taskInitializer.CreateDSARunner();
				var lastDotNetRecord = DateTime.MinValue;
				var loginTime = Db.Connection.LoginTime;

				RunTaskDispatchingLoop(
					taskScheduler,
					taskQueue,
					allTasks.GetAll(),
					schedulerDispatcher,
					productRegistrationPeriodicChecker,
					serviceHostTerminator,
					throttlingPeriodCalculator,
					controllerUpgrade,
					dsaTaskRunner,
					loginTime,
					lastDotNetRecord,
					cancellationToken);

				processRunnerPoolFactory.GetOrCreate().Stop(Service.RequireSwitchOffTime());
			}

			taskScheduler.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected void RunTaskDispatchingLoop(
			ITaskScheduler scheduler,
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
			// If we get a SqlException, check if an upgrade has occurred
			// before running any more tasks.
			var needUpgradeCheck = false;

			while (!cancellationToken.IsCancellationRequested)
			{
				using (hostLogger.LogSection(LogLevel.Debug,
					"Starting dispatch round.",
					"Dispatch round is finished."))
				{
					try
					{
						serviceHostTerminator.TerminateInactiveServiceTaskHosts(hostLogger);
						actionQueue.InvokeActions();

						if (regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown())
						{
							runnableServiceTasksScheduleUpdater.ReloadUpdatedFromDatabase(allTasks, serviceTasksReloaderFactory.CreateServiceTasksReloader(), scheduler);
							RunTaskDispatchingRound(taskQueue, schedulerDispatcher);
						}
						else
						{
							if (taskQueue.GetQueueSnapshot().Any())
							{
								hostLogger.Log(LogLevel.Debug, "Product is unregistered, emptying running queue.");
								taskQueue.EmptyQueue();
							}
						}

						var sleepFor = throttlingPeriodProvider.GetTimeToSleep(taskQueue.GetQueueSnapshot(), allTasks, regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown());
						if (sleepFor > TimeSpan.Zero)
						{
							hostLogger.Log(LogLevel.Debug, "Sleeping for " + sleepFor);
							WaitForDispatchingThreadResumeRequest(sleepFor, cancellationToken);
						}

						if (Db.Connection.LoginTime != loginTime)
						{
							loginTime = Db.Connection.LoginTime;
							needUpgradeCheck = true;
							dsaTaskRunner.RunDsaTaskIfDbServerRestarts();
						}
					}
					catch (DatabaseUpgradeException)
					{
						needUpgradeCheck = true;
					}
					catch (Exception ex) when (ex.FlattenInnerExceptions().Any(x => x is DatabaseUpgradeException))
					{
						needUpgradeCheck = true;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						errorReporterProxy.ReportOnce("Controller Dispatching Loop", ex);
					}

					var lastDotNetVersionRecordingLimitHours = TimeSpan.FromDays(1);
					if (!needUpgradeCheck && DateTime.UtcNow.Subtract(lastDotNetRecord) > lastDotNetVersionRecordingLimitHours)
					{
						hostLogger.Log(LogLevel.Debug, "Dispatcher loop - recording .Net version.");
						var dbInternals = (IDbConnectionInternals)Db.Connection;

						var sqlConnection = (SqlConnection)dbInternals.InternalDbConnection;
						var sqlTransaction = (SqlTransaction)dbInternals.InternalDbTransaction;

						var dbHandler = new DbHandlerForDotNet(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
						var recorder = new DotNetRecorder(dbHandler);
						try
						{
							recorder.RecordDotNetVersionAndRuntimes();
						}
						catch (Exception ex) when (ex is SqlException or InvalidOperationException)
						{
							Db.Connection.EnsureIsOpen();
							recorder.RecordDotNetVersionAndRuntimes();
						}

						lastDotNetRecord = DateTime.UtcNow;
#if DEBUG
						IsDotNetRecorded = true;
#endif
					}

					if (needUpgradeCheck || upgrader.TimeSinceLastUpgradeCheck > TimeSpan.FromMinutes(15))
					{
						hostLogger.Log(LogLevel.Debug, "Dispatcher loop - checking for software upgrade.");
						upgrader.UpgradeSoftwareIfNeeded();
						needUpgradeCheck = false;
					}
				}
			}
		}

#if DEBUG
		protected
#endif
			void RunTaskDispatchingRound(ITaskQueue taskQueue, ISchedulerDispatcher schedulerDispatcher)
		{
			schedulerDispatcher.Schedule(taskQueue);
			schedulerDispatcher.Dispatch(taskQueue);
		}

#if DEBUG
		protected
#endif
			bool WaitForDispatchingThreadResumeRequest(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return actionQueue.WaitForEnqueue(timeout, cancellationToken);
		}

		public string Name { get; } = typeof(Controller).FullName;
		public TimeSpan RunDelay { get; } = TimeSpan.Zero;
		public TimeSpan ErrorDelay { get; } = TimeSpan.Zero;

		public void Initialise(CancellationToken cancellationToken)
		{
		}

		public void Run(CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				RunTaskDispatcher(cancellationToken);
			}
		}

		readonly IControllerUpgrade controllerUpgrade;
		readonly IEventLogger eventLogger;
		readonly IHostLogger hostLogger;
		readonly IProcessRunnerPoolFactory processRunnerPoolFactory;
		readonly IProductRegistrationPeriodicChecker productRegistrationPeriodicChecker;
		readonly IHttpRequestProcessorInitialiser httpRequestProcessorInitialiser;
		readonly IRunnableServiceTasksScheduleUpdater runnableServiceTasksScheduleUpdater;
		readonly IQueueMonitorInitializer queueMonitorInitializer;
		readonly IServiceTasksInitializerFactory serviceTasksInitializerFactory;
		readonly ITaskScheduler taskScheduler;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IServiceTaskScheduleManager serviceTaskScheduleManager;
		readonly ITransactionAdapter transactionAdapter;
		readonly IServiceTasksReloaderFactory serviceTasksReloaderFactory;
		protected IBackgroundThreadActionQueue actionQueue;
		protected IServiceHostProviderFactory hostProviderFactory;
		BusinessObjectFactory referenceFactory;
		readonly IHostRegistrySettings hostRegistry;
		readonly IResourceThrottler resourceThrottler;
		readonly IServiceTaskScheduleStatusProvider serviceTaskScheduleStatusProvider;
		readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;
		readonly IServiceHostsCache serviceHostsCache;
		readonly IBackgroundThreadActionQueueFactory backgroundThreadActionQueueFactory;
		readonly ITaskRunRequestProcessor taskRunner;
		readonly IAllTasksCollection allTasks;

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
				if (actionQueue != null)
				{
					actionQueue.Dispose();
					actionQueue = null;
				}
			}
		}

		#endregion
	}
}
