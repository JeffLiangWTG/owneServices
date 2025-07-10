using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class RemotePrintingServiceAdaptorTest : TestCase
	{
		public void TestCredentialsWithProtectedPassword()
		{
			using (RestoreWebRequestDefaultWebProxy())
			{
				var protectedPwd = ProtectedDataHelper.Protect("pwd1");
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				config.WebServicePwd = protectedPwd;
				adaptor.SetWebServiceUrlAndCredentials(config);

				var password = ((NetworkCredential)adaptor.PrintingService_Exposed.Credentials).Password;
				CombineAssertions(() =>
				{
					AssertNotEquals("pwd1", protectedPwd);
					AssertEquals("pwd1", password);
				});
			}
		}

		public void TestAllMethodRetryCounter()
		{
			var retryProcessor = new RetryProcessorForTest();
			var config = Configurator.GetProxyDefaultSystemSettings();
			var adaptor = new RemotePrintingServiceAdaptorForTest(config, retryProcessor);

			adaptor.CheckClientUpdate();
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.CheckClientUpdate2();
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetChangedQueues("Test Server", Array.Empty<string>());
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetJobs("Test Server");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetJobs("Test Server");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetJobsCompressed("Test Server");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetJobsCompressed2("Test Server");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetWatermarkInfo();
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.SetJobFailure(Array.Empty<PrintJobFailed>());
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.Limit10Seconds_Exposed, retryProcessor.TimeLimit);

			adaptor.SetJobSuccess(Array.Empty<Guid>());
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.Limit10Seconds_Exposed, retryProcessor.TimeLimit);

			adaptor.SetJobSuccess(Array.Empty<Guid>());
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.Limit10Seconds_Exposed, retryProcessor.TimeLimit);

			adaptor.SetQueues("Test Server", Array.Empty<string>());
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.Limit10Seconds_Exposed, retryProcessor.TimeLimit);

			adaptor.SendNotificationEmail("Test subject", "Test body");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetCNSWClientApplicationSetting("Test machineName");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetTWNCATKClientSetting("Test machineName");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetCLSMSClientSetting("Test machineName");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);

			adaptor.GetJPNACCSClientSetting("Test machineName");
			AssertEquals(RetryProcessor.MaxRetries, retryProcessor.Retries);
			AssertEquals(adaptor.NoTimeLimit_Exposed, retryProcessor.TimeLimit);
		}

		class RetryProcessorForTest : RetryProcessor
		{
			public int Retries { get; private set; }
			public TimeSpan TimeLimit { get; private set; }

			protected override T Retry<T>(Delegate action, int retries, TimeSpan timeLimit, params object[] args)
			{
				Retries = retries;
				TimeLimit = timeLimit;
				return default;
			}
		}

		public void TestSetDefaultWebProxyToEmptyIfProxyNotEnabled()
		{
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				config.ProxyEnabled = false;

				var mockProxy = new Mock<IWebProxy>();
				mockProxy.Setup(p => p.GetProxy(It.IsAny<Uri>())).Returns(new Uri("http://abc.def.net"));

				WebRequest.DefaultWebProxy = mockProxy.Object;

				AssertNotNull("WebRequest.DefaultWebProxy", WebRequest.DefaultWebProxy);

				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);

				AssertNotNull("RemotePrintingService.Proxy", adaptor.PrintingService_Exposed.Proxy);
				var webProxy = adaptor.PrintingService_Exposed.Proxy as WebProxy;
				AssertNotNull("RemotePrintingService.Proxy should be WebProxy", webProxy);
				AssertNull("Proxy should be empty", webProxy.Address);

				AssertSame("WebRequest.DefaultWebProxy should not be cleared", mockProxy.Object, WebRequest.DefaultWebProxy);
			}
		}

		public void TestClientAddSoapHeaderWithVersionInfo()
		{
			using (UpdateProcessor.OverrideInstalledVersionForTest("2.16.6"))
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);
				AssertEquals("2.16.6", adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.VersionInfo);
			}
		}

		public void TestClientDotNetVersionCantBeRetreived()
		{
			using (RemotePrintingServiceAdaptor.OverrideInstalledDotNetVersionForTest(1111))
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);

				AssertContains("No match found for Dot Net release number", ErrorReporter.LastMessageReported);
				AssertContains("InstalledDotNetRelease: 1111", ErrorReporter.LastMessageReported);
				AssertEquals("Return 4.8 by default", "4.8", adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.InstalledDotNetVersion);
			}

			using (RemotePrintingServiceAdaptor.OverrideInstalledDotNetVersionForTest(393295))
			using (RestoreWebRequestDefaultWebProxy())
			{
				ErrorReporter.ClearLastReported();
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);

				AssertNull("No reported error", ErrorReporter.LastExceptionReported);
				AssertEquals("Should return InstalledDotNetVersion", "4.6", adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.InstalledDotNetVersion);
			}

			using (RemotePrintingServiceAdaptor.OverrideInstalledDotNetVersionForTest("Test"))
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);

				AssertContains("Dot Net release number is not an integer", ErrorReporter.LastMessageReported);
				AssertContains("InstalledDotNetRelease: Test", ErrorReporter.LastMessageReported);
				AssertEquals("Return 4.8 by default", "4.8", adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.InstalledDotNetVersion);
			}
		}

		public void TestClientAddSoapHeaderWithDotNetVersion()
		{
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);

				var defaultVersion = new Version("4.8");
				var installedDotNetVersion = new Version(adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.InstalledDotNetVersion);
				Assert("SoapHeader should have .Net version default 4.8 or greater", installedDotNetVersion >= defaultVersion);
			}
		}

		public void TestClientAddSoapHeaderWithOsVersion()
		{
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				config.WebServiceUrl = "http://cargowise.com";
				adaptor.SetWebServiceUrlAndCredentials(config);
				AssertNotNullOrEmpty(adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue.OSVersion);
			}
		}

		public void TestPrepareSoapHeader()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			config.LocalMachineName = "TestMachineName";
			config.WebServiceUrl = "http://cargowise.com";
			var adaptor = new RemotePrintingServiceAdaptorForTest(config);

			using (UpdateProcessor.OverrideInstalledVersionForTest("1.2.3"))
			using (RestoreWebRequestDefaultWebProxy())
			{
				adaptor.SetWebServiceUrlAndCredentials(config);
				var soapHeader = adaptor.PrintingService_Exposed.RemotePrintingSoapHeaderValue;

				AssertEquals("1.2.3", soapHeader.VersionInfo);
				AssertEquals("TestMachineName", soapHeader.LocalMachineName);
			}
		}

		IDisposable RestoreWebRequestDefaultWebProxy()
		{
			var originProxy = WebRequest.DefaultWebProxy;
			return new DisposableAction(() => WebRequest.DefaultWebProxy = originProxy);
		}

		#region TestCookiesInWebRequest

		public void TestPrintingServiceHasCookiesContainer()
		{
			var config = Configurator.GetProxyDefaultSystemSettings();
			config.LocalMachineName = "TestMachineName";
			config.WebServiceUrl = "http://cargowise.com";

			var adaptor = new RemotePrintingServiceAdaptorForTest(config);

			AssertNotNull("CookieContainer on PrintingService should be initialized", adaptor.PrintingService_Exposed.CookieContainer);
		}

		#endregion

		public void TestSetupRedirectResponseProcessor()
		{
			using (RestoreWebRequestDefaultWebProxy())
			{
				var config = Configurator.GetProxyDefaultSystemSettings();
				config.LocalMachineName = "TestMachineName";
				config.WebServiceUrl = "http://cargowise.com";
				var logs = new List<string>();

				var adaptor = new RemotePrintingServiceAdaptorForTest(config);
				adaptor.SetWebServiceUrlAndCredentials(config, (message) => logs.Add(message));

				AssertEquals("RemotePrintingService AllowAutoRedirect should set to false", false, adaptor.PrintingService_Exposed.AllowAutoRedirect);
				AssertEquals("SoapVersion should be set to Soap12", SoapProtocolVersion.Soap12, adaptor.PrintingService_Exposed.SoapVersion);
				AssertEquals("http://cargowise.com/", adaptor.ResponseProcessor.ServiceUrl);
				AssertEquals("http://cargowise.com/", adaptor.PrintingService_Exposed.Url);

				adaptor.ResponseProcessor.LogInfo("Jerry Test Message");

				AssertEquals("Jerry Test Message", string.Join("\r\n", logs));
			}
		}

		public void TestIsSupportUser()
		{
			Assert("Should CWSupport user", new RemotePrintingServiceAdaptor(new WebClientConfiguration { WebServiceUser = "CWSupport-invalidtoken" }).IsSupportUser);
			Assert("Should not CWSupport user", !new RemotePrintingServiceAdaptor(new WebClientConfiguration { WebServiceUser = "JerryTestUser" }).IsSupportUser);
		}
	}
	class RemotePrintingServiceAdaptorForTest : RemotePrintingServiceAdaptor
	{
		public RemotePrintingServiceAdaptorForTest(WebClientConfiguration config, INotifications notifications = null)
			: base(config, notifications)
		{
		}

		public RemotePrintingServiceAdaptorForTest(WebClientConfiguration config, RetryProcessor retryProcessor)
			: base(config, retryProcessor)
		{
		}

		public RemotePrintingService PrintingService_Exposed => base.printingService;

		public TimeSpan NoTimeLimit_Exposed => NoTimeLimit;

		public TimeSpan Limit10Seconds_Exposed => Limit10Seconds;
	}
}
