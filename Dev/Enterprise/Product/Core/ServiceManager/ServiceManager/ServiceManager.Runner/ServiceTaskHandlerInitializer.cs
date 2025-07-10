using System;
using System.Threading;
using Enterprise.ServiceManager.Runner.Extensions;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using WTG.ApplicationLogging.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ServiceTaskHandlerInitializer : IServiceTaskHandlerInitializer
	{
		public ServiceTaskHandlerInitializer(IHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IServiceTaskHandlerFactory serviceTaskHandlerFactory,
			IResourceManagement resourceManagement,
			IRunnerLogger runnerLogger,
			IApplicationLoggerFactory loggerFactory)
		{
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
			this.serviceTaskHandlerFactory = serviceTaskHandlerFactory ?? throw new ArgumentNullException(nameof(serviceTaskHandlerFactory));
			this.resourceManagement = resourceManagement ?? throw new ArgumentNullException(nameof(resourceManagement));
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateRunnerLogger();
		}

		(IHostedServiceAttribute hostedServiceAttribute, IDisposableServiceTaskHandler disposableServiceTaskHandler) Initialize(string assemblyName, string code)
		{
			using var activity = logger.ActivitySource.StartActivity("ServiceTaskHandlerInitializer.Initialize")!;
			activity.AddTag("ServiceTaskCode", code);
			activity.AddTag("ServiceTaskAssembly", assemblyName);

			System.Environment.SetEnvironmentVariable("ServiceTaskAssembly", assemblyName);
			System.Environment.SetEnvironmentVariable("ServiceTaskCode", code);

			var hostedServiceAttribute = hostedServiceAttributeProvider.GetHostedServiceAttribute(assemblyName, code);
			var serviceTaskHandler = serviceTaskHandlerFactory.CreateServiceTaskHandler(assemblyName, hostedServiceAttribute.TypeName);

			return (hostedServiceAttribute, new DisposableServiceTaskHandler(serviceTaskHandler, resourceManagement));
		}

		public IDisposableServiceTaskHandler CreateServiceTaskHandler(string assemblyName, string code, string taskConfigString = "")
		{
			var (hostedServiceAttribute, disposableServiceTaskHandler) = Initialize(assemblyName, code);
			disposableServiceTaskHandler.InitializeRunningEnvironment(hostedServiceAttribute, taskConfigString, runnerLogger);
			return disposableServiceTaskHandler;
		}

		readonly IHostedServiceAttributeProvider hostedServiceAttributeProvider;
		readonly IRunnerLogger runnerLogger;
		readonly IApplicationLogger logger;
		readonly IServiceTaskHandlerFactory serviceTaskHandlerFactory;
		readonly IResourceManagement resourceManagement;
	}

	sealed record DisposableServiceTaskHandler : IDisposableServiceTaskHandler
	{
		public DisposableServiceTaskHandler(IServiceTaskHandler serviceTaskHandler, IResourceManagement resourceManagement)
		{
			this.serviceTaskHandler = serviceTaskHandler;
			this.resourceManagement = resourceManagement;
		}

		public IHostedServiceAttribute HostedServiceAttribute => serviceTaskHandler.HostedServiceAttribute;

		public void Run(CancellationToken cancellationToken = default)
			=> serviceTaskHandler.Run(cancellationToken);
		public void InitializeRunningEnvironment(IHostedServiceAttribute hostedServiceAttribute, string taskConfigString, ILogger logger)
			=> serviceTaskHandler.InitializeRunningEnvironment(hostedServiceAttribute, taskConfigString, logger);

		public void Dispose()
		{
			resourceManagement.ReclaimMemory(ref serviceTaskHandler);
		}

		public void HandleException(Exception exception, string serviceTaskCode)
		{
			serviceTaskHandler.HandleException(exception, serviceTaskCode);
		}

		IServiceTaskHandler serviceTaskHandler;
		readonly IResourceManagement resourceManagement;
	}
}
