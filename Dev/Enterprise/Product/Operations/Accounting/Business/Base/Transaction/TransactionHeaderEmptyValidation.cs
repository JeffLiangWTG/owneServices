using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderEmptyValidation : AccTransactionHeaderValidation
	{
		public TransactionHeaderEmptyValidation(TransactionHeader parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateMultipleReversing();
		}

		protected new TransactionHeader Parent
		{
			get { return (TransactionHeader)base.Parent; }
		}

		protected void ValidateMultipleReversing()
		{
			if (Parent.MultipleReversingErrors != null)
			{
				foreach (string error in Parent.MultipleReversingErrors)
				{
					Parent.AddRowError(error);
				}
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return Parent.IsInMatchingContext;
			}
		}
	}
}