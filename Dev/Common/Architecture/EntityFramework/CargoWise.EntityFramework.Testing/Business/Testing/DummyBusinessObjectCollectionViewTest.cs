using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyBusinessObjectCollectionView))]
	sealed class DummyBusinessObjectCollectionViewTest : BusinessObjectCollectionViewTestCase<DummyBusinessObjectCollectionView>
	{
		protected override DummyBusinessObjectCollectionView GetCollectionToTest()
		{
			var collectionToFilter = new DummyBusinessObjectCollection(Factory);
			return new DummyBusinessObjectCollectionView(collectionToFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DummyBusinessObject>();
		}
	}
}
