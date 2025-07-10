using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AirCTOCARSTBusinessObjectLoaderOrCreatorTestInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = true;
			AirCTOCARSTBusinessObjectLoaderOrCreator creator = new AirCTOCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO HAWBUnderbondApproval Interest", true, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
			AssertEquals("CTO OC Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910));
		}

		public void TestIsInterestedNotCTO()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = false;
			AirCTOCARSTBusinessObjectLoaderOrCreator creator = new AirCTOCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("CTO HAWBUnderbondApproval Interest", false, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
			AssertEquals("CTO OC Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceSubUBMovMessage_TRCU3382910));

			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			CusHAWB cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_HAWB = HAWBCargoStatusMessage.MAWB;
			Factory.Save();
			AssertEquals("Should not throw exception when not CTOCusMAWB", false, creator.IsInterestedInCARST(HAWBCargoStatusMessage));

			cusHAWB.CS_HAWB = ZString.Empty;
			CTOCusMAWB cToCusMAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB cTOCusHAWB = cToCusMAWB.ChildBills.AddNew();
			cTOCusHAWB.CS_HAWB = HAWBCargoStatusMessage.MAWB;
			Factory.Save();
			AssertEquals("Should still work with CTOCusMAWB", true, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
		}
	}
}
