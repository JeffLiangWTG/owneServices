using System.Collections.Concurrent;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	interface IServiceTasksInitializerFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1082:DoNotUseTooManyArguments")]
		IServiceTasksInitializer Create(
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
			IServiceHostsCache serviceHostsCache);
	}
}
