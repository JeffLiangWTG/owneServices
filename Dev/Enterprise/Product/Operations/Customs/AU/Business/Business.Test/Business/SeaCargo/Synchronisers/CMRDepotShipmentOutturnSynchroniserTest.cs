using System;
using Enterprise.Freight.CFS.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRDepotShipmentOutturnSynchroniserTest : SeaCargoDepotTestCase
	{
		public void TestSynchroniseLCLShipmentDetails()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			outturn.C5_ContainerNumber = ContainerNumber1;
			outturn.C5_ParentID = shipment.PK;
			outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			outturn.C5_OuterPacks = 9;

			CMRDepotShipmentOutturnSynchroniser synchroniser = new CMRDepotShipmentOutturnSynchroniser(outturn, shipment);
			shipment.JS_OuterPacks = 1;
			shipment.OuterPackLines[0].SetContainer(consol, container);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Start));
			shipment.JS_OuterPacks = 10;
			AssertEquals("Outturn Package Count", 9, outturn.C5_OuterPacks);
			shipment.OuterPackLines[0].JL_Outturn = 8;
			AssertEquals("Outturn Packages Outturned Count", 8, outturn.C5_PackagesOutturned);
		}

		public void TestSynchroniseLCLShipmentGoodsReciept()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;
			shipment.OuterPackLines[0].SetContainer(consol, container);

			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			outturn.C5_OuterPacks = 1;
			outturn.C5_PackagesOutturned = 1;
			outturn.C5_ReceiptOnlyIndicator = true;

			CMRDepotShipmentOutturnSynchroniser synchroniser = new CMRDepotShipmentOutturnSynchroniser(outturn, shipment);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Start));
			shipment.JS_OuterPacks = 10;
			AssertEquals("Reciept Only Indicator means no synchronisation occures - Package Count", 10, outturn.C5_PackagesOutturned);
			shipment.OuterPackLines[0].JL_Outturn = 8;
			AssertEquals("Reciept Only Indicator means no synchronisation occures - Outturn Packages Outturned Count", 10, outturn.C5_PackagesOutturned);
		}

		public void TestSynchroniserBreakBulkShipment()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateBreakBulkConsol();
			CFSShipment breakBulkShipment = consol.Shipments.AddNew();
			breakBulkShipment.JS_OuterPacks = 1;
			breakBulkShipment.JS_F3_NKPackType = "PCE";
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			outturn.C5_ParentID = breakBulkShipment.PK;
			outturn.C5_ReceiptOnlyIndicator = true;

			CMRDepotShipmentOutturnSynchroniser synchroniser = new CMRDepotShipmentOutturnSynchroniser(outturn, breakBulkShipment);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Start));

			breakBulkShipment.JS_OuterPacks = 4;
			AssertEquals("Break Bulk package count should be Synch'd for Goods Receipt", 4, outturn.C5_PackagesOutturned);
		}
	}
}
