using System;
using CargoWise.Data;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	class RequestProcessorFactory : IRequestProcessorFactory
	{
		public RequestProcessorFactory(IHostLogger hostLogger, IQueueStatusProviderFactory queueStatusProviderFactory, IHostServiceStatusProvider hostServiceStatusProvider, IJsonConverter jsonConverter, IServiceHostRequestProvider serviceHostRequestProvider)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.queueStatusProviderFactory = queueStatusProviderFactory ?? throw new ArgumentNullException(nameof(queueStatusProviderFactory));
			this.hostServiceStatusProvider = hostServiceStatusProvider ?? throw new ArgumentNullException(nameof(hostServiceStatusProvider));
			this.jsonConverter = jsonConverter ?? throw new ArgumentNullException(nameof(jsonConverter));
			this.serviceHostRequestProvider = serviceHostRequestProvider ?? throw new ArgumentNullException(nameof(serviceHostRequestProvider));
		}

		public IRequestProcessor CreateRequestProcessor(IActionQueue actionQueue, ITaskScheduler scheduler, ITaskStatusProvider statusProvider)
		{
			if (actionQueue == null)
			{
				throw new ArgumentNullException(nameof(actionQueue));
			}

			if (scheduler == null)
			{
				throw new ArgumentNullException(nameof(scheduler));
			}

			if (statusProvider == null)
			{
				throw new ArgumentNullException(nameof(statusProvider));
			}

			var responseStringBuilder = new ResponseStringBuilder(
				Db.ServerName,
				Db.DatabaseName,
				scheduler,
				statusProvider,
				queueStatusProviderFactory,
				hostServiceStatusProvider,
				jsonConverter,
				serviceHostRequestProvider);
			return new RequestProcessor(responseStringBuilder, hostLogger, actionQueue);
		}

		readonly IHostLogger hostLogger;
		readonly IQueueStatusProviderFactory queueStatusProviderFactory;
		readonly IHostServiceStatusProvider hostServiceStatusProvider;
		readonly IJsonConverter jsonConverter;
		readonly IServiceHostRequestProvider serviceHostRequestProvider;
	}
}
