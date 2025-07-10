using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingExcludeOrgValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCEX_BillingSystem()
		{
			var excludeOrg = Factory.New<ClientLicenceBillingExcludeOrg>();
			excludeOrg.Validation.ValidateCEX_BillingSystem();
			AssertHasErrors(excludeOrg.CEX_BillingSystemInfo);

			excludeOrg.CEX_BillingSystem = "AAA";
			excludeOrg.Validation.ValidateCEX_BillingSystem();
			AssertHasErrors(excludeOrg.CEX_BillingSystemInfo);

			excludeOrg.CEX_BillingSystem = BillingConstants.BillingSystem.AirlineMessaging;
			excludeOrg.Validation.ValidateCEX_BillingSystem();
			AssertNoErrors(excludeOrg.CEX_BillingSystemInfo);
		}

		public void TestCEX_LicenceCode()
		{
			var excludeOrg = Factory.New<ClientLicenceBillingExcludeOrg>();
			excludeOrg.Validation.ValidateCEX_LicenceCode();
			AssertHasErrors(excludeOrg.CEX_LicenceCodeInfo);

			excludeOrg.CEX_LicenceCode = "AAABBBCCC";
			excludeOrg.Validation.ValidateCEX_LicenceCode();
			AssertNoErrors(excludeOrg.CEX_LicenceCodeInfo);
		}
	}
}