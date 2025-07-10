using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IDataRefreshBusSubscriber
	{
		BusinessObjectFactory Factory { get; }
		void UpdatedByDataRefresh(IEnumerable<object> publishedObjects);
		bool IncludeDeletedObjectsInRefresh { get; }
	}
}
