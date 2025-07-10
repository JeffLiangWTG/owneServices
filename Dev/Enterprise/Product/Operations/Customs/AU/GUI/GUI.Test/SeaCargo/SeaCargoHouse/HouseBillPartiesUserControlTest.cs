using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class HouseBillPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestCreateControl()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house = oceanBill.FilteredHouseBills.AddNew();
			house.CA_HouseBill = "HB123";
			house.CA_ConsigneeName = "CONEE";
			house.CA_ConsignorName = "CONOR";
			house.CA_NotifyName = "NOTIF";
			using (var testForm = new ZForm(house))
			using (var houseBillPartiesUserControl = new HouseBillPartiesUserControl())
			{
				testForm.Controls.Add(houseBillPartiesUserControl);
				houseBillPartiesUserControl.SetDataBinding(house, "");
				testForm.Show();
				var tabControl = testForm.FindSingle<ZTemplateTabControl>("ConsigneeTabControl");
				var consigneeTabPage = testForm.FindSingle<ZTabPage>("ConsigneeTabPage");
				AssertSame("ConsigneeTabPage is selected by default", consigneeTabPage, tabControl.SelectedTab);
				AssertEquals("CONEE", consigneeTabPage.FindSingle<ZArchitecture.ZTextBox>("CA_ConsigneeNameBoundTextBox19").Text);
				var consignorTabPage = testForm.FindSingle<ZTabPage>("ConsignorTabPage");
				tabControl.SelectedTab = consignorTabPage;
				AssertEquals("CONOR", consignorTabPage.FindSingle<ZArchitecture.ZTextBox>("CA_ConsignorNameBoundTextBox").Text);
				var notifyPartyTabPage = testForm.FindSingle<ZTabPage>("NotifyPartyTabPage");
				tabControl.SelectedTab = notifyPartyTabPage;
				AssertEquals("NOTIF", notifyPartyTabPage.FindSingle<ZArchitecture.ZTextBox>("CA_NotifyNameBoundTextBox").Text);
			}
		}

		public void TestZAddressControlOrgListBindings()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house = oceanBill.FilteredHouseBills.AddNew();

			using (var userControl = new HouseBillPartiesUserControl())
			{
				userControl.SetDataBinding(house, string.Empty);
				AssertEquals("Lookups+Consignee_List", userControl.ConsigneezAddressControl.BindToOrgList);
				AssertEquals("Lookups+Consignor_List", userControl.ConsignorzAddressControl.BindToOrgList);

				userControl.SetDataBinding(oceanBill, nameof(CusSCAOceanBill.FilteredHouseBills));
				AssertEquals("FilteredHouseBills.Lookups+Consignee_List", userControl.ConsigneezAddressControl.BindToOrgList);
				AssertEquals("FilteredHouseBills.Lookups+Consignor_List", userControl.ConsignorzAddressControl.BindToOrgList);
			}
		}
	}
}
