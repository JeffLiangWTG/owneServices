using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsPickLineCollection))]
	sealed class DocWhsPickLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsPickLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsPickLine pickLine = Factory.New<WhsPickLine>();
			return DocWhsPickLine.New(pickLine, Factory);
		}

		protected override DocWhsPickLineCollection GetCollectionToTest()
		{
			return new DocWhsPickLineCollection(Factory);
		}
	}
}
