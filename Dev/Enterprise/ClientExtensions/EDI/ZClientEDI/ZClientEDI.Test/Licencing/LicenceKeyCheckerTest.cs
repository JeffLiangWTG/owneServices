using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Testing
{
	internal class LicenceKeyCheckerTest : TestCaseWithFactory
	{
		public void TestCheckOrgNameLength()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org = lic.Company.Header;
			org.OH_FullName = ZString.Replicate('A', 51);
			Factory.Save();
			var checker = new LicenceKeyChecker(org, lic.Database, true);
			AssertEquals(false, checker.CheckCanGenerateLicenceKey);
			AssertEquals("Organization name exceeds 50 characters: " + org.OH_FullName, checker.ErrorMessage);
		}
	}
}
