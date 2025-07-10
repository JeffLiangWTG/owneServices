using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.CashBook.DirectPayment;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferChargeLineEmptyValidation : DirectPaymentLineValidation
	{
		public BankTransferChargeLineEmptyValidation(BankTransferChargeLine parent)
			: base(parent)
		{
		}

		new BankTransferChargeLine Parent
		{
			get { return (BankTransferChargeLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAL_OSExTaxAmount();
		}

		protected override void CheckAL_AT()
		{
		}

		protected override void CheckAL_OSExTaxAmount()
		{
		}

		protected override void CheckAL_AG()
		{
		}

		protected override void CheckAL_Desc()
		{
		}

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				var bankTransferParent = Parent.BankTransferCharge != null ? Parent.BankTransferCharge.BankTransferParent : null;
				if (bankTransferParent != null && bankTransferParent.IsReverseTransaction)
				{
					return CargoWise.EntityFramework.NotificationType.Warning;
				}

				return base.NotificationTypeForBranchDepartmentCombination;
			}
		}

		protected override void CheckAL_GovtChargeCode()
		{
		}
	}
}