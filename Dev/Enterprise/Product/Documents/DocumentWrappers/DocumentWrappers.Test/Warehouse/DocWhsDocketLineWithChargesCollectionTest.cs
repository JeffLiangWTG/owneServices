using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsDocketLineWithChargesCollection))]
	sealed class DocWhsDocketLineWithChargesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsDocketLineWithChargesCollection>
	{
		protected override DocWhsDocketLineWithChargesCollection GetCollectionToTest()
		{
			return new DocWhsDocketLineWithChargesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsReceiveLine whsDocketLine = Factory.New<WhsReceiveLine>();
			return DocWhsDocketLineWithCharges.New(whsDocketLine, Factory);
		}
	}
}
