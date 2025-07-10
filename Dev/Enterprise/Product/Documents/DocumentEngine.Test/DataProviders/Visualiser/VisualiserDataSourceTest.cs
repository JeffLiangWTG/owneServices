using System.Data;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class VisualiserDataSourceTest : ADODataSourceTest
	{
		public void TestGroupByUsesFormattedFieldNames()
		{
			string tableName = VisualiserDataSet.GetTableName("Blah");
			string fieldName = "ColumnOne";

			VisualiserDataSet dataSet = new VisualiserDataSet();
			DataTable originalTable = dataSet.Tables.Add(tableName);
			originalTable.Columns.Add(fieldName);
			originalTable.Rows.Add("value");
			originalTable.Rows.Add("foo");
			originalTable.Rows.Add("bar");
			originalTable.Rows.Add("bar");
			originalTable.Rows.Add("foo");
			originalTable.Rows.Add("foo");

			VisualiserDataSource dataSource = new VisualiserDataSource(dataSet, tableName);

			AssertEquals(@"dataSource.GroupCount(new string[] { ""Blah.Format({"" + fieldName + "":Upper})"" })", 3, dataSource.GroupCount(new string[] { "Blah.Format({" + fieldName + ":Upper})" }));
		}

		public override void TestFilter()
		{
			DataTable testData = new DataTable("Test");
			testData.Columns.Add("Text", typeof(string));
			testData.Columns.Add("Decimal", typeof(decimal));
			testData.Columns.Add("Bool", typeof(bool));

			testData.Rows.Add("Stop", 1, true);
			testData.Rows.Add("Blah", 1, false);
			testData.Rows.Add("Stop", 2, true);
			testData.Rows.Add("Blah", 2, false);

			ADODataSource source = GetNewDataSource(testData);
			AssertEquals("Filter should have no effect.", 4, source.Filter("(\"<Text>\"==\"Stop\" && <Decimal>==1) || (<Bool>==true && <Decimal>==2)").RowCount);
		}

		public void TestGroupCountCountsFromOriginalDataset()
		{
			string tableName = "Test";
			string fieldName = "ColumnOne";

			VisualiserDataSet dataSet = new VisualiserDataSet();
			DataTable originalTable = dataSet.Tables.Add(tableName);
			originalTable.Columns.Add(fieldName);
			originalTable.Rows.Add("value");
			originalTable.Rows.Add("foo");
			originalTable.Rows.Add("bar");
			originalTable.Rows.Add("bar");
			originalTable.Rows.Add("foo");
			originalTable.Rows.Add("foo");

			VisualiserDataSource dataSource = new VisualiserDataSource(dataSet, tableName);

			AssertEquals("dataSource.GroupCount(new string[] { \"" + fieldName + "\" })", 3, dataSource.GroupCount(new string[] { fieldName }));
		}

		protected override ADODataSource GetNewDataSource(DataTable wrappedDataTable)
		{
			AssertNotEquals("Precondition: wrappedDataTable.TableName (table name must be filled in)", "", wrappedDataTable.TableName);
			VisualiserDataSet dataSet = new VisualiserDataSet();
			dataSet.Tables.Add(wrappedDataTable);

			return new VisualiserDataSource(dataSet, wrappedDataTable.TableName);
		}
	}
}
