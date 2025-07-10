using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	internal class ClientCompanyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLCC_OH()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateDependentLicence(lic1, "BBB");
			var orgWithoutLicence = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgWithoutLicence.OH_Code = "DDDNYC";

			var licOtherEnterprise = BillingTestHelper.CreateLicence(Factory, "CCC");
			Factory.Save();

			var client1 = lic1.ClientCompany;
			var client2 = lic2.ClientCompany;

			client1.LCC_OH = lic1.Company.LC_OH;
			client2.LCC_OH = lic2.Company.LC_OH;

			AssertNoErrors(client1.LCC_OHInfo);
			AssertNoErrors(client2.LCC_OHInfo);

			client2.LCC_OH = lic1.Company.LC_OH;
			AssertHasError(client2.LCC_OHInfo, "Organization is already set on another company on the database.");

			client2.LCC_OH = orgWithoutLicence.PK;
			AssertHasError(client2.LCC_OHInfo, "Organization does not have a licence.");

			client2.LCC_OH = licOtherEnterprise.Company.LC_OH;
			AssertHasError(client2.LCC_OHInfo, "Organization enterprise CCC does not match database enterprise AAA.");
		}
	}
}