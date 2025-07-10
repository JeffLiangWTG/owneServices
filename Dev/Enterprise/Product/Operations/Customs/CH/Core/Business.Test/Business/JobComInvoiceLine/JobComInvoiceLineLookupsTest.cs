using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceLineLookups))]
sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPrimaryPreferenceList()
	{
		RefCusCodeTestHelper.CreatePrimaryPreferenceCodeList(Factory);
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, Lookups.PrimaryPreferenceList.ContainsCode(RefCusCodeTestHelper.ValidPrimaryPreferenceCode));
			AssertEquals("invalid code", false, Lookups.PrimaryPreferenceList.ContainsCode(RefCusCodeTestHelper.InvalidPrimaryPreferenceCode));
		});
	}

	public void TestProcedureCodeList()
	{
		RefCusCodeTestHelper.CreateProcedureCodeList(Factory);
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: valid import code", true, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.ValidImportProcedureCode));
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: valid export code", false, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.ValidExportProcedureCode));
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: invalid code", false, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.InvalidProcedureCode));
			AssertSame($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: cached", Lookups.Procedures, Lookups.Procedures);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: valid import code", false, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.ValidImportProcedureCode));
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: valid export code", true, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.ValidExportProcedureCode));
			AssertEquals($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: invalid code", false, Lookups.Procedures.ContainsCode(RefCusCodeTestHelper.InvalidProcedureCode));
			AssertSame($"{nameof(Declaration.JE_MessageType)}={Declaration.JE_MessageType}: cached", Lookups.Procedures, Lookups.Procedures);
		});
	}

	public void TestTaxOrFeeCodeList()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;
		var taxOrFeeCodeList = Lookups.TaxOrFeeCodeList;

		AssertContainsExactElementsInAnyOrder("Tax or Fee CodeList should countain: ", new string[] { UniversalReferenceConstants.TaxCodes.StandardRate, UniversalReferenceConstants.TaxCodes.ReducedRate, UniversalReferenceConstants.TaxCodes.ExemptVat, UniversalReferenceConstants.TaxCodes.RelocationProcedure, UniversalReferenceConstants.TaxCodes.ProcessingTraffic, UniversalReferenceConstants.TaxCodes.DeferredTaxation }, taxOrFeeCodeList.GetAllCodes());
		AssertSame("TaxOrFeeCodeList should be cached", Lookups.TaxOrFeeCodeList, Lookups.TaxOrFeeCodeList);
	}

	public void TestDutyRateAdditionalCodesList()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		var tariffWithMultipleRates = helper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);
		var tariffWithSingleRate = helper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		RefCusCodeTestHelper.CreateAdditionalCodes(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = tariffWithMultipleRates.ZZ1_TariffCode;
			AssertEquals("codes", "AC001, AC002", Lookups.AdditionalCodesList.CodesAsString);
			AssertEquals("description AC001", Lookups.AdditionalCodesList.GetDescriptionFromCode("AC001"));
			AssertEquals("description AC002", Lookups.AdditionalCodesList.GetDescriptionFromCode("AC002"));

			InvoiceLine.JI_Tariff = tariffWithSingleRate.ZZ1_TariffCode;
			AssertEquals("not multiple rates", 0, Lookups.AdditionalCodesList.Count);
		});
	}

	public void TestRefundTypeList()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		RefCusCodeTestHelper.CreateRefundTypeList(Factory);
		AssertEquals("List Codes", "1, 3, 4, 5, 6", Lookups.RefundTypeList.CodesAsString);
	}

	public void TestPermitObligationCodeList()
	{
		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory);
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, Lookups.PermitObligationCodeList.ContainsCode(RefCusCodeTestHelper.ValidPermitObligationCode));
			AssertEquals("invalid code", false, Lookups.PermitObligationCodeList.ContainsCode(RefCusCodeTestHelper.InvalidPermitObligationCode));
		});
	}

	public void TestNonCustomsLawObligationCodeList()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawObligationCodeList(Factory);
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, Lookups.NonCustomsLawObligationCodeList.ContainsCode(RefCusCodeTestHelper.ValidNonCustomsLawObligationCode));
			AssertEquals("invalid code", false, Lookups.NonCustomsLawObligationCodeList.ContainsCode(RefCusCodeTestHelper.InvalidNonCustomsLawObligationCode));
		});
	}

	public void TestStorageCodeCodeList()
	{
		RefCusCodeTestHelper.CreateStorageTypeCodeList(Factory);
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, Lookups.StorageTypeCodeList.ContainsCode(RefCusCodeTestHelper.ValidStorageTypeCode));
			AssertEquals("invalid code", false, Lookups.StorageTypeCodeList.ContainsCode(RefCusCodeTestHelper.InvalidStorageTypeCode));
		});
	}

	[TestDate(2022, 09, 29)]
	public void TestCusCodeList() => CombineAssertions(() =>
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "European Customs Inventory of Chemical Substance");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "EU-1", "DESC-1", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "EU-2", "DESC-2", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "EU-3", "DESC-3", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 02, 01));
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "SwissCustoms Inventory of Chemical Substance");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, "CH-1", "DESC-1", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		Factory.Save();

		var list = Lookups.CusCodeList;
		list.Load();

		AssertContainsExactElementsInAnyOrder("Elements", new[] { "EU-1", "EU-2" }, list.Select(x => x.ZZD_Code));
		AssertSame("Cached", list, Lookups.CusCodeList);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= InvoiceHeader.InvoiceLines.AddNew();
	JobComInvoiceLine invoiceLine;

	JobComInvoiceLineLookups Lookups => InvoiceLine.Lookups;
}
