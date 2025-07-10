namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public class DirectPaymentLineValidation : DirectTransactionLineBaseValidation
	{
		public DirectPaymentLineValidation(DirectPaymentLine parent)
			: base(parent)
		{
		}

		protected override bool IsAPTaxMessageMandatoryRegistry() => true;
	}
}
