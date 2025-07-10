using System;
using System.Net;
using System.Net.Http;
using WTG.Foundation.Http;

namespace Enterprise.ZArchitecture.GlowInterop
{
	sealed class GlowHttpClientFactory : IHttpClientFactory
	{
		public GlowHttpClientFactory()
		{
			inner = new WTG.Foundation.Http.HttpClientFactory(() => new HttpClientHandlerWithDiagnostics(new CookieContainer()));
		}

		readonly IHttpClientFactory inner;

		public HttpClient Create() => inner.Create();

		public HttpClient Create(TimeSpan timeout) => inner.Create(timeout);

		public HttpClient CreateNew(HttpMessageHandler handler = null) => inner.CreateNew(handler);

		public HttpClient CreateNew(HttpMessageHandler handler, TimeSpan timeout) => inner.CreateNew(handler, timeout);

		public void Dispose() => inner.Dispose();
	}
}
