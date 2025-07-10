using System;
using CargoWise.Schema;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Host.Queue;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class QueueStatusProviderFactory : IQueueStatusProviderFactory
	{
		public QueueStatusProviderFactory(IMemoryCache memoryCache, IApplicationSchemaResolver applicationSchemaResolver, ICancellationTokenProvider cancellationTokenProvider, IErrorReporterProxy errorReporterProxy, IHostedServiceQueuesProvider hostedServiceQueuesProvider)
		{
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			this.applicationSchemaResolver = applicationSchemaResolver ?? throw new ArgumentNullException(nameof(applicationSchemaResolver));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.hostedServiceQueuesProvider = hostedServiceQueuesProvider ?? throw new ArgumentNullException(nameof(hostedServiceQueuesProvider));
		}

		public IQueueStatusProvider Create(ITaskStatusProvider statusProvider)
		{
			if (statusProvider == null)
			{
				throw new ArgumentNullException(nameof(statusProvider));
			}

			return new QueueStatusProvider(statusProvider, memoryCache, applicationSchemaResolver, cancellationTokenProvider, errorReporterProxy, hostedServiceQueuesProvider);
		}

		readonly IMemoryCache memoryCache;
		readonly IApplicationSchemaResolver applicationSchemaResolver;
		readonly ICancellationTokenProvider cancellationTokenProvider;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IHostedServiceQueuesProvider hostedServiceQueuesProvider;
	}
}
