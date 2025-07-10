using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecAdditionalTaxDataProvider))]
sealed class EdecGoodsItemValuationDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null-argument", EdecGoodsItemValuationDataProvider.New(null));
			AssertNotNull("Non-null-argument", EdecGoodsItemValuationDataProvider.New(entryLine));
		});
	}

	public void TestProvider()
	{
		invoiceLine.JI_WeightIncludingInnerPackage = 10;
		invoiceLine.JI_TareSupplementPercentage = 12;
		invoiceLine.JI_TareSupplementConfirmation = true;
		invoiceLine.JI_Tariff = "12345678901234";
		entryLine.CL_CustomsValue = 14;
		invoiceLine.JI_VATValueConfirmation = true;
		invoiceLine.JI_ZZF_NKTaxType = "7";
		invoiceLine.JI_VATCodeConfirmation = true;
		invoiceLine.JI_RateOverride = false;

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.NetDuty), true, dataProvider.NetDuty);
			AssertEquals(nameof(dataProvider.TareSupplement), 12m, dataProvider.TareSupplement);
			AssertEquals(nameof(dataProvider.TareSupplementConfirmation), true, dataProvider.TareSupplementConfirmation);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(nameof(dataProvider.CustomsFavourCode), "901", dataProvider.CustomsFavourCode);
			AssertEquals(nameof(dataProvider.VATValue), 14m, dataProvider.VATValue);
			AssertEquals(nameof(dataProvider.VATValueConfirmation), true, dataProvider.VATValueConfirmation);
			AssertEquals(nameof(dataProvider.VATCode), "7", dataProvider.VATCode);
			AssertEquals(nameof(dataProvider.VATCodeConfirmation), true, dataProvider.VATCodeConfirmation);
			AssertEquals(nameof(dataProvider.RateConfirmation), false, dataProvider.RateConfirmation);
		});
	}

	public void TestNetDuty()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_WeightIncludingInnerPackage = 0;
			AssertEquals(nameof(dataProvider.NetDuty), false, dataProvider.NetDuty);

			var invoiceLine2 = invoiceLine.InvoiceHeader.InvoiceLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine2);
			invoiceLine2.JI_WeightIncludingInnerPackage = 5;
			AssertEquals(nameof(dataProvider.NetDuty), true, dataProvider.NetDuty);
		});
	}

	public void TestTareSupplement()
	{
		CombineAssertions(() =>
		{
			AssertNull("when empty", dataProvider.TareSupplement);

			invoiceLine.JI_TareSupplementPercentage = 3;
			invoiceLine.JI_TareSupplementConfirmation = true;
			AssertEquals("when JI_TareSupplementConfirmation true is filled", invoiceLine.JI_TareSupplementPercentage, dataProvider.TareSupplement);

			invoiceLine.JI_TareSupplementConfirmation = false;
			AssertNull("when false", dataProvider.TareSupplement);
		});
	}

	public void TestTareSupplementConfirmation()
	{
		CombineAssertions(() =>
		{
			AssertNull(dataProvider.TareSupplementConfirmation);

			invoiceLine.JI_WeightIncludingInnerPackage = 1;
			invoiceLine.JI_TareSupplementConfirmation = true;
			AssertEquals(nameof(dataProvider.TareSupplementConfirmation), true, dataProvider.TareSupplementConfirmation);
			invoiceLine.JI_TareSupplementConfirmation = false;
			AssertEquals(nameof(dataProvider.TareSupplementConfirmation), false, dataProvider.TareSupplementConfirmation);
		});
	}

	public void TestVATValueConfirmation()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_VATValueConfirmation = true;
			AssertEquals(nameof(dataProvider.VATValueConfirmation), true, dataProvider.VATValueConfirmation);
			invoiceLine.JI_VATValueConfirmation = false;
			AssertEquals(nameof(dataProvider.VATValueConfirmation), false, dataProvider.VATValueConfirmation);
		});
	}

	public void TestCustomsFavorCode()
	{
		CombineAssertions(() =>
		{
			AssertNull("empty", dataProvider.CustomsFavourCode);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoiceLine.JI_Tariff = "12345678000234";
			AssertNull("empty with 000", dataProvider.CustomsFavourCode);

			invoiceLine.JI_Tariff = "12345678040234";
			AssertEquals("non-empty without leading zero", "40", dataProvider.CustomsFavourCode);
		});
	}

	public void TestVATValue()
	{
		CombineAssertions(() =>
		{
			entryLine.CL_CustomsValue = CargoWise.Types.ZDecimal.Zero;
			AssertEquals("When empty", 0m, dataProvider.VATValue);
		});
	}

	public void TestVATCode()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			AssertEquals("When empty", 0m, dataProvider.VATValue);
		});
	}

	public void TestVATCodeConfirmation()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_VATCodeConfirmation = true;
			AssertEquals(nameof(dataProvider.VATCodeConfirmation), true, dataProvider.VATCodeConfirmation);
			invoiceLine.JI_VATCodeConfirmation = false;
			AssertEquals(nameof(dataProvider.VATCodeConfirmation), false, dataProvider.VATCodeConfirmation);
		});
	}

	public void TestRate()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffWithSingleRate = tariffTestHelper.CreateImportTariffWithSingleRate("10000000000000");
		var tariffWithMultipleRates = tariffTestHelper.CreateImportTariffWithMultipleRates("20000000000000", rateFormulas: new[] { "1.2 * [NAR]", "1.5 * [KGMG]" });

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffWithSingleRate.ZZ1_TariffCode;
			AssertNull("single rate", dataProvider.Rate);

			invoiceLine.JI_Tariff = tariffWithMultipleRates.ZZ1_TariffCode;
			invoiceLine.DutyRateAdditionalCode = "AC001";
			AssertEquals("multiple rates", 1.2m, dataProvider.Rate);
		});
	}

	public void TestRate_Overridden()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffWithRateKGM = tariffTestHelper.CreateImportTariffWithMultipleRates("20000000000000", rateFormulas: new[] { "1.5 * [KGM]" });
		var tariffWithRateLTR = tariffTestHelper.CreateImportTariffWithMultipleRates("30000000000000", rateFormulas: new[] { "1.5 * [LTR]" });

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		invoiceLine.JI_RateOverride = true;

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = tariffWithRateKGM.ZZ1_TariffCode;

			invoiceLine.JI_OverriddenRate = ZDecimal.Zero;
			AssertEquals("KGM Rate overridden zero", 0m, dataProvider.Rate);

			invoiceLine.JI_OverriddenRate = 2m;
			AssertEquals("KGM Rate overridden value (*100)", 200m, dataProvider.Rate);

			invoiceLine.JI_Tariff = tariffWithRateLTR.ZZ1_TariffCode;

			invoiceLine.JI_OverriddenRate = 2m;
			AssertEquals("LTR Rate overridden value", 2m, dataProvider.Rate);
		});
	}

	public void TestRateConfirmation_Overridden() => CombineAssertions(() =>
	{
		invoiceLine.JI_RateOverride = true;
		AssertEquals("rate confirmation if rate overridden", true, dataProvider.RateConfirmation);

		invoiceLine.JI_RateOverride = false;
		AssertEquals("rate confirmation if rate not overridden", false, dataProvider.RateConfirmation);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		dataProvider = EdecGoodsItemValuationDataProvider.New(entryLine);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	EdecGoodsItemValuationDataProvider dataProvider;
}
