using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwStagingTable))]
	class EdwStagingTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdwStagingTable("Schema", "Table");
		}

		public void TestEdwStagingTable()
		{
			var stagingTable = new EdwStagingTable("Schema", "Table");
			AssertEquals("Schema", stagingTable.Schema);
			AssertEquals("Table", stagingTable.Name);
		}
	}
}
