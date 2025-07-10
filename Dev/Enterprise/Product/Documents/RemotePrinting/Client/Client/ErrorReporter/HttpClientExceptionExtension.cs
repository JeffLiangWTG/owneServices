using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using WTG.ErrorReporting.Extensibility;

namespace Enterprise.RemotePrinting.Client
{
	public sealed class HttpClientExceptionExtension : IAdditionalDetailContributor
	{
		void IAdditionalDetailContributor.Analyze(in ExceptionContext context)
		{
			if (context.Exception is HttpClientException ex && ex.Response != null)
			{
				var responseMessage = ex.Response;

				if (responseMessage.RequestMessage != null)
				{
					context.AddDetail("RequestUri", responseMessage.RequestMessage.RequestUri?.ToString() ?? string.Empty);

					context.AddDetail("RequestHeaders", GetHeadersText(responseMessage.RequestMessage.Headers));
				}

				context.AddDetail("ResponseStatusCode", responseMessage.StatusCode.ToString());
				context.AddDetail("ResponseReasonPhrase", responseMessage.ReasonPhrase);

				context.AddDetail("ResponseHeaders", GetHeadersText(responseMessage.Headers));
			}
		}

		static string GetHeadersText(HttpHeaders headers)
		{
			if (headers != null)
			{
				var sb = new StringBuilder();

				foreach (var header in headers)
				{
					string headerValue = header.Value == null ? string.Empty : string.Join(",", header.Value);
					sb.Append(header.Key).Append(": ").AppendLine(headerValue);
				}

				return sb.ToString().TrimEnd();
			}

			return string.Empty;
		}
	}
}
