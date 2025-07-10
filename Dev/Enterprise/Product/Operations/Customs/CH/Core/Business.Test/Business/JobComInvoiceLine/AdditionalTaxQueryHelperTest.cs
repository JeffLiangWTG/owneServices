using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class AdditionalTaxQueryHelperTest : TestCaseWithFactory
{
	[TestDate]
	public void TestGetAdditionalTaxTariffsCacheKey()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
		jobDeclaration.JE_ValuationDate = ZDate.Today;

		var additionalTaxTariffsCacheKey = AdditionalTaxQueryHelper.GetAdditionalTaxTariffsCacheKey(invoiceLine);

		AssertEquals($"12345678912_CH_{ZDate.Today}_{parentTariff.ZZ1_ZZI_TariffType}", additionalTaxTariffsCacheKey);
	}

	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(1, tariffs.Length);
		AssertEquals("290-000", tariffs[0].ZZ1_TariffCode);
	}

	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode_FilterByTariffTypeDataGrouping()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var additionalTaxTariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		additionalTaxTariff.CusTariffType.ZZI_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		Factory.Save();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode_FilterByTariffType()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var additionalTaxTariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		additionalTaxTariff.CusTariffType.ZZI_TariffType = UniversalReferenceConstants.RateTypes.AdditionalFees;
		Factory.Save();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode_FilterByAdditionalCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		const string invalidAdditionalCode = "123";
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(additionalCode: invalidAdditionalCode);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}
	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode_FilterByTradeGroupCountryCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		const string invalidTradeGroupCountry = Core.Constants.CountryCodes.Belgium;
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tradeGroupCountry: invalidTradeGroupCountry);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_AdditionalCode_FilterByExcludedTradeGroupCountryCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		const string excludedTradeGroupCountry = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(excludedTradeGroupCountry: excludedTradeGroupCountry);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_RelatedTariff()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(1, tariffs.Length);
		AssertEquals("280-000", tariffs[0].ZZ1_TariffCode);
	}

	public void TestGetApplicableAdditionalTaxTariffs_ChildTariff_FilterByTariffDataGrouping()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var additionalTaxTariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);
		additionalTaxTariff.CusTariffType.ZZI_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Belgium;
		Factory.Save();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_ChildTariff_FilterByTariffType()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var additionalTaxTariff = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);
		additionalTaxTariff.CusTariffType.ZZI_TariffType = UniversalReferenceConstants.RateTypes.AdditionalFees;
		Factory.Save();
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_ChildTariff_FilterByChildTariffCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, childTariffCode: "123");
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_ChildTariff_FilterByTradeGroupCountryCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		const string invalidTradeGroupCountry = Core.Constants.CountryCodes.Belgium;
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, tradeGroupCountry: invalidTradeGroupCountry);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	public void TestGetApplicableAdditionalTaxTariffs_ChildTariff_FilterByExcludedTradeGroupCountryCode()
	{
		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		const string excludedTradeGroupCountry = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, excludedTradeGroupCountry: excludedTradeGroupCountry);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var tariffs = AdditionalTaxQueryHelper.GetApplicableAdditionalTaxTariffs(invoiceLine);

		AssertEquals(0, tariffs.Length);
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		invoiceHeader = jobDeclaration.Invoices.AddNew();

		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
	}

	JobDeclaration jobDeclaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
}
