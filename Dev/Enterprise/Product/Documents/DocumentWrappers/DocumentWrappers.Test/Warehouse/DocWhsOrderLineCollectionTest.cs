using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsOrderLineCollection))]
	sealed class DocWhsOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsOrderLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsOrderLine = Factory.New<WhsOrderLine>();
			return DocWhsPickableDocketLine.New(whsOrderLine, Factory);
		}

		protected override DocWhsOrderLineCollection GetCollectionToTest()
		{
			return new DocWhsOrderLineCollection(Factory);
		}
	}
}
