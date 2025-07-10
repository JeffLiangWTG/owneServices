using CargoWise.EntityFramework;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlCollectorForDisplayCollection : NonPersistentBusinessObjectCollection<StlCollectorForDisplay>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new StlCollectorForDisplay();
		}
	}
}
