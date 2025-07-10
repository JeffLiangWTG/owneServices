using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CNJobDocAddressControlTest : TestCaseWithFactory
	{
		public void TestVisibles()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new CNJobDocAddressControl())
			{
				declaration.JE_MessageType = "IMP";
				declaration.JE_MessageSubType = "CUS";
				control.SetDataBinding(declaration, ".ImporterDocumentaryAddress");
				control.BindToOrganisations = "Lookups.ImportersList";
				form.Controls.Add(control);
				form.Show();
				declaration.ImporterDocumentaryAddress.E2_AddressOverride = false;
				AssertControl(form, "SocialCreditCodeTextBox", true);
				AssertControl(form, "CustomsCodeTextBox", true);
				AssertControl(form, "CIQCodeTextBox", true);
				AssertControl(form, "OverseasPartyCodeTypeDropEdit", false);
				AssertControl(form, "OverseasPartyCodeTextBox", false);
				declaration.JE_MessageType = "EXP";
				control.Hide();
				control.Show();
				AssertControl(form, "SocialCreditCodeTextBox", false);
				AssertControl(form, "CustomsCodeTextBox", false);
				AssertControl(form, "CIQCodeTextBox", false);
				AssertControl(form, "OverseasPartyCodeTypeDropEdit", true);
				AssertControl(form, "OverseasPartyCodeTextBox", true);
				AssertTabVisible(form, "AddressTabPage", true);
				AssertTabVisible(form, "OverrideAddressTabPage", false);
				AssertControl(form, "OrganisationPanel", true);
				declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
				control.Hide();
				control.Show();
				AssertTabVisible(form, "AddressTabPage", false);
				AssertTabVisible(form, "OverrideAddressTabPage", true);
				AssertControl(form, "OrganisationPanel", false);
			}
		}

		static void AssertControl(ZForm form, string controlName, bool shouldBeVisible)
		{
			var control = form.Controls.Find(controlName, true)[0];
			Assert(controlName + "should be " + (shouldBeVisible ? "visible" : "invisible"), control.Visible == shouldBeVisible);
		}

		static void AssertTabVisible(ZForm form, string controlName, bool shouldBeVisible)
		{
			var controls = form.Controls.Find(controlName, true);
			if (controls.Length > 0)
			{
				Assert(controlName + "should be " + (shouldBeVisible ? "visible" : "invisible"), ((ZTabPage)controls[0]).TabVisible);
			}
			else
			{
				Assert(controlName + " should be invisible", !shouldBeVisible);
			}
		}
	}
}
