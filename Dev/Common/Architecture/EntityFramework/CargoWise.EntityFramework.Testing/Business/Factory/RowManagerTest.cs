using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RowManagerTest : TestCaseWithFactory
	{
		public void TestGetRowsUsingIncorrectZBoolParameterType()
		{
			DataTable table = RowFactory.GetTable(RefUNLOCOSchema.Constants.TableName);
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_IsActive, true);
			RowFactory.IndexingEnabled = false;
			Assert("Precondition", RowFactory.Load(RefUNLOCOSchema.Constants.TableName, query).Length > RowFactory.MaximumRowsBeforeUsingIndex);
			RowFactory.IndexingEnabled = true;

			RowIndexManager rowIndexManager = new RowIndexManager();
			Assert("Index manager failed to return rows for bool query", rowIndexManager.GetRows(table, query, RowFactory).Length > 0);
		}

		public void TestGetRows()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowMatching = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
			rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching[DummyBizoSchema.Z0_Number.Name] = 1;
			rowMatching.Table.Rows.Add(rowMatching);

			DataRow rowNotMatching = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowNotMatching);
			rowNotMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowNotMatching[DummyBizoSchema.Z0_Number.Name] = 2;
			rowNotMatching.Table.Rows.Add(rowNotMatching);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);

			RowIndexManager rowIndexManager = new RowIndexManager();

			DataRow[] rows = rowIndexManager.GetRows(table, filter, RowFactory);
			AssertNull(rows);

			rowIndexManager.MaximumRowsBeforeUsingIndex = 0;

			rows = rowIndexManager.GetRows(table, filter, RowFactory);
			AssertEquals(1, rows.Length);
			AssertEquals(rowMatching, rows[0]);
			AssertEquals(1, rowIndexManager.CompositePartLoadCount);
		}

		public void TestGetRowsWithOrderBy()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowMatching = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
			rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching[DummyBizoSchema.Z0_Number.Name] = 1;
			rowMatching.Table.Rows.Add(rowMatching);

			DataRow rowNotMatching = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowNotMatching);
			rowNotMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowNotMatching[DummyBizoSchema.Z0_Number.Name] = 2;
			rowNotMatching.Table.Rows.Add(rowNotMatching);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			filter.OrderBy = DummyBizoSchema.Z0_Code.Name + " ASC, " + DummyBizoSchema.Z0_Number.Name + " DESC";

			RowIndexManager manager = new RowIndexManager();
			manager.MaximumRowsBeforeUsingIndex = 0;
			DataRow[] rows = manager.GetRows(table, filter, RowFactory);
			AssertEquals(rowNotMatching, rows[0]);
			AssertEquals(rowMatching, rows[1]);
		}

		public void TestGetRowsWithOrderByAndTopN()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowSecond = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowSecond);
			rowSecond[DummyBizoSchema.Z0_Code.Name] = "123";
			rowSecond[DummyBizoSchema.Z0_Number.Name] = 1;
			rowSecond.Table.Rows.Add(rowSecond);

			DataRow rowFirst = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowFirst);
			rowFirst[DummyBizoSchema.Z0_Code.Name] = "123";
			rowFirst[DummyBizoSchema.Z0_Number.Name] = 2;
			rowFirst.Table.Rows.Add(rowFirst);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			filter.OrderBy = DummyBizoSchema.Z0_Code.Name + " ASC, " + DummyBizoSchema.Z0_Number.Name + " DESC";
			filter.MaximumRows = 1;

			RowIndexManager manager = new RowIndexManager();
			manager.MaximumRowsBeforeUsingIndex = 0;
			DataRow[] rows = manager.GetRows(table, filter, RowFactory);
			AssertEquals(1, rows.Length);
			AssertEquals(rowFirst, rows[0]);
		}

		public void TestFilterWithOrsThatReturnSameRowTwice()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow rowMatching = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(rowMatching);
			rowMatching[DummyBizoSchema.Z0_Code.Name] = "123";
			rowMatching[DummyBizoSchema.Z0_Number.Name] = 1;
			rowMatching.Table.Rows.Add(rowMatching);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);

			RowIndexManager manager = new RowIndexManager();
			manager.MaximumRowsBeforeUsingIndex = 0;
			DataRow[] rows = manager.GetRows(table, filter, RowFactory);
			AssertEquals(1, rows.Length);
		}

		public void TestGetRowsUsingNestedOrStatement()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);
			RowIndexManager manager = new RowIndexManager();

			ZQuery filter = new ZQuery();
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			innerFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);

			filter.AddToFilter(innerFilter);
			manager.MaximumRowsBeforeUsingIndex = 0;
			manager.GetRows(table, filter, RowFactory);

			AssertEquals(0, manager.CompositePartLoadCount);
			AssertEquals(1, manager.OrPartLoadCount);
			AssertEquals(0, manager.InsufficientRowsToLoadCount);
			AssertEquals(0, manager.DataViewNotOptimisableCount);
		}

		public void TestGetRowsUsingNestedOrStatementWithPrecedingAnd()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);
			RowIndexManager manager = new RowIndexManager();

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, "Hello");
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			innerFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 1);

			filter.AddToFilter(innerFilter, JoinCondition.And);
			manager.MaximumRowsBeforeUsingIndex = 0;
			manager.GetRows(table, filter, RowFactory);

			AssertEquals(1, manager.CompositePartLoadCount);
			AssertEquals(0, manager.OrPartLoadCount);
			AssertEquals(0, manager.InsufficientRowsToLoadCount);
			AssertEquals(0, manager.DataViewNotOptimisableCount);
		}

		public void TestGetRowsUsingOrStatement()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow row1 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			row1[DummyBizoSchema.Z0_Code.Name] = "1";
			row1.Table.Rows.Add(row1);

			DataRow row2 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			row2[DummyBizoSchema.Z0_Code.Name] = "2";
			row2.Table.Rows.Add(row2);

			DataRow row3 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row3);
			row3[DummyBizoSchema.Z0_Code.Name] = "3";
			row3.Table.Rows.Add(row3);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "1");
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "2");

			RowIndexManager manager = new RowIndexManager();
			manager.MaximumRowsBeforeUsingIndex = 0;
			DataRow[] rows = manager.GetRows(table, filter, RowFactory);

			AssertEquals(2, rows.Length);
			AssertEquals(row1, rows[0]);
			AssertEquals(row2, rows[1]);
		}

		public void TestGetRowsUsingZSQLInFilter()
		{
			DataTable table = RowFactory.GetTable(DummyBizoSchema.Constants.TableName);

			DataRow row1 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			row1[DummyBizoSchema.Z0_Code.Name] = "1";
			row1.Table.Rows.Add(row1);

			DataRow row2 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			row2[DummyBizoSchema.Z0_Code.Name] = "2";
			row2.Table.Rows.Add(row2);

			DataRow row3 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row3);
			row3[DummyBizoSchema.Z0_Code.Name] = "3";
			row3.Table.Rows.Add(row3);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, new string[] { "1", "2" });

			RowIndexManager manager = new RowIndexManager();
			manager.MaximumRowsBeforeUsingIndex = 0;
			DataRow[] rows = manager.GetRows(table, filter, RowFactory);

			AssertEquals(2, rows.Length);
			AssertEquals(row1, rows[0]);
			AssertEquals(row2, rows[1]);
		}

		#region Implementation

		RowFactory RowFactory
		{
			get { return Factory.RowFactory; }
		}

		#endregion
	}
}
