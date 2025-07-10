using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHConsolPlugInTest : ZPlugInGenericTest
	{
		public void TestCreateHouseBillsIfRequired()
		{
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			var house1 = master.HouseBills.AddNew();
			house1.BW_ParentID = shipment1.PK;

			AssertEquals(1, master.HouseBills.Count);

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals(2, master.HouseBills.Count);
				var house2 = master.HouseBills.FirstOrDefault(x => x.BW_ParentID == shipment2.PK);
				AssertNotNull(house2);
			}
		}

		public void TestCreateHouseBillsIfRequired_ShipmentIsAssemblyMaster()
		{
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USPHL";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USCHI";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USCHI";
			transport2.JW_RL_NKDiscPort = "CATOR";

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("Assembly Master cannot be synchronized.", 1, master.HouseBills.Count);
				var bill = master.HouseBills[0];
				AssertEquals("Movement type should be defaulted to 24", eMHMovementTypeList.Codes.Import, bill.BW_MovementType);
			}

			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			transport1.JW_RL_NKDiscPort = "CAVAN";
			transport2.JW_RL_NKLoadPort = "CAVAN";
			transport2.JW_RL_NKDiscPort = "USCHI";

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("Assembly Master cannot be synchronized.", 2, master.HouseBills.Count);
				AssertEquals("Movement type should be defaulted to 23", true, master.HouseBills.Any(x => x.BW_MovementType == eMHMovementTypeList.Codes.InTransit));
			}
		}

		public void TestCreateHouseBillsIfRequired_ShipmentIsASMWhenSynchronizeAssemblyMasterwithLeadShipmentOn()
		{
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_HouseBill = "123";

			var shipment2 = shipment1.CoLoadShipments.AddNew();
			shipment2.JS_HouseBill = "456";
			var shipment3 = shipment1.CoLoadShipments.AddNew();
			shipment3.JS_HouseBill = "789";
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("Assembly Master should be synchronized.", 1, master.HouseBills.Count);
				var bill = master.HouseBills[0];
				AssertEquals("123", bill.BW_HouseBill);
				AssertEquals("Consolidated should not be changed", false, bill.BW_IsMasterHouse);
				AssertEquals("Movement type should be defaulted to 24", eMHMovementTypeList.Codes.Import, bill.BW_MovementType);
			}

			consol.JK_RL_NKDischargePort = "USCHI";

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("Assembly Master should be synchronized.", 1, master.HouseBills.Count);
				var bill = master.HouseBills[0];
				AssertEquals("Movement type should be defaulted to 23", eMHMovementTypeList.Codes.InTransit, bill.BW_MovementType);
			}
		}

		public void TestCreateHouseBills_MovementType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			var house1 = master.HouseBills.AddNew();
			house1.BW_ParentID = shipment1.PK;

			AssertEquals(1, master.HouseBills.Count);

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals(2, master.HouseBills.Count);
				var house2 = master.HouseBills.FirstOrDefault(x => x.BW_ParentID == shipment2.PK);
				AssertNotNull(house2);
				AssertEquals("Movement type should be defaulted to 24", eMHMovementTypeList.Codes.Import, house2.BW_MovementType);
				house2.BW_MovementType = ZString.Empty;
			}

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = "USLAX";
			var shipment12 = consol2.Shipments.AddNew();
			var shipment22 = consol2.Shipments.AddNew();
			shipment22.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var master2 = Factory.New<CusCAeMHMaster>();
			master2.BP_ParentID = consol2.PK;
			var house12 = master2.HouseBills.AddNew();
			house12.BW_ParentID = shipment12.PK;
			var transport12 = consol2.Transports.AddNew();
			transport12.JW_RL_NKDiscPort = "CATOR";
			var transport22 = consol2.Transports.AddNew();
			transport22.JW_RL_NKLoadPort = "CATOR";
			transport22.JW_RL_NKDiscPort = "USLAX";
			consol2.JK_RL_NKDischargePort = "USLAX";

			using (var form = new ZForm(consol2))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals(2, master2.HouseBills.Count);
				var house22 = master2.HouseBills.FirstOrDefault(x => x.BW_ParentID == shipment22.PK);
				AssertNotNull(house22);
				AssertEquals("Movement type should be defaulted to 23", eMHMovementTypeList.Codes.InTransit, house22.BW_MovementType);
			}
		}

		public void TestCreateHouseBillsIfRequired_ShipmentIsHVLShipment()
		{
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			((BusinessObject)consignment1).FillWithValidTestData();
			((BusinessObject)consignment2).FillWithValidTestData();

			consignment1.HVC_JS_ManifestedOnShipment = shipment2.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("1 housebill for standard shipment and 2 housebills for HVL shipment is created", 3, master.HouseBills.Count);
			}
		}

		public void TestCreateHouseBillsIfRequired_AddMappingsForConvertedHVLVShipments()
		{
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.New<IHVLVConsignment>();
			((BusinessObject)consignment).FillWithValidTestData();

			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			var masterTRFLogs = master.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferredCode);
			var shipmentTRFLogs = shipment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferredCode);

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("There are no TRF logs on master.", 0, masterTRFLogs.Count());
				AssertEquals("There are no TRF logs on shipment.", 0, shipmentTRFLogs.Count());
				AssertEquals("No housebills for HVL shipment have been created.", 0, master.HouseBills.Count);
			});

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				Factory.Save();

				masterTRFLogs = master.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferredCode);
				shipmentTRFLogs = shipment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.TransferredCode);

				CombineAssertions("House bills are created, logs are made and shipment is converted", () =>
				{
					AssertEquals("There is 1 TRF log on master.", 1, masterTRFLogs.Count());
					AssertEquals("There is 1 TRF log on the shipment.", 1, shipmentTRFLogs.Count());
					AssertEquals("1 housebill for HVL shipment is created.", 1, master.HouseBills.Count);
				});

				plugin.OnGUIShown();
				AssertEquals("No new house bills are created as shipment is already mapped.", 1, master.HouseBills.Count);
			}
		}

		public void TestCreateHouseBillsIfRequired_NoDuplicateCreationBeforeSaving()
		{
			CACustomsDataRegistry.Instance.SynchronizeBlindColoadMasterWithConsol.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";

			var stdShipment = consol.Shipments.AddNew();
			var hvlvShipment = consol.Shipments.AddNew();
			hvlvShipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			((BusinessObject)consignment1).FillWithValidTestData();
			((BusinessObject)consignment2).FillWithValidTestData();

			consignment1.HVC_JS_ManifestedOnShipment = hvlvShipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = hvlvShipment.PK;
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;
			master.BP_ParentTableCode = consol.TablePrefix;

			Factory.Save();

			using (var form = new ZForm(consol))
			{
				form.PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
				form.Show();
				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				plugin.OnGUIShown();

				AssertEquals("1 housebill for standard shipment and 2 housebills for HVL shipment is created", 3, master.HouseBills.Count);

				plugin.OnGUIShown();

				AssertEquals("No extra house bill created when OnGUIShown called multiple times", 3, master.HouseBills.Count);
			}
		}

		public void TestInitialiseBusinessObject()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (var plugin = new CusCAeMHConsolPlugIn(consol))
			{
				AssertNull(plugin.MasterBill);
				plugin.InitialiseBusinessObject();
				AssertNotNull(plugin.MasterBill);
				Assert(!plugin.MasterBill.IsInDatabase);
			}

			Factory.Save();

			using (var plugin = new CusCAeMHConsolPlugIn(consol))
			{
				AssertNull(plugin.MasterBill);
				plugin.InitialiseBusinessObject();
				AssertNotNull(plugin.MasterBill);
				Assert(plugin.MasterBill.IsInDatabase);
			}
		}

		public void TestVisibilityChange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var plugin = new CusCAeMHConsolPlugIn(consol))
			{
				consol.JK_RL_NKDischargePort = "AUSYD";
				Assert(!plugin.Enabled);
				consol.JK_RL_NKDischargePort = "CATOR";
				Assert(plugin.Enabled);

				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_RL_NKFirstForeignPort = "CATOR";
				Assert(plugin.Enabled);

				consol.JK_RL_NKFirstForeignPort = string.Empty;
				consol.JK_RL_NKLastForeignPort = "CATOR";
				Assert(plugin.Enabled);

				consol.JK_RL_NKLastForeignPort = string.Empty;
				consol.JK_RL_NKPortOfFirstArrival = "CATOR";
				Assert(plugin.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestVisibilityChange_WhenConsolIsDeleted()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "CATOR";

			consol.Delete();

			using (var plugin = new CusCAeMHConsolPlugIn(consol))
			{
				Assert(!plugin.Enabled);
			}
		}

		public void TestVisibilityChangesWhenPortsChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USCHI";

			using (var plugIn = new CusCAeMHConsolPlugIn(consol))
			{
				AssertVisibilityChangesWithSpecificPort(consol, JobConsolSchema.JK_RL_NKDischargePort, plugIn);
				AssertVisibilityChangesWithSpecificPort(consol, JobConsolSchema.JK_RL_NKFirstForeignPort, plugIn);
				AssertVisibilityChangesWithSpecificPort(consol, JobConsolSchema.JK_RL_NKLastForeignPort, plugIn);
				AssertVisibilityChangesWithSpecificPort(consol, JobConsolSchema.JK_RL_NKPortOfFirstArrival, plugIn);
				consol.Transports.AddNew().JW_RL_NKDiscPort = "CATOR";
				Assert(plugIn.Enabled);
				consol.Transports.RemoveAndDelete(consol.Transports[1]);
				Assert(!plugIn.Enabled);
			}
		}

		public void AssertVisibilityChangesWithSpecificPort(ForwardingConsol consol, SchemaColumn portColumn, CusCAeMHConsolPlugIn plugIn)
		{
			consol[portColumn] = "CATOR";
			Assert(plugIn.Enabled);
			consol[portColumn] = "USCHI";
			Assert(!plugIn.Enabled);
		}

		public void TestTopLevelMenuEnabled()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USCHI";

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (CusCAeMHConsolPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsoleManifest);
				Assert("eManifest Fwdr Menu on Consol Form should be invisible", !plugIn.TopLevelMenu.Visible);
				Assert("eManifest Fwdr Menu on Consol Form should be enabled", plugIn.TopLevelMenu.Enabled);

				consol.JK_RL_NKDischargePort = "CATOR";
				Assert("eManifest Fwdr Menu on Consol Form should be visible", plugIn.TopLevelMenu.Visible);
				Assert("eManifest Fwdr Menu on Consol Form should be enabled", plugIn.TopLevelMenu.Enabled);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			var result = new CusCAeMHConsolPlugIn(consol);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			result.InitialiseBusinessObject();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
		}

		sealed class ConsolForm : ZTemplateForm
		{
			public ConsolForm(ForwardingConsol consol)
				: base(consol)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.CAConsoleManifest);
			}
		}
	}
}
