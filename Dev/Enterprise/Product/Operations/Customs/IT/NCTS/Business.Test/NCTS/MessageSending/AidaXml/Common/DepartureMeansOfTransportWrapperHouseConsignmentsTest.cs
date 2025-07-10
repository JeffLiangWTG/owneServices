using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class DepartureMeansOfTransportWrapperHouseConsignmentsTest : TestCaseWithFactory
{
	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsPostalConsignment_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
		bill.TransportAtDeparture = "RX2839A";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		AssertEquals(0, wrapper.DepartureMeansOfTransports.Count);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsFixedTransportInstallations_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
		bill.TransportAtDeparture = "RX2839A";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 10,
			expectedIdentificationNumber: "RX2839A",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsSeaTransport_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
		bill.VesselNameAtDeparture = "Sea-N Schelde";
		bill.VesselCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 11,
			expectedIdentificationNumber: "Sea-N Schelde",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRailTransport_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._21;
		bill.TransportAtDeparture = "Rail-train";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 21,
			expectedIdentificationNumber: "Rail-train",
			expectedNationality: "IT");
	}

	public void TestDepartureMeansOfTransports_ForRailTransport_WithAdditionalWagons_WithAdditionalWagons()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When TransportAtDeparture is empty", 0, departureMeansOfTransports.Count);

		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._20;
		bill.TransportAtDeparture = "Wagon ID 1";
		bill.TransportCountryAtDeparture = "IT";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When TransportAtDeparture has value", 1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(), 20, "Wagon ID 1", "IT");

		var wagon1 = bill.AdditionalWagons.AddNew();
		wagon1.WagonNumber = "Wagon ID 2";
		wagon1.WagonNationality = "IT";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When additional wagon has values in Wagon Number and Nationality", 2, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1), 20, "Wagon ID 2", "IT");

		var wagon2 = bill.AdditionalWagons.AddNew();
		wagon2.WagonNumber = "";
		wagon2.WagonNationality = "IT";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When additional wagon has empty Wagon Number", 2, departureMeansOfTransports.Count);

		var wagon3 = bill.AdditionalWagons.AddNew();
		wagon3.WagonNumber = "Wagon ID 3";
		wagon3.WagonNationality = "";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When additional wagon has empty Nationality", 3, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2), 20, "Wagon ID 3", "");

		var wagon4 = bill.AdditionalWagons.AddNew();
		wagon4.WagonNumber = "";
		wagon4.WagonNationality = "";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("When both Wagon Nubmer and Nationality are empty", 3, departureMeansOfTransports.Count);

		bill.TransportAtDeparture = "";
		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals("More button hidden when Wagon/Train No set to Empty, Expect not to output Records in Additional Wagons", 1, departureMeansOfTransports.Count);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsAirTransport_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
		bill.TransportAtDeparture = "Air-Flight#";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 40,
			expectedIdentificationNumber: "Air-Flight#",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsInlandWaterwayTransport_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;
		bill.TransportAtDeparture = "Water-ENICode";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 80,
			expectedIdentificationNumber: "Water-ENICode",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsOwnPropulsion_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
		bill.TransportAtDeparture = "Own-Transport";
		bill.TransportCountryAtDeparture = "IT";
		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 10,
			expectedIdentificationNumber: "Own-Transport",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRoadTransport_PartialData_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		var wrapper = CreateHouseConsignmentWrapper();
		AssertEquals(0, wrapper.DepartureMeansOfTransports.Count);

		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._30;
		bill2.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._31;

		wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "",
			expectedNationality: "");

		bill.Trailer1IDAtDeparture = "Trailer1";
		bill.TransportCountryAtDeparture = "IT";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(2, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "",
			expectedNationality: "IT");

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: "");

		bill.Trailer2NationalityAtDeparture = "DE";
		bill.Trailer2IDAtDeparture = "Trailer2";
		bill.Trailer1NationalityAtDeparture = "FR";
		bill.TransportAtDeparture = "Trailer";

		wrapper = CreateHouseConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals(3, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "Trailer",
			expectedNationality: "IT");

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: "FR");

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer2",
			expectedNationality: "DE");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRoadTransport_HouseConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._30;

		bill.Trailer2IDAtDeparture = "Trailer2";
		bill.Trailer2NationalityAtDeparture = "DE";

		bill.Trailer1NationalityAtDeparture = "FR";
		bill.Trailer1IDAtDeparture = "Trailer1";

		bill.TransportAtDeparture = "Trailer";
		bill.TransportCountryAtDeparture = "IT";

		var wrapper = CreateHouseConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals(3, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "Trailer",
			expectedNationality: "IT");

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: "FR");

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer2",
			expectedNationality: "DE");
	}

	protected override void SetUp()
	{
		header = Factory.NewDepartureNctsHeaderPhase5();
		bill = header.Bills.AddNew();
		bill2 = header.Bills.AddNew();
		movementHeader = header.MovementHeader;
	}

	void AssertDepartureMeansOfTransport(IMeansOfTransport departureMeansOfTransport
		, int expectedTypeOfIdentification
		, string expectedIdentificationNumber
		, string expectedNationality)
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), expectedIdentificationNumber, departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), expectedTypeOfIdentification, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(IMeansOfTransport.Nationality), expectedNationality, departureMeansOfTransport.Nationality);
		});
	}

	IHouseConsignmentCustomsMessageWrapper CreateHouseConsignmentWrapper()
	{
		return new HouseConsignmentCustomsMessageWrapper(bill);
	}

	NctsHeader header;
	NctsBill bill;
	NctsBill bill2;
	NctsDepartureMovementHeader movementHeader;
}
