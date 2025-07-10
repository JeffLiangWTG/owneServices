namespace Enterprise.Accounting.Integration
{
	// If you add members to these types, create a new file for them.
	public interface IARInvoice { }
	public interface IARInvoiceLine { }
	public interface IDepositBatchCollectionProvider { }
	public interface ITransactionExportFilterProvider { }
	public interface IGLJournalFlatFileDataImporter { }
	public interface IARAPJournalDataAdapter { }
	public interface IFinancialInvoiceAsJournalDataAdapter { }
	public interface IAccHotCheque { }
	public interface IBillingDataAdapter { }
	public interface IAgencyBillingDataAdapter { }
	public interface IeNettInboundInvoiceDataAdapter { }
	public interface IeNettInboundPaymentDataAdapter { }
	public interface IeNettEDIMessageTypeDecider { }
	public interface IJobRelatedARInvoicesExportFilter { }
	public interface IAccAlternateGLAccountCollectionProvider { }
}
