using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Register this service in ServiceContainer to act after transaction gets committed.
	/// </summary>
	public interface IAfterCommittedService : IService
	{
		void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder);
	}

	class AfterCommittedServiceProvider : ServiceProviderBase, IAfterCommittedService
	{
		// tested in BusinessObjectFactoryWithoutTransactionTest
		public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var service in Services.Values.OfType<IAfterCommittedService>())
			{
				service.DoAfterCommitted(businessObjectsInOnSavingOrder);
			}
		}
	}
}
