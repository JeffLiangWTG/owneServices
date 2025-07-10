using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDGatePassPlugInTest : SCDPlugInTest
	{
		public void TestLoadZPlugIn()
		{
			using (SCDGatePassPlugIn testPlugIn = new SCDGatePassPlugIn(gatePassShipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.BusinessEntity);
			}
		}

		public void TestSeaCargoDepotDeliveries()
		{
			gatePassShipment.JS_TotalPackageCount = 10;
			gatePassShipment.OuterPackLines.AddNew();
			gatePassShipment.OuterPackLines[0].JL_Outturn = 10;
			EDIMessage cargoStatusAdvice = Factory.New<EDIMessage>();
			cargoStatusAdvice.EM_ApplicationCode = "SCA";
			cargoStatusAdvice.EM_LinkUniqueID = gatePassShipment.PK;
			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(gatePassShipment))
			{
				testForm.Show();
				AssertEquals("Pre-Condition", false, gatePassShipment.DocsAndCartage.JP_IsContingencyRelease);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should get error about unclear cargo and no contingency release ticked", "This cargo has not been cleared and is not ticked as a contingency release", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should get error about unclear cargo and no contingency release ticked", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				gatePassShipment.DocsAndCartage.JP_IsContingencyRelease = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should get confirmation dialog to continue", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Should get confirmation dialog to continue", "This cargo has not been cleared by customs.  Are you sure you want to release it?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("New Delivery should have been added", 1, gatePassShipment.DestinationCFSDepartures.Count);
			}
		}

		public void TestIsActive()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			using (SCDGatePassPlugInForTesting testPlugIn = new SCDGatePassPlugInForTesting(gatePassShipment))
			{
				AssertEquals("Shipment with no arrival consol should not be active Sea Cargo Job", false, testPlugIn.IsActiveForTesting);
			}
		}

		public void TestForceLegacy()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol(typeof(GatePassLoadListConsol));
			CFSContainer container = loadList.Containers.AddNew();
			CFSShipment shipment = loadList.Shipments.AddNew();
			EDIMessage newMessage = Factory.New<EDIMessage>();
			newMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.SeaCargo;
			newMessage.EM_LinkedObject = shipment;
			newMessage.EM_MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
			using (SCDGatePassPlugIn testPlugIn = new SCDGatePassPlugIn(shipment))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceLegacy);
			}
		}

		public void TestForceCMR()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol(typeof(GatePassLoadListConsol));
			CFSContainer container = loadList.Containers.AddNew();
			CFSShipment shipment = loadList.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			underbond.C4_ParentID = shipment.PK;
			using (SCDGatePassPlugIn testPlugIn = new SCDGatePassPlugIn(shipment))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceCMR);
			}
		}

		GatePassShipment gatePassShipment;
		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			base.SetUp();
			gatePassShipment = Factory.New<GatePassShipment>();
		}

		protected override SCDPlugIn GetPlugIn() => new SCDGatePassPlugIn(gatePassShipment);

		sealed class SCDGatePassPlugInForTesting : SCDGatePassPlugIn
		{
			public SCDGatePassPlugInForTesting(IBusiness hostEntity)
				: base(hostEntity)
			{
			}

			public bool IsActiveForTesting => ShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
