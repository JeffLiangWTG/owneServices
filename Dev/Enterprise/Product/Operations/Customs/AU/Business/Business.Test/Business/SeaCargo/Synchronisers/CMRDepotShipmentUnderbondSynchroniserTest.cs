using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using SynchroniseAction = Enterprise.Customs.Business.SynchroniseAction;
using SynchroniseEventArgs = Enterprise.Customs.Business.SynchroniseEventArgs;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRDepotShipmentUnderbondSynchroniserTest : SeaCargoDepotTestCase
	{
		public void TestSynchroniseLCLShipmentDetails()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol(); //SG -> AU
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.LinkedObject = CFSShipmentWrapper.Load(shipment);

			CMRDepotShipmentUnderbondSynchroniser synchroniser = new CMRDepotShipmentUnderbondSynchroniser(underbond, shipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			shipment.JS_OuterPacks = 20;
			shipment.OuterPackLines[0].SetContainer(consol, container);
			ZDateTime arrivalTime = ZDateTime.Now;

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Empty;

			//Deliver Address is set via a Getter
			//ArrivalLeg.DeliverToPK = GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK;
			//AssertEquals("Deliver Address should be org proxy", GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK, ArrivalLeg.DeliverToDocAddress.E2_OA_Address);
			container.DestinationCFSArrival.EU_PickupDeliveryTime = arrivalTime;

			AssertEquals("Setting the arrival time on the shipment synch the arrival date", arrivalTime, underbond.C4_ArrivalDate);
			AssertEquals("Goods Reciept should have been created", 1, underbond.Outturns.Count);
			CusOutturn outturn = underbond.Outturns[0];
			AssertEquals("Outturn Receipt Only", true, outturn.C5_ReceiptOnlyIndicator);
			shipment.JS_OuterPacks = 20;
			AssertEquals("Underbond Package Count", 20, outturn.C5_PackagesOutturned);
		}

		public void TestSynchroniserBreakBulkShipment()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateBreakBulkConsol();
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK;

			CFSShipment breakBulkShipment = consol.Shipments.AddNew();
			breakBulkShipment.JS_OuterPacks = 1;
			breakBulkShipment.JS_F3_NKPackType = "PCE";

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.LinkedObject = CFSShipmentWrapper.Load(breakBulkShipment);

			CMRDepotShipmentUnderbondSynchroniser synchroniser = new CMRDepotShipmentUnderbondSynchroniser(underbond, breakBulkShipment);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			CommonPickupDeliveryConfirm arrivalLeg = breakBulkShipment.DestinationCFSArrivals.AddNew();
			ZDateTime arrivalDate = ZDateTime.Now;
			arrivalLeg.EU_PickupDeliveryTime = arrivalDate;
			AssertEquals("Arrival Date should synch with arrival leg", arrivalDate, underbond.C4_ArrivalDate);
			AssertEquals("Outturn line should be created as BBK was arrived", 1, underbond.Outturns.Count);
			AssertEquals("Outturn should be goods receipt", true, underbond.Outturns[0].C5_ReceiptOnlyIndicator);
			AssertEquals("Should be linked to shipment", breakBulkShipment.PK, underbond.Outturns[0].C5_ParentID);
		}
	}
}
