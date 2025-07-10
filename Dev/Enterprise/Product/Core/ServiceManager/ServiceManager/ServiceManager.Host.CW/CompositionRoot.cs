using System;
using System.Collections;
using System.Linq;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Extensions;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Shared.Interfaces;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Host.CW.Resource;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Host
{
	public static class CompositionRoot
	{
		public static IServiceCollection AddRegistrations(this IServiceCollection services, string[] args)
		{
			_ = services ?? throw new ArgumentNullException(nameof(services));
			_ = args ?? throw new ArgumentNullException(nameof(args));

			return services
				.AddSingleton<ServiceManagerHost>()
				.AddSingleton<IHostedServiceQueuesProvider, HostedServiceQueuesProvider>()
				.AddTransient(provider => CargoWise.Application.ObjectFactory
								.Get<IEnumerable>("HostedServiceQueuesSubProviders")
								.Cast<IHostedServiceQueuesSubProvider>()
								.ToArray())

				// Registry
				.AddSingleton<HostRegistrySettings>()
				.AddSingleton<IHostRegistrySettings>(o => o.GetService<HostRegistrySettings>())
				.AddSingleton<ISharedRegistrySettings>(o => o.GetService<HostRegistrySettings>())
				.AddSingleton<ILoggerRegistrySettings>(o => o.GetService<HostRegistrySettings>())
				.AddSingleton<IHostRegistry, HostRegistry>()

				.AddTransient<IQueueStatusProviderFactory, QueueStatusProviderFactory>()
				.AddTransient<IServiceManagerTask, RefreshRegistryTask>()
				.AddTransient<IDefaultScheduleConfigurer, DefaultScheduleConfigurer>()

				// Core
				.AddTransient<IServiceManagerTask, OldVersionsRemoverTask>()
				.AddTransient<IResourceThrottler, ResourceThrottler>()
				.AddTransient<BaseExceptionReporter, HostServiceErrorReporter>()

				// Startup Commands
				.AddTransient<IHostStartupCommandResolver, HostStartupCommandResolver>()
				.AddTransient<InstallServiceStartupCommand>()
				.AddTransient<UninstallServiceStartupCommand>()
				.AddTransient<RunControllerStartupCommand>()
				.AddTransient<ConsoleServiceStartupCommand>()
				.AddTransient<StartServiceStartupCommand>()
				.AddTransient<StopServiceStartupCommand>()

				// Business
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IClientHostedServiceAttributeProvider>())

				// Shared
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IServiceTaskScheduleStatusProvider>())
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<INudgingController>())

				// Common.ServiceHostClient
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IServiceHostsCache>())
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IServiceHostRequestProvider>())

				// CargoWise
				.AddTransient(o => CargoWise.Application.ObjectFactory.Get<IApplicationSchemaResolver>())
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IProductRegistration>())
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IDatabaseAspectVersions>())

				// CW1 Services
				.RegisterCommonServices()
				.RegisterSharedServices()
				.RegisterHostLoggerServices()

				// Extension
				.RegisterHostServices(args);
		}
	}
}
