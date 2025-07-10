using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public class DirectPaymentLineCollection : DependentTransactionLineCollection
	{
		public DirectPaymentLineCollection(DirectPayment directPayment, BusinessObjectFactory factory)
			: base(directPayment, factory)
		{
		}

		public new DirectPaymentLine this[int index]
		{
			get { return (DirectPaymentLine)Elements[index]; }
		}

		public new DirectPaymentLine AddNew()
		{
			return (DirectPaymentLine)base.AddNew();
		}
	}
}