using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(NonStandardExchangeRatesGridColumnBag))]
sealed class NonStandardExchangeRatesGridColumnBagTest : TestCase
{
	public void TestColumns()
	{
		var controlBag = NonStandardExchangeRatesGridColumnBag.Instance;

		CombineAssertions(() =>
		{
			LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.CurrencyTextBoxColumn, NonStandardExchangeRate.Schema.CSI_RX_NKCurrency, 40);
			LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.BankNameTextBoxColumn, NonStandardExchangeRate.Schema.CSI_Description, 100);
			LayoutTestHelper.AssertGridColumn<ZDateTimeOffsetEditColumnStyleInfo>(controlBag.EffectiveDateDateTimeOffsetEditColumn, NonStandardExchangeRate.Schema.CSI_EffectiveDate, 100);
			LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.CertificateNumberTextBoxColumn, NonStandardExchangeRate.Schema.CSI_ReferenceNumber, 100);
			LayoutTestHelper.AssertGridColumn<ZDateEditColumnStyleInfo>(controlBag.CertificateDateDateEditColumn, NonStandardExchangeRate.Schema.CSI_DateOfIssue, 100);
			LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.UnitInINRCalcEditColumn, NonStandardExchangeRate.Schema.CSI_Quantity, 100);
			LayoutTestHelper.AssertGridColumn<ZCalcEditColumnStyleInfo>(controlBag.ExchangeRateCalcEditColumn, NonStandardExchangeRate.Schema.CSI_Value, 100);
			LayoutTestHelper.AssertGridColumn<ZTextBoxColumnStyleInfo>(controlBag.ExchangeRatePrintInfoTextBoxColumn, NonStandardExchangeRate.Schema.CSI_AdditionalDescription, 100);
		});
	}
}
