using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Register this service in ServiceContainer to act between changes being posted to DB and transaction gets committed.
	/// </summary>
	public interface IAfterSaveInTransactionService : IService
	{
		void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder);
	}

	class AfterSaveInTransactionService : ServiceProviderBase, IAfterSaveInTransactionService
	{
		// tested in BusinessObjectFactoryWithoutTransactionTest
		public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var service in Services.Values.OfType<IAfterSaveInTransactionService>())
			{
				service.DoFinalCheckBeforeCommit(businessObjectsInOnSavingOrder);
			}
		}
	}
}
