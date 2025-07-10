using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;
using WTG.Foundation.Http;

namespace GlowIndexQueryService.Common
{
	class GlowWebRequest : IGlowWebRequest
	{
		internal enum HttpRequestType { Get, Post }

		public string Get(string relativeAddress)
		{
			using var client = GetGlowClient(headers);
			return GlowHttpRequest(client, relativeAddress, HttpRequestType.Get, null);
		}
		public string Post(string relativeAddress, string content)
		{
			using var client = GetGlowClient(headers);
			return GlowHttpRequest(client, relativeAddress, HttpRequestType.Post, content);
		}

		internal static string GlowHttpRequest(IGlowServiceClient client, string relativeUrl, HttpRequestType method, string content, bool addOdataPerferHeader = false)
		{
			return GlowHttpRequestCoreAsync(client, relativeUrl, method, content).GetAwaiter().GetResult();
		}

		void IGlowWebRequest.SetAdditionalHeader(string headerName, string headerValue)
		{
			headers ??= [];
			headers[headerName] = headerValue;
		}
		Dictionary<string, string> headers;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
		internal static async Task<string> GlowHttpRequestCoreAsync(IGlowServiceClient client, string relativeUrl, HttpRequestType method, string content)
		{
			using (Db.DisposableActionForDbConnection())
			{
				HttpResponseMessage response = null;
				switch (method)
				{
					case HttpRequestType.Get:
						response = await client.GetAsync(relativeUrl).ConfigureAwait(false);
						break;
					case HttpRequestType.Post:
						using (var stringContent = new StringContent(content))
						{
							response = await client.PostAsync(relativeUrl, stringContent).ConfigureAwait(false);
						}
						break;
					default:
						throw new ArgumentException($"Can only send GET or POST messages, not {method}");
				}

				if (!response.IsSuccessStatusCode)
				{
					throw new GlowHttpRequestException("Unsuccessful Glow Request", response.StatusCode);
				}
				return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			}
		}

		internal static IGlowServiceClient GetGlowClient(Dictionary<string, string> headers)
		{
			var glowServiceUri = GlowRegistry.Instance.GlowServiceUri;
			var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
			var client = clientFactory.Create(new Uri(glowServiceUri));
			if (headers != null)
			{
				foreach (var header in headers)
				{
					client.SetAdditionalHeader(header.Key, header.Value);
				}
			}
			return client;
		}
	}
}
