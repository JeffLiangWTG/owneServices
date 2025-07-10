using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationCreditorInvoicedExchangeRateProvider
	{
		public APReconciliationCreditorInvoicedExchangeRateProvider(AccDraftInvoiceHeader draftInvoiceHeader)
		{
			this.draftInvoiceHeader = draftInvoiceHeader;
		}

		public bool TrySetExchangeRateAndAmounts(APReconciliationLine reconciliationLine, ExchangeRate exchangeRate)
		{
			if (!draftInvoiceHeader.IsInLocalCurrency || reconciliationLine.OSCurrency == reconciliationLine.LocalCurrency)
			{
				return false;
			}

			var draftInvoiceEchangeRate = GetCreditorInvoicedExchangeRate(reconciliationLine.OSCurrency);

			if (draftInvoiceEchangeRate == null)
			{
				return false;
			}

			reconciliationLine.ExchangeRate = draftInvoiceEchangeRate.AIE_ExchangeRate;
			reconciliationLine.LocalExTaxAmount = exchangeRate.ForeignToLocal(reconciliationLine.OSExTaxAmount, reconciliationLine.ExchangeRate);

			return true;
		}

		AccDraftInvoiceExRate GetCreditorInvoicedExchangeRate(ZString currency)
		{
			return (AccDraftInvoiceExRate)draftInvoiceHeader.ExchangeRates.SingleOrDefault(x => ((AccDraftInvoiceExRate)x).AIE_RX_NKRateCurrency == currency);
		}

		readonly AccDraftInvoiceHeader draftInvoiceHeader;
	}
}
