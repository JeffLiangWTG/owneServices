using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UAInvoiceLineValidation : APInvoiceLineValidation
	{
		public UAInvoiceLineValidation(UAInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckAL_AC()
		{
			if (ShouldRunValidation)
			{
				base.CheckAL_AC();
			}
		}

		protected override void CheckGenericCharge()
		{
			if (ShouldRunValidation)
			{
				base.CheckGenericCharge();
			}
		}

		protected override void CheckAL_JH()
		{
			if (ShouldRunValidation)
			{
				base.CheckAL_JH();
			}
		}

		protected override void CheckAL_GE()
		{
			if (ShouldRunValidation)
			{
				base.CheckAL_GE();
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get { return true; }
		}

		bool ShouldRunValidation
		{
			get { return Parent.AL_LineType == TransactionLineTypes.Cost || Parent.InvoiceBase == null || !Parent.InvoiceBase.IsCreatedByENett; }
		}
	}
}
