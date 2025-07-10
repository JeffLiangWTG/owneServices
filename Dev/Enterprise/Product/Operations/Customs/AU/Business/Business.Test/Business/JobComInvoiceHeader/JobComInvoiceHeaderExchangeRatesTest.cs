using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderExchangeRatesTest : TestCaseWithFactory
	{
		public void TestSelectingInvoiceCurrencySetExchangeRate()
		{
			testDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 1.5000m, eURCurrency);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.7190m, uSDCurrency);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 1.6000m, eURCurrency, Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate);
			helper.SetExchangeRate(ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), 0.8190m, uSDCurrency, Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate);

			testDeclaration.JE_ExportDate = ZDateTime.Today;
			header.JZ_RX_NKInvoice_Currency = eURCurrency.RX_Code;
			AssertEquals("Exchange Rate is read only", true, header.JZ_InvoiceCurrExRateInfo.ReadOnly);
			AssertEquals("Exchange Rate for Euro", 1.5000m, header.JZ_InvoiceCurrExRate);
			AssertEquals("Exchange Rate for Euro", 1.6000m, header.JZ_PaymentExRate);

			header.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			AssertEquals("Exchange rate for USD", 0.7190m, header.JZ_InvoiceCurrExRate);
			AssertEquals("Exchange rate for USD", 0.8190m, header.JZ_PaymentExRate);

			header.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			AssertEquals("Exchange rate for AUD", 1m, header.JZ_InvoiceCurrExRate);
			AssertEquals("Exchange rate for AUD", 1m, header.JZ_PaymentExRate);

			header.JZ_RX_NKInvoice_Currency = ZString.Empty;
			AssertEquals("Exchange rate for no currency", 0m, header.JZ_InvoiceCurrExRate);
			AssertEquals("Exchange rate for no currency", 0m, header.JZ_PaymentExRate);
		}

		#region Implementation

		BaseJobDeclaration testDeclaration;
		BaseJobComInvoiceGroupHeader allInvoicesGroup;
		BaseJobComInvoiceHeader header;
		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;
		RefCurrency eURCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = Factory.New<BaseJobDeclaration>();
			allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
			header = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			uSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			eURCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "EUR");
		}

		#endregion
	}
}
