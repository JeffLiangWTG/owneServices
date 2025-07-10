using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	class ApplicationRedirectUrlItemTest : TestCase
	{
		public void TestConstructorWithPostPath()
		{
			ExpectedValue("", "signincallback", null);
			ExpectedValue("http://test.com", "", "http://test.com");
			ExpectedValue("http://test.com/", "signincallback", "http://test.com/signincallback");
			ExpectedValue("http://test.com", "/signincallback", "http://test.com/signincallback");
			ExpectedValue("http://test.com/", "/signincallback", "http://test.com/signincallback");
		}

		void ExpectedValue(string registryValue, string postPath, string expectedResult)
		{
			var mockIRegistryItem = new Mock<IRegistryItemInternals>();
			mockIRegistryItem.Setup(registryItem => registryItem.Value).Returns(registryValue);
			var stringRegistryItem = new StringRegistryItem(mockIRegistryItem.Object);
			var applicationRedirectUrlItem = new ApplicationRedirectUrlItem("testapp", stringRegistryItem, postPath);
			AssertEquals(expectedResult, applicationRedirectUrlItem.RedirectUrl);
		}

		public void TestApplicationRedirectUrlItemToIdentityRedirectUrlRequest()
		{
			var applicationRedirectUrlItem = new ApplicationRedirectUrlItem("https://web.com", "testapp");
			var request = applicationRedirectUrlItem.Request;

			AssertEquals(applicationRedirectUrlItem.RedirectUrl, request.RedirectUrl);
			AssertEquals(applicationRedirectUrlItem.RedirectUrlType, request.RedirectUrlType);
			AssertEquals(applicationRedirectUrlItem.ApplicationName, request.ApplicationName);
		}

		public void TestInvalidRedirectType()
		{
			AssertRedirectType(RedirectUrlTypes.Web, false);
			AssertRedirectType(RedirectUrlTypes.SinglePage, false);
			AssertRedirectType(RedirectUrlTypes.InstalledClient, false);
			AssertRedirectType("invalidType", true);
		}

		void AssertRedirectType(string type, bool errorOccur)
		{
			if (errorOccur)
			{
				var exception = AssertExceptionThrown<InvalidOperationException>(() => new ApplicationRedirectUrlItem("https://web.com", "testapp", type));
				AssertEquals("Invalid redirect type: " + type, exception.Message);
			}
			else
			{
				var applicationRedirectUrlItem = new ApplicationRedirectUrlItem("https://web.com", "testapp", type);
				AssertEquals(type, applicationRedirectUrlItem.RedirectUrlType);
			}
		}
	}
}
