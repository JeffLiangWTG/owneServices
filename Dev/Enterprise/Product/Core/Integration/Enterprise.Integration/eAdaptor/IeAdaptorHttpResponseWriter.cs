using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Enterprise.Integration
{
	public interface IeAdaptorHttpResponseWriter
	{
		HttpResponseMessage CreateResponse(IeAdaptorRequestProcessorResult data);

		HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, Exception exception);

		HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message);

		HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, HttpError error);
	}
}
