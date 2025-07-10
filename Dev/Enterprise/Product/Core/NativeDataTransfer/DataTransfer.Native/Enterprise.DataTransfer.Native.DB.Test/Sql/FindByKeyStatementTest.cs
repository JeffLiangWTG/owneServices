using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Utils;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class FindByKeyStatementTest : TransactionedTestCase
	{
		public void TestFindByKeySql()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			var pk = dummy.PK;
			factory.Save();

			var entityDefinition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
			var columnDefinition = entityDefinition.PropertyDefinitions.Find("PK").ColumnDef;
			var command = new FindByKeyStatement("DummyBizo", columnDefinition, "Z0_PK", pk.ToString()).GetCommand(Db.Connection);

			AssertEquals("SELECT DummyBizo.* FROM dbo.DummyBizo WHERE Z0_PK = @param1", command.CommandText);

			var dataRow = command.ExecuteDataRow();
			AssertEquals("Query should find the dummy bizo", pk, dataRow.ItemArray[0]);
		}
	}
}
