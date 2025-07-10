using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNDGSubs()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var iatSubstance = Factory.New<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			var imoSubstance = Factory.New<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubs = invoiceLine.Lookups.UNDGSubs;
			AssertNull("Non-IMO substance should not be found", undgSubs.FindByPK(iatSubstance.PK));
			AssertNotNull("IMO substance should be found", undgSubs.FindByPK(imoSubstance.PK));
		}

		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestCustomsEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction2.PK }, invoiceLine.Lookups.CustomsEntryInstructions.Cast<CusEntryInstruction>().Select(x => x.PK));
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK, instruction2.PK }, invoiceLine.Lookups.CustomsEntryInstructions.Cast<CusEntryInstruction>().Select(x => x.PK));
			instruction2.CEI_CEI_Parent = instruction1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.PK }, invoiceLine.Lookups.CustomsEntryInstructions.Cast<CusEntryInstruction>().Select(x => x.PK));
		}

		public void TestPrimaryPreferenceList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry("NORMAL", "NORMAL", Core.Constants.CountryCodes.China);
			helper.CreatePreferenceForCountry("MFN", "MFN", Core.Constants.CountryCodes.China);
			helper.CreatePreferenceForCountry("FTA", "FTA", Core.Constants.CountryCodes.China);
			helper.CreatePreferenceForCountry("LDC", "LDC", Core.Constants.CountryCodes.China);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var preferenceList = invoiceLine.Lookups.PrimaryPreferenceList as CodeDescriptionPairList;
			AssertContainsExactElementsInAnyOrder(new[] { "FTA", "LDC", "MFN", "NORMAL" }, preferenceList.GetAllCodes());
		}

		public void TestTradeAgreementCodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CNPTA", "CN Prefential Trade Agreement");
			var cnpta_01 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "01", "01", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var cnpta_02 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "02", "02", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var cnpta_03 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CNPTA", "03", "03", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_01.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_02.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.NewZealand);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cnpta_03.PK, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, Core.Constants.CountryCodes.UnitedKingdom);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.LeastDevelopedCountries;
			var lookups = invoiceLine.Lookups;
			AssertContainsExactElementsInAnyOrder(new[] { "13" }, lookups.TradeAgreementCodeList.GetAllCodes());
			invoiceLine.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertContainsExactElementsInAnyOrder(new[] { "01" }, lookups.TradeAgreementCodeList.GetAllCodes());
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			AssertContainsExactElementsInAnyOrder(new[] { "02" }, lookups.TradeAgreementCodeList.GetAllCodes());
			invoiceLine.CertificateOfOriginCountry = Core.Constants.CountryCodes.Australia;
			AssertContainsExactElementsInAnyOrder("Should use EffectiveCountryOfOrigin when getting list.", new[] { "01" }, lookups.TradeAgreementCodeList.GetAllCodes());

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			AssertContainsExactElementsInAnyOrder(new[] { "03" }, lookups.TradeAgreementCodeList.GetAllCodes());
		}

		public void TestEndUseList()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(24, invoiceLine.Lookups.EndUseList.Count);
			Assert("End use list should not be translatable", invoiceLine.Lookups.EndUseList is UntranslatableCodeDescriptionPairList);
		}

		[TestDate(2016, 5, 5)]
		public void TestDutyModeList()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "EXP";
			AssertEquals(9, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("1"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("2"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("3"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("4"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("5"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("6"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("7"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("8"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("9"));
			testItems.JobDeclaration.JE_MessageType = "IMP";
			AssertEquals(8, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(!testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("9"));
			testItems.EntryInstruction.CEI_LevyType = "101";
			AssertEquals(4, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("1"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("4"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("6"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("7"));
			testItems.EntryInstruction.CEI_LevyType = "423";
			AssertEquals(5, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("1"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("3"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("4"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("6"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("7"));
			testItems.EntryInstruction.CEI_LevyType = "501";
			AssertEquals(2, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("3"));
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("4"));
			testItems.EntryInstruction.CEI_LevyType = "888";
			AssertEquals(1, testItems.InvoiceLine.Lookups.DutyModes.Count);
			Assert(testItems.InvoiceLine.Lookups.DutyModes.ContainsCode("4"));
		}

		[TestDate(2016, 5, 5)]
		public void TestCIQTariffList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			var ciqTariffTypeUS = helper.CreateNewOrGetExistingTariffType("US", "CIQ");
			var xxxTariffType = helper.CreateNewOrGetExistingTariffType("CN", "XXX");
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			Factory.Save();
			var ciqTariff1 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff2 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 5, 5));
			var ciqTariffUS = helper.LoadOrCreateNewTariff("US", ciqTariffTypeUS.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var xxxTariff = helper.LoadOrCreateNewTariff("CN", xxxTariffType.PK, "10000012999", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000012", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateTariffRelationship(ciqTariff1.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff2.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariffUS.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(xxxTariff.PK, cusTariffType.PK, "10000012");
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.InvoiceLine.JI_Tariff = "10000012";
			var testList = testItems.InvoiceLine.Lookups.CIQTariffList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<TariffView>().Any(tariff => tariff.ZZ1_TariffCode == "10000012001"));
			Assert(testList.Cast<TariffView>().Any(tariff => tariff.ZZ1_TariffCode == "10000012002"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Tariff Code:Property"];
			AssertEquals("10000012", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestCIQOriginStateList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CIQST", "CIQST");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101001", "CIQ State1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101002", "CIQ State2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQST", "101003", "CIQ State3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.CountryOfOrigin.RN_IsoNumericUNM49Code = "840";
			var testList = invoiceLine.Lookups.CIQOriginStateList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101001"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101002"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("840", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestOrigDistrictList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "EXP";
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.JI_OriginRegion = "101011";
			var testList = invoiceLine.Lookups.OrigDistrictList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10101"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10102"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Origin Region", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestDestDistrictList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "IMP";
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.JI_DestinationRegion = "101011";
			var testList = invoiceLine.Lookups.DestDistrictList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10101"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "10102"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Destination Region", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestOrigRegionList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "EXP";
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.JI_OriginDistrict = "10101";
			var testList = invoiceLine.Lookups.OrigRegionList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101011"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101022"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Origin District", "1010", listTypeFilter.Value);
		}

		[TestDate(2016, 5, 5)]
		public void TestDestRegionList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("DISTR", "District");
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10101", "District1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10102", "District2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "DISTR", "10103", "District3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			helper.CreateNewOrGetExistingCusCodeType("CIQDT", "Region");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101011", "Region1", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101022", "Region2", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList("CN", "CIQDT", "101033", "Region3", new ZDateTime(2016, 5, 3), new ZDateTime(2016, 5, 4));
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "IMP";
			var invoiceLine = testItems.InvoiceLine;
			invoiceLine.JI_DestinationDistrict = "10101";
			var testList = invoiceLine.Lookups.DestRegionList;
			testList.Load();
			AssertEquals(2, testList.Count);
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101011"));
			Assert(testList.Cast<ZZRefCusCodeListCombined>().Any(code => code.ZZD_Code == "101022"));
			var filters = testList.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["Code:Property"];
			AssertEquals("The code filter should start with the first 4 digits of Destination District", "1010", listTypeFilter.Value);
		}

		public void TestUNDGPackageTypes()
		{
			var fullList = Factory.GetCachedValue<UNDGPackageTypeList>();
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save()).InvoiceLine;
			var testList = testItem.Lookups.UNDGPackageTypes;
			AssertEquals("UNDGPackageTypeList Count", fullList.Count, testList.Count);
			Assert("UNDGPackageTypeList Type", testList is UntranslatableCodeDescriptionPairList);
			AssertEquals("UNDGPackageTypeList Descriptions", "\u94a2\u5236\u4e0d\u53ef\u62c6\u88c5\u6876\u9876\u5706\u6876", testList.GetDescriptionFromCode("1A1"));
		}

		public void TestShowDescriptionFilterAsDefault()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory).InvoiceLine;
			var filterDestDistrictList = testItem.Lookups.DestDistrictList.FilterBusinessObjectDefaults["Description:Property"];
			AssertNotNull(filterDestDistrictList);
			var filterDestRegionList = testItem.Lookups.DestRegionList.FilterBusinessObjectDefaults["Description:Property"];
			AssertNotNull(filterDestRegionList);
			var filterCIQTariffList = testItem.Lookups.CIQTariffList.FilterBusinessObjectDefaults["Description (Default Language):Property"];
			AssertNotNull(filterCIQTariffList);
			var filterCIQOriginStateList = testItem.Lookups.CIQOriginStateList.FilterBusinessObjectDefaults["Description:Property"];
			AssertNotNull(filterCIQOriginStateList);
		}

		public void TestConfirmationTypeList()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save()).InvoiceLine;
			var testList = testItem.Lookups.ConfirmationTypeList;
			AssertEquals("Should only have 2 items.", 2, testList.Count);
			Assert("Should have excluded 9.", !testList.ContainsCode(ConfirmationTypeList.Codes.Uncertain));
			var testItem2 = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save()).InvoiceLine;
			AssertSame("Should have cached.", testList, testItem2.Lookups.ConfirmationTypeList);
		}
	}
}
