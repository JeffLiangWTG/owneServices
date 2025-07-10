using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PivotSynchroniserSeaCargoTest : SeaCargoTestCase
	{
		public void TestLiquidContainerMode()
		{
			CommonConsol consol = CreateBulkConsol();
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			CommonShipment shipment = consol.Shipments.AddNew();
			SeaCargoSynchroniser synch = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synch.OceanBill;
			CusSCAHouse house = synch.GetHouseBill(shipment);
			AssertEquals("Ocean Bill container mode should be Bulk", CMRImportCargoTypes.Codes.Bulk, oceanBill.Containers[0].CN_ContainerMode);
		}
	}
}
