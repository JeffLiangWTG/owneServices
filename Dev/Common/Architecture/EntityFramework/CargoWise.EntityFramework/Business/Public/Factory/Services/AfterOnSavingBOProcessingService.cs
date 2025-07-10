using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Register this service in ServiceContainer to act after OnSaving was called and before changes have been posted to DB.
	///
	/// Please note that OnSaving will not run after this, so changes made in this service
	/// </summary>
	public interface IAfterOnSavingBOProcessingService : IService
	{
		void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder);
	}

	// tested in BusinessObjectFactoryTest.TestAfterOnSavingBOProcessingService
	class AfterOnSavingBOProcessingServiceProvider : ServiceProviderBase, IAfterOnSavingBOProcessingService
	{
		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var service in Services.Values.OfType<IAfterOnSavingBOProcessingService>())
			{
				service.ProcessBusinesObjects(businessObjectsInOnSavingOrder);
			}
		}
	}
}
