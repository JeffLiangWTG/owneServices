using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.RemoteDesktopServices.Shared;
using Enterprise.ZArchitecture.Core;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.OpenIDConnect.Login;

namespace Enterprise.RemoteDesktopServices.Testing
{
	[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
	class OIDCLoginServerTest : RemoteDesktopServicesTest
	{
		public void TestLocalLoginSuccessForDefaultOIDCLoginFactory_AzureServer()
		{
			TestLocalLoginSuccessForDefaultOIDCLoginFactory(OIDCLoginRequestMessage.OIDCServer.Azure, false);
		}

		public void TestLocalLoginSuccessForDefaultOIDCLoginFactory_GenericServer()
		{
			TestLocalLoginSuccessForDefaultOIDCLoginFactory(OIDCLoginRequestMessage.OIDCServer.Generic, true);
		}

		public void TestLocalLoginSuccessForDefaultOIDCLoginFactory_OneLoginServer()
		{
			TestLocalLoginSuccessForDefaultOIDCLoginFactory(OIDCLoginRequestMessage.OIDCServer.OneLogin, true);
		}

		public void TestLocalLoginSuccessForDefaultOIDCLoginFactory_OktaServer()
		{
			TestLocalLoginSuccessForDefaultOIDCLoginFactory(OIDCLoginRequestMessage.OIDCServer.Okta, true);
		}

		void TestLocalLoginSuccessForDefaultOIDCLoginFactory(OIDCLoginRequestMessage.OIDCServer oidcServer, bool shouldFail)
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;

			using (Mock80PortOccupied())
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				response = new OIDCLoginServer().LoginLocal(
					CreateOIDCLoginRequestMessage(identityServer.Port, oidcServer),
					(startUrl) =>
					{
						var redirectPos = startUrl.IndexOf("redirect_uri=");
						var redirectUri = startUrl.Substring(redirectPos + "redirect_uri=".Length, startUrl.IndexOf('&', redirectPos) - redirectPos - "redirect_uri=".Length).Replace("%3A", ":").Replace("%2F", "/");
						var statePos = startUrl.IndexOf("state=");
						var state = startUrl.Substring(statePos, startUrl.IndexOf('&', statePos) - statePos);
						var pretendServer = new HttpClient();
						pretendServer.GetAsync(redirectUri + "?code=F99D7E65C1029E67B2B00565FB7D32C5B4FC4DAB53B6FF5B003E338C15BCE327&scope=openid%20profile%20api%20offline_access&" + state + "&session_state=PZV2X0rn1FueCpnIYz3CUzWhM1EYnq4D2N3aB6ajtsk.D490E01EF7BE079E072FBA3A8450574A");
					},
					CancellationToken.None);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));

			if (shouldFail)
			{
				AssertEquals(OIDCLoginResponseMessage.ErrorType.Exception, response.Error);
				AssertContains("Failed to listen on prefix 'http://127.0.0.1:80/CargowiseOne/Authorize/'", response.ErrorDescription);
			}
			else
			{
				AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.None, response.Error);
			}
		}

		public void TestValidateIdentityTokenWithSecurityTokenValidationException()
		{
			var request = new OIDCLoginRequestMessage(new Uri("https://invalid.com"), "", "", null, OIDCLoginRequestMessage.LoginPrompt.Default, OIDCLoginRequestMessage.OIDCServer.Azure, "", "", 0, "Azure", Guid.NewGuid());
			var response = OIDCLoginResponseMessage.CreateSuccessResponse("any token", "any token", new DateTimeOffset());
			var loginServer = new OIDCLoginServer();
			var exception = AssertExceptionThrown<SecurityTokenValidationException>(() => _ = loginServer.ValidateIdentityToken(request, response, CancellationToken.None));
			AssertEquals("Unable to obtain configuration from https://invalid.com/, The remote name could not be resolved: 'invalid.com'", exception.Message);
		}

		public void TestLocalLoginSuccess()
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				response = new OIDCLoginServer().LoginLocal(
					CreateOIDCLoginRequestMessage(identityServer.Port),
					(startUrl) =>
					{
						var statePos = startUrl.IndexOf("state=");
						var state = startUrl.Substring(statePos, startUrl.IndexOf('&', statePos) - statePos);
						var pretendServer = new HttpClient();
						pretendServer.GetAsync(@"http://127.0.0.1:1234/?code=F99D7E65C1029E67B2B00565FB7D32C5B4FC4DAB53B6FF5B003E338C15BCE327&scope=openid%20profile%20api%20offline_access&" + state + "&session_state=PZV2X0rn1FueCpnIYz3CUzWhM1EYnq4D2N3aB6ajtsk.D490E01EF7BE079E072FBA3A8450574A");
					},
					CancellationToken.None,
					CreateOIDCLoginFactory()
					);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.None, response.Error);
		}

		public void TestLocalLoginAutoCancel()
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				Task task = null;
				var request = CreateOIDCLoginRequestMessage(identityServer.Port);
				request.TimeoutSeconds = 5;
				response = new OIDCLoginServer().LoginLocal(
					request,
					(startUrl) =>
					{
						task = Task.Run(() =>
						{
							Thread.Sleep(10 * 1000);
						});
					},
					cancellationTokenSource.Token,
					CreateOIDCLoginFactory()
					);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
				task.Wait(15 * 1000);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.Timeout, response.Error);
		}

		public void TestLocalLoginManualCancel()
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;

			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				Task task = null;
				var waitHandle = new ManualResetEvent(false);
				var responseTask = Task.Run(() => new OIDCLoginServer().LoginLocal(
					CreateOIDCLoginRequestMessage(identityServer.Port),
					(startUrl) =>
					{
						task = Task.Run(() =>
						{
							waitHandle.Set();
							Thread.Sleep(10 * 1000);
						});
					},
					cancellationTokenSource.Token,
					CreateOIDCLoginFactory()
					));

				waitHandle.WaitOne(5 * 1000);
				cancellationTokenSource.Cancel();
				response = responseTask.GetAwaiter().GetResult();
				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
				task.Wait(15 * 1000);
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.OperationCanceled, response.Error);
		}

		OIDCLoginRequestMessage CreateOIDCLoginRequestMessage(int port, OIDCLoginRequestMessage.OIDCServer oidcServer = OIDCLoginRequestMessage.OIDCServer.Generic)
		{
			return new OIDCLoginRequestMessage(
						new Uri($"https://{GetHostName()}:{port}"),
						"interactive.public",
						"",
						new[] { "openid", "profile", "offline_access" },
						OIDCLoginRequestMessage.LoginPrompt.Login,
						oidcServer,
						"",
						"",
						5,
						"EDI",
						Guid.NewGuid());
		}

		OIDCLoginFactory CreateOIDCLoginFactory()
		{
			return (webLauncher) =>
			{
				return new OIDCLogin(
					@"http://127.0.0.1:1234/",
					OIDCLoginShared.RefreshTokenPath,
					new NullLogger(),
					webLauncher
					);
			};
		}

		public void TestRemoteLoginTimeout()
		{
			// Mocking the server side authorization flow /connect/authorize is too hard so we just wait for a timeout
			OIDCLoginResponseMessage response;
			Exception[] exceptions;
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				response = new OIDCLoginServer().LoginRemote(
					new OIDCLoginRequestMessage(
						new Uri($"https://{GetHostName()}:{identityServer.Port}"),
						"interactive.public",
						"",
						new[] { "openid", "profile", "offline_access" },
						OIDCLoginRequestMessage.LoginPrompt.Login,
						OIDCLoginRequestMessage.OIDCServer.Generic,
						"",
						"",
						5,
						"EDI",
						Guid.NewGuid()),
					CancellationToken.None);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.Timeout, response.Error);
		}

		public void TestRemoteLoginAutoCancel()
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				response = new OIDCLoginServer().LoginRemote(
					new OIDCLoginRequestMessage(
						new Uri($"https://{GetHostName()}:{identityServer.Port}"),
						"interactive.public",
						"",
						new[] { "openid", "profile", "offline_access" },
						OIDCLoginRequestMessage.LoginPrompt.Login,
						OIDCLoginRequestMessage.OIDCServer.Generic,
						"",
						"",
						Int32.MaxValue / 1000,
						"EDI",
						Guid.NewGuid()),
					cancellationTokenSource.Token);

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.OperationCanceled, response.Error);
		}

		public void TestRemoteLoginManualCancel()
		{
			OIDCLoginResponseMessage response;
			Exception[] exceptions;
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var responseTask = Task.Run(() => new OIDCLoginServer().LoginRemote(
					new OIDCLoginRequestMessage(
						new Uri($"https://{GetHostName()}:{identityServer.Port}"),
						"interactive.public",
						"",
						new[] { "openid", "profile", "offline_access" },
						OIDCLoginRequestMessage.LoginPrompt.Login,
						OIDCLoginRequestMessage.OIDCServer.Generic,
						"",
						"",
						5000,
						"EDI",
						Guid.NewGuid()),
					cancellationTokenSource.Token));

				cancellationTokenSource.Cancel();
				response = responseTask.GetAwaiter().GetResult();

				identityServer.Dispose();
				exceptions = identityServer.Exceptions;
			}

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), exceptions.Select(x => x.ToString()));
			AssertEquals($"{response.Error} : {response.ErrorDescription}", OIDCLoginResponseMessage.ErrorType.OperationCanceled, response.Error);
		}

		static string GetHostName()
		{
			var serverName = System.Environment.MachineName; //host name sans domain
			var commonName = Dns.GetHostEntry(serverName).HostName;
			return commonName.ToLowerInvariant();
		}

		HttpListener Mock80PortOccupied()
		{
			try
			{
				var listener = new HttpListener();
				listener.Prefixes.Add("http://127.0.0.1/CargowiseOne/Authorize/");
				listener.Start();
				return listener;
			}
			catch (Exception)
			{
				return null; // Port is already occupied
			}
		}
	}
}
