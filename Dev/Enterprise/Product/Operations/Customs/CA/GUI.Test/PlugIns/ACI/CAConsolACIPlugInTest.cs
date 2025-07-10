using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAConsolACIPlugInTest : ZPlugInGenericTest
	{
		public void TestVisibilityChangesWithMode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			using (var plugIn = new CAConsolACIPlugIn(consol))
			{
				Assert(plugIn.Enabled);
				consol.JK_TransportMode = Constants.TransportModes.Road;
				Assert(!plugIn.Enabled);
				consol.JK_TransportMode = Constants.TransportModes.Air;
				Assert(plugIn.Enabled);
				consol.JK_TransportMode = Constants.TransportModes.Rail;
				Assert(!plugIn.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestVisibilityChangesWithMode_WhenConsolDeleted()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";

			consol.Delete();

			using (var plugIn = new CAConsolACIPlugIn(consol))
			{
				Assert(!plugIn.Enabled);
			}
		}

		public void TestVisibilityChangesWithDischargePort()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USCHI";

			using (var plugIn = new CAConsolACIPlugIn(consol))
			{
				Assert(!plugIn.Enabled);
				consol.JK_RL_NKDischargePort = "CATOR";
				Assert(plugIn.Enabled);
				consol.JK_RL_NKDischargePort = "USCHI";
				Assert(!plugIn.Enabled);
				consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
				Assert(plugIn.Enabled);
				consol.Transports[0].JW_RL_NKDiscPort = "USCHI";
				Assert(!plugIn.Enabled);
				consol.Transports.AddNew().JW_RL_NKDiscPort = "CATOR";
				Assert(plugIn.Enabled);
				consol.Transports.RemoveAndDelete(consol.Transports[1]);
				Assert(!plugIn.Enabled);
			}
		}

		public void TestVisibilityChangeWithFirstAndLastForeignPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var plugIn = new CAConsolACIPlugIn(consol))
			{
				consol.JK_RL_NKFirstForeignPort = "CATOR";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKFirstForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "CATOR";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "CATOR";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestChangingDiscchargeAfterPluginCreatedDoesNotCauseException()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consol.Transports[0].JW_RL_NKDiscPort = "USLAX";
				plugIn.OnGUIShown();
			}
		}

		public void TestCusSCAHouse_OnApplicationTypeChanged()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				using (var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn.OnGUIShown();
					var oceanBill = (CusSCAOceanBill)plugIn.BusinessEntity;
					var houseBill1 = oceanBill.HouseBills[0];
					var houseBill2 = oceanBill.HouseBills[1];

					AssertEquals("houseBill1.CA_FROBTransitImportCode should be defaulted", InTransitCodeList.Codes.FROB, houseBill1.CA_FROBTransitImportCode);
					AssertEquals("houseBill2.CA_FROBTransitImportCode should be defaulted", InTransitCodeList.Codes.FROB, houseBill2.CA_FROBTransitImportCode);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					consol.Shipments.AddNew();
					plugIn.OnGUIShown();
					AssertEquals("House bill added", 3, oceanBill.HouseBills.Count);
					var houseBill3 = oceanBill.HouseBills[2];
					AssertEquals("houseBill3.CA_FROBTransitImportCode should be defaulted from provious", InTransitCodeList.Codes.FROB, houseBill3.CA_FROBTransitImportCode);
				}
			}
		}

		public void TestSCAObjectsCreatedWhenGUIShown()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				Assert("Precondition", Factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK)) == null);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnGUIShown();
				var oceanBills = Factory.Load<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK));
				AssertEquals("1 ocean bill created", 1, oceanBills.Length);
				var houseBills = Factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBills[0].PK));
				AssertEquals("with 2 attached house bills", 2, houseBills.Length);
			}
		}

		public void TestSCAObjectsCreatedWhenGUIShown_ShipmentIsAssemblyMaster()
		{
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				Assert("Precondition", Factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK)) == null);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnGUIShown();
				var oceanBills = Factory.Load<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK));
				AssertEquals("1 ocean bill created", 1, oceanBills.Length);
				var houseBills = Factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBills[0].PK));
				AssertEquals("Assembly Master cannot be synchronized.", 1, houseBills.Length);
			}
		}

		public void TestSCAObjectsCreatedWhenGUIShown_ShipmentIsAssemblyMasterWhenSynchronizeAssemblyMasterwithLeadShipmentOn()
		{
			CACustomsDataRegistry.Instance.SynchronizeAssemblyMasterwithLeadShipment.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_HouseBill = "123";

			var shipment2 = shipment1.CoLoadShipments.AddNew();
			shipment2.JS_HouseBill = "456";
			var shipment3 = shipment1.CoLoadShipments.AddNew();
			shipment3.JS_HouseBill = "789";

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
				AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
				Assert("Precondition", Factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK)) == null);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.OnGUIShown();

				var oceanBills = Factory.Load<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK));
				AssertEquals("1 ocean bill created", 1, oceanBills.Length);
				var houseBills = Factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBills[0].PK));
				AssertEquals("Assembly Master should be synchronized.", 1, houseBills.Length);
				var bill = houseBills[0];
				AssertEquals("123", bill.CA_HouseBill);
			}
		}

		public void TestPlugInNotDisplayedMessage()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			using (var plugIn = new CAConsolACIPlugIn(consol))
			{
				AssertEquals(plugIn.CoveringLabelText, plugIn.PlugInNotDisplayedMessage);
			}
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
				var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
				Assert("ACI Menu on Consol Form shoudl be invisible", !plugIn.TopLevelMenu.Visible);
				Assert("ACI Menu on Consol Form should be enabled", plugIn.TopLevelMenu.Enabled);

				consol.JK_RL_NKDischargePort = "CATOR";
				Assert("ACI Menu on Consol Form shoudl be visible", plugIn.TopLevelMenu.Visible);
				Assert("ACI Menu on Consol Form should be enabled", plugIn.TopLevelMenu.Enabled);
			}
		}

		public void TestShipmentPlugInsCreatedMutexWillOnlyBeDisposedUntilConsolDispose()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment0 = consol.Shipments.AddNew();
			var shipment1 = consol.Shipments.AddNew();
			var consolMutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, consol.PK + ShipmentCargoReportPlugIn.ConsolMutexString);
			var shipment0Mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment0.PK + ShipmentCargoReportPlugIn.HouseMutexString);
			var shipment1Mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipment1.PK + ShipmentCargoReportPlugIn.HouseMutexString);
			AssertEquals("Shipment0 is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("Shipment1 is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				using (var plugIn = (CAConsolACIPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI))
				{
					AssertNotNull("Precondition: PlugIn on Consol Form", plugIn);
					Assert("Precondition", Factory.LoadTop1<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK)) == null);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					plugIn.OnGUIShown();
					var oceanBills = Factory.Load<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK));
					AssertEquals("1 ocean bill created", 1, oceanBills.Length);
					var houseBills = Factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_CB, oceanBills[0].PK));
					AssertEquals("with 2 attached house bills", 2, houseBills.Length);
					AssertEquals("Shipment0 is locked", true, shipment0Mutex.IsLocked);
					AssertEquals("Shipment1 is locked", true, shipment1Mutex.IsLocked);
					AssertEquals("consolMutex is locked", true, consolMutex.IsLocked);
				}
			}
			AssertEquals("Shipment0 is unlocked", false, shipment0Mutex.IsLocked);
			AssertEquals("Shipment1 is unlocked", false, shipment1Mutex.IsLocked);
			AssertEquals("consolMutex is unlocked", false, consolMutex.IsLocked);
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_RL_NKDiscPort = "CATOR";
			var plugIn = new CAConsolACIPlugIn(consol);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			plugIn.InitialiseBusinessObject();
			return plugIn;
		}

		protected override void SetUp()
		{
			base.SetUp();
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
		}

		protected override void TearDown()
		{
			ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			base.TearDown();
		}

		sealed class ConsolForm : ZTemplateForm
		{
			public ConsolForm(ForwardingConsol consol)
				: base(consol)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.CAConsolACI);
			}
		}
	}
}
