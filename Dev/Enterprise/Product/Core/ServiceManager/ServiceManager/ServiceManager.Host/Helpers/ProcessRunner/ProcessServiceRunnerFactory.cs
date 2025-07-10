using System;
using Enterprise.Integration.Licensing;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ProcessServiceRunnerFactory(
		IProcessFactory processFactory,
		IGrpcClientSynchronizerFactory grpcClientSynchronizerFactory,
		IProcessRunnerRemotingServices processRunnerRemotingServices,
		IBackgroundThreadActionQueueFactory backgroundThreadActionQueueFactory,
		IHostLogger hostLogger,
		IServiceTaskLocksCleaner serviceTaskLocksCleaner,
		IDateTimeProvider dateTimeProvider,
		IErrorReporterProxy errorReporterProxy,
		IHostRegistrySettings hostRegistry,
		IProductRegistration productRegistration) : IServiceRunnerFactory
	{
		public IServiceRunner Create(ITaskScheduler taskScheduler, string taskGroup)
		{
			return new ProcessServiceRunner(taskScheduler, backgroundThreadActionQueueFactory.BackgroundThreadActionQueue, processFactory, processRunnerRemotingServices, hostLogger, grpcClientSynchronizerFactory, serviceTaskLocksCleaner, dateTimeProvider, errorReporterProxy, hostRegistry, taskGroup, productRegistration);
		}

		readonly IProcessFactory processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
		readonly IGrpcClientSynchronizerFactory grpcClientSynchronizerFactory = grpcClientSynchronizerFactory ?? throw new ArgumentNullException(nameof(grpcClientSynchronizerFactory));
		readonly IProcessRunnerRemotingServices processRunnerRemotingServices = processRunnerRemotingServices ?? throw new ArgumentNullException(nameof(processRunnerRemotingServices));
		readonly IBackgroundThreadActionQueueFactory backgroundThreadActionQueueFactory = backgroundThreadActionQueueFactory ?? throw new ArgumentNullException(nameof(backgroundThreadActionQueueFactory));
		readonly IHostLogger hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
		readonly IServiceTaskLocksCleaner serviceTaskLocksCleaner = serviceTaskLocksCleaner ?? throw new ArgumentNullException(nameof(serviceTaskLocksCleaner));
		readonly IDateTimeProvider dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
		readonly IErrorReporterProxy errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		readonly IHostRegistrySettings hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		readonly IProductRegistration productRegistration = productRegistration ?? throw new ArgumentNullException(nameof(productRegistration));
	}
}
