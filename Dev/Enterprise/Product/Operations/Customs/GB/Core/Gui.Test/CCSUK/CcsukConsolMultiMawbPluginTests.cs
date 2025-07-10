using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukConsolMultiMawbPluginTests : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			// Add two Awbs to the consol so that the right usercontrol is loaded and bashed
			consol = MakeGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			return new CcsukConsolMultiMawbPlugin(consol);
		}

		public void TestType()
		{
			consol = MakeGoodConsol();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertType(typeof(CcsukConsolMultiMawbPlugin), plugin);
				}
			}
		}

		public void TestPluginIsEnabledEvenForRoadWhenOptionsAreCorrect()
		{
			GBCustomsDataRegistry.Instance.CcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			consol = MakeGoodConsol(Core.Constants.TransportModes.Road);
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals("Plugin is disabled because there's no GB air shipment", false, plugin.Enabled);
				}
			}

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "GBLHR";
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals("Plugin is enabled now that we have a GB air shipment", true, plugin.Enabled);
				}
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals("Plugin is disabled because there's no GB air shipment", false, plugin.Enabled);
				}
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			GBCustomsDataRegistry.Instance.CcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals("Plugin is disabled because registry is disabled", false, plugin.Enabled);
				}
			}
		}

		public void TestNoBasicExists()
		{
			consol = MakeGoodConsol();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertNoCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNoMawbExistsInFactory();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertNoMawbExistsInFactory();
				AssertEquals(CcsukConsolMultiMawbPlugin.YouHaveChosenNotToCreateARecordNowText, ccsukPluginTab.PlugInNotDisplayedMessage);
				AssertNoCcsukMenu(consolForm);

				consolForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Routing);
				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ccsukPluginTab.SelectTabPage();
				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
				var basicCreated = consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertNotNull("Basic should have been created", basicCreated);
				AssertHasCcsukMenu(consolForm);
				AssertEquals("Basic is linked by FK to consol to allow opening of consol form from ccsuk module", consol.PK, basicCreated.CM_JK);
				AssertEquals("Basic has NK to consol", "12512345678", basicCreated.CM_MAWB);
				AssertContains("Synchronised to CCSUK MAWB 125-12345678 LHR", consol.Logs.MostRecentLog.SL_Reference);
			}

			//Save and reopen
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertHasCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				Assert("No question about creating", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));

				var basicReloaded = consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertNotNull("Basic should have been reloaded", basicReloaded);
				AssertHasCcsukMenu(consolForm);
				AssertEquals("No additional basic created, check by FK", 1, consol.Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				AssertEquals("No additional basic created, check by NK", 1, consol.Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, consol.JK_MasterBillNum)).Length);
			}

			Env.Security.AirCcsukMaster.IsAllowed = false;
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = (CcsukConsolMultiMawbPlugin)consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals("Plugin is disabled if no master security right", false, plugin.Enabled);
				}
			}
			Env.Security.AirCcsukMaster.IsAllowed = true;
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				using (var plugin = (CcsukConsolMultiMawbPlugin)consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory))
				{
					AssertEquals(true, plugin.Enabled);
				}
			}
		}

		public void TestNoMawbExists1()
		{
			consol = MakeGoodConsol();
			var shipment01 = consol.Shipments.AddNew();
			shipment01.JS_RL_NKDestination = "GBLHR";
			shipment01.JS_HouseBill = "DANIEL01";
			shipment01.JS_UniqueConsignRef = "S01";
			var shipment02 = consol.Shipments.AddNew();
			shipment02.JS_RL_NKDestination = "GBLHR";
			shipment02.JS_HouseBill = "DANIEL02";
			shipment02.JS_UniqueConsignRef = "S02";

			Env.Security.AirCcsukHouse.IsAllowed = false;
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertNoCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNoMawbExistsInFactory();

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ccsukPluginTab.SelectTabPage();
				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
				Assert("No right - does not prompt to create HAWBs", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("No local HAWBs exist for these shipments. Create them?"));
				Assert("No right - warns about security", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("security"));

				var mawbCreated = consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertNotNull("Mawb should have been created", mawbCreated);
				AssertEquals("No hawb created", 0, mawbCreated.ChildBills.Count);
			}
		}

		public void TestNoMawbExists2()
		{
			consol = MakeGoodConsol();
			var shipment01 = consol.Shipments.AddNew();
			shipment01.JS_RL_NKDestination = "GBLHR";
			shipment01.JS_HouseBill = "DANIEL01";
			shipment01.JS_UniqueConsignRef = "S01";
			var shipment02 = consol.Shipments.AddNew();
			shipment02.JS_RL_NKDestination = "GBLHR";
			shipment02.JS_HouseBill = "DANIEL02";
			shipment02.JS_UniqueConsignRef = "S02";

			Env.Security.AirCcsukHouse.IsAllowed = true;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertNoCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNoMawbExistsInFactory();

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ccsukPluginTab.SelectTabPage();

				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
				Assert("Warning about making hawbs too", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("No local HAWBs exist for these shipments. Create them?\r\n\r\n\tDANIEL01 for shipment S01\r\n\tDANIEL02 for shipment S02"));
				Assert("Rights - no warning", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("security"));

				var mawbCreated = consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertNotNull("Mawb should have been created", mawbCreated);
				AssertHasCcsukMenu(consolForm);
				AssertEquals("Mawb is linked by FK to consol to allow opening of consol form from ccsuk module", consol.PK, mawbCreated.CM_JK);
				AssertEquals("Mawb has NK to consol", "12512345678", mawbCreated.CM_MAWB);
				var hawbCreated = mawbCreated.ChildBills[0];
				AssertNotNull(hawbCreated);
				AssertEquals(shipment01.PK, hawbCreated.CS_JS);
				AssertEquals("DANIEL01", hawbCreated.CS_HAWB);
			}

			//Save and reopen
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertHasCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				Assert("No question about creating", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));

				var mawbReloaded = consol.Factory.LoadTop1<CusMAWB>(new ZQuery());
				AssertNotNull("Mawb should have been reloaded", mawbReloaded);
				AssertHasCcsukMenu(consolForm);
				AssertEquals("No additional mawb created, check by FK", 1, consol.Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				AssertEquals("No additional mawb created, check by NK", 1, consol.Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, consol.JK_MasterBillNum)).Length);
				AssertEquals("No additional hawb created, check by FK", 1, consol.Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment01.PK)).Length);
				AssertEquals("No additional hawb created, check by NK", 1, consol.Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, shipment01.JS_HouseBill)).Length);
			}
		}

		public void TestOneBasicExistsByNkNotFk()
		{
			consol = MakeGoodConsol();
			var existingBasic = Factory.New<CusMAWB>();
			existingBasic.CM_MAWB = consol.JK_MasterBillNum;  // NK link
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertHasCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertEquals("Has NO question about creating", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertEquals("Basic has is wired to consol by FK", consol.PK, existingBasic.CM_JK);
				AssertEquals("No additional basic created, check by FK", 1, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				AssertEquals("No additional basic created, check by NK", 1, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, consol.JK_MasterBillNum)).Length);
			}
		}

		public void TestSeveralBasicsExistAtVariousSheds()
		{
			consol = MakeGoodConsol();
			var existingMawb1 = Factory.New<CusMAWB>();
			existingMawb1.CM_MAWB = consol.JK_MasterBillNum;
			existingMawb1.CargoTerminalOperator = "DJC";
			existingMawb1.CargoTerminalOperatorAirport = "LHR";
			existingMawb1.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			var existingMawb2 = Factory.New<CusMAWB>();
			existingMawb2.CM_MAWB = consol.JK_MasterBillNum;
			existingMawb2.CargoTerminalOperator = "LSM";
			existingMawb2.CargoTerminalOperatorAirport = "LHR";
			var existingMawb3AlsoLinkedByFK = Factory.New<CusMAWB>();
			existingMawb3AlsoLinkedByFK.CM_MAWB = consol.JK_MasterBillNum;
			existingMawb3AlsoLinkedByFK.CargoTerminalOperator = "LSM";
			existingMawb3AlsoLinkedByFK.CargoTerminalOperatorAirport = "MAN";
			existingMawb3AlsoLinkedByFK.CM_JK = consol.PK;
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var ccsukMenu = AssertHasCcsukMenu(consolForm);
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678 @ LHRDJC CA"));
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678 @ LHRLSM "));
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678 @ MANLSM "));

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertEquals("Has NO question about creating", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertEquals("Mawb has is wired to consol by FK", consol.PK, existingMawb1.CM_JK);
				AssertEquals("Mawb has is wired to consol by FK", consol.PK, existingMawb2.CM_JK);
				AssertEquals("Mawb has is not unwired from consol ", consol.PK, existingMawb3AlsoLinkedByFK.CM_JK);
				AssertEquals("No additional mawb created, check by FK", 3, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				AssertEquals("No additional mawb created, check by NK", 3, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, consol.JK_MasterBillNum)).Length);
			}
		}

		public void TestSeveralBasicsExistButOneIsForeign()
		{
			consol = MakeGoodConsol();
			var gbExistingMawb = Factory.New<CusMAWB>();
			gbExistingMawb.CM_MAWB = consol.JK_MasterBillNum;
			gbExistingMawb.CM_JK = consol.PK;
			gbExistingMawb.CargoTerminalOperator = "DJC";
			gbExistingMawb.CargoTerminalOperatorAirport = "LHR";
			gbExistingMawb.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			var gbExistingMawb2AlsoLinkedByFK = Factory.New<CusMAWB>();
			gbExistingMawb2AlsoLinkedByFK.CM_MAWB = consol.JK_MasterBillNum;
			gbExistingMawb2AlsoLinkedByFK.CargoTerminalOperator = "LSM";
			gbExistingMawb2AlsoLinkedByFK.CargoTerminalOperatorAirport = "MAN";
			gbExistingMawb2AlsoLinkedByFK.CM_JK = consol.PK;
			Factory.Save();
			var auExistingMawbInOtherFactory = new BusinessObjectFactory().New<Customs.Business.CusMAWB>();
			auExistingMawbInOtherFactory.CM_MAWB = consol.JK_MasterBillNum;
			auExistingMawbInOtherFactory.CM_JK = consol.PK;
			auExistingMawbInOtherFactory.Factory.Save();
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var memoryOnlyQuery = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
				memoryOnlyQuery.FetchOnlyFromLocalCache = true;
				AssertEquals("The AU mawb was not loaded by the plugin", 2, Factory.Load<CusMAWB>(memoryOnlyQuery).Length);
			}
		}

		public void TestReloadFormForConcurrency()
		{
			consol = MakeGoodConsol();
			var existingBasic = Factory.New<CusMAWB>();
			existingBasic.CM_MAWB = consol.JK_MasterBillNum;  // NK link
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				AssertHasCcsukMenu(consolForm);

				var ccsukPluginTab = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventory) as CcsukConsolMultiMawbPlugin;
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNotNull(ccsukPluginTab.UserControl);

				existingBasic.Delete();
				Factory.Save();

				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertEquals(0, ccsukPluginTab.PluginHelper.ResetReloadAndCount());
				AssertNull(ccsukPluginTab.BusinessEntity);

				AssertEquals("Someone else has modified the CCS-UK job for this consol - please reload the form.", ((IPlugInInternals)ccsukPluginTab).CoveringLabel.Text);
			}
		}

		public void TestNoBindingExceptionWhenMultipleInstancesCreateMawbs()
		{
			consol = MakeGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			mawb1.CM_JK = consol.PK;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var ccsukPluginTab =
					((CcsukConsolMultiMawbPlugin)consolForm.PlugIns.GetPlugIn(
						ControllerIDs.Customs.GB.CcsukAirInventory)).TabPage;
				var ccsukMenu = AssertHasCcsukMenu(consolForm);
				ccsukMenu.PerformClick();
				var mawb2 = Factory.New<CusMAWB>();
				mawb2.CM_MAWB = consol.JK_MasterBillNum;
				mawb2.CM_JK = consol.PK;

				var tabControl = (ZTabControl)ccsukPluginTab.Parent;
				tabControl.SelectedTab = ccsukPluginTab;
			}
		}

		void AssertNoMawbExistsInFactory()
		{
			AssertEquals("No mawb was created", 0, consol.Factory.Load<CusMAWB>(new ZQuery()).Length);
		}

		void AssertNoCcsukMenu(ConsolForm consolForm)
		{
			var menu = consolForm.Menu;
			AssertNull(menu.MenuItems.FindByText(CcsukMenu.MenuCaption));
		}

		MenuItem AssertHasCcsukMenu(ConsolForm consolForm)
		{
			var mainMenu = consolForm.Menu;
			var ccsukMenu = mainMenu.MenuItems.FindByText(CcsukMenu.MenuCaption);
			AssertNotNull(ccsukMenu);
			return ccsukMenu;
		}

		ForwardingConsol MakeGoodConsol(string transportMode = Core.Constants.TransportModes.Air)
		{
			var c = Factory.New<ForwardingConsol>();
			c.JK_TransportMode = transportMode;
			c.JK_MasterBillNum = "12512345678";
			c.JK_RL_NKDischargePort = "GBLHR";
			return c;
		}

		ForwardingConsol consol;
	}
}
