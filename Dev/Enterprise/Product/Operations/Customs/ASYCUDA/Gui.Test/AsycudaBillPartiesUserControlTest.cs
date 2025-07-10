using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaBillPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestCreateOrganizationFromShipper()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Enterprise.Environment.Env.Registry.CanUserEditOrganisationCode = true;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZFormForTest(header))
			{
				form.Show();
				form.ShipperAddressControl.Focus();

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				bill.ABL_ShipperName = "TEST NAME";
				bill.ABL_ShipperStreet1 = "TEST STREET 1";
				bill.ABL_ShipperStreet2 = "TEST STREET 2";
				bill.ABL_RN_NKShipperCountry = "AU";
				bill.ABL_ShipperState = "NSW";
				bill.ABL_ShipperCity = "TEST CITY";
				bill.ABL_ShipperPostcode = "1234";

				AssertEquals("button should be visible if address is entered", true, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				form.ShipperCodeBox.Code = "TESTORG";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, create new organization
				form.BillPartiesControl.ConvertShipperToOrganizationButton.PerformClick();
				AssertEquals("The code 'TESTORG' does not exist. Would you like to create a new Organization?", UnitTestUserNotification.Instance.LastMessage.Text);

				ZOrganisationsForm organizationsForm = (ZOrganisationsForm)ZFormModaliser.LastFormShownForTest;
				organizationsForm.Organisation.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals(ContinueWithSave.Yes, organizationsForm.FireSaveButton());
				organizationsForm.Close();

				Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				AssertNotNull("organization with original code must be created", org);

				// check that organization properties was copied from bill
				AssertEquals("TEST NAME", org.OH_FullName);
				AssertEquals("TEST STREET 1", org.MainAddress.Address1);
				AssertEquals("TEST STREET 2", org.MainAddress.Address2);
				AssertEquals("AU", org.MainAddress.OA_RN_NKCountryCode);
				AssertEquals("NSW", org.MainAddress.OA_State);
				AssertEquals("TEST CITY", org.MainAddress.OA_City);
				AssertEquals("1234", org.MainAddress.OA_PostCode);

				AssertEquals("organization must be set to bill", org.MainAddress.PK, bill.ABL_OA_Shipper);
				bill.ABL_OA_Shipper = org.PK;

				AssertEquals("button should be invisible if organization is set", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);
			}
		}

		public void TestCreateOrganizationFromConsignee()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Enterprise.Environment.Env.Registry.CanUserEditOrganisationCode = true;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZFormForTest(header))
			{
				form.Show();
				form.ConsigneeAddressControl.Focus();

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				bill.ABL_ConsigneeName = "TEST NAME";
				bill.ABL_ConsigneeStreet1 = "TEST STREET 1";
				bill.ABL_ConsigneeStreet2 = "TEST STREET 2";
				bill.ABL_RN_NKConsigneeCountry = "AU";
				bill.ABL_ConsigneeState = "NSW";
				bill.ABL_ConsigneeCity = "TEST CITY";
				bill.ABL_ConsigneePostcode = "1234";
				bill.ABL_ConsigneePhone = "+61212341234";

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be visible if address is entered", true, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				form.ConsigneeCodeBox.Code = "TESTORG";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, create new organization
				form.BillPartiesControl.ConvertConsigneeToOrganizationButton.PerformClick();
				AssertEquals("The code 'TESTORG' does not exist. Would you like to create a new Organization?", UnitTestUserNotification.Instance.LastMessage.Text);

				ZOrganisationsForm organizationsForm = (ZOrganisationsForm)ZFormModaliser.LastFormShownForTest;
				organizationsForm.Organisation.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals(ContinueWithSave.Yes, organizationsForm.FireSaveButton());
				organizationsForm.Close();

				Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				AssertNotNull("organization with original code must be created", org);

				// check that organization properties was copied from bill
				AssertEquals("TEST NAME", org.OH_FullName);
				AssertEquals("TEST STREET 1", org.MainAddress.Address1);
				AssertEquals("TEST STREET 2", org.MainAddress.Address2);
				AssertEquals("AU", org.MainAddress.OA_RN_NKCountryCode);
				AssertEquals("NSW", org.MainAddress.OA_State);
				AssertEquals("TEST CITY", org.MainAddress.OA_City);
				AssertEquals("1234", org.MainAddress.OA_PostCode);
				AssertEquals("+61212341234", org.MainAddress.OA_Phone);

				AssertEquals("organization must be set to bill", org.MainAddress.PK, bill.ABL_OA_Consignee);
				bill.ABL_OA_Consignee = org.PK;

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if organization is set", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);
			}
		}

		public void TestCreateOrganizationFromNotifyParty()
		{
			// otherwise organizations code will be overriden when UNLOCO is set
			Enterprise.Environment.Env.Registry.CanUserEditOrganisationCode = true;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			using (var form = new ZFormForTest(header))
			{
				form.Show();
				form.NotifyPartyAddressControl.Focus();

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				bill.ABL_NotifyPartyName = "TEST NAME";
				bill.ABL_NotifyPartyStreet1 = "TEST STREET 1";
				bill.ABL_NotifyPartyStreet2 = "TEST STREET 2";
				bill.ABL_RN_NKNotifyPartyCountry = "AU";
				bill.ABL_NotifyPartyState = "NSW";
				bill.ABL_NotifyPartyCity = "TEST CITY";
				bill.ABL_NotifyPartyPostcode = "1234";
				bill.ABL_NotifyPartyPhone = "+61212341234";

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be visible if address is entered", true, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);

				form.NotifyPartyCodeBox.Code = "TESTORG";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // YES, create new organization
				form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.PerformClick();
				AssertEquals("The code 'TESTORG' does not exist. Would you like to create a new Organization?", UnitTestUserNotification.Instance.LastMessage.Text);

				ZOrganisationsForm organizationsForm = (ZOrganisationsForm)ZFormModaliser.LastFormShownForTest;
				organizationsForm.Organisation.OH_RL_NKClosestPort = "AUSYD";
				organizationsForm.Organisation.OH_IsSalesLead = ZBool.True;
				AssertEquals(ContinueWithSave.Yes, organizationsForm.FireSaveButton());
				organizationsForm.Close();

				Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG"));
				AssertNotNull("organization with original code must be created", org);

				// check that organization properties was copied from bill
				AssertEquals("TEST NAME", org.OH_FullName);
				AssertEquals("TEST STREET 1", org.MainAddress.Address1);
				AssertEquals("TEST STREET 2", org.MainAddress.Address2);
				AssertEquals("AU", org.MainAddress.OA_RN_NKCountryCode);
				AssertEquals("NSW", org.MainAddress.OA_State);
				AssertEquals("TEST CITY", org.MainAddress.OA_City);
				AssertEquals("1234", org.MainAddress.OA_PostCode);
				AssertEquals("+61212341234", org.MainAddress.OA_Phone);

				AssertEquals("organization must be set to bill", org.MainAddress.PK, bill.ABL_OA_NotifyParty);
				bill.ABL_OA_NotifyParty = org.PK;

				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertShipperToOrganizationButton.Visible);
				AssertEquals("button should be invisible if no address is entered", false, form.BillPartiesControl.ConvertConsigneeToOrganizationButton.Visible);
				AssertEquals("button should be invisible if organization is set", false, form.BillPartiesControl.ConvertNotifyPartyToOrganizationButton.Visible);
			}
		}

		sealed class ZFormForTest : ZForm
		{
			public ZFormForTest(AsycudaManifestHeader header) : base(header)
			{
				BindingSource.SetBindingMember(BillPartiesControl, "Bills");
				Controls.Add(BillPartiesControl);
			}

			internal AsycudaBillPartiesUserControlForTest BillPartiesControl { get; } = new AsycudaBillPartiesUserControlForTest();

			internal IFindBox ShipperCodeBox => this.FindSingle<ZAddressControl>(c => c.Name == "ShipperAddressControl").FindSingle<IFindBox>();

			internal IFindBox ConsigneeCodeBox => this.FindSingle<ZAddressControl>(c => c.Name == "ConsigneeAddressControl").FindSingle<IFindBox>();

			internal IFindBox NotifyPartyCodeBox => this.FindSingle<ZAddressControl>(c => c.Name == "NotifyPartyAddressControl").FindSingle<IFindBox>();

			internal ZAddressControl NotifyPartyAddressControl => this.FindSingle<ZAddressControl>(c => c.Name == "NotifyPartyAddressControl");

			internal ZAddressControl ConsigneeAddressControl => this.FindSingle<ZAddressControl>(c => c.Name == "ConsigneeAddressControl");

			internal ZAddressControl ShipperAddressControl => this.FindSingle<ZAddressControl>(c => c.Name == "ShipperAddressControl");
		}

		sealed class AsycudaBillPartiesUserControlForTest : AsycudaBillPartiesUserControl
		{
			internal ZButton ConvertNotifyPartyToOrganizationButton => this.FindSingle<ZButton>(c => c.Name == "ConvertNotifyPartyToOrganizationButton");

			internal ZButton ConvertShipperToOrganizationButton => this.FindSingle<ZButton>(c => c.Name == "ConvertShipperToOrganizationButton");

			internal ZButton ConvertConsigneeToOrganizationButton => this.FindSingle<ZButton>(c => c.Name == "ConvertConsigneeToOrganizationButton");
		}
	}
}
