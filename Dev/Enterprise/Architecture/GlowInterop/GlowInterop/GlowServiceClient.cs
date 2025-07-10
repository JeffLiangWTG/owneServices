using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CargoWise.Authentication.Glow.Client;
using WTG.Foundation.Http;

namespace Enterprise.ZArchitecture.GlowInterop
{
	class SessionInfo : ISessionInfo
	{
		public Uri BaseUri { get; set; }
	}

	public interface IGlowServiceClient : IDisposable
	{
		Task<HttpResponseMessage> PostAsync(string relativeAddress, HttpContent content);
		Task<HttpResponseMessage> PostAsJsonAsync<T>(string relativeAddress, T content);
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption option);
		Task<HttpResponseMessage> GetAsync(string relativeAddress);
		void SetAdditionalHeader(string headerName, string headerValue);
	}

	class GlowServiceClient : IGlowServiceClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "standard http header")]
		public GlowServiceClient(Uri baseUri, HttpClientHandler innerHandler, IHttpClientFactory clientFactory, IUserSession userSession, IAuthenticationController controller)
		{
			sessionInfo = new SessionInfo();
			clientSessionService = new ClientSessionService(clientFactory, userSession, sessionInfo);

			handler = new GlowAuthenticationHandler(
				innerHandler,
				innerHandler.CookieContainer,
				clientSessionService,
				userSession,
				sessionInfo,
				controller,
				disposeHandler: false
			);

			sessionInfo.BaseUri = baseUri;

			client = new HttpClient(handler, disposeHandler: false);
			client.BaseAddress = baseUri;
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		readonly SessionInfo sessionInfo;
		readonly IClientSessionService clientSessionService;
		readonly HttpMessageHandler handler;
		readonly HttpClient client;

		public async Task<HttpResponseMessage> PostAsync(string relativeAddress, HttpContent content)
		{
			var uri = new Uri(sessionInfo.BaseUri, relativeAddress);
			return await ProcessWithExceptionHandlerAsync(() => client.PostAsync(uri, content)).ConfigureAwait(false);
		}

		public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string relativeAddress, T content)
		{
			var uri = new Uri(sessionInfo.BaseUri, relativeAddress);
			return await ProcessWithExceptionHandlerAsync(() => client.PostAsJsonAsync(uri, content)).ConfigureAwait(false);
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption option)
		{
			return await ProcessWithExceptionHandlerAsync(() => client.SendAsync(request, option)).ConfigureAwait(false);
		}

		public async Task<HttpResponseMessage> GetAsync(string relativeAddress)
		{
			var uri = new Uri(sessionInfo.BaseUri, relativeAddress);
			return await ProcessWithExceptionHandlerAsync(() => client.GetAsync(uri)).ConfigureAwait(false);
		}

		public void SetAdditionalHeader(string headerName, string headerValue) => client.DefaultRequestHeaders.Add(headerName, headerValue);

		async Task<HttpResponseMessage> ProcessWithExceptionHandlerAsync(Func<Task<HttpResponseMessage>> clientMethodRunner)
		{
			try
			{
				return await clientMethodRunner().ConfigureAwait(false);
			}
			catch (HttpRequestException ex) when (ex.InnerException is WebException inner)
			{
				throw new GlowHttpRequestException("Network exception", inner.Response is HttpWebResponse hwr ? hwr.StatusCode : default, ex);
			}
			catch (HttpRequestException ex) when (ex.InnerException is SocketException)
			{
				throw new GlowHttpRequestException("Network exception", 0, ex);
			}
		}

		public void Dispose()
		{
			client.Dispose();
			handler.Dispose();
		}
	}
}
