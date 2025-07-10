using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Utils
{
	public class DbCommandExtensionTest : TransactionedTestCase
	{
		public void TestExecuteDataRow()
		{
			var command = Db.Connection.Command("SELECT DummyBizo.* FROM dbo.DummyBizo");

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var pk1 = dummy1.PK;

			CombineAssertions(() =>
			{
				var dataRow = command.ExecuteDataRow(table => table.Rows.Cast<DataRow>().SingleOrDefault(r => r.Field<Guid>("Z0_PK") == pk1));
				AssertNull("Query should not find the dummy bizo", dataRow);

				factory.Save();

				dataRow = command.ExecuteDataRow(table => table.Rows.Cast<DataRow>().SingleOrDefault(r => r.Field<Guid>("Z0_PK") == pk1));
				AssertEquals("should be the right pk1", pk1, dataRow["Z0_PK"]);

				var dummy2 = factory.NewWithValidTestData<DummyBusinessObject>();
				var pk2 = dummy2.PK;
				factory.Save();

				dataRow = command.ExecuteDataRow(table => table.Rows.Cast<DataRow>().SingleOrDefault(r => r.Field<Guid>("Z0_PK") == pk2));
				AssertEquals("should be the right pk2", pk2, dataRow["Z0_PK"]);

				dataRow = command.ExecuteDataRow();
				AssertNull("Query should when multi row in table and no filter passed-in", dataRow);
			});
		}
	}
}
