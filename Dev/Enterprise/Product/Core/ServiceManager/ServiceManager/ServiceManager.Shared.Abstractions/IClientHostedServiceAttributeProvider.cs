using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IClientHostedServiceAttributeProvider
	{
		IEnumerable<IHostedServiceAttribute> GetClientHostedServiceAttributes();
		IHostedServiceAttribute GetClientHostedServiceAttribute(string code);
	}
}
