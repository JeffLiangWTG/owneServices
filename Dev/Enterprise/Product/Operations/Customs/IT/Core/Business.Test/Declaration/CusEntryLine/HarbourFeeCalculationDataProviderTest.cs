using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class HarbourFeeCalculationDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When entryLine parameter is null",
			() => new HarbourFeeCalculationDataProvider(entryLine: null));
	}

	public void TestHarbourRateProvider()
	{
		var calculationData = (IHarbourFeeCalculationDataProvider)new HarbourFeeCalculationDataProvider(entryLine);
		var harbourRateProvider = calculationData.HarbourRateProvider;
		AssertType<JobComInvoiceLineHarbourRateProvider>($"{nameof(calculationData.HarbourRateProvider)} Type", harbourRateProvider);
		AssertSame($"{nameof(calculationData.HarbourRateProvider)} Cached", harbourRateProvider, calculationData.HarbourRateProvider);
	}

	public void TestRateCalculationData()
	{
		invoiceLine.JI_Weight = 2000m;
		invoiceLine.JI_WeightUQ = "KG";

		var calculationData = (IHarbourFeeCalculationDataProvider)new HarbourFeeCalculationDataProvider(entryLine);
		var rateCalculationData = calculationData.RateCalculationData;
		AssertType<HarbourFeeUniversalRateData>($"{nameof(calculationData.RateCalculationData)} Type", rateCalculationData);
		AssertSame($"{nameof(calculationData.RateCalculationData)} Cached", rateCalculationData, calculationData.RateCalculationData);
		AssertEquals($"{nameof(rateCalculationData.UnitOfMeasureValueList)} Count", 1, rateCalculationData.UnitOfMeasureValueList.Count);
		var singleUnitOfMeasureValue = rateCalculationData.UnitOfMeasureValueList.Single();
		AssertEquals(nameof(singleUnitOfMeasureValue.Key), "TNE", singleUnitOfMeasureValue.Key);
		AssertEquals(nameof(singleUnitOfMeasureValue.Value), 2m, singleUnitOfMeasureValue.Value);
	}

	protected override void SetUp()
	{
		base.SetUp();
		new ITUniversalReferenceTestDataHelper(Factory).SetupHarbourRates();

		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;
}
