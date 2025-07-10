using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwAggregateTable))]
	class EdwAggregateTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdwAggregateTable("Schema", "Table");
		}

		public void TestEdwAggregateTable()
		{
			var aggregateTable = new EdwAggregateTable("Schema", "Table");
			AssertEquals("Schema", aggregateTable.Schema);
			AssertEquals("Table", aggregateTable.Name);
		}
	}
}
