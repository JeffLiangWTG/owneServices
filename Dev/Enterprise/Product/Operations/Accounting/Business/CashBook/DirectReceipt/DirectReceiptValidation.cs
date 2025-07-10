using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt
{
	public class DirectReceiptValidation : DirectTransactionHeaderBaseValidation
	{
		public DirectReceiptValidation(DirectReceipt parent)
			: base(parent)
		{
		}

		new DirectReceipt Parent
		{
			get { return (DirectReceipt)base.Parent; }
		}

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			ListValidation.ErrorIfInvalidCode(Parent.AH_ReceiptTypeInfo, Parent.ReceiptMethods);
		}
	}
}