using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.BufferManagement.Service.Client
{
	public sealed class PAVEHttpClient : IPAVEHttpClient
	{
		readonly Uri baseUri;
		readonly IGlowServiceClientFactory glowServiceClientFactory;

		internal PAVEHttpClient(Uri baseUri)
		{
			this.baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
			glowServiceClientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
		}

		public T Post<T>(string action, object body)
		{
			return Task
				.Run(async () => await PostAsync<T>(action, body).ConfigureAwait(false))
				.GetAwaiter()
				.GetResult();
		}

		public async Task<T> PostAsync<T>(string action, object body)
		{
			StringContent createContent() => new StringContent(JsonConvert.SerializeObject(body), PAVEServicesConstants.Encoding, PAVEServicesConstants.MediaType);
			return await Execute<T>(client => client.PostAsync("cw1api/" + PAVEServicesConstants.ControllersPath + action, createContent())).ConfigureAwait(false); // URL path
		}

		async Task<T> Execute<T>(Func<IGlowServiceClient, Task<HttpResponseMessage>> method)
		{
			using (var client = glowServiceClientFactory.Create(baseUri))
			{
				string responseContent = null;
				var responseMessage = await method.Invoke(client).ConfigureAwait(false);

				if (responseMessage.Content != null)
				{
					responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
				}

				if (!responseMessage.IsSuccessStatusCode)
				{
					var errorMessage = FormattableString.Invariant($"Error sending a {responseMessage.RequestMessage?.Method.Method} request to {responseMessage.RequestMessage?.RequestUri}, Code: {(int)responseMessage.StatusCode}, Phrase: {responseMessage.ReasonPhrase}, Response Content: {responseContent}"); // System Notification
					throw new HttpRequestException(errorMessage);
				}

				return JsonConvert.DeserializeObject<T>(responseContent);
			}
		}
	}
}
