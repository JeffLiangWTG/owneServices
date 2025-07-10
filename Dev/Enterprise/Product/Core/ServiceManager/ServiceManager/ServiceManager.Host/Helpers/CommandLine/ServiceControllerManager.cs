using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host
{
	sealed class ServiceControllerManager : IServiceControllerManager
	{
		readonly IProcessFactory processFactory;
		readonly IServiceControllerFactory serviceControllerFactory;

		public ServiceControllerManager(IProcessFactory processFactory, IServiceControllerFactory serviceControllerFactory)
		{
			this.processFactory = processFactory;
			this.serviceControllerFactory = serviceControllerFactory;
		}

		public IReadOnlyDictionary<ServiceType, IServiceController> GetServiceControllers(IServiceManagerHostOptions hostOptions)
		{
			Dictionary<ServiceType, IServiceController> GetServiceControllersByServiceType(
				IEnumerable<IServiceController> allServices,
				ServiceType[] serviceTypes,
				string dbServer,
				string databaseName)
			{
				var serviceTypeByServiceName = serviceTypes.ToDictionary(
					serviceType => ServiceHostProcess.GetServiceName(serviceType, dbServer, databaseName),
					serviceType => serviceType,
					StringComparer.OrdinalIgnoreCase);

				return allServices
					.Select(sc => new
					{
						ServiceController = sc,
						ServiceType = serviceTypeByServiceName.TryGetValue(sc.ServiceName, out var serviceType) ? serviceType : (ServiceType?)null,
					})
					.Where(x => x.ServiceType.HasValue)
					.ToDictionary(
						x => x.ServiceType!.Value,
						x => x.ServiceController);
			}

			var serviceHost = hostOptions.Host;
			var allServices = serviceControllerFactory.GetServices(serviceHost).ToArray();
			var hostName = hostOptions.ServerName;
			var defaultToLocalhost = serviceHost.StartsWith(hostName, StringComparison.OrdinalIgnoreCase);
			var allServiceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().ToArray();
			var servicesByServiceType = GetServiceControllersByServiceType(allServices, allServiceTypes, hostName, hostOptions.DatabaseName);

			if (!defaultToLocalhost )
			{
				return servicesByServiceType;
			}

			var remainingServiceTypes = allServiceTypes.Except(servicesByServiceType.Keys).ToArray();
			var alternateServicesByServiceType = GetServiceControllersByServiceType(allServices, remainingServiceTypes, "localhost", hostOptions.DatabaseName);
			foreach (var alternatePair in alternateServicesByServiceType)
			{
				servicesByServiceType.Add(alternatePair.Key, alternatePair.Value);
			}

			return servicesByServiceType;
		}

		void SetRecoveryActionToRestart(IServiceController sc)
		{
			const int restartTimeInMinutes = 3;
			var startInfo = new ProcessStartInfo
			{
				FileName = "sc",
				Arguments = string.Format("failure {0} reset= 0 actions= restart/{1}/restart/{1}/restart/{1}", sc.ServiceName, restartTimeInMinutes * 60 * 1000)
			};
			var process = processFactory.Create(startInfo);
			process.Start();
		}

		public void Start(IServiceController sc)
		{
			try
			{
				if (sc.Status == ServiceControllerStatus.Running)
				{
					return;
				}

				sc.Start();
				sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2));
				SetRecoveryActionToRestart(sc);
			}
			catch (System.ServiceProcess.TimeoutException ex)
			{
				ex.Data.Add("waitRequest", "start");
				ex.Data.Add("serviceName", sc.ServiceName);
				throw;
			}
		}

		public void Stop(IServiceController sc)
		{
			try
			{
				if (sc.Status != ServiceControllerStatus.Running)
				{
					return;
				}

				sc.Stop();
				sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2));
			}
			catch (System.ServiceProcess.TimeoutException ex)
			{
				ex.Data.Add("waitRequest", "stop");
				ex.Data.Add("serviceName", sc.ServiceName);
				throw;
			}
		}
	}
}
