using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	public class RowRepositoryTest : TransactionedTestCase
	{
		public void TestMerge()
		{
			TestUtil.AlterDummyTable();
			var tableDef = Table.Get("DummyBizo");
			var rowRepository = new RowRepository(TestUtil.Connection);
			var from = SetupDataRow(tableDef);

			from["Z0_PK"] = Guid.NewGuid();
			from["Z0_Code"] = "ABC";
			from["Z0_Guid"] = null;

			var to = SetupDataRow(tableDef);
			to["Z0_PK"] = Guid.NewGuid();

			to = rowRepository.Merge(from, to);

			AssertNotEquals("Should not merge primary key to row", from["Z0_PK"], to["Z0_PK"]);
			AssertEquals("Should merge other property to row", from["Z0_Code"], to["Z0_Code"]);
			AssertEquals("Should be null if property value is null", DBNull.Value, to["Z0_Guid"]);
		}

		public void TestLoadMany_SomeNewRows_SomeExistingRows()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);
			var colName = "Z0_Decimal";

			var rowRepositoryForSave = new RowRepository(TestUtil.Connection);
			Guid pk2 = Guid.NewGuid();
			var rowToSave = rowRepositoryForSave.Create(tableDef, pk2);
			rowToSave[colName] = 3m;
			rowRepositoryForSave.Save();

			var rowRepository = new RowRepository(TestUtil.Connection);
			Guid pk1 = Guid.NewGuid();
			var row = rowRepository.Create(tableDef, pk1);
			row[colName] = 2m;

			var criteria1 = new List<Criteria>()
				{ new Criteria { Value = 2m, ColumnName = colName, TableName = tableDef.Name } };
			var criteria2 = new List<Criteria>()
				{ new Criteria { Value = 3m, ColumnName = colName, TableName = tableDef.Name } };
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			var result = rowRepository.LoadMany(criterias, tableDef);
			AssertNotNull(result[0]);
			AssertNotNull(result[1]);
			AssertEquals(2m, (decimal)result[0][colName]);
			AssertEquals(3m, (decimal)result[1][colName]);
		}

		public void TestLoadMany_NoNewRows_NoExistingRows()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);

			var rowRepository = new RowRepository(TestUtil.Connection);
			var colName = "Z0_Decimal";

			var criteria1 = new List<Criteria>()
				{ new Criteria { Value = 2m, ColumnName = colName, TableName = tableDef.Name } };
			var criteria2 = new List<Criteria>()
				{ new Criteria { Value = 3m, ColumnName = colName, TableName = tableDef.Name } };
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			var result = rowRepository.LoadMany(criterias, tableDef);
			AssertNull(result[0]);
			AssertNull(result[1]);
		}

		public void TestLoadMany_NoNewRows_SomeExistingRows()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);
			var colName = "Z0_Decimal";

			var rowRepositoryForSave = new RowRepository(TestUtil.Connection);
			Guid pk2 = Guid.NewGuid();
			var rowToSave = rowRepositoryForSave.Create(tableDef, pk2);
			rowToSave[colName] = 3m;
			rowRepositoryForSave.Save();

			var rowRepository = new RowRepository(TestUtil.Connection);

			var criteria1 = new List<Criteria>()
				{ new Criteria { Value = 2m, ColumnName = colName, TableName = tableDef.Name } };
			var criteria2 = new List<Criteria>()
				{ new Criteria { Value = 3m, ColumnName = colName, TableName = tableDef.Name } };
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			var result = rowRepository.LoadMany(criterias, tableDef);
			AssertNull(result[0]);
			AssertNotNull(result[1]);
			AssertEquals(3m, (decimal)result[1][colName]);
		}

		public void TestLoadMany_AllNewRows()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);
			var colName = "Z0_Decimal";

			var rowRepository = new RowRepository(TestUtil.Connection);
			Guid pk1 = Guid.NewGuid();
			var row1 = rowRepository.Create(tableDef, pk1);
			row1[colName] = 2m;

			Guid pk2 = Guid.NewGuid();
			var row2 = rowRepository.Create(tableDef, pk2);
			row2[colName] = 3m;

			var criteria1 = new List<Criteria>()
				{ new Criteria { Value = 2m, ColumnName = colName, TableName = tableDef.Name } };
			var criteria2 = new List<Criteria>()
				{ new Criteria { Value = 3m, ColumnName = colName, TableName = tableDef.Name } };
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			var result = rowRepository.LoadMany(criterias, tableDef);
			AssertNotNull(result[0]);
			AssertNotNull(result[1]);
			AssertEquals(2m, (decimal)result[0][colName]);
			AssertEquals(3m, (decimal)result[1][colName]);
		}

		public void TestSave_RemovesTempTables()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);

			var col1Name = "Z0_Decimal";
			var col2Name = "Z0_AnotherDecimal";
			var intColumnName = "Z0_Number";

			var rowRepository1 = new RowRepository(TestUtil.Connection);
			rowRepository1.Save();

			for (int i = 1; i <= 4; ++i)
			{
				Guid pk = Guid.NewGuid();
				var row = rowRepository1.Create(tableDef, pk);
				row[col1Name] = (decimal)i;
				row[col2Name] = (decimal)(i * 10);
				row[intColumnName] = i;
			}

			rowRepository1.Save();

			// Need a new repository so the rows aren't in the local cache and have to be fetched from the DB via a temp table
			var rowRepository2 = new RowRepository(TestUtil.Connection);
			var criteria1 = new List<Criteria>() {
				new Criteria { Value = 1m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 10m, ColumnName = col2Name, TableName = tableDef.Name },
				new Criteria { Value = 1, ColumnName = intColumnName, TableName = tableDef.Name }
			};
			var criteria2 = new List<Criteria>() {
				new Criteria { Value = 2m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 20m, ColumnName = col2Name, TableName = tableDef.Name },
				new Criteria { Value = 2, ColumnName = intColumnName, TableName = tableDef.Name }
			};
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			rowRepository2.LoadMany(criterias, tableDef);
			rowRepository2.Save();

			// Repeat with two distinct sets of criteria names so there's more than one temp table to cleanup
			rowRepository2 = new RowRepository(TestUtil.Connection);
			rowRepository2.LoadMany(criterias, tableDef);
			var criteria3 = new List<Criteria>() {
				new Criteria { Value = 3m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 30m, ColumnName = col2Name, TableName = tableDef.Name },
			};
			var criteria4 = new List<Criteria>() {
				new Criteria { Value = 4m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 40m, ColumnName = col2Name, TableName = tableDef.Name },
			};
			var criterias1b = new List<IEnumerable<Criteria>>() { criteria3, criteria4 };
			rowRepository2.LoadMany(criterias1b, tableDef);
			rowRepository2.Save();

			// In a new repository, load the rows again with less columns in the criteria.
			// If the temp tables from earlier were not removed the non-nullable missing column would cause errors.
			var rowRepository3 = new RowRepository(TestUtil.Connection);
			var criterias2 = new List<IEnumerable<Criteria>>()
			{
				new List<Criteria>() { criteria1[1] },
				new List<Criteria>() { criteria2[1] },
			};
			var criterias2b = new List<IEnumerable<Criteria>>()
			{
				new List<Criteria>() { criteria3[0] },
				new List<Criteria>() { criteria4[0] },
			};
			var result2 = rowRepository3.LoadMany(criterias2, tableDef);
			var result2b = rowRepository3.LoadMany(criterias2b, tableDef);
			AssertEquals(1m, (decimal)result2[0][col1Name]);
			AssertEquals(2m, (decimal)result2[1][col1Name]);
			AssertEquals(3m, (decimal)result2b[0][col1Name]);
			AssertEquals(4m, (decimal)result2b[1][col1Name]);
		}

		public void TestCreatedTempTablesAreUniquePerInstance()
		{
			var tableDef = Table.Get("DummyBizo");
			SetupDataTable(tableDef);

			var col1Name = "Z0_Decimal";
			var col2Name = "Z0_AnotherDecimal";
			var intColumnName = "Z0_Number";

			var criteria1 = new List<Criteria>() {
				new Criteria { Value = 1m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 10m, ColumnName = col2Name, TableName = tableDef.Name },
				new Criteria { Value = 1, ColumnName = intColumnName, TableName = tableDef.Name }
			};
			var criteria2 = new List<Criteria>() {
				new Criteria { Value = 2m, ColumnName = col1Name, TableName = tableDef.Name },
				new Criteria { Value = 20m, ColumnName = col2Name, TableName = tableDef.Name },
				new Criteria { Value = 2, ColumnName = intColumnName, TableName = tableDef.Name }
			};
			var criterias = new List<IEnumerable<Criteria>>() { criteria1, criteria2 };

			var rowRepository1 = new RowRepository(TestUtil.Connection);
			rowRepository1.LoadMany(criterias, tableDef);
			var tempTable1 = rowRepository1.GetCreatedTempTablesForTest().Single();

			var rowRepository2 = new RowRepository(TestUtil.Connection);
			rowRepository2.LoadMany(criterias, tableDef);
			var tempTable2 = rowRepository2.GetCreatedTempTablesForTest().Single();

			AssertNotEquals("names are unique", tempTable1, tempTable2);
		}

		#region Implementation

		DataTable SetupDataTable(Table tableDef)
		{
			var table = new DataTable(tableDef.Name);

			foreach (var columnDef in tableDef.Columns)
			{
				var column = new DataColumn
				{
					ColumnName = columnDef.Name
				};
				table.Columns.Add(column);
				if (columnDef is PrimaryKey)
				{
					table.PrimaryKey = new[] { column };
				}
			}
			return table;
		}

		DataRow SetupDataRow(Table tableDef)
		{
			var table = SetupDataTable(tableDef);
			return table.NewRow();
		}

		#endregion
	}
}
