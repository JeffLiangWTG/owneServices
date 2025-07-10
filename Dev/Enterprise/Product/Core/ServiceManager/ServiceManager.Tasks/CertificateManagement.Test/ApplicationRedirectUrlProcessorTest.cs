using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	[TestedType(typeof(ApplicationRedirectUrlProcessor))]
	class ApplicationRedirectUrlProcessorTest : TransactionedTestCase
	{
		public void TestApplicationRedirectUrlItemsShouldHaveUniqueApplicationNameForGeneric()
		{
			RedirectUrlItemsShouldHaveUniqueApplicationNameCore();
		}

		public void TestApplicationRedirectUrlItemsShouldHaveUniqueApplicationNameForEDI()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				RedirectUrlItemsShouldHaveUniqueApplicationNameCore();
			}
		}

		void RedirectUrlItemsShouldHaveUniqueApplicationNameCore()
		{
			var processor = new ApplicationRedirectUrlProcessor();
			var appNames = processor.ApplicationRedirectUrlItems.Select(redirectUrl => redirectUrl.ApplicationName).ToList();
			AssertContainsExactElementsInAnyOrder("Should not have duplicated application name.", appNames, appNames.Distinct());
		}

		public void TestRedirectUrlsForGenericSystem()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceUri.com"))
			using (GlowRegistry.Instance.GlowServiceExternalUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://GlowServiceExternalUri.com"))
			{
				var processor = new ApplicationRedirectUrlProcessor();
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder(new [] { "Glow Service", "Glow Service External", }, processor.ApplicationRedirectUrlItems.Select(redirectUrl => redirectUrl.ApplicationName));
					var glowServiceUri = processor.ApplicationRedirectUrlItems.FirstOrDefault(item => item.ApplicationName == "Glow Service");
					AssertEquals("glowServiceUri", "https://GlowServiceUri.com/signin-oidc", glowServiceUri?.RedirectUrl);
					AssertEquals("glowServiceUri", RedirectUrlTypes.SinglePage, glowServiceUri?.RedirectUrlType);
					var glowServiceExternalUri = processor.ApplicationRedirectUrlItems.FirstOrDefault(item => item.ApplicationName == "Glow Service External");
					AssertEquals("glowServiceExternalUri", "https://GlowServiceExternalUri.com/signin-oidc", glowServiceExternalUri?.RedirectUrl);
					AssertEquals("glowServiceExternalUri", RedirectUrlTypes.SinglePage, glowServiceExternalUri?.RedirectUrlType);
				});
			}
		}

		public void TestRedirectUrlsForEDI()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount.com"))
			{
				var processor = new ApplicationRedirectUrlProcessor();
				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder(new[] { "Glow Service", "Glow Service External", "MyAccount", }, processor.ApplicationRedirectUrlItems.Select(redirectUrl => redirectUrl.ApplicationName));
					var myAccountUri = processor.ApplicationRedirectUrlItems.FirstOrDefault(item => item.ApplicationName == "MyAccount");
					AssertEquals("myAccountUri", "https://myaccount.com/api/oidc/callback", myAccountUri?.RedirectUrl);
					AssertEquals("myAccountUri", RedirectUrlTypes.SinglePage, myAccountUri?.RedirectUrlType);
				});
			}
		}

		public void TestProcess()
		{
			var oidcConfig = OIDCConfigHelper.GetOIDCConfig();
			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testapp.com"))
			using (GlowRegistry.Instance.GlowServiceExternalUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowext.com"))
			{
				var mockIAuthenticationService = new Mock<IAuthenticationService>();
				mockIAuthenticationService
					.Setup(m => m.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
					.ReturnsAsync("accesstoken");
				var mockISystemToSystemTrustApiHelper = new Mock<ISystemToSystemTrustApiHelper>();
				StringContent postedContent = null;
				mockISystemToSystemTrustApiHelper
					.Setup(api => api.SystemToSystemTrustApiPost("application/oidcredirecturls", It.IsAny<StringContent>(), "accesstoken"))
					.Returns((string relativeUrl, StringContent content, string token) =>
					{
						postedContent = content;
						return new SystemToSystemTrustApiResponse(HttpStatusCode.OK, content.ToString());
					});

				var mockILogger = new Mock<ILogger>();
				var loggers = new List<string>();
				mockILogger.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType type, string message) => loggers.Add(message));
				var processor = new ApplicationRedirectUrlProcessor();

				CallProcess(processor, mockISystemToSystemTrustApiHelper.Object, mockIAuthenticationService.Object, mockILogger.Object);

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInExactOrder(new[] { "Start to send the redirect urls.", "Redirect urls have been sent.", }, loggers);
					AssertEquals("utf-8", postedContent?.Headers.ContentType.CharSet);
					AssertEquals("application/json", postedContent?.Headers.ContentType.MediaType);
					var jsonContent = postedContent?.ReadAsStringAsync().Result;

					var expectStringContent = """
						{"authorityUrl":"https://test.com","clientId":"","redirectUrls":[{"redirectUrl":"https://testapp.com/signin-oidc","redirectUrlType":"SPA","applicationName":"Glow Service"},{"redirectUrl":"https://glowext.com/signin-oidc","redirectUrlType":"SPA","applicationName":"Glow Service External"}]}
						""";
					AssertEquals(expectStringContent, jsonContent);
				});
			}
		}

		public void TestProcessWithFailedResponse()
		{
			var oidcConfig = OIDCConfigHelper.GetOIDCConfig();
			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testapp.com"))
			{
				var mockIAuthenticationService = new Mock<IAuthenticationService>();
				mockIAuthenticationService
					.Setup(m => m.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
					.ReturnsAsync("accesstoken");
				var mockISystemToSystemTrustApiHelper = new Mock<ISystemToSystemTrustApiHelper>();
				mockISystemToSystemTrustApiHelper
					.Setup(api => api.SystemToSystemTrustApiPost("application/oidcredirecturls", It.IsAny<StringContent>(), "accesstoken"))
					.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, "Failed on sending redirect urls"));

				var mockILogger = new Mock<ILogger>();
				var loggers = new List<string>();
				mockILogger.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType type, string message) => loggers.Add(message));
				var processor = new ApplicationRedirectUrlProcessor();

				CallProcess(processor, mockISystemToSystemTrustApiHelper.Object, mockIAuthenticationService.Object, mockILogger.Object);

				AssertContainsExactElementsInExactOrder(new[] { "Start to send the redirect urls.", "Error when sending redirect urls: Failed on sending redirect urls", }, loggers);
			}
		}

		public void TestProcessWithTokenException()
		{
			var oidcConfig = OIDCConfigHelper.GetOIDCConfig();
			using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testapp.com"))
			{
				var mockIAuthenticationService = new Mock<IAuthenticationService>();
				var testException = new InvalidOperationException("Failed to get access token - unit test");
				mockIAuthenticationService
					.Setup(authService => authService.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
					.Throws(testException);

				var mockISystemToSystemTrustApiHelper = new Mock<ISystemToSystemTrustApiHelper>();
				var mockILogger = new Mock<ILogger>();
				var loggers = new List<string>();
				mockILogger.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType type, string message) => loggers.Add(message));
				var processor = new ApplicationRedirectUrlProcessor();
				var processTask = CallProcessAsync(processor, mockISystemToSystemTrustApiHelper.Object, mockIAuthenticationService.Object, mockILogger.Object);
				var e = AssertExceptionThrown<InvalidOperationException>(() => processTask.GetAwaiter().GetResult());
				AssertEquals(testException, e);

				AssertContainsExactElementsInExactOrder(new[] { "Start to send the redirect urls.", }, loggers);
			}
		}

		void CallProcess(ApplicationRedirectUrlProcessor processor, ISystemToSystemTrustApiHelper systemToSystemTrustApiHelper, IAuthenticationService authenticationService, ILogger logger)
		{
			CallProcessAsync(processor, systemToSystemTrustApiHelper, authenticationService, logger)
				.Wait(TimeSpan.FromSeconds(10));
		}

		async Task CallProcessAsync(ApplicationRedirectUrlProcessor processor, ISystemToSystemTrustApiHelper systemToSystemTrustApiHelper, IAuthenticationService authenticationService, ILogger logger)
		{
			var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
			await processor.ProcessAsync(systemToSystemTrustApiHelper, authenticationService, logger, cancellationTokenSource.Token);
		}
	}
}
