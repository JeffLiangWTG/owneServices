using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoShipmentUserControlTest : TestCaseWithFactory
	{
		public void TestUnderbondAndPackingControlVisibility()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();

			using (var form = new ZForm(houseBill))
			using (var seaCargoShipmentUserControl = new SeaCargoShipmentUserControl())
			{
				form.Controls.Add(seaCargoShipmentUserControl);
				form.Show();

				AssertEquals("UserControl.UnderbondAndPackingUserControlForCMR.Visible", true, seaCargoShipmentUserControl.FindSingle<SeaCargoUnderbondAndPackingUserControlForCMR>("UnderbondAndPackingUserControlForCMR").Visible);

				var houseDetailsUsersControl = seaCargoShipmentUserControl.FindSingle<SeaCargoHouseDetailsUserControl>("HouseDetailsUsersControl");
				AssertEquals("UserControl.UnderbondAndPackingUserControl.ParentBillLabel.Visible", true, houseDetailsUsersControl.FindSingle<ZLabel>("ParentBillLabel").Visible);
				AssertEquals("UserControl.UnderbondAndPackingUserControl.ParentBillTextBox.Visible", true, houseDetailsUsersControl.FindSingle<ZTextBox>("ParentBillTextBox").Visible);
				AssertEquals("HouseBillPartiesUserControl Visible", true, houseDetailsUsersControl.FindSingle<HouseBillPartiesUserControl>("HouseBillPartiesUserControl").Visible);

				var billDetailsTabControl = houseDetailsUsersControl.FindSingle<ZTemplateTabControl>("BillDetailsTabControl");
				billDetailsTabControl.SelectedTab = (ZTabPage)billDetailsTabControl.TabPages["OceanBillDetailsTabPage"];
				AssertEquals("UserControl.ShippingLineGuidFindBox.Visible", true, houseDetailsUsersControl.FindSingle<ZGuidFindBox>("ShippingLineGuidBoundFindBox").Visible);
				AssertEquals("UserControl.ShippingLineLabel.Visible", true, houseDetailsUsersControl.FindSingle<ZLabel>("ShippingLineLabel").Visible);
				AssertEquals("UserControl.PrincipalIDLabel.Visible", true, houseDetailsUsersControl.FindSingle<ZLabel>("PrincipalIDLabel").Visible);
				AssertEquals("UserControl.CB_PrincipalIDBoundTextBox.Visible", true, houseDetailsUsersControl.FindSingle<ZTextBox>("CB_PrincipalIDBoundTextBox").Visible);
				AssertEquals("OverrideFreightDefaultsCheckBox is not Visible without a Consol", false, houseDetailsUsersControl.FindSingle<ZCheckBox>("overrideFreightDefaultsCheckBox").Visible);
			}
		}

		public void TestOverrideFreightDefaultsCheckBoxVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill = oceanBill.HouseBills.AddNew();

			using (var form = new ZForm(houseBill))
			using (var seaCargoShipmentUserControl = new SeaCargoShipmentUserControl())
			{
				form.Controls.Add(seaCargoShipmentUserControl);
				form.Show();

				var houseDetailsUsersControl = seaCargoShipmentUserControl.FindSingle<SeaCargoHouseDetailsUserControl>("HouseDetailsUsersControl");
				var billDetailsTabControl = houseDetailsUsersControl.FindSingle<ZTemplateTabControl>("BillDetailsTabControl");
				billDetailsTabControl.SelectedTab = (ZTabPage)billDetailsTabControl.TabPages["OceanBillDetailsTabPage"];

				var overrideFreightDefaultsCheckBox = houseDetailsUsersControl.FindSingle<ZCheckBox>("overrideFreightDefaultsCheckBox");
				AssertEquals("OverrideFreightDefaultsCheckBox is Visible with a Consol", true, overrideFreightDefaultsCheckBox.Visible);
				AssertEquals("OverrideFreightDefaultsCheckBox is ReadOnly", true, overrideFreightDefaultsCheckBox.ReadOnly);
			}
		}
	}
}
