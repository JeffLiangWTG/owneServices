using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineSubAccountValidation : AccTransactionLineSubAccountValidation
	{
		public TransactionLineSubAccountValidation(TransactionLineSubAccount parent) : base(parent)
		{
			Parent = parent;
		}

		protected new TransactionLineSubAccount Parent;

		protected override void CheckAL1_SubClassParentIdIsNotEmpty()
		{
		}

		protected override void CheckAL1_SubClassParentId()
		{
			base.CheckAL1_SubClassParentId();
			SubAccountHelper.ValidateSubClassParentId(Parent.AL1_SubClassParentIdInfo, Parent.AL1_SubClassParentTableCode, Parent.SubAccountParent);
		}
	}
}
