using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Moq;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class D1ConsignmentWrapperTest : D1ConsignmentWrapperTestBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => new D1ConsignmentWrapper(null, messageSendingObjectFactory.Object));
		AssertExceptionThrown<ArgumentNullException>("When messageSendingObjectFactory is null", () => new D1ConsignmentWrapper(Factory.New<NctsHeader>(), null));
		AssertExceptionThrown<ArgumentNullException>("When header without MovementHeader", () => new D1ConsignmentWrapper(Factory.New<NctsHeader>(), messageSendingObjectFactory.Object));
		AssertNoExceptionThrown(() => new D1ConsignmentWrapper(header, messageSendingObjectFactory.Object));
	}

	public override void TestActiveBorderTransportMeans()
	{
		var wrapper = CreateWrapper();
		var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
		AssertNotNull(nameof(ID1Consignment.ActiveBorderTransportMeans), activeBorderTransportMeans);
		AssertEquals($"{nameof(ID1Consignment.ActiveBorderTransportMeans)} Count", 0, activeBorderTransportMeans.Count);

		var borderTransportMeans = movementHeader.AdditionalTransportAtBorderList.AddNew();
		CombineAssertions("When all involved [MovementHeader, BorderTransportMeans] fields are empty", () =>
		{
			wrapper = CreateWrapper();
			activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;

			AssertEquals($"{nameof(ID1Consignment.ActiveBorderTransportMeans)} Count", 0, activeBorderTransportMeans.Count);
		});

		CombineAssertions("When all involved [BorderTransportMeans] fields are empty", () =>
		{
			movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			wrapper = CreateWrapper();
			activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
			AssertEquals($"{nameof(ID1Consignment.ActiveBorderTransportMeans)} Count", 1, activeBorderTransportMeans.Count);
			AssertType<ActiveBorderMeansOfTransportWrapper>(wrapper.ActiveBorderTransportMeans.Single());
		});

		CombineAssertions("When all involved [MovementHeader, BorderTransportMeans] fields are not empty", () =>
		{
			borderTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
			wrapper = CreateWrapper();
			activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
			AssertEquals($"{nameof(ID1Consignment.ActiveBorderTransportMeans)} Count", 2, activeBorderTransportMeans.Count);
		});
	}

	public override void TestAdditionalDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.AdditionalDeclarationType), wrapper.AdditionalDeclarationType);

		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.AdditionalDeclarationType), NctsTypeOfAdditionalDeclarationList.Codes.A, wrapper.AdditionalDeclarationType);
	}

	public override void TestAmendment()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.Amendment), wrapper.Amendment);

		var amendmentMock = new Mock<INctsAmendment>();
		wrapper = new D1ConsignmentWrapper(header, messageSendingObjectFactory.Object, amendmentMock.Object);
		AssertSame(nameof(ID1Consignment.Amendment), amendmentMock.Object, wrapper.Amendment);
	}

	public override void TestAdditionalInformation()
	{
		var wrapper = CreateWrapper();
		var additionalInformation = wrapper.AdditionalInformation;
		AssertNotNull(nameof(ID1Consignment.AdditionalInformation), additionalInformation);
		AssertEquals($"{nameof(ID1Consignment.AdditionalInformation)} Count", 0, additionalInformation.Count);

		var additionalDocument = header.AdditionalDocuments.AddNew();
		CombineAssertions("When type is not INF", () =>
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			wrapper = CreateWrapper();
			additionalInformation = wrapper.AdditionalInformation;

			AssertNotNull(nameof(ID1Consignment.AdditionalReferences), additionalInformation);
			AssertEquals($"{nameof(ID1Consignment.AdditionalInformation)} Count", 0, additionalInformation.Count);
		});

		CombineAssertions("When type is INF", () =>
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			wrapper = CreateWrapper();
			additionalInformation = wrapper.AdditionalInformation;

			AssertNotNull(nameof(ID1Consignment.AdditionalInformation), additionalInformation);
			AssertEquals($"{nameof(ID1Consignment.AdditionalInformation)} Count", 1, additionalInformation.Count);
			AssertType<AdditionalInformationWrapper>(additionalInformation.Single());
		});
	}

	public override void TestAdditionalReferences()
	{
		var wrapper = CreateWrapper();
		var additionalReferences = wrapper.AdditionalReferences;
		AssertNotNull(nameof(ID1Consignment.AdditionalReferences), additionalReferences);
		AssertEquals($"{nameof(ID1Consignment.AdditionalReferences)} Count", 0, additionalReferences.Count);
		var additionalDocument = header.AdditionalDocuments.AddNew();

		CombineAssertions("When type is not REF", () =>
		{
			wrapper = CreateWrapper();
			AssertNotNull(nameof(ID1Consignment.AdditionalReferences), additionalReferences);
			AssertEquals(nameof(ID1Consignment.AdditionalReferences), 0, additionalReferences.Count);
		});

		CombineAssertions("When type is REF", () =>
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			wrapper = CreateWrapper();

			additionalReferences = wrapper.AdditionalReferences;
			AssertNotNull(nameof(ID1Consignment.AdditionalReferences), additionalReferences);
			AssertEquals(nameof(ID1Consignment.AdditionalReferences), 1, additionalReferences.Count);
			AssertSame(nameof(ID1Consignment.AdditionalReferences), additionalReferences, wrapper.AdditionalReferences);
			AssertType<AdditionalReferenceWrapper>(additionalReferences.Single());
		});
	}

	public override void TestAdditionalSupplyChainActors()
	{
		var wrapper = CreateWrapper();
		var additionalSupplyChainActors = wrapper.AdditionalSupplyChainActors;
		AssertNotNull(nameof(ID1Consignment.AdditionalSupplyChainActors), additionalSupplyChainActors);
		AssertEquals($"{nameof(ID1Consignment.AdditionalSupplyChainActors)} Count", 0, additionalSupplyChainActors.Count);

		var cusSupplyChainActor = header.MovementHeader.CusSupplyChainActors.AddNew();
		CombineAssertions("When type is not SCA", () =>
		{
			wrapper = CreateWrapper();
			AssertEquals(nameof(ID1Consignment.AdditionalSupplyChainActors), 0, additionalSupplyChainActors.Count);
		});

		CombineAssertions("When type is SCA", () =>
		{
			cusSupplyChainActor.CFR_Type = CusReferenceTypeList.Codes.SupplyChainActor;
			cusSupplyChainActor.CFR_Code = "FW";
			cusSupplyChainActor.CFR_Reference = "IT02028530281";
			wrapper = CreateWrapper();
			additionalSupplyChainActors = wrapper.AdditionalSupplyChainActors;

			AssertNotNull(nameof(ID1Consignment.AdditionalSupplyChainActors), additionalSupplyChainActors);
			AssertEquals(nameof(ID1Consignment.AdditionalSupplyChainActors), 1, additionalSupplyChainActors.Count);
			AssertSame(nameof(ID1Consignment.AdditionalSupplyChainActors), additionalSupplyChainActors, wrapper.AdditionalSupplyChainActors);
			AssertType<AdditionalSupplyChainActorWrapper>(additionalSupplyChainActors.Single());
		});
	}

	public override void TestAuthorizations()
	{
		var wrapper = CreateWrapper();
		var authorizations = wrapper.Authorizations;
		AssertNotNull(nameof(ID1Consignment.Authorizations), authorizations);
		AssertEquals($"{nameof(ID1Consignment.Authorizations)} Count", 0, authorizations.Count);

		var cusAuthorizationUsage = header.MovementHeader.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		cusAuthorizationUsage.AGC_Number = "456";

		wrapper = CreateWrapper();
		authorizations = wrapper.Authorizations;
		AssertNotNull(nameof(ID1Consignment.Authorizations), authorizations);
		AssertEquals($"{nameof(ID1Consignment.Authorizations)} Count", 1, authorizations.Count);
		AssertSame(nameof(ID1Consignment.Authorizations), authorizations, wrapper.Authorizations);
		AssertType<CustomsCodeAuthorizationWrapper>(authorizations.Single());
	}

	public override void TestBorderMeansOfTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.BorderMeansOfTransportMode), wrapper.BorderMeansOfTransportMode);

		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.BorderMeansOfTransportMode), 1, wrapper.BorderMeansOfTransportMode);
	}

	public override void TestCarrier()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.Carrier), wrapper.Carrier);

		var carrierHeader = Factory.New<OrgHeader>();
		carrierHeader.CustomsCodes.AddNew("EOR", "385040449", "IT");
		movementHeader.Carrier.E2_OA_Address = carrierHeader.MainAddress.PK;

		wrapper = CreateWrapper();
		AssertType<CarrierWrapper>(nameof(ID1Consignment.Carrier), wrapper.Carrier);
	}

	public override void TestConsignee()
	{
		var wrapper = CreateWrapper();
		var consignee = wrapper.Consignee;
		AssertNull(nameof(ID1Consignment.Consignee), consignee);

		var org = Factory.New<OrgHeader>();
		org.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = org.Addresses.AddNew();
		header.Consignee.E2_OA_Address = address.PK;
		nctsHeaderWrapper.Setup(x => x.GetConsignee()).Returns(new EoriOrTcuTraderWrapper(header.Consignee));
		wrapper = CreateWrapper();
		consignee = wrapper.Consignee;

		AssertNotNull(nameof(ID1Consignment.Consignee), consignee);
		AssertType<EoriOrTcuTraderWrapper>(consignee);
		AssertEquals("IT385040449", consignee.IdentificationNumber);
	}

	public override void TestConsignmentRoutings()
	{
		var wrapper = CreateWrapper();
		var consignmentRoutings = wrapper.ConsignmentRoutings;
		AssertNotNull(nameof(ID1Consignment.ConsignmentRoutings), consignmentRoutings);
		AssertEquals($"{nameof(ID1Consignment.ConsignmentRoutings)} Count", 0, consignmentRoutings.Count);

		header.CountriesOfRouting.AddNew().CY_Data = "IT";
		header.CountriesOfRouting.AddNew().CY_Data = "IL";
		header.CountriesOfRouting.AddNew();

		wrapper = CreateWrapper();
		consignmentRoutings = wrapper.ConsignmentRoutings;

		CombineAssertions("Automatic SequenceNumber", () =>
		{
			AssertNotNull(nameof(ID1Consignment.ConsignmentRoutings), consignmentRoutings);
			AssertEquals($"{nameof(ID1Consignment.ConsignmentRoutings)} Count", 2, consignmentRoutings.Count);
			AssertSame(nameof(ID1Consignment.Authorizations), consignmentRoutings, wrapper.ConsignmentRoutings);
			var countriesOfRouting = consignmentRoutings.ToArray();
			AssertEquals("SequenceNumber", 1, countriesOfRouting[0].SequenceNumber);
			AssertEquals("CountryCode", "IT", countriesOfRouting[0].CountryCode);

			AssertEquals("SequenceNumber", 2, countriesOfRouting[1].SequenceNumber);
			AssertEquals("CountryCode", "IL", countriesOfRouting[1].CountryCode);
			AssertType<ConsignmentCountryRoutingWrapper>(countriesOfRouting[1]);
		});
	}

	public override void TestConsignor()
	{
		var wrapper = CreateWrapper();
		var consignor = wrapper.Consignor;
		AssertNull(nameof(ID1Consignment.Consignor), consignor);

		var supplier = Factory.New<OrgHeader>();
		supplier.CustomsCodes.AddNew("EOR", "385040451", "IT");
		var address = supplier.Addresses.AddNew();
		header.Consignor.E2_OA_Address = address.PK;
		wrapper = CreateWrapper();
		consignor = wrapper.Consignor;

		AssertNotNull(nameof(ID1Consignment.Consignor), consignor);
		AssertType<EoriOrTcuTraderWrapper>(consignor);
		AssertSame(nameof(ID1Consignment.Consignor), consignor, wrapper.Consignor);
		AssertEquals("IT385040451", consignor.IdentificationNumber);
	}

	public override void TestCountryOfDestination()
	{
		using (SetTransitionPeriod(false))
		{
			var bill1 = header.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			var goodItem12 = bill1.GoodsItems.AddNew();
			var bill2 = header.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();

			var wrapper = CreateWrapper();
			AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDestination);

			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			wrapper = CreateWrapper();
			AssertEquals("Consignment: IT, HouseConsignment and GoodsItem: all empty", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDestination);

			bill1.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			wrapper = CreateWrapper();
			AssertNullOrEmpty("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, empty, GoodsItem: all empty", wrapper.CountryOfDestination);

			bill2.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			wrapper = CreateWrapper();
			AssertEquals("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDestination);

			goodItem11.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;
			goodItem12.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			goodItem21.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			wrapper = CreateWrapper();
			AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, PL", wrapper.CountryOfDestination);

			goodItem11.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Poland;
			wrapper = CreateWrapper();
			AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, PL", Core.Constants.CountryCodes.Poland, wrapper.CountryOfDestination);
		}

		using (SetTransitionPeriod(true))
		{
			AssertWrapperFieldWithSingleLine<NctsDepartureCargoDesc>(
			"CountryOfDestination",
			(goodsItem, value) => { goodsItem.BY_RN_NKCountryOfDestination = value; },
			(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
			wrapper => wrapper.CountryOfDestination);

			AssertWrapperFieldWithMultiLines<NctsDepartureCargoDesc>(
				"CountryOfDestination",
				(line, value) => { line.BY_RN_NKCountryOfDestination = value; },
				(movementHeader, value) => { movementHeader.BM_RL_NKDestinationPort = value; },
				(wrapper) => wrapper.CountryOfDestination);
		}
	}

	public override void TestCountryOfDispatch()
	{
		var bill1 = header.Bills.AddNew();
		var goodItem11 = bill1.GoodsItems.AddNew();
		var goodItem12 = bill1.GoodsItems.AddNew();
		var bill2 = header.Bills.AddNew();
		var goodItem21 = bill2.GoodsItems.AddNew();

		var wrapper = CreateWrapper();
		AssertNullOrEmpty("Consignment, HouseConsignment and GoodsItem: all empty", wrapper.CountryOfDispatch);

		movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		wrapper = CreateWrapper();
		AssertEquals("Consignment: IT, HouseConsignment and GoodsItem: all empty", Core.Constants.CountryCodes.Italy, wrapper.CountryOfDispatch);

		bill1.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Spain;
		wrapper = CreateWrapper();
		AssertNullOrEmpty("Different at HouseConsignment level, Consignment: IT, HouseConsignment: ES, empty, GoodsItem: all empty", wrapper.CountryOfDispatch);

		bill2.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.Spain;
		wrapper = CreateWrapper();
		AssertEquals("Same at HouseConsignment level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: all empty", Core.Constants.CountryCodes.Spain, wrapper.CountryOfDispatch);

		goodItem11.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Spain;
		goodItem12.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		goodItem21.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		wrapper = CreateWrapper();
		AssertNullOrEmpty("Different at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: ES, PL, PL", wrapper.CountryOfDispatch);

		goodItem11.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Poland;
		wrapper = CreateWrapper();
		AssertEquals("Same at GoodsItem level, Consignment: IT, HouseConsignment: ES, ES, GoodsItem: PL, PL, PL", Core.Constants.CountryCodes.Poland, wrapper.CountryOfDispatch);
	}

	public override void TestCustomsOfficesOfExitTransit()
	{
		var wrapper = CreateWrapper();
		var customsOfficesOfExitTransit = wrapper.CustomsOfficesOfExitTransit;
		AssertNotNull(nameof(ID1Consignment.CustomsOfficesOfExitTransit), customsOfficesOfExitTransit);
		AssertEquals($"{nameof(ID1Consignment.CustomsOfficesOfExitTransit)} Count", 0, customsOfficesOfExitTransit.Count);

		var customsOffices = header.MovementHeader.CustomsOffices;
		customsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).CY_Data = "IT1234";
		customsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).CY_Data = "IE1234";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IT2345";

		wrapper = CreateWrapper();
		customsOfficesOfExitTransit = wrapper.CustomsOfficesOfExitTransit;
		AssertNotNull(nameof(ID1Consignment.CustomsOfficesOfExitTransit), customsOfficesOfExitTransit);
		AssertEquals($"{nameof(ID1Consignment.CustomsOfficesOfExitTransit)} Count", 2, customsOfficesOfExitTransit.Count);

		AssertSequencesEqual(new[] { "IT1234", "IE1234" }, customsOfficesOfExitTransit);
	}

	public override void TestCustomsOfficesOfTransit()
	{
		var wrapper = CreateWrapper();
		var customsOfficesOfExitTransit = wrapper.CustomsOfficesOfTransit;
		AssertNotNull(nameof(ID1Consignment.CustomsOfficesOfTransit), customsOfficesOfExitTransit);
		AssertEquals($"{nameof(ID1Consignment.CustomsOfficesOfTransit)} Count", 0, customsOfficesOfExitTransit.Count);

		var customsOffices = header.MovementHeader.CustomsOffices;
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IT2345";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IE2345";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance).CY_Data = "BB1234";

		wrapper = CreateWrapper();
		customsOfficesOfExitTransit = wrapper.CustomsOfficesOfTransit;
		AssertNotNull(nameof(ID1Consignment.CustomsOfficesOfTransit), customsOfficesOfExitTransit);
		AssertEquals($"{nameof(ID1Consignment.CustomsOfficesOfTransit)} Count", 2, customsOfficesOfExitTransit.Count);
		AssertType<CustomsOfficeOfTransitWrapper>(customsOfficesOfExitTransit.First());
	}

	public override void TestDeclarationCustomsOffice()
	{
		var customsOffices = header.MovementHeader.CustomsOffices;
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IT2345";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance).CY_Data = "BB1234";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.DeclarationCustomsOffice), wrapper.DeclarationCustomsOffice);

		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "IT898989";
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.DeclarationCustomsOffice), "898989", wrapper.DeclarationCustomsOffice);
	}

	public override void TestDeclarationType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.DeclarationType), wrapper.DeclarationType);

		movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.DeclarationType), NctsDeclarationTypeList.Codes.TIR, wrapper.DeclarationType);
	}

	public override void TestDepartureCustomsOffice()
	{
		var customsOffices = header.MovementHeader.CustomsOffices;
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IT2345";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance).CY_Data = "BB1234";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.DepartureCustomsOffice), wrapper.DepartureCustomsOffice);

		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "IT5678";
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.DepartureCustomsOffice), "IT5678", wrapper.DepartureCustomsOffice);
	}

	public override void TestDepartureMeansOfTransports()
	{
		var wrapper = CreateWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertNotNull(nameof(ID1Consignment.DepartureMeansOfTransports), departureMeansOfTransports);
		AssertEquals($"{nameof(ID1Consignment.DepartureMeansOfTransports)}", 0, departureMeansOfTransports.Count);

		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		movementHeader.BM_TransportAtDepartureType = NctsTransportTypeOfIdList.Codes._11;
		movementHeader.BM_TransportAtDeparture = "Sea-N Schelde";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = "IT";
		wrapper = CreateWrapper();

		departureMeansOfTransports = wrapper.DepartureMeansOfTransports;
		AssertEquals($"{nameof(ID1Consignment.DepartureMeansOfTransports)}", 1, departureMeansOfTransports.Count);
		AssertSame(departureMeansOfTransports, wrapper.DepartureMeansOfTransports);
		AssertType<DepartureMeansOfTransportWrapper>(departureMeansOfTransports.Single());
	}

	public void TestDepartureMeansOfTransports_SameAtHouseConsignmentLevel()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

		var bills = header.Bills;
		var houseConsignment1 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment1);
		var houseConsignment2 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment2);

		var wrapper = CreateWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		CombineAssertions("When all Transport Departure fields of House Consignments are the same, they should be written at the header level.", () =>
		{
			AssertEquals(nameof(departureMeansOfTransports.Count), 1, departureMeansOfTransports.Count);
			var departureMeansOfTransport = departureMeansOfTransports.First();
			AssertEquals(nameof(departureMeansOfTransport.TypeOfIdentification), 11, departureMeansOfTransport.TypeOfIdentification);
			AssertEquals(nameof(departureMeansOfTransport.IdentificationNumber), "9222091", departureMeansOfTransport.IdentificationNumber);
			AssertEquals(nameof(departureMeansOfTransport.Nationality), "IT", departureMeansOfTransport.Nationality);
		});

		static void SetUpHouseConsignmentTransportDeparture(NctsBill houseConsignment)
		{
			houseConsignment.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
			houseConsignment.VesselNameAtDeparture = "9222091";
			houseConsignment.VesselCountryAtDeparture = "IT";
		}
	}

	public void TestDepartureMeansOfTransports_DifferentAtHouseConsignmentLevel()
	{
		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;

		var bills = header.Bills;
		var houseConsignment1 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment1, NctsTransportTypeOfIdList.Codes._10, "1111117", "IT");
		var houseConsignment2 = bills.AddNew();
		SetUpHouseConsignmentTransportDeparture(houseConsignment2, NctsTransportTypeOfIdList.Codes._11, "9222091", "IT");

		var wrapper = CreateWrapper();
		var departureMeansOfTransports = wrapper.DepartureMeansOfTransports;

		AssertEquals("When the Transport Departure fields of House Consignments differ, they should be written at the line level.", 0, departureMeansOfTransports.Count);

		static void SetUpHouseConsignmentTransportDeparture(NctsBill houseConsignment, ZString typeOfIdentification, ZString identificationNumber, ZString transportNationality)
		{
			houseConsignment.TransportTypeAtDeparture = typeOfIdentification;
			houseConsignment.VesselNameAtDeparture = identificationNumber;
			houseConsignment.VesselCountryAtDeparture = transportNationality;
		}
	}

	public override void TestDestinationCustomsOffice()
	{
		var customsOffices = header.MovementHeader.CustomsOffices;
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit).CY_Data = "IT2345";
		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance).CY_Data = "BB1234";
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.DestinationCustomsOffice), wrapper.DestinationCustomsOffice);

		customsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "IE9876";
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.DestinationCustomsOffice), "IE9876", wrapper.DestinationCustomsOffice);
	}

	public override void TestGoodsPresentationDateTime()
	{
		var presentationDateTime = new DateTime(2023, 9, 20, 8, 30, 15);
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.GoodsPresentationDateTime), wrapper.GoodsPresentationDateTime);

		movementHeader.BM_PresentationDateTime = presentationDateTime;
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.GoodsPresentationDateTime), presentationDateTime, wrapper.GoodsPresentationDateTime);
	}

	public override void TestGrossMass()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.GrossMass), ZDecimal.Zero, wrapper.GrossMass);

		movementHeader.BM_GrossWeight = 123.45m;
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.GrossMass), 123.45m, wrapper.GrossMass);
	}

	public override void TestGuarantees()
	{
		var wrapper = CreateWrapper();
		var guarantee = wrapper.Guarantees;
		AssertNotNull(nameof(ID1Consignment.Guarantees), guarantee);
		AssertEquals($"{nameof(ID1Consignment.Guarantees)} Count", 0, guarantee.Count);

		var guarantee1 = movementHeader.Guarantees.AddNew();
		guarantee1.PW_BondType = "6";
		wrapper = CreateWrapper();
		guarantee = wrapper.Guarantees;
		AssertNotNull(nameof(ID1Consignment.Guarantees), guarantee);
		AssertEquals($"{nameof(ID1Consignment.Guarantees)} Count", 1, guarantee.Count);
		AssertType<GuaranteeWrapper>(guarantee.Single());

		guarantee1.PW_BondType = "0";
		wrapper = CreateWrapper();
		guarantee = wrapper.Guarantees;
		AssertNotNull(nameof(ID1Consignment.Guarantees), guarantee);
		AssertEquals($"{nameof(ID1Consignment.Guarantees)} Count", 1, guarantee.Count);
		AssertType<GuaranteeWrapperWithDetails>(guarantee.Single());
	}

	public override void TestHolderOfTransitProcedure()
	{
		var wrapper = CreateWrapper();
		var holderOfTransitProcedure = wrapper.HolderOfTransitProcedure;
		AssertNull(nameof(ID1Consignment.HolderOfTransitProcedure), holderOfTransitProcedure);

		var factory = Factory;
		var org = factory.New<OrgHeader>();
		org.CustomsCodes.AddNew("EOR", "385040449", "IT");
		var address = org.Addresses.AddNew();
		header.Principal.E2_OA_Address = address.PK;
		var jobDocAddress = factory.New<JobDocAddress>();
		jobDocAddress.OrganisationPK = org.PK;
		wrapper = CreateWrapper();
		holderOfTransitProcedure = wrapper.HolderOfTransitProcedure;

		AssertNotNull(nameof(ID1Consignment.HolderOfTransitProcedure), holderOfTransitProcedure);
		AssertType<HolderOfTransitProcedureWrapper>(holderOfTransitProcedure);
		AssertSame(nameof(ID1Consignment.HolderOfTransitProcedure), holderOfTransitProcedure, wrapper.HolderOfTransitProcedure);
		AssertEquals("IT385040449", holderOfTransitProcedure.IdentificationNumber);
	}

	public override void TestInlandTransportMode()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.InlandTransportMode), wrapper.InlandTransportMode);

		movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.InlandTransportMode), 4, wrapper.InlandTransportMode);
	}

	public override void TestIsBindingItinerary()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.IsBindingItinerary), false, wrapper.IsBindingItinerary);

		header.CountriesOfRouting.AddNew().CY_Data = "IT";
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.IsBindingItinerary), true, wrapper.IsBindingItinerary);
	}

	public override void TestIsContainerizedTransport()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.IsContainerizedTransport), false, wrapper.IsContainerizedTransport);

		var headerContainers = header.DepartureHeaderContainers;

		var cnt2 = headerContainers.AddNew();
		cnt2.BC_Mode = "NCT";
		cnt2.BC_ContainerNum = "CURE123456";
		cnt2.Seal1 = "Seal4";
		wrapper = CreateWrapper();
		AssertEquals("when not exist container", false, wrapper.IsContainerizedTransport);

		var cnt1 = headerContainers.AddNew();
		cnt1.BC_Mode = "CNT";
		cnt1.BC_ContainerNum = "TURE123456";
		cnt1.Seal1 = "Seal1";
		cnt1.Seal2 = "Seal2";
		cnt1.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";
		wrapper = CreateWrapper();
		AssertEquals("when exist container", true, wrapper.IsContainerizedTransport);
	}

	public override void TestLimitDate()
	{
		var wrapper = CreateWrapper();
		movementHeader.BM_ExportDate = ZDateTime.Empty;
		AssertNull(nameof(ID1Consignment.LimitDate), wrapper.LimitDate);

		movementHeader.BM_ExportDate = new DateTime(2023, 9, 20, 8, 30, 15);
		wrapper = CreateWrapper();

		AssertEquals(nameof(ID1Consignment.LimitDate), new DateTime(2023, 9, 20), wrapper.LimitDate);
	}

	public override void TestLocationOfGoods()
	{
		var wrapper = CreateWrapper();
		var locationOfGoods = wrapper.LocationOfGoods;
		AssertNull(nameof(ID1Consignment.LocationOfGoods), locationOfGoods);

		var goodsLocation = movementHeader.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		goodsLocation.CGL_Type = "D";

		wrapper = CreateWrapper();
		locationOfGoods = wrapper.LocationOfGoods;

		AssertNotNull(nameof(ID1Consignment.LocationOfGoods), locationOfGoods);
		AssertType<LocationOfGoodsWrapper>(locationOfGoods);
		AssertSame(nameof(ID1Consignment.LocationOfGoods), locationOfGoods, wrapper.LocationOfGoods);
		AssertEquals("D", locationOfGoods.TypeOfLocation);
	}

	public override void TestLrn()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.Lrn), "_IT_MSG_NMBR_PLCHLDR_", wrapper.Lrn);
	}

	public override void TestPlaceOfLoading()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.PlaceOfLoading), wrapper.PlaceOfLoading);

		movementHeader.BM_PortOfPresentationCode = "ITMIL";
		movementHeader.BM_PlaceOfLoading = "Milano";

		wrapper = CreateWrapper();
		var placeOfLoading = wrapper.PlaceOfLoading;
		AssertNotNull(nameof(ID1Consignment.PlaceOfLoading), placeOfLoading);
		AssertSame(placeOfLoading, wrapper.PlaceOfLoading);
		AssertType<PlaceOfLoadingWrapper>(placeOfLoading);
		AssertEquals(nameof(placeOfLoading.UNLOCode), "ITMIL", placeOfLoading.UNLOCode);
	}

	public override void TestPlaceOfUnloading()
	{
		var wrapper = CreateWrapper();
		AssertNull(nameof(ID1Consignment.PlaceOfUnloading), wrapper.PlaceOfUnloading);

		using (SetTransitionPeriod(false))
		{
			CombineAssertions("When TP OFF", () =>
			{
				movementHeader.BM_ForeignDestPortKCode = "ITMIL";
				movementHeader.BM_PlaceOfLoading = "Milano";
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				wrapper = CreateWrapper();
				var placeOfUnloading = wrapper.PlaceOfUnloading;
				AssertNull("BM_TypeOfSecurity == NON", placeOfUnloading);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				wrapper = CreateWrapper();
				placeOfUnloading = wrapper.PlaceOfUnloading;
				AssertNotNull("BM_TypeOfSecurity != NON", placeOfUnloading);
				AssertSame(placeOfUnloading, wrapper.PlaceOfUnloading);
				AssertType<PlaceOfUnloadingWrapper>(placeOfUnloading);
				AssertEquals(nameof(placeOfUnloading.UNLOCode), "ITMIL", placeOfUnloading.UNLOCode);
			});
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("When TP ON", () =>
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				wrapper = CreateWrapper();
				var placeOfUnloading = wrapper.PlaceOfUnloading;
				AssertNotNull("BM_TypeOfSecurity == NON", placeOfUnloading);
				AssertSame(placeOfUnloading, wrapper.PlaceOfUnloading);
				AssertType<PlaceOfUnloadingWrapper>(placeOfUnloading);
				AssertEquals(nameof(placeOfUnloading.UNLOCode), "ITMIL", placeOfUnloading.UNLOCode);
			});
		}
	}

	public override void TestPreviousDocuments()
	{
		var wrapper = CreateWrapper();
		var previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(ID1Consignment.PreviousDocuments), previousDocuments);
		AssertEquals($"{nameof(ID1Consignment.PreviousDocuments)} Count", 0, previousDocuments.Count);

		var previousDocument = header.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;

		wrapper = CreateWrapper();
		previousDocuments = wrapper.PreviousDocuments;
		AssertNotNull(nameof(ID1Consignment.PreviousDocuments), previousDocuments);
		AssertSame(previousDocuments, wrapper.PreviousDocuments);
		AssertType<PreviousDocumentWrapper>($"{nameof(ID1Consignment.PreviousDocuments)}, Type", previousDocuments.Single());
		AssertEquals($"{nameof(ID1Consignment.PreviousDocuments)} Count", 1, previousDocuments.Count);
	}

	public override void TestReducedDatasetIndicator()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.ReducedDatasetIndicator), false, wrapper.ReducedDatasetIndicator);

		movementHeader.BM_ReducedDatasetIndicator = true;
		wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.ReducedDatasetIndicator), true, wrapper.ReducedDatasetIndicator);
	}

	public override void TestRepresentative()
	{
		var wrapper = CreateWrapper();
		var representative = wrapper.Representative;
		AssertNull(nameof(ID1Consignment.Representative), representative);

		var org = Factory.New<OrgHeader>();
		org.CustomsCodes.AddNew("EOR", "385050449", "IT");
		var address = org.Addresses.AddNew();
		movementHeader.Representative.E2_OA_Address = address.PK;
		wrapper = CreateWrapper();
		representative = wrapper.Representative;

		AssertNotNull(nameof(ID1Consignment.Representative), representative);
		AssertType<RepresentativeWrapper>(representative);
		AssertSame(representative, wrapper.Representative);
		AssertEquals("IT385050449", representative.IdentificationNumber);
	}

	public override void TestSpecificCircumstanceIndicator()
	{
		CombineAssertions(() =>
		{
			var wrapper = CreateWrapper();
			AssertEquals(GetMessage(), string.Empty, wrapper.SpecificCircumstanceIndicator);

			movementHeader.BM_SpecificCircumstance = "ABC";
			wrapper = CreateWrapper();
			AssertEquals(GetMessage(), "ABC", wrapper.SpecificCircumstanceIndicator);
		});

		string GetMessage() => $"{nameof(movementHeader.BM_SpecificCircumstance)} = {movementHeader.BM_SpecificCircumstance}";
	}

	public override void TestSupportingDocuments()
	{
		var wrapper = CreateWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(ID1Consignment.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(ID1Consignment.SupportingDocuments)} Count", 0, supportingDocuments.Count);

		var supportingDocument = header.MovementHeader.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "C647";

		wrapper = CreateWrapper();
		supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(ID1Consignment.SupportingDocuments), supportingDocuments);
		AssertEquals($"{nameof(ID1Consignment.SupportingDocuments)} Count", 1, supportingDocuments.Count);
	}

	public override void TestTirCarnetNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ID1Consignment.TirCarnetNumber), wrapper.TirCarnetNumber);
		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
		movementHeader.TirCarnetNumber = "GB12345678";
		AssertNullOrEmpty($"{nameof(ID1Consignment.TirCarnetNumber)}, When BM_InBondEntryType is not TIR", wrapper.TirCarnetNumber);

		movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
		movementHeader.TirCarnetNumber = "GB12345678";

		wrapper = CreateWrapper();
		AssertEquals($"{nameof(ID1Consignment.TirCarnetNumber)} When BM_InBondEntryType is TIR", "GB12345678", wrapper.TirCarnetNumber);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		using (SetTransitionPeriod(false))
		{
			AssertWrapperFieldWithSingleLine<NctsBill>(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				wrapper => wrapper.TransportChargesMethodOfPayment);

			AssertWrapperFieldWithMultiLines<NctsBill>(
				"MethodOfPayment",
				(bill, value) => { bill.B0_TransportPaymentMethod = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				wrapper => wrapper.TransportChargesMethodOfPayment);
		}

		using (SetTransitionPeriod(true))
		{
			AssertWrapperFieldWithSingleLine<NctsDepartureCargoDesc>(
				"MethodOfPayment",
				(line, value) => { line.BY_TransportChargesMethodOfPayment = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				wrapper => wrapper.TransportChargesMethodOfPayment);

			AssertWrapperFieldWithMultiLines<NctsDepartureCargoDesc>(
				"MethodOfPayment",
				(line, value) => { line.BY_TransportChargesMethodOfPayment = value; },
				(movementHeader, value) => { movementHeader.BM_MethodOfPayment = value; },
				(wrapper) => wrapper.TransportChargesMethodOfPayment);
		}
	}

	public override void TestTransportDocuments()
	{
		var wrapper = CreateWrapper();
		var transportDocuments = wrapper.TransportDocuments;
		AssertNotNull(nameof(ID1Consignment.TransportDocuments), transportDocuments);
		AssertEquals($"{nameof(ID1Consignment.TransportDocuments)} Count", 0, transportDocuments.Count);

		var additionalDocument = header.AdditionalDocuments.AddNew();
		additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		wrapper = CreateWrapper();
		transportDocuments = wrapper.TransportDocuments;
		AssertNotNull(nameof(ID1Consignment.TransportDocuments), transportDocuments);
		AssertType<TransportDocumentWrapper>(transportDocuments.Single());
		AssertSame(transportDocuments, wrapper.TransportDocuments);
		AssertEquals($"{nameof(ID1Consignment.TransportDocuments)} Count", 1, transportDocuments.Count);
	}

	public override void TestTransportEquipment()
	{
		var wrapper = CreateWrapper();
		var transportEquipment = wrapper.TransportEquipment;
		AssertNotNull(nameof(ID1Consignment.TransportEquipment), transportEquipment);
		AssertEquals($"{nameof(ID1Consignment.TransportEquipment)} Count", 0, transportEquipment.Count);

		var container1 = header.DepartureHeaderContainers.AddNew();
		container1.BC_Mode = "CNT";
		container1.BC_ContainerNum = "TURE123456";
		container1.Seal1 = "Seal1";
		container1.Seal2 = "Seal2";
		container1.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";

		var container2 = header.DepartureHeaderContainers.AddNew();
		container2.BC_Mode = "NCT";
		container2.BC_ContainerNum = "CURE123456";
		container2.Seal1 = "Seal4";

		var container3 = header.DepartureHeaderContainers.AddNew();
		container3.BC_Mode = "NCT";
		container3.BC_ContainerNum = "MURE123456";

		wrapper = CreateWrapper();
		transportEquipment = wrapper.TransportEquipment;
		AssertNotNull(nameof(ID1Consignment.TransportEquipment), transportEquipment);
		AssertEquals($"{nameof(ID1Consignment.TransportEquipment)} Count", 2, transportEquipment.Count);
		AssertSame(nameof(ID1Consignment.TransportEquipment), transportEquipment, wrapper.TransportEquipment);
		AssertEquals($"{nameof(ID1Consignment.TransportEquipment)} Items are {nameof(TransportEquipmentWrapper)}", true, transportEquipment.All(x => x is TransportEquipmentWrapper));
		AssertContainsExactElementsInAnyOrder($"{nameof(ID1Consignment.TransportEquipment)} ContainerIDs", new[] { "TURE123456", "" }, transportEquipment.Select(x => x.ContainerID));
	}

	public override void TestTypeOfSecurity()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ID1Consignment.TypeOfSecurity), 0, wrapper.TypeOfSecurity);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		wrapper = CreateWrapper();
		AssertEquals($"{nameof(ID1Consignment.TypeOfSecurity)} when NON", 0, wrapper.TypeOfSecurity);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		wrapper = CreateWrapper();
		AssertEquals($"{nameof(ID1Consignment.TypeOfSecurity)} when ENT", 1, wrapper.TypeOfSecurity);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
		wrapper = CreateWrapper();
		AssertEquals($"{nameof(ID1Consignment.TypeOfSecurity)} when EXI", 2, wrapper.TypeOfSecurity);

		movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
		wrapper = CreateWrapper();
		AssertEquals($"{nameof(ID1Consignment.TypeOfSecurity)} when BTH", 3, wrapper.TypeOfSecurity);
	}

	public override void TestUcr()
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

		var ucr = (NctsHeader h) => CreateWrapper(h).Ucr;

		using (SetTransitionPeriod(false))
		{
			AssertEquals(string.Empty, ucr(header));
		}

		using (SetTransitionPeriod(true))
		{
			CombineAssertions("Transition Period True", () =>
			{
				AssertEquals("PL", ucr(header));

				var wrapper = CreateWrapper();
				AssertNullOrEmpty(nameof(ID1Consignment.Ucr), wrapper.Ucr);

				movementHeader.BM_UniqueConsignmentReference = "UQREF123";
				wrapper = CreateWrapper();
				AssertEquals(nameof(ID1Consignment.Ucr), "UQREF123", wrapper.Ucr);
			});
		}
	}

	protected override ID1Consignment CreateWrapper() => CreateWrapper(header);

	ID1Consignment CreateWrapper(NctsHeader header) => new D1ConsignmentWrapper(header, messageSendingObjectFactory.Object);

	void AssertWrapperFieldWithSingleLine<T>(
		string fieldName,
		Action<T, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<ID1Consignment, string> getWrapperFieldValue) where T : class
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = header.MovementHeader;
		var wrapper = CreateWrapper(header);
		AssertNullOrEmpty($"When no header or lines are found, {fieldName}", getWrapperFieldValue(wrapper));

		T lineItem;
		if (typeof(T) == typeof(NctsDepartureCargoDesc))
		{
			lineItem = header.Bills.AddNew().GoodsItems.AddNew() as T;
		}
		else if (typeof(T) == typeof(NctsBill))
		{
			lineItem = header.Bills.AddNew() as T;
		}
		else
		{
			throw new InvalidOperationException("Unsupported type");
		}

		CombineAssertions(() =>
		{
			setHeaderTo(movementHeader, "X");
			setLineFieldTo(lineItem, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is only provided at header level and not at line, {fieldName}", "X", getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem, "X");
			wrapper = CreateWrapper(header);
			AssertEquals($"When line and header have same {fieldName}, {fieldName}", "X", getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem, "Y");
			wrapper = CreateWrapper(header);
			AssertEquals($"When line has {fieldName} different from header, {fieldName}", "Y", getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When line has {fieldName} and header is empty, {fieldName}", "Y", getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When both line and header don't have value, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});
	}

	void AssertWrapperFieldWithMultiLines<T>(
		string fieldName,
		Action<T, string> setLineFieldTo,
		Action<NctsDepartureMovementHeader, string> setHeaderTo,
		Func<ID1Consignment, string> getWrapperFieldValue) where T : class
	{
		var header = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = header.MovementHeader;
		var wrapper = CreateWrapper(header);
		var lineItems = new List<T>();
		if (typeof(T) == typeof(NctsDepartureCargoDesc))
		{
			lineItems.Add(header.Bills.AddNew().GoodsItems.AddNew() as T);
			lineItems.Add(header.Bills.AddNew().GoodsItems.AddNew() as T);
		}
		else if (typeof(T) == typeof(NctsBill))
		{
			lineItems.Add(header.Bills.AddNew() as T);
			lineItems.Add(header.Bills.AddNew() as T);
		}
		else
		{
			throw new InvalidOperationException("Unsupported type");
		}

		AssertNullOrEmpty($"When no header or lines are found, {fieldName}", getWrapperFieldValue(wrapper));
		var lineItem1 = lineItems[0];
		var lineItem2 = lineItems[1];

		CombineAssertions($"{fieldName} only in lines", () =>
		{
			setHeaderTo(movementHeader, string.Empty);

			setLineFieldTo(lineItem1, "X");
			setLineFieldTo(lineItem2, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is only available in one line, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem1, "Y");
			setLineFieldTo(lineItem2, "Y");
			wrapper = CreateWrapper(header);
			AssertEquals($"When all lines have same {fieldName}, {fieldName}", "Y", getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem1, "Z");
			wrapper = CreateWrapper(header);
			AssertEquals($"When lines have different {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem2, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When lines have different {fieldName} (one does not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(lineItem1, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});

		CombineAssertions($"{fieldName} both in lines and header", () =>
		{
			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(lineItem1, "Y");
			setLineFieldTo(lineItem2, "Y");
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is the same in lines and header, {fieldName}", "Y", getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(lineItem1, "X");
			setLineFieldTo(lineItem2, "Z");
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is present in lines and header and different in all cases, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(lineItem1, "X");
			setLineFieldTo(lineItem2, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setHeaderTo(movementHeader, "Y");
			setLineFieldTo(lineItem1, string.Empty);
			setLineFieldTo(lineItem2, string.Empty);
			wrapper = CreateWrapper(header);
			AssertEquals($"When {fieldName} is only available at header level, {fieldName}", "Y", getWrapperFieldValue(wrapper));
		});
	}

	IDisposable SetTransitionPeriod(bool isActive) =>
		ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeaderWrapper = new Mock<INctsHeaderWrapper>();
		messageSendingObjectFactory = new Mock<IMessageSendingWrapperFactory>();
		messageSendingObjectFactory.Setup(x => x.GetNewNctsHeaderWrapper(It.IsAny<NctsHeader>())).Returns(nctsHeaderWrapper.Object);
	}

	Mock<IMessageSendingWrapperFactory> messageSendingObjectFactory;
	Mock<INctsHeaderWrapper> nctsHeaderWrapper;
}
