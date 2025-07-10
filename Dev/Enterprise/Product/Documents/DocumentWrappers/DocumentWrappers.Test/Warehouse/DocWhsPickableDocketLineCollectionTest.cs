using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickableDocketLineCollection))]
	sealed class DocWhsPickableDocketLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsPickableDocketLineCollection>
	{
		protected override DocWhsPickableDocketLineCollection GetCollectionToTest()
		{
			return new DocWhsPickableDocketLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var whsDocketLine = Factory.New<WhsOrderLine>();
			return DocWhsPickableDocketLine.New(whsDocketLine, Factory);
		}
	}
}
