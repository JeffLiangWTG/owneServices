using System.Collections.Generic;

namespace CargoWise.EntityFramework.Business.Public.Interfaces
{
	public interface IFactoryProcessingExtension
	{
		void OnFactoryBusinessObjectsSaved(BusinessObjectFactory factory, List<BusinessObject> businessObjects);
	}
}
