using System;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class LineMatchingValidation : TransactionLineEmptyValidation
	{
		public LineMatchingValidation(InvoicingLineBase parent)
			: base(parent)
		{
			ILineMatchingParent = parent;
		}

		readonly ILineMatching ILineMatchingParent;

		public new InvoicingLineBase Parent
		{
			get { return (InvoicingLineBase)base.Parent; }
		}

		public void ValidatePaidAmount()
		{
			ValidateCalculatedProperty(ILineMatchingParent.PaidAmountInfo);
		}

		protected void CheckPaidAmount()
		{
			if (!ILineMatchingParent.PaidAmount.IsEmpty)
			{
				if (Math.Abs(ILineMatchingParent.PaidAmount) > Math.Abs(ILineMatchingParent.AL_OSAmount) && Math.Sign(ILineMatchingParent.PaidAmount) == Math.Sign(ILineMatchingParent.AL_OSAmount)
					|| Math.Sign(ILineMatchingParent.PaidAmount) != Math.Sign(ILineMatchingParent.AL_OSAmount))
				{
					ILineMatchingParent.PaidAmountInfo.AddError(Res.GetString("2b09ba77-2683-4352-a633-d4268dad49c8", "This value cannot be greater than the value of the transaction line"));
				}
				else if (Math.Abs(ILineMatchingParent.PaidAmount) > Math.Abs(ILineMatchingParent.OutstandingAmount) && Math.Sign(ILineMatchingParent.PaidAmount) == Math.Sign(ILineMatchingParent.OutstandingAmount)
					|| Math.Sign(ILineMatchingParent.PaidAmount) != Math.Sign(ILineMatchingParent.OutstandingAmount))
				{
					ILineMatchingParent.PaidAmountInfo.AddError(Res.GetString("1a428761-c78c-4710-84fc-6b48fad865e9", "This value cannot be greater than the outstanding amount of the transaction line"));
				}
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		public override void ValidateAll()
		{
			ValidatePaidAmount();
			ValidateAL_GE();
		}
	}
}