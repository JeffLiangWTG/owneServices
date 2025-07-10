using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.NCTS.MessageSending.AidaXml;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(D2AdditionalDataProvider))]
sealed class D2AdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new D2AdditionalDataProvider(null, false));
	}

	public void TestGetSecurity()
	{
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetSecurityType()).Returns(6);
		AssertEquals(6, dataProvider.GetSecurity(CreateMovementHeader()));
	}

	public void TestGetReducedDatasetIndicator()
	{
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetReducedDatasetIndicator()).Returns(1);
		AssertEquals(1, dataProvider.GetReducedDatasetIndicator(CreateMovementHeader()));
	}

	public void TestGetSpecificCircumstanceIndicator()
	{
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetSpecificCircumstanceIndicator()).Returns("A20");
		AssertEquals("A20", dataProvider.GetSpecificCircumstanceIndicator(CreateMovementHeader()));
	}

	public void TestGetLimitDate()
	{
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetLimitDate()).Returns(new DateTime(2025, 01, 01));
		AssertEquals(new DateTime(2025, 01, 01), dataProvider.GetLimitDate(CreateMovementHeader()));
	}

	public void TestGetBindingItinerary()
	{
		nctsHeaderWrapperMock.Setup(h => h.GetBindingItinerary()).Returns(1);
		AssertEquals(1, dataProvider.GetBindingItinerary(CreateHeader()));
	}

	public void TestGetCarrier()
	{
		var carrier = Mock.Of<CarrierTypeDataProviderAbstractClass>();
		nctsHeaderWrapperMock.Setup(h => h.GetCarrier()).Returns(carrier);
		AssertSame(carrier, dataProvider.GetCarrier(CreateHeader()));
	}

	public void TestGetContainerIndicator()
	{
		nctsHeaderWrapperMock.Setup(n => n.GetContainerIndicator()).Returns(1);
		AssertEquals(1, dataProvider.GetContainerIndicator(CreateHeader()));
	}

	public void TestGetPlaceOfLoading()
	{
		var placeOfLoading = Mock.Of<PlaceOfLoadingTypeDataProviderAbstractClass>();
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetPlaceOfLoading()).Returns(placeOfLoading);
		AssertSame(placeOfLoading, dataProvider.GetPlaceOfLoading(CreateHeader()));
	}

	public void TestGetPlaceOfUnloading()
	{
		var placeOfUnloading = Mock.Of<PlaceOfUnloadingTypeDataProviderAbstractClass>();
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetPlaceOfUnloading()).Returns(placeOfUnloading);
		AssertSame(placeOfUnloading, dataProvider.GetPlaceOfUnloading(CreateHeader()));
	}

	public void TestGetLocationOfGoods()
	{
		var locationOfGoodsType = Mock.Of<LocationOfGoodsTypeDataProviderAbstractClass>();
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetLocationOfGoods()).Returns(locationOfGoodsType);
		AssertSame(locationOfGoodsType, dataProvider.GetLocationOfGoods(CreateHeader()));
	}

	public void TestGetDepartureTransportMeans()
	{
		var departureTransportMeansMock = Mock.Of<DepartureTransportMeansTypeDataProviderAbstractClass>();
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetDepartureTransportMeans())
			.Returns(new[] { departureTransportMeansMock });

		var means = dataProvider.GetDepartureTransportMeans(CreateHeader());
		AssertNotNull("DepartureTransportMeans", means);
		AssertEquals("Count", 1, means.Count);
	}

	public void TestGetActiveBorderTransportMeans()
	{
		movementHeaderCusMessageWrapperMock.Setup(m => m.GetActiveBorderTransportMeans())
			.Returns(new[] { Mock.Of<ActiveBorderTransportMeansTypeDataProviderAbstractClass>() });

		var activeBorderTransportMeans = dataProvider.GetActiveBorderTransportMeans(CreateHeader());
		AssertNotNull("ActiveBorderTransportMeans", activeBorderTransportMeans);
		AssertEquals("Count", 1, activeBorderTransportMeans.Count);
	}

	public void TestGetGoodsReference()
	{
		nctsHeaderWrapperMock.Setup(m => m.GetGoodsReference(It.IsAny<NctsDepartureHeaderContainer>()))
			.Returns(new[] { Mock.Of<GoodsReferenceTypeDataProviderAbstractClass>() });
		var goodsReferences = dataProvider.GetGoodsReference(Factory.NewWithValidTestData<NctsDepartureHeaderContainer>());
		AssertNotNull("GetGoodsReference", goodsReferences);
		AssertEquals("Count", 1, goodsReferences.Count);
	}

	public void TestGetSeal()
	{
		var seals = dataProvider.GetSeal(null);
		AssertNull("Seals", seals);

		var nctsHeader = Factory.NewDepartureNctsHeader();
		var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
		seals = dataProvider.GetSeal(headerContainer);
		AssertNotNull(seals);
		AssertSequencesEqual(Array.Empty<string>(), seals.Select(s => s.Identifier));

		headerContainer.BC_ContainerNum = "TURE123456";
		headerContainer.Seal1 = "Seal1";
		headerContainer.Seal2 = "Seal2";

		seals = dataProvider.GetSeal(headerContainer);
		AssertSequencesEqual(new[] { "Seal1", "Seal2" }, seals.Select(s => s.Identifier));

		headerContainer.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";
		seals = dataProvider.GetSeal(headerContainer);
		AssertSequencesEqual(new[] { "Seal1", "Seal2", "Seal3" }, seals.Select(s => s.Identifier));
	}

	public void TestGetNumberOfPackages()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var package = (NctsPackage)goodsItem.Packages.AddNew();

		package.B5_UnitCount = 99;
		AssertEquals("GetNumberOfPackages", 99, dataProvider.GetNumberOfPackages(package));

		package.B5_UnitCount = 99999999;
		AssertEquals("GetNumberOfPackages", 99999999, dataProvider.GetNumberOfPackages(package));
		AssertEquals("IT NCTS Package", 1, dataProvider.GetNumberOfPackages(Factory.New<NctsPackage>()));
	}

	public void TestGetHouseConsignmentToBeDeleted()
	{
		var header = CreateHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_Status = "DLR";

		var bill = header.Bills.AddNew();
		bill.B0_BillStatus = "DLR";
		bill.GoodsItems.AddNew();

		var sendingObject = new NctsHeaderMessageSendingObject(header);
		sendingObject.MessageType = "AMD";

		var houseConsignmentToBeDeleted = dataProvider.GetHouseConsignmentToBeDeleted(sendingObject);
		AssertEquals("When MessageType is AMD, GetHouseConsignmentToBeDeleted", 2, houseConsignmentToBeDeleted.Count);

		sendingObject.MessageType = "";
		AssertNull("GetHouseConsignmentToBeDeleted", dataProvider.GetHouseConsignmentToBeDeleted(sendingObject));
	}

	public void TestGetDangerousGoods()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

		var substance0010 = UNDGSubstanceLoader.LoadSubstances(Factory, "0010", "", "IMO").First();
		var substance0012a = UNDGSubstanceLoader.LoadSubstances(Factory, "0012", "a", "IMO").First();

		goodsItem.UNDGs.AddNew();
		goodsItem.UNDGs.AddNew().DI_DG = substance0010.PK;
		goodsItem.UNDGs.AddNew().DI_DG = substance0012a.PK;
		goodsItem.UNDGs.AddNew().DI_DG = substance0012a.PK;

		var dangerousGoodsWrapper = dataProvider.GetDangerousGoods(goodsItem);
		AssertContainsExactElementsInExactOrder("Dangerous Goods SequenceNumbers", new[] { 1, 2 }, dangerousGoodsWrapper.Select(x => x.SequenceNumber).ToArray());
		AssertContainsExactElementsInAnyOrder("Dangerous Goods UNNumbers", new[] { "0010", "0012" }, dangerousGoodsWrapper.Select(x => x.UNNumber).ToArray());
	}

	public void TestConsigneeAddressWithoutIdentificationNumber()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		var identificationNumber = jobDocAddress.GetEuIdentificationNumber();
		AssertNullOrEmpty(nameof(identificationNumber), identificationNumber);

		var address = dataProvider.GetAddress(jobDocAddress);
		AssertNotNull(nameof(address), address);
	}

	public void TestConsigneeAddressWithIdentificationNumber()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var jobDocAddress = Factory.New<JobDocAddress>();

		jobDocAddress.E2_OA_Address = orgAddress.PK;
		jobDocAddress.Organisation.CustomsCodes.AddNew("EOR", "385040449", "US");

		var identificationNumber = jobDocAddress.GetEuIdentificationNumber();
		AssertNotNullOrEmpty(nameof(identificationNumber), identificationNumber);

		var address = dataProvider.GetAddress(jobDocAddress);
		AssertNull(nameof(address), address);
	}

	public void TestConsigneeNameWithoutIdentificationNumber()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		orgAddress.CompanyName = "GigaCorp";

		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;

		var identificationNumber = jobDocAddress.GetEuIdentificationNumber();
		AssertNullOrEmpty(nameof(identificationNumber), identificationNumber);

		var name = jobDocAddress.GetTraderNameOrNull(false);
		AssertEquals("Company Name should be present", "GigaCorp", name);
	}

	public void TestConsigneeNameWithIdentificationNumber()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		orgAddress.CompanyName = "GigaCorp";

		var jobDocAddress = Factory.New<JobDocAddress>();
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		jobDocAddress.Organisation.CustomsCodes.AddNew("EOR", "385040449", "US");

		var identificationNumber = jobDocAddress.GetEuIdentificationNumber();
		AssertNotNullOrEmpty(nameof(identificationNumber), identificationNumber);

		var name = jobDocAddress.GetTraderNameOrNull(false);
		AssertNullOrEmpty(nameof(name), name);
	}

	public void TestConsigneeAddressTruncation()
	{
		var isInPhase5TransitionPeriod = true;
		var testCases = CreateTestCases(isInPhase5TransitionPeriod);
		ValidateTestCases(testCases, isInPhase5TransitionPeriod);

		isInPhase5TransitionPeriod = false;
		testCases = CreateTestCases(isInPhase5TransitionPeriod);
		ValidateTestCases(testCases, isInPhase5TransitionPeriod);

		void ValidateTestCases((string TestCaseName, string City,
			string StreetAndNumber, string PostCode,
			string ExpectedCity, string ExpectedPostCode,
			string ExpectedStreetAndNumber)[] testCases, bool isInPhase5TransitionPeriod)
		{
			foreach (var testCase in testCases)
			{
				var orgAddress = Factory.New<OrgHeader>().MainAddress;
				orgAddress.Address1 = testCase.StreetAndNumber.Substring(0, testCase.StreetAndNumber.Length / 2);
				orgAddress.Address2 = testCase.StreetAndNumber.Substring(testCase.StreetAndNumber.Length / 2);
				orgAddress.City = testCase.City;
				orgAddress.Postcode = testCase.PostCode;

				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_OA_Address = orgAddress.PK;

				dataProvider = new D2AdditionalDataProvider(wrapperProvider, isInPhase5TransitionPeriod);
				var address = dataProvider.GetAddress(jobDocAddress);
				AssertNotNull(nameof(address), address);

				AssertEquals(testCase.TestCaseName + " address.StreetAndNumber", testCase.ExpectedStreetAndNumber, address.StreetAndNumber);
				AssertEquals(testCase.TestCaseName + " address.City", testCase.ExpectedCity, address.City);
				AssertEquals(testCase.TestCaseName + " address.Postcode", testCase.ExpectedPostCode, address.Postcode);
			}
		}

		(string TestCaseName, string City, string StreetAndNumber,
			string PostCode, string ExpectedCity, string ExpectedPostCode,
			string ExpectedStreetAndNumber)[]
			CreateTestCases(bool isInPhase5TransitionPeriod)
		{
			var maxStreetAndNumberLen = isInPhase5TransitionPeriod ? 35 : 70;
			var maxCityLen = 35;
			var maxPostCodeLen = 9;

			return new[] {
				(
					TestCaseName: "Oversized content",
					City: new string('a', maxCityLen + 5),
					StreetAndNumber: new string('b', maxStreetAndNumberLen + 5),
					PostCode: new string('1', maxPostCodeLen + 1),
					ExpectedCity: new string('a', maxCityLen),
					ExpectedPostCode: new string('1', maxPostCodeLen),
					ExpectedStreetAndNumber: (new string('b', (maxStreetAndNumberLen + 5) / 2) + " " + new string('b', maxStreetAndNumberLen - (maxStreetAndNumberLen + 5) / 2 - 1))
				),
				(
					TestCaseName: "Max sized content",
					City: new string('a', maxCityLen),
					StreetAndNumber: new string('b', maxStreetAndNumberLen),
					PostCode: new string('1', maxPostCodeLen),
					ExpectedCity: new string('a', maxCityLen),
					ExpectedPostCode: new string('1', maxPostCodeLen),
					ExpectedStreetAndNumber: (new string('b', maxStreetAndNumberLen / 2) + " " + new string('b', maxStreetAndNumberLen - maxStreetAndNumberLen / 2 - 1))
				),
				(
					TestCaseName: "Undersized content",
					City: new string('a', maxCityLen - 5),
					StreetAndNumber: new string('b', maxStreetAndNumberLen - 5),
					PostCode: new string('1', maxPostCodeLen - 5),
					ExpectedCity: new string('a', maxCityLen - 5),
					ExpectedPostCode: new string('1', maxPostCodeLen - 5),
					ExpectedStreetAndNumber: (new string('b', (maxStreetAndNumberLen - 5) / 2) + " " + new string('b', (maxStreetAndNumberLen - 5) - (maxStreetAndNumberLen - 5) / 2))
				),
			};
		}
	}

	public void TestConsigneeNameTruncation()
	{
		var isInPhase5TransitionPeriod = true;
		var testCases = CreateTestCases(isInPhase5TransitionPeriod);
		ValidateTestCases(testCases, isInPhase5TransitionPeriod);

		isInPhase5TransitionPeriod = false;
		testCases = CreateTestCases(isInPhase5TransitionPeriod);
		ValidateTestCases(testCases, isInPhase5TransitionPeriod);

		void ValidateTestCases((string TestCaseName, string Name, string ExpectedName)[] testCases, bool isInPhase5TransitionPeriod)
		{
			foreach (var testCase in testCases)
			{
				var orgAddress = Factory.New<OrgHeader>().MainAddress;
				orgAddress.CompanyName = testCase.Name;

				var jobDocAddress = Factory.New<JobDocAddress>();
				jobDocAddress.E2_OA_Address = orgAddress.PK;

				var name = jobDocAddress.GetTraderNameOrNull(isInPhase5TransitionPeriod);
				AssertEquals(testCase.TestCaseName + " Company Name should be truncated", testCase.ExpectedName, name);
			}
		}

		(string TestCaseName, string Name, string ExpectedName)[]
			CreateTestCases(bool isInPhase5TransitionPeriod)
		{
			var maxNameLen = isInPhase5TransitionPeriod ? 35 : 70;

			return new[] {
				(
					TestCaseName: "Oversized content",
					Name: new string('a', maxNameLen + 5),
					ExpectedName: new string('a', maxNameLen)
				),
				(
					TestCaseName: "Max sized content",
					Name: new string('a', maxNameLen),
					ExpectedName: new string('a', maxNameLen)
				),
				(
					TestCaseName: "Undersized content",
					Name: new string('a', maxNameLen - 5),
					ExpectedName: new string('a', maxNameLen - 5)
				),
			};
		}
	}

	public void TestGetCountryOfDestinationForConsignmentHeader()
	{
		using (SetTransitionPeriod(false))
		{
			var header = Factory.NewDepartureNctsHeaderPhase5();

			var bill1 = header.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			var goodItem12 = bill1.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();

			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", dataProvider.GetCountryOfDestination(header));

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			AssertEquals("Consignment: IT, HouseConsignment and GoodsItem: all empty", Core.Constants.CountryCodes.Italy, dataProvider.GetCountryOfDestination(header));

			bill1.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			AssertNullOrEmpty("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, empty, GoodsItem: all empty", dataProvider.GetCountryOfDestination(header));

			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			AssertEquals("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, dataProvider.GetCountryOfDestination(header));

			goodItem11.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			goodItem12.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			goodItem21.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, PL", dataProvider.GetCountryOfDestination(header));

			goodItem11.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, PL", Core.Constants.CountryCodes.Poland, dataProvider.GetCountryOfDestination(header));
		}

		using (SetTransitionPeriod(true))
		{
			AssertConsignmentHeaderFieldWithSingleLine("CountryOfDestination",
			(goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; },
			(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
			(header) => dataProvider.GetCountryOfDestination(header));

			AssertConsignmentHeaderFieldWithMultiLines("CountryOfDestination",
				(goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; },
				(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
				(header) => dataProvider.GetCountryOfDestination(header));
		}
	}

	public void TestGetCountryOfDestinationForHouseConsignment()
	{
		using (SetTransitionPeriod(false))
		{
			var header = Factory.NewDepartureNctsHeaderPhase5();
			var bill = header.Bills.AddNew();
			var goodItem1 = bill.GoodsItems.AddNew();
			var goodItem2 = bill.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			var goodItem3 = bill2.GoodsItems.AddNew();

			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", dataProvider.GetCountryOfDestination(bill));

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment: empty, IE,GoodsItem: all empty", Core.Constants.CountryCodes.Italy, dataProvider.GetCountryOfDestination(bill));

			bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			AssertEquals("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, IE, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, dataProvider.GetCountryOfDestination(bill));

			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			AssertNullOrEmpty("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", dataProvider.GetCountryOfDestination(bill));

			goodItem1.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			goodItem2.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			goodItem3.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, IE", dataProvider.GetCountryOfDestination(bill));

			goodItem1.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, IE", Core.Constants.CountryCodes.Poland, dataProvider.GetCountryOfDestination(bill));
		}
	}

	public void TestGetCountryOfDestinationForConsignmentItem()
	{
		using (SetTransitionPeriod(false))
		{
			var header = Factory.NewDepartureNctsHeaderPhase5();
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var goodsItem2 = bill.GoodsItems.AddNew();

			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", dataProvider.GetCountryOfDestination(goodsItem));

			header.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			goodsItem2.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			AssertEquals("Inherit from Consignment, Consignment: IT, HouseConsignment and GoodsItem: empty, PL", Core.Constants.CountryCodes.Italy, dataProvider.GetCountryOfDestination(goodsItem));

			bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Ireland;
			AssertEquals("Inherit from HouseConsignment, Consignment: IT, HouseConsignment: IE, GoodsItem: empty, PL", Core.Constants.CountryCodes.Ireland, dataProvider.GetCountryOfDestination(goodsItem));

			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			AssertNullOrEmpty("Same at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: PL, PL", dataProvider.GetCountryOfDestination(goodsItem));

			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			AssertEquals("Different at GoodsItem level, Consignment: IT, HouseConsignment: IE, GoodsItem: ES, PL", Core.Constants.CountryCodes.Spain, dataProvider.GetCountryOfDestination(goodsItem));
		}

		using (SetTransitionPeriod(true))
		{
			AssertConsignmentItemFieldWithSingleLine("CountryOfDestination",
			(goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; },
			(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
			(consignmentItem) => dataProvider.GetCountryOfDestination(consignmentItem));

			AssertConsignmentItemFieldWithMultiLine("CountryOfDestination",
				(goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; },
				(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
				(consignmentItem) => dataProvider.GetCountryOfDestination(consignmentItem));
		}
	}

	public void TestGetTransportInformation()
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();
		var bill = header.Bills.AddNew();
		bill.Header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
		bill.TransportAtDeparture = "Air-N Schelde";
		bill.TransportCountryAtDeparture = "IT";

		AssertEquals("When all Transport Departure fields of House Consignments are the same, they should be written at the header level.", 0, dataProvider.GetTransportInformation(bill).Count);

		var bill2 = header.Bills.AddNew();
		bill2.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._41;
		bill.TransportAtDeparture = "Air-N Schelde";
		bill.TransportCountryAtDeparture = "IT";

		var transportInformationCollection = dataProvider.GetTransportInformation(bill);

		AssertEquals("When the Transport Departure fields of House Consignments differ, they should be written at the line level.", 1, transportInformationCollection.Count);
		AssertEquals("TypeOfIdentification at line level", 40, transportInformationCollection.First().TypeOfIdentification);
	}

	public void TestGetMethodOfPaymentForConsignmentItem()
	{
		using (SetTransitionPeriod(true))
		{
			AssertConsignmentItemFieldWithSingleLine(
				"MethodOfPayment",
				(goodsItem, value) => { goodsItem.BY_TransportChargesMethodOfPayment = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(consignmentItem) => dataProvider.GetMethodOfPayment(consignmentItem));

			AssertConsignmentItemFieldWithMultiLine(
				"MethodOfPayment",
				(goodsItem, value) => { goodsItem.BY_TransportChargesMethodOfPayment = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(consignmentItem) => dataProvider.GetMethodOfPayment(consignmentItem));
		}

		using (SetTransitionPeriod(false))
		{
			AssertBillConsignmentItemFieldWithSingleLine(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(consignmentItem) => dataProvider.GetMethodOfPayment(consignmentItem));

			AssertBillConsignmentItemFieldWithMultiLine(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(consignmentItem) => dataProvider.GetMethodOfPayment(consignmentItem));
		}
	}

	public void TestGetReferenceNumber()
	{
		var header = Factory.NewDepartureNctsHeader();
		var supportingDocument = header.MovementHeader.SupportingDocuments.AddNew();
		AssertEquals("Empty", "-", dataProvider.GetReferenceNumber(supportingDocument));

		supportingDocument.CSI_ReferenceNumber = "SUPDOC001";
		AssertEquals("CSI_ReferenceNumber", "SUPDOC001", dataProvider.GetReferenceNumber(supportingDocument));

		supportingDocument.CSI_YearOfIssue = "2024";
		supportingDocument.CSI_RN_NKCountryCode = "IT";

		AssertEquals("CSI_ReferenceNumber", "2024-IT-SUPDOC001", dataProvider.GetReferenceNumber(supportingDocument));
	}

	public void TestGetConsigneeForHeader()
	{
		var consignee = new Mock<ITrader>();
		consignee.Setup(x => x.IdentificationNumber).Returns("     IT385040449  ");
		nctsHeaderWrapperMock.Setup(x => x.GetConsignee()).Returns(consignee.Object);

		var consigneeProvider = dataProvider.GetConsignee(CreateHeader());
		AssertNotNull(consigneeProvider);
		AssertEquals("IT385040449", consigneeProvider.IdentificationNumber);
	}

	public void TestGetConsigneeForBill()
	{
		var consignee = new Mock<ITrader>();
		consignee.Setup(x => x.IdentificationNumber).Returns("    IT385040449   ");
		houseConsignmentWrapperMock.Setup(x => x.Consignee).Returns(consignee.Object);

		var header = CreateHeader();
		var bill = header.Bills.AddNew();
		var consigneeProvider = dataProvider.GetConsignee(bill);

		AssertNotNull(consigneeProvider);
		AssertEquals("IT385040449", consigneeProvider.IdentificationNumber);
	}

	public void TestConsigneeTypeDataProviderProperties()
	{
		houseConsignmentWrapperMock.Setup(x => x.Consignee).Returns(null as ITrader);
		AssertNull("Consignee", dataProvider.GetConsignee(CreateHeader()));

		var consignee = new Mock<ITrader>();
		consignee.Setup(x => x.IdentificationNumber).Returns("IT385040449");
		consignee.Setup(x => x.Address).Returns(null as IAddress);
		nctsHeaderWrapperMock.Setup(x => x.GetConsignee()).Returns(consignee.Object);
		var consigneeProvider = dataProvider.GetConsignee(CreateHeader());

		CombineAssertions("Consignee with no address", () =>
		{
			AssertNotNull("Consignee", consigneeProvider);
			AssertEquals("IdentificationNumber", "IT385040449", consigneeProvider.IdentificationNumber);
			AssertNull("Name", consigneeProvider.Name);
			AssertNull("Address", consigneeProvider.Address);
		});

		var address = new Mock<IAddress>();
		address.Setup(x => x.City).Returns("Test City");
		address.Setup(x => x.Country).Returns("Test Country");
		address.Setup(x => x.Name).Returns("Test Name");
		address.Setup(x => x.StreetAndNumber).Returns("Test StreetAndNumber");
		address.Setup(x => x.ZipCode).Returns("Test ZipCode");
		consignee.Setup(x => x.Address).Returns(address.Object);
		consigneeProvider = dataProvider.GetConsignee(CreateHeader());

		CombineAssertions("Consignee with address", () =>
		{
			AssertNotNull("Consignee", consigneeProvider);
			AssertEquals("IdentificationNumber", "IT385040449", consigneeProvider.IdentificationNumber);
			AssertEquals("Name", "Test Name", consigneeProvider.Name);

			AssertNotNull("Address", consigneeProvider.Address);
			AssertEquals("Address StreetAndNumber", "Test StreetAndNumber", consigneeProvider.Address.StreetAndNumber);
			AssertEquals("Address Country", "Test Country", consigneeProvider.Address.Country);
			AssertEquals("Address Postcode", "Test ZipCode", consigneeProvider.Address.Postcode);
			AssertEquals("Address City", "Test City", consigneeProvider.Address.City);
		});
	}

	public void TestUcr()
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();

		var b1 = header.Bills.AddNew();
		var b1gi1 = b1.GoodsItems.AddNew();
		var b1gi2 = b1.GoodsItems.AddNew();

		var b2 = header.Bills.AddNew();
		var b2gi1 = b2.GoodsItems.AddNew();
		var b2gi2 = b2.GoodsItems.AddNew();

		header.MovementHeader.BM_UniqueConsignmentReference = "PL";

		b1.B0_ReferenceID = "ES";
		b1gi1.BY_CommercialReferenceNumber = "PL";
		b1gi2.BY_CommercialReferenceNumber = "ES";

		b2.B0_ReferenceID = "PL";
		b2gi1.BY_CommercialReferenceNumber = "IT";
		b2gi2.BY_CommercialReferenceNumber = "IT";

		using (SetTransitionPeriod(false))
		{
			CombineAssertions("Transition Period False", () =>
			{
				AssertEquals(string.Empty, dataProvider.GetReferenceNumberUCR(header));

				AssertEquals(string.Empty, dataProvider.GetReferenceNumberUCR(b1));
				AssertEquals("IT", dataProvider.GetReferenceNumberUCR(b2));

				AssertEquals("PL", dataProvider.GetReferenceNumberUCR(b1gi1));
				AssertEquals("ES", dataProvider.GetReferenceNumberUCR(b1gi2));
				AssertEquals(string.Empty, dataProvider.GetReferenceNumberUCR(b2gi1));
				AssertEquals(string.Empty, dataProvider.GetReferenceNumberUCR(b2gi2));
			});
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("Transition Period True", () =>
			{
				AssertEquals("PL", dataProvider.GetReferenceNumberUCR(header));

				AssertEquals("ES", dataProvider.GetReferenceNumberUCR(b1));
				AssertEquals("PL", dataProvider.GetReferenceNumberUCR(b2));

				AssertEquals("PL", dataProvider.GetReferenceNumberUCR(b1gi1));
				AssertEquals("ES", dataProvider.GetReferenceNumberUCR(b1gi2));
				AssertEquals("IT", dataProvider.GetReferenceNumberUCR(b2gi1));
				AssertEquals("IT", dataProvider.GetReferenceNumberUCR(b2gi2));
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		movementHeaderCusMessageWrapperMock = new Mock<INctsDepartureMovementHeaderWrapper>();
		houseConsignmentWrapperMock = new Mock<IHouseConsignmentCustomsMessageWrapper>();
		nctsHeaderWrapperMock = new Mock<INctsHeaderWrapper>();
		wrapperProvider = Mock.Of<INctsEntityWrapperProvider>(
			p => p.GetNctsDepartureMovementHeaderWrapper(It.IsAny<NctsDepartureMovementHeader>()) == movementHeaderCusMessageWrapperMock.Object
			&& p.GetNctsHeaderWrapper(It.IsAny<NctsHeader>()) == nctsHeaderWrapperMock.Object
			&& p.GetHouseConsignmentWrapper(It.IsAny<NctsBill>()) == houseConsignmentWrapperMock.Object);
		dataProvider = new D2AdditionalDataProvider(wrapperProvider, false);
	}

	NctsDepartureMovementHeader CreateMovementHeader() => Factory.New<NctsDepartureMovementHeader>();

	NctsHeader CreateHeader() => Factory.New<NctsHeader>();

	void AssertConsignmentHeaderFieldWithSingleLine(
		string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsHeader, string> getConsignmentFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		AssertNullOrEmpty($"When no header or lines are found, {fieldName}", getConsignmentFieldValue(header));

		var movementHeader = header.MovementHeader;
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(fieldName, () =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(goodsItem, string.Empty);
			AssertEquals($"When {fieldName} is only provided at header level and not at line", "X", getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem, "X");
			AssertEquals($"When line and header  have same {fieldName}", "X", getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem, "Y");
			AssertEquals($"When line has {fieldName} different from header", "Y", getConsignmentFieldValue(header));

			setHeaderTo(movementHeader, string.Empty);
			AssertEquals($"When line has {fieldName} and header is empty", "Y", getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem, string.Empty);
			AssertEquals($"When both line and header don't have value", string.Empty, getConsignmentFieldValue(header));
		});
	}

	void AssertBillConsignmentItemFieldWithMultiLine(
		string fieldName,
		Action<NctsBill, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsBill, string> getConsignmentItemFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		var movementHeader = header.MovementHeader;
		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, string.Empty);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "X", getConsignmentItemFieldValue(bill1));
			AssertEquals($"When {fieldName} is only available in one line, in line 2", string.Empty, getConsignmentItemFieldValue(bill2));

			setLineFieldTo(bill1, "Y");
			setLineFieldTo(bill2, "Y");
			AssertEquals($"When all lines have same {fieldName}", string.Empty, getConsignmentItemFieldValue(bill1));
			AssertEquals($"When all lines have same {fieldName}", string.Empty, getConsignmentItemFieldValue(bill2));

			setLineFieldTo(bill2, "Z");
			AssertEquals($"When lines have different {fieldName}", "Y", getConsignmentItemFieldValue(bill1));
			AssertEquals($"When lines have different {fieldName}", "Z", getConsignmentItemFieldValue(bill2));

			setLineFieldTo(bill2, string.Empty);
			AssertEquals($"When lines have different {fieldName} (one does not have value)", "Y", getConsignmentItemFieldValue(bill1));
			AssertEquals($"When lines have different {fieldName} (one does not have value)", string.Empty, getConsignmentItemFieldValue(bill2));

			setLineFieldTo(bill1, string.Empty);
			AssertEquals($"When lines have same {fieldName} (all do not have value)", string.Empty, getConsignmentItemFieldValue(bill1));
			AssertEquals($"When lines have same {fieldName} (all do not have value)", string.Empty, getConsignmentItemFieldValue(bill2));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "Y");
			setLineFieldTo(bill2, "Y");
			AssertEquals($"When {fieldName} is the same in lines and header", string.Empty, getConsignmentItemFieldValue(bill1));
			AssertEquals($"When {fieldName} is the same in lines and header", string.Empty, getConsignmentItemFieldValue(bill2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, "Z");
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases", "X", getConsignmentItemFieldValue(bill1));
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases", "Z", getConsignmentItemFieldValue(bill2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, "X");
			setLineFieldTo(bill2, string.Empty);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere else", "X", getConsignmentItemFieldValue(bill1));
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere else", "Y", getConsignmentItemFieldValue(bill2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(bill1, string.Empty);
			setLineFieldTo(bill2, string.Empty);
			AssertEquals($"When {fieldName} is only available at header level", string.Empty, getConsignmentItemFieldValue(bill1));
			AssertEquals($"When {fieldName} is only available at header level", string.Empty, getConsignmentItemFieldValue(bill2));
		});
	}

	void AssertBillConsignmentItemFieldWithSingleLine(
		string fieldName,
		Action<NctsBill, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsBill, string> getConsignmentItemFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		var movementHeader = header.MovementHeader;
		var bill = header.Bills.AddNew();

		AssertNullOrEmpty($"When no header or lines are found, {fieldName}", getConsignmentItemFieldValue(bill));

		CombineAssertions(() =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(bill, string.Empty);
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line", string.Empty, getConsignmentItemFieldValue(bill));

			setLineFieldTo(bill, "X");
			AssertEquals($"When invoice line and Declaration have same {fieldName}", string.Empty, getConsignmentItemFieldValue(bill));

			setLineFieldTo(bill, "Y");
			AssertEquals($"When invoice line has {fieldName} different from declaration", string.Empty, getConsignmentItemFieldValue(bill));

			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(bill, "Y");
			AssertEquals($"When invoice line has {fieldName} and declaration is empty", string.Empty, getConsignmentItemFieldValue(bill));

			setLineFieldTo(bill, string.Empty);
			AssertEquals($"When both line and declaration don't have {fieldName}", string.Empty, getConsignmentItemFieldValue(bill));
		});
	}

	void AssertConsignmentHeaderFieldWithMultiLines(string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsHeader, string> getConsignmentFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		AssertNullOrEmpty($"When no header or lines are found, {fieldName}", getConsignmentFieldValue(header));

		var movementHeader = header.MovementHeader;
		var bill = header.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		var goodsItem2 = bill.GoodsItems.AddNew();

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is only available in one line", string.Empty, getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			AssertEquals($"When all lines have same {fieldName}", "Y", getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem1, "Z");
			AssertEquals($"When lines have different {fieldName}", string.Empty, getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When lines have different {fieldName} (one does not have value)", string.Empty, getConsignmentFieldValue(header));

			setLineFieldTo(goodsItem1, string.Empty);
			AssertEquals($"When lines have same {fieldName} (all do not have value)", string.Empty, getConsignmentFieldValue(header));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			AssertEquals($"When {fieldName} is the same in lines and header", "Y", getConsignmentFieldValue(header));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, "Z");
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases", string.Empty, getConsignmentFieldValue(header));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere", string.Empty, getConsignmentFieldValue(header));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, string.Empty);
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is only available at header level", "Y", getConsignmentFieldValue(header));
		});
	}

	void AssertConsignmentItemFieldWithSingleLine(string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsDepartureCargoDesc, string> getConsignmentItemFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		var movementHeader = header.MovementHeader;
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(goodsItem, string.Empty);
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line", string.Empty, getConsignmentItemFieldValue(goodsItem));

			setLineFieldTo(goodsItem, "X");
			AssertEquals($"When invoice line and Declaration have same {fieldName}", string.Empty, getConsignmentItemFieldValue(goodsItem));

			setLineFieldTo(goodsItem, "Y");
			AssertEquals($"When invoice line has {fieldName} different from declaration", string.Empty, getConsignmentItemFieldValue(goodsItem));

			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(goodsItem, "Y");
			AssertEquals($"When invoice line has {fieldName} and declaration is empty", string.Empty, getConsignmentItemFieldValue(goodsItem));

			setLineFieldTo(goodsItem, string.Empty);
			AssertEquals($"When both line and declaration don't have {fieldName}", string.Empty, getConsignmentItemFieldValue(goodsItem));
		});
	}

	void AssertConsignmentItemFieldWithMultiLine(string fieldName,
		Action<NctsDepartureCargoDesc, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<NctsDepartureCargoDesc, string> getConsignmentItemFieldValue)
	{
		var header = Factory.NewDepartureNctsHeader();
		var movementHeader = header.MovementHeader;
		var bill = header.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		var goodsItem2 = bill.GoodsItems.AddNew();

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is only available in one line, in line 1", "X", getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When {fieldName} is only available in one line, in line 2", string.Empty, getConsignmentItemFieldValue(goodsItem2));

			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			AssertEquals($"When all lines have same {fieldName}", string.Empty, getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When all lines have same {fieldName}", string.Empty, getConsignmentItemFieldValue(goodsItem2));

			setLineFieldTo(goodsItem2, "Z");
			AssertEquals($"When lines have different {fieldName}", "Y", getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When lines have different {fieldName}", "Z", getConsignmentItemFieldValue(goodsItem2));

			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When lines have different {fieldName} (one does not have value)", "Y", getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When lines have different {fieldName} (one does not have value)", string.Empty, getConsignmentItemFieldValue(goodsItem2));

			setLineFieldTo(goodsItem1, string.Empty);
			AssertEquals($"When lines have same {fieldName} (all do not have value)", string.Empty, getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When lines have same {fieldName} (all do not have value)", string.Empty, getConsignmentItemFieldValue(goodsItem2));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "Y");
			setLineFieldTo(goodsItem2, "Y");
			AssertEquals($"When {fieldName} is the same in lines and header", string.Empty, getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When {fieldName} is the same in lines and header", string.Empty, getConsignmentItemFieldValue(goodsItem2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, "Z");
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases", "X", getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases", "Z", getConsignmentItemFieldValue(goodsItem2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, "X");
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere else", "X", getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere else", "Y", getConsignmentItemFieldValue(goodsItem2));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(goodsItem1, string.Empty);
			setLineFieldTo(goodsItem2, string.Empty);
			AssertEquals($"When {fieldName} is only available at header level", string.Empty, getConsignmentItemFieldValue(goodsItem1));
			AssertEquals($"When {fieldName} is only available at header level", string.Empty, getConsignmentItemFieldValue(goodsItem2));
		});
	}

	IDeclarationD2AdditionalDataProvider dataProvider;
	INctsEntityWrapperProvider wrapperProvider;
	Mock<INctsDepartureMovementHeaderWrapper> movementHeaderCusMessageWrapperMock;
	Mock<INctsHeaderWrapper> nctsHeaderWrapperMock;
	Mock<IHouseConsignmentCustomsMessageWrapper> houseConsignmentWrapperMock;

	IDisposable SetTransitionPeriod(bool isActive) =>
		ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);
}
