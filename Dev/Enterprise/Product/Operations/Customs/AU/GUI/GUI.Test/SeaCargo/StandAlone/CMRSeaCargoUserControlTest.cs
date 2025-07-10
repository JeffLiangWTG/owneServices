using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI.Layout;
using Enterprise.Core.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class CMRSeaCargoUserControlTest : TestCaseWithFactory
	{
		[StressTest]
		public void TestCustomFields()
		{
			var acrTemplate1 = Factory.New<ProcessTaskTemplate>();
			acrTemplate1.P0_ProcessType = WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode;
			acrTemplate1.P0_Name = "TEST ACR 1";
			acrTemplate1.P0_Description = "TEST ACR 1 DESC";
			acrTemplate1.P0_DischargePortCountry = "NZAKL";
			var acrCustomField1 = acrTemplate1.GenCustomColumnDefinitions.AddNew();
			acrCustomField1.XC_Name = "ACRSTRING";
			acrCustomField1.XC_Type = AddOnColumnDataType.Codes.String;
			var acrCustomField2 = acrTemplate1.GenCustomColumnDefinitions.AddNew();
			acrCustomField2.XC_Name = "ACRINT";
			acrCustomField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			var acrTemplate2 = Factory.New<ProcessTaskTemplate>();
			acrTemplate2.P0_ProcessType = WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode;
			acrTemplate2.P0_Name = "TEST ACR 2";
			acrTemplate2.P0_Description = "TEST ACR 2 DESC";
			acrTemplate2.P0_DischargePortCountry = "NZCHC";
			var acrCustomField3 = acrTemplate2.GenCustomColumnDefinitions.AddNew();
			acrCustomField3.XC_Name = "ACRDECIMAL";
			acrCustomField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
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
			using (ZForm testForm = new ZForm(oceanBill))
			{
				CMRSeaCargoUserControl control = new CMRSeaCargoUserControl();
				testForm.Controls.Add(control);
				control.SetDataBinding(oceanBill, "");
				testForm.Show();
				var oceanBillTabControl = testForm.FindSingle<ZTemplateTabControl>("OceanBillSpecificsTabControl");
				var masterCustomFieldsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
				oceanBillTabControl.SelectedTab = masterCustomFieldsTabPage;
				var masterAirCargoCustomFieldsControl = masterCustomFieldsTabPage.FindSingle<SeaCargoCustomFieldsUserControl>("CustomFieldsControl");
				var acrCustomFieldsControl = masterAirCargoCustomFieldsControl.FindSingle<ProcessTemplateCustomFieldsControl>("CustomFieldsControl");
				var acrRowLayoutPanel = acrCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				AssertEquals("acrRowLayoutPanel.Controls.Count", 0, acrRowLayoutPanel.Controls.Count);
				oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
				oceanBillTabControl.SelectedTab = masterCustomFieldsTabPage;
				AssertEquals("acrRowLayoutPanel.Controls.Count", 2, acrRowLayoutPanel.Controls.Count);
				AssertNotNull(acrRowLayoutPanel.FindSingle<ZTextBox>((x) => x.BindTo.Contains("ACRSTRING")));
				AssertNotNull(acrRowLayoutPanel.FindSingle<ZCalcEdit>((x) => x.BindTo.Contains("ACRINT")));
				oceanBill.CB_RL_NKPortOfDischarge = "NZCHC";
				oceanBillTabControl.SelectedTab = masterCustomFieldsTabPage;
				AssertEquals("acrRowLayoutPanel.Controls.Count", 1, acrRowLayoutPanel.Controls.Count);
				AssertNotNull(acrRowLayoutPanel.FindSingle<ZCalcEdit>((x) => x.BindTo.Contains("ACRDECIMAL")));
				var houseBillsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("HouseBillsTabPage");
				var houseBillsGrid = houseBillsTabPage.FindSingle<ZGrid>("HouseBillsGrid");
				oceanBillTabControl.SelectedTab = houseBillsTabPage;
				houseBillsGrid.Select(0);
				var billTabControl = houseBillsTabPage.FindSingle<ZTabControl>("HouseBillTabControl");
				var billDetailsTabPage = billTabControl.FindSingle<ZTabPage>("DetailsTabPage");
				var billCustomFieldsTabPage = billTabControl.FindSingle<ZTabPage>("HouseBillCustomFieldsTabPage");
				var billHouseAirCargoCustomFieldsControl = billCustomFieldsTabPage.FindSingle<SeaCargoHouseBillCustomFieldsUserControl>("HouseBillCustomFieldsControl");
				var hacCustomFieldsControl = billHouseAirCargoCustomFieldsControl.FindSingle<ProcessTemplateCustomFieldsControl>("CustomFieldsControl");
				var hacRowLayoutPanel = hacCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				AssertEquals("hacRowLayoutPanel.Controls.Count", 0, hacRowLayoutPanel.Controls.Count);
				billTabControl.SelectedTab = billDetailsTabPage;
				houseBill.CA_RL_NK_PortOfDestination = "AUSYD";
				billTabControl.SelectedTab = billCustomFieldsTabPage;
				AssertEquals("hacRowLayoutPanel.Controls.Count", 2, hacRowLayoutPanel.Controls.Count);
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZCalcEdit>((x) => x.BindTo.Contains("HACSHORT")));
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZCheckBox>((x) => x.BindTo.Contains("HACBOOLEAN")));
				billTabControl.SelectedTab = billDetailsTabPage;
				houseBill.CA_RL_NK_PortOfDestination = "AUMEL";
				billTabControl.SelectedTab = billCustomFieldsTabPage;
				AssertEquals("hacRowLayoutPanel.Controls.Count", 1, hacRowLayoutPanel.Controls.Count);
				AssertNotNull(hacRowLayoutPanel.FindSingle<ZDateEdit>((x) => x.BindTo.Contains("HACDATETIME")));
			}
		}

		public void TestUserFriendlyStatuses()
		{
			var ocean = Factory.New<CusSCAOceanBill>();
			var container = ocean.Containers.AddNew();
			var house1 = ocean.FilteredHouseBills.AddNew();
			var house2 = ocean.FilteredHouseBills.AddNew();
			container.Pivots.Add(house2.Pivot.AddNew());
			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2HF2 5CBC D7BF:1+8'
DTM+9:20051103122504817010:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+QA123++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00001052/SYD1::1'
RFF+MB:1234564'
RFF+AAQ:LCLU99999999'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
			message.EM_LinkedObject = house2.Pivot[0];
			message.EM_LinkUniqueID = house2.Pivot[0].PK;
			message.EM_LinkTable = "CusSCAHouse";
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			house2.Pivot[0].Messages.Add(message);
			using (ZForm testForm = new ZForm(ocean))
			{
				CMRSeaCargoUserControl control = new CMRSeaCargoUserControl();
				testForm.Controls.Add(control);
				control.SetDataBinding(ocean, "");
				testForm.Show();
				control.OceanBillSpecificsTabControl.SelectedIndex = 1;
				control.HouseBillsGrid.ListManager.Position = control.HouseBillsGrid.List.IndexOf(house2);
				string statusResult = @"CONSOLIDATED STATUS : HELD
COMPLETE UNDERBOND SERIES APPROVED : N/A
LCL UNDERBOND SATISFIED : NO
CARGO REPORT ACS EVALUATED : NO
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
				var button = control.FindSingle<ZButton>("DetailsButton");
				button.PerformClick();
				AssertEquals(statusResult, UnitTestUserNotification.Instance.LastMessage.Text);
				control.HouseBillsGrid.ListManager.Position = control.HouseBillsGrid.List.IndexOf(house1);
				button.PerformClick();
				AssertEquals(UserFriendlyStatusMessages.StatusNotAvailable, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBindingToMessagesForBinding()
		{
			using (var control = new CMRSeaCargoUserControl())
			{
				AssertEquals("FilteredHouseBills.MessagesForBinding", control.MessageUserControl.MessagesGrid.BindTo);
				AssertStartsWith("Replaced binding string", "FilteredHouseBills.MessagesForBinding", control.MessageUserControl.MessageTextTextBox.BindTo);
			}
		}

		public void TestOnDeletingOceanBillIsHookedUp()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (CMRSeaCargoUserControl control = new CMRSeaCargoUserControl())
			{
				control.SetDataBinding(oceanBill, "");
				var	 houseToRemove = oceanBill.FilteredHouseBills.AddNew();
				houseToRemove.CA_ShipmentStatus = CMRBaseStatuses.Codes.WithdrawalAccepted;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				oceanBill.FilteredHouseBills.RemoveAndDelete(houseToRemove);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var houseDisallowedToRemove = oceanBill.FilteredHouseBills.AddNew();
				houseDisallowedToRemove.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				oceanBill.FilteredHouseBills.RemoveAndDelete(houseDisallowedToRemove);
				AssertEquals("You should withdraw the HouseBill prior to delete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPlugInExists()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (ZForm testForm = new ZForm(oceanBill))
			{
				using (CMRSeaCargoUserControl control = new CMRSeaCargoUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					AssertNotNull("Plug In does not exist", control.OceanBillSpecificsTabControl.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController));
				}
			}
		}

		public void TestDropEditDefaultVisibility()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControl())
			{
				testForm.Controls.Add(seaCargoUserControl);
				testForm.Show();
				AssertEquals("Drop edit Should be visible", true, seaCargoUserControl.FindSingle<ZDropEdit>("CB_ApplicationCodeBoundDropEdit").Visible);
				AssertEquals("Drop edit Should be visible", true, seaCargoUserControl.FindSingle<ZLabel>("MessagingModeLabel").Visible);
			}
		}

		public void TestIsStandalone()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControl())
			{
				testForm.Controls.Add(seaCargoUserControl);
				seaCargoUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				var oceanBillDetailsUserControl = testForm.FindSingle<Control>("OceanBillDetailsUserControl");
				AssertEquals("MessagingModeLabel.Visible", true, oceanBillDetailsUserControl.FindSingle<Control>("MessagingModeLabel").Visible);
				AssertEquals("CB_ApplicationCodeBoundDropEdit.Visible", true, oceanBillDetailsUserControl.FindSingle<Control>("CB_ApplicationCodeBoundDropEdit").Visible);
			}

			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControl(true))
			{
				testForm.Controls.Add(seaCargoUserControl);
				seaCargoUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				var oceanBillDetailsUserControl = testForm.FindSingle<Control>("OceanBillDetailsUserControl");
				AssertEquals("MessagingModeLabel.Visible", false, oceanBillDetailsUserControl.FindSingle<Control>("MessagingModeLabel").Visible);
				AssertEquals("CB_ApplicationCodeBoundDropEdit.Visible", false, oceanBillDetailsUserControl.FindSingle<Control>("CB_ApplicationCodeBoundDropEdit").Visible);
			}
		}

		public void TestOutturnControlDisabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (ZForm testForm = new ZForm(oceanBill))
			using (CMRSeaCargoUserControl control = new CMRSeaCargoUserControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.OceanBillSpecificsTabControl.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.AU.CusSCAContainerUnderbondPluginController);
				CusUnderbondUserControl userControl = FindUnderbondControl(control.OceanBillSpecificsTabControl.SelectedTab);
				AssertEquals("Outturn should be disabled", true, userControl.OutturnDisabled);
			}
		}

		[ExpectNoExceptions()]
		public void TestIssue00007869()
		{
			using (ZForm testForm = new ZForm(Factory.New(typeof(CusSCAOceanBill))))
			{
				using (CMRSeaCargoUserControl control = new CMRSeaCargoUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					control.SetDataBinding(null, "");
				}
			}
		}

		public void TestEditHouseBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Enterprise.Core.Constants.CountryCodes.Australia, ZDateTime.Now, true))
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControlForTesting())
			{
				try
				{
					testForm.Controls.Add(seaCargoUserControl);
					seaCargoUserControl.SetDataBinding(oceanBill, "");
					testForm.Show();
					var oceanBillTabControl = testForm.FindSingle<ZTemplateTabControl>("OceanBillSpecificsTabControl");
					var houseBillsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("HouseBillsTabPage");
					oceanBillTabControl.SelectedTab = houseBillsTabPage;
					var houseBillsGrid = houseBillsTabPage.FindSingle<ZGrid>("HouseBillsGrid");
					houseBillsGrid.ListManager.Position = -1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					houseBillsGrid.PerformDoubleClickForTest();
					AssertEquals("Please select a Housebill to Edit.", UnitTestUserNotification.Instance.LastMessage.Text);
					var editMenuItem = houseBillsGrid.ContextMenu.MenuItems.FindByText("Edit");
					AssertNotNull("Edit Menu is on Grid", editMenuItem);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					editMenuItem.PerformClick();
					AssertEquals("Please select a Housebill to Edit.", UnitTestUserNotification.Instance.LastMessage.Text);
					var house = oceanBill.FilteredHouseBills.AddNew();
					var shipment = Factory.New<ForwardingShipment>();
					house.CA_JS = shipment.PK;
					houseBillsGrid.ListManager.Position = 0;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					editMenuItem.PerformClick();
					AssertEquals("Please edit shipments from the Consol Details tab.", UnitTestUserNotification.Instance.LastMessage.Text);
					house.CA_JS = ZGuid.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					editMenuItem.PerformClick();
					AssertEquals("Please save the form before you edit this HouseBill.", UnitTestUserNotification.Instance.LastMessage.Text);
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					editMenuItem.PerformClick();
					AssertNull("No Error prompts", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertType<SeaCargoHouseForm>(seaCargoUserControl.LastShownForm);
				}
				finally
				{
					seaCargoUserControl.LastShownForm?.Dispose();
				}
			}
		}

		public void TestEditHouseBill_SeaCargoHouseDisabled()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControlForTesting())
			{
				try
				{
					testForm.Controls.Add(seaCargoUserControl);
					seaCargoUserControl.SetDataBinding(oceanBill, "");
					testForm.Show();
					var oceanBillTabControl = testForm.FindSingle<ZTemplateTabControl>("OceanBillSpecificsTabControl");
					var houseBillsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("HouseBillsTabPage");
					oceanBillTabControl.SelectedTab = houseBillsTabPage;
					var houseBillsGrid = houseBillsTabPage.FindSingle<ZGrid>("HouseBillsGrid");
					houseBillsGrid.ListManager.Position = -1;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					houseBillsGrid.PerformDoubleClickForTest();
					AssertNull("No Error prompts", UnitTestUserNotification.Instance.LastMessage.Text);
					var house = oceanBill.FilteredHouseBills.AddNew();
					houseBillsGrid.ListManager.Position = 0;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					houseBillsGrid.PerformDoubleClickForTest();
					AssertNull("No Error prompts", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Form was not shown", seaCargoUserControl.LastShownForm);
					var editMenuItem = houseBillsGrid.ContextMenu.MenuItems.FindByText("Edit");
					AssertNull("Edit Menu is not on Grid", editMenuItem);
				}
				finally
				{
					seaCargoUserControl.LastShownForm?.Dispose();
				}
			}
		}

		public void TestHouseBillPackingContainersGridIsReadOnly()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var house = oceanBill.FilteredHouseBills.AddNew();
			house.CA_HouseBill = "HB1";
			var pivot = house.Pivot.AddNew();
			var container = oceanBill.Containers.AddNew();
			container.Pivots.Add(pivot);
			AssertSame("pivot.Container is container", container, pivot.Container);
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControlForTesting())
			{
				testForm.Controls.Add(seaCargoUserControl);
				seaCargoUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();
				var oceanBillTabControl = testForm.FindSingle<ZTemplateTabControl>("OceanBillSpecificsTabControl");
				var houseBillsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("HouseBillsTabPage");
				oceanBillTabControl.SelectedTab = houseBillsTabPage;
				var houseBillsGrid = houseBillsTabPage.FindSingle<ZGrid>("HouseBillsGrid");
				houseBillsGrid.ListManager.Position = 0;
				var mainTabControl = houseBillsTabPage.FindSingle<ZTemplateTabControl>("HouseBillTabControl");
				var packingTabPage = mainTabControl.FindSingle<ZTabPage>("PackingTabPage");
				mainTabControl.SelectedTab = packingTabPage;
				var containersGrid = packingTabPage.FindSingle<ZGrid>("ContainersGrid");
				containersGrid.CurrentRowIndex = 1;
				AssertEquals("Associated Container is not readonly", false, containersGrid.GetColumnStyle(CusSCAPivot.Schema.CV_AssociatedContainer).IsReadOnly);
				AssertEquals("CN_SealNumber is ReadOnly", true, containersGrid.GetColumnStyle(CusSCAPivot.Schema.CN_SealNumber).IsReadOnly);
			}
		}

		public void TestHouseBillGridColumnsAreMappedForDataImport()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB001";
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "HB1";
			var pivot = houseBill.Pivot.AddNew();
			var container = oceanBill.Containers.AddNew();
			container.Pivots.Add(pivot);
			AssertSame("pivot.Container is container", container, pivot.Container);
			using (var testForm = new ZForm(oceanBill))
			using (var seaCargoUserControl = new CMRSeaCargoUserControlForTesting())
			{
				testForm.Controls.Add(seaCargoUserControl);
				seaCargoUserControl.SetDataBinding(oceanBill, "");
				testForm.Show();

				var oceanBillTabControl = testForm.FindSingle<ZTemplateTabControl>("OceanBillSpecificsTabControl");
				var houseBillsTabPage = oceanBillTabControl.FindSingle<ZTabPage>("HouseBillsTabPage");
				oceanBillTabControl.SelectedTab = houseBillsTabPage;
				var houseBillsGrid = houseBillsTabPage.FindSingle<ZGrid>("HouseBillsGrid");

				CombineAssertions("Grid Column has matching PropertyInfo", () =>
				{
					foreach (ZGridColumnInfo column in houseBillsGrid.ColumnStyles)
					{
						Assert(column.ColumnName, houseBill.ZPropertyInfoHash.ContainsKey(column.ColumnName));
					}
				});
			}
		}

		CusUnderbondUserControl FindUnderbondControl(Control parent)
		{
			CusUnderbondUserControl result = parent as CusUnderbondUserControl;
			if (result == null)
			{
				foreach (Control control in parent.Controls)
				{
					result = FindUnderbondControl(control);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		sealed class CMRSeaCargoUserControlForTesting : CMRSeaCargoUserControl
		{
			public CMRSeaCargoUserControlForTesting() : base()
			{
			}

			public IZForm LastShownForm => lastShownForm;
		}
	}
}
