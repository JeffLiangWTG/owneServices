using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.RemoteDesktopServices.Client;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Shared;
using WTG.OpenIDConnect.Login;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public class WtsPluginForTest : WtsPlugin
	{
		public WtsPluginForTest() : base(ClientEntry.Rds)
		{
			MessageHandlers.Register(EnterpriseChannelMessageTypes.SystemToSystemTrustMessage, new MockSystemToSystemTrustMessageHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.OIDCLogin, new MockOIDCLoginHandler(null));
		}
	}

	class MockSystemToSystemTrustMessageHandler : XmlMessageHandler<SystemToSystemTrustMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, SystemToSystemTrustMessage message)
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", message.AccessToken);

				_ = httpClient.PostAsync(message.PostUrl, new StringContent(string.Empty)).Result;
			}
		}
	}

	class MockOIDCLoginHandler : OIDCLoginHandler
	{
		internal MockOIDCLoginHandler(OIDCLoginFactory oidcClientOptionsFactory) : base(oidcClientOptionsFactory)
		{
		}

		protected override OIDCLogin GetOIDCLogin(string redirectUri)
		{
			return new OIDCLogin(OIDCLoginShared.RedirectURI, OIDCLoginShared.RefreshTokenPath, new NullLogger(), delegate (string startUrl)
			{
				Task.Run(() =>
				{
					Thread.Sleep(5000); // Simulate user interaction
					var redirectPos = startUrl.IndexOf("redirect_uri=");
					var redirectUri = startUrl.Substring(redirectPos + "redirect_uri=".Length, startUrl.IndexOf('&', redirectPos) - redirectPos - "redirect_uri=".Length).Replace("%3A", ":").Replace("%2F", "/");
					var statePos = startUrl.IndexOf("state=");
					var state = startUrl.Substring(statePos, startUrl.IndexOf('&', statePos) - statePos);
					var pretendServer = new HttpClient();
					pretendServer.GetAsync(redirectUri + "?code=F99D7E65C1029E67B2B00565FB7D32C5B4FC4DAB53B6FF5B003E338C15BCE327&scope=openid%20profile%20api%20offline_access&" + state + "&session_state=PZV2X0rn1FueCpnIYz3CUzWhM1EYnq4D2N3aB6ajtsk.D490E01EF7BE079E072FBA3A8450574A");
				});
			});
		}
	}
}
