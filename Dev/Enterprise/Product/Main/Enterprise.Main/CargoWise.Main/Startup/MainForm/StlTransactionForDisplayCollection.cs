using CargoWise.EntityFramework;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlTransactionForDisplayCollection : NonPersistentBusinessObjectCollection<StlTransactionForDisplay>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlTransactionForDisplay();
		}
	}
}
