using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwCustomTable))]
	class EdwCustomTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdwCustomTable("Schema", "Table");
		}

		public void TestEdwCustomTable()
		{
			var customTable = new EdwCustomTable("Schema", "Table");
			AssertEquals("Schema", customTable.Schema);
			AssertEquals("Table", customTable.Name);
		}
	}
}
