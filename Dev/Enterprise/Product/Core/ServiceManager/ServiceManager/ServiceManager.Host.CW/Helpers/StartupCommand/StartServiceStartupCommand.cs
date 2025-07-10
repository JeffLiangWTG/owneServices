using System.Collections.Generic;
using System.Linq;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.CW
{
	class StartServiceStartupCommand : ServiceControllerStartupCommand
	{
		public StartServiceStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IServiceControllerManager serviceControllerManager)
			: base(hostLogger, hostOptions, serviceControllerManager)
		{
		}

		protected override void ExecuteCore(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers)
		{
			var servicesStarted = new List<string>();
			var serviceTypesToStart = ServiceManagerHelper.GetExtraServiceTypes().Append(ServiceType.ProcessController);
			// note: unlike stop, here we are starting only the required services
			// we start processController last, so it can check the state of other services when isAlive is called.
			var serviceControllersToStart = serviceControllers
				.Where(p => serviceTypesToStart.Contains(p.Key))
				.OrderByDescending(p => p.Key)
				.Select(p => p.Value)
				.ToList();

			try
			{
				foreach (var sc in serviceControllersToStart)
				{
					serviceControllerManager.Start(sc);
					servicesStarted.Add(sc.ServiceName);
				}
			}
			finally
			{
				if (servicesStarted.Count > 0)
				{
					hostLogger.Log(LogLevel.Information, $"Services have been successfully started: {string.Join(", ", servicesStarted)}");
				}
			}
		}
	}
}
