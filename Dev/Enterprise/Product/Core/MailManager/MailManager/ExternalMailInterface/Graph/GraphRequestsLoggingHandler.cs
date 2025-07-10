using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.MailManager.ExternalMailInterface
{
	class GraphRequestsLoggingHandler : DelegatingHandler
	{
		readonly Action<string> logAction;

		public GraphRequestsLoggingHandler(Action<string> logAction)
		{
			this.logAction = logAction;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
		{
			logAction?.Invoke($"Sending Graph request: {httpRequest}");// log the request before it goes out.
			var response = await base.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			logAction?.Invoke($"Received Graph response： {response}");// log the response as it comes back.
			if (response.Content != null)
			{
				// Drain response content to free connections. see https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/dev/docs/logging-requests.md for details
				await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
			}
			return response;
		}
	}
}
