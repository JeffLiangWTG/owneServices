using System;
using System.Linq;
using System.ServiceProcess;

namespace Enterprise.Dat.Implementation.Preconditions
{
	static class OlapServerChecker
	{
		internal static bool IsRunning()
		{
			try
			{
				var serviceName = ServiceName;
				return ServiceController.GetServices().FirstOrDefault(service => service.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))?.Status == ServiceControllerStatus.Running;
			}
			catch
			{
				return false;
			}
		}

		static string ServiceName
		{
			get
			{
				var serviceName = LocalDBConnection.GetServiceName();
				if (!string.IsNullOrEmpty(serviceName) && serviceName != "MSSQLSERVER")
				{
					return "MSOLAP$" + serviceName;
				}
				else
				{
					return "MSSQLServerOLAPService";
				}
			}
		}
	}
}
