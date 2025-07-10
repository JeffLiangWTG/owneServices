using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Israel;

public class IsraelEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
{
	public override bool CanEvaluateByComplianceDate(AccTransactionHeader transaction, DateTime complianceDate)
	{
		if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
		{
			return transaction.AH_PostDate.Date >= (ZDate)complianceDate.Date;
		}

		if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
		{
			return transaction.AH_InvoiceDate.Date >= (ZDate)complianceDate.Date;
		}

		return false;
	}

	public override bool CanEvaluateByTransaction(AccTransactionHeader transaction)
	{
		return transaction.IsAPTransactionConvertedFromIncompleteTransaction
				|| transaction.IsAPTransactionConvertedFromUnapprovedTransaction
				|| transaction.IsAPTransactionConvertedFromTransactionPendingAllocation
				|| !transaction.IsInDatabase;
	}
}
