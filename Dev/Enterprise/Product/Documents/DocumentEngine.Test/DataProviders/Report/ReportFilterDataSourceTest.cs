using System.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class ReportFilterDataSourceTest : TestCaseWithFactory
	{
		public void TestMultipleConditions()
		{
			AssertEquals(true, new ReportFilterDataSource(TestData.Rows[0], "(\"<Text>\"  == \"Stop\" && <Number> == 3)").Evaluate());
		}

		public void TestANDsAndORs()
		{
			AssertEquals(true, new ReportFilterDataSource(TestData.Rows[0], "(\"<Text>\"  == \"blah\" && <Number> == 3) || <Bool>!=false").Evaluate());
		}

		public void TestANDsAndORsExtended()
		{
			AssertEquals(false, new ReportFilterDataSource(TestData.Rows[0], "(\"<Text>\" == \"tom\" || \"<Text>\"  == \"blah\") && (<Number> == 3 || <Number>==0)").Evaluate());
		}

		public void TestGetValueForFieldNotFound()
		{
			AssertExceptionThrown(typeof(FieldNotFoundException), "Field <Test from [(\"Test\" == \"test\" || \"Test\"  == \"test\") && (Test == 1 || Test==1)]> not found on DataSource Type [DataRow].",
				delegate
				{
					new ReportFilterDataSource(TestData.Rows[0], "(\"<Test>\" == \"test\" || \"<Test>\"  == \"test\") && (<Test> == 1 || <Test>==1)").Evaluate();
				});
		}

		DataTable TestData
		{
			get
			{
				if (testData == null)
				{
					testData = new DataTable();
					testData.Columns.Add("Text", typeof(string));
					testData.Columns.Add("Number", typeof(decimal));
					testData.Columns.Add("Bool", typeof(bool));

					testData.Rows.Add(testData.NewRow());

					testData.Rows[0]["Text"] = "Stop";
					testData.Rows[0]["Number"] = 3;
					testData.Rows[0]["Bool"] = true;
				}
				return testData;
			}
		}
		DataTable testData;
	}
}
