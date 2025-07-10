using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public interface IPostingJob
	{
		ZGuid Branch { get; }
		ZGuid Department { get; }
		ZString JobNumber { get; }
		ZGuid PK { get; }
		IJobInvoicingPlugIn Consumer { get; }
		ZShort UniqueJobInvoiceNumber { get; }
		void IncrementUniqueJobInvoiceNumber();
		void DecrementUniqueJobInvoiceNumber();
		ZString JH_Status { get; set; }
		void UpdateBaseExchangeRate(ZString currency, ExchangeRateValidLedgerEnum ledger, ZDecimal newExchangeRate, ZGuid? orgPK = null, bool ignoreNoLedgerExRates = false, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable);
		void UpdateBaseExchangeRate(ZString currency, ExchangeRateValidLedgerEnum ledger, ZDecimal newExchangeRate, IEnumerable<Charge> charges);
		TaxDateDefaultingOption GetTaxDateDefaultingOptionForJob(BusinessObjectFactory factory, ZString ledger);
		Tuple<ZDate, ZString> GetTaxDateBasedOnRegistryDefaultingOption(IJobInvoicingSupporter invoicingSupporter, ZString taxDateOption, ZDate invoiceDate);
	}
}
