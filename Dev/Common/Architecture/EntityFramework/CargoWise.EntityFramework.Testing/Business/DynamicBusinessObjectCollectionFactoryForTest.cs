using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	public class DynamicBusinessObjectCollectionFactoryForTest : IDynamicBusinessObjectCollectionFactory
	{
		public IEnumerable<DynamicBusinessObjectCollection> Collections => collections;

		readonly List<DynamicBusinessObjectCollection> collections = new List<DynamicBusinessObjectCollection>();

		public DynamicBusinessObjectCollection Create(BusinessObjectFactory bizoFactory)
		{
			var collection = new DynamicBusinessObjectCollection(bizoFactory);
			collections.Add(collection);
			return collection;
		}
	}
}
