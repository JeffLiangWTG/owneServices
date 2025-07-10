using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class EnRouteIncidentTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteIncidentTransportMeansWrapper>
	{
		public void TestTypeOfIdentification()
		{
			AssertEquals("TypeOfIdentification should be equal to BN_TransportAtDepartureType.", "PL", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be equal to BN_TransportAtDepartureID.", "12", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality should be equal to BN_RN_NKTransportAtDepartureIDNationality.", "FR", Provider.Nationality);
		}

		protected override EnRouteIncidentTransportMeansWrapper GetProvider()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_RN_NKTransportAtDepartureIDNationality = "FR";
			incident.BN_TransportAtDepartureID = "12";
			incident.BN_TransportAtDepartureType = "PL";
			return EnRouteIncidentTransportMeansWrapper.New(incident);
		}
	}
}
