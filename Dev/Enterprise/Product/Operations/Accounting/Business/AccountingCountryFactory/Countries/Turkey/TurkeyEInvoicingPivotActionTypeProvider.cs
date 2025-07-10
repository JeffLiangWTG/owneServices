using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TurkeyEInvoicingPivotActionTypeProvider : IEInvoicingPivotActionTypeProvider
	{
		string IEInvoicingPivotActionTypeProvider.GetPivotActionType(AccTransactionHeader transaction)
		{
			if (transaction.AH_Ledger == LedgerTypes.TransactionsPendingAllocation && !transaction.IsInDatabase)
			{
				return EInvoicingPivotActionType.ConfirmTransactionReceived;
			}
			else if (transaction.IsCancellationRequest())
			{
				return EInvoicingPivotActionType.Cancel;
			}
			else
			{
				return EInvoicingPivotActionType.Submit;
			}
		}
	}
}
