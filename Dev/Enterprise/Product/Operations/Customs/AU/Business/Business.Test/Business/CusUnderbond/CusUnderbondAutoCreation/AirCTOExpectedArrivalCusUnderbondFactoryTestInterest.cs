using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AirCTOExpectedArrivalCusUnderbondFactoryTestInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9938N";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = true;
			AirCTOExpectedArrivalCusUnderbondFactory creator = new AirCTOExpectedArrivalCusUnderbondFactory();
			AssertEquals("CTO HAWBUnderbondApproval Interest", true, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("CTO OC Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
		}

		public void TestIsInterestedNotCTO()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsAirCTO = false;
			AirCTOExpectedArrivalCusUnderbondFactory creator = new AirCTOExpectedArrivalCusUnderbondFactory();
			AssertEquals("CTO HAWBUnderbondApproval Interest", false, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("CTO OC Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
		}
	}
}
