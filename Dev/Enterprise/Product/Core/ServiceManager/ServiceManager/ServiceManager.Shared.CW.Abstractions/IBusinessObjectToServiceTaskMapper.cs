using System.Collections.Generic;
using CargoWise.EntityFramework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Shared.Interfaces
{
	public interface IBusinessObjectToServiceTaskMapper
	{
		IDictionary<string, ICollection<string>> MapBusinessObjectsToServiceTasks(INudgingController nudgingController, IEnumerable<BusinessObject> businessObjects);
	}
}
