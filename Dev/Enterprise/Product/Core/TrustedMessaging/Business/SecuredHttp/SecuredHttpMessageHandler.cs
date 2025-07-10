using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.TrustedMessaging.Business.SecuredHttp;

namespace Enterprise.TrustedMessaging.Business
{
	public class SecuredHttpMessageHandler : HttpClientHandler
	{
		public SecuredHttpMessageHandler(ISecuredHttpTrustedClientConfiguration clientConfiguration)
		{
			this.clientConfiguration = clientConfiguration;
		}

		readonly ISecuredHttpTrustedClientConfiguration clientConfiguration;

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var statusCode = HttpStatusCode.OK;
			string bodyContent = string.Empty;

			try
			{
				var rawContent = await request?.Content.ReadAsStringAsync();
				var relativePath = request.RequestUri.AbsoluteUri.Replace(clientConfiguration.RemoteEndpointRootUrl, string.Empty);
				var client = new SecuredHttpTrustedClient(clientConfiguration);

				var remoteResponse = await client.SendRequest<string, string>(rawContent, clientConfiguration.ProductCode, clientConfiguration.SystemId, relativePath, cancellationToken);

				if (remoteResponse.Success)
				{
					bodyContent = remoteResponse.Response;
				}
				else
				{
					statusCode = HttpStatusCode.InternalServerError;
					var stringBuilder = new StringBuilder();
					remoteResponse.Messages?.ToList().ForEach(o => stringBuilder.AppendLine($"ErrorCode:{o.Code}, ErrorMessage:{o.Message}"));
					bodyContent = stringBuilder.ToString();
				}
			}
			catch (Exception ex)
			{
				statusCode = HttpStatusCode.InternalServerError;
				bodyContent = ex.Message;
			}

			return new HttpResponseMessage(statusCode) { Content = new StringContent(bodyContent), RequestMessage = request };
		}
	}
}
