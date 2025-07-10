using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IMostRecentPivotProvider
	{
		AccEInvoicingTransactionPivot GetMostRecentPivot(ElectronicInvoicingTransactionProxy electronicInvoicingTransaction, AccTransactionHeader transaction);
	}
}
