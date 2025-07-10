using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotLoadList))]
	sealed class SeaCargoDepotLoadListTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		public void TestProxyLoadListValues()
		{
			Transport transport = loadList.Transports[0];
			transport.JW_JX = CreateNewImportSailing();

			SeaCargoDepotLoadList depotLoadList = SeaCargoDepotLoadList.Load(loadList);
			AssertEquals("Ocean Bill", loadList.JK_MasterBillNum, depotLoadList.OceanBill);
			AssertEquals("Vessel", loadList.Voyage.JV_RV_NKVessel, depotLoadList.Vessel);
			AssertEquals("Voyage", loadList.Voyage.JV_VoyageFlight, depotLoadList.Voyage);
			AssertEquals("Lloyds", loadList.Voyage.Vessel.RV_LloydsNumber, depotLoadList.Lloyds);
		}

		#region Implementation

		CFSLoadListConsol loadList;

		protected override void SetUp()
		{
			base.SetUp();
			loadList = Factory.New<CFSLoadListConsol>();
			//			Shipment = CommonShipment.New(Factory);
			//			Shipment.JS_TotalPackageCount = 20;
			//			ShipmentsContainer = Factory.New<Container>();
			//			ShipmentsContainer.JC_ContainerNum = ContainerNum1;
			//			ShipmentsContainer.JC_OH_CFSClient = SomeForwarder();
			//			Shipment.OuterPackLines.AddNew();
			//			Shipment.OuterPackLines[0].SetContainer(ShipmentsContainer.PK);

			OrgHeader client = SomeForwarder("C93830292");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SeaCargoDepotLoadList result = SeaCargoDepotLoadList.Load(loadList);
			return result;
		}

		ZGuid CreateNewImportSailing()
		{
			ZGuid result = ZGuid.Empty;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage.JV_VoyageFlight = "4";
			voyage.Origins.AddNew();
			voyage.Destinations.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "USLAX";
			voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(2);
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			result = voyage.Sailings[0].PK;
			return result;
		}

		#endregion
	}
}
