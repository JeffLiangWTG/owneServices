using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class DepartureTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansWrapper>
	{
		public void TestTypeOfIdentification()
		{
			AssertEquals("TypeOfIdentification should be mapped to TransportTypeAtDeparture", "9", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be mapped to TransportAtDeparture", "510PZ47", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality should be mapped to TransportCountryAtDeparture", Core.Constants.CountryCodes.Romania, Provider.Nationality);
		}

		protected override DepartureTransportMeansWrapper GetProvider()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.TransportTypeAtDeparture = "9";
			nctsHeader.MovementHeader.TransportAtDeparture = "510PZ47";
			nctsHeader.MovementHeader.TransportCountryAtDeparture = Core.Constants.CountryCodes.Romania;
			return DepartureTransportMeansWrapper.New(nctsHeader.MovementHeader);
		}
	}
}
