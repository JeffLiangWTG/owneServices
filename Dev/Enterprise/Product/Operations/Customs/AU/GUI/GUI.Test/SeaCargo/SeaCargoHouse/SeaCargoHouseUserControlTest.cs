using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Layout;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoHouseUserControlTest : TestCaseWithFactory
	{
		public void TestTabOrder()
		{
			// Has tabs in the following order:
			// ‘Details’, Packing’, ‘Custom Fields’ and ‘Customs Underbond Movement’
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "ABC123";
			using (var testForm = new ZForm(houseBill))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(houseBill, "");
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var allPages = mainTabControl.AllTabPages;
				var detailsTabPage = mainTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				AssertEquals("Details", detailsTabPage.Text);
				var detailsTabPageIdx = Array.IndexOf(allPages, detailsTabPage);
				var packingTabPage = mainTabControl.FindSingle<ZTabPage>("PackingTabPage");
				AssertEquals("Packing", packingTabPage.Text);
				var packingTabPageIdx = Array.IndexOf(allPages, packingTabPage);
				AssertGreaterThan(packingTabPageIdx, detailsTabPageIdx);
				var customFieldsTabPage = mainTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
				AssertEquals("Custom Fields", customFieldsTabPage.CaptionResourceString.Caption);
				var customFieldsTabPageIdx = Array.IndexOf(allPages, customFieldsTabPage);
				AssertGreaterThan(customFieldsTabPageIdx, packingTabPageIdx);
				var customsUnderbondMovementTabPage = mainTabControl.FindSingle<ZTabPage>("CustomsUnderbondMovementTabPage");
				AssertEquals("Customs Underbond Movement", customsUnderbondMovementTabPage.Text);
				var customsUnderbondMovementTabPageIdx = Array.IndexOf(allPages, customsUnderbondMovementTabPage);
				AssertGreaterThan(customsUnderbondMovementTabPageIdx, customFieldsTabPageIdx);
				AssertEquals("Tab Count", 4, allPages.Length);
			}
		}

		public void TestOceanBillDetails()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "HB123";
			using (var testForm = new ZForm(houseBill))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(houseBill, "");
				testForm.Show();
				var oceanBillDetailsUserControl = testForm.FindSingle<OceanBillDetailsUserControl>("OceanBillDetailsUserControl");
				var oceanBillTextBox = oceanBillDetailsUserControl.FindSingle<ZArchitecture.ZTextBox>("CB_OceanBillBoundTextBox");
				AssertEquals("OB001", oceanBillTextBox.Text);
				AssertEquals("oceanBillTextBox Visible", true, oceanBillTextBox.Visible);
				AssertEquals("oceanBillTextBox Enabled", true, oceanBillTextBox.Enabled);
				AssertEquals("oceanBillTextBox ReadOnly", true, oceanBillTextBox.ReadOnly);

				var overrideCheckbox = oceanBillDetailsUserControl.FindSingle<ZCheckBox>("overrideFreightDefaultsCheckBox");
				AssertEquals("OverrideFreightDefaultsCheckBox is not Visible without a Consol", false, overrideCheckbox.Visible);
				AssertEquals("OverrideFreightDefaultsCheckBox is ReadOnly", true, overrideCheckbox.ReadOnly);
			}
		}

		public void TestDetailsTab()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "HB123";
			houseBill.CA_ConsigneeName = "HAROLD";
			using (var testForm = new ZForm(houseBill))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(houseBill, "");
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var detailsTabPage = mainTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				mainTabControl.SelectedTab = detailsTabPage;
				var houseBillDetailsUserControl = detailsTabPage.FindSingle<HouseBillDetailsUserControl>("HouseBillDetailsUserControl");
				var customsStatusTextBox = houseBillDetailsUserControl.FindSingle<ZArchitecture.ZTextBox>("CA_ShipmentStatusBoundTextBox");
				AssertEquals("Customs Status TextBox Visible", true, customsStatusTextBox.Visible);
				AssertEquals("Customs Status TextBox Enabled", true, customsStatusTextBox.Enabled);
				AssertEquals("Customs Status TextBox ReadOnly", true, customsStatusTextBox.ReadOnly);
				var messageStatusTextBox = houseBillDetailsUserControl.FindSingle<ZArchitecture.ZTextBox>("MessageStatusTextBox");
				AssertEquals("Message Status TextBox Visible", true, messageStatusTextBox.Visible);
				AssertEquals("Message Status TextBox Enabled", true, messageStatusTextBox.Enabled);
				AssertEquals("Message Status TextBox ReadOnly", true, messageStatusTextBox.ReadOnly);
				var houseBillTextBox = houseBillDetailsUserControl.FindSingle<ZArchitecture.ZTextBox>("CA_HouseBillBoundTextBox");
				AssertEquals("HB123", houseBillTextBox.Text);
				AssertEquals("houseBillTextBox Visible", true, houseBillTextBox.Visible);
				AssertEquals("houseBillTextBox Enabled", true, houseBillTextBox.Enabled);
				AssertEquals("houseBillTextBox Not ReadOnly", false, houseBillTextBox.ReadOnly);
				var houseBillPartiesUserControl = detailsTabPage.FindSingle<HouseBillPartiesUserControl>("HouseBillPartiesUserControl");
				var consigneeTabControl = houseBillPartiesUserControl.FindSingle<ZTemplateTabControl>("ConsigneeTabControl");
				var consigneeTabPage = consigneeTabControl.FindSingle<ZTabPage>("ConsigneeTabPage");
				consigneeTabControl.SelectedTab = consigneeTabPage;
				var consigneeNameTextBox = consigneeTabPage.FindSingle<ZArchitecture.ZTextBox>("CA_ConsigneeNameBoundTextBox19");
				AssertEquals("HAROLD", consigneeNameTextBox.Text);
				AssertNotNull("ConsignorTabPage", consigneeTabControl.FindSingle<ZTabPage>("ConsignorTabPage"));
				AssertNotNull("NotifyPartyTabPage", consigneeTabControl.FindSingle<ZTabPage>("NotifyPartyTabPage"));
			}
		}

		public void TestPackingTab()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house1 = oceanBill.FilteredHouseBills.AddNew();
			house1.CA_HouseBill = "HB1";
			var house2 = oceanBill.FilteredHouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			var container1 = oceanBill.Containers.AddNew();
			var container2 = oceanBill.Containers.AddNew();
			var house1Pivot = house1.Pivot.AddNew();
			house1Pivot.CV_GoodsDescription = "SOME GOODS";
			container1.Pivots.Add(house1Pivot);
			AssertSame("house1Pivot.Container", container1, house1Pivot.Container);
			var house2Pivot = house2.Pivot.AddNew();
			house2Pivot.CV_GoodsDescription = "MORE GOODS";
			container2.Pivots.Add(house2Pivot);
			AssertSame("house2Pivot.Container", container2, house2Pivot.Container);
			using (var testForm = new ZForm(house1))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(house1, "");
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var packingTabPage = mainTabControl.FindSingle<ZTabPage>("PackingTabPage");
				mainTabControl.SelectedTab = packingTabPage;
				var containersGrid = packingTabPage.FindSingle<ZArchitecture.ZGrid>("ContainersGrid");
				AssertEquals("containersGrid.Count", 1, containersGrid.ListManager.Count);
				containersGrid.CurrentRowIndex = 1;
				var associatedContainerStyle = containersGrid.GetColumnStyle(CusSCAPivot.Schema.CV_AssociatedContainer);
				AssertEquals("Associated Container is not readonly", false, associatedContainerStyle.IsReadOnly);
				var sealNumberStyle = containersGrid.GetColumnStyle(CusSCAPivot.Schema.CN_SealNumber);
				AssertEquals("CN_SealNumber is ReadOnly", true, sealNumberStyle.IsReadOnly);
				var goodsDescriptionTextBox = packingTabPage.FindSingle<ZArchitecture.ZTextBox>("CV_GoodsDescriptionBoundTextBox");
				AssertEquals("SOME GOODS", goodsDescriptionTextBox.Text);
			}
		}

		public void TestCustomFieldsTab()
		{
			var hacTemplate1 = Factory.New<ProcessTaskTemplate>();
			hacTemplate1.P0_ProcessType = WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
			hacTemplate1.P0_Name = "TEST HAC 1";
			hacTemplate1.P0_Description = "TEST HAC 1 DESC";
			hacTemplate1.P0_DischargePortCountry = "AUSYD";
			var hacCustomField1 = hacTemplate1.GenCustomColumnDefinitions.AddNew();
			hacCustomField1.XC_Name = "HACSHORT";
			hacCustomField1.XC_Type = AddOnColumnDataType.Codes.Short;
			var hacCustomField2 = hacTemplate1.GenCustomColumnDefinitions.AddNew();
			hacCustomField2.XC_Name = "HACBOOLEAN";
			hacCustomField2.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var hacTemplate2 = Factory.New<ProcessTaskTemplate>();
			hacTemplate2.P0_ProcessType = WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode;
			hacTemplate2.P0_Name = "TEST HAC 2";
			hacTemplate2.P0_Description = "TEST HAC 2 DESC";
			hacTemplate2.P0_DischargePortCountry = "AUMEL";
			var hacCustomField3 = hacTemplate2.GenCustomColumnDefinitions.AddNew();
			hacCustomField3.XC_Name = "HACDATETIME";
			hacCustomField3.XC_Type = AddOnColumnDataType.Codes.Datetime;
			Factory.Save();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			using (var testForm = new ZForm(houseBill))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(houseBill, "");
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var customFieldsTabPage = mainTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
				mainTabControl.SelectedTab = customFieldsTabPage;
				var seaCargoCustomFieldsControl = customFieldsTabPage.FindSingle<SeaCargoHouseBillCustomFieldsUserControl>("HouseBillCustomFieldsControl");
				var hacCustomFieldsControl = seaCargoCustomFieldsControl.FindSingle<ProcessTemplateCustomFieldsControl>("CustomFieldsControl");
				var hacRowLayoutPanel = hacCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				AssertEquals("hacRowLayoutPanel.Controls.Count", 0, hacRowLayoutPanel.Controls.Count);
				var detailsTabPage = testForm.FindSingle<ZTabPage>("DetailsTabPage");
				mainTabControl.SelectedTab = detailsTabPage;
				houseBill.CA_RL_NK_PortOfDestination = "AUSYD";
				mainTabControl.SelectedTab = customFieldsTabPage;
				AssertEquals("hacRowLayoutPanel.Controls.Count", 2, hacRowLayoutPanel.Controls.Count);
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZArchitecture.ZCalcEdit>((x) => x.BindTo.Contains("HACSHORT")));
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZCheckBox>((x) => x.BindTo.Contains("HACBOOLEAN")));
				mainTabControl.SelectedTab = detailsTabPage;
				houseBill.CA_RL_NK_PortOfDestination = "AUMEL";
				mainTabControl.SelectedTab = customFieldsTabPage;
				AssertEquals("hacRowLayoutPanel.Controls.Count", 1, hacRowLayoutPanel.Controls.Count);
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZDateEdit>((x) => x.BindTo.Contains("HACDATETIME")));
			}
		}

		public void TestUnderbondMovementTab()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			var underbond = houseBill.AllUnderbonds.AddNew();
			underbond.C4_SendersMessageReference = "U00003137";
			using (var testForm = new ZForm(houseBill))
			using (var seaCargoHouseUserControl = new SeaCargoHouseUserControl())
			{
				testForm.Controls.Add(seaCargoHouseUserControl);
				seaCargoHouseUserControl.SetDataBinding(houseBill, "");
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var customsUnderbondMovementTabPage = mainTabControl.FindSingle<ZTabPage>("CustomsUnderbondMovementTabPage");
				mainTabControl.SelectedTab = customsUnderbondMovementTabPage;
				var underbondMainTabControl = customsUnderbondMovementTabPage.FindSingle<ZTemplateTabControl>("MainTabControl");
				var underbondDetailsTabPage = underbondMainTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				underbondMainTabControl.SelectedTab = underbondDetailsTabPage;
				var underbondSendersReferenceTextBox = underbondDetailsTabPage.FindSingle<ZArchitecture.ZTextBox>("SendersReferenceTextBox");
				AssertEquals("U00003137", underbondSendersReferenceTextBox.Text);
				AssertNotNull("MessagesTabPage is visible", underbondMainTabControl.FindSingle<ZTabPage>("MessagesTabPage"));
				AssertEquals("OutturnTabPage is hidden", 2, underbondMainTabControl.TabCount);
			}
		}
	}
}
