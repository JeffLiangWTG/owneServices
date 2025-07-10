using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.Business.Testing
{
	[TestedType(typeof(EdiIdentityTenant))]
	internal class EdiIdentityTenantTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTenantHasDefaultValue()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			AssertEquals("", tenant.IDT_TenantId);
			AssertEquals("", tenant.IDT_OidcClientId);
			AssertEquals("", tenant.IDT_Name);
			AssertEquals("", tenant.IDT_AuthorityUrl);
			AssertEquals("", tenant.IDT_GraphClientId);
			Assert(!tenant.IDT_Onboarding);
		}

		public void TestTenantHumanReadableName()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_Name = "Test Tenant";
			AssertEquals("Tenant - Test Tenant", tenant.HumanReadableName);
		}

		public void TestTenantHumanShortcutName()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_Name = "Test Tenant";
			AssertEquals("Test Tenant", tenant.HumanReadableShortcutName);
		}

		public void TestTenantIDT_Name()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_Name = "Test Tenant";
			Factory.Save();
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_Name = "Test Tenant";
			var message = AssertExceptionThrown<ZSaveException>(() => Factory.Save()).Message;
			AssertContains("The duplicate key value is (Test Tenant).", message);
		}

		public void TestAuthorityUrlUnique()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_AuthorityUrl = "https://test.com";
			Factory.Save();

			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_AuthorityUrl = "https://test.com";
			var exception = AssertExceptionThrown<ZSaveException>(Factory.Save);
			AssertContains("NR_UX__IDT_AuthorityUrl", exception.Message);
		}
	}
}
