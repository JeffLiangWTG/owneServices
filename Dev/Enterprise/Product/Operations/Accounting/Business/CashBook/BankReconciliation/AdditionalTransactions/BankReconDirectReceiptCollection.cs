using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectReceiptCollection : TransactionHeaderCollection
	{
		public BankReconDirectReceiptCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZDateTime StatementDate;

		public new BankReconDirectReceipt this[int index]
		{
			get { return (BankReconDirectReceipt)Elements[index]; }
		}

		public new BankReconDirectReceipt AddNew()
		{
			return (BankReconDirectReceipt)base.AddNew();
		}
	}
}