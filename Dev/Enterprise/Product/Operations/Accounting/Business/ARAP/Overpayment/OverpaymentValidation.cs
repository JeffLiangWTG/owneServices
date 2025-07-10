using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Overpayment
{
	public class OverpaymentValidation : TransactionHeaderValidation
	{
		public OverpaymentValidation(Overpayment parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new Overpayment Parent;

		#region BindableInvoiceAmount

		protected override void CheckBindableInvoiceAmount()
		{
			base.CheckBindableInvoiceAmount();
			if (Parent.BindableInvoiceAmount < 0)
			{
				Parent.BindableInvoiceAmountInfo.AddError(Res.GetString("13e987c6-d3e1-4dd3-aa37-6c60e943f54e", "Overpayment amount cannot be less than zero"));
			}
		}

		#endregion
	}
}
