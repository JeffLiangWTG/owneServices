using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(ProductPendingUpdateDataObjectCollection))]
	public class ProductPendingUpdateDataObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProductPendingUpdateDataObjectCollection>
	{
		protected override ProductPendingUpdateDataObjectCollection GetCollectionToTest() => new ProductPendingUpdateDataObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ProductPendingUpdateDataObject(Factory);
	}
}
