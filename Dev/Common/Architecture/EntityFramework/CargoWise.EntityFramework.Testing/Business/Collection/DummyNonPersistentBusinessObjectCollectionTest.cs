using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyNonPersistentBusinessObjectCollection))]
	sealed class DummyNonPersistentBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DummyNonPersistentBusinessObjectCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyNonPersistentBusinessObject();
		}

		protected override DummyNonPersistentBusinessObjectCollection GetCollectionToTest()
		{
			return new DummyNonPersistentBusinessObjectCollection(Factory);
		}
	}
}
