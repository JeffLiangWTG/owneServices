using CargoWise.Types;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRDepotContainerUnderbondSynchroniserTest : CMRSeaCargoDepotTestCase
	{
		public void TestSynchroniseLCLContainerDetails()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();

			CusUnderbond underbond = ExpectedArrivalUnderbond(container);

			CMRDepotContainerUnderbondSynchroniser synchroniser = new CMRDepotContainerUnderbondSynchroniser(underbond, container);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Start));

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;
			AssertEquals("Arrival Time of container should be synchronised with Unpack Date on Underbond", container.JC_ArrivalTime, underbond.C4_DateOfArrivalIntoDestinationPremise);
		}

		public void TestSynchroniserFCLContainerDetails()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateFCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PCE";
			CusUnderbond underbond = Factory.New<CusUnderbond>();

			CMRDepotContainerUnderbondSynchroniser synchroniser = new CMRDepotContainerUnderbondSynchroniser(underbond, container);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Start));

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;
			AssertEquals("Arrival Time of container should be synchronised with Unpack Date on Underbond", container.JC_ArrivalTime, underbond.C4_DateOfArrivalIntoDestinationPremise);
		}
	}
}
