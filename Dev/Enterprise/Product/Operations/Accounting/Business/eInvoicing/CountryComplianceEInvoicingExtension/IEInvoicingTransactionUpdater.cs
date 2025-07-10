using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IEInvoicingTransactionUpdater
	{
		void UpdateTransaction(BusinessObjectFactory factory, TransactionHeader transaction, string actionType);
	}
}
