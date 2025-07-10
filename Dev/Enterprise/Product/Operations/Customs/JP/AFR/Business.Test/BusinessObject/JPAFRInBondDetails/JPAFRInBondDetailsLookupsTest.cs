using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRInBondDetailsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTemporaryLandingReasonCodeList()
		{
			var inbond = Factory.New<JPAFRInBondDetails>();
			AssertEquals(Factory.GetCachedValue<TemporaryLandingReasonCodeList>(), inbond.Lookups.TemporaryLandingReasonCodeList);
		}

		public void TestTransportModeList()
		{
			var inbond = Factory.New<JPAFRInBondDetails>();
			AssertEquals(Factory.GetCachedValue<TransportModeList>(), inbond.Lookups.TransportModeList);
		}
	}
}
