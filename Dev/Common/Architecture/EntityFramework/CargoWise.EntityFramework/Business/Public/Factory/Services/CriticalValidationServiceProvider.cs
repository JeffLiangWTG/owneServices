using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public interface ICriticalValidationService : IService
	{
		void ProcessBusinessObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder);
	}

	class CriticalValidationServiceProvider : ServiceProviderBase, ICriticalValidationService
	{
		public void ProcessBusinessObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var service in Services.Values.OfType<ICriticalValidationService>())
			{
				service.ProcessBusinessObjects(businessObjectsInOnSavingOrder);
			}
		}
	}
}
