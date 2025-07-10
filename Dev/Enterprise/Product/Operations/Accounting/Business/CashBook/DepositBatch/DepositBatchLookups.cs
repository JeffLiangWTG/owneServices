using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public class DepositBatchLookups : AccTransactionHeaderLookups
	{
		public DepositBatchLookups(DepositBatch depositBatch)
			: base(depositBatch)
		{
		}

		#region Branches

		public override GlbBranchCollection Branches
		{
			get
			{
				return FindboxLookupCollections.GetActiveBranchForCompanyCollection(Factory);
			}
		}

		#endregion
	}
}
