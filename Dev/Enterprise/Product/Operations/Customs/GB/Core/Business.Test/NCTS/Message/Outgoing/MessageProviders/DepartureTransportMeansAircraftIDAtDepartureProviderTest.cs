using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class DepartureTransportMeansAircraftIDAtDepartureProviderTest : DataProviderTestCase<DepartureTransportMeansAircraftIDAtDepartureProvider>
	{
		public void TestIdentificationNumber()
		{
			const string identificationNumber = "123";
			movementHeader.BM_TransportAtDeparture = "456";
			movementHeader.BM_AircraftIDAtDeparture = identificationNumber;

			AssertEquals(identificationNumber, Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			const string nationality = "BE";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = nationality;

			AssertEquals(nationality, Provider.Nationality);
		}

		public void TestTypeOfIdentification()
		{
			movementHeader.BM_InlandTransportMode = ZString.Empty;

			AssertEquals(0, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenSeaTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

			AssertEquals(11, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenAirTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;

			AssertEquals(41, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenInlandWaterwayTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenOwnPropulsion()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			const string transportAtDepartureType = "11";
			movementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			AssertEquals(11, Provider.TypeOfIdentification);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			movementHeader = header.MovementHeader;

			provider = new DepartureTransportMeansAircraftIDAtDepartureProvider(movementHeader, 1);
		}

		protected override DepartureTransportMeansAircraftIDAtDepartureProvider GetProvider() => provider;

		NctsDepartureMovementHeader movementHeader;
		DepartureTransportMeansAircraftIDAtDepartureProvider provider;
	}
}
