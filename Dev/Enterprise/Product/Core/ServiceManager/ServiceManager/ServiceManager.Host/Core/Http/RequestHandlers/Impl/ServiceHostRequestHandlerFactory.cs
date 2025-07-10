using System;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host
{
	static class ServiceHostRequestHandlerFactory
	{
		internal static RequestHandler GetHandler(IServiceHostRequest request, IJsonConverter jsonConverter, Uri uri)
		{
			return request switch
			{
				IServiceHostGetRequest getRequest => new ServiceHostGetRequestHandler(getRequest, uri),
				IServiceHostPostRequest postRequest => new ServiceHostPostRequestHandler(postRequest, jsonConverter, uri),
				_ => throw new InvalidOperationException("No request handler found for type " + request.GetType())
			};
		}
	}
}
