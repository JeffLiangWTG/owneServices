using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsInventoryCollection))]
	sealed class DocWhsInventoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocWhsInventoryCollection>
	{
		int ReceiveCount;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var docketLine = Helper.CreateWhsReceiveWithInventory(Data.Org1, Data.Whs1, "r" + ReceiveCount++, Data.Part1, 5m).Lines[0];
			docketLine.RunPreSaveValidation();
			return DocWhsInventory.New(docketLine, Factory);
		}

		protected override DocWhsInventoryCollection GetCollectionToTest()
		{
			return new DocWhsInventoryCollection(Factory);
		}

		TestDataSimpleEnvironment data;

		TestDataSimpleEnvironment Data { get { return data ?? (data = new TestDataSimpleEnvironment(Factory)); } }

		WhsTestHelperFunctions helperFunctions;

		WhsTestHelperFunctions Helper { get { return helperFunctions ?? (helperFunctions = new WhsTestHelperFunctions(Factory)); } }
	}
}
