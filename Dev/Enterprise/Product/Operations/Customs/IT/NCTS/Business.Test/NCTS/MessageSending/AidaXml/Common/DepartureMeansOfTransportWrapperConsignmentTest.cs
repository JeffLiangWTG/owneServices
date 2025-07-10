using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class DepartureMeansOfTransportWrapperConsignmentsTest : TestCaseWithFactory
{
	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsPostalConsignment()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
		movementHeader.BM_TransportAtDeparture = "RX2839A";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		AssertEquals(0, wrapper.DepartureMeansOfTransports.Count);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsFixedTransportInstallations()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;
		movementHeader.BM_TransportAtDeparture = "RX2839A";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 10,
			expectedIdentificationNumber: "RX2839A",
			expectedNationality: "IT");
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsSeaTransport()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._11;
		movementHeader.BM_TransportAtDeparture = "Sea-N Schelde";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 11,
			expectedIdentificationNumber: "Sea-N Schelde",
			expectedNationality: "IT"
			);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRailTransport()
	{
		NctsConfigurationTestHelper.RunAssertionsInAndOutPhase5TransitionPeriod(
			insidePhase5: () => AssertInlandTransportModeIsRailTransport("When NCTSTP is ON, and InlandTransportModeAtDeparture is rail (RuleB1897)", expectedNationality: string.Empty),
			outsidePhase5: () => AssertInlandTransportModeIsRailTransport("When NCTSTP is OFF, and InlandTransportModeAtDeparture is rail", expectedNationality: "IT"));
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRailTransport_WithAdditionalWagons_WhenTransitionPeriodIsON()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, true))
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_TransportAtDepartureType = ZString.Empty;
			CombineAssertions(() =>
			{
				var wrapper = CreateConsignmentWrapper();
				var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
				AssertEquals("Expect 0 when BM_TransportAtDeparture is empty", 0, departureMeansOfTransports.Count);

				movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._20;
				movementHeader.BM_TransportAtDeparture = "Wagon ID 1";
				movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";

				wrapper = CreateConsignmentWrapper();
				departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
				AssertEquals("Expect 1 when BM_TransportAtDeparture has value", 1, departureMeansOfTransports.Count);

				var wagon1 = movementHeader.InlandTransportList.AddNew();
				wagon1.WagonNumber = "Wagon ID 2";
				wagon1.WagonNationality = "IT";

				wrapper = CreateConsignmentWrapper();
				departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
				AssertEquals("No additional wagon is output when TP is ON", 1, departureMeansOfTransports.Count);
			});
		}
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRailTransport_WithAdditionalWagons_WhenTransitionPeriodIsOFF()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, "EUN", ZDate.Today, false))
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_TransportAtDepartureType = ZString.Empty;
			var wrapper = CreateConsignmentWrapper();
			var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When BM_TransportAtDeparture is empty", 0, departureMeansOfTransports.Count);

			movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._20;
			movementHeader.BM_TransportAtDeparture = "Wagon ID 1";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";

			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When BM_TransportAtDeparture has value", 1, departureMeansOfTransports.Count);
			AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(), 20, "Wagon ID 1", "IT");

			var wagon1 = movementHeader.AdditionalWagons.AddNew();
			wagon1.WagonNumber = "Wagon ID 2";
			wagon1.WagonNationality = "IT";

			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When additional wagon has values in Wagon Number and Nationality", 2, departureMeansOfTransports.Count);
			AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1), 20, "Wagon ID 2", "IT");

			var wagon2 = movementHeader.AdditionalWagons.AddNew();
			wagon2.WagonNumber = "";
			wagon2.WagonNationality = "IT";

			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When additional wagon has empty Wagon Number", 2, departureMeansOfTransports.Count);

			var wagon3 = movementHeader.AdditionalWagons.AddNew();
			wagon3.WagonNumber = "Wagon ID 3";
			wagon3.WagonNationality = "";

			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When additional wagon has empty Nationality", 3, departureMeansOfTransports.Count);
			AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2), 20, "Wagon ID 3", "");

			var wagon4 = movementHeader.AdditionalWagons.AddNew();
			wagon4.WagonNumber = "";
			wagon4.WagonNationality = "";

			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("When both Wagon Nubmer and Nationality are empty", 3, departureMeansOfTransports.Count);

			movementHeader.BM_TransportAtDeparture = "";
			wrapper = CreateConsignmentWrapper();
			departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
			AssertEquals("More button hidden when Wagon/Train No set to Empty, Expect not to output Records in Additional Wagons", 1, departureMeansOfTransports.Count);
		}
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsAirTransport()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._40;
		movementHeader.BM_TransportAtDeparture = "Air-Flight#";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 40,
			expectedIdentificationNumber: "Air-Flight#",
			expectedNationality: "IT"
			);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsInlandWaterwayTransport()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._80;
		movementHeader.BM_TransportAtDeparture = "Air-ENICode";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 80,
			expectedIdentificationNumber: "Air-ENICode",
			expectedNationality: "IT"
			);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsOwnPropulsion()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._10;
		movementHeader.BM_TransportAtDeparture = "Own-Transport";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 10,
			expectedIdentificationNumber: "Own-Transport",
			expectedNationality: "IT"
			);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRoadTransport_PartialData()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		movementHeader.BM_TransportAtDepartureType = ZString.Empty;
		var wrapper = CreateConsignmentWrapper();
		AssertEquals(0, wrapper.DepartureMeansOfTransports.Count);

		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._30;

		wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(1, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "",
			expectedNationality: ""
			);

		movementHeader.Trailer1IDAtDeparture = "Trailer1";
		movementHeader.TransportCountryAtDeparture = "IT";

		wrapper = CreateConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals(2, wrapper.DepartureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "",
			expectedNationality: "IT"
			);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: ""
			);

		movementHeader.Trailer2NationalityAtDeparture = "DE";
		movementHeader.Trailer2IDAtDeparture = "Trailer2";
		movementHeader.Trailer1NationalityAtDeparture = "FR";
		movementHeader.BM_TransportAtDeparture = "Trailer";

		wrapper = CreateConsignmentWrapper();
		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals(3, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "Trailer",
			expectedNationality: "IT"
			);

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: "FR"
			);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer2",
			expectedNationality: "DE"
			);
	}

	public void TestGetDepartureMeansOfTransportWrapper_WhenInlandTransportModeIsRoadTransport_MovementHeader()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._30;

		movementHeader.Trailer2IDAtDeparture = "Trailer2";
		movementHeader.Trailer2NationalityAtDeparture = "DE";

		movementHeader.Trailer1NationalityAtDeparture = "FR";
		movementHeader.Trailer1IDAtDeparture = "Trailer1";

		movementHeader.BM_TransportAtDeparture = "Trailer";
		movementHeader.TransportCountryAtDeparture = "IT";

		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals(3, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(0),
			expectedTypeOfIdentification: 30,
			expectedIdentificationNumber: "Trailer",
			expectedNationality: "IT"
			);

		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(1),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer1",
			expectedNationality: "FR"
			);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.ElementAt(2),
			expectedTypeOfIdentification: 31,
			expectedIdentificationNumber: "Trailer2",
			expectedNationality: "DE"
			);
	}

	protected override void SetUp()
	{
		header = Factory.NewDepartureNctsHeaderPhase5();
		header.Bills.AddNew();
		movementHeader = header.MovementHeader;

		nctsHeaderWrapper = new Mock<INctsHeaderWrapper>();
		nctsHeaderWrapper.Setup(x => x.GetConsignee()).Returns(new EoriOrTcuTraderWrapper(header.Consignee));
		messageSendingObjectFactory = new Mock<IMessageSendingWrapperFactory>();
		messageSendingObjectFactory.Setup(x => x.GetNewNctsHeaderWrapper(It.IsAny<NctsHeader>())).Returns(nctsHeaderWrapper.Object);
	}

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;

	void AssertDepartureMeansOfTransport(IMeansOfTransport departureMeansOfTransport
		, int expectedTypeOfIdentification
		, string expectedIdentificationNumber
		, string expectedNationality, string message = null)
	{
		CombineAssertions(message, () =>
		{
			AssertEquals(nameof(IMeansOfTransport.IdentificationNumber), expectedIdentificationNumber, departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(IMeansOfTransport.TypeOfIdentification), expectedTypeOfIdentification, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(IMeansOfTransport.Nationality), expectedNationality, departureMeansOfTransport.Nationality);
		});
	}

	void AssertInlandTransportModeIsRailTransport(string message, string expectedNationality)
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._21;
		movementHeader.BM_TransportAtDeparture = "Rail-train";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";

		var wrapper = CreateConsignmentWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals(1, departureMeansOfTransports.Count);
		AssertDepartureMeansOfTransport(departureMeansOfTransports.Single(),
			expectedTypeOfIdentification: 21,
			expectedIdentificationNumber: "Rail-train",
			expectedNationality: expectedNationality,
			message
			);
	}

	ID1Consignment CreateConsignmentWrapper()
	{
		return new D1ConsignmentWrapper(header, messageSendingObjectFactory.Object);
	}

	Mock<IMessageSendingWrapperFactory> messageSendingObjectFactory;
	Mock<INctsHeaderWrapper> nctsHeaderWrapper;
}
