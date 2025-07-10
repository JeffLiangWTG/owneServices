using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(TokenAuthOnboardingService))]
	[TestRequiresAdministrativePrivileges("Need to create http listener")]
	class TokenAuthOnboardingServiceTest : TransactionedTestCase
	{
		public void TestGetOidcConfig()
		{
			var mockISystemToSystemTrustService = new Mock<SystemToSystemTrustService>();
			mockISystemToSystemTrustService.Setup(service => service.GetAccessToken()).Returns("mockaccesstoken");

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest = 100;

			var expectedData = new TokenAuthOnboardingDataResponse()
			{
				AuthorityUrl = "https://test.com",
				ConfigurationIdentifier = Guid.NewGuid().ToString(),
				ClaimMappingName = "mappingname",
				ClaimMappingIdentifier = "GlbStaff.GS_LoginName",
				DomainHint = "Azure"
			};

			using (var mockService = new MockTokenAuthOnboardingService("mockaccesstoken", 100, expectedData))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockService.Endpoint))
			{
				var tokenAuthOnboardingService = new TokenAuthOnboardingService(mockISystemToSystemTrustService.Object);
				var config = tokenAuthOnboardingService.FetchOidcConfig();
				AssertEquals(expectedData.AuthorityUrl, config.AuthorityUrl);
				AssertEquals(expectedData.ConfigurationIdentifier, config.ConfigurationIdentifier);
				AssertEquals(expectedData.ClaimMappingName, config.ClaimMappingName);
				AssertEquals(expectedData.ClaimMappingIdentifier, config.ClaimMappingIdentifier);
				AssertEquals(expectedData.DomainHint, config.DomainHint);
			}
		}

		public void TestEnableTokenAuthentication()
		{
			var mockISystemToSystemTrustService = new Mock<SystemToSystemTrustService>();
			mockISystemToSystemTrustService.Setup(service => service.GetAccessToken()).Returns("mockaccesstoken");

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest = 100;

			using (var mockService = new MockTokenAuthOnboardingService("mockaccesstoken", 100, null))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockService.Endpoint))
			{
				var tokenAuthOnboardingService = new TokenAuthOnboardingService(mockISystemToSystemTrustService.Object);
				var enable = tokenAuthOnboardingService.EnableTokenAuthentication();
				Assert(enable);
			}
		}

		public void TestInvalidToken()
		{
			var mockISystemToSystemTrustService = new Mock<SystemToSystemTrustService>();
			mockISystemToSystemTrustService.Setup(service => service.GetAccessToken()).Returns("invalidToken");

			using (var mockService = new MockTokenAuthOnboardingService("mockaccesstoken", 100, null))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockService.Endpoint))
			{
				var tokenAuthOnboardingService = new TokenAuthOnboardingService(mockISystemToSystemTrustService.Object);
				var exception = AssertExceptionThrown<TokenAuthOnboardingApiException>(() => tokenAuthOnboardingService.FetchOidcConfig());
				AssertEquals("Unauthorized", exception.Message);
			}
		}

		public void TestInvalidDatabase()
		{
			var mockISystemToSystemTrustService = new Mock<SystemToSystemTrustService>();
			mockISystemToSystemTrustService.Setup(service => service.GetAccessToken()).Returns("mockaccesstoken");

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest = 80;

			using (var mockService = new MockTokenAuthOnboardingService("mockaccesstoken", 100, null))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockService.Endpoint))
			{
				var tokenAuthOnboardingService = new TokenAuthOnboardingService(mockISystemToSystemTrustService.Object);
				var exception = AssertExceptionThrown<TokenAuthOnboardingApiException>(() => tokenAuthOnboardingService.FetchOidcConfig());
				AssertEquals("invalid database", exception.Message);
			}
		}

		public void TestDataNotReady()
		{
			var mockISystemToSystemTrustService = new Mock<SystemToSystemTrustService>();
			mockISystemToSystemTrustService.Setup(service => service.GetAccessToken()).Returns("mockaccesstoken");

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest = 100;

			using (var mockService = new MockTokenAuthOnboardingService("mockaccesstoken", 100, null))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mockService.Endpoint))
			{
				var tokenAuthOnboardingService = new TokenAuthOnboardingService(mockISystemToSystemTrustService.Object);
				var exception = AssertExceptionThrown<TokenAuthOnboardingApiException>(() => tokenAuthOnboardingService.FetchOidcConfig());
				AssertEquals("data is not ready", exception.Message);
			}
		}
	}

	class MockTokenAuthOnboardingService : IDisposable
	{
		public MockTokenAuthOnboardingService(string validToken, int validDBNum, TokenAuthOnboardingDataResponse expectedOnboardingData)
		{
			var port = 29000;
			do
			{
				Endpoint = "http://localhost:" + port;
				listener = new HttpListener();
				listener.Prefixes.Add(Endpoint + "/api/TokenAuthOnBoarding/oidcconfig/");

				try
				{
					listener.Start();
				}
				catch (HttpListenerException) when (port <= 29050)
				{
					port++;
				}
			}
			while (!listener.IsListening);

			taskListener = Task.Run(() =>
			{
				var context = listener.GetContextAsync().Result;

				var accessToken = context.Request.Headers["Authorization"].Split(' ')[1];

				var dbNumber = int.Parse(context.Request.Url.AbsolutePath.Substring(context.Request.Url.AbsolutePath.LastIndexOf("/") + 1));

				using (var listenerResponse = context.Response)
				{
					if (accessToken != validToken)
					{
						var bytes = Encoding.UTF8.GetBytes("Unauthorized");
						listenerResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
						listenerResponse.ContentLength64 = bytes.Length;
						listenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
						listenerResponse.Close();
						return;
					}

					if (dbNumber != validDBNum)
					{
						var bytes = Encoding.UTF8.GetBytes("invalid database");
						listenerResponse.StatusCode = (int)HttpStatusCode.BadRequest;
						listenerResponse.ContentLength64 = bytes.Length;
						listenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
						listenerResponse.Close();
						return;
					}

					if (context.Request.Url.AbsolutePath.Contains("/api/TokenAuthOnBoarding/oidcconfig/fetch"))
					{
						if (expectedOnboardingData == null)
						{
							var bytes = Encoding.UTF8.GetBytes("data is not ready");
							listenerResponse.StatusCode = (int)HttpStatusCode.BadRequest;
							listenerResponse.ContentLength64 = bytes.Length;
							listenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
							listenerResponse.Close();
							return;
						}
						else
						{
							var response = JsonConvert.SerializeObject(expectedOnboardingData);

							var bytes = Encoding.UTF8.GetBytes(response);
							listenerResponse.StatusCode = (int)HttpStatusCode.OK;
							listenerResponse.ContentLength64 = bytes.Length;
							listenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
							listenerResponse.Close();
							return;
						}
					}

					if (context.Request.Url.AbsolutePath.Contains("/api/TokenAuthOnBoarding/oidcconfig/enable"))
					{
						listenerResponse.StatusCode = (int)HttpStatusCode.OK;
						listenerResponse.Close();
						return;
					}
					
					{
						var bytes = Encoding.UTF8.GetBytes("unknownrequest");
						listenerResponse.StatusCode = (int)HttpStatusCode.BadRequest;
						listenerResponse.ContentLength64 = bytes.Length;
						listenerResponse.OutputStream.Write(bytes, 0, bytes.Length);
						listenerResponse.Close();
						return;
					}
				}
			});
		}

		public string Endpoint { get; private set; }

		readonly Task taskListener;
		readonly HttpListener listener;

		public void Dispose()
		{
			listener.Stop();
			if (!taskListener.IsCompleted)
			{
				taskListener.Wait(TimeSpan.FromSeconds(30));
			}
		}
	}
}
