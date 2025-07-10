using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(AuditTable))]
	class AuditTableTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AuditTable("Schema.Table");
		}

		public void TestAuditTable()
		{
			var auditTable = new AuditTable("Schema.Table");
			AssertEquals("Schema", auditTable.Schema);
			AssertEquals("Table", auditTable.Name);
		}
	}
}
