using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(INonPersistentBusinessObjectCollectionView), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute))]
	public abstract class NonPersistentBusinessObjectCollectionViewTestCase<T> : BusinessObjectCollectionViewTestCase<T> where T : IBusinessObjectCollectionView, INonPersistentBusinessObjectCollectionView
	{
	}
}
