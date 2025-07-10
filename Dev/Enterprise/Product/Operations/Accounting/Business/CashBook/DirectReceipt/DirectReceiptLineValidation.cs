namespace Enterprise.Accounting.Business.CashBook.DirectReceipt
{
	public class DirectReceiptLineValidation : DirectTransactionLineBaseValidation
	{
		public DirectReceiptLineValidation(DirectReceiptLine parent)
			: base(parent)
		{
		}

		protected override bool IsARTaxMessageMandatoryRegistry() => true;
	}
}
