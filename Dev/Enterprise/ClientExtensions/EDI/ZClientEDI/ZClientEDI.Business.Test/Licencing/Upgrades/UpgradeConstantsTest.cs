using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class UpgradeConstantsTest : TransactionedTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("WebServerClientSpecificPath", @"\\web\updates\ediEnterprise\ClientSpecific\", UpgradeConstants.WebServerClientSpecificPath);
			AssertEquals("WebServerGenericPath", @"\\web\updates\ediEnterprise\Generic\", UpgradeConstants.WebServerGenericPath);
			AssertEquals("HttpClientSpecificBaseUrl", @"http://www.cargowise.com/ftpmirror/ediEnterprise/ClientSpecific/", UpgradeConstants.HttpClientSpecificBaseUrl);
			AssertEquals("HttpGenericBaseUrl", @"http://www.cargowise.com/ftpmirror/ediEnterprise/Generic/", UpgradeConstants.HttpGenericBaseUrl);
			AssertEquals("HttpDownloadUserName", "", UpgradeConstants.HttpDownloadUserName);
			AssertEquals("HttpDownloadPassword", "", UpgradeConstants.HttpDownloadPassword);
		}

		public void TestOveriddenValues()
		{
			EDIDataRegistry.Instance.WebServerClientSpecificPath = "a";
			EDIDataRegistry.Instance.WebServerGenericPath = "b";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "c";
			EDIDataRegistry.Instance.HttpGenericBaseUrl = "d";
			EDIDataRegistry.Instance.HttpDownloadUserName = "e";
			EDIDataRegistry.Instance.HttpDownloadPassword = "f";

			AssertEquals("WebServerClientSpecificPath", "a", UpgradeConstants.WebServerClientSpecificPath);
			AssertEquals("WebServerGenericPath", "b", UpgradeConstants.WebServerGenericPath);
			AssertEquals("HttpClientSpecificBaseUrl", "c", UpgradeConstants.HttpClientSpecificBaseUrl);
			AssertEquals("HttpGenericBaseUrl", "d", UpgradeConstants.HttpGenericBaseUrl);
			AssertEquals("HttpDownloadUserName", "e", UpgradeConstants.HttpDownloadUserName);
			AssertEquals("HttpDownloadPassword", "f", UpgradeConstants.HttpDownloadPassword);
		}
	}
}