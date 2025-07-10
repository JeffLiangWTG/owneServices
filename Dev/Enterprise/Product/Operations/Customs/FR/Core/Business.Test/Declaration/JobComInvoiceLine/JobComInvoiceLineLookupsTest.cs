using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryOfOrigins()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
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
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CountryOfOrigins;
			list.Load();

			AssertType<ZZRefCusCodeListCombinedCollection>("Export Collection Type", list);
			AssertContainsExactElementsInAnyOrder("Country of Origins should be based on CO17 + EU17 + EX17 FR only code lists", new ZString[] { "FR", "QP", "QQ", "QR", "QS", "QX", "IT", "ES", "MQ", "RE" }, list.Select(v => v.ZZD_Code).OrderBy(v => v).ToArray());
		}

		public void TestAdditionalCodesList_TariffAdditionalCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			helper.CreateCusTariffAdditionalCodeCategory(dataGrouping, "SIP");
			var tradeGroup = helper.CreateTradeGroup(dataGrouping, "CN-FR", date1, date2, "STANDARD DEC");
			helper.AddCountry(tradeGroup, "CN");
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "IMP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1000000000", date1, date2, "dummy Description 0");
			var additionalCode1 = helper.CreateTariffAdditionalCodeView(cusTariff, "SEP", "SEP1");
			helper.CreateCusApplicability(additionalCode1, tradeGroup, date1, date2);
			var additionalCode2 = helper.CreateTariffAdditionalCodeView(cusTariff, "SIP", "SIP1", dataGrouping);
			helper.CreateCusApplicability(additionalCode2, tradeGroup, date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "1000000000";
			var additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder("Import : SIP", new ZString[] { "SIP1" }, additionalCodesList.GetAllCodesZString());

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			additionalCodesList = invoiceLine.Lookups.AdditionalCodesList;
			AssertContainsExactElementsInAnyOrder("Export : SEP", new ZString[] { "SEP1" }, additionalCodesList.GetAllCodesZString());
		}

		public void TestPrimaryPreferenceList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.Lookups.PrimaryPreferenceList.Count);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.FrenchGuyana;
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			AssertEquals(1, invoiceLine.Lookups.PrimaryPreferenceList.Count);
			AssertEquals(Core.Constants.Customs.Universal.RefCusPreference.Codes._100, ((CodeDescriptionPairList)invoiceLine.Lookups.PrimaryPreferenceList).CodesAsString);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			AssertEquals(0, invoiceLine.Lookups.PrimaryPreferenceList.Count);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.FrenchGuyana;
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.REUNI;
			AssertEquals(0, invoiceLine.Lookups.PrimaryPreferenceList.Count);
		}

		public void TestCPCList()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM RefDatabase_RefCusProcedure");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.France, "", "10", "00", "000", "Export DeltaG Procedure Group 10", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "21", "00", "000", "Export DeltaG Procedure Group 21", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "22", "00", "000", "Export DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Export, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "31", "00", "000", "Export DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Export, group: "B2");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "01", "00", "000", "Import DeltaG Procedure Group 01", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(CountryCodes.France, "", "02", "00", "000", "Import DeltaG Procedure Group 02", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "07", "00", "000", "Import DeltaIE Procedure Group B1", EU.Business.MessageTypeList.Codes.Import, group: "B1");
			helper.CreateRefCusProcedure(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "", "40", "00", "000", "Import DeltaIE Procedure Group B2", EU.Business.MessageTypeList.Codes.Import, group: "B2");
			Factory.Save();

			CombineAssertions("List should filter on FR Data Grouping,  Message Type, Declaration Application Code and DeclarationType", () =>
			{
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B1", "1000000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Export, "B2", "2100000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B1", "0100000");
				AssertProcedureList(EU.Business.MessageTypeList.Codes.Import, "B2", "0200000");
			});
		}

		void AssertProcedureList(string messageType, string declarationType, string expectedResult)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = declarationType;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			AssertContainsExactElementsInAnyOrder(new string[] { expectedResult }, invoiceLine.Lookups.CPCList.Select(x => x.ZZ6_ProcedureCode + x.ZZ6_PreviousProcedureCode + x.ZZ6_Concession).ToArray());
		}

		public void TestTaxOrFeeDetailEntities()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("FR").PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("FR", "IMP").PK;
			Factory.Save();
			var contiTradeGroupPK = helper.CreateTradeGroup("FR", "CONTI", startDate, endDate, "France continentale").PK;
			var mgpreTradeGroupPK = helper.CreateTradeGroup("FR", "MGPRE", startDate, endDate, "Martinique").PK;
			var redFee = helper.CreateTaxOrFee("RED", 0.02m, currentCountry, description: "Reduced");
			redFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var srrFee = helper.CreateTaxOrFee("SRR", 0.01m, currentCountry, description: "Super Reduced");
			srrFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";
			var nilFee = helper.CreateTaxOrFee("NIL", 9999m, currentCountry, description: "Free");
			nilFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";

			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "RED", startDate: startDate, endDate: endDate, additionalCode: "V910", category: UniversalReferenceConstants.RefCusRateCodes.A445, tradeGroup: contiTradeGroupPK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "RED", startDate: startDate, endDate: endDate, additionalCode: "V910", category: UniversalReferenceConstants.RefCusRateCodes.A445, tradeGroup: mgpreTradeGroupPK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "NIL", startDate: startDate, endDate: endDate, additionalCode: "V911", tradeGroup: contiTradeGroupPK);

			helper.CreateCusCodeType("ADDIN", "Additional Code");
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V910", "Test Additional Code 1", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V911", "Test Additional Code 1", startDate: startDate, endDate: endDate);

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_RegionOrTerritoryOfDestination = "CONTI";
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;
			AssertEquals("Entity related to region MGPRE should be skipped.", 2, taxOrFeeDetailEntityList.Count);
			Assert(taxOrFeeDetailEntityList.Any(x => x.VATCode == "RED" && x.Description == "Reduced, V910, A445, 2.00%" && x.AdditionalCode == "V910" && x.Category == UniversalReferenceConstants.RefCusRateCodes.A445));
			Assert(taxOrFeeDetailEntityList.Any(x => x.VATCode == "NIL" && x.Description == "Free, V911, 999900.00%" && x.AdditionalCode == "V911" && x.Category == ""));
		}

		public void TestInvoiceLine()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			var eu1 = helper.CreateCusCodeList("EUN", "CUSUQ", "EU1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var fr1 = helper.CreateCusCodeList("FR", "CUSUQ", "FR1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var fr2 = helper.CreateCusCodeList("FR", "CUSUQ", "FR2", ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(3));
			var fr3 = helper.CreateCusCodeList("FR", "CUSUQ", "FR3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			var us1 = helper.CreateCusCodeList("US", "CUSUQ", "US1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456789012";

			var customsUQList = invoiceLine.Lookups.CustomsUQList;
			AssertEquals(2, customsUQList.Count);
			Assert("Should not load parent codes", !customsUQList.ContainsCode("EU1"));
			Assert("valid code", customsUQList.ContainsCode("FR1"));
			Assert("valid code", customsUQList.ContainsCode("FR2"));
			Assert("should not load expired codes", !customsUQList.ContainsCode("FR3"));
			Assert("should not load codes from other country.", !customsUQList.ContainsCode("US1"));

			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(2);
			customsUQList = invoiceLine.Lookups.CustomsUQList;
			AssertEquals(1, customsUQList.Count);
			Assert("Should not load parent codes", !customsUQList.ContainsCode("EU1"));
			Assert("invalid code because FR1 is not active on issue date", !customsUQList.ContainsCode("FR1"));
			Assert("valid code", customsUQList.ContainsCode("FR2"));
			Assert("should not load expired codes", !customsUQList.ContainsCode("FR3"));
			Assert("should not load codes from other country.", !customsUQList.ContainsCode("US1"));
		}

		public void TestTariffBypassCodeLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var lookups = invoiceLine.AddInfoLookups.TariffBypassCodeList;

			AssertEquals("Should be 4 Tariff Bypass Codes Lookups", 4, lookups.Count);
			Assert("Should contain valid code", lookups.ContainsOnly("D", "E", "G", "I"));
		}

		public void TestTaxOrFeeCodeList()
		{
			SetUpTaxOrFee();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var lookups = invoiceLine.Lookups.TaxOrFeeCodeList;
			AssertEquals("List Without Other Type.", 3, lookups.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { UniversalReferenceConstants.RefCusRateCodes.A10, UniversalReferenceConstants.RefCusRateCodes.A20, "V10" }, lookups.GetAllCodes());

			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "0304798000";
			lookups = invoiceLine.Lookups.TaxOrFeeCodeList;
			AssertEquals("List Without Other Type and within VATApplicability.", 3, lookups.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { UniversalReferenceConstants.RefCusRateCodes.A10, UniversalReferenceConstants.RefCusRateCodes.A20, "V10" }, lookups.GetAllCodes());

			declaration.JE_RegionOrTerritoryOfDestination = "CORSE";
			lookups = invoiceLine.Lookups.TaxOrFeeCodeList;
			AssertEquals("List Without Other Type and within VATApplicabilityWithTradeGroup Mapping.", 1, lookups.Count);
			AssertEquals("VAT type for Corsica.", "V10", lookups[0].Code);
			AssertEquals("VAT Description.", "8.50%", lookups[0].Description);

			declaration.JE_RegionOrTerritoryOfDestination = "MARTI";
			lookups = invoiceLine.Lookups.TaxOrFeeCodeList;
			AssertEquals("List Without Other Type and within VATApplicabilityWithTradeGroup.", 1, lookups.Count);
			AssertEquals("VAT type for Martinique.", UniversalReferenceConstants.RefCusRateCodes.A20, lookups[0].Code);
			AssertEquals("VAT Description.", "5.15%", lookups[0].Description);
		}

		public void TestTaxOrFeeCodeList_StandAloneCommercialInvoices()
		{
			SetUpTaxOrFee();

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = standAloneInvoice.InvoiceLines.AddNew();
			AssertNull("Prerequisite: Not attached to any declaration.", invoiceLine.Declaration);

			CodeDescriptionPairList lookups = null;
			AssertNoExceptionThrown(() =>
			{
				lookups = invoiceLine.Lookups.TaxOrFeeCodeList;
			});

			AssertContainsExactElementsInAnyOrder(new ZString[] { UniversalReferenceConstants.RefCusRateCodes.A10, UniversalReferenceConstants.RefCusRateCodes.A20, "V10" }, lookups.GetAllCodes());
		}

		void SetUpTaxOrFee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusTaxOrFeeType("OTH");
			helper.CreateRefCusTaxOrFeeType("ABC");
			helper.CreateRefCusTaxOrFeeType("VAT");
			Factory.Save();

			var othFee = helper.CreateTaxOrFee("123", 5, Core.Constants.CountryCodes.France);
			othFee.ZZF_ZX0_NKTaxOrFeeType = "OTH";
			var abcFee = helper.CreateTaxOrFee(UniversalReferenceConstants.RefCusRateCodes.A10, 0.10, Core.Constants.CountryCodes.France);
			abcFee.ZZF_ZX0_NKTaxOrFeeType = "ABC";
			var abcFee2 = helper.CreateTaxOrFee(UniversalReferenceConstants.RefCusRateCodes.A20, 0.0515, Core.Constants.CountryCodes.France);
			abcFee2.ZZF_ZX0_NKTaxOrFeeType = "ABC";
			var vatFee = helper.CreateTaxOrFee("V10", 0.085, Core.Constants.CountryCodes.France);
			vatFee.ZZF_ZX0_NKTaxOrFeeType = "VAT";

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", parentDataGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusRateCodes.A10);

			var tradeGroup1 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, "METRO");
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "V10", tradeGroup: tradeGroup1.PK);

			var tradeGroup2 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, "MARTI");
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusRateCodes.A20, tradeGroup: tradeGroup2.PK);

			Factory.Save();
		}

		public void TestFRAddInRefCusCodeListWithCategoryDCCAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "additional info");
			helper.CreateCusCodeList("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "70000", "doc 70000", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeList("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "80000", "doc 80000", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9101", "doc I9101", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "I9104", "doc I9104", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"I9101", "I9104"
			}, invoiceLine.Lookups.FRAddInRefCusCodeListWithCategoryDCCAttribute);
		}

		[TestDate(2023, 1, 31)]
		public void TestSupplementaryCodeList()
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("FR").PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("FR", "IMP").PK;
			Factory.Save();

			var asiaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6));
			helper.AddCountry(asiaTradeGroup, Core.Constants.CountryCodes.China);

			var metroTradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, FRLocalGroups.Codes.METRO);
			var dpdomTradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, FRLocalGroups.Codes.DPDOM);
			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Alpha Bravo", compositeKey: "99...99.99");
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DTY");
			var rateCode = helper.CreateCusRateCode(Factory, "A00", rateType.PK);
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "VDF * 10%");
			helper.CreateCusApplicability(rate1, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q003", secondTradeGroup: metroTradeGroup);
			helper.CreateCusApplicability(rate1, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q004", secondTradeGroup: metroTradeGroup);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "VDF * 5%");
			helper.CreateCusApplicability(rate2, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q234", secondTradeGroup: dpdomTradeGroup);
			helper.CreateCusApplicability(rate2, asiaTradeGroup, ZDateTime.BrettsBirthday, new ZDateTime(2079, 6, 6), "Q235", secondTradeGroup: dpdomTradeGroup);

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.MARTI;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "FR";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;

			AssertContainsExactElementsInAnyOrder("MARTI region declaration should contain DPDOM codes", new ZString[] { "Q234", "Q235" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());

			declaration.JE_RegionOrTerritoryOfDestination = FRDomesticOverseasTerritories.Codes.CONTI;
			AssertContainsExactElementsInAnyOrder("CONTI region declaration should contain METRO codes", new ZString[] { "Q003", "Q004" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());
		}

		[TestDate(2023, 1, 31)]
		public void TestSupplementaryCodeListPrunedOfUselessVATCanas()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping(currentCountry).PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType(currentCountry, "IMP").PK;

			helper.CreateCusCodeType("ADDIN", "Additional Code");
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V001", "RED Additional Code 1", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V002", "RED Additional Code 2", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V011", "NIL Additional Code 1", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V012", "NIL Additional Code 2", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V910", "SRD Additional Code 1", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "V911", "SRD Additional Code 2", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "C920", "Control Additional Code 1", startDate: startDate, endDate: endDate);
			helper.CreateCusCodeList(currentCountry, "ADDIN", "C921", "Control Additional Code 2", startDate: startDate, endDate: endDate);
			Factory.Save();

			var contiTradeGroup = helper.CreateTradeGroup(currentCountry, "CONTI", startDate, endDate, "France continentale");
			var chineseTradeGroup = helper.CreateTradeGroup(currentCountry, "CN", startDate, endDate, "China");
			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");

			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "RED", startDate: startDate, endDate: endDate, additionalCode: "V001", category: UniversalReferenceConstants.RefCusRateCodes.A465, tradeGroup: contiTradeGroup.PK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "RED", startDate: startDate, endDate: endDate, additionalCode: "V002", category: UniversalReferenceConstants.RefCusRateCodes.A465, tradeGroup: contiTradeGroup.PK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "STD", startDate: startDate, endDate: endDate, category: UniversalReferenceConstants.RefCusRateCodes.A445, tradeGroup: contiTradeGroup.PK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "NIL", startDate: startDate, endDate: endDate, additionalCode: "V011", tradeGroup: contiTradeGroup.PK);
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "NIL", startDate: startDate, endDate: endDate, additionalCode: "V012", tradeGroup: contiTradeGroup.PK);

			var vatConditionType = helper.CreateOrGetExistingRefCusConditionType(currentCountry, "CLASS", "VAT", "Condition for VAT");
			var controlConditionType = helper.CreateOrGetExistingRefCusConditionType(currentCountry, "CLASS", "CTRL", "Control Condition");
			var vatCondition1 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition1, chineseTradeGroup, startDate, endDate, "V001");
			var vatCondition2 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition2, chineseTradeGroup, startDate, endDate, "V002");
			var vatCondition3 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition3, chineseTradeGroup, startDate, endDate, "V011");
			var vatCondition4 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition4, chineseTradeGroup, startDate, endDate, "V012");
			var vatCondition5 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition5, chineseTradeGroup, startDate, endDate, "V910");
			var vatCondition6 = helper.CreateOrGetExistingRefCusCondition(currentCountry, vatConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(vatCondition6, chineseTradeGroup, startDate, endDate, "V911");
			var otherCondition1 = helper.CreateOrGetExistingRefCusCondition(currentCountry, controlConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(otherCondition1, chineseTradeGroup, startDate, endDate, "C920");
			var otherCondition2 = helper.CreateOrGetExistingRefCusCondition(currentCountry, controlConditionType.PK, tariff.PK, "", true, false, startDate, endDate);
			helper.CreateCusApplicability(otherCondition2, chineseTradeGroup, startDate, endDate, "C921");

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RegionOrTerritoryOfDestination = "CONTI";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_Tariff = "99999999";

			invoiceLine.JI_ZZF_NKTaxType = "RED";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "V001", "V002", "C920", "C921" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());

			invoiceLine.JI_ZZF_NKTaxType = "NIL";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "V011", "V012", "C920", "C921" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());

			invoiceLine.JI_ZZF_NKTaxType = "STD";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "C920", "C921" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodes());
		}
	}
}
