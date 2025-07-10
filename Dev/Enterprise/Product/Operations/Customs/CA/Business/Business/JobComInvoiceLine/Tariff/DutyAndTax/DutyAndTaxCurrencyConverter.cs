using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxCurrencyConverter : ICurrencyConverterDataProviderWithFixedExRates
	{
		public DutyAndTaxCurrencyConverter(JobComInvoiceHeader invoiceHeader)
		{
			this._invoiceHeader = invoiceHeader;
		}
		readonly JobComInvoiceHeader _invoiceHeader;

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return !_invoiceHeader.JZ_InvoiceDate.IsEmpty ? _invoiceHeader.JZ_InvoiceDate : _invoiceHeader.EffectiveValuationDate; }
		}

		ZArchitecture.Core.ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return ZArchitecture.Core.ExchangeRateType.Customs; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return JobDeclaration.CurrencyConverterMaximumDaysToFallBack; }
		}

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return _invoiceHeader.Branch != null ? _invoiceHeader.Branch.Company : GlbCompany.CurrentCompany; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return _invoiceHeader.LocalCurrencyCode; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return _invoiceHeader.IsReciprocalRates; }
		}

		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return 1m; }
		}

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return Core.Constants.CurrencyCodes.Canada; }
		}

		#endregion
	}
}
