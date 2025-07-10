using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;

namespace Enterprise.ServiceManager.Runner
{
	class ServiceTaskHandlerFactory : IServiceTaskHandlerFactory
	{
		public ServiceTaskHandlerFactory(IServiceTaskLogger serviceTaskLogger, IRunnerRegistrySettings registry, IServiceTaskLoaderFactory taskLoaderFactory, ILoggerFactory loggerFactory)
		{
			this.serviceTaskLogger = serviceTaskLogger ?? throw new ArgumentNullException(nameof(serviceTaskLogger));
			this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
			this.taskLoaderFactory = taskLoaderFactory ?? throw new ArgumentNullException(nameof(taskLoaderFactory));
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public IServiceTaskHandler CreateServiceTaskHandler(string assemblyName, string typeName)
		{
			var assembly = Assembly.Load(assemblyName);

			if (registry.SwitchRunnerToNetCore)
			{
				serviceTaskLogger.Log(LogLevel.Information, $"Assembly version is {assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkDisplayName}");
			}

			var serviceObject = assembly.CreateInstance(typeName) ?? throw new InvalidOperationException($"The service task ({typeName}) was not found in the assembly: {assemblyName}");

			return new ServiceProviderImplProxy(
				serviceObject as ServiceProviderImpl ?? throw new InvalidOperationException($"{typeName} is not of type ServiceProviderImpl."),
				taskLoaderFactory.CreateServiceTaskLoader(),
				loggerFactory);
		}

		readonly IServiceTaskLogger serviceTaskLogger;
		readonly IRunnerRegistrySettings registry;
		readonly IServiceTaskLoaderFactory taskLoaderFactory;
		readonly ILoggerFactory loggerFactory;
	}
}
