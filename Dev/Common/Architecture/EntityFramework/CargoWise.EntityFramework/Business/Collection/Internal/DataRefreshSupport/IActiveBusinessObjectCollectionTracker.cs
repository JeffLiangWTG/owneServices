using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	internal interface IActiveBusinessObjectCollectionTracker
	{
		void NotifyCollectionIndexCreated(IActiveBusinessObjectCollectionIndex collection);
		void NotifyCollectionIndexDisposed(IActiveBusinessObjectCollectionIndex collection);
		bool MatchesInAnyCollectionFilter(BusinessObject businessObject);
		IEnumerable<BusinessObject> GetBizosMatchingInAnyCollectionFilter(IEnumerable<BusinessObject> objects);
	}
}
