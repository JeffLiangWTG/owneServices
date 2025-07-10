using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Schema.Testing;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class ColumnChangeMetadataTest : SchemaSyncTestCase
	{
		public void TestColumnsMatchSelectStatement()
		{
			RunTestInAnotherThreadWithTimeout(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					TestConnection.ExecuteNonQuery("set LOCK_TIMEOUT 5000;");
					ColumnChangeRetriever changeRetriever = new ColumnChangeRetriever(TestConnection, Db.DatabaseName, Db.DatabaseName);

					// Add Column script
					DataTable columnsToAdd = changeRetriever.GetColumnsToAdd();
					DataRow addColumnRow = columnsToAdd.NewRow();
					ColumnChangeMetadata addColumnMetadata = new ColumnChangeMetadata(addColumnRow);
					AssertEquals("Table Name [not specified]", "", addColumnMetadata.TableName);

					// Alter Column script
					DataTable columnsToAlter = changeRetriever.GetColumnsToAlter();
					DataRow alterColumnRow = columnsToAlter.NewRow();
					ColumnChangeMetadata alterColumnMetadata = new ColumnChangeMetadata(alterColumnRow);
					AssertEquals("Table Name [not specified]", "", alterColumnMetadata.TableName);
				}
			}, TimeSpan.FromMinutes(2));
		}

		public void TestMandatoryFields()
		{
			var testTable = new DataTable();
			testTable.Columns.Add("Dummy", typeof(string));

			try
			{
				var columnMetadataErr = new ColumnChangeMetadata(testTable.NewRow());
			}
			catch (ArgumentException ex)
			{
				Assert(
					"Caught exception does not match:\r\n" + ex.Message,
					ex.Message.StartsWith("Column 'TabSchema' does not belong to table", StringComparison.OrdinalIgnoreCase));
			}

			//  Add mandatory columns
			testTable.Columns.Add("TabSchema", typeof(string));
			testTable.Columns.Add("TabName", typeof(string));
			testTable.Columns.Add("ColName", typeof(string));

			var columnMetadataOk = new ColumnChangeMetadata(testTable.NewRow());

			AssertEquals("Table Schema", "", columnMetadataOk.TableSchema);
			AssertEquals("Table Name", "", columnMetadataOk.TableName);
			AssertEquals("Column Name", "", columnMetadataOk.ColumnName);
			AssertEquals("Data Type", null, columnMetadataOk.DataType.Description);

			AssertEquals("Column Declaration", "[]", columnMetadataOk.ColumnDeclaration.Trim());
			AssertEquals("Full add column declaration", "[]", columnMetadataOk.FullAddColumnDeclaration.Trim());

			AssertEquals("Old Data Type", null, columnMetadataOk.OldDataType);
			AssertEquals("Default", null, columnMetadataOk.GetDefaultClause());
			AssertEquals("Length", null, columnMetadataOk.GetLengthClause());

			AssertEquals("Null Clause", true, columnMetadataOk.IsNullable);
			AssertEquals("Sparse", false, columnMetadataOk.IsSparse);
			AssertEquals("IsNewLengthLessThanOld", false, columnMetadataOk.IsNewLengthLessThanOld());
			AssertEquals("IsNewPrecisionLessThanOld", false, columnMetadataOk.IsNewPrecisionLessThanOld());
			AssertEquals("OldDefaultName", "", columnMetadataOk.OldDefaultName);
		}

		public void TestOldDefaultName()
		{
			var testTable = new DataTable();
			testTable.Columns.Add("TabSchema", typeof(string));
			testTable.Columns.Add("TabName", typeof(string));
			testTable.Columns.Add("ColName", typeof(string));
			testTable.Columns.Add("OldDefName", typeof(string));

			var row = testTable.NewRow();
			row["OldDefName"] = "SomeConstraintName";
			AssertEquals("OldDefaultName", "SomeConstraintName", new ColumnChangeMetadata(row).OldDefaultName);

			row["OldDefName"] = DBNull.Value;
			AssertEquals("OldDefaultName", "", new ColumnChangeMetadata(row).OldDefaultName);
		}
	}
}
