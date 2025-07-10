using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IOverrideTransactionLineSequenceProvider
	{
		/// <summary>
		/// When true, we can override Transactiton Line Sequence through action menu "Override Transaction Line Sequence".
		/// </summary>
		bool CanOverrideTransactionLineSequence(InvoicingBase invoicingBase);
	}
}
