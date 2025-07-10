using System;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class HostStartupCommandResolver : IHostStartupCommandResolver
	{
		public HostStartupCommandResolver(IServiceProvider serviceProvider, IServiceManagerHostOptions hostOptions)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.hostOptions = hostOptions ?? throw new ArgumentNullException(nameof(hostOptions));
		}

		public IHostStartupCommand Resolve()
		{
			var commandType = ResolveCommandType();

			return serviceProvider.GetRequiredService(commandType) as IHostStartupCommand;

			Type ResolveCommandType()
			{
				if (hostOptions.OptionInstall)
				{
					return typeof(InstallServiceStartupCommand);
				}

				if (hostOptions.OptionUninstall)
				{
					return typeof(UninstallServiceStartupCommand);
				}

				if (hostOptions.OptionStart)
				{
					return typeof(StartServiceStartupCommand);
				}

				if (hostOptions.OptionStop)
				{
					return typeof(StopServiceStartupCommand);
				}

				if (hostOptions.OptionConsole)
				{
					return typeof(ConsoleServiceStartupCommand);
				}

				return typeof(RunControllerStartupCommand);
			}
		}

		readonly IServiceProvider serviceProvider;
		readonly IServiceManagerHostOptions hostOptions;
	}
}
