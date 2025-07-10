using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace ServiceManager.Host.CW
{
	sealed class HostedServiceQueueProviderAttributeSubProvider : IHostedServiceQueuesSubProvider
	{
		IEnumerable<IHostedServiceQueue> IHostedServiceQueuesSubProvider.Queues => hostedServiceQueueProviderAttributes.Value;

		readonly Lazy<IEnumerable<IHostedServiceQueue>> hostedServiceQueueProviderAttributes = new Lazy<IEnumerable<IHostedServiceQueue>>(
			() => AssemblyMetaDataReader
				.GetAttributes<HostedServiceQueueProviderAttribute>()
				.Select(attribute => new HostedServiceQueue(attribute))
				.Cast<IHostedServiceQueue>()
				.ToArray(),
			LazyThreadSafetyMode.ExecutionAndPublication);

		class HostedServiceQueue : IHostedServiceQueue
		{
			public HostedServiceQueue(HostedServiceQueueProviderAttribute attribute)
			{
				provider = (IHostedServiceQueueProvider)attribute
					.Type
					.GetConstructor(Array.Empty<Type>())
					.Invoke(null);
				Name = attribute.QueueName;
				ServiceTaskCode = attribute.ServiceTaskCode;
			}

			public QueueResult QueueResult => provider.QueueResult;
			public string Name { get; }
			public string ServiceTaskCode { get; }

			readonly IHostedServiceQueueProvider provider;
		}
	}
}
