
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public abstract class DynamicTransactionCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DynamicTransactionCreator()
			: base()
		{
		}

		public DynamicTransactionCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public abstract IMatchingCollection CreateTransactions();
	}
}
