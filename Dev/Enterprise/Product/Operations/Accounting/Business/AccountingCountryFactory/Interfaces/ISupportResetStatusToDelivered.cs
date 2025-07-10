using System.Collections.Generic;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface ISupportResetStatusToDelivered
	{
		IEnumerable<AccEInvoicingTransactionPivot> GetEligiblePivotToRequeue(IEnumerable<AccEInvoicingTransactionPivot> pivots);
	}
}
