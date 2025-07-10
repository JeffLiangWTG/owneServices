#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchValidation
	{
		public bool ShouldValidateBranchDepartmentCombination_ForTestOnly => ShouldValidateBranchDepartmentCombination;
	}
}

#endif
