using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.GDM.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.Testing
{
	sealed class UniversalReferenceDataHelperTests : TestCaseWithFactory
	{
		public void TestCountryOfOrigins()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "3rd Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "FR", "France", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QP", "High seas", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QQ", "Stores and provisions", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QR", "Stores and provisions within the framework of intra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QS", "Stores and provisions within the framework of extra-Union trade", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "QX", "Countries and territories not specified for commercial or military reasons", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "ZZ", "Generic 3rd country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "IT", "Italy", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "ES", "Spain", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "YY", "Generic EU country", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO Country of destination");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "MQ", "Martinique", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "RE", "Reunion", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "XX", "Generic DROM", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lookups = invoiceLine.Lookups;
			var list = UniversalReferenceDataHelper.GetCustomsApprovedCountryList(Factory, "FR");
			list.Load();

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", list);
			AssertContainsExactElementsInAnyOrder("Country of Origins should be based on CO17 + EU17 + EX17 FR only code lists", new ZString[] { "FR", "QP", "QQ", "QR", "QS", "QX", "IT", "ES", "MQ", "RE" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
		}

		public void TestGetVATApplicabilitiesWithSecondaryTradeGroup()
		{
			GuidedDecisionMakingVATCollectionTest.SetUpVATApplicabilities(Factory);
			var loader = new TariffView.Loader(base.Factory);
			var tariff = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.EuropeanUnion, "1111111111", ZDateTime.Today);

			var vats = UniversalReferenceDataHelper.GetVATApplicabilitiesWithSecondaryTradeGroup(Factory, tariff, Core.Constants.CountryCodes.France, new ZString[] { "CONTI" }, ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("One VATApplicability should be loaded.", 1, vats.Count());
				AssertEquals("The VAT code of loaded VATApplicability should be V001", "V001", vats.First().ZX5_AdditionalCode);
			});
		}

		public void TestGetCachedPackageTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = UniversalReferenceDataHelper.GetCachedPackageTypeList(Factory);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "VG", "NE" }, list.GetAllCodes());
				var list2 = UniversalReferenceDataHelper.GetCachedPackageTypeList(Factory);
				AssertSame("Cached", list, list2);
			});
		}

		public void TestGetChargePaymentOrDestinationID()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateHarbourRate("IMP", "395", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([TLCL] > 1, MAX(2, 0.5 * [TLCL]), 0)", "FR");
			CreatePortTax(referenceDataHelper, "230", "BAYONNE", "FRLEH", "FR004560");
			CreatePortTax(referenceDataHelper, "395", "PORT DE BAYONNE", "FRLEH", "FR004560");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR004560";
			declaration.JE_RL_NKPortOfArrival = "FRLEH";
			declaration.JE_ContainerMode = "ULD";
			AssertContainsExactElementsInAnyOrder("Prerequisite: Combination of customs office and unloading port leads to two port codes.", new string[] { "230", "395" }, declaration.Lookups.ChargePaymentOrDestinationIDs.GetAllCodes());
			AssertEquals("Only Port code 395 has got records in RefCusHarbourRate. GetPortCode should then return 395.", "395", UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(Factory, declaration));
		}

		public void TestGetLocalHarbourFeeCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusMapType("HAFEE", "OUT", "Harbour Fee", true);
			var refHarbourRate1 = referenceDataHelper.CreateHarbourRate("IMP", "FRPAR", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([T] > 1, MAX(2, 0.5 * [T]), 0)", "FR");
			Factory.Save();

			AssertEquals("V905", UniversalReferenceDataHelper.GetLocalHarbourFeeCode(Factory, declaration, refHarbourRate1, ZDateTime.Today));

			var newFactory = new BusinessObjectFactory();
			referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);
			referenceDataHelper.CreateCusMapType("HAFEE", "OUT", "Harbour Fee", true);
			referenceDataHelper.CreateCusMap("HAFEE", "FRPAR", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			refHarbourRate1 = referenceDataHelper.CreateHarbourRate("IMP", "FRPAR", "CON", "", ZDate.Today.AddDays(-2), ZDate.Today.AddDays(2), "IF([T] > 1, MAX(2, 0.5 * [T]), 0)", "FR");
			newFactory.Save();

			AssertEquals("111", UniversalReferenceDataHelper.GetLocalHarbourFeeCode(newFactory, declaration, refHarbourRate1, ZDateTime.Today));
		}

		public void TestPaymentDestinationList()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusCodeType("PTX", "FR Port Tax Combination", "FR", 3);
			Factory.Save();
			CreatePortTax(referenceDataHelper, "010", "BORDEAUX BASSENS", "FRBAS", "FR000120");
			CreatePortTax(referenceDataHelper, "202", "BORDEAUX-BASSENS", "FRBAS", "FR000120");
			CreatePortTax(referenceDataHelper, "810", "GRAND PORT MARITIME BORDEAUX BASSENS", "FRBAS", "FR000120");
			CreatePortTax(referenceDataHelper, "039", "BAYONNE", "FRBAY", "FR000390");
			CreatePortTax(referenceDataHelper, "148", "PORT DE BAYONNE", "FRBAY", "FR000390");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsOffice = "FR000120";
			declaration.JE_RL_NKPortOfArrival = "FRBAS";
			var list = UniversalReferenceDataHelper.PaymentDestinationList(Factory, declaration);

			CombineAssertions(() =>
			{
				AssertEquals("Count should be 3", "010, 202, 810", list.CodesAsString);
				AssertEquals("010", "BORDEAUX BASSENS", list.GetDescriptionFromCode("010"));
				AssertEquals("202", "BORDEAUX-BASSENS", list.GetDescriptionFromCode("202"));
				AssertEquals("810", "GRAND PORT MARITIME BORDEAUX BASSENS", list.GetDescriptionFromCode("810"));

				declaration.JE_CustomsOffice = "FR000390";
				declaration.JE_RL_NKPortOfArrival = "FRBAY";
				list = UniversalReferenceDataHelper.PaymentDestinationList(Factory, declaration);
				AssertEquals("Count should be 2", "039, 148", list.CodesAsString);
				AssertEquals("039", "BAYONNE", list.GetDescriptionFromCode("039"));
				AssertEquals("148", "PORT DE BAYONNE", list.GetDescriptionFromCode("148"));

				declaration.JE_CustomsOffice = "AAA";
				declaration.JE_RL_NKPortOfArrival = "BBB";
				list = UniversalReferenceDataHelper.PaymentDestinationList(Factory, declaration);
				AssertEquals("Count should be 0", 0, list.Count);
			});
		}

		public void TestGetCodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "DeltaIE", euGrouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsImport", "Is For Import", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsExport", "Is For Export", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE);

			var importCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN01", "TRN01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			importCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);
			var exportCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN02", "TRN02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			exportCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);
			var omniDirectionalCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN03", "TRN032", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			omniDirectionalCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);
			omniDirectionalCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);

			Factory.Save();

			CombineAssertions("Transaction Nature list", () =>
			{
				AssertEquals("Transaction Nature list should contain the EU Tran Natures whatever their attributes when no direction specified.", "TRN01, TRN02, TRN03", Factory.GetTranNatureList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE).CodesAsString);
				AssertEquals("List should contain the EU Tran Natures with attributes IsImport ='Y' when specified direction is Import.", "TRN01, TRN03", Factory.GetCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, SharedJobMessageTypeList.Codes.Import).CodesAsString);
				AssertEquals("List should contain the EU Tran Natures with attributes IsExport ='Y' when specified direction is Export.", "TRN02, TRN03", Factory.GetCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, SharedJobMessageTypeList.Codes.Export).CodesAsString);
			});
		}

		public void TestGetDv1Threshold()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateTaxOrFee("DV1", 0, "FR", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "D.V.1 Value", true, 10000);
			Factory.Save();

			AssertEquals(10000m, UniversalReferenceDataHelper.GetDv1Threshold(Factory));
		}

		public void TestGetNationalRateTypes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "AMC");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DEV");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "EXC");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "RED");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "OMR");
			helper.CreateCusRateType(Core.Constants.CountryCodes.France, "OME");

			var listWithoutGrantOfSea = new ZString[] { "AMC", "DEV", "EXC", "RED" };
			var listWithGrantOfSea = new ZString[] { "AMC", "DEV", "EXC", "RED", "OME", "OMR" };

			AssertCusRateType(FRDomesticOverseasTerritories.Codes.CORSE, 4, listWithoutGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.CONTI, 4, listWithoutGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.GUADE, 6, listWithGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.MARTI, 6, listWithGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.GUYAN, 6, listWithGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.MAYOT, 6, listWithGrantOfSea);
			AssertCusRateType(FRDomesticOverseasTerritories.Codes.REUNI, 6, listWithGrantOfSea);

			void AssertCusRateType(ZString region, int countOfCriteria, ZString[] listOfRateType)
			{
				var nationalRateSelectionCriteria = UniversalReferenceDataHelper.GetNationalRateTypes(Factory, region);
				AssertNotNull("CountervailingRateSelectionCriteria", nationalRateSelectionCriteria);
				AssertEquals(countOfCriteria, nationalRateSelectionCriteria.Length);
				AssertContainsExactElementsInAnyOrder(listOfRateType, nationalRateSelectionCriteria.Select(x => x.ZZR_RateType));
			}
		}

		public void TestIsGrantingOfSea()
		{
			CombineAssertions("RegionalGrantingOfSea && ExternalGrantingOfSea are granted of sea type", () =>
			{
			Assert("ExternalGrantingOfSea", UniversalReferenceDataHelper.IsGrantingOfSea(FRConstants.GrantingOfSeaRateTypes.ExternalGrantingOfSea));
			Assert("RegionalGrantingOfSea", UniversalReferenceDataHelper.IsGrantingOfSea(FRConstants.GrantingOfSeaRateTypes.RegionalGrantingOfSea));
			Assert("Other", !UniversalReferenceDataHelper.IsGrantingOfSea("OTH"));
			});
		}

		public void TestIsRegionApplyingGrantingOfSea()
		{
			CombineAssertions("CONTI && CORSE are not granted of sea region", () =>
			{
				Assert("CONTI", !UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.CONTI));
				Assert("CORSE", !UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.CORSE));
				Assert("GUADE", UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.GUADE));
				Assert("MARTI", UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.MARTI));
				Assert("REUNI", UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.REUNI));
				Assert("MAYOT", UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.MAYOT));
				Assert("GUYAN", UniversalReferenceDataHelper.IsRegionApplyingGrantingOfSea(FRDomesticOverseasTerritories.Codes.GUYAN));
			});
		}

		public static void CreatePortTax(Universal.Testing.UniversalReferenceTestDataHelper referenceDataHelper, ZString tHI, ZString designation, ZString port, ZString customsOffice)
		{
			var code1 = referenceDataHelper.CreateNewOrGetExistingCusCodeList("FR", "PTX", $"{port}/{customsOffice}/{tHI}", $"UNLOCO: {port}, Customs Office: {customsOffice}, THI: {tHI}", new ZDateTime(2017, 7, 28), new ZDateTime(2079, 6, 6));
			referenceDataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "THI", tHI, true);
			referenceDataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "Designation", designation, true);
			referenceDataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "PORT", port, true);
			referenceDataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "CUSTOMSOFFICE", customsOffice, true);
		}
	}
}
