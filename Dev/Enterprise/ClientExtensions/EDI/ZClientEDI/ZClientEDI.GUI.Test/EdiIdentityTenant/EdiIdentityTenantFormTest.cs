using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityTenant.GUI.Testing
{
	[TestedType(typeof(EdiIdentityTenantForm))]
	internal class EdiIdentityTenantFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var ediIdentityTenant = Factory.New<EdiIdentityTenant>();
			return new EdiIdentityTenantForm(ediIdentityTenant);
		}

		public void TestCheckTenantSave()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			UnitTestUserNotification.Instance.ClearMessages();
			using var form = new EdiIdentityTenantForm(tenant);
			form.Show();
			form.FireSaveButton();
			AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertHasError(tenant.IDT_TenantIdInfo, "Please enter a value.");
			AssertHasError(tenant.IDT_NameInfo, "Please enter a value.");
			AssertHasError(tenant.IDT_GraphClientIdInfo, "Please enter a value.");
			AssertHasError(tenant.IDT_OidcClientIdInfo, "Please enter a value.");
		}

		public void TestCheckOnboarding()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var tenantId = Guid.NewGuid().ToString();
			tenant1.IDT_Onboarding = true;
			tenant1.IDT_TenantId = tenantId;
			Factory.Save();

			var tenant = Factory.New<EdiIdentityTenant>();
			tenant.IDT_TenantId = Guid.NewGuid().ToString();
			tenant.IDT_OidcClientId = Guid.NewGuid().ToString();
			tenant.IDT_Name = "Test Tenant";
			tenant.IDT_GraphClientId = Guid.NewGuid().ToString();

			UnitTestUserNotification.Instance.ClearMessages();
			using var form = new EdiIdentityTenantForm(tenant);
			form.Show();
			form.OnboardingCheckBox.Checked = true;

			form.FireSaveButton();
			AssertEquals("Should show message", $"When you set this Onboarding to true, the Onboarding of tenant '{tenantId}' will automatically be set to false. Only one Onboarding can be active at a time.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			form.FireSaveButton();
			Assert(tenant.IDT_Onboarding);
			Assert(!tenant1.IDT_Onboarding);
		}
	}
}
