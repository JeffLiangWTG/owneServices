using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class DepartureTransportMeansTransportAtDepartureProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansTransportAtDepartureProvider>
	{
		public void TestIdentificationNumber()
		{
			const string identificationNumber = "123";
			movementHeader.BM_TransportAtDeparture = identificationNumber;

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

			AssertNull(Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenSeaTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

			AssertEquals(11, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenSeaTransportWithVessel()
		{
			var refVessel = Factory.New<RefVessel>();

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			refVessel.RV_LloydsNumber = movementHeader.BM_TransportAtDeparture;

			AssertEquals(10, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenRailTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

			AssertEquals(21, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenRoadTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;

			AssertEquals(30, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenAirTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;

			AssertEquals(40, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenInlandWaterwayTransport()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentificationWhenInlandWaterwayTransportWithVessel()
		{
			var refVessel = Factory.New<RefVessel>();

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			refVessel.RV_LloydsNumber = movementHeader.BM_TransportAtDeparture;
			AssertEquals(80, Provider.TypeOfIdentification);
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

			provider = new DepartureTransportMeansTransportAtDepartureProvider(movementHeader, 1);
		}

		protected override DepartureTransportMeansTransportAtDepartureProvider GetProvider() => provider;

		NctsDepartureMovementHeader movementHeader;
		DepartureTransportMeansTransportAtDepartureProvider provider;
	}
}
