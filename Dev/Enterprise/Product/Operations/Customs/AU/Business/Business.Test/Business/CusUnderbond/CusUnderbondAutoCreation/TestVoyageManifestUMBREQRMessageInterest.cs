using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TestVoyageManifestUMBREQRMessageInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9912J";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = true;
			VoyageManifestExpectedArrivalCusUnderbondFactory creator = new VoyageManifestExpectedArrivalCusUnderbondFactory();
			AssertEquals("CTO HAWB Interest", false, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("CTO SEA Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
			AssertEquals("CTO SEA Interest", false, creator.IsInterestedInUBMREQRInternal(IFTU723492ExpectedCargoAdvice));
			// Put in test for approval
		}

		public void TestIsInterestedNotCTO()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsSeaCTO = false;
			VoyageManifestExpectedArrivalCusUnderbondFactory creator = new VoyageManifestExpectedArrivalCusUnderbondFactory();
			AssertEquals("CTO HAWBUnderbondApproval Interest", false, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("CTO OC Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
		}
	}
}
