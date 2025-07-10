using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwSourceTable))]
	class EdwSourceTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdwSourceTable("Schema", "Table");
		}

		public void TestEdwSourceTable()
		{
			var customTable = new EdwSourceTable("Schema", "Table");
			AssertEquals("Schema", customTable.Schema);
			AssertEquals("Table", customTable.Name);
		}
	}
}
