using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host;

public sealed class ServiceControllerFactory : IServiceControllerFactory
{
	public IEnumerable<IServiceController> GetServices(string host)
	{
		return ServiceController.GetServices(host)
			.Select(sc => new ServiceControllerAdapter(sc));
	}
}
