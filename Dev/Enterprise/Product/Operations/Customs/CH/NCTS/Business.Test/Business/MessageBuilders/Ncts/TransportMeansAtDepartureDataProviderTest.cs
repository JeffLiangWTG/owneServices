using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class TransportMeansAtDepartureDataProviderTest : BaseDepartureDataProviderTest<TransportMeansAtDepartureDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNew() => AssertNull("NctsHeader==null", TransportMeansAtDepartureDataProvider.New(null));

	public void TestProvider()
	{
		const string nationality = Core.Constants.CountryCodes.Belgium;
		const string identificationNumber = "ID";

		NctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = nationality;
		NctsHeader.MovementHeader.BM_TransportAtDeparture = identificationNumber;

		CombineAssertions(() =>
		{
			AssertEquals("Nationality", nationality, DataProvider.Nationality);
			AssertEquals("IdentificationNumber", identificationNumber, DataProvider.IdentificationNumber);
			AssertEquals("TypeOfIdentification", string.Empty, DataProvider.TypeOfIdentification);
		});
	}

	public void TestIdentificationNumber_TransportMode4()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			NctsHeader.MovementHeader.BM_TransportAtDeparture = "TransportAtDeparture";
			var transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TypeOfID == 40, value should be taken from BM_TransportAtDeparture", "TransportAtDeparture", transportMeansDataProvider.IdentificationNumber);

			NctsHeader.MovementHeader.BM_AircraftIDAtDeparture = "AircraftIDAtDeparture";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TypeOfID == 41, value should be taken from BM_AircraftIDAtDeparture", "AircraftIDAtDeparture", transportMeansDataProvider.IdentificationNumber);

			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			NctsHeader.MovementHeader.BM_TransportAtDeparture = "TransportAtDeparture";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("Default value should be taken from BM_TransportAtDeparture", "TransportAtDeparture", transportMeansDataProvider.IdentificationNumber);
		});
	}

	public void TestTypeOfIdentification_TransportMode2()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			NctsHeader.MovementHeader.BM_TransportAtDeparture = "";
			var transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is RAIL and TransportAtDeparture is empty, TypeOfID should be: ", ZString.Empty, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.BM_TransportAtDeparture = "X";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is RAIL and TransportAtDeparture != empty, TypeOfID should be: ", NctsTransportTypeOfIdList.Codes._21, transportMeansDataProvider.TypeOfIdentification);
		});
	}

	public void TestTypeOfIdentification_TransportMode3()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			NctsHeader.MovementHeader.BM_TransportAtDeparture = "";
			var transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is ROAD and TransportAtDeparture is empty, TypeOfID should be: ", ZString.Empty, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.BM_TransportAtDeparture = "X";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is ROAD and TransportAtDeparture != empty, TypeOfID should be: ", NctsTransportTypeOfIdList.Codes._30, transportMeansDataProvider.TypeOfIdentification);
		});
	}

	public void TestTypeOfIdentification_TransportMode4()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			NctsHeader.MovementHeader.BM_AircraftIDAtDeparture = "";
			NctsHeader.MovementHeader.BM_TransportAtDeparture = "";
			var transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is AIR and both TransportAtDeparture and AircraftIDAtDeparture are empty, TypeOfID should be: ", ZString.Empty, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.BM_TransportAtDeparture = "X";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is AIR and AircraftIDAtDeparture == empty, TypeOfID should be: ", NctsTransportTypeOfIdList.Codes._40, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.BM_AircraftIDAtDeparture = "X";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When TransportMode is AIR and TransportAtDeparture and AircraftIDAtDeparture != empty, TypeOfID should be: ", NctsTransportTypeOfIdList.Codes._41, transportMeansDataProvider.TypeOfIdentification);
		});
	}

	public void TestTypeOfIdentification_TransportMode8()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			NctsHeader.MovementHeader.VesselNameAtDeparture = "1234";
			var transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When VesselNameAtDeparture is all digit but not less then 8 characters should be:", NctsTransportTypeOfIdList.Codes._81, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.VesselNameAtDeparture = "123456789";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When VesselNameAtDeparture is all digit but not More then 8 characters should be:", NctsTransportTypeOfIdList.Codes._81, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.VesselNameAtDeparture = "Test1234";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When VesselNameAtDeparture is not all digit should be:", NctsTransportTypeOfIdList.Codes._81, transportMeansDataProvider.TypeOfIdentification);

			NctsHeader.MovementHeader.VesselNameAtDeparture = "12345678";
			transportMeansDataProvider = CreateDataProvider();
			AssertEquals("When VesselNameAtDeparture is all digit and 8 characters should be:", NctsTransportTypeOfIdList.Codes._80, transportMeansDataProvider.TypeOfIdentification);
		});
	}

	public void TestTypeOfIdentification_TransportMode9()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			NctsHeader.MovementHeader.BM_TransportAtDepartureType = "TI";
			AssertEquals("When BM_InlandTransportMode == OWN, then ID should be BM_TransportAtDepartureType", "TI", DataProvider.TypeOfIdentification);
		});
	}

	protected override TransportMeansAtDepartureDataProvider CreateDataProvider() => TransportMeansAtDepartureDataProvider.New(NctsHeader.MovementHeader);
}
