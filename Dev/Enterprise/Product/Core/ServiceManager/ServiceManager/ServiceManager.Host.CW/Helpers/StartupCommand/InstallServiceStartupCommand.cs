using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class InstallServiceStartupCommand : ServiceInstallerStartupCommand
	{
		public InstallServiceStartupCommand(
			IHostLogger hostLogger,
			IServiceManagerHostOptions hostOptions,
			IManagedInstallerAdapter managedInstallerHelper,
			IProductRegistration productRegistration)
			: base(hostLogger, hostOptions, managedInstallerHelper, productRegistration)
		{
		}

		public override int Execute()
		{
			try
			{
				var installerArgs = CreateServiceInstallerArguments(hostOptions);

				var result = RunInstaller(installerArgs.ToArray());
				if (result > 0)
				{
					hostLogger.LogInformation("Service has been successfully installed.");
				}

				return result;
			}
			catch (Exception ex)
			{
				hostLogger.Log(LogLevel.Error, $"{DbConnectionConstants.ApplicationNames.ServiceHost} has encountered an install error:{ex}", ex);
			}

			return -1;
		}
	}
}
