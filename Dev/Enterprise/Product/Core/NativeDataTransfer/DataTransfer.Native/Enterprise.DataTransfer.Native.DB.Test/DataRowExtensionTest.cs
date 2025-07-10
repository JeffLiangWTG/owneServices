using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	class DataRowExtensionTest : TransactionedTestCase
	{
		public void TestHasSameSchema()
		{
			var table1 = new DataTable("Table1");
			var table2 = new DataTable("Table2");
			var column1 = new DataColumn("Column1");
			var column2 = new DataColumn("Column2");
			var column3 = new DataColumn("Column3");

			table1.Columns.Add(column1);
			table1.Columns.Add(column2);
			table1.Columns.Add(column3);

			var row1 = table1.NewRow();
			var row2 = table1.NewRow();
			AssertEquals("Should have same schema", true, row1.HasSameSchema(row2));

			var column4 = new DataColumn("Column1");
			var column5 = new DataColumn("Column2");
			var column6 = new DataColumn("Column3");
			table2.Columns.Add(column4);
			table2.Columns.Add(column5);

			var row3 = table2.NewRow();
			AssertEquals("Should have different schema", false, row1.HasSameSchema(row3));

			table2.Columns.Add(column6);
			var row4 = table2.NewRow();
			AssertEquals("Should have same schema", true, row1.HasSameSchema(row4));
		}

		public void TestPrimaryKey()
		{
			var table = new DataTable("Table");
			var column1 = new DataColumn("Column1");
			var column2 = new DataColumn("Column2");
			var column3 = new DataColumn("Column3");
			table.Columns.Add(column1);
			table.Columns.Add(column2);
			table.Columns.Add(column3);

			table.PrimaryKey = new[] { column1 };

			var row = table.NewRow();
			row["Column1"] = "PK";
			row["Column2"] = "Other";

			var primaryKey = row.PrimaryKey();
			AssertEquals("Should be equals to primary key column number", 1, primaryKey.Count);
			AssertEquals("Should be equals to value of primary key", "PK", primaryKey["Column1"]);
		}

		public void TestGetTableDefFromCache()
		{
			DataRowExtension.ClearConstraintCache();
			var tableDef = Table.Get("DummyBizo");
			var dataTable = tableDef.ToDataTable();
			var row1 = dataTable.NewRow();
			var row2 = dataTable.NewRow();

			AssertEquals("TableCache is empty", 0, DataRowExtension.ConstraintCacheCount());

			Assert(ConstraintsEquals(tableDef.CandidateKeyConstraints, row1.Constraints()));
			AssertEquals("TableCache has one entry", 1, DataRowExtension.ConstraintCacheCount());

			Assert(ConstraintsEquals(tableDef.CandidateKeyConstraints, row2.Constraints()));
			AssertEquals("TableCache still has one entry", 1, DataRowExtension.ConstraintCacheCount());

			DataRowExtension.ClearConstraintCache();
			AssertEquals("TableCache is empty", 0, DataRowExtension.ConstraintCacheCount());
			Assert(ConstraintsEquals(tableDef.CandidateKeyConstraints, row2.Constraints()));
			AssertEquals("TableCache still has one entry", 1, DataRowExtension.ConstraintCacheCount());
		}

		public void TestGetDefFromCacheForMultipleTables()
		{
			DataRowExtension.ClearConstraintCache();
			var tableDef1 = Table.Get("DummyBizo");
			var tableDef2 = Table.Get("OrgContact");
			var dataTable1 = tableDef1.ToDataTable();
			var dataTable2 = tableDef2.ToDataTable();
			var row1 = dataTable1.NewRow();
			var row2 = dataTable2.NewRow();

			AssertEquals("TableCache is empty", 0, DataRowExtension.ConstraintCacheCount());
			Assert(ConstraintsEquals(tableDef1.CandidateKeyConstraints, row1.Constraints()));
			AssertEquals("TableCache has one entry", 1, DataRowExtension.ConstraintCacheCount());

			Assert(ConstraintsEquals(tableDef2.CandidateKeyConstraints, row2.Constraints()));
			AssertEquals("TableCache has two entry", 2, DataRowExtension.ConstraintCacheCount());

			Assert(ConstraintsEquals(tableDef2.CandidateKeyConstraints, row2.Constraints()));
			AssertEquals("TableCache still has two entry", 2, DataRowExtension.ConstraintCacheCount());
		}

		public void TestGetTableDefFromMultipleThreads()
		{
			DataRowExtension.ClearConstraintCache();
			var tableDef = Table.Get("DummyBizo");
			var dataTable = tableDef.ToDataTable();
			var row = dataTable.NewRow();
			var threadCount = 10;

			AssertEquals("TableCache is empty", 0, DataRowExtension.ConstraintCacheCount());

			var tasks = new List<Task<IEnumerable<ReadOnlyConstraint>>>();
			for (var i = 0; i < threadCount; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						return row.Constraints();
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());

			var firstResult = tasks.First().Result;
			foreach (var task in tasks)
			{
				AssertEquals(firstResult, task.Result);
			}
			AssertEquals("TableCache has one entry", 1, DataRowExtension.ConstraintCacheCount());
		}

		bool ConstraintsEquals(IEnumerable<Constraint> constraints, IEnumerable<ReadOnlyConstraint> readOnlyConstraints)
		{
			var sortedConstraints = constraints.OrderBy(c => c.Name).ToList();
			var sortedReadOnlyConstraints = readOnlyConstraints.OrderBy(r => r.Name).ToList();

			if (sortedConstraints.Count != sortedReadOnlyConstraints.Count)
			{
				return false;
			}
			return sortedConstraints.Zip(sortedReadOnlyConstraints, (constraint, readOnlyConstraint) =>
				constraint.Name == readOnlyConstraint.Name
			).All(equal => equal);
		}
	}
}
