using System;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;

namespace Enterprise.ServiceManager.Host
{
	class QueueRequestHandler : RequestHandler
	{
		public QueueRequestHandler(IQueueStatusProvider statusProvider, IJsonConverter jsonConverter, string hostName)
		{
			this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
			this.jsonConverter = jsonConverter ?? throw new ArgumentNullException(nameof(jsonConverter));
			Uri = ServiceManagerHelper.GetQueueStatusUri(hostName);
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			try
			{
				return jsonConverter.Serialize(statusProvider.GetQueueStatus());
			}
			catch (OperationCanceledException)
			{
				return jsonConverter.Serialize(new QueueListDTO(Array.Empty<QueueDTO>()));
			}
		}

		readonly IQueueStatusProvider statusProvider;
		readonly IJsonConverter jsonConverter;
	}
}
