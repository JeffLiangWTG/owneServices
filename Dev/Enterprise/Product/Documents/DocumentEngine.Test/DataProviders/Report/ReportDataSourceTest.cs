using System.Data;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class ReportDataSourceTest : ADODataSourceTest
	{
		public override void TestFilter()
		{
			DataTable testData = new DataTable("Test");
			testData.Columns.Add("Text", typeof(string));
			testData.Columns.Add("Decimal", typeof(decimal));
			testData.Columns.Add("Bool", typeof(bool));

			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());

			testData.Rows[0]["Text"] = "Stop";
			testData.Rows[1]["Text"] = "Blah";
			testData.Rows[2]["Text"] = "Stop";
			testData.Rows[3]["Text"] = "Blah";

			testData.Rows[0]["Decimal"] = 1;
			testData.Rows[1]["Decimal"] = 1;
			testData.Rows[2]["Decimal"] = 2;
			testData.Rows[3]["Decimal"] = 2;

			testData.Rows[0]["Bool"] = true;
			testData.Rows[1]["Bool"] = false;
			testData.Rows[2]["Bool"] = true;
			testData.Rows[3]["Bool"] = false;

			ADODataSource source = GetNewDataSource(testData);
			AssertEquals(2, source.Filter("(\"<Text>\"==\"Stop\" && <Decimal>==1) || (<Bool>==true && <Decimal>==2)").RowCount);
		}

		#region Implementation
		protected override ADODataSource GetNewDataSource(DataTable wrappedDataTable)
		{
			return new ReportDataSource(wrappedDataTable);
		}
		#endregion
	}
}
