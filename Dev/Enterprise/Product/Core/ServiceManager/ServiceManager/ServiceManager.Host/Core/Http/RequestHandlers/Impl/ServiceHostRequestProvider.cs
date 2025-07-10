using System.Collections.Generic;
using System.Linq;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	public class ServiceHostRequestProvider : IServiceHostRequestProvider
	{
		public IEnumerable<IServiceHostRequest> ServiceHostRequests => Enumerable.Empty<IServiceHostRequest>();
	}
}
