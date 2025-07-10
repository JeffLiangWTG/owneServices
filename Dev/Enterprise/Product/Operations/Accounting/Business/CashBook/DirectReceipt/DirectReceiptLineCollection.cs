using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt
{
	public class DirectReceiptLineCollection : DependentTransactionLineCollection
	{
		public DirectReceiptLineCollection(DirectReceipt directReceipt, BusinessObjectFactory factory)
			: base(directReceipt, factory)
		{
		}

		public new DirectReceiptLine this[int index]
		{
			get { return (DirectReceiptLine)Elements[index]; }
		}

		public new DirectReceiptLine AddNew()
		{
			return (DirectReceiptLine)base.AddNew();
		}
	}
}