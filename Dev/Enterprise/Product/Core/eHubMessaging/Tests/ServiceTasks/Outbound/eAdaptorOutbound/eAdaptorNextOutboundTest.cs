using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.Business.DownloadHandler;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TestHelpers.MockServer;

namespace Enterprise.eHubMessaging.Tests
{
	public class eAdaptorNextOutboundTest : TestCaseWithFactory
	{
		#region Mock OAuth2 Server

		public DynamicMockServer CreateOAuth2Server()
		{
			return new DynamicMockServer("Mock OAuth2 Server");
		}

		#endregion

		#region Mock Resource Server

		public DynamicMockServer CreateBasicResourceServer(string mockServerName, int statusCode)
		{
			var mockDynamicServer = new DynamicMockServer(mockServerName);
			mockDynamicServer.AddRoute("POST", "/private", (p) => {
				var authHeader = p.Headers["Authorization"];
				if (authHeader.StartsWith("Basic"))
				{
					var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
					var encoding = Encoding.GetEncoding("iso-8859-1");
					var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
					var separatorIndex = usernamePassword.IndexOf(':');

					var username = usernamePassword.Substring(0, separatorIndex);
					var password = usernamePassword.Substring(separatorIndex + 1);

					return new MockResponseJson($"Basic {username}:{password}", statusCode);
				}
				else if (authHeader.StartsWith("Bearer"))
				{
					var token = authHeader.Substring("Bearer ".Length).Trim();
					return new MockResponseJson($"Bearer {token}", statusCode);
				}

				return new MockResponseJson("Invalid Authorization", statusCode);
			});

			mockDynamicServer.AddRoute("POST", "/public", (p) => {
				return new MockResponseJson($"Public route", statusCode);
			});

			return mockDynamicServer;
		}

		public DynamicMockServer CreateBasicResourceServer(int statusCode)
		{
			return CreateBasicResourceServer("Mock Resource Server", statusCode);
		}

		#endregion

		#region Direct OAuth2Connect Tests 

		public void TestOAuth2Connect(OAuth20ConfigForTest config, AuthToken mockResponseToken, int statusCode)
		{
			AuthToken authToken = null;
			var cancellationToken = new CancellationToken();
			using (var identityServer = CreateOAuth2Server())
			{
				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
					return new MockResponseJson(mockResponse, statusCode);
				});

				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				var task = Task.Run(async () => { authToken = await OAuth2Connect.RequestAuthToken(config, cancellationToken); });
				task.Wait();
			}
			AssertNotNull(authToken);

			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOAuth2Connect_ClientCredentials()
		{
			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
			};
			var mockResponseToken = new AuthToken()
			{
				AccessToken = Guid.NewGuid().ToString(),
				ExpiresIn = 86400,
				TokenType = "Bearer ",
				RefreshToken = Guid.NewGuid().ToString()
			};
			TestOAuth2Connect(config, mockResponseToken, 200);
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOAuth2Connect_Password()
		{
			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.Password,
				Username = "abc",
				Password = "123"
			};
			var mockResponseToken = new AuthToken()
			{
				AccessToken = Guid.NewGuid().ToString(),
				ExpiresIn = 86400,
				TokenType = "Bearer ",
				RefreshToken = Guid.NewGuid().ToString()
			};
			TestOAuth2Connect(config, mockResponseToken, 200);
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOAuth2Connect_ClientCredentials_Timeout()
		{
			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
			};

			var mockResponseToken = new AuthToken()
			{
				AccessToken = Guid.NewGuid().ToString(),
				ExpiresIn = 86400,
				TokenType = "Bearer ",
				RefreshToken = Guid.NewGuid().ToString(),
			};

			AuthToken authToken = null;
			Exception exception = null;
			var cancellationToken = new CancellationToken();

			using (var identityServer = CreateOAuth2Server())
			{
				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
					return new MockResponseJson(mockResponse, 200);
				});

				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				var task = Task.Run(async () => {
					try
					{
						using (HttpClientProvider.TemporaryOverrideTimeout(TimeSpan.FromTicks(1)))
						{
							authToken = await OAuth2Connect.RequestAuthToken(config, cancellationToken);
						}
					}
					catch (Exception ex)
					{
						exception = ex;
					}
				});
				task.Wait();

				AssertNotNull(exception);
				AssertEquals(typeof(OAuth2Exception), exception.GetType());
				AssertEquals("The request timed out.", exception.Message);
			}
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOAuth2Connect_Unauthorized()
		{
			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				RefreshToken = Guid.NewGuid().ToString(),
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials
			};

			var mockResponseToken = new ErrorResponse()
			{
				Error = OAuth2Errors.EnumToString(OAuth2ErrorTypes.InvalidRequest),
				ErrorDescription = "error",
				ErrorURI = "link",
			};
			var mockResponse = JsonConvert.SerializeObject(mockResponseToken);

			AuthToken authToken = null;
			Exception exception = null;
			var cancellationToken = new CancellationToken();
			using (var identityServer = new DynamicMockServer("OAuth Server"))
			{
				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					return new MockResponseJson(mockResponse, 401);
				});
				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				var task = Task.Run(async () => {
					try
					{
						authToken = await OAuth2Connect.RequestAuthToken(config, cancellationToken);
					}
					catch (Exception ex)
					{
						exception = ex;
					}
				});
				task.Wait();

				AssertEquals(typeof(OAuth2Exception), exception.GetType());

				var oAuth2Exception = (OAuth2Exception)exception;
				AssertEquals(OAuth2ErrorTypes.InvalidRequest, oAuth2Exception.ErrorType);
				AssertEquals("error", oAuth2Exception.ErrorResponse.ErrorDescription);
				AssertEquals("link", oAuth2Exception.ErrorResponse.ErrorURI);
			}
			OAuth2ConnectFactory.ClearTokens(config);
		}

		#endregion

		#region E2E eAdaptorNextOutbound Tests (using EAM)

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_NoEDIMessage()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			var interchange = TestInterchangeMessage.GetNew(TestHelpers.ValidCompanyForTest(Factory).FirstActiveBranch.PK, from: "FABLOVGOB", to: "ARCFOOBAR", receiveTransmit: "TRX", factory: Factory, onlyCreateInterchange: true);
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_Status = "AQU";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange.EI_BodyText = "Noodle noodle chicken stroodle";

			Factory.Save();

			using (var mockServer = new DynamicMockServer())
			{
				mockServer.AddRoute("POST", "/something", (p) => MockResponseFactory.OkJson(""));

				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockServer.GetFullUrl("something"));

				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertCollectionContains("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications);
				AssertEquals(1, mockServer.Requests.Count);

				AssertCustomHeaders(mockServer.Requests[0].Headers, interchange.EI_To, false);
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_OAuth2Registry_Success()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true
			};

			var interchange = eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var identityServer = CreateOAuth2Server())
			using (var resourceServer = CreateBasicResourceServer(200))
			{
				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockResponseToken = new AuthToken()
					{
						AccessToken = Guid.NewGuid().ToString(),
						ExpiresIn = 86400,
						TokenType = "Bearer ",
						RefreshToken = Guid.NewGuid().ToString()
					};
					var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
					return new MockResponseJson(mockResponse, 200);
				});

				var eamURL = resourceServer.GetFullUrl("private");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);

				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				var index = notifier.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
				AssertNotEquals(-1, index);
				AssertContains(eAdaptorNextLogs.NewToken(""), notifier.notifications[index + 1]);
				AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications[index + 2]);
				AssertEquals(1, resourceServer.Requests.Count);

				AssertCustomHeaders(resourceServer.Requests[0].Headers, interchange.EI_To, false);
			}
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_SOAP_Success()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SOAP");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var resourceServer = CreateBasicResourceServer(400))
			{
				var eamURL = resourceServer.GetFullUrl("public");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);
				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertContains("(400) Bad Request.", notifier.ToString());
				AssertEquals(1, resourceServer.Requests.Count);
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_400()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var resourceServer = CreateBasicResourceServer(400))
			{
				var eamURL = resourceServer.GetFullUrl("public");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);
				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertContains("Address did not return success status.", notifier.ToString());
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]

		public void TestOutbound_REST_504()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			var interchange = eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var resourceServer = CreateBasicResourceServer(504))
			{
				var eamURL = resourceServer.GetFullUrl("public");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);
				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertContains("Address did not return success status.", notifier.ToString());
				interchange.Reload();
				AssertEquals(EDIInterchangeStatusList.Codes.eAdaptorQueued, interchange.EI_Status);
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_Timeout()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);

			using (var resourceServer = CreateBasicResourceServer(400))
			{
				resourceServer.AddRoute("POST", "/exception", (r) => throw new TaskCanceledException("task cancelled"));

				var eamURL = resourceServer.GetFullUrl("exception");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);
				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertContains("A task was canceled", notifier.ToString());
			}
		}

		public void TestOutbound_REST_Abort()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var resourceServer = CreateBasicResourceServer(400))
			{
				resourceServer.AddRoute("POST", "/exception", (r) => throw new WebException("request aborted"));

				var eamURL = resourceServer.GetFullUrl("exception");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);
				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

				AssertContains("The underlying connection was closed", notifier.ToString());
			}
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_OAuth2Registry_IdentityFailThenConnect()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true
			};

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			
			using (var identityServer = CreateOAuth2Server())
			using (var resourceServer = CreateBasicResourceServer(200))
			{
				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockErrorResponseToken = new ErrorResponse()
					{
						Error = OAuth2Errors.EnumToString(OAuth2ErrorTypes.InvalidRequest),
						ErrorDescription = "error",
						ErrorURI = "link",
					};
					var mockErrorResponse = JsonConvert.SerializeObject(mockErrorResponseToken);
					return new MockResponseJson(mockErrorResponse, 400);
				});

				var eamURL = resourceServer.GetFullUrl("private");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);

				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier, true);

				var index = notifier.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
				AssertNotEquals(-1, index);
				AssertContains(eAdaptorNextLogs.ErrorOccurredWhenClaimingToken("error"), notifier.notifications[index + 1]);

				AssertContains("eHubAdapterException: Exception thrown when trying to connect to endpoint.", notifier.ToString());
				AssertEquals(0, resourceServer.Requests.Count);

				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockResponseToken = new AuthToken()
					{
						AccessToken = Guid.NewGuid().ToString(),
						ExpiresIn = 86400,
						TokenType = "Bearer ",
						RefreshToken = Guid.NewGuid().ToString()
					};
					var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
					return new MockResponseJson(mockResponse, 200);
				});

				var notifier2 = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier2, true);

				var index2 = notifier2.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
				AssertNotEquals(-1, index2);
				AssertContains(eAdaptorNextLogs.NewToken(""), notifier2.notifications[index2 + 1]);
				AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier2.notifications[index2 + 2]);

				AssertEquals(1, resourceServer.Requests.Count);
			}
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_OAuth2Registry_CacheToken()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "REST");
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true
			};

			eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			using (var identityServer = CreateOAuth2Server())
			using (var resourceServer = new DynamicMockServer("Mock Resource Server"))
			{
				resourceServer.AddRoute("POST", "/route", (p) => {
					return new MockResponseJson("", 200);
				});

				identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
					var mockResponseToken = new AuthToken()
					{
						AccessToken = Guid.NewGuid().ToString(),
						ExpiresIn = 86400,
						TokenType = "Bearer ",
						RefreshToken = Guid.NewGuid().ToString()
					};
					var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
					return new MockResponseJson(mockResponse, 200);
				});

				var eamURL = resourceServer.GetFullUrl("route");
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eamURL);

				config.AuthorizationURL = identityServer.GetFullUrl("oauth/oauth2/token");
				eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier, true); //Send one message to cache a token

				var index = notifier.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
				AssertNotEquals(-1, index);
				AssertContains(eAdaptorNextLogs.NewToken(""), notifier.notifications[index + 1]);
				AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications[index + 2]);

				AssertEquals(1, resourceServer.Requests.Count);

				eAdaptorNextTestHelper.AddSampleInterchange(Factory);

				int count = 0;
				resourceServer.AddRoute("POST", "/route", (p) => {
					count++;
					if (count == 1)
					{
						return new MockResponseJson("", 400);
					}
					else
					{
						return new MockResponseJson("", 200);
					}
				});

				var notifier2 = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier2, true);

				var index2 = notifier2.notifications.IndexOf(eAdaptorNextLogs.TokenCacheCleared());
				AssertNotEquals(-1, index2);
				AssertEquals(eAdaptorNextLogs.NoTokenInCache(), notifier2.notifications[index2 + 1]);
				AssertContains(eAdaptorNextLogs.NewToken(""), notifier2.notifications[index2 + 2]);
				AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier2.notifications[index2 + 3]);

				AssertEquals(3, resourceServer.Requests.Count); // This is important shows 3 requests
			}
			OAuth2ConnectFactory.ClearTokens(config);
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_OAuth2_Success()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

				var accessToken = Guid.NewGuid().ToString();

				using (var identityServer = CreateOAuth2Server())
				using (var resourceServer = CreateBasicResourceServer(200))
				{
					identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
						var mockResponseToken = new AuthToken()
						{
							AccessToken = accessToken,
							ExpiresIn = 86400,
							TokenType = "Bearer ",
							RefreshToken = Guid.NewGuid().ToString()
						};
						var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
						return new MockResponseJson(mockResponse, 200);
					});

					var party = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"Mr Test Client",
						resourceServer.GetFullUrl("private"),
						EDICommunicationAuthModesList.Codes.OAuthAuthentication,
						identityServer.GetFullUrl("oauth/oauth2/token"),
						EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials
					);

					var interchange = CreateInterchangeWithEDIClient(party.OutboundConfig);

					Factory.Save();

					var notifier = new Notifier();
					eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

					var index = notifier.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
					AssertNotEquals(-1, index);
					AssertContains(eAdaptorNextLogs.NewToken(""), notifier.notifications[index + 1]);
					AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications[index + 2]);
					AssertEquals(1, resourceServer.Requests.Count);

					AssertCustomHeaders(resourceServer.Requests[0].Headers, interchange.EI_To, true, party.OutboundConfig.Party.ECP_Name);

					var authHeader = resourceServer.Requests[0].Headers["Authorization"];

					var token = authHeader.Substring("Bearer ".Length).Trim();

					AssertEquals(accessToken, token);
				}
			});
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_OAuth2_Success_Certificates()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

				var accessToken = Guid.NewGuid().ToString();

				using (var identityServer = CreateOAuth2Server())
				using (var resourceServer = CreateBasicResourceServer(200))
				{
					identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
						var mockResponseToken = new AuthToken()
						{
							AccessToken = accessToken,
							ExpiresIn = 86400,
							TokenType = "Bearer ",
							RefreshToken = Guid.NewGuid().ToString()
						};
						var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
						return new MockResponseJson(mockResponse, 200);
					});

					var party = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"EDIClientName",
						resourceServer.GetFullUrl("private"),
						EDICommunicationAuthModesList.Codes.OAuthAuthentication,
						identityServer.GetFullUrl("oauth/oauth2/token"),
						EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate
					);

					var interchange = CreateInterchangeWithEDIClient(party.OutboundConfig);

					Factory.Save();

					var notifier = new Notifier();
					eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

					var index = notifier.notifications.IndexOf(eAdaptorNextLogs.NoTokenInCache());
					AssertNotEquals(-1, index);
					AssertContains(eAdaptorNextLogs.NewToken(""), notifier.notifications[index + 1]);
					AssertEquals("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications[index + 2]);
					AssertEquals(1, resourceServer.Requests.Count);

					var authHeader = resourceServer.Requests[0].Headers["Authorization"];

					var token = authHeader.Substring("Bearer ".Length).Trim();

					AssertEquals(accessToken, token);
				}
			});
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_Basic_Success()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

				using (var resourceServer = CreateBasicResourceServer(200))
				{
					var party = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"Mr Test Client",
						resourceServer.GetFullUrl("private"),
						EDICommunicationAuthModesList.Codes.BasicAuthentication
					);
					party.OutboundConfig.Auth.ECA_Username = "Abc";
					party.OutboundConfig.Auth.ECA_Password = "123";

					var interchange = CreateInterchangeWithEDIClient(party.OutboundConfig);

					Factory.Save();

					var notifier = new Notifier();
					eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

					AssertCollectionContains("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications);
					AssertEquals(1, resourceServer.Requests.Count);

					AssertCustomHeaders(resourceServer.Requests[0].Headers, interchange.EI_To, true, party.ECP_Name);

					var authHeader = resourceServer.Requests[0].Headers["Authorization"];

					var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
					var encoding = Encoding.GetEncoding("iso-8859-1");
					var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
					var separatorIndex = usernamePassword.IndexOf(':');

					var username = usernamePassword.Substring(0, separatorIndex);
					var password = usernamePassword.Substring(separatorIndex + 1);

					AssertEquals(party.OutboundConfig.Auth.ECA_Username, username);
					AssertEquals(party.OutboundConfig.Auth.ECA_Password, password);
				}
			});
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_NoAuth_Success()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

				using (var resourceServer = CreateBasicResourceServer(200))
				{
					var party = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"Mr Test Clien",
						resourceServer.GetFullUrl("public"),
						EDICommunicationAuthModesList.Codes.NoAuthentication
					);

					var interchange = CreateInterchangeWithEDIClient(party.OutboundConfig);

					Factory.Save();

					var notifier = new Notifier();
					eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

					AssertCollectionContains("1 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications);
					AssertEquals(1, resourceServer.Requests.Count);

					AssertCustomHeaders(resourceServer.Requests[0].Headers, interchange.EI_To, true, party.ECP_Name);
				}
			});
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_OAuth2_Multi_Success()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

				using (var identityServer = CreateOAuth2Server())
				using (var resourceServer = CreateBasicResourceServer("Resource Server 1", 200))
				using (var resourceServer2 = CreateBasicResourceServer("Resource Server 2", 200))
				{
					identityServer.AddRoute("POST", "/oauth/oauth2/token", (p) => {
						var mockResponseToken = new AuthToken()
						{
							AccessToken = Guid.NewGuid().ToString(),
							ExpiresIn = 86400,
							TokenType = "Bearer ",
							RefreshToken = Guid.NewGuid().ToString()
						};
						var mockResponse = JsonConvert.SerializeObject(mockResponseToken);
						return new MockResponseJson(mockResponse, 200);
					});

					var party1 = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"Mr Test Client 1",
						resourceServer.GetFullUrl("private"),
						EDICommunicationAuthModesList.Codes.OAuthAuthentication,
						identityServer.GetFullUrl("oauth/oauth2/token"),
						EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials
					);

					var interchange1party1 = CreateInterchangeWithEDIClient(party1.OutboundConfig);
					var interchange2party1 = CreateInterchangeWithEDIClient(party1.OutboundConfig);
					var interchange3party1 = CreateInterchangeWithEDIClient(party1.OutboundConfig);

					var party2 = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
						"Mr Test Client 2",
						resourceServer2.GetFullUrl("private"),
						EDICommunicationAuthModesList.Codes.OAuthAuthentication,
						identityServer.GetFullUrl("oauth/oauth2/token"),
						EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials
					);

					var interchange1party2 = CreateInterchangeWithEDIClient(party2.OutboundConfig);
					var interchange2party2 = CreateInterchangeWithEDIClient(party2.OutboundConfig);
					var interchange3party2 = CreateInterchangeWithEDIClient(party2.OutboundConfig);

					Factory.Save();

					var notifier = new Notifier();
					eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);

					var claimTokenOccurrences = Enumerable.Range(0, notifier.notifications.Count)
						.Where(i => notifier.notifications[i] == eAdaptorNextLogs.NoTokenInCache())
						.ToList();
					AssertEquals(2, claimTokenOccurrences.Count);
					foreach (var index in claimTokenOccurrences)
					{
						AssertContains(eAdaptorNextLogs.NewToken(""), notifier.notifications[index + 1]);
						AssertEquals("3 interchange(s) sent to eAdaptor Outbound Messages.", notifier.notifications[index + 2]);
					}

					AssertEquals(3, resourceServer.Requests.Count);
					AssertEquals(3, resourceServer2.Requests.Count);

					AssertCustomHeaders(resourceServer.Requests[0].Headers, interchange1party1.EI_To, true, party1.OutboundConfig.Party.ECP_Name);
					AssertCustomHeaders(resourceServer.Requests[1].Headers, interchange2party1.EI_To, true, party1.OutboundConfig.Party.ECP_Name);
					AssertCustomHeaders(resourceServer.Requests[2].Headers, interchange3party1.EI_To, true, party1.OutboundConfig.Party.ECP_Name);
					AssertCustomHeaders(resourceServer2.Requests[0].Headers, interchange1party2.EI_To, true, party2.OutboundConfig.Party.ECP_Name);
					AssertCustomHeaders(resourceServer2.Requests[1].Headers, interchange2party2.EI_To, true, party2.OutboundConfig.Party.ECP_Name);
					AssertCustomHeaders(resourceServer2.Requests[2].Headers, interchange3party2.EI_To, true, party2.OutboundConfig.Party.ECP_Name);
				}
			});
		}

		[TestRequiresAdministrativePrivileges("Adding Trusted Self Signed Certificate")]
		public void TestOutbound_REST_EDIClients_WithoutEAdaptorNextEnabled()
		{
			DataRegistry.Instance.RawRegistry.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			eAdaptorRegistry.Instance.OutboundTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);

			using (var resourceServer = CreateBasicResourceServer(200))
			{
				var party = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory,
					"Mr Test Client",
					resourceServer.GetFullUrl("private"),
					EDICommunicationAuthModesList.Codes.BasicAuthentication
				);
				party.OutboundConfig.Auth.ECA_Username = "Abc";
				party.OutboundConfig.Auth.ECA_Password = "123";

				var interchange = CreateInterchangeWithEDIClient(party.OutboundConfig);
				interchange.EI_InterchangeNum = "1234567890";

				Factory.Save();

				var notifier = new Notifier();
				eAdaptorNextTestHelper.RunEAdaptorOutboundServiceTask(notifier);
				interchange.Reload();

				AssertCollectionContains("eAdaptorNext features are deactivated, please contact WiseTech Global for further information.", notifier.notifications);
				AssertCollectionContains("Interchange(s) Rejected (1234567890).", notifier.notifications);
				AssertEquals(0, resourceServer.Requests.Count);
				AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			}
		}

		void AssertCustomHeaders(Dictionary<string, string> headers, string expectedRecipientID, bool containsClientName, string expectedEdiClientName = null)
		{
			Assert("eAdaptor Outbound Message request contains header eAdaptor-RecipientID", headers.ContainsKey("eAdaptor-RecipientID"));
			AssertEquals(expectedRecipientID, headers["eAdaptor-RecipientID"]);
			if (!containsClientName)
			{
				Assert("eAdaptor Outbound Message request does not contain header eAdaptor-EDIClientName", !headers.ContainsKey("eAdaptor-EDIClientName"));
			} else
			{
				Assert("eAdaptor Outbound Message request contains header eAdaptor-EDIClientName", headers.ContainsKey("eAdaptor-EDIClientName"));
				AssertEquals(expectedEdiClientName, headers["eAdaptor-EDIClientName"]);
			}
		}

		EDIInterchange CreateInterchangeWithEDIClient(EDICommunicationPartyConfig partyConfig)
		{
			var interchange = eAdaptorNextTestHelper.AddSampleInterchange(Factory);
			interchange.EI_ECC_CommunicationPartyConfig = partyConfig.PK;
			return interchange;
		}

		#endregion
	}
}
