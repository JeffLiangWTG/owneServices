using System;
using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlSaverWithRetryTest : TestCaseWithFactory
	{
		#region Reorder Rows

		[ExpectNoExceptions]
		public void TestNoFailsWithStandardZSqlSaver()
		{
			var dummyTable = CreateDummyDataTable();
			AssertReorderRows(new ZSqlSaver(dummyTable, ((IDbConnected)Factory).Connection, GetNewSchemaResolver()), dummyTable);
		}

		[ExpectNoExceptions]
		public void TestDoesNotFailWithRowReorder()
		{
			DataTable dummyTable = CreateDummyDataTable();

			ZSqlSaverWithRetry saver = new ZSqlSaverWithRetry(dummyTable, ((IDbConnected)Factory).Connection, GetNewSchemaResolver());
			AssertReorderRows(saver, dummyTable);

			string expectedIndexName = string.Format("NR_UX_{0}_{1}", AutoDummyBizo.Schema.TableName, AutoDummyBizo.Schema.Z0_Code);
			AssertEquals(null, saver.lastDisableIndexNameForTest);
		}

		void AssertReorderRows(ZSaver saver, DataTable dummyTable)
		{
			DataRow row1 = CreateDummyDataRow(dummyTable, "XX1");
			DataRow row2 = CreateDummyDataRow(dummyTable, "XX2");
			DataRow row3 = CreateDummyDataRow(dummyTable, "XX3");
			DataRow row4 = CreateDummyDataRow(dummyTable, "XX4");

			SaveRowsWithZSaver(saver, row1, row2, row3, row4);
			dummyTable.AcceptChanges();

			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row1.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row2.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row3.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row4.RowState);

			row1[AutoDummyBizo.Schema.Z0_Code] = "XX5";
			row2[AutoDummyBizo.Schema.Z0_Code] = "XX3";
			row3[AutoDummyBizo.Schema.Z0_Code] = "XX6";
			row4[AutoDummyBizo.Schema.Z0_Code] = "XX7";

			AssertEquals("Rows should be changed by now", DataRowState.Modified, row1.RowState);
			AssertEquals("Rows should be changed by now", DataRowState.Modified, row2.RowState);
			AssertEquals("Rows should be changed by now", DataRowState.Modified, row3.RowState);
			AssertEquals("Rows should be changed by now", DataRowState.Modified, row4.RowState);

			SaveRowsWithZSaver(saver, row1, row2, row3, row4);

			ZQuery query =
				new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "XX")
				{ OrderBy = AutoDummyBizo.Schema.Z0_Code };
			DummyBusinessObject[] result = Factory.Load<DummyBusinessObject>(query);

			AssertEquals(4, result.Length);

			AssertEquals("XX3", result[0].Z0_Code);
			AssertEquals("XX5", result[1].Z0_Code);
			AssertEquals("XX6", result[2].Z0_Code);
			AssertEquals("XX7", result[3].Z0_Code);
		}

		#endregion

		#region Mixed Changes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMixedChanges()
		{
			DataTable dummyTable = CreateDummyDataTable();
			DataTable dependantTable = CreateDependantDataTable();
			DataSet dataSet = new DataSet();
			dataSet.Tables.Add(dummyTable);
			dataSet.Tables.Add(dependantTable);

			DataRow row1 = CreateDummyDataRow(dummyTable, "XX1");
			DataRow row2 = CreateDummyDataRow(dummyTable, "XX2");
			DataRow row3 = CreateDummyDataRow(dummyTable, "XX3");

			DataRow rowA = CreateDependantDataRow(dependantTable, "YY1");
			DataRow rowB = CreateDependantDataRow(dependantTable, "YY2");
			DataRow rowC = CreateDependantDataRow(dependantTable, "YY3");

			ZSqlSaverWithRetry saver = new ZSqlSaverWithRetry(dataSet, ((IDbConnected)Factory).Connection, GetNewSchemaResolver());
			SaveRowsWithZSaver(saver, row1, row2, row3, rowA, rowB, rowC);
			dataSet.AcceptChanges();

			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row1.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row2.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, row3.RowState);

			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, rowA.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, rowB.RowState);
			AssertEquals("Rows should be saved initially", DataRowState.Unchanged, rowC.RowState);

			row1.Delete();
			row2[AutoDummyBizo.Schema.Z0_Code] = "XX3";
			row3[AutoDummyBizo.Schema.Z0_Code] = "XX5";
			DataRow row4 = CreateDummyDataRow(dummyTable, "XX4");

			rowA.Delete();
			rowB[AutoDummyDependentBizo.Schema.ZD1_Code] = "YY3";
			rowC[AutoDummyDependentBizo.Schema.ZD1_Code] = "YY5";
			DataRow rowD = CreateDependantDataRow(dependantTable, "YY4");

			AssertEquals(DataRowState.Deleted, row1.RowState);
			AssertEquals(DataRowState.Modified, row2.RowState);
			AssertEquals(DataRowState.Modified, row3.RowState);
			AssertEquals(DataRowState.Added, row4.RowState);

			AssertEquals(DataRowState.Deleted, rowA.RowState);
			AssertEquals(DataRowState.Modified, rowB.RowState);
			AssertEquals(DataRowState.Modified, rowC.RowState);
			AssertEquals(DataRowState.Added, rowD.RowState);

			SaveRowsWithZSaver(saver, row1, row2, row3, row4, rowA, rowB, rowC, rowD);

			ZQuery query1 =
				new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "XX") { OrderBy = AutoDummyBizo.Schema.Z0_Code };
			DummyBusinessObject[] result1 = Factory.Load<DummyBusinessObject>(query1);

			AssertEquals(3, result1.Length);

			AssertEquals("XX3", result1[0].Z0_Code);
			AssertEquals("XX4", result1[1].Z0_Code);
			AssertEquals("XX5", result1[2].Z0_Code);

			ZQuery query2 =
				new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, "YY") { OrderBy = AutoDummyDependentBizo.Schema.ZD1_Code };
			DummyDependantBusinessObject[] result2 = Factory.Load<DummyDependantBusinessObject>(query2);

			AssertEquals(3, result2.Length);

			AssertEquals("YY3", result2[0].ZD1_Code);
			AssertEquals("YY4", result2[1].ZD1_Code);
			AssertEquals("YY5", result2[2].ZD1_Code);
		}

		#endregion

		#region Implementation

		DataTable CreateDummyDataTable()
		{
			CreateUniqueIndex(AutoDummyBizo.Schema.TableName, AutoDummyBizo.Schema.Z0_Code);

			DataTable dummyTable = new DataTable(AutoDummyBizo.Schema.TableName);
			dummyTable.Columns.Add(AutoDummyBizo.Schema.PK, typeof(Guid));
			dummyTable.Columns.Add(AutoDummyBizo.Schema.Z0_Code, typeof(string));
			return dummyTable;
		}

		DataTable CreateDependantDataTable()
		{
			CreateUniqueIndex(AutoDummyDependentBizo.Schema.TableName, AutoDummyDependentBizo.Schema.ZD1_Code);

			DataTable dependantTable = new DataTable(AutoDummyDependentBizo.Schema.TableName);
			dependantTable.Columns.Add(AutoDummyDependentBizo.Schema.PK, typeof(Guid));
			dependantTable.Columns.Add(AutoDummyDependentBizo.Schema.ZD1_Code, typeof(string));
			return dependantTable;
		}

		DataRow CreateDummyDataRow(DataTable table, string code)
		{
			DataRow row = table.NewRow();
			row[AutoDummyBizo.Schema.PK] = Guid.NewGuid();
			row[AutoDummyBizo.Schema.Z0_Code] = code;
			table.Rows.Add(row);
			return row;
		}

		DataRow CreateDependantDataRow(DataTable table, string code)
		{
			DataRow row = table.NewRow();
			row[AutoDummyDependentBizo.Schema.PK] = Guid.NewGuid();
			row[AutoDummyDependentBizo.Schema.ZD1_Code] = code;
			table.Rows.Add(row);
			return row;
		}

		void SaveRowsWithZSaver(ZSaver saver, params DataRow[] rows)
		{
			saver.GetType().GetMethod("SaveRows", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
				Invoke(saver, new object[] { rows });
		}

		void CreateUniqueIndex(string tableName, string columnName)
		{
			((IDbConnected)Factory).Connection.ExecuteNonQuery(
				string.Format("CREATE UNIQUE NONCLUSTERED INDEX [NR_UX_{0}_{1}] ON [dbo].[{0}] ([{1}] ASC)", tableName, columnName)
				);
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		#endregion
	}
}
