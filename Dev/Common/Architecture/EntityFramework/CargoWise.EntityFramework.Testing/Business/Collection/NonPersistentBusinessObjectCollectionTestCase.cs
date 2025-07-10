using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(INonPersistentBusinessObjectCollection), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute))]
	public abstract class NonPersistentBusinessObjectCollectionTestCase<T> : BusinessObjectCollectionBaseTestCase<T> where T : INonPersistentBusinessObjectCollection
	{
	}
}
