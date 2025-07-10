using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class TransactionLineSummaryCollection : NonPersistentBusinessObjectCollection<TransactionLineSummary>
	{
		public TransactionLineSummaryCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransactionLineSummary(null);
		}
	}
}
