using System;
using System.IO;
using System.Text;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	class ServiceHostPostRequestHandler : RequestHandler
	{
		public ServiceHostPostRequestHandler(IServiceHostPostRequest requestor, IJsonConverter jsonConverter, Uri uri)
		{
			this.requestor = requestor;
			this.jsonConverter = jsonConverter;
			Uri = uri;
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body));
			return jsonConverter.Serialize(requestor.RunRequest(request, stream));
		}

		readonly IServiceHostPostRequest requestor;
		readonly IJsonConverter jsonConverter;
	}
}
