using Enterprise.Accounting.Business.CashBook.DirectPayment;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public partial class BankTransferChargeValidation : DirectPaymentValidation
	{
		public BankTransferChargeValidation(BankTransferCharge parent)
			: base(parent)
		{
		}

		new BankTransferCharge Parent
		{
			get { return (BankTransferCharge)base.Parent; }
		}

		static string ExchangeRatesPositiveMessage
		{
			get { return Res.GetString("7f3e606e-7546-4690-85ba-fca764893470", "Exchange rates must be a positive value."); }
		}

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();
			if (Parent.AH_ExchangeRate <= 0m)
			{
				Parent.AH_ExchangeRateInfo.AddError(ExchangeRatesPositiveMessage);
			}
		}

		protected override void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
			if (Parent.BankTransferParent == null || !Parent.BankTransferParent.IsReverseTransaction)
			{
				base.CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines();
			}
		}

		protected override void CheckAH_ChequeDrawer()
		{
		}

		protected override void CheckAH_ChequeOrReference()
		{
		}
	}
}
