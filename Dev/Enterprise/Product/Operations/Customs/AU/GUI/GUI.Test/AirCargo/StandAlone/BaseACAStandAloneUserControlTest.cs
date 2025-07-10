using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI.Layout;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class BaseACAStandAloneUserControlTest : TestCaseWithFactory
	{
		public void TestAirCargoCustomFieldsControl()
		{
			var mawb = Factory.New<CusMAWB>();
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var houseBillsPanel = form.FindSingle<Control>("HouseBillsPanel");
				var masterTabControl = houseBillsPanel.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var customFieldsTabPage = houseBillsPanel.FindSingle<ZTabPage>("CustomFieldsTabPage");
				masterTabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = customFieldsTabPage.FindSingle<Control>("AirCargoCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var wrapperControl = (Customs.GUI.CustomFieldsWrapperControl)airCargoCustomFieldsControl;
				AssertEquals("To make use of this tab, please setup Air Cargo (ACR) custom fields in Workflow Manager", wrapperControl.NothingSetupMessageLabelText);
			}
		}

		public void TestAirCargoCustomFieldsControl_LoadsCustomFields()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template1.P0_Name = "ACR-AU";
			template1.P0_Description = "TEST ACR for AU";
			template1.P0_LoadPortCountry = "";
			template1.P0_DischargePortCountry = "AU";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestStringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			ICustomFieldProvider customFieldProvider = mawb;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == "ZString");
			AssertContains("TestStringField", customProperty.Identifier, ignoreCase: true);
			customProperty.TrySetValue(mawb, new ZString("Test Value"));
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var houseBillsPanel = form.FindSingle<Control>("HouseBillsPanel");
				var masterTabControl = houseBillsPanel.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var customFieldsTabPage = houseBillsPanel.FindSingle<ZTabPage>("CustomFieldsTabPage");
				masterTabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = customFieldsTabPage.FindSingle<Control>("AirCargoCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var customFieldPanel = airCargoCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				var customField = customFieldPanel.Controls.OfType<ZTextBox>().FirstOrDefault(x => x.Text == "Test Value");
				AssertNotNull(customField);
			}
		}

		public void TestAirCargoCustomFieldsControl_CustomBusinessObjectChanged()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template1.P0_Name = "ACR-NZ";
			template1.P0_Description = "TEST ACR for NZ";
			template1.P0_LoadPortCountry = "AU";
			template1.P0_DischargePortCountry = "NZ";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestStringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template2.P0_Name = "ACR-US";
			template2.P0_Description = "TEST ACR for US";
			template2.P0_LoadPortCountry = "AU";
			template2.P0_DischargePortCountry = "US";
			var customField2 = template2.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "TestIntegerField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var houseBillsPanel = form.FindSingle<Control>("HouseBillsPanel");
				var masterTabControl = houseBillsPanel.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var customFieldsTabPage = houseBillsPanel.FindSingle<ZTabPage>("CustomFieldsTabPage");
				masterTabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = customFieldsTabPage.FindSingle<Control>("AirCargoCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var customFieldPanel = airCargoCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				var customTextField_NZ = customFieldPanel.Controls.OfType<ZTextBox>().FirstOrDefault();
				AssertNotNull(customTextField_NZ);
				AssertContains("TestStringField", ((CargoWise.Windows.UI.IDataBoundControl)customTextField_NZ).DataMember, true);
				AssertEquals(1, customFieldPanel.Controls.Count);
				mawb.CM_RL_NKDischargePort = "USLAX";
				var customIntegerField_US = customFieldPanel.Controls.OfType<ZCalcEdit>().FirstOrDefault();
				AssertNotNull(customIntegerField_US);
				AssertContains("TestIntegerField", ((CargoWise.Windows.UI.IDataBoundControl)customIntegerField_US).DataMember, true);
				AssertEquals(1, customFieldPanel.Controls.Count);
			}
		}

		public void TestAirCargoHouseCustomFieldsControl()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.FilteredChildBills.AddNew();
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var houseBillsPanel = form.FindSingle<Control>("HouseBillsPanel");
				var masterTabControl = houseBillsPanel.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var houseBillsTabPage = houseBillsPanel.FindSingle<ZTabPage>("houseBillsTabPage");
				masterTabControl.SelectTab(houseBillsTabPage);
				var hawbTabControl = houseBillsTabPage.FindSingle<ZTemplateTabControl>("HAWBTabControl");
				var houseCustomFieldsTabPage = houseBillsTabPage.FindSingle<ZTabPage>("HouseCustomFieldsTabPage");
				hawbTabControl.SelectTab(houseCustomFieldsTabPage);
				var airCargoHouseCustomFieldsControl = houseCustomFieldsTabPage.FindSingle<Control>("AirCargoHouseCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoHouseCustomFieldsControl loaded", airCargoHouseCustomFieldsControl.Visible);
				var wrapperControl = (Customs.GUI.CustomFieldsWrapperControl)airCargoHouseCustomFieldsControl;
				AssertEquals("To make use of this tab, please setup Air Cargo House (HAC) custom fields in Workflow Manager", wrapperControl.NothingSetupMessageLabelText);
			}
		}

		public void TestAirCargoHouseCustomFieldsControl_LoadsCustomFields()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template1.P0_Name = "HAC-AU";
			template1.P0_Description = "TEST HAC for AU";
			template1.P0_LoadPortCountry = "";
			template1.P0_DischargePortCountry = "AU";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestStringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			var hawb = mawb.FilteredChildBills.AddNew();
			ICustomFieldProvider customFieldProvider = hawb;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var customProperty = customBusinessObject.CustomProperties.FirstOrDefault(x => x.Info.Type.Name == "ZString");
			AssertContains("TestStringField", customProperty.Identifier, ignoreCase: true);
			customProperty.TrySetValue(hawb, new ZString("Test Value"));
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var tabControl = form.FindSingle<ZTemplateTabControl>("HAWBTabControl");
				var customFieldsTabPage = form.FindSingle<ZTabPage>("HouseCustomFieldsTabPage");
				tabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = form.FindSingle<Control>("AirCargoHouseCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var customFieldPanel = airCargoCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				var customField = customFieldPanel.Controls.OfType<ZTextBox>().FirstOrDefault(x => x.Text == "Test Value");
				AssertNotNull(customField);
			}
		}

		public void TestAirCargoHouseCustomFieldsControl_CustomBusinessObjectChanged()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template1.P0_Name = "HAC-NZ";
			template1.P0_Description = "TEST HAC for NZ";
			template1.P0_LoadPortCountry = "AU";
			template1.P0_DischargePortCountry = "NZ";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "TestStringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template2.P0_Name = "HAC-US";
			template2.P0_Description = "TEST HAC for US";
			template2.P0_LoadPortCountry = "AU";
			template2.P0_DischargePortCountry = "US";
			var customField2 = template2.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "TestIntegerField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			var hawb = mawb.FilteredChildBills.AddNew();
			AssertEquals("Precondition", "AUSYD", hawb.CS_RL_NKOrigin);
			AssertEquals("Precondition", "NZAKL", hawb.CS_RL_NKDestination);
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var tabControl = form.FindSingle<ZTemplateTabControl>("HAWBTabControl");
				var customFieldsTabPage = form.FindSingle<ZTabPage>("HouseCustomFieldsTabPage");
				tabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = form.FindSingle<Control>("AirCargoHouseCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var customFieldPanel = airCargoCustomFieldsControl.FindSingle<RowLayoutPanel>("rowLayoutPanel");
				var customTextField_NZ = customFieldPanel.Controls.OfType<ZTextBox>().FirstOrDefault();
				AssertNotNull(customTextField_NZ);
				AssertContains("TestStringField", ((CargoWise.Windows.UI.IDataBoundControl)customTextField_NZ).DataMember, true);
				AssertEquals(1, customFieldPanel.Controls.Count);
				hawb.CS_RL_NKDestination = "USLAX";
				var customIntegerField_US = customFieldPanel.Controls.OfType<ZCalcEdit>().FirstOrDefault();
				AssertNotNull(customIntegerField_US);
				AssertContains("TestIntegerField", ((CargoWise.Windows.UI.IDataBoundControl)customIntegerField_US).DataMember, true);
				AssertEquals(1, customFieldPanel.Controls.Count);
			}
		}

		public void TestDoubleClickToEditHAWB()
		{
			var mawb = Factory.New<CusMAWB>();
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				var houseBillsModuleButtonGrid = form.FindSingle<ZGrid>("HouseBillsModuleButtonGrid");
				houseBillsModuleButtonGrid.PerformDoubleClickForTest();
				AssertEquals("Please select a Housebill to Edit.", UnitTestUserNotification.Instance.LastMessage.Text);
				mawb.FilteredChildBills.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				houseBillsModuleButtonGrid.PerformDoubleClickForTest();
				AssertEquals("Please save the form before you edit this HouseBill.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				object lastShownForm = null;
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					houseBillsModuleButtonGrid.PerformDoubleClickForTest();
					AssertNull("Please save the form before you edit this HouseBill.", UnitTestUserNotification.Instance.LastMessage.Text);
					lastShownForm = control.GetLastShownForm();
					AssertEquals(typeof(AirCargoHouseForm), lastShownForm.GetType());
				}
				finally
				{
					if (lastShownForm != null)
					{
						((ZForm)lastShownForm).Dispose();
					}
				}
			}
		}

		public void TestDoubleClickToEditHAWBOnShipmentNotAllowed()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			var mawb = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			var hawb = CusHAWB.CreateNew(mawb, shipment);
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var houseBillsModuleButtonGrid = form.FindSingle<ZGrid>("HouseBillsModuleButtonGrid");
				houseBillsModuleButtonGrid.PerformDoubleClickForTest();
				AssertEquals("Please edit shipments from the Consol Details tab.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHAWBisUpdatedWhenConsolFormIsUsed()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.FilteredChildBills.AddNew();
			var hawb2 = mawb.FilteredChildBills.AddNew();
			Factory.Save();
			using (var form = new ZForm(mawb))
			using (var userControl = new BaseACAStandAloneUserControlForTest())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.SetDataBinding(mawb, "");
				userControl.HouseBillsModuleButtonGrid.ListManager.Position = userControl.HouseBillsModuleButtonGrid.List.IndexOf(hawb1);
				userControl.HookHouseBillsModuleButtonGridEventsExposed();
				userControl.HouseBillsModuleButtonGrid.ListManager.Position = userControl.HouseBillsModuleButtonGrid.List.IndexOf(hawb2);
				AssertEquals(hawb2, userControl.HAWB);
				userControl.HouseBillsModuleButtonGrid.ListManager.Position = userControl.HouseBillsModuleButtonGrid.List.IndexOf(hawb1);
				AssertEquals(hawb1, userControl.HAWB);
			}
		}

		public void TestHousebillsGridEditMenu()
		{
			var mawb = Factory.New<CusMAWB>();
			using (var form = new ZForm(mawb))
			using (var control = new BaseACAStandAloneUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				MenuItem editMenuItem = null;
				foreach (MenuItem item in control.HouseBillsModuleButtonGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Edit")
					{
						editMenuItem = item;
						break;
					}
				}

				AssertNotNull("edit menu found", editMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editMenuItem.PerformClick();
				AssertEquals("Please select a Housebill to Edit.", UnitTestUserNotification.Instance.LastMessage.Text);
				mawb.FilteredChildBills.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editMenuItem.PerformClick();
				AssertEquals("Please save the form before you edit this HouseBill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.ActiveForm);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				object lastShownForm = null;
				try
				{
					editMenuItem.PerformClick();
					AssertNull("Please save the form before you edit this HouseBill.", UnitTestUserNotification.Instance.LastMessage.Text);
					lastShownForm = control.GetLastShownForm();
					AssertEquals(typeof(AirCargoHouseForm), lastShownForm.GetType());
				}
				finally
				{
					if (lastShownForm != null)
					{
						((ZForm)lastShownForm).Dispose();
					}
				}
			}
		}

		public void TestHousebillsGridEditHAWBOnShipmentNotAllowed()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			CusMAWB mawb = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			CusHAWB hawb = CusHAWB.CreateNew(mawb, shipment);
			Factory.Save();
			using (ZForm form = new ZForm(mawb))
			using (BaseACAStandAloneUserControl control = new BaseACAStandAloneUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(mawb, "");
				MenuItem editMenuItem = null;
				foreach (MenuItem item in control.HouseBillsModuleButtonGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Edit")
					{
						editMenuItem = item;
						break;
					}
				}

				AssertNotNull("edit menu found", editMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editMenuItem.PerformClick();
				AssertEquals("Please edit shipments from the Consol Details tab.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCMROnlyColumnsOnlyVisibleForCMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var mawb = Factory.New<CusMAWB>();
			using (var form = new AirCargoMasterForm(mawb))
			{
				form.Show();
				UserIdleWorker.Flush();
				BaseACAStandAloneUserControl control = form.AirCargoDeclarationUserControl;
				AssertGridColumnAvailable(control.HouseBillsModuleButtonGrid, Customs.Business.AutoCusHAWB.Schema.CS_IsPersonalEffects);
				AssertGridColumnAvailable(control.HouseBillsModuleButtonGrid, Customs.Business.AutoCusHAWB.Schema.CS_IsSelfAssessedClearance);
			}
		}

		public void TestCurrentHouseBillVisible()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.FilteredChildBills.AddNew();
			hawb1.CS_HAWB = "12345";
			var hawb2 = mawb.FilteredChildBills.AddNew();
			hawb2.CS_HAWB = "54321";
			using (var form = new AirCargoMasterFormTestHelper(mawb))
			{
				BaseACAStandAloneUserControl control = form.AirCargoDeclarationUserControl;
				form.Show();
				UserIdleWorker.Flush();
				control.HouseBillsModuleButtonGrid.ListManager.Position = control.HouseBillsModuleButtonGrid.List.IndexOf(hawb1);
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, form.AirCargoMenu, new object[] { EventArgs.Empty });
				AssertEquals("House Bill:" + hawb1.CS_HAWB, form.HouseMenu.Text);
				AssertEquals(hawb1, control.HAWB);
				control.HouseBillsModuleButtonGrid.ListManager.Position = control.HouseBillsModuleButtonGrid.List.IndexOf(hawb2);
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, form.AirCargoMenu, new object[] { EventArgs.Empty });
				AssertEquals("House Bill:" + hawb2.CS_HAWB, form.HouseMenu.Text);
				AssertEquals(hawb2, control.HAWB);
			}
		}

		void AssertGridColumnAvailable(ZGrid grid, string columnName)
		{
			AssertNotNull(columnName + " column availability", grid.Columns[columnName]);
		}

		sealed class BaseACAStandAloneUserControlForTest : BaseACAStandAloneUserControl
		{
			public BaseACAStandAloneUserControlForTest() : base()
			{
			}

			public void HookHouseBillsModuleButtonGridEventsExposed() => HookHouseBillsModuleButtonGridEvents();

			public object GetLastShownForm() => lastShownForm;
		}

		sealed class AirCargoMasterFormTestHelper : AirCargoMasterForm
		{
			public AirCargoMasterFormTestHelper(CusMAWB masterBill) : base(masterBill)
			{
			}

			public AirCargoMasterMenu AirCargoMenu => airCargoMenu;

			public AirCargoShipmentMenu HouseMenu => (AirCargoShipmentMenu)AirCargoMenu.GetType().InvokeMember("houseMenu", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, AirCargoMenu, null);
		}
	}
}
