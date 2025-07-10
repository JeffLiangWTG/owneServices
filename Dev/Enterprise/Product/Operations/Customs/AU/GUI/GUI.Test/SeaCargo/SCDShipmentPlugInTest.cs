using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDShipmentPlugInTest : SCDPlugInTest
	{
		public void TestLoadZPlugIn()
		{
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.BusinessEntity);
			}
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestCanDelete()
		{
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				testPlugIn.OnUserControlShown();
				SeaCargoDepotShipment sCDShipment = (SeaCargoDepotShipment)testPlugIn.BusinessEntity;
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
				sCDShipment.Shipment.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
				AssertEquals("Can Delete should be false if Customs Messaging is active", false, testPlugIn.CanDelete);
				AssertNotNull(testPlugIn.BusinessEntity);
				sCDShipment.Shipment.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled);
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
			}
		}

		public void TestSetShipmentAsImpendingArrival()
		{
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				testPlugIn.OnUserControlShown();
				SeaCargoDepotShipment sCDShipment = (SeaCargoDepotShipment)testPlugIn.BusinessEntity;
				testPlugIn.ForceImpendingArrival();
				AssertEquals("Forcing a shipment ready to be delivered should add the delivered error event", DepotState.ImpendingCargo, sCDShipment.MessageState);
			}
		}

		public void TestSetShipmentReadyForDelivery()
		{
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				testPlugIn.OnUserControlShown();
				SeaCargoDepotShipment sCDShipment = (SeaCargoDepotShipment)testPlugIn.BusinessEntity;
				testPlugIn.ForceReadyForDelivery();
				AssertEquals("Forcing a shipment ready to be delivered should add the delivered error event", DepotState.CargoDeliveredError, sCDShipment.MessageState);
			}
		}

		public void TestForceLegacy()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			loadList.Containers.AddNew();
			CFSShipment shipment = loadList.Shipments.AddNew();
			EDIMessage newMessage = Factory.New<EDIMessage>();
			newMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.SeaCargo;
			newMessage.EM_LinkedObject = shipment;
			newMessage.EM_MessageType = SeaCargoMessageTypes.CargoStatusAdvice;
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceLegacy);
			}
		}

		public void TestForceCMR()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			loadList.Containers.AddNew();
			CFSShipment shipment = loadList.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			underbond.C4_ParentID = shipment.PK;
			using (SCDShipmentPlugIn testPlugIn = new SCDShipmentPlugIn(shipment))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceCMR);
			}
		}

		public void TestIsActive()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			using (SCDShipmentPlugInForTesting plugIn = new SCDShipmentPlugInForTesting(shipment))
			{
				AssertEquals("Should Show Plug In", false, plugIn.IsActiveForTesting());
				CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
				consol.Shipments.Add(shipment);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Should Show Plug In", true, plugIn.IsActiveForTesting());
			}
		}

		CFSShipment shipment;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<CFSShipment>();
		}

		protected override SCDPlugIn GetPlugIn() => new SCDShipmentPlugIn(shipment);

		sealed class SCDShipmentPlugInForTesting : SCDShipmentPlugIn
		{
			public SCDShipmentPlugInForTesting(CFSShipment shipment) : base(shipment)
			{
			}

			public bool IsActiveForTesting() => ShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
