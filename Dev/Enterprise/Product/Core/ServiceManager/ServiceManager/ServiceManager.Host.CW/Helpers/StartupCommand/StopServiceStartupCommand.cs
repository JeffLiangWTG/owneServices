using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.CW
{
	class StopServiceStartupCommand : ServiceControllerStartupCommand
	{
		public StopServiceStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IServiceControllerManager serviceControllerManager)
			: base(hostLogger, hostOptions, serviceControllerManager)
		{
		}

		protected override void ExecuteCore(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers)
		{
			var servicesStopped = new List<string>();
			// note: unlike start, here we are stopping all services regardless of the registry settings,
			// to ensure that the services we do not need anymore are stopped
			// we stop processController first, to avoid bad status report when isAlive is called.
			var serviceControllersToStop = serviceControllers
				.OrderBy(p => p.Key)
				.Select(p => p.Value);
			try
			{
				foreach (var sc in serviceControllersToStop)
				{
					serviceControllerManager.Stop(sc);
					servicesStopped.Add(sc.ServiceName);
				}
			}
			finally
			{
				if (servicesStopped.Count > 0)
				{
					hostLogger.Log(LogLevel.Information, $"Services have been successfully stopped: {string.Join(", ", servicesStopped)}");
				}
			}
		}
	}
}
