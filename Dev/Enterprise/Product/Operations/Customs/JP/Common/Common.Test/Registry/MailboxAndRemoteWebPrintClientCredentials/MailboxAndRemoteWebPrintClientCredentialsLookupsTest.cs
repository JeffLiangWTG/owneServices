using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common.Testing
{
	sealed class MailboxAndRemoteWebPrintClientCredentialsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var lookups = new MailboxAndRemoteWebPrintClientCredentials().Lookups;
			AssertType<XtCredentialStatusList>(lookups.StatusList);
		}

		public void TestFailureNotificationGroupList()
		{
			var lookups = new MailboxAndRemoteWebPrintClientCredentials().Lookups;
			AssertType<GlbGroupCollection>(lookups.FailureNotificationGroupList);
		}
	}
}
