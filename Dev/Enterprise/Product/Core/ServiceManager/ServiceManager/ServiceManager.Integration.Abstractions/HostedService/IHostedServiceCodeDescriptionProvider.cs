using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceCodeDescriptionProvider
	{
		IEnumerable<HostedServiceCodeDescription> GetHostedServices();
	}
}
