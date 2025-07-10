using Enterprise.Accounting.Business.AccountingCountryFactory.China;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ChinaAccountingCountryFactory :
		IAccountingCountryFactory,
		IOverrideTransactionLineSequenceProvider,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IQueueInvoiceForTransmissionProvider>,
		IInstanceProvider<IGLJournalTypesProvider>,
		IMarkIssuedInvoiceAsReversed
	{
		bool IOverrideTransactionLineSequenceProvider.CanOverrideTransactionLineSequence(InvoicingBase invoicingBase) => new ChinaOverrideTransactionLineSequenceProvider().CanOverrideTransactionLineSequence(invoicingBase);

		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new ChinaEInvoicingEligibilityDecider();

		IQueueInvoiceForTransmissionProvider IInstanceProvider<IQueueInvoiceForTransmissionProvider>.Get() => new ChinaQueueInvoiceForTransmissionProvider();

		IGLJournalTypesProvider IInstanceProvider<IGLJournalTypesProvider>.Get() => new ChinaGLJournalTypesProvider();
	}
}
