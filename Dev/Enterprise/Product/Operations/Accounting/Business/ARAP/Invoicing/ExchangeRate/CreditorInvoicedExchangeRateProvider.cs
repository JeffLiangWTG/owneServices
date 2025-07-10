using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CreditorInvoicedExchangeRateProvider : IService
	{
		public CreditorInvoicedExchangeRateProvider(AccDraftInvoiceHeader draftInvoiceHeader)
		{
			this.draftInvoiceHeader = draftInvoiceHeader;
		}

		public bool TrySetCreditorInvoicedExchangeRate(InvoicingLineBase line)
		{
			if (!draftInvoiceHeader.IsInLocalCurrency || line.AL_RX_NKTransactionCurrency == LocalCurrency || !IsCreditorInvoicedExchangeRateApplicable(line.TransactionHeader))
			{
				return false;
			}

			var exRate = GetCreditorInvoicedExchangeRate(line.AL_RX_NKTransactionCurrency);

			if (exRate == null)
			{
				return false;
			}

			line.AL_ExchangeRate = exRate.AIE_ExchangeRate;

			return true;
		}

		public bool TrySetCreditorInvoicedExchangeRate(JobConsolCost consolCost)
		{
			if (!draftInvoiceHeader.IsInLocalCurrency || consolCost.E6_RX_NKCurrency == LocalCurrency)
			{
				return false;
			}

			var exRate = GetCreditorInvoicedExchangeRate(consolCost.E6_RX_NKCurrency);

			if (exRate == null)
			{
				return false;
			}

			consolCost.E6_ExchangeRate = exRate.AIE_ExchangeRate;

			return true;
		}

		internal static bool IsCreditorInvoicedExchangeRateApplicable(AccTransactionHeader transactionHeader)
		{
			if (transactionHeader  == null)
			{
				return false;
			}

			return transactionHeader.AH_Ledger.ToString() == LedgerTypes.AccountsPayable &&
				   new List<string>() { TransactionTypes.Invoice, TransactionTypes.CreditNote }.Contains(transactionHeader.AH_TransactionType);
		}

		string LocalCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		AccDraftInvoiceExRate GetCreditorInvoicedExchangeRate(ZString currency)
		{
			return (AccDraftInvoiceExRate)draftInvoiceHeader.ExchangeRates.SingleOrDefault(x => ((AccDraftInvoiceExRate)x).AIE_RX_NKRateCurrency == currency);
		}

		readonly AccDraftInvoiceHeader draftInvoiceHeader;
	}
}
