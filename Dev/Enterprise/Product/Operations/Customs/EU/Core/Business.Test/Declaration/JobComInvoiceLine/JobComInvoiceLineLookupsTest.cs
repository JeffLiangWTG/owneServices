using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class BaseJobComInvoiceLineLookupsTest : TestCaseWithFactory
	{
		public void TestNationalAdditionalCodeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var codeList = invoiceLine.Lookups.NationalAdditionalCodeList;

			AssertEquals("NationalAdditionalCodeList", "", codeList.CodesAsString);
		}

		public void TestCustomsUQListNoDeclaration()
		{
			SetupCustomsUQReferenceData();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("No Declaration falls back to logged in company and Todays date", "ABC, DEF, JKL", invoiceLine.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestCustomsUQListNoDateOfValuation()
		{
			SetupCustomsUQReferenceData();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("No EntryHeader for valuation date falls back to todays date", "ABC, DEF, JKL", invoiceLine.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestCustomsUQRemainsSealed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var lookups = invoiceLine.Lookups;

			var customsUQListPropertyInfo = lookups.GetType().GetProperty("CustomsUQList");
			Assert("CustomsUQList is made sealed. Please do not remove it in order to be able to override it. Declaration.GetDefaultDataGroupingCode will return a country and the query used will take care of retrieving EU ones in addition to the country-specific ones", customsUQListPropertyInfo.GetMethod.IsFinal);
		}

		public void TestCustomsUQListWithDateOfValuation()
		{
			SetupCustomsUQReferenceData();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456789012";
			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Today.AddMonths(2);
			AssertEquals("EntryHeader for valuation used", "DEF, JKL", invoiceLine.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestCustomsUQListWithDataGroupCode()
		{
			SetupCustomsUQReferenceData();
			var declaration = Factory.New<JobDeclarationForTest>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456789012";
			entry.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
			AssertEquals("Data Group Code used", "GHI", invoiceLine.Lookups.CustomsUQList.CodesAsString);
		}

		void SetupCustomsUQReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("DGC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();
		}

		class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString DefaultDataGroupingCore => "DGC";
		}
	}

	public class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxOrFeeDetailEntities()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
			Factory.Save();

			helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
			helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");
			helper.CreateTaxOrFee("VT3", 9999m, currentCountry, description: "VAT Three");

			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate, endDate: endDate, additionalCode: "Z001");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate, endDate: endDate);

			helper.CreateCusCodeType("ADDIN", "Additional Code");
			helper.CreateCusCodeList(currentCountry, "ADDIN", "Z001", "Test Additional Code 1", startDate: startDate, endDate: endDate);

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;
			Assert(taxOrFeeDetailEntityList.Any(x => x.VATCode == "VT1" && x.Description == "VAT One, Z001, 2.00%" && x.AdditionalCode == "Z001"));
			Assert(taxOrFeeDetailEntityList.Any(x => x.VATCode == "VT2" && x.Description == "VAT Two, 1.00%" && x.AdditionalCode == ""));
		}

		public void TestBondedWhsUnitQtyList()
		{
			SetupCustomsUQReferenceData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var bondedWhsUnitQtyList1 = invoiceLine.Lookups.BondedWhsUnitQtyList;
			var bondedWhsUnitQtyList2 = invoiceLine.Lookups.BondedWhsUnitQtyList;
			AssertSame("Should have cahced value", bondedWhsUnitQtyList1, bondedWhsUnitQtyList2);

			AssertEquals(2, invoiceLine.Lookups.BondedWhsUnitQtyList.Count);
			AssertEquals("ABC", invoiceLine.Lookups.BondedWhsUnitQtyList[0].Code);
			AssertEquals("DEF", invoiceLine.Lookups.BondedWhsUnitQtyList[1].Code);
		}

		void SetupCustomsUQReferenceData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("DGC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			Factory.Save();
		}

		public void TestSupervisingOfficeList()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNotNull(invoiceLine.Lookups.SupervisingOfficeList);
		}

		public void TestInvoiceLine()
		{
			JobComInvoiceLine parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestGetNewPartCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertType(typeof(EuOrgSupplierPartCollection), invoiceLine.Lookups.PartsList);
		}

		public void TestCPCList()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IFD");
			var procedure3 = helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "ICR");
			var procedure4 = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");

			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IFD";
			dec.JE_MessageType = "IMP";

			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invLine.JI_RN_NKCountryOfExport = GlbCompany.CurrentCompany.Country.Code;
			invLine.JI_CEI = cei.PK;
			AssertEquals("JI_CEI automatically set when assigning JE_DeclarationType", cei.PK, invLine.JI_CEI);
			AssertEquals(null, invLine.CusProcedure);
			var cpcs = invLine.Lookups.CPCList;
			AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 2, cpcs.Count);
			Assert("CPC List", cpcs.Contains(procedure1));
			Assert("CPC List", cpcs.Contains(procedure2));
		}

		public void TestValuationCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = new JobComInvoiceLineLookups(invoiceLine);

			CombineAssertions(() =>
			{
				AssertSame("Cached", lookups.ValuationCodeList, lookups.ValuationCodeList);
				AssertEquals("CodesAsString", "1, 2, 3, 4, 5, 6", lookups.ValuationCodeList.CodesAsString);
			});
		}

		public void TestTaxOrFeeCodeList()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusVATApplicability");
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusTariff");
			Db.Connection.ExecuteNonQuery("delete from RefDatabase_RefCusTariffType");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
			Factory.Save();

			helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
			helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");
			helper.CreateTaxOrFee("VT3", 9999m, currentCountry, description: "VAT Three");

			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate, endDate: endDate, additionalCode: "Z001");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate, endDate: endDate);

			helper.CreateCusCodeType("ADDIN", "Additional Code");
			helper.CreateCusCodeList(currentCountry, "ADDIN", "Z001", "Test Additional Code 1", startDate: startDate, endDate: endDate);

			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeCodeList = invoiceLine.Lookups.TaxOrFeeCodeList;

			AssertContainsExactElementsInAnyOrder(new ZString[] { "VT1", "VT2" }, taxOrFeeCodeList.ToArray().Select(x => $"{x.Code}"));

			var vat1DescriptionListItem = taxOrFeeCodeList["VT1"];
			AssertEquals($"VAT with code [VT1], Description", "VAT One - Z001 - Test Additional Code 1", vat1DescriptionListItem.Description);

			var vat2DescriptionListItem = taxOrFeeCodeList["VT2"];
			AssertEquals($"VAT with code [VT2], Description", "VAT Two", vat2DescriptionListItem.Description);
		}

		public void TestCountryOfOrigins()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ZZZ_ZZZ_Grouping = eun.PK;
			var group1011 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var group1012 = helper.CreateTradeGroup(Core.Constants.CountryCodes.Australia, "1012", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(group1011, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1011, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(group1012, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = invoiceLine.Lookups;
			var countryOfOriginsList = lookups.CountryOfOrigins as CodeDescriptionPairList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", countryOfOriginsList, lookups.CountryOfOrigins);
				AssertEquals("CodesAsString", "DE, FR", countryOfOriginsList?.CodesAsString);
			});
		}

		public void TestConsignorList()
		{
			var lookups = Factory.New<JobComInvoiceLine>().Lookups;
			AssertType(typeof(ConsignorCollection), lookups.ConsignorList);
		}

		public void TestBuyerList()
		{
			var lookups = Factory.New<JobComInvoiceLine>().Lookups;
			AssertType(typeof(OrgHeaderCollection), lookups.BuyerList);
		}

		public void TestSellerList()
		{
			var lookups = Factory.New<JobComInvoiceLine>().Lookups;
			AssertType(typeof(OrgHeaderCollection), lookups.SellerList);
		}

		public void TestValuationIndicators()
		{
			var lookups = Factory.New<JobComInvoiceLine>().Lookups;
			AssertSame("ValuationIndicators", Factory.GetCachedValue<ValuationIndicatorCodeList>(), lookups.ValuationIndicators);
		}

		public void TestAdditionalCodeForExport()
		{
			SetupTariffRateAndCondition();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoice.JZ_ValuationDateOverride = new ZDateTime(2019, 12, 10, 0, 0, 0);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Australia;

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "8086";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "imp1", "imp2", "imp6", "imp7", "imp8" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodesZString());

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_Tariff = "8088";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "exp3", "exp4", "exp5", "exp9", "exp10" }, invoiceLine.Lookups.AdditionalCodesList.GetAllCodesZString());
		}

		void SetupTariffRateAndCondition()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupImp = refDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
			refDataHelper.AddCountry(tradeGroupImp, Core.Constants.CountryCodes.Botswana, date1, date2);

			var tradeGroupExp = refDataHelper.CreateTradeGroup("AU", "STANDARD", date1, date2);
			refDataHelper.AddCountry(tradeGroupExp, Core.Constants.CountryCodes.Australia, date1, date2);

			var impTariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "IMP");
			var expTariffType = refDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			var dutyRateType = refDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = refDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var impTariff = refDataHelper.CreateTariff(dataGrouping, impTariffType.PK, "8086", date1, date2, "Imp Description 0");
			var expTariff = refDataHelper.CreateTariff(dataGrouping, expTariffType.PK, "8088", date1, date2, "Exp Description 0");

			var conditionType = refDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, "CTRL", "TY1");
			var impConditionCode = refDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType.PK, impTariff.PK, "", isImport: true, isExport: false, date1, date2);
			var expConditionCode = refDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType.PK, expTariff.PK, "", isImport: false, isExport: true, date1, date2);

			var impRate = refDataHelper.CreateRate(impTariff, rateCode1.PK, date1, date2, "0");
			refDataHelper.CreateCusApplicability(impRate, tradeGroupImp, date1, date2, "imp1");
			refDataHelper.CreateCusApplicability(impRate, tradeGroupImp, date1, date2, "imp2");

			var expRate = refDataHelper.CreateRate(expTariff, rateCode1.PK, date1, date2, "0");
			refDataHelper.CreateCusApplicability(expRate, tradeGroupExp, date1, date2, "exp3");
			refDataHelper.CreateCusApplicability(expRate, tradeGroupExp, date1, date2, "exp4");
			refDataHelper.CreateCusApplicability(expRate, tradeGroupExp, date1, date2, "exp5");

			refDataHelper.CreateCusApplicability(impConditionCode, tradeGroupImp, date1, date2, "imp6");
			refDataHelper.CreateCusApplicability(impConditionCode, tradeGroupImp, date1, date2, "imp7");
			refDataHelper.CreateCusApplicability(impConditionCode, tradeGroupImp, date1, date2, "imp8");
			refDataHelper.CreateCusApplicability(expConditionCode, tradeGroupExp, date1, date2, "exp9");
			refDataHelper.CreateCusApplicability(expConditionCode, tradeGroupExp, date1, date2, "exp10");
		}
	}
}
