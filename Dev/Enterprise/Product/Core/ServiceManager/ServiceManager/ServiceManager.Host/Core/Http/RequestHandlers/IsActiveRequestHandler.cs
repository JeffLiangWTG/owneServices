using System;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class IsActiveRequestHandler : RequestHandler
	{
		public IsActiveRequestHandler(IHostServiceStatusProvider hostServiceStatusProvider, IJsonConverter jsonConverter, string hostName)
		{
			this.hostServiceStatusProvider = hostServiceStatusProvider;
			this.jsonConverter = jsonConverter;
			Uri = ServiceManagerHelper.GetIsAliveUri(hostName);
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			var isAlive = hostServiceStatusProvider.IsReady();
			return jsonConverter.Serialize(isAlive);
		}

		readonly IHostServiceStatusProvider hostServiceStatusProvider;
		readonly IJsonConverter jsonConverter;
	}
}
