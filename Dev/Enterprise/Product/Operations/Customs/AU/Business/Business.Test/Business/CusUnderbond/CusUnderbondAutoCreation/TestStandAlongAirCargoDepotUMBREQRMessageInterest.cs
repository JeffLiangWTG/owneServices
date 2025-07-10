using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TestStandAlongAirCargoDepotUMBREQRMessageInterest : MessageFactoryInterestTest
	{
		public override void TestIsInterested()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9938N";
			currentCompany.OrgProxy.OH_IsUnpackDepot = true;
			StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory creator = new StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory();
			AssertEquals("Depot HAWBUnderbondApproval Interest", true, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("Depot Sea Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
		}

		public void TestIsInterestedNotDepot()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "";
			GlbCompany.CurrentCompany.OrgProxy.OH_IsUnpackDepot = false;
			StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory creator = new StandAloneAirCargoDepotExpectedArrivalCusUnderbondFactory();
			AssertEquals("Depot HAWBUnderbondApproval Interest", false, creator.IsInterestedInUBMREQRInternal(HAWBUnderbondApproval));
			AssertEquals("Depot Sea Interest", false, creator.IsInterestedInUBMREQRInternal(OBL250805001_ExpectedCargoArrivalAdviceMessage_FRCU3948922));
		}
	}
}
