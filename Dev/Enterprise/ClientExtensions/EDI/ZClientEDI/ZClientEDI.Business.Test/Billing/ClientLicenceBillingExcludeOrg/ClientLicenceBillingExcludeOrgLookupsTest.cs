using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingExcludeOrgLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSystemCodes()
		{
			var excludeOrg = Factory.New<ClientLicenceBillingExcludeOrg>();
			Assert(excludeOrg.Lookups.SystemCodes.ContainsCode(BillingConstants.BillingSystem.AirlineMessaging));
		}
	}
}