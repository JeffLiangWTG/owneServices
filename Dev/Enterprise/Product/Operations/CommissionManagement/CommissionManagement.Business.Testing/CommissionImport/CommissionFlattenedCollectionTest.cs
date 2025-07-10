using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionFlattenedCollection))]
	public class CommissionFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommissionFlattenedCollection>
	{
		protected override CommissionFlattenedCollection GetCollectionToTest()
		{
			return new CommissionFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CommissionFlattened();
		}
	}
}
