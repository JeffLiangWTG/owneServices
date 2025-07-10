using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class UniversalLookupsHelperTest : TestCaseWithFactory
	{
		public void TestGetCountryLists()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("C0009", "C0009");
			helper.CreateNewOrGetExistingCusCodeList("LV", "C0009", "GB", "United Kingdom", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("LV", "C0009", "AD", "Andorra", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var countryList = Factory.GetCountryList("LV", "C0009");
			countryList.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Collection Values", new[] { "AD", "GB" }, countryList.Select(c => c.ZZD_Code).ToArray());
				AssertEquals("Latvia is the default for Country/Region or Grouping", "LV", countryList.FilterBusinessObjectDefaults["Country/Region or Grouping:Property"].Value);
				AssertEquals("C0009 is the default for List Type", "C0009", countryList.FilterBusinessObjectDefaults["List Type:Property"].Value);
			});
		}

		public void TestGetCountryNC008Collection_NoDuplicates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR_ForCountryAndEU(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.Germany);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var countryCodes = Factory.GetCountryNC008Collection(Core.Constants.CountryCodes.Germany);
				countryCodes.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryCodes.Select(x => x.ZZD_Code));
			}
		}

		public void TestGetCountryNC008Collection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008, Core.Constants.CountryCodes.Germany);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var countryCodes = Factory.GetCountryNC008Collection(Core.Constants.CountryCodes.Germany);
				countryCodes.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryCodes.Select(x => x.ZZD_Code));
			}
		}

		public void TestGetCountryNC008Collection_EUN_Fallback_DateGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR_ForCountryAndEU(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var countryCodes = Factory.GetCountryNC008Collection(Core.Constants.CountryCodes.Switzerland);
				countryCodes.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "DE", "FR" }, countryCodes.Select(x => x.ZZD_Code));
			}
		}

		public void TestGetCountryCL234List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL234, "AA", "Document AA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL234, "BB", "Document BB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var documentTypeExciseCodes = UniversalLookupsHelper.GetCL234List(nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("AA, BB", documentTypeExciseCodes.CodesAsString);
				AssertSame("Cached", documentTypeExciseCodes, UniversalLookupsHelper.GetCL234List(nctsHeader));
			});
		}

		public void TestGetCountryCL234List_FallbackCH()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL234, "C651", "Document AA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL234, "C658", "Document BB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			var documentTypeExciseCodes = UniversalLookupsHelper.GetCL234List(nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("C651, C658", documentTypeExciseCodes.CodesAsString);
				AssertSame("Cached", documentTypeExciseCodes, UniversalLookupsHelper.GetCL234List(nctsHeader));
			});
		}

		public void TestGetCountryCL505List_DateGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(RefCusCodeListTypes.Codes.Code_CL505);
			Factory.Save();

			var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var countryWithoutZipList = UniversalLookupsHelper.GetCountryCL505List(nctsHeader);
			CombineAssertions(() =>
			{
				AssertEquals("AU, DE, FR", countryWithoutZipList.CodesAsString);
				AssertSame("Cached", countryWithoutZipList, UniversalLookupsHelper.GetCountryCL505List(nctsHeader));
			});
		}

		public void TestGetNCNATCountryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var countries = Factory.GetNCNATCountryList();
			countries.Load();

			var expectedCodes = new ZString[] { "AU", "DE", "FR" };
			AssertContainsExactElementsInAnyOrder(expectedCodes, countries.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestGetCL010CountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: euGroup);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CL010");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "1", "EU 1", tomorrow, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, "2", "EU 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "3", "EU 3", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "4", "DE 4", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "5", "DE 5", yesterday, tomorrow);
			Factory.Save();

			AssertEquals("Should contain values only from EU (EUN) and time contain today", "3", Factory.GetCL010CountryCodes().CodesAsString);
		}

		public void TestGetCountryCodesCTC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: euGroup);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL012");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "1", "EU 1", tomorrow, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT, "2", "EU 2", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "3", "EU 3", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "4", "DE 4", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "5", "DE 5", yesterday, tomorrow);
			Factory.Save();

			AssertEquals("Should contain values only from EU (EUN) and time contain today", "3", Factory.GetCountryCodesCTC().CodesAsString);
		}

		public void TestGetCL112CountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL112, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL112, "AA", "Document AA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL112, "BB", "Document BB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var documentTypeExciseCodes = UniversalLookupsHelper.GetCountryCodesCTC(nctsHeader.Factory);

			CombineAssertions(() =>
			{
				AssertEquals("AA, BB", documentTypeExciseCodes.CodesAsString);
				AssertSame("Cached", documentTypeExciseCodes, UniversalLookupsHelper.GetCountryCodesCTC(nctsHeader.Factory));
			});
		}

		public void TestGetCL1178PreviousDocuments()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL178, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL178, "AA", "Document AA", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_CL178, "BB", "Document BB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var documentTypeExciseCodes = UniversalLookupsHelper.GetNctsPreviousDocumentUnionGoodsCode(nctsHeader.Factory);

			CombineAssertions(() =>
			{
				AssertEquals("AA, BB", documentTypeExciseCodes.CodesAsString);
				AssertSame("Cached", documentTypeExciseCodes, UniversalLookupsHelper.GetNctsPreviousDocumentUnionGoodsCode(nctsHeader.Factory));
			});
		}

		public void TestGetCountryC0009List_DateGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009;
			var eurpoeanUnionCode = RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			var lvId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, Core.Constants.CountryCodes.Latvia, "Latvia", startDate, endDate);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, Core.Constants.CountryCodes.Switzerland, "Switzerland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, Core.Constants.CountryCodes.Liechtenstein, "Liechtenstein", startDate, endDate);

			Factory.Save();

			var countryOfDestinationListDE = Factory.GetCountryC0009List(Core.Constants.CountryCodes.Germany);
			var countryOfDestinationListLV = Factory.GetCountryC0009List(Core.Constants.CountryCodes.Latvia);
			var countryOfDestinationListCH = Factory.GetCountryC0009List(Core.Constants.CountryCodes.Switzerland);
			var countryOfDestinationListTR = Factory.GetCountryC0009List(Core.Constants.CountryCodes.Turkey);

			CombineAssertions(() =>
			{
				AssertEquals("EU list", "AU, DE, FR", countryOfDestinationListDE.CodesAsString);
				AssertEquals("own country codes with EU", "AU, DE, FR, LV", countryOfDestinationListLV.CodesAsString);
				AssertEquals("own country list", "CH, LI", countryOfDestinationListCH.CodesAsString);
				AssertEquals("Fallback to EU", "AU, DE, FR", countryOfDestinationListTR.CodesAsString);

				AssertSame("Cached", countryOfDestinationListDE, Factory.GetCountryC0009List(Core.Constants.CountryCodes.Germany));
			});
		}

		public void TestGetCL229EUGuaranteeTypeCTC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: euGroup);
			helper.CreateNewOrGetExistingCusCodeType("CL229", "CL229");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL229", "0", "DEC 0", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL229", "1", "DEC 1", tomorrow, tomorrow);
			Factory.Save();

			var cl229EUGuaranteeType = Factory.GetCL229EUGuaranteeTypeCTC();
			CombineAssertions(() =>
			{
				AssertEquals("Should contain values only from EU (EUN) and time contain today", "0", cl229EUGuaranteeType.CodesAsString);
				AssertSame("Cached", cl229EUGuaranteeType, Factory.GetCL229EUGuaranteeTypeCTC());
			});
		}

		public void TestGetCL230EUGuaranteeTypeEUNonTIR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: euGroup);
			helper.CreateNewOrGetExistingCusCodeType("CL230", "CL230");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL230", "0", "DEC 0", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL230", "1", "DEC 1", tomorrow, tomorrow);
			Factory.Save();

			var cl230EUGuaranteeType = Factory.GetCL230EUGuaranteeTypeEUNonTIR();
			CombineAssertions(() =>
			{
				AssertEquals("Should contain values only from EU (EUN) and time contain today", "0", cl230EUGuaranteeType.CodesAsString);
				AssertSame("Cached", cl230EUGuaranteeType, Factory.GetCL230EUGuaranteeTypeEUNonTIR());
			});
		}

		public void TestGetGetCL294CustomsOfficeExitCodes()
		{
			SetupCustomsOfficeExitCodes();
			SetupCustomsOfficeDestinationCodes();

			var cl274codeDescriptionPairList = Factory.GetCL294CustomsOfficeExitCodes();
			AssertEquals("DE009103 is Customs Office Exit Code", true, cl274codeDescriptionPairList.ContainsCode("DE009103"));
			AssertEquals("IESNN400 is Customs Office Exit Code", true, cl274codeDescriptionPairList.ContainsCode("IESNN400"));
			AssertEquals("DE004204 is  Customs Office Destination Code ", false, cl274codeDescriptionPairList.ContainsCode("DE004204"));
			AssertEquals("IELON200 is  Customs Office Destination Code ", false, cl274codeDescriptionPairList.ContainsCode("IELON200"));
		}

		public void TestGetCL172CustomsOfficeDestinationCodes()
		{
			SetupCustomsOfficeExitCodes();
			SetupCustomsOfficeDestinationCodes();

			var cl172codeDescriptionPairList = Factory.GetCL172CustomsOfficeDestinationCodes();
			AssertEquals("DE009103 is Customs Office Exit Code", false, cl172codeDescriptionPairList.ContainsCode("DE009103"));
			AssertEquals("IESNN400 is Customs Office Exit Code", false, cl172codeDescriptionPairList.ContainsCode("IESNN400"));
			AssertEquals("DE004204 is  Customs Office Destination Code ", true, cl172codeDescriptionPairList.ContainsCode("DE004204"));
			AssertEquals("IELON200 is  Customs Office Destination Code ", true, cl172codeDescriptionPairList.ContainsCode("IELON200"));
		}

		void SetupCustomsOfficeExitCodes()
		{
			var factory = Factory;

			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var codeIESNN400ExitCL294 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IESNN400",
				description: "Shannon Airport",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeIESNN400ExitCL294.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			var codeDE009103ExitCL294 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Germany,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "DE009103",
				description: "Wismar",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeDE009103ExitCL294.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();
		}

		void SetupCustomsOfficeDestinationCodes()
		{
			var factory = Factory;

			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var codeDE004204DestinationCL172 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Germany,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "DE004204",
				description: "Laufenburg",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeDE004204DestinationCL172.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDestination);

			var codeIELON200DestinationCL172 = helper.CreateNewOrGetExistingCusCodeList(
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				code: "IELON200",
				description: "LONGFORD CUSTOMS OFFICE",
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue
			);
			helper.CreateCusCodeListAttribute(codeIELON200DestinationCL172.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDestination);

			Factory.Save();
		}

		public void TestGetCL740()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: euGroup);
			helper.CreateNewOrGetExistingCusCodeType("CL740", "CL740");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL740", "100000", "Cash control", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL740", "200000", "Counterfeit – Intellectual Property Rights (IPR)", tomorrow, tomorrow);
			Factory.Save();

			var cl740 = Factory.GetCL740();
			CombineAssertions(() =>
			{
				AssertEquals("Should contain values only from EU (EUN) and time contain today", "100000", cl740.CodesAsString);
				AssertSame("Cached", cl740, Factory.GetCL740());
			});
		}
	}
}
