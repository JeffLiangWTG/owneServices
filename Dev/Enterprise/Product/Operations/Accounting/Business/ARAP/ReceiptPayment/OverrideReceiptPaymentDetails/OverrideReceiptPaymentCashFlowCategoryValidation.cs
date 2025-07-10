using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class OverrideReceiptPaymentCashFlowCategoryValidation : OverrideReceiptPaymentDetailValidation
	{
		public OverrideReceiptPaymentCashFlowCategoryValidation(ReceiptPaymentBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_TransactionCategory()
		{
			base.CheckAH_TransactionCategory();
			if (Parent is ReceiptPaymentBase)
			{
				var parent = (ReceiptPaymentBase)Parent;
				parent.DisplayCashFlowCategoryOverrideInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(parent.DisplayCashFlowCategoryOverrideInfo);
				if (!parent.DisplayCashFlowCategoryOverride.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(parent.DisplayCashFlowCategoryOverrideInfo);
				}
			}
		}
	}
}
