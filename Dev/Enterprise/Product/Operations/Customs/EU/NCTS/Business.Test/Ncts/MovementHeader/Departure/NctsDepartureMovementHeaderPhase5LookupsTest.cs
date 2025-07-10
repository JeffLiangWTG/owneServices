using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RefVesselCollection = Enterprise.MasterFiles.Business.RefVesselCollection;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestNctsMessageStatusList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.NctsMessageStatusList, "ACC, ACK, ERR, FAL, INV, SNT");
	}

	public void TestOrganisations()
	{
		AssertType<OrgHeaderCollection>("Type", lookups.Organisations);
	}

	public void TestBondedWarehouseCollection()
	{
		AssertType<BondedWarehouseCollection>("Type", lookups.BondedWarehouseCollection);
	}

	public void TestSpecificCircumstanceIndicatorList()
	{
		var list = lookups.SpecificCircumstanceIndicatorList;
		AssertEquals("Phase4 List should be empty", string.Empty, list.CodesAsString);
	}

	public void TestNctsSpecificCircumstanceIndicatorList()
	{
		var list = lookups.NctsSpecificCircumstanceIndicatorList;
		CombineAssertions(() =>
		{
			AssertEquals("List values", "A20, XXX", list.CodesAsString);
			AssertSame("Cached", list, lookups.NctsSpecificCircumstanceIndicatorList);
		});
	}

	public void TestTransportChargesModeOfPaymentList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.TransportChargesModeOfPaymentList, "A, B, C, D, H, Y, Z");
	}

	public void TestNctsControlResultList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.NctsControlResultList, "A1, A2, A3, A4, A5, B1");
	}

	public void TestSealTypeList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.SealTypeList, "CON, PAC");
	}

	public void TestModeOfTransportList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.ModeOfTransportList, "1, 2, 3, 4, 5, 7, 8, 9");
	}

	public void TestAdditionalDeclarationTypeList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.AdditionalDeclarationTypeList, "A, D");
	}

	public void TestTransportAtDepartureTypeOfIdList()
	{
		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._1_SeaTransport;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "10, 11");
		AssertEquals("Values", "10", lookups.TransportAtDepartureTypeOfIdList.DefaultCode);

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "20, 21");
		AssertEquals("Values", "20", lookups.TransportAtDepartureTypeOfIdList.DefaultCode);

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "30");
		AssertEquals("Values", "30", lookups.TransportAtDepartureTypeOfIdList.DefaultCode);

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "40, 41");
		AssertEquals("Values", "40", lookups.TransportAtDepartureTypeOfIdList.DefaultCode);

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "80, 81");
		AssertEquals("Values", "80", lookups.TransportAtDepartureTypeOfIdList.DefaultCode);

		movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtDepartureTypeOfIdList, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81");
	}

	public void TestBorderModeOfTransportList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.BorderModeOfTransportList, "1, 2, 3, 4, 5, 7, 8, 9");
	}

	public void TestTransportAtBorderTypeOfIdList()
	{
		movementHeader.BM_ExportTransportMode = "1";
		AssertEquals("TransportAtBorderTypeOfIdList Codes", "10, 11", lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		AssertEquals("TransportAtBorderTypeOfIdList DefaultCode", "10", lookups.TransportAtBorderTypeOfIdList.DefaultCode);

		movementHeader.BM_ExportTransportMode = "2";
		AssertEquals("TransportAtBorderTypeOfIdList Codes", "21", lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		AssertEquals("TransportAtBorderTypeOfIdList DefaultCode", "21", lookups.TransportAtBorderTypeOfIdList.DefaultCode);

		movementHeader.BM_ExportTransportMode = "3";
		AssertEquals("TransportAtBorderTypeOfIdList Codes", "30", lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		AssertEquals("TransportAtBorderTypeOfIdList DefaultCode", "30", lookups.TransportAtBorderTypeOfIdList.DefaultCode);

		movementHeader.BM_ExportTransportMode = "4";
		AssertEquals("TransportAtBorderTypeOfIdList Codes", "40, 41", lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		AssertEquals("TransportAtBorderTypeOfIdList DefaultCode", "40", lookups.TransportAtBorderTypeOfIdList.DefaultCode);

		movementHeader.BM_ExportTransportMode = "8";
		AssertEquals("TransportAtBorderTypeOfIdList Codes", "80, 81", lookups.TransportAtBorderTypeOfIdList.CodesAsString);
		AssertEquals("TransportAtBorderTypeOfIdList DefaultCode", "80", lookups.TransportAtBorderTypeOfIdList.DefaultCode);

		movementHeader.BM_ExportTransportMode = "9";
		AssertListHasCorrectValuesAndIsCached(x => x.TransportAtBorderTypeOfIdList, "10, 11, 21, 30, 40, 41, 80, 81, 99");
	}

	public void TestRepresentatives()
	{
		CombineAssertions(() =>
		{
			AssertType<OrganisationsFindBoxCollection>("Type", lookups.Representatives);

			var defaultFilter = lookups.Representatives.FilterBusinessObjectDefaults["Category:Property"];

			AssertNotNull("Representatives default filter must contain Category", defaultFilter);
			AssertEquals("Category default filter must be NAT", (ZString)OrgConstants.Category.NaturalPersonIndividual, defaultFilter.Value);
		});
	}

	public void TestLocationOfGoodsCodeList()
	{
		AssertEquals(string.Empty, lookups.LocationOfGoodsCodeList.CodesAsString);
	}

	public void TestTypeOfSecurityList()
	{
		AssertListHasCorrectValuesAndIsCached(x => x.TypeOfSecurityList, "NON, ENT, EXI, BTH");
	}

	public void TestTOLCarrierIDList()
	{
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._11;
		var carrierIDList = lookups.TOLCarrierIDList;
		var vessels = carrierIDList as RefVesselCollection;
		AssertNotNull("Should be RefVesselCollection", vessels);
		AssertEquals("vessels.UseLloyds", false, vessels.UseLloyds);

		movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._10;
		carrierIDList = lookups.TOLCarrierIDList;
		vessels = carrierIDList as RefVesselCollection;
		AssertNotNull("Should be RefVesselCollection", vessels);
		AssertEquals("vessels.UseLloyds", true, vessels.UseLloyds);

		movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._2_RailTransport;
		AssertNull("Should not be RefVesselCollection", lookups.TOLCarrierIDList as RefVesselCollection);
	}

	public void TestTOLCarrierNationalityList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
		Factory.Save();

		var countries = lookups.TOLCarrierNationalityList as ZZRefCusCodeListCombinedCollection;
		countries.Load();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	public void TestForeignDestPortCodes_Countries()
	{
		movementHeader.BM_ForeignDestPortKCode = "DE";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.Germany);
		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		{
			var countryOfExportList = lookups.ForeignDestPortCodes as ZZRefCusCodeListCombinedCollection;
			countryOfExportList.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryOfExportList.Select(x => x.ZZD_Code));
		}
	}

	public void TestForeignDestPortCodes_UNLOCO()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ForeignDestPortKCode = string.Empty;
			AssertType<RefUNLOCOCollection>("BM_ForeignDestPortKCode is empty", lookups.ForeignDestPortCodes);

			movementHeader.BM_ForeignDestPortKCode = "ABCDE";
			AssertType<RefUNLOCOCollection>("BM_ForeignDestPortKCode is 5 characters", lookups.ForeignDestPortCodes);
		});
	}

	public void TestPortOfPresentationCodes_Countries()
	{
		movementHeader.BM_PortOfPresentationCode = "DE";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.Germany);
		Factory.Save();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
		{
			var countryOfExportList = lookups.PortOfPresentationCodes as ZZRefCusCodeListCombinedCollection;
			countryOfExportList.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryOfExportList.Select(x => x.ZZD_Code));
		}
	}

	public void TestPortOfPresentationCodes_UNLOCO()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_PortOfPresentationCode = string.Empty;
			AssertType<RefUNLOCOCollection>("BM_PortOfPresentationCode is empty", lookups.PortOfPresentationCodes);

			movementHeader.BM_PortOfPresentationCode = "ABCDE";
			AssertType<RefUNLOCOCollection>("BM_PortOfPresentationCode is 5 characters", lookups.PortOfPresentationCodes);
		});
	}

	public void TestOfficeCodeList()
	{
		movementHeader.CustomsOfficesForDeparture.RemoveAll();
		AddCustomsOffice("FR001", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		AddCustomsOffice("FR002", OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		AddCustomsOffice("FR003", OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
		AddCustomsOffice("FR004", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		AddCustomsOffice("FR005", OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);

		CombineAssertions(() =>
		{
			var result = lookups.OfficeCodeList;
			AssertContainsExactElementsInAnyOrder("Departure offices", new[] { "FR001", "FR002", "FR003", "FR004" }, result.GetAllCodes());
			AssertSame("Cached", result, lookups.OfficeCodeList);

			AddCustomsOffice("FR006", OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AssertContainsExactElementsInAnyOrder("New office is provided, Cache has been cleared", new[] { "FR001", "FR002", "FR003", "FR004", "FR006" }, lookups.OfficeCodeList.GetAllCodes());
		});

		void AddCustomsOffice(string code, string role)
		{
			var office = movementHeader.CustomsOfficesForDeparture.AddNew();
			office.CY_Code = role;
			office.CY_Data = code;
		}
	}

	public void TestNctsTransitStatusList()
	{
		CombineAssertions(() =>
		{
			AssertType<NCTS5DepartureCustomsStatusList>("Type", lookups.NctsTransitStatusList);
			AssertSame("Cached", lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		lookups = new NctsDepartureMovementHeaderPhase5Lookups(movementHeader);
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeaderPhase5Lookups lookups;
	NctsDepartureMovementHeader movementHeader;

	void AssertListHasCorrectValuesAndIsCached(Func<NctsDepartureMovementHeaderPhase5Lookups, CodeDescriptionPairList> getList, string expectedValues)
	{
		CombineAssertions(() =>
		{
			var list = getList.Invoke(lookups);
			AssertEquals("Values", expectedValues, list.CodesAsString);
			AssertSame("Cached", list, getList.Invoke(lookups));
		});
	}
}
