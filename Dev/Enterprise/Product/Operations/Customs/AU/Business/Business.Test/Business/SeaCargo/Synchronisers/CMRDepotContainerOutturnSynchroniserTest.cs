using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRDepotContainerOutturnSynchroniserTest : SeaCargoDepotTestCase
	{
		public void TestSynchroniseLCLContainerDetails()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateLCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			underbond.LinkedObject = CFSContainerWrapper.Load(container);
			outturn.C5_ParentID = container.PK;
			outturn.C5_ParentTableCode = JobContainerSchema.Constants.Prefix;
			outturn.C5_OuterPacks = 1;
			outturn.C5_ReceiptOnlyIndicator = true;

			CMRDepotContainerOutturnSynchroniser synchroniser = new CMRDepotContainerOutturnSynchroniser(outturn, container);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Start));
			AssertEquals("Container Packages Outturned Count", 0, outturn.C5_PackagesOutturned);

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;
			AssertEquals("Container Packages Outturned Count", 1, outturn.C5_PackagesOutturned);
			AssertEquals("Container Package Count", 1, outturn.C5_OuterPacks);
		}

		public void TestSynchroniserFCLContainerDetails()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)CreateFCLConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber1;

			CFSShipment shipment = consol.Shipments.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			underbond.LinkedObject = CFSContainerWrapper.Load(container);
			outturn.C5_ParentID = container.PK;
			outturn.C5_ParentTableCode = JobContainerSchema.Constants.Prefix;

			CMRDepotContainerOutturnSynchroniser synchroniser = new CMRDepotContainerOutturnSynchroniser(outturn, container);
			synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Enterprise.Customs.Business.SynchroniseAction.Start));

			container.DestinationCFSArrival.EU_PickupDeliveryTime = ZDateTime.Now;
			AssertEquals("Underbond Package Count - Move Only", 1, outturn.C5_PackagesOutturned);
		}
	}
}
