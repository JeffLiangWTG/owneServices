using System;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection RegisterRunnerServices(this IServiceCollection services)
		{
			_ = services ?? throw new ArgumentNullException(nameof(services));

			return services
				// Runner.Loggers
				.AddTransient<IGrpcLogger, GrpcLoggerProxy>()

				// Runner
				.AddTransient<IEnvironmentCheckerStrategy, EnvironmentCheckerStrategy>()
				.AddTransient<ICommandExecutionStrategy, CommandExecutionStrategy>()
				.AddTransient<IHostCommunicationStrategy, HostCommunicationStrategy>()
				.AddTransient<IProcessInfo, ProcessInfo>()
				.AddTransient<IResourceManagement, ResourceManagement>()
				.AddTransient<ISqlApplicationLocker, SqlApplicationLocker>()

				.AddTransient<IServiceTaskHandlerInitializer, ServiceTaskHandlerInitializer>()
				.AddTransient<IServiceTaskRunner, ServiceTaskRunner>();
		}
	}
}
