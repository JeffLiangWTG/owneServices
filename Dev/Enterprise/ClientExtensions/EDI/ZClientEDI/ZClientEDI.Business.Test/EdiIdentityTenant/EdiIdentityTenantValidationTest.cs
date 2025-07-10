using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IdentityTenant.Business.Testing
{
	internal class EdiIdentityTenantValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTenantIdEmpty()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_TenantId = string.Empty;
			AssertHasError(tenant.IDT_TenantIdInfo, "Please enter a value.");
		}

		public void TestValidateTenantNameEmpty()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_Name = string.Empty;
			AssertHasError(tenant.IDT_NameInfo, "Please enter a value.");
		}

		public void TestValidateGraphClientIdEmpty()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_GraphClientId = string.Empty;
			AssertHasError(tenant.IDT_GraphClientIdInfo, "Please enter a value.");
		}

		public void TestValidateAuthorityUrl()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_AuthorityUrl = string.Empty;
			AssertNoErrors(tenant.IDT_AuthorityUrlInfo);

			tenant.IDT_AuthorityUrl = "Test";
			AssertHasError(tenant.IDT_AuthorityUrlInfo, "Please enter the correct URL.");

			tenant.IDT_AuthorityUrl = "https://www.example.com";
			AssertNoErrors(tenant.IDT_AuthorityUrlInfo);
		}
	}
}
