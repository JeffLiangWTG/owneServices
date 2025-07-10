using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.CashBook.DirectPayment;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferChargeEmptyValidation : DirectPaymentValidation
	{
		public BankTransferChargeEmptyValidation(BankTransferCharge parent)
			: base(parent)
		{
		}

		new BankTransferCharge Parent
		{
			get { return (BankTransferCharge)base.Parent; }
		}

		protected override void CheckAH_AB()
		{
		}

		protected override void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
		}

		protected override void CheckAH_ChequeDrawer()
		{
		}

		protected override void CheckAH_ChequeOrReference()
		{
		}

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				if (Parent.BankTransferParent != null && Parent.BankTransferParent.IsReverseTransaction)
				{
					return CargoWise.EntityFramework.NotificationType.Warning;
				}

				return base.NotificationTypeForBranchDepartmentCombination;
			}
		}
	}
}