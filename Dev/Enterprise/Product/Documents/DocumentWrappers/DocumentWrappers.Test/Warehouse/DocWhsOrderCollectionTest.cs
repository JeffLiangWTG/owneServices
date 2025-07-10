using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickableDocketCollection))]
	sealed class DocWhsOrderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsPickableDocketCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			return DocWhsPickableDocket.New(order, Factory);
		}

		protected override DocWhsPickableDocketCollection GetCollectionToTest()
		{
			return new DocWhsPickableDocketCollection(Factory);
		}
	}
}
