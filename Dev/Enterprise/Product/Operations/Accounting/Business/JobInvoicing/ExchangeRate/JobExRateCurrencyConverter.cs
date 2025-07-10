using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobExRateCurrencyConverter : CurrencyConverter, IJobExRateCurrencyConverter
	{
		public JobExRateCurrencyConverter(ExchangeRatesCollection exchangeRates, ZDateTime dateToRateForFallback, ExchangeRateType rateType)
			: base(exchangeRates.Factory, dateToRateForFallback, rateType)
		{
			this.ExchangeRates = exchangeRates;
		}

		public readonly ExchangeRatesCollection ExchangeRates;

		public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
		{
			ErrorReporter.ReportOnce(GetType() + "GetExchangeRate", "Not supported in " + GetType().Name);
			foundRateDate = ZDateTime.Empty;
			return GetExchangeRate(currency);
		}

		public override ZDecimal GetExchangeRate(ICurrency currency)
		{
			var costOrSell = RateType == ExchangeRateType.Sell ? CostSell.Revenue : CostSell.Cost;
			return GetExchangeRate(currency, ZGuid.Empty, costOrSell);
		}

		public ZDecimal GetExchangeRate(ICurrency currency, ZGuid orgPK, CostSell costOrSell)
		{
			var company = ExchangeRates.ParentJob?.Company ?? Company;
			if (currency.Code == company.GC_RX_NKLocalCurrency)
			{
				return 1;
			}

			ZDecimal result = 1;

			var invoiceCurrencyTypeForAR = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(company, ExchangeRateValidLedgerEnum.AR, InvoiceCurrencyType.Foreign);
			var rate = ExchangeRates.AddRate(Factory.Load<RefCurrency>(currency.PK), ZDecimal.Zero, orgPK, costOrSell == CostSell.Revenue ? ExchangeRateOrgTypeEnum.Debtor : ExchangeRateOrgTypeEnum.Creditor, costOrSell == CostSell.Revenue ? invoiceCurrencyTypeForAR : InvoiceCurrencyType.NotApplicable);
			if (rate != null)
			{
				if (RateType == ExchangeRateType.Buy)
				{
					result = rate.JF_BaseRate;
				}
				else if (RateType == ExchangeRateType.Sell)
				{
					result = rate.JF_SellRate;
				}
				else
				{
					ErrorReporter.ReportOnce(new StackTrace().ToString(), "Unsupported ExchangeRateType " + RateType.ToString());
					result = 0m;
				}

				if (!rate.IsInDatabase && Factory.HasContext(BusinessContext.ConvertingAmountsForExportAWBHeader))
				{
					rate.ShouldDeleteDuplicateExchangeRateBeforeSave = true;
				}
			}

			return result;
		}

		public Money ConvertExact(Money monetaryAmount, ICurrency destinationCurrency, ZGuid orgPK, CostSell costOrSell, bool roundToDestinationCurrencyDecimals = true)
		{
			return ConvertExact(monetaryAmount, destinationCurrency, currency => GetExchangeRate(currency, orgPK, costOrSell), roundToDestinationCurrencyDecimals);
		}
	}
}
