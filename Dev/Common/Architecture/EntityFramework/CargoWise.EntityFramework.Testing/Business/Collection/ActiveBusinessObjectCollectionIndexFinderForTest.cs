using System.Linq;

namespace CargoWise.EntityFramework.Testing
{
	public static class ActiveBusinessObjectCollectionIndexFinderForTest
	{
		public static int GetNumberOfRetainedIndexes(string tableName, BusinessObjectFactory factory)
		{
			var adder = ActiveBusinessObjectCollectionDataRefreshAdder.GetInstance(tableName, factory);

			var dependentCollectionsCount = adder.collectionTracker.DependentCollections.Masters.Values.Sum(l => l.Count);
			var manyToManyCollectionsCount = adder.collectionTracker.ManyToManyCollections.Pivots.Values.Sum(l => l.Count);

			return adder.collectionTracker.NonDependentCollections.NonDependentCollections.Count + dependentCollectionsCount + manyToManyCollectionsCount;
		}
	}
}
