using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CurrencyExchangeProviderTest : Customs.Business.Testing.DataProviderTestCase<CurrencyExchangeProvider>
{
	public void TestInternalCurrencyUnit()
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		AssertEquals("EUR", provider.InternalCurrencyUnit);
	}

	public void TestExchangeRate()
	{
		invoiceHeader.JZ_InvoiceCurrExRate = 1.234567;
		AssertEquals(new decimal(1.234567), provider.ExchangeRate);
	}

	protected override CurrencyExchangeProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
		provider = new CurrencyExchangeProvider(invoiceHeader);
	}
	JobComInvoiceHeader invoiceHeader;
	CurrencyExchangeProvider provider;
}
