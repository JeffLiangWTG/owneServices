using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface IHostedServiceAttributeProvider
	{
		IEnumerable<IHostedServiceAttribute> GetHostedServiceAttributes();
		IHostedServiceAttribute GetHostedServiceAttribute(string assemblyName, string code);
	}
}
