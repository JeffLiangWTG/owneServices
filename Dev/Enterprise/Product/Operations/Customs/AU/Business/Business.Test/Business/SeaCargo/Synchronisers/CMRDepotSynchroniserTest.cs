using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRDepotSynchroniserTest : SeaCargoDepotTestCase
	{
		public void TestStartSynchronisingDoesNotCreateNewRecords()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;
			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);
			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Start);
			CFSLoadListConsolWrapper consolWrapper = CFSLoadListConsolWrapper.Load(consol);
			CFSContainerWrapper containerWrapper = CFSContainerWrapper.Load(container);
			AssertEquals("CusUnderbond should not have been created until forced", 0, containerWrapper.Underbonds.Count);
		}

		public void TestCreateGoodsRecieptForLCLContainer()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = ContainerNumber1;
			CFSContainerWrapper containerWrapper = CFSContainerWrapper.Load(container);
			CusUnderbond underbond = containerWrapper.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;

			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;

			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Force);
			Assert("CusOutturn should have been created for Container", containerWrapper.Underbonds[0].Outturns.Count > 0);
		}

		public void TestCreateGoodsRecipetForBreakBulk()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateBreakBulkConsol();
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK;

			CFSShipment breakBulkShipment = consol.Shipments.AddNew();
			breakBulkShipment.JS_OuterPacks = 1;
			breakBulkShipment.JS_F3_NKPackType = "PCE";
			CFSShipmentWrapper shipmentWrapper = CFSShipmentWrapper.Load(breakBulkShipment);
			CusUnderbond underbond = shipmentWrapper.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;

			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);

			CommonPickupDeliveryConfirm arrivalLeg = breakBulkShipment.DestinationCFSArrivals.AddNew(); //Destination - Loose to CFS
			arrivalLeg.EU_PickupDeliveryTime = ZDateTime.Now;

			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Force);
			Assert("CusUnderbod should have been created for break bulk shipment", shipmentWrapper.Underbonds.Count > 0);
		}

		public void TestDefaultingInformation()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = TestPremiseCode;

			CFSLoadListConsol consol = (CFSLoadListConsol)CreateFCLConsol();
			consol.JK_OA_UnpackDepotAddress = GlbBranch.CurrentBranch.OrgProxy.GetAddressWithFallback(AddressType.DLV).PK;

			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 20;
			shipment.JS_F3_NKPackType = "BOX";
			shipment.OuterPackLines[0].SetContainer(consol, container);
			CFSShipmentWrapper shipmentWrapper = CFSShipmentWrapper.Load(shipment);
			CusUnderbond shipmentUnderbond = shipmentWrapper.Underbonds.AddNew();
			shipmentUnderbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;
			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;

			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Force);

			Assert("CusOutturn should have been created for break bulk shipment", shipmentWrapper.Underbonds[0].Outturns.Count > 0);
			CusOutturn outturn = shipmentWrapper.Underbonds[0].Outturns[0];
			AssertEquals("Goods Receipt Indicator", true, outturn.C5_ReceiptOnlyIndicator);
		}

		public void TestDefaultOutturnLinesForMovementUnderbondRequest()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = TestPremiseCode;
			CFSLoadListConsol consol = CreateLCLTestConsolidation();
			CFSContainer container = consol.Containers.AddNew();

			CFSContainerWrapper containerWrapper = CFSContainerWrapper.Load(container);
			CusUnderbond underbond = containerWrapper.Underbonds.AddNew();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.OtherMovement;

			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);
			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;
			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Force);

			AssertEquals("Outturn Lines - Movement", 1, containerWrapper.Underbonds[0].Outturns.Count);
		}

		public void TestDefultOutturnLinesForDeconsolidationUnderbondRequest()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = TestPremiseCode;
			CFSLoadListConsol consol = CreateLCLTestConsolidation();
			CFSContainer container = consol.Containers[0];

			CFSContainerWrapper containerWrapper = CFSContainerWrapper.Load(container);
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.LinkedObject = containerWrapper;

			CMRDepotSynchroniser synchroniser = new CMRDepotSynchroniser(consol);

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;

			CFSShipment shipment1 = consol.Shipments[0];
			CFSShipment shipment2 = consol.Shipments[1];
			shipment2.OuterPackLines[0].JL_Outturn = 10;

			synchroniser.Synchronise(Enterprise.Customs.Business.SynchroniseAction.Force);

			AssertEquals("Outturn Lines - Deconsolidation", 3, containerWrapper.Underbonds[0].Outturns.Count);

			CusOutturn outturn1 = GetOutturnLineForParent(underbond, shipment1);
			CusOutturn outturn2 = GetOutturnLineForParent(underbond, shipment2);
			CusOutturn outturn3 = GetOutturnLineForParent(underbond, container);
			AssertNotNull("Outturn for Shipment 1", outturn1);
			AssertNotNull("Outturn for Shipment 2", outturn2);
			AssertNotNull("Outturn for Container", outturn3);
			AssertEquals("Container line is a receipt", true, outturn3.C5_ReceiptOnlyIndicator);
			AssertEquals("Shipment Lines are not receipts", false, outturn1.C5_ReceiptOnlyIndicator || outturn2.C5_ReceiptOnlyIndicator);
			AssertEquals("Shipment 1 Outturn Count", 0, outturn1.C5_PackagesOutturned);
			AssertEquals("Shipment 2 Outturn Count", 10, outturn2.C5_PackagesOutturned);
		}

		#region Implementation

		CusOutturn GetOutturnLineForParent(CusUnderbond underbond, BusinessObject parent)
		{
			CusOutturn result = null;
			foreach (CusOutturn outturn in underbond.Outturns)
			{
				if (outturn.C5_ParentID == parent.PK)
				{
					result = outturn;
					break;
				}
			}
			return result;
		}

		CFSLoadListConsol CreateLCLTestConsolidation()
		{
			CFSLoadListConsol result = (CFSLoadListConsol)CreateFCLConsol();
			CFSContainer container = result.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;
			CFSShipment shipment = result.Shipments.AddNew();
			CFSShipment shipment2 = result.Shipments.AddNew();
			shipment.JS_HouseBill = "TEST1";
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "BOX";
			shipment2.JS_HouseBill = "TEST2";
			shipment2.JS_OuterPacks = 20;
			shipment2.JS_F3_NKPackType = "PKG";
			shipment.OuterPackLines[0].SetContainer(result, container);
			shipment2.OuterPackLines[0].SetContainer(result, container);

			return result;
		}

		#endregion
	}
}
