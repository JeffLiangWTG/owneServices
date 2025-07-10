using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class TestVoyageManifestCARSTMessageInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = true;
			VoyageManifestCARSTBusinessObjectLoaderOrCreator creator = new VoyageManifestCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO HAWB Interest", false, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
			AssertEquals("CTO House Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_TRCU3382910));
			AssertEquals("CTO SEA no data", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910));
			CreateOceanBill(new BusinessObjectFactory());
			AssertEquals("CTO SEA with data", true, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910));
		}

		public void TestIsInterestedCargoLine()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = true;
			VoyageManifestCARSTBusinessObjectLoaderOrCreator creator = new VoyageManifestCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO SEA no data", false, creator.IsInterestedInCARST(OBL250805001_CargoLineStatusAdviceMessage_TRCU3382910));
			CreateCargoLine(new BusinessObjectFactory());
			AssertEquals("CTO SEA with data", true, creator.IsInterestedInCARST(OBL250805001_CargoLineStatusAdviceMessage_TRCU3382910));
		}

		public void TestIsInterestedNotCTO()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = false;
			VoyageManifestCARSTBusinessObjectLoaderOrCreator creator = new VoyageManifestCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO HAWB Interest", false, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
			AssertEquals("CTO SEA Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910));
		}

		public void TestCARSTWithHouseBillNotProcessed()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = true;
			VoyageManifestCARSTBusinessObjectLoaderOrCreator creator = new VoyageManifestCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO House Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_TRCU3382910));
		}

		protected void CreateOceanBill(BusinessObjectFactory factory)
		{
			var manifest = factory.New<CusSeaManTranHead>();
			manifest.BT_LloydsIMO = "8811924";
			manifest.BT_VoyageNum = "936";
			var oBHeader = manifest.OceanBills.AddNew();
			oBHeader.BO_OceanBill = "OBL250805001";
			factory.Save();
		}

		protected void CreateCargoLine(BusinessObjectFactory factory)
		{
			var manifest = factory.New<CusSeaManTranHead>();
			manifest.BT_LloydsIMO = "8811924";
			manifest.BT_VoyageNum = "936";
			var arrival1 = manifest.Arrivals.AddNew();
			arrival1.BA_SendersMessageReference = "H00000018";
			var cargoLine1 = arrival1.CargoLines.AddNew();
			cargoLine1.Detail.BD_ContainerNumber = "TRCU3382910";
			factory.Save();
		}
	}
}
