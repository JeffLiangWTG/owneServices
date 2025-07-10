using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(NctsDepartureMovementHeaderWrapper))]
sealed class NctsDepartureMovementHeaderWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsDepartureMovementHeaderWrapper(null));
	}

	public void TestGetSecurityType()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals("When BM_TypeOfSecurity=ENT", 1, wrapper.GetSecurityType());

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals("When BM_TypeOfSecurity=EXI", 2, wrapper.GetSecurityType());

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals("When BM_TypeOfSecurity=BTH", 3, wrapper.GetSecurityType());

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals("When BM_TypeOfSecurity=NON", 0, wrapper.GetSecurityType());

			movementHeader.BM_TypeOfSecurity = "XXX";
			AssertEquals("When BM_TypeOfSecurity=XXX", 0, wrapper.GetSecurityType());
		});
	}

	public void TestGetReducedDatasetIndicator()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ReducedDatasetIndicator = ZBool.False;
			AssertEquals("When BM_ReducedDatasetIndicator=False", 0, wrapper.GetReducedDatasetIndicator());

			movementHeader.BM_ReducedDatasetIndicator = ZBool.True;
			AssertEquals("When BM_ReducedDatasetIndicator=True", 1, wrapper.GetReducedDatasetIndicator());
		});
	}

	public void TestSpecificCircumstanceIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals(GetMessage(), string.Empty, wrapper.GetSpecificCircumstanceIndicator());

			movementHeader.BM_SpecificCircumstance = "ABC";
			AssertEquals(GetMessage(), "ABC", wrapper.GetSpecificCircumstanceIndicator());
		});

		string GetMessage() => $"{nameof(movementHeader.BM_SpecificCircumstance)} = {movementHeader.BM_SpecificCircumstance}";
	}

	public void TestGetLimitDate()
	{
		movementHeader.BM_ExportDate = ZDateTime.Empty;
		AssertNull("When BM_ExportDate is empty, LimitDate", wrapper.GetLimitDate());

		movementHeader.BM_ExportDate = new DateTime(2023, 9, 20, 8, 30, 15);
		AssertEquals("When BM_ExportDate is filled, LimitDate", new DateTime(2023, 9, 20), wrapper.GetLimitDate());
	}

	public void TestGetPlaceOfLoading()
	{
		AssertNull("PlaceOfLoading", wrapper.GetPlaceOfLoading());

		movementHeader.BM_PortOfPresentationCode = "ITMIL";
		movementHeader.BM_PlaceOfLoading = "Milano";

		var placeOfLoading = wrapper.GetPlaceOfLoading();
		AssertNotNull("PlaceOfLoading", placeOfLoading);
		AssertType<PlaceOfLoadingWrapper>(placeOfLoading);
		AssertEquals(nameof(placeOfLoading.UNLOCODE), "ITMIL", placeOfLoading.UNLOCODE);
	}

	public void TestGetPlaceOfUnloading()
	{
		AssertNull("PlaceOfUnloading", wrapper.GetPlaceOfUnloading());

		using (TemporarilySetTransitionPeriod(false))
		{
			CombineAssertions("When TP OFF", () =>
			{
				movementHeader.BM_ForeignDestPortKCode = "ITMIL";
				movementHeader.BM_PlaceOfLoading = "Milano";
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				var placeOfUnloading = wrapper.GetPlaceOfUnloading();
				AssertNull("BM_TypeOfSecurity == NON", placeOfUnloading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				placeOfUnloading = wrapper.GetPlaceOfUnloading();
				AssertNotNull("BM_TypeOfSecurity != NON", placeOfUnloading);
				AssertType<PlaceOfUnloadingWrapper>(placeOfUnloading);
				AssertEquals(nameof(placeOfUnloading.UNLOCODE), "ITMIL", placeOfUnloading.UNLOCODE);
			});
		}

		using (TemporarilySetTransitionPeriod(true))
		{
			CombineAssertions("When TP ON", () =>
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				var placeOfUnloading = wrapper.GetPlaceOfUnloading();
				AssertNotNull("BM_TypeOfSecurity == NON", placeOfUnloading);
				AssertType<PlaceOfUnloadingWrapper>(placeOfUnloading);
				AssertEquals(nameof(placeOfUnloading.UNLOCODE), "ITMIL", placeOfUnloading.UNLOCODE);
			});
		}
	}

	public void TestGetLocationOfGoods()
	{
		var goodsLocation = movementHeader.GoodsLocation;
		AssertNull("LocationOfGoods", wrapper.GetLocationOfGoods());

		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		goodsLocation.CGL_Type = "D";

		var locationOfGoods = wrapper.GetLocationOfGoods();
		AssertNotNull("LocationOfGoods", locationOfGoods);
		AssertType<LocationOfGoodsWrapper>(locationOfGoods);
		AssertEquals("D", locationOfGoods.TypeOfLocation);
	}

	public void TestGetDepartureTransportMeans()
	{
		var departureMeansOfTransports = wrapper.GetDepartureTransportMeans();
		AssertNotNull("GetDepartureTransportMeans", departureMeansOfTransports);
		AssertEquals("GetDepartureTransportMeans", 0, departureMeansOfTransports.Count);

		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._11;
		movementHeader.BM_TransportAtDeparture = "Sea-N Schelde";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";

		departureMeansOfTransports = wrapper.GetDepartureTransportMeans();
		AssertEquals("GetDepartureTransportMeans", 1, departureMeansOfTransports.Count);
		AssertType<DepartureMeansOfTransportWrapper>(departureMeansOfTransports.Single());
	}

	public void TestGetDepartureTransportMeans_SameAtHouseConsignmentLevel()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		var bills = header.Bills;
		var houseConsignment1 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment1, NctsTransportTypeOfIdList.Codes._20, "WN001", "IT", "AWN001", "IT", "AWN002", "IT");
		var houseConsignment2 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment2, NctsTransportTypeOfIdList.Codes._20, "WN001", "IT", "AWN002", "IT", "AWN001", "IT");

		var departureMeansOfTransports = wrapper.GetDepartureTransportMeans().ToArray();

		CombineAssertions("When all Transport Departure fields of House Consignments are the same, they should be written at the header level.", () =>
		{
			AssertEquals("Count", 3, departureMeansOfTransports.Length);
			var departureMeansOfTransport = departureMeansOfTransports[0];
			AssertEquals(nameof(departureMeansOfTransport.TypeOfIdentification), 20, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(departureMeansOfTransport.IdentificationNumber), "WN001", departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(departureMeansOfTransport.Nationality), "IT", departureMeansOfTransport.Nationality);

			var departureMeansOfTransport2 = departureMeansOfTransports[1];
			AssertEquals("Wagon Number 1", "AWN001", departureMeansOfTransport2.IdentificationNumber);
			AssertEquals("Wagon Nationality 1", "IT", departureMeansOfTransport2.Nationality);

			var departureMeansOfTransport3 = departureMeansOfTransports[2];
			AssertEquals("Wagon Number 2", "AWN002", departureMeansOfTransport3.IdentificationNumber);
			AssertEquals("Wagon Nationality 2", "IT", departureMeansOfTransport3.Nationality);
		});
	}

	static void SetUpHouseConsignmentTransportDeparture(NctsBill houseConsignment, ZString typeOfIdentification, ZString identificationNumber, ZString transportNationality, ZString wagonNumber1, ZString wagonNationality1, ZString wagonNumber2, ZString wagonNationality2)
	{
		houseConsignment.TransportTypeAtDeparture = typeOfIdentification;
		houseConsignment.TransportAtDeparture = identificationNumber;
		houseConsignment.TransportCountryAtDeparture = transportNationality;

		var additionalWagon1 = houseConsignment.AdditionalWagons.AddNew();
		additionalWagon1.WagonNumber = wagonNumber1;
		additionalWagon1.WagonNationality = wagonNationality1;

		var additionalWagon2 = houseConsignment.AdditionalWagons.AddNew();
		additionalWagon2.WagonNumber = wagonNumber2;
		additionalWagon2.WagonNationality = wagonNationality2;
	}

	public void TestGetDepartureTransportMeans_DifferentAtHouseConsignmentLevel()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;

		var bills = header.Bills;
		var houseConsignment1 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment1, NctsTransportTypeOfIdList.Codes._20, "WN001", "IT", "AWN001", "IT", "AWN002", "IT");
		var houseConsignment2 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment2, NctsTransportTypeOfIdList.Codes._20, "WN001", "IT", "AWN001", "IT", "AWN003", "IT");

		var departureMeansOfTransports = wrapper.GetDepartureTransportMeans();

		AssertNull("When the Transport Departure fields of House Consignments differ, they should be written at the line level.", departureMeansOfTransports);
	}

	public void TestGetActiveBorderTransportMeans()
	{
		var activeBorderTransportMeans = wrapper.GetActiveBorderTransportMeans();
		AssertNotNull("ActiveBorderTransportMeans", activeBorderTransportMeans);
		AssertEquals("ActiveBorderTransportMeans Count", 0, activeBorderTransportMeans.Count);

		var borderTransportMeans = movementHeader.AdditionalTransportAtBorderList.AddNew();
		CombineAssertions("When all involved [MovementHeader, BorderTransportMeans] fields are empty", () =>
		{
			activeBorderTransportMeans = wrapper.GetActiveBorderTransportMeans();
			AssertEquals("ActiveBorderTransportMeans Count", 0, activeBorderTransportMeans.Count);
		});

		CombineAssertions("When all involved [BorderTransportMeans] fields are empty", () =>
		{
			movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			activeBorderTransportMeans = wrapper.GetActiveBorderTransportMeans();
			AssertEquals("ActiveBorderTransportMeans Count", 1, activeBorderTransportMeans.Count);
			AssertType<ActiveBorderMeansOfTransportWrapper>(wrapper.GetActiveBorderTransportMeans().Single());
		});

		CombineAssertions("When all involved [MovementHeader, BorderTransportMeans] fields are not empty", () =>
		{
			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
			activeBorderTransportMeans = wrapper.GetActiveBorderTransportMeans();
			AssertEquals("ActiveBorderTransportMeans Count", 2, activeBorderTransportMeans.Count);
		});
	}

	IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isTransitionPeriodActive);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		wrapper = new NctsDepartureMovementHeaderWrapper(movementHeader);
	}

	NctsHeader header;
	NctsDepartureMovementHeader movementHeader;
	INctsDepartureMovementHeaderWrapper wrapper;
}
