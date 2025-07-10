using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	class CcsukShipmentMultiHawbPluginTests : ZPlugInGenericTest
	{
		public void TestAttachAConsolWithACcsukMawbToAShipment()
		{
			// Mawb with hawb. Consol. Consol linked to mawb. Shipment not yet linked to consol or hawb. Attach consol to shipment.  Press shipment's ccsuk tab with our newly-acquired consol. 
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "12512345678";
			consol.JK_RL_NKDischargePort = "GBLHR";
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_HouseBill = "DANIEL01";
			shipment.JS_RL_NKDestination = "GBLHR";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals("Plugin not yet enabled, no consol", false, plugin.Enabled);
					shipment.Consols.Add(consol);
					AssertEquals("Plugin is enabled now that we have a consol", true, plugin.Enabled);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					plugin.SelectTabPage();
					// No boom
					AssertEquals("No explosion, and shipment is no linked to hawb", shipment.PK, hawb.CS_JS);
				}
			}

			Env.Security.AirCcsukHouse.IsAllowed = false;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals("Plugin is disabled if no house security right", false, plugin.Enabled);
				}
			}
			Env.Security.AirCcsukHouse.IsAllowed = true;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals("Plugin is enabled if has house security right", true, plugin.Enabled);
				}
			}
		}

		public void TestDUCRHyperlinkVisibilityWhenHawbLinkedToShipment()
		{
			shipment = Factory.New<ForwardingShipment>();
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			Factory.Save();
			using (var hawbForm = new CcsukAirInventoryFormHouse(hawb))
			{
				hawbForm.Show();
				AssertEquals("DUCR link", true, hawbForm.Controls.Find("LinkLabelEntry", true)[0].Visible);
			}
			hawb.CS_JS = shipment.PK;
			Factory.Save();
			using (var hawbForm = new CcsukAirInventoryFormHouse(hawb))
			{
				hawbForm.Show();
				AssertEquals("No DUCR link", false, hawbForm.Controls.Find("LinkLabelEntry", true)[0].Visible);
			}
		}

		public void TestDUCRHyperlinkOpensUpInShipmentPlugin()
		{
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_HouseBill = "DANIEL01";
			shipment.JS_RL_NKDestination = "GBLHR";
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			hawb.CargoTerminalOperator = "ELX";
			hawb.CargoTerminalOperatorAirport = "LBA";
			hawb.CS_JS = shipment.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			var oFC = OpenedFormCache.GetInstance();
			var houseControiller = ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);

			using (var form = houseControiller.ShowEditForm(hawb))
			{
				form.Show();
				var awbControl = (AwbStatusUserControl)(((CcsukAirInventoryFormHouse)form).Controls.Find("awbStatusUserControl1", true)[0]);
				awbControl.LinkLabelEntry_LinkClicked(null, null);
				var shipmentForm = oFC.GetForm(hawb.Shipment.PK.ToGuid(), "JobShipment");
				AssertNotNull(shipmentForm);
				shipmentForm.Dispose();
			}

			hawb.Declaration.JE_JS = Guid.Empty;
			Factory.Save();

			using (var form = houseControiller.ShowEditForm(hawb))
			{
				form.Show();
				var awbControl = (AwbStatusUserControl)(((CcsukAirInventoryFormHouse)form).Controls.Find("awbStatusUserControl1", true)[0]);
				awbControl.LinkLabelEntry_LinkClicked(null, null);
				var declarationForm = oFC.GetForm(hawb.Declaration.PK.ToGuid(), "JobDeclaration");
				AssertNotNull(declarationForm);
				declarationForm.Dispose();
			}
		}

		public void TestNoExplosionOnShipmentValidateAllIfNoHawbYet()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			// No hawb yet
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var fileMainMenu = shipmentForm.Menu.MenuItems.FindByText("File");
				var validateAllMenu = fileMainMenu.MenuItems.FindByText("Validate All");
				validateAllMenu.PerformClick();
				Assert("No explosion when there's not yet a HAWB", true);
			}
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertEquals("Hawb is not loaded or validated yet", 0, hawb.CS_GoodsDescriptionInfo.Notifications.Count());
				var fileMainMenu = shipmentForm.Menu.MenuItems.FindByText("File");
				var validateAllMenu = fileMainMenu.MenuItems.FindByText("Validate All");
				validateAllMenu.PerformClick();
				AssertEquals("Calling File > Validate All on the shipment form finds the hawb and validates it", 1, hawb.CS_GoodsDescriptionInfo.Notifications.Count());
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = shipment.JS_HouseBill;
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = shipment.JS_HouseBill;
			return new CcsukShipmentMultiHawbPlugin(shipment);
		}

		public void TestType()
		{
			MakeGoodShipmentOnGoodConsol();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertType(typeof(CcsukShipmentMultiHawbPlugin), plugin);
				}
			}
		}

		public void TestWhenTwoMawbsButVaryingNumbersofHawbs()
		{
			// e.g. airline basic and ERTS with house
			MakeGoodShipmentOnGoodConsol();
			var basicMawb1 = Factory.New<CusMAWB>();
			basicMawb1.CM_MAWB = consol.JK_MasterBillNum;
			basicMawb1.CargoTerminalOperator = "DJC";
			basicMawb1.CargoTerminalOperatorAirport = "LHR";
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			mawb2.CargoTerminalOperator = "LSM";
			mawb2.CargoTerminalOperatorAirport = "LHR";
			var hawb = mawb2.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertEquals("Has NO question about creating", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertType("Two mawbs, but only one with a hawb, see single plugin", typeof(CcsukHawbControl), ccsukPluginTab.UserControl);
				AssertType("Two mawbs, but only one with a hawb, see single plugin", typeof(CusHAWB), ccsukPluginTab.BusinessEntity);
				AssertEquals("Hawb has is wired to shipment by FK", shipment.PK, hawb.CS_JS);
				AssertEquals("Hawbs now linked, check by FK", 1, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK)).Length);
				AssertEquals("Hawbs now linked, check by NK", 1, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, shipment.JS_HouseBill)).Length);
			}
			var hawbAtAirline = basicMawb1.ChildBills.AddNew();
			hawbAtAirline.CS_HAWB = shipment.JS_HouseBill;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertType("Two mawbs, now two hawbs, see multi plugin", typeof(CcsukAirConsignmentUserControlHawbMany), ccsukPluginTab.UserControl);
				AssertType("Two mawbs, now two hawbs, see multi plugin", typeof(ShipmentToManyHawbsPluginHelper), ccsukPluginTab.BusinessEntity);
				AssertEquals("Hawbs now linked, check by FK", 2, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK)).Length);
				AssertEquals("Hawbs now linked, check by NK", 2, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, shipment.JS_HouseBill)).Length);
			}
		}

		[ExpectNoExceptions]
		public void TestNumberOfHawbsChanges()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			mawb1.CargoTerminalOperator = "DJC";
			mawb1.CargoTerminalOperatorAirport = "LHR";
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			mawb2.CargoTerminalOperator = "LSM";
			mawb2.CargoTerminalOperatorAirport = "LHR";
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = shipment.JS_HouseBill;
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				mawb1.ChildBills.Remove(hawb1.PK);
				ccsukPluginTab.SelectTabPage();
			}
		}

		public void TestNoParentMawbExists()
		{
			MakeGoodShipmentOnGoodConsol();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertNoCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNoMawbExistsInFactory();
				AssertNoHawbExistsInFactory();

				// Even if use says yes they can't create a hawb when no mawb exists. 
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ccsukPluginTab.SelectTabPage();
				AssertNoMawbExistsInFactory();
				AssertNoHawbExistsInFactory();
				AssertNoCcsukMenu(shipmentForm);
			}
		}

		public void TestMawbExistsButNoHawb()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;

			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertNoCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertNoHawbExistsInFactory();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
				Assert("Has warning about being a basic", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("No other HAWB records exist on MAWB 12512345678"));

				AssertNoHawbExistsInFactory();
				AssertNoCcsukMenu(shipmentForm);

				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Consol);
				var irrelevantShipment = consol.Shipments.AddNew();
				irrelevantShipment.JS_UniqueConsignRef = "S123";
				var irrelevantHawb = mawb.ChildBills.AddNew();
				irrelevantHawb.CS_HAWB = "IRRELEVE";
				irrelevantHawb.CS_JS = irrelevantShipment.PK;
				irrelevantHawb.SetCustomsActionCode("CA", ZDateTime.Now);
				irrelevantHawb.PresenceOnNetworkStatus = "POO";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				shipmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.Routing);
				ccsukPluginTab.SelectTabPage();
				Assert("Has question about creating", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
				Assert("Details the other hawbs already present", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Existing HAWB records on MAWB 12512345678:\r\n\tIRRELEVE: pres.: POO, status CA, S123"));
				AssertHasCcsukMenu(shipmentForm);
				var query = new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false);
				query.AddToFilter(CusHAWBSchema.CS_HAWB, SQLComparisonOperator.NotEqual, "IRRELEVE");
				var hawbCreated = Factory.LoadTop1<CusHAWB>(query);
				AssertNotNull("Hawb should have been created", hawbCreated);
				AssertEquals("Hawb is linked by FK to shipment to allow opening of shipment form from ccsuk module", shipment.PK, hawbCreated.CS_JS);
				AssertEquals("Hawb is linked by FK to mawb", mawb.PK, hawbCreated.CS_CM);
				AssertEquals("Hawb has NK to shipment", "DANIEL01", hawbCreated.CS_HAWB);
				AssertContains("Synchronised to CCSUK HAWB 125-12345678-DANIEL01", shipment.Logs.MostRecentLog.SL_Reference);
			}

			// Set shipment's header's department otherwise we cannot save. The header is created during shipmentForm.Show(). 
			shipment.Job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			// Save and reopen
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertHasCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				ccsukPluginTab.SelectTabPage();
				Assert("No question about creating", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one"));
			}
		}

		public void TestOneHawbExistsByNkNotFk()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertHasCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				ccsukPluginTab.SelectTabPage();
				Assert("No question about creating", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
			}
		}

		public void TestSeveralHawbsExistAtVariousSheds()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			mawb1.CargoTerminalOperator = "DJC";
			mawb1.CargoTerminalOperatorAirport = "LHR";
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = shipment.JS_HouseBill;
			hawb1.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			mawb2.CargoTerminalOperator = "LSM";
			mawb2.CargoTerminalOperatorAirport = "LHR";
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = shipment.JS_HouseBill;
			var mawb3AlsoLinkedByFK = Factory.New<CusMAWB>();
			mawb3AlsoLinkedByFK.CM_MAWB = consol.JK_MasterBillNum;
			mawb3AlsoLinkedByFK.CargoTerminalOperator = "LSM";
			mawb3AlsoLinkedByFK.CargoTerminalOperatorAirport = "MAN";
			mawb3AlsoLinkedByFK.CM_JK = consol.PK;
			var hawb3 = mawb3AlsoLinkedByFK.ChildBills.AddNew();
			hawb3.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var ccsukMenu = AssertHasCcsukMenu(shipmentForm);
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678-DANIEL01 @ LHRDJC CA"));
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678-DANIEL01 @ LHRLSM "));
				AssertNotNull(ccsukMenu.MenuItems.FindByText("125-12345678-DANIEL01 @ MANLSM "));

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertEquals("Has NO question about creating", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertEquals("Hawb has is wired to shipment by FK", shipment.PK, hawb1.CS_JS);
				AssertEquals("Hawb has is wired to shipment by FK", shipment.PK, hawb2.CS_JS);
				AssertEquals("Hawb has is not unwired from consol ", shipment.PK, hawb3.CS_JS);
				AssertEquals("No additional hawb created, check by FK", 3, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK)).Length);
				AssertEquals("No additional hawb created, check by NK", 3, Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, shipment.JS_HouseBill)).Length);
			}
		}

		public void TestSeveralHawbsExistOneForeign()
		{
			MakeGoodShipmentOnGoodConsol();
			var gbMawb1 = Factory.New<CusMAWB>();
			gbMawb1.CM_MAWB = consol.JK_MasterBillNum;
			gbMawb1.CargoTerminalOperator = "DJC";
			gbMawb1.CargoTerminalOperatorAirport = "LHR";
			var gbHawb1 = gbMawb1.ChildBills.AddNew();
			gbHawb1.CS_HAWB = shipment.JS_HouseBill;
			gbHawb1.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday);
			var gbMawb2 = Factory.New<CusMAWB>();
			gbMawb2.CM_MAWB = consol.JK_MasterBillNum;
			gbMawb2.CargoTerminalOperator = "LSM";
			gbMawb2.CargoTerminalOperatorAirport = "LHR";
			var gbHawb2 = gbMawb2.ChildBills.AddNew();
			gbHawb2.CS_HAWB = shipment.JS_HouseBill;
			Factory.Save();
			var auMawb = new BusinessObjectFactory().New<Customs.Business.CusMAWB>();
			auMawb.CM_MAWB = consol.JK_MasterBillNum;
			auMawb.CM_JK = consol.PK;
			auMawb.CM_ApplicationCode = "CMR";
			var auHawb = auMawb.ChildBills.AddNew();
			auHawb.CS_HAWB = shipment.JS_HouseBill;
			auMawb.Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				ccsukPluginTab.SelectTabPage();
				var memoryOnlyQuery = new ZQuery(CusHAWBSchema.CS_JS, shipment.PK);
				memoryOnlyQuery.FetchOnlyFromLocalCache = true;
				AssertEquals("The AU hawb was not loaded by the plugin", 2, Factory.Load<CusHAWB>(memoryOnlyQuery).Length);
			}
		}

		public void TestSeveralMawbsExistAtVariousShedsButDontCreateHawb()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = consol.JK_MasterBillNum;
			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = consol.JK_MasterBillNum;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertNoCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
				AssertNotNull(ccsukPluginTab);
				AssertEquals(true, ccsukPluginTab.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ccsukPluginTab.SelectTabPage();
				AssertEquals("Has NO question about creating", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Would you like to create a new one?"));
				AssertNoHawbExistsInFactory();  // Should not have created new hawbs when there are multiple mawbs
				AssertEquals(ccsukPluginTab.PlugInNotDisplayedMessage, CcsukShipmentMultiHawbPlugin.MultipleMawbsExistText);
			}
		}

		public void TestReloadFormForConcurrency()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				AssertHasCcsukMenu(shipmentForm);

				var ccsukPluginTab = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse) as CcsukShipmentMultiHawbPlugin;
				AssertNotNull(ccsukPluginTab);
				AssertNotNull(ccsukPluginTab.UserControl);

				hawb.Delete();
				Factory.Save();

				AssertEquals(true, ccsukPluginTab.Enabled);
				AssertEquals(0, ccsukPluginTab.HawbPluginHelper.ResetReloadAndCount());
				AssertNull(ccsukPluginTab.BusinessEntity);

				AssertEquals("Someone else has modified the CCS-UK job for this shipment - please reload the form.", ((IPlugInInternals)ccsukPluginTab).CoveringLabel.Text);
			}
		}

		void AssertNoMawbExistsInFactory()
		{
			AssertEquals("No mawb was created", 0, shipment.Factory.Load<CusMAWB>(new ZQuery()).Length);
		}

		void AssertNoHawbExistsInFactory()
		{
			AssertEquals("No hawb was created", 0, shipment.Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_IsMasterHouse, false)).Length);
		}

		void AssertNoCcsukMenu(ZForm form)
		{
			var menu = form.Menu;
			AssertNull(menu.MenuItems.FindByText(CcsukMenu.MenuCaption));
		}

		MenuItem AssertHasCcsukMenu(ZForm form)
		{
			var mainMenu = form.Menu;
			var ccsukMenu = mainMenu.MenuItems.FindByText(CcsukMenu.MenuCaption);
			AssertNotNull(ccsukMenu);
			return ccsukMenu;
		}

		protected void MakeGoodShipmentOnGoodConsol()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "12512345678";
			consol.JK_RL_NKDischargePort = "GBLHR";
			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "DANIEL01";
			shipment.JS_RL_NKDestination = "GBLHR";
		}

		protected ForwardingShipment shipment;
		protected ForwardingConsol consol;

		public void TestPluginToShipmentDoestShowEmptySplitsTab()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab not visible, not a shed", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab not visible, no splits exist", 0, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
				}
			}
			hawb.Splits.AddNew();
			hawb.Splits.AddNew();
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab not visible, not a shed", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
					AssertEquals("Splits tab visible, splits now exist", 1, plugin.UserControl.Controls.Find("SplitsTabPage", true).Length);
				}
			}
			hawb.Profile = "CUKAIR98LHRXXX";
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab not yet visible for a shed, not in DB", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
				}
			}
			hawb.Factory.Save();
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab visible for a shed", 1, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
				}
			}
			hawb.Profile = "CUKFFW98000XXX";
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					plugin.SelectTabPage();
					AssertEquals("Delivery tab not visible, not a shed", 0, plugin.UserControl.Controls.Find("DeliveryTabPage", true).Length);
				}
			}
		}

		public void TestMakeNewHawb()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugin.SelectTabPage();
					AssertEquals(1, plugin.HawbPluginHelper.Hawbs.Count);
					AssertEquals(shipment.PK, plugin.HawbPluginHelper.Hawbs[0].CS_JS);
				}
			}
		}

		public void TestGetNewTopLevelMenuWhenViewOnly()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = shipment.JS_HouseBill;
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var form = new ShipmentForm(shipment))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				form.Show();
				using (var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					var menu = (CcsukMenu)plugin.TopLevelMenu;
					menu.RefreshMenu();
					AssertEquals(1, menu.MenuItems.Count);
					form.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit;
					menu = (CcsukMenu)plugin.TopLevelMenu;
					menu.RefreshMenu();
					Assert(menu.MenuItems.Count > 1);
				}
			}
		}

		public void TestCusHawb_ExistsButNotYetLinkedButRightConsolByMawbNumber()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			consol.JK_MasterBillNum = "125-87654321";
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "12345678";
			shipment.JS_HouseBill = "12345678";
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertEquals(ZGuid.Empty, plugin.HawbPluginHelper.Hawbs[0].CS_JS);
					plugin.SelectTabPage();
					AssertEquals(shipment.PK, plugin.HawbPluginHelper.Hawbs[0].CS_JS);
				}
			}
		}

		public void TestCreateCcsUKJobIfRequired()
		{
			MakeGoodShipmentOnGoodConsol();
			shipment.JS_GoodsDescription = "STUFF";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				shipmentForm.Show();
				using (var ccsukPluginTab = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					ccsukPluginTab.SelectTabPage();
					AssertEquals("Consol has no CCSUK CusMAWB, so no CusHAWB created", 0, ccsukPluginTab.HawbPluginHelper.Hawbs.Count);
				}
			}

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				shipmentForm.Show();
				using (var ccsukPluginTab = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					ccsukPluginTab.SelectTabPage();
					AssertEquals("Consol has CCSUK CusMAWB, but user said NO, so no Hawb created", 0, ccsukPluginTab.HawbPluginHelper.Hawbs.Count);
				}
			}
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				shipmentForm.Show();
				using (var ccsukPluginTab = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					ccsukPluginTab.SelectTabPage();
					AssertEquals("Consol has CCSUK CusMAWB, but user said YES, so a Hawb is created", 1, ccsukPluginTab.HawbPluginHelper.Hawbs.Count);
				}
			}
		}

		public void TestNoSynchWhenShipmentIsDomestic()
		{
			MakeGoodShipmentOnGoodConsol();
			shipment.JS_RL_NKOrigin = "GBLHR";
			AssertEquals("Pre-req: shipment is domestic", true, shipment.IsDomestic());
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					Assert("No synching", !plugin.Enabled);
				}
			}
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("Pre-req: shipment is not domestic", false, shipment.IsDomestic());
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					Assert("Synched", plugin.Enabled);
				}
			}
		}

		public void TestNoSynchWhenConsolShipmentIsDirect()
		{
			MakeGoodShipmentOnGoodConsol();
			AssertEquals("Pre-req: consol is not direct", false, consol.IsDirect);
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					Assert("Synched", plugin.Enabled);
					AssertType(typeof(CcsukShipmentMultiHawbPlugin), plugin);
				}
			}
			consol.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					AssertType(typeof(CcsukShipmentMultiHawbPluginForDirectConsols), plugin);
				}
			}
		}

		public void TestNoKnowledgeOfForeignHawb()
		{
			MakeGoodShipmentOnGoodConsol();
			var gbMawb = Factory.New<CusMAWB>();
			gbMawb.CM_JK = consol.PK;
			gbMawb.CM_MAWB = "Ziggy";
			var auMawb = Factory.New<Customs.Business.CusMAWB>();
			auMawb.CM_JK = consol.PK;
			auMawb.CM_MAWB = "Ziggy";
			auMawb.CM_ApplicationCode = "CMR";
			var auHawb = auMawb.ChildBills.AddNew();
			auHawb.CS_JS = shipment.PK;
			auHawb.CS_HAWB = shipment.JS_HouseBill;
			auHawb.CS_ApplicationCode = "CMR";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				using (var ccsukPluginTab = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					ccsukPluginTab.SelectTabPage();
					AssertNotEquals(auHawb.PK, ccsukPluginTab.HawbPluginHelper.Hawbs[0].PK);
					AssertEquals(gbMawb.ChildBills[0].PK, ccsukPluginTab.HawbPluginHelper.Hawbs[0].PK);
				}
			}
		}

		public void TestSynchWithPadding()
		{
			MakeGoodShipmentOnGoodConsol();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = consol.JK_MasterBillNum;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "00DANIEL";
			shipment.JS_HouseBill = "DANIEL";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new ShipmentForm(shipment))
			{
				shipmentForm.Show();
				using (var plugin = (CcsukShipmentMultiHawbPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.CcsukAirInventoryHouse))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugin.SelectTabPage();
					AssertEquals(1, plugin.HawbPluginHelper.Hawbs.Count);
					AssertEquals(shipment.PK, plugin.HawbPluginHelper.Hawbs[0].CS_JS);
				}
			}
		}
	}
}
