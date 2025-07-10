using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IEvaluateComplianceRule
	{
		ZString Ledger { get; }
		ZString TransactionType { get; }
		ZString ComplianceSubType { get; }
		ZBool IsDisbursementOrFinal { get; }
		ZBool IsSelfBillingInvoice { get; }
		ZBool IsAmendingTransaction { get; }
		ZBool IsReversalTransaction { get; }
		OrgHeader Header { get; }
		GlbCompany Company { get; }
		IEnumerable<AccTransactionLines> Lines { get; }
		ZBool EmptyLedgerMatchesAll { get; }
		ZBool EmptyTransactionTypeMatchesAll { get; }
		IEnumerable<AccTaxTransaction> TaxTransactions { get; }
		ZDecimal LocalTotalAmount { get; }
	}
}
