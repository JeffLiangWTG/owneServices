using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.ClientLink
{
	static class ClientLinkMethods
	{
		public static IHubConnectionAndProxy GetHubConnectionAndProxy(string clientLinkId)
		{
			return new HubConnectionAndProxy(Registry.Business.GlowRegistry.Instance.GlowServiceUri, clientLinkId);
		}

		public static async Task<string> GetClientLinkIdAsync()
		{
			var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
			var lifecycleValues = new Dictionary<string, string> { { "lifecycleToken", EnvProxy.Instance.SemaphoreProvider.InternalHeartbeat.HeartbeatId.ToString() } };

			using var client = clientFactory.Create(new Uri(Registry.Business.GlowRegistry.Instance.GlowServiceUri));

			var response = await client.PostAsync("clientlink/create", new FormUrlEncodedContent(lifecycleValues));
			if (!response.IsSuccessStatusCode)
			{
				throw new ClientLinkException($"Unable to create ClientLink session: {response.StatusCode}");
			}

			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var clientLinkId = JObject.Parse(content)["id"].ToString();

			return clientLinkId;
		}

		public static void HandleMessage(IMessageTransportLayer clientLinkMessageHandler, IList<JToken> tokens)
		{
			try
			{
				foreach (var token in tokens)
				{
					clientLinkMessageHandler.HandleFromBrowser(token.ToString());
				}
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce("ClientLinkUnexpectedException", e);
			}
		}

		public static async Task SendToBrowserAsync(IHubConnectionAndProxy hubConnectionAndProxy, string message)
		{
			await hubConnectionAndProxy.HubProxy.Invoke("SendMessage", message).ConfigureAwait(false);
		}

		public static string GetConnectionLostMessage()
		{
			return Res.GetString("f1f04dc4-914a-4a6b-aa2a-5cb0de5efd96", "Connection Lost - Please try again. If this keeps happening unexpectedly, please contact your system administrator.");
		}

		public static string GetClientLinkConnectionUrl(string url, string clientLinkId)
		{
#pragma warning disable CW1161 // String is a URL
			return url + "&clid=" + clientLinkId;
#pragma warning restore CW1161 // String is a URL

		}
	}
}
