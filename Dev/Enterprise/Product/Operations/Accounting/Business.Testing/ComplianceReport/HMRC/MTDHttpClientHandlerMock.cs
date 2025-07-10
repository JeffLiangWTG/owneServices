using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDHttpClientHandlerMock : HttpMessageHandler
	{
		readonly IDictionary<string, HttpResponseMessage> ResponseMessageLookup =
			new Dictionary<string, HttpResponseMessage>();

		MTDHttpClientHandlerMock()
		{
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var url = request.RequestUri.AbsoluteUri;

			var responseMessage = ResponseMessageLookup.ContainsKey(url)
				? ResponseMessageLookup[url]
				: new HttpResponseMessage(HttpStatusCode.NotFound);

			return Task.FromResult(responseMessage);
		}

		public static MTDHttpClientHandlerMock New()
		{
			return new MTDHttpClientHandlerMock();
		}

		public void ClearAllResponses()
		{
			ResponseMessageLookup.Clear();
		}

		public void ClearResponse(string url)
		{
			if (ResponseMessageLookup.ContainsKey(url))
			{
				ResponseMessageLookup.Remove(url);
			}
		}

		public void AddJsonResponse(string url, HttpStatusCode statusCode, string content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> userDefinedContentHeaders = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null)
		{
			var contentHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
			contentHeaders.Add(new KeyValuePair<string, IEnumerable<string>>("Content-Type", new string[] { "application/json" }));

			if (userDefinedContentHeaders != null)
			{
				contentHeaders.AddRange(userDefinedContentHeaders);
			}

			var responseMessage = CreateHttpResponseMessage(statusCode, content, contentHeaders, headers);
			ResponseMessageLookup[url] = responseMessage;
		}

		HttpResponseMessage CreateHttpResponseMessage(HttpStatusCode statusCode, string content, IEnumerable<KeyValuePair<string, IEnumerable<string>>> contentHeaders = null, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null)
		{
			var result = new HttpResponseMessage(statusCode) { Content = new StringContent(content) };

			if (contentHeaders != null && contentHeaders.Any())
			{
				foreach (var header in contentHeaders)
				{
					if (result.Content.Headers.Contains(header.Key))
					{
						result.Content.Headers.Remove(header.Key);
					}
					result.Content.Headers.Add(header.Key, header.Value);
				}
			}

			if (headers != null && headers.Any())
			{
				foreach (var header in headers)
				{
					if (result.Headers.Contains(header.Key))
					{
						result.Headers.Remove(header.Key);
					}
					result.Headers.Add(header.Key, header.Value);
				}
			}

			return result;
		}
	}
}
