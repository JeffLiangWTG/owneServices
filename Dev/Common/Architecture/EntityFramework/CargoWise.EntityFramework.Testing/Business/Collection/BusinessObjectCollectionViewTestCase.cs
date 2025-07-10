using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(BusinessObjectCollectionView<>), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute))]
	public abstract class BusinessObjectCollectionViewTestCase<T> : BusinessObjectCollectionBaseTestCase<T> where T : IBusinessObjectCollectionView
	{
	}
}
