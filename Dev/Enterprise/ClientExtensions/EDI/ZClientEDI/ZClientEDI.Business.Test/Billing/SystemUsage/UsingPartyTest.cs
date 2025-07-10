using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class UsingPartyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "PRD");
			var periodStart = ZDateTime.UtcToday;
			periodStart = new ZDateTime(periodStart.Year, periodStart.Month, 1);
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, lic, 10);

			var user1 = new UsingParty(lic);
			var user2 = new UsingParty(lic.Company.Header, "ZZZ");
			var user3 = new UsingParty(lic.ClientCompany);
			var user4 = new UsingParty(usage);

			AssertEquals(lic.Company.LC_OH, user1.OrganisationPK);
			AssertEquals(lic.Company.LC_OH, user2.OrganisationPK);
			AssertEquals(lic.Company.LC_OH, user3.OrganisationPK);
			AssertEquals(lic.Company.LC_OH, user4.OrganisationPK);

			AssertEquals(lic.Company.PK, user1.LicenceCompanyPK);
			AssertEquals(lic.Company.PK, user2.LicenceCompanyPK);
			AssertEquals(lic.Company.PK, user3.LicenceCompanyPK);
			AssertEquals(lic.Company.PK, user4.LicenceCompanyPK);

			AssertEquals("AAA", user1.EnterpriseCode);
			AssertEquals("AAA", user2.EnterpriseCode);
			AssertEquals("AAA", user3.EnterpriseCode);
			AssertEquals("AAA", user4.EnterpriseCode);

			AssertEquals("SYD", user1.CompanyCode);
			AssertEquals("SYD", user2.CompanyCode);
			AssertEquals("SYD", user3.CompanyCode);
			AssertEquals("SYD", user4.CompanyCode);

			AssertEquals(null, user1.CompanyName);
			AssertEquals(null, user2.CompanyName);
			AssertEquals("SYD Co", user3.CompanyName);
			AssertEquals("SYD Co", user4.CompanyName);

			AssertEquals("PRD", user1.ServerCode);
			AssertEquals("ZZZ", user2.ServerCode);
			AssertEquals("PRD", user3.ServerCode);
			AssertEquals("PRD", user4.ServerCode);

			AssertEquals(true, user1.IsOrganisationActive);
			AssertEquals(true, user2.IsOrganisationActive);
			AssertEquals(true, user3.IsOrganisationActive);
			AssertEquals(true, user4.IsOrganisationActive);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB", "MEL", "TS1");
			lic2.Company.Header.OH_IsActive = false;
			var user5 = new UsingParty(lic2);
			AssertEquals(false, user5.IsOrganisationActive);
		}

		public void TestConstructor_ShouldUseClientCompanyOrg()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "PRD");
			var periodStart = ZDateTime.UtcToday;
			periodStart = new ZDateTime(periodStart.Year, periodStart.Month, 1);
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, lic, 10);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			lic.ClientCompany.LCC_OH = org.PK;
			var user1 = new UsingParty(lic.ClientCompany);

			AssertEquals(lic.ClientCompany.LCC_OH, user1.OrganisationPK);
			AssertEquals(lic.Company.PK, user1.LicenceCompanyPK);
			AssertEquals("AAA", user1.EnterpriseCode);
			AssertEquals("SYD", user1.CompanyCode);
			AssertEquals("SYD Co", user1.CompanyName);
			AssertEquals("PRD", user1.ServerCode);
			AssertEquals(true, user1.IsOrganisationActive);
		}
	}
}