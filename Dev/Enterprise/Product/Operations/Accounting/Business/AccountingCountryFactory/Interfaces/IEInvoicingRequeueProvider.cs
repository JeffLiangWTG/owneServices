using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingRequeueProvider
	{
		AccEInvoicingTransactionPivot[] GetAdditionalTransactionPivotsToRequeue(BusinessObjectFactory factory, IEnumerable<TransactionHeader> selectedTransactions);
	}
}
