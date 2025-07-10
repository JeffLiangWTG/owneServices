using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(DummyNonPersistentBusinessObjectWithoutAttributeCollection))]
	sealed class DummyNonPersistentBusinessObjectWithoutAttributeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DummyNonPersistentBusinessObjectWithoutAttributeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyNonPersistentBusinessObjectWithoutAttribute();
		}

		protected override DummyNonPersistentBusinessObjectWithoutAttributeCollection GetCollectionToTest()
		{
			return new DummyNonPersistentBusinessObjectWithoutAttributeCollection(Factory);
		}
	}
}
