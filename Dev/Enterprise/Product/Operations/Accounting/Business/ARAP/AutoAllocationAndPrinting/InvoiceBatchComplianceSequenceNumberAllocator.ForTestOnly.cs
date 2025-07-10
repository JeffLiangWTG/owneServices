#if DEBUG

using System.Collections.Generic;

namespace Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting
{
	public partial class InvoiceBatchComplianceSequenceNumberAllocator
	{
		public void OnFactorySavingBeforeTransactionCore_ForTestOnly()
		{
			OnFactorySavingBeforeTransactionCore();
		}

		public bool ShouldAskUseCurrentBranchBooksOption_ForTestOnly(Base.Transaction.TransactionHeader transaction)
		{
			return ShouldAskUseCurrentBranchBooksOption(transaction);
		}

		public bool ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(Base.Transaction.TransactionHeader transaction)
		{
			return ShouldAskUseCurrentBranchDepartmentBooksOption(transaction);
		}

		public bool CanContinueWithAllocation_ForTestOnly => CanContinueWithAllocation;

		public void AutoAllocateComplianceSequenceNumbers_ForTestOnly()
		{
			AutoAllocateComplianceSequenceNumbers();
		}

		public bool AllocationProcessFailed_ForTestOnly => allocationProcessFailed;

		public bool ResetSequenceNumberIfRequiredByUser_ForTestOnly()
		{
			return ResetSequenceNumberIfRequiredByUser();
		}

		public List<Base.Transaction.TransactionHeader> TransactionsGoingToBePrinted_ForTestOnly => TransactionsGoingToBePrinted;
	}
}

#endif
