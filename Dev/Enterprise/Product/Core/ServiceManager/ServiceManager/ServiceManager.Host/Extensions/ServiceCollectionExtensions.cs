using CargoWise.Common;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection RegisterHostServices(this IServiceCollection services, string[] args)
		{
			return services
				.AddSingleton<ControllerService>()
				.AddSingleton<IControllerService>(o => o.GetService<ControllerService>())
				.AddSingleton<IServiceManagerHostOptions>(o => new HostCommandLineArgsParser(args))
				.AddSingleton<IServiceNameProvider>(o => (HostCommandLineArgsParser)o.GetService<IServiceManagerHostOptions>())
				.AddSingleton<ICancellationRequester, ICancellationTokenProvider, CancellationTokenSourceWrapper>()

				// Common
				.AddTransient<IApplicationEmergencyExit, ApplicationEmergencyExit>()
				.AddTransient<IApplicationExitProxy, ApplicationExitProxy>()
				.AddSingleton<IMemoryCache, MemoryCache>()
				.AddTransient<IDateTimeProvider, ServiceManagerDateTimeProvider>()
				.AddTransient<IJsonConverter, JsonNetConverter>()

				// CargoWise.Common
				.AddTransient<IBackgroundDataSaverFactory, BackgroundDataSaverFactory>()

				// Core
				.AddTransient<IRunnableServiceTasksScheduleUpdater, RunnableServiceTasksScheduleUpdater>()
				.AddTransient<ITaskScheduler, TaskScheduler>()
				.AddSingleton<IController, IServiceManagerTask, Controller>()
				.AddTransient<IServiceManagerTask, LogFileArchiveCleanerTask>()
				.AddSingleton<IQueueMonitorInitializer, IServiceManagerTask, QueueMonitorTask>()
				.AddSingleton<IServiceStopRequestConsumer, ServiceStopper>()
				.AddSingleton<IHostApplicationLockAcquirer, HostApplicationLockAcquirer>()
				.AddTransient<IServiceTaskLocksCleaner, SqlMutexLocksCleaner>()
				.AddTransient<IGrpcClientSynchronizerFactory, GrpcClientSynchronizerFactory>()
				.AddSingleton<IBackgroundThreadActionQueueFactory, BackgroundThreadActionQueueFactory>()
				.AddTransient<IJobObject, JobObject>()
				.AddTransient<IProcessFactory, ProcessWrapperFactory>()
				.AddTransient<IServiceRunnerFactory, ProcessServiceRunnerFactory>()
				.AddTransient<IServiceControllerFactory, ServiceControllerFactory>()
				.AddTransient<IDbConnectionSetup, DbConnectionSetup>()
				.AddTransient<IExceptionHandler, HostExceptionHandler>()
				.AddTransient<ITaskRunRequestProcessor, TaskRunner>()

				.AddTransient<NativeServiceTaskTransactionAdapter>()
				.AddTransient<NativeServiceTasksReloader>()
				.AddTransient<NativeServiceTaskLoader>()
				.AddTransient<ITransactionAdapter, NativeServiceTaskTransactionAdapter>()
				.AddSingleton<ITransactionAdapterFactory, HostTransactionAdapterFactory>()
				.AddSingleton<IServiceTasksReloaderFactory, HostServiceTasksReloaderFactory>()
				.AddSingleton<IServiceTaskLoaderFactory, HostServiceTaskLoaderFactory>()
				.AddSingleton<IManagedInstallerAdapter, ManagedInstallerAdapter>()
				.AddSingleton<IAllTasksCollection, IAllTasksConsumer, AllTasksCollection>()

				// Startup Commands
				.AddTransient<IServiceControllerManager, ServiceControllerManager>()

				// Core.Http
				.AddSingleton<IHttpListenerTask, IServiceManagerTask, HttpListenerTask>()
				.AddTransient<IHttpListenerFactory, HttpListenerWrapperFactory>()
				.AddTransient<IRequestProcessorFactory, RequestProcessorFactory>()
				.AddSingleton<IRequestQueueProduceable, IRequestQueueConsumable, RequestQueue>()
				.AddSingleton<IRequestQueueProcessor, IHttpRequestProcessorInitialiser, IServiceManagerTask, RequestQueueProcessor>()
				.AddTransient<IHttpListenerExceptionHandler, HttpListenerExceptionHandler>()

				// Host.Controller
				.AddTransient<IAsyncDelayProvider, DelayProvider>()
				.AddTransient<IDelayProvider, DelayProvider>()

				//
				.AddTransient<ILogFileArchiveCleaner, LogFileArchiveCleaner>()
				.AddSingleton<IProductRegistrationPeriodicChecker, ProductRegistrationPeriodicChecker>()
				.AddTransient<IServiceManagerApplicationInitializer, ServiceManagerApplicationInitializer>()
				.AddTransientWithLazy<IServiceManagerApplication, ServiceManagerApplication>()
				.AddTransient<IControllerUpgrade, ControllerUpgrade>()
				.AddTransient<ITcpIpRegistryAdjuster, TcpIpRegistryAdjuster>()

				.AddTransient<IWindowsRegistryAdapter, WindowsRegistryAdapter>()
				.AddTransient<IProcessRunnerPoolFactory, ProcessRunnerPoolFactory>()
				.AddSingleton<IProcessRunnerPool, ProcessRunnerPool>()
				.AddTransient<IServiceTasksInitializerFactory, ServiceTasksInitializerFactory>()
				.AddTransient<IServiceHostProviderFactory, ServiceHostProviderFactory>()
				.AddTransient<IHostServiceStatusProvider, HostServiceStatusProvider>()

				// ServiceManager.Shared
				.AddTransient<IProcessRunnerRemotingServices, ProcessRunnerRemotingServices>()
				.AddTransient<IServiceTaskScheduleManager, ServiceTaskScheduleManager>()
				.AddTransient<IServiceTaskRequirementsChecker, ServiceTaskRequirementsChecker>();
		}
	}
}
