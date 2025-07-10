using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceQueuesSubProvider
	{
		IEnumerable<IHostedServiceQueue> Queues { get; }
	}
}
