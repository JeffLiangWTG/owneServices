using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IEInvoicingTransactionValidation
	{
		ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType);

		ZString GetCantReverseErrorMessage(IReversing originalTransaction);

		ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType);
	}
}
