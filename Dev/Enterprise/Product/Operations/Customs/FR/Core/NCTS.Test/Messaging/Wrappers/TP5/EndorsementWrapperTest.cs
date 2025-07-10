using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class EndorsementWrapperTest : Customs.Business.Testing.DataProviderTestCase<EndorsementWrapper>
	{
		public void TestDate()
		{
			AssertEquals("Date should be equal to BN_EndorsementDate.", ZDateTime.Today.AddDays(1), Provider.Date);
		}

		public void TestAuthority()
		{
			AssertEquals("Authority should be equal to BN_EndorsementAuthority.", "auth", Provider.Authority);
		}

		public void TestPlace()
		{
			AssertEquals("Place should be equal to BN_EndorsementPlace.", "paris", Provider.Place);
		}

		public void TestCountry()
		{
			AssertEquals("BN_EndorsementCountryCode should be equal to BN_EndorsementCountryCode.", "FR", Provider.Country);
		}

		protected override EndorsementWrapper GetProvider()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_EndorsementDate = ZDateTime.Today.AddDays(1);
			incident.BN_EndorsementAuthority = "auth";
			incident.BN_EndorsementPlace = "paris";
			incident.BN_EndorsementCountryCode = "FR";
			return EndorsementWrapper.New(incident);
		}
	}
}
