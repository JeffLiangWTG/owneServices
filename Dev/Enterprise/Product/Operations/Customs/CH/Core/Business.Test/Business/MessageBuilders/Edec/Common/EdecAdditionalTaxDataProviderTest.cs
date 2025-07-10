using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecAdditionalTaxDataProvider))]
sealed class EdecAdditionalTaxDataProviderTest : TestCaseWithFactory
{
	public void TestType()
	{
		additionalTaxDetail.BZ_Tariff = "123-456";
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		AssertEquals("123", dataProvider.Type);
	}

	public void TestKey()
	{
		additionalTaxDetail.BZ_Tariff = "123-456";
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		AssertEquals("456", dataProvider.Key);
	}

	public void TestQuantity()
	{
		additionalTaxDetail.BZ_Qty1 = 200;
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		CombineAssertions(() =>
		{
			additionalTaxDetail.BZ_BaseValue = ZDecimal.Zero;
			AssertEquals("No BaseValue", 200m, dataProvider.Quantity);
			additionalTaxDetail.BZ_BaseValue = 300;
			AssertEquals("With BaseValue", 300m, dataProvider.Quantity);
		});
	}

	public void TestQuantityForSpiritsTaxType()
	{
		invoiceLine.JI_CustomsThirdQuantity = 50;
		additionalTaxDetail.BZ_TaxType = UniversalReferenceConstants.AdditionalTaxesTypes.Spirits;
		additionalTaxDetail.BZ_Qty1 = 100;
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);

		AssertEquals($"Tax type = {UniversalReferenceConstants.AdditionalTaxesTypes.Spirits} (Spirits)", invoiceLine.JI_CustomsThirdQuantity, dataProvider.Quantity);
	}

	public void TestRate()
	{
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		CombineAssertions(() =>
		{
			additionalTaxDetail.BZ_ManualRate = ZDecimal.Zero;
			AssertNull("No ManualRate", dataProvider.Rate);
			additionalTaxDetail.BZ_ManualRate = 100;
			AssertEquals("With ManualRate", 100m, dataProvider.Rate);
		});
	}

	public void TestRateConfirmation()
	{
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		AssertEquals(false, dataProvider.RateConfirmation);
	}

	public void TestAlcoholLevel()
	{
		var dataProvider = EdecAdditionalTaxDataProvider.New(additionalTaxDetail);
		CombineAssertions(() =>
		{
			additionalTaxDetail.BZ_AlcoholPercentage = ZDecimal.Zero;
			AssertNull("No AlcoholPercentage", dataProvider.AlcoholLevel);
			additionalTaxDetail.BZ_AlcoholPercentage = 35.6;
			AssertEquals("With AlcoholPercentage", 35.6m, dataProvider.AlcoholLevel);
		});
	}

	public void TestNewCollection()
	{
		additionalTaxDetail.BZ_Tariff = "123-456";
		var additionalTaxDetail2 = invoiceLine.AdditionalTaxes.AddNew();
		additionalTaxDetail2.BZ_Tariff = "234-567";
		var additionalTaxDetail3 = invoiceLine.AdditionalTaxes.AddNew();
		additionalTaxDetail3.BZ_Tariff = "345-000";
		var additionalTaxDetail4 = invoiceLine.AdditionalTaxes.AddNew();
		additionalTaxDetail3.BZ_Qty1 = 12345;
		var dataProviders = EdecAdditionalTaxDataProvider.NewCollection(entryLine);
		CombineAssertions(() =>
		{
			AssertEquals("123-456", true, dataProviders.Any(a => a.Type == "123"));
			AssertEquals("234-567", true, dataProviders.Any(a => a.Type == "234"));
			AssertEquals("345-000", false, dataProviders.Any(a => a.Type == "345"));
			AssertEquals("empty", false, dataProviders.Any(a => a.Quantity == 12345));
		});
	}

	public void TestNewCollection_NullArgument()
	{
		AssertEquals(0, EdecAdditionalTaxDataProvider.NewCollection(null).Count());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		additionalTaxDetail = invoiceLine.AdditionalTaxes.AddNew();
	}
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	CusLineTariffDetail additionalTaxDetail;
}
