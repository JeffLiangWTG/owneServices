using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectPaymentCollection : TransactionHeaderCollection
	{
		public BankReconDirectPaymentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZDateTime StatementDate;

		public new BankReconDirectPayment this[int index]
		{
			get { return (BankReconDirectPayment)Elements[index]; }
		}

		public new BankReconDirectPayment AddNew()
		{
			return (BankReconDirectPayment)base.AddNew();
		}
	}
}