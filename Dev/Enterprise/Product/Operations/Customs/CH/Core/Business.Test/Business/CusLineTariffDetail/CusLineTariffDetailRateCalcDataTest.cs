using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class CusLineTariffDetailRateCalcDataTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("tariffDetail is null", () => new CusLineTariffDetailRateCalcData(null));
	}

	public void TestDateOfValuation()
	{
		AssertEquals(ZDateTime.Today, calcData.DateOfValuation);

		var tomorrow = ZDate.Today.AddDays(1);
		jobDeclaration.JE_ValuationDate = tomorrow;
		AssertEquals(tomorrow, calcData.DateOfValuation);
	}

	public void TestValueForDuty()
	{
		AssertEquals(0m, calcData.ValueForDuty);

		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		invoiceLine.JI_LinePrice = 1;
		AssertEquals(1m, calcData.ValueForDuty);
	}

	public void TestCustomsValue()
	{
		AssertEquals(0m, calcData.CustomsValue);

		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		invoiceLine.JI_LinePrice = 1;
		AssertEquals(1m, calcData.CustomsValue);
	}

	public void TestUnitOfMeasureValueList()
	{
		cusLineTariffDetail.BZ_Qty1 = 1;
		cusLineTariffDetail.BZ_UQ1 = UniversalReferenceConstants.QuantityUnit.LiterPureAlcohol;

		CombineAssertions(() =>
		{
			AssertEquals("Count", 1, calcData.UnitOfMeasureValueList.Count);
			AssertContainsKey(calcData.CountrySpecificValueList, UniversalReferenceConstants.QuantityUnit.LiterPureAlcohol);
			AssertContainsValue(calcData.UnitOfMeasureValueList, UniversalReferenceConstants.QuantityUnit.LiterPureAlcohol, 1m);
		});
	}

	public void TestCountrySpecificValueList()
	{
		CombineAssertions(() =>
		{
			AssertEquals(1, calcData.CountrySpecificValueList.Count);
			AssertContainsKey(calcData.CountrySpecificValueList, UniversalReferenceConstants.FormulaPlaceholder.DutyCalculation);
		});
	}

	public void TestCountrySpecificValueList_DutyCalculation()
	{
		const decimal testValue = 100m;

		invoiceLine.JobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryLine = invoiceLine.JobDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var fee = entryLine.Fees.GetOrAddFeeByFeeType(FeeTypeList.Codes.A00);
		fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		fee.CF_ChargeAmount = testValue;
		invoiceLine.JI_CL = entryLine.PK;

		AssertContainsValue(calcData.CountrySpecificValueList, UniversalReferenceConstants.FormulaPlaceholder.DutyCalculation, testValue);
	}

	public void TestAdditionalInformationList()
	{
		AssertNull(calcData.AdditionalInformationList);
	}

	public void TestMeursingExpressionList()
	{
		AssertArrayEqualsByElements(nameof(calcData.MeursingExpressionList), new Dictionary<string, string>().ToArray(), calcData.MeursingExpressionList.ToArray());
	}

	void AssertContainsKey(IDictionary<string, decimal> dictionary, string key) => Assert($"Expected key '{key}' in dictionary.", !string.IsNullOrEmpty(key) || dictionary.ContainsKey(key));

	void AssertContainsValue(IDictionary<string, decimal> dictionary, string key, decimal value) => AssertEquals($"Expected value '{value}' for key '{key}'.", value, dictionary[key]);

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		cusLineTariffDetail = invoiceLine.AdditionalTaxes.AddNew();

		calcData = new CusLineTariffDetailRateCalcData(cusLineTariffDetail);
	}

	JobDeclaration jobDeclaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusLineTariffDetail cusLineTariffDetail;
	CusLineTariffDetailRateCalcData calcData;
}
