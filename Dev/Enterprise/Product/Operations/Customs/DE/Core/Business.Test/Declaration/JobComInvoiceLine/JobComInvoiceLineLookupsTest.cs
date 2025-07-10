using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOriginFederalStateList_JI_CountryOfOriginIsDE()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var list = lookups.StateOrRegionOfOriginList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16", list.CodesAsString);
				AssertSame("Cached", list, lookups.StateOrRegionOfOriginList);
			});
		}

		public void TestOriginFederalStateList_JI_CountryOfOriginIsNotDE()
		{
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			var list = lookups.StateOrRegionOfOriginList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "99", list.CodesAsString);
				AssertSame("JI_CountryOfOrigin is empty, cached", list, lookups.StateOrRegionOfOriginList);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
				AssertSame("JI_CountryOfOrigin is 'AU', same cache", list, lookups.StateOrRegionOfOriginList);
			});
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "de", eun);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "2", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "3", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "4", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var list = lookups.CustomsUQList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 2", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsUQList);
			});
		}

		public void TestBondedWhsUnitQtyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Unit Quantities");

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "de", eun);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "22", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "333", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "4444", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "555", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var list = lookups.BondedWhsUnitQtyList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "1, 22, 333, 4444", list.CodesAsString);
				AssertSame("Cached", list, lookups.BondedWhsUnitQtyList);
			});
		}

		public void TestCPCList_Import()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "10", "11", "111", "One", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "B", "10", "22", "222", "Two", "IMP", group: "IFD");
			helper.CreateRefCusProcedure(currentCountry, "C", "10", "33", "333", "Three", "EXP", group: "ICR");
			helper.CreateRefCusProcedure(currentCountry, "D", "20", "44", "444", "Four", "IMP", group: "IFD");

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
				var cpcList = invoiceLine.Lookups.CPCList;
				AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 3, cpcList.Count);
				AssertEquals("CPC List does not contain default filter for CPC", false, cpcList.FilterBusinessObjectDefaults.ContainsDefaultFor("CPC:Property"));

				entryInstruction.CEI_Procedure = "10";
				cpcList = invoiceLine.Lookups.CPCList;
				var filterProperty = cpcList.FilterBusinessObjectDefaults["CPC:Property"];
				AssertEquals("Filter value for CPC", "10", filterProperty.Value);
				AssertEquals("Property Not removable", false, filterProperty.IsRemovable);
			});
		}

		public void TestCPCList_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "10", "00", "F61", "Description for 1000F61", JobMessageTypeList.Codes.Export, "0000****,10000*00,11000000,12000000,20000000");
			var procedure2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "21", "00", "B53", "Description for 2100B53", JobMessageTypeList.Codes.Export, "0020****");
			var procedure3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "21", "48", "B53", "Description for 2148B53", JobMessageTypeList.Codes.Export, "**1*****");
			var procedure4 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "22", "00", "B53", "Description for 2200B53", JobMessageTypeList.Codes.Export, "**11****,**12****");
			Factory.Save();

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				instruction.CEI_SubStyle = "11";
				instruction.CEI_Style = "000000";
				Assert("Matched SubStyle + Style", procedure1.MatchesFilter(lookups.CPCList.CompleteFilter));

				instruction.CEI_SubStyle = "10";
				instruction.CEI_Style = "000900";
				Assert("Matched SubStyle + Style.SubstringSafe(0, 3)", procedure1.MatchesFilter(lookups.CPCList.CompleteFilter));

				instruction.CEI_SubStyle = "00";
				instruction.CEI_Style = "200100";
				Assert("Matched SubStyle + Style.SubstringSafe(0, 2)", procedure2.MatchesFilter(lookups.CPCList.CompleteFilter));

				instruction.CEI_Style = "110100";
				Assert("Matched Style.SubstringSafe(0, 1)", procedure3.MatchesFilter(lookups.CPCList.CompleteFilter));

				Assert("Matched Style.SubstringSafe(0, 2)", procedure4.MatchesFilter(lookups.CPCList.CompleteFilter));

				AssertEquals("JE_EntryStyle <> 'CO'", false, lookups.CPCList.FilterBusinessObjectDefaults.ContainsDefaultFor("Procedure Code:Property"));

				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				AssertNoExceptionThrown("No instruction", () =>
				{
					_ = lookups.CPCList;
				});
			});
		}

		public void TestCPCList_ExportPatterns()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			instruction.CEI_SubStyle = "11";
			instruction.CEI_Style = "000000";
			var lookups = new JobComInvoiceLineLookupsForTest(invoiceLine);

			var patterns = lookups.GroupPatternsExposed;

			Assert("Pattern length should be 8", patterns.All(x => x.Length == 8));
		}

		public void TestCPCList_Export_EntryStyleCO()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;

			AssertPropertyAndComparisonOperatorDefaultAndNotRemovable("Procedure Code:Property", CustomsProcedureCodeList.Export.ProcedureCode._10, "Procedure Code:ComparisonOperator", ModuleTextFilter.ComparisonConstants.StartsWith);
		}

		public void TestCustomsEntryInstructions_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AddCustomsEntryInstruction(ImportDeclarationTypeList.Codes.EGN, ImportSubStyleList.Codes.A);
			AddCustomsEntryInstruction(ImportDeclarationTypeList.Codes.AAV, ImportSubStyleList.Codes.B);
			AddCustomsEntryInstruction(ImportDeclarationTypeList.Codes.LUZ, ImportSubStyleList.Codes.C);
			AddCustomsEntryInstruction(ImportDeclarationTypeList.Codes.EGZ, ImportSubStyleList.Codes.D);
			var list = lookups.CustomsEntryInstructions;

			CombineAssertions(() =>
			{
				AssertEquals("Sorted by code", "AAV, EGN, EGZ, LÜZ", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsEntryInstructions);
			});
		}

		public void TestCustomsEntryInstructions_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AddCustomsEntryInstruction(ExportDeclarationTypeProcedureList.Codes._000100, ExportDeclarationTypeTimeList.Codes._10);
			AddCustomsEntryInstruction(ExportDeclarationTypeProcedureList.Codes._000100, ExportDeclarationTypeTimeList.Codes._00);
			AddCustomsEntryInstruction(ExportDeclarationTypeProcedureList.Codes._000100, ExportDeclarationTypeTimeList.Codes._11);
			AddCustomsEntryInstruction(ExportDeclarationTypeProcedureList.Codes._000210, ExportDeclarationTypeTimeList.Codes._00);
			var list = lookups.CustomsEntryInstructions;

			CombineAssertions(() =>
			{
				AssertEquals("Sorted by code", "00|000100, 00|000210, 10|000100, 11|000100", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsEntryInstructions);
			});
		}

		public void TestExportCountryList()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var list = lookups.ExportCountryList;
			AssertEquals("All countries", 253, list.Count);
		}

		public void TestExportCountryList_Export()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				var list = lookups.ExportCountryList;

				var countryCodes = string.Join(", ", list.Select(x => x.RN_Code).ToArray());
				AssertContains("EU country", Core.Constants.CountryCodes.Germany, countryCodes);

				AssertNotContains("Not EU country", Core.Constants.CountryCodes.Tajikistan, countryCodes);
			});
		}

		public void TestPrimaryPreferenceList_Import()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(Factory);

			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var tradeGroup = testHelper.CreateTradeGroup(dataGrouping, "TradeGroupTest", date1, date2);
			testHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Botswana, date1, date2);

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = testHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			testHelper.CreatePreferenceForCountry("MFN", "MFN", dataGrouping);
			var cusTariff = testHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "123456789", date1, date2, "dummy Description 0");
			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroup, date1, date2, "add11", "ord11");
			var testRate2 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceRED.PK);
			testHelper.CreateCusApplicability(testRate2, tradeGroup, date1, date2, "add21", "ord21");

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "123456789";

			CombineAssertions(() =>
			{
				AssertEquals("JI_CountryOfOrigin and ZG_CountryOfSupply are empty", "MFN, RED, STD", ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
				AssertEquals("Match country using JI_CountryOfOrigin", "RED, STD", ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);

				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.AlandIslands;
				AssertEquals("No match country using JI_CountryOfOrigin", string.Empty, ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Botswana;
				AssertEquals("Match country using ZG_CountryOfSupply", "RED, STD", ((CodeDescriptionPairList)lookups.PrimaryPreferenceList).CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			lookups = new JobComInvoiceLineLookups(invoiceLine);
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLineLookups lookups;
		JobComInvoiceHeader invoiceHeader;

		void AssertPropertyAndComparisonOperatorDefaultAndNotRemovable(ZString propertyName, ZString propertyValue, ZString comparisonOperatorName, ZString comparisonOperatorValue)
		{
			var cpcList = invoiceLine.Lookups.CPCList;
			CombineAssertions(() =>
			{
				var filterProperty = cpcList.FilterBusinessObjectDefaults[propertyName];
				AssertEquals("PropertyFilter value", propertyValue, filterProperty.Value);
				AssertEquals("PropertyFilter isn't removable", false, filterProperty.IsRemovable);

				var filterComparisonOperator = cpcList.FilterBusinessObjectDefaults[comparisonOperatorName];
				AssertEquals("ComparisonOperatorFilter value", comparisonOperatorValue, filterComparisonOperator.Value);
				AssertEquals("ComparisonOperatorFilter isn't removable", false, filterComparisonOperator.IsRemovable);
			});
		}

		void AddCustomsEntryInstruction(ZString style, ZString substyle)
		{
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = style;
			entry.CEI_SubStyle = substyle;
		}

		class JobComInvoiceLineLookupsForTest : JobComInvoiceLineLookups
		{
			public JobComInvoiceLineLookupsForTest(JobComInvoiceLine parent) : base(parent)
			{
			}

			public IEnumerable<string> GroupPatternsExposed => this.GroupPatterns;
		}
	}
}
