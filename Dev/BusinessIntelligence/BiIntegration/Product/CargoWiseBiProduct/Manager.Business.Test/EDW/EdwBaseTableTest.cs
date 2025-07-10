using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(EdwBaseTable))]
	class EdwBaseTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EdwBaseTable("Schema", "Table");
		}

		public void TestEdwBaseTable()
		{
			var baseTable = new EdwBaseTable("Schema", "Table");
			AssertEquals("Schema", baseTable.Schema);
			AssertEquals("Table", baseTable.Name);
		}
	}
}
