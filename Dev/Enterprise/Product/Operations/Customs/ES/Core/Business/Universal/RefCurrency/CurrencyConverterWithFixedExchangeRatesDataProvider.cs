using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class CurrencyConverterWithFixedExchangeRatesDataProvider : Enterprise.MasterFiles.Business.CurrencyConverterWithFixedExchangeRatesDataProvider
	{
		public CurrencyConverterWithFixedExchangeRatesDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProviderWithFixedExRates dataProvider) : base(factory, dataProvider)
		{
			invoiceHeader = ((JobComInvoiceHeader)dataProvider);
		}
		readonly JobComInvoiceHeader invoiceHeader;

		protected override ZDecimal GetExchangeRateCore(ICurrency currency, out ZDateTime foundRateDate)
		{
			var baseExchangeRate = base.GetExchangeRateCore(currency, out foundRateDate);
			var invoiceHeaderExchangeRate = invoiceHeader.JZ_InvoiceCurrExRate;
			var differentBaseAndHeaderRate = !baseExchangeRate.IsEmpty && !invoiceHeaderExchangeRate.IsEmpty && baseExchangeRate != invoiceHeaderExchangeRate;
			var currencyIsInHeader = currency != null && !invoiceHeader.JZ_RX_NKInvoice_Currency.IsEmpty
				&& !currency.Code.IsNullOrEmpty() && invoiceHeader.JZ_RX_NKInvoice_Currency == currency.Code;
			return currencyIsInHeader && differentBaseAndHeaderRate ? invoiceHeaderExchangeRate : baseExchangeRate;
		}

		public ZDecimal GetBaseExchangeRate(ICurrency currency)
		{
			return base.GetExchangeRateCore(currency, out _);
		}
	}
}
