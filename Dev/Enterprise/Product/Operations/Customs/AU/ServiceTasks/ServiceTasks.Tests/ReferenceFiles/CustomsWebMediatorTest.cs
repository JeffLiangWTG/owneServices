using System;
using System.Net;
#if NETCOREAPP
using System.Net.Http;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.AU.ServiceTasks.Testing
{
	sealed class CustomsWebMediatorTest : TransactionedTestCase
	{
		public void TestProxyDetailsAreLogged()
		{
			var logger = new TestServiceLogger();
			var factory = new BusinessObjectFactory();

			var webMediator = new CustomsWebMediatorForTesting(factory, logger, false)
			{
#if NETCOREAPP
				ExceptionToThrow = new WebException("Cannot get web response", new HttpRequestException("Cannot get web response"))
#elif NETFRAMEWORK
				ExceptionToThrow = new WebException("Cannot get web response")
#endif
			};

			webMediator.GetWebSourceLastModifiedTime(out var _);
			var logs = logger.ToString();
			AssertEquals("Logs contain: CargoWise One is not hosted?", true, logs.Contains("Information|CargoWise One is not hosted"));
			AssertEquals("Logs contain: Try URL?", true, logs.Contains("Information|Try URL: " + webMediator.UrlProduction));
			AssertEquals("Logs contain: Try URL?", true, logs.Contains("Information|Try URL: " + webMediator.AlternateUrlProduction));
			Assert(logs.IndexOf("Information|Try URL: " + webMediator.UrlProduction, StringComparison.OrdinalIgnoreCase) <
					logs.IndexOf("Information|Try URL: " + webMediator.AlternateUrlProduction, StringComparison.OrdinalIgnoreCase));
			AssertEquals("Logs contain: Cannot get web response?", true, logs.Contains("Cannot get web response"));
			AssertEquals("Logs contain: Warning|URL?", true, logs.Contains("Warning|URL: " + webMediator.UrlProduction));
			AssertEquals("Logs contain: Warning|URL?", true, logs.Contains("Warning|URL: " + webMediator.AlternateUrlProduction));
			Assert(logs.IndexOf("Warning|URL: " + webMediator.UrlProduction, StringComparison.OrdinalIgnoreCase) <
					logs.IndexOf("Warning|URL: " + webMediator.AlternateUrlProduction, StringComparison.OrdinalIgnoreCase));

#if NETCOREAPP
			var expectedProxyMessage = "Warning|Proxy: System.Net.WebProxy";
#else
			var expectedProxyMessage = "Warning|Proxy: NONE";
#endif
			AssertEquals("Logs contain: Warning|Proxy?", true, logs.Contains(expectedProxyMessage));

			AssertEquals("Logs contain: Warning|Credentials?", true,
				logs.Contains($"Warning|Credentials: Domain ({System.Environment.UserDomainName}) UserName ({System.Environment.UserName})"));

			logger.ClearLog();
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("Precondition-IsHostedWithCargowise", true, EnvProxy.IsHostedWithCargowise);
			webMediator.GetWebSourceLastModifiedTime(out var _);
			logs = logger.ToString();
			AssertEquals("Logs contain: CargoWise One is hosted?", true, logs.Contains("Information|CargoWise One is hosted"));
			AssertEquals("Logs contain: Try URL?", true, logs.Contains("Information|Try URL: " + webMediator.AlternateUrlProduction));
			AssertEquals("Logs contain: Try URL?", true, logs.Contains("Information|Try URL: " + webMediator.UrlProduction));
			Assert(logs.IndexOf("Information|Try URL: " + webMediator.AlternateUrlProduction, StringComparison.OrdinalIgnoreCase) <
					logs.IndexOf("Information|Try URL: " + webMediator.UrlProduction, StringComparison.OrdinalIgnoreCase));
			AssertEquals("Logs contain: Cannot get web response?", true, logs.Contains("Cannot get web response"));
			AssertEquals("Logs contain: Warning|URL?", true, logs.Contains("Warning|URL: " + webMediator.UrlProduction));
			AssertEquals("Logs contain: Warning|URL?", true, logs.Contains("Warning|URL: " + webMediator.AlternateUrlProduction));
			Assert(logs.IndexOf("Warning|URL: " + webMediator.AlternateUrlProduction, StringComparison.OrdinalIgnoreCase) <
					logs.IndexOf("Warning|URL: " + webMediator.UrlProduction, StringComparison.OrdinalIgnoreCase));
#if NETCOREAPP
			expectedProxyMessage = "Warning|Proxy: System.Net.WebProxy";
#else
			expectedProxyMessage = "Warning|Proxy: NONE";
#endif
			AssertEquals("Logs contain: Warning|Proxy?", true, logs.Contains(expectedProxyMessage));
			AssertEquals("Logs contain: Warning|Credentials?", true,
				logs.Contains($"Warning|Credentials: Domain ({System.Environment.UserDomainName}) UserName ({System.Environment.UserName})"));
			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestWebRequestSettings()
		{
			var logger = new TestServiceLogger();

			var factory = new BusinessObjectFactory();

			var webMediator = new CustomsWebMediatorForTesting(factory, logger, true);
			webMediator.GetWebSourceLastModifiedTime(out var _);

			AssertEquals(webMediator.UrlTest, webMediator.WebRequestForTesting.RequestUri.AbsoluteUri);
			AssertEquals(WebRequest.DefaultWebProxy, webMediator.WebRequestForTesting.Proxy);

			webMediator = new CustomsWebMediatorForTesting(factory, logger, false);
			webMediator.GetWebSourceLastModifiedTime(out var _);

			AssertEquals(webMediator.UrlProduction, webMediator.WebRequestForTesting.RequestUri.AbsoluteUri);
			AssertEquals(WebRequest.DefaultWebProxy, webMediator.WebRequestForTesting.Proxy);
		}

		public void TestAlternateUrl()
		{
			var factory = new BusinessObjectFactory();
			var logger = new TestServiceLogger();

			var webMediator1 = new CustomsWebMediatorForTesting(factory, logger, true);
			AssertEquals(webMediator1.AlternateUrlTest, webMediator1.AlternateUrl);

			var webMediator2 = new CustomsWebMediatorForTesting(factory, logger, false);
			AssertEquals(webMediator2.AlternateUrlProduction, webMediator2.AlternateUrl);
		}

		class CustomsWebMediatorForTesting : CustomsWebMediator
		{
			public CustomsWebMediatorForTesting(BusinessObjectFactory factory, ILogger serviceLogger, bool cmrTestMode)
				: base(factory, serviceLogger, cmrTestMode)
			{
			}

			protected override HttpWebResponse GetWebResponse(HttpWebRequest webRequest)
			{
				WebRequestForTesting = webRequest;
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}
				return null;
			}
			public HttpWebRequest WebRequestForTesting;
			public WebException ExceptionToThrow;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();

			var configType = factory.New<IRefSysConfigType>();
			configType.ZRT_ConfigCode = "AURefURL";
			configType.ZRT_Description = "AU Customs Reference data URL";
			configType.ZRT_LongDescription = "URL root address to source CMR reference files for Australian Customs.";

			var config = factory.New<IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode;
			config.ZRC_StringValue = "https://www.ccf.border.gov.au/reference";
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			config.ZRC_EndDate = new ZDateTime(2079, 06, 06);

			factory.Save();
		}
	}
}
