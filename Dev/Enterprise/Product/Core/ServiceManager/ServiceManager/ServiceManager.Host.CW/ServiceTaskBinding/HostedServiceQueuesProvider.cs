using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Host.CW
{
	public sealed class HostedServiceQueuesProvider : IHostedServiceQueuesProvider
	{
		public HostedServiceQueuesProvider(IHostedServiceQueuesSubProvider[] hostedServiceQueuesSubProviders)
		{
			if (hostedServiceQueuesSubProviders == null)
			{
				throw new ArgumentNullException(nameof(hostedServiceQueuesSubProviders));
			}

			hostedServiceQueues = new Lazy<IEnumerable<IHostedServiceQueue>>(
				() => hostedServiceQueuesSubProviders
					.SelectMany(h => h.Queues)
					.ToArray(),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public IEnumerable<IHostedServiceQueue> Queues => hostedServiceQueues.Value;

		readonly Lazy<IEnumerable<IHostedServiceQueue>> hostedServiceQueues;
	}
}
