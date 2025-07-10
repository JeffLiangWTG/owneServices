using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class TestStandAloneAirCargoCARSTMessageInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9938N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = true;
			StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator creator = new StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("Depot SEA Job", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_FRCU3948922));
			CreateCTOMAWBRecord("08112347775");
			AssertEquals("Depot HAWB Interest", true, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
		}

		public void TestIsInterestedNotDepot()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
			StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator creator = new StandAloneAirCargoCARSTBusinessObjectLoaderOrCreator();
			AssertEquals("Depot HAWB Interest", true, creator.IsInterestedInCARST(HAWBCargoStatusMessage));
			AssertEquals("Depot SEA Interest", false, creator.IsInterestedInCARST(OBL250805001_CargoStatusAdviceMessage_FRCU3948922));
		}

		#region Implementation

		protected void CreateCTOMAWBRecord(ZString mAWBNumber)
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = mAWBNumber;
			Factory.Save();
		}

		#endregion
	}
}
