using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	public interface IHostedServiceQueuesProvider
	{
		IEnumerable<IHostedServiceQueue> Queues { get; }
	}
}
