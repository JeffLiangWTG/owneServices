using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CurrencyConverterWithFixedExchangeRatesDataProviderTest : TestCaseWithFactory
	{
		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetExchangeRate()
		{
			SetUpExchangeRate();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var refCurrencyCurrencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, invoiceHeader);
			invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("ExchangeRate is 0 cause the Currency is empty", 0m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates)));

				invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.UnitedStates;
				AssertEquals("ExchangeRate is 0 cause the Currency is null", 0m, refCurrencyCurrencyConverter.GetExchangeRate(null));

				AssertEquals("ExchangeRate is 0 cause the Currency does not exists", 0m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates)));

				invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.Japan;
				invoiceHeader.JZ_InvoiceCurrExRate = 0m;
				AssertEquals("ExchangeRate is 2 cause the JZ_InvoiceCurrExRate is Empty", 2m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan)));

				invoiceHeader.JZ_InvoiceCurrExRate = 2m;
				AssertEquals("ExchangeRate is 0 cause the JZ_RX_NKInvoice_Currency is not the same as GetExchangeRate country and the currency is JPY", 0m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates)));

				AssertEquals("ExchangeRate is 2 cause the JZ_InvoiceCurrExRate is the same as GetExchangeRate", 2m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan)));

				invoiceHeader.JZ_InvoiceCurrExRate = 0.5m;
				AssertEquals("ExchangeRate is 0.5 cause the JZ_InvoiceCurrExRate is not the same as GetExchangeRate", 0.5m, refCurrencyCurrencyConverter.GetExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan)));
			});
		}

		[TestDate(1986, 3, 12, 4, 0, 0)]
		public void TestGetBaseExchangeRate()
		{
			SetUpExchangeRate();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var refCurrencyCurrencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, invoiceHeader);
			invoiceHeader.JZ_RX_NKInvoice_Currency = CurrencyCodes.Japan;
			invoiceHeader.JZ_InvoiceCurrExRate = 0.5m;

			AssertEquals("ExchangeRate is 2 cause the base calculation does not use the user entered exRate", 2m, refCurrencyCurrencyConverter.GetBaseExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan)));
		}

		void SetUpExchangeRate()
		{
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedStates);
			usd.ExchangeRates.DeleteAll();

			var jpy = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Japan);
			jpy.ExchangeRates.DeleteAll();

			var thisMonthUsdRate = jpy.ExchangeRates.AddNew();
			thisMonthUsdRate.RE_ExRateType = "CUS";
			thisMonthUsdRate.RE_StartDate = new ZDateTime(1986, 3, 1, 0, 0, 1);
			thisMonthUsdRate.RE_ExpiryDate = new ZDateTime(1987, 3, 1, 0, 0, 1);
			thisMonthUsdRate.RE_SellRate = 2;
			Factory.Save();
		}
	}
}
