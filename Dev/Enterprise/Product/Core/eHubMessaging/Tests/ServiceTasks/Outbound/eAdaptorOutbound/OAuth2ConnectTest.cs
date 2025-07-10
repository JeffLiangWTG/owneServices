using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Enterprise.eHubMessaging.Tests
{
	public class OAuth2ConnectTest : TestCaseWithFactory
	{
		T RunAsyncFunc<T>(Func<Task<T>> func)
		{
			T authToken = default(T);
			Exception exception = null;
			var task = Task.Run(async () => {
				try
				{
					using (HttpClientProvider.TemporaryOverrideHandler(GetMockHttpMsgHandler))
					{
						authToken = await func();
					}
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			});
			task.Wait();

			if (exception != null)
			{
				throw exception;
			}
			return authToken;
		}

		AuthToken RequestAuthTokenSync(IOAuth2Parameters config)
		{
			return RunAsyncFunc(new Func<Task<AuthToken>>(async () => await OAuth2Connect.RequestAuthToken(config, new CancellationToken())));
		}

		(AuthToken, bool) RequestTokenSync()
		{
			return RunAsyncFunc(new Func<Task<(AuthToken, bool)>>(async () => await OAuth2ConnectFactory.GetToken(Config, Notifier, new CancellationToken())));
		}

		public void TestOutboundConnect_AuthToken_Success()
		{
			var mockResponseToken = new AuthToken()
			{
				AccessToken = "abc",
				ExpiresIn = 10,
				TokenType = "Bearer",
				RefreshToken = "lookingfresh"
			};

			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			AuthToken authToken = null;
			AssertNoExceptionThrown(() => authToken = RequestAuthTokenSync(Config));

			AssertNotNull(authToken);

			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);
		}

		public void TestOutboundConnect_AuthToken_InvalidResponse()
		{
			var mockResponseToken = "invalid success response";

			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			Config.AuthorizationURL = "https://validlookingurl.com";

			OAuth2Exception exception = null;
			try
			{
				RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);
		}

		public void TestOutboundConnect_ErrorResponse_Success()
		{
			var mockResponseToken = new ErrorResponse()
			{
				Error = OAuth2Errors.EnumToString(OAuth2ErrorTypes.InvalidRequest),
				ErrorDescription = "error",
				ErrorURI = "link",
			};

			ResponseStatus = HttpStatusCode.Unauthorized;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				AuthorizationURL = "https://validlookingurl.com"
			};

			OAuth2Exception exception = null;
			try
			{
				RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);

			AssertEquals(OAuth2ErrorTypes.InvalidRequest, exception.ErrorType);

			AssertEquals(mockResponseToken.Error, exception.ErrorResponse.Error);
			AssertEquals(mockResponseToken.ErrorDescription, exception.ErrorResponse.ErrorDescription);
			AssertEquals(mockResponseToken.ErrorURI, exception.ErrorResponse.ErrorURI);
		}

		public void TestOutboundConnect_ErrorResponse_InvalidResponse()
		{
			var mockResponseToken = "invalid error response";

			ResponseStatus = HttpStatusCode.Unauthorized;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			OAuth20ConfigForTest config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				AuthorizationURL = "https://validlookingurl.com"
			};

			OAuth2Exception exception = null;
			try
			{
				RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);

			AssertEquals(OAuth2ErrorTypes.InvalidRequest, exception.ErrorType);
			AssertContains("Authorization URL returned a 401 Unauthorized response.", exception.Message);
			AssertContains("invalid error response", exception.Message);
		}

		public void TestOutboundConnect_ErrorResponse_NotFoundResponse_NoResponseContent()
		{
			var mockResponseToken = "";

			ResponseStatus = HttpStatusCode.NotFound;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			var config = new OAuth20ConfigForTest()
			{
				ClientID = "thisismyid",
				ClientSecret = "plznohacks",
				FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				AuthorizationURL = "https://validlookingurl.com"
			};

			OAuth2Exception exception = null;
			try
			{
				RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);

			AssertEquals(OAuth2ErrorTypes.InvalidRequest, exception.ErrorType);
			AssertContains("Authorization URL returned a 404 NotFound response.", exception.Message);
		}

		public void TestOutboundConnect_Request_MalformedURL()
		{
			OAuth2Exception exception = null;
			try
			{
				Config.AuthorizationURL = "I'm Malformed";
				var authToken = RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);
			AssertEquals("Malformed Authorization URL.", exception.Message);
		}

		public void TestOutboundConnect_Request_NonSecuredURL()
		{
			OAuth2Exception exception = null;
			try
			{
				Config.AuthorizationURL = "http://notsecure.com";
				var authToken = RequestAuthTokenSync(Config);
			}
			catch (OAuth2Exception ex)
			{
				exception = ex;
			}

			AssertNotNull(exception);
			AssertEquals("Authorization URL must be https.", exception.Message);
		}

		public void TestAuthorizationProviderCache()
		{
			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true,
				AuthorizationURL = "https://www.test.com"
			};
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			var mockResponseToken = new AuthToken()
			{
				AccessToken = "abc",
				ExpiresIn = 100,
				TokenType = "Bearer",
				RefreshToken = "lookingfresh"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			AuthToken authToken = null;
			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertNotNull(authToken);

			AssertEquals(eAdaptorNextLogs.NoTokenInCache(), Notifier.notifications[0]);
			AssertContains(eAdaptorNextLogs.NewToken(""), Notifier.notifications[1]);
			Notifier.Clear();

			//Should make new token 
			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);

			var mockResponseToken2 = new AuthToken()
			{
				AccessToken = "changed",
				ExpiresIn = 10000,
				TokenType = "changed",
				RefreshToken = "changed"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken2);

			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertNotNull(authToken);

			AssertContains("", Notifier.ToString());

			//Should not make new token
			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);
		}

		public void TestAuthorizationProviderCache_Expired()
		{
			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true,
				AuthorizationURL = "https://www.test.com"
			};
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			var mockResponseToken = new AuthToken()
			{
				AccessToken = "abc",
				ExpiresIn = 0,
				TokenType = "Bearer",
				RefreshToken = "lookingfresh"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			AuthToken authToken = null;
			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertNotNull(authToken);

			AssertEquals(eAdaptorNextLogs.NoTokenInCache(), Notifier.notifications[0]);
			AssertContains(eAdaptorNextLogs.NewToken(""), Notifier.notifications[1]);
			Notifier.Clear();

			//Should make new token 
			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);

			var mockResponseToken2 = new AuthToken()
			{
				AccessToken = "changed",
				ExpiresIn = 10000,
				TokenType = "changed",
				RefreshToken = "changed"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken2);

			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertNotNull(authToken);

			AssertEquals(eAdaptorNextLogs.NoTokenInCache(), Notifier.notifications[0]);
			AssertContains(eAdaptorNextLogs.NewToken(""), Notifier.notifications[1]);

			//Should make a new token as old expiresIn 0 seconds
			AssertEquals(mockResponseToken2.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken2.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken2.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken2.RefreshToken, authToken.RefreshToken);
		}

		public void TestAuthorizationProviderCache_Clear()
		{
			eAdaptorNextOutboundConfig config = new eAdaptorNextOutboundConfig()
			{
				AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
				ClientID = "IDIDIDIDIDIDIDID",
				ClientSecret = "IAmCoolerThanYou",
				IsOAuth2Enabled = true,
				AuthorizationURL = "https://www.test.com"
			};
			eAdaptorRegistry.Instance.eAdaptorNextOutbound.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

			var mockResponseToken = new AuthToken()
			{
				AccessToken = "abc",
				ExpiresIn = 10000,
				TokenType = "Bearer",
				RefreshToken = "lookingfresh"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken);

			AuthToken authToken = null;
			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertNotNull(authToken);

			AssertEquals(eAdaptorNextLogs.NoTokenInCache(), Notifier.notifications[0]);
			AssertContains(eAdaptorNextLogs.NewToken(""), Notifier.notifications[1]);
			Notifier.Clear();

			//Should make new token 
			AssertEquals(mockResponseToken.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken.RefreshToken, authToken.RefreshToken);

			OAuth2ConnectFactory.ClearTokens(Config);

			var mockResponseToken2 = new AuthToken()
			{
				AccessToken = "changed",
				ExpiresIn = 10000,
				TokenType = "changed",
				RefreshToken = "changed"
			};
			ResponseStatus = HttpStatusCode.OK;
			ResponseContent = JsonConvert.SerializeObject(mockResponseToken2);

			AssertNoExceptionThrown(() => (authToken, _) = RequestTokenSync());

			AssertEquals(eAdaptorNextLogs.NoTokenInCache(), Notifier.notifications[0]);
			AssertContains(eAdaptorNextLogs.NewToken(""), Notifier.notifications[1]);

			AssertNotNull(authToken);

			//Should make a new token as old expiresIn 0 seconds
			AssertEquals(mockResponseToken2.AccessToken, authToken.AccessToken);
			AssertEquals(mockResponseToken2.ExpiresIn, authToken.ExpiresIn);
			AssertEquals(mockResponseToken2.TokenType, authToken.TokenType);
			AssertEquals(mockResponseToken2.RefreshToken, authToken.RefreshToken);
		}

		protected override void TearDown()
		{
			OAuth2ConnectFactory.ClearTokens(Config);
		}

		public OAuth20ConfigForTest Config
		{
			get
			{
				if (config == null)
				{
					config = new OAuth20ConfigForTest()
					{
						ClientID = "thisismyid",
						ClientSecret = "plznohacks",
						FlowCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
						AuthorizationURL = "https://validlookingurl.com"
					};
				}
				return config;
			}
			set
			{
				config = value;
			}
		}
		OAuth20ConfigForTest config;

		public string ResponseContent
		{
			get
			{
				if (responseContent == null)
				{
					responseContent = "";
				}
				return responseContent;
			}
			set
			{
				responseContent = value;
			}
		}
		string responseContent;

		public HttpStatusCode ResponseStatus {
			get
			{
				return responseStatus;
			}
			set
			{
				responseStatus = value;
			}
		}
		HttpStatusCode responseStatus = HttpStatusCode.OK;

		public HttpClient httpClient => new HttpClient(GetMockHttpMsgHandler());

		HttpMessageHandler GetMockHttpMsgHandler()
		{
			var mockHttpMsgHandler = new Mock<HttpMessageHandler>();

			mockHttpMsgHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage { StatusCode = ResponseStatus, Content = new StringContent(ResponseContent) });

			return mockHttpMsgHandler.Object;
		}

		Notifier Notifier
		{
			get
			{
				if (notifier == null)
				{
					notifier = new Notifier();
				}
				return notifier;
			}
		}
		Notifier notifier;
	}
}
