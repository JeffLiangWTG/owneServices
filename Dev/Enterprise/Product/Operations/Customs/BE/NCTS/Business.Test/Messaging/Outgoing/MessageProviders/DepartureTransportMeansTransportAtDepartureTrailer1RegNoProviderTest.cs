using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class DepartureTransportMeansTransportAtDepartureTrailer1RegNoProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider>
	{
		public void TestIdentificationNumber()
		{
			const string identificationNumber = "123";
			movementHeader.BM_TransportAtDeparture = "456";
			movementHeader.BM_TransportAtDepartureTrailer1RegNo = identificationNumber;

			AssertEquals(identificationNumber, Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			const string nationality = "BE";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = nationality;

			AssertEquals(nationality, Provider.Nationality);
		}

		public void TestTypeOfIdentification()
		{
			movementHeader.BM_InlandTransportMode = ZString.Empty;

			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenSeaTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

			AssertEquals(11, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenRailTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

			AssertEquals(20, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenRoadTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;

			AssertEquals(31, Provider.TypeOfIdentification);
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
			header.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = header.MovementHeader;

			provider = new DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(movementHeader, 1);
		}

		protected override DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider GetProvider() => provider;

		NctsDepartureMovementHeader movementHeader;
		DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider provider;
	}
}
