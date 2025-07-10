using System;
using System.IO;
using System.Text;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	class ServiceHostGetRequestHandler : RequestHandler
	{
		public ServiceHostGetRequestHandler(IServiceHostGetRequest requestor, Uri uri)
		{
			this.requestor = requestor;
			Uri = uri;
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			using (var streamReader = new StreamReader(requestor.RunRequest(request), Encoding.UTF8))
			{
				return streamReader.ReadToEnd();
			}
		}

		readonly IServiceHostGetRequest requestor;
	}
}
