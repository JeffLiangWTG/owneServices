using System;
using System.Data;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	abstract class ADODataSourceTest : TestCaseWithFactory
	{
		public void TestGetRowsFromIndexs()
		{
			var dataSource = GetNewDataSource(CreateTestTable());
			AssertEquals("SlimeyLimey", dataSource.TableName);
			AssertEquals(4, dataSource.RowCount);
			var newSource = dataSource.GetRowsFromIndexes(new int[3] { 1, 3, 4 });
			AssertEquals(3, newSource.RowCount);
		}

		public void TestGroupbyInvalidColumnNameReportFatalError()
		{
			TestData.CreateLinesTestTable();
			using (Report.TemporarilyUseMainConnection())
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "Data:Lines=select top 140 * from ##LinesTest";
					workSheet[4, 0] = "#DocumentHeader";
					workSheet[5, 0] = "#SectionBody:Data=Lines";
					workSheet[7, 0] = "#GroupBy:Lines.BadName";
					workSheet[10, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = new Report(null, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					using (MemoryStream outputStream = new MemoryStream())
					{
						AssertNoExceptionThrown(() => report.Save(outputStream));
						AssertEquals("Report.Errors", @"
Severity: [Fatal] Message: [Error processing GroupBy columns - Could not find column [BadName].]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
					}
				}
			}
		}

		public void TestExposedDataTablePropertiesAndMethods()
		{
			ADODataSource dataSource = GetNewDataSource(CreateTestTable());
			AssertEquals("SlimeyLimey", dataSource.TableName);
			AssertEquals(4, dataSource.RowCount);
			AssertEquals(true, dataSource.ColumnExists("Column1"));
			AssertEquals(false, dataSource.ColumnExists("BlimeyCrikeyBugger"));
			AssertEquals(typeof(DataRow), dataSource.RowByIndex(0).GetType());
		}

		public void TestGroupBy()
		{
			DataTable testData = CreateTestTable();
			testData.Rows[0]["Column1"] = DBNull.Value;
			testData.Rows[1]["Column1"] = "Y";
			testData.Rows[3]["Column1"] = "";

			ADODataSource testDataSource = GetNewDataSource(testData);

			IDataRowSource[] result = testDataSource.GroupBy(new string[] { "Column1" });
			AssertEquals(2, result.Length);
		}

		public void TestSplit()
		{
			var testData = CreateTestTable();
			testData.Rows[0]["Column1"] = "A";
			testData.Rows[1]["Column1"] = "B";
			testData.Rows[2]["Column1"] = "C";
			testData.Rows[3]["Column1"] = "D";

			var dataSource = GetNewDataSource(testData);
			AssertEquals("Precondition", 4, dataSource.RowCount);

			var splitSource = dataSource.Split(5);
			AssertEquals("Should have all initial rows", 4, dataSource.RowCount);
			AssertEquals("Nothing moved here", 0, splitSource.RowCount);

			splitSource = dataSource.Split(4);
			AssertEquals("Should have all initial rows", 4, dataSource.RowCount);
			AssertEquals("Nothing moved here", 0, splitSource.RowCount);

			splitSource = dataSource.Split(2);
			AssertEquals("Should have first 2 rows", 2, dataSource.RowCount);
			AssertEquals("A", dataSource.RowByIndex(0)["Column1"]);
			AssertEquals("B", dataSource.RowByIndex(1)["Column1"]);
			AssertEquals("2 last rows should be moved here", 2, splitSource.RowCount);
			AssertEquals("C", ((ADODataSource)splitSource).RowByIndex(0)["Column1"]);
			AssertEquals("D", ((ADODataSource)splitSource).RowByIndex(1)["Column1"]);

			dataSource = GetNewDataSource(CreateTestTable());
			AssertEquals("Precondition", 4, dataSource.RowCount);
			splitSource = dataSource.Split(0);
			AssertEquals("Nothing should be left in initial data source", 0, dataSource.RowCount);
			AssertEquals("All rows should be moved here", 4, splitSource.RowCount);
		}

		public abstract void TestFilter();

		public void TestGetFirstNRows()
		{
			DataTable testData = CreateTestTable();
			testData.Rows[0]["Column1"] = "Row0";
			testData.Rows[1]["Column1"] = "Row1";
			testData.Rows[2]["Column1"] = "Row2";
			testData.Rows[3]["Column1"] = "Row3";

			ADODataSource testDataSource = GetNewDataSource(testData);

			IDataRowSource firstThree = testDataSource.GetFirstNRows(3);
			AssertEquals(3, firstThree.RowCount);
		}

		public void TestGetFirstNRowsWithMoreRowsThanInDataSource()
		{
			DataTable testData = CreateTestTable();
			testData.Rows[0]["Column1"] = "Row0";
			testData.Rows[1]["Column1"] = "Row1";
			testData.Rows[2]["Column1"] = "Row2";
			testData.Rows[3]["Column1"] = "Row3";

			ADODataSource testDataSource = GetNewDataSource(testData);

			IDataRowSource firstSix = testDataSource.GetFirstNRows(6);
			AssertEquals(6, firstSix.RowCount);
		}

		public void TestGroupByWithDateTimeColumns()
		{
			DataTable testData = new DataTable("Test");
			testData.Columns.Add("Column1", typeof(DateTime));
			testData.Columns.Add("Column2", typeof(string));

			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());

			testData.Rows[0]["Column1"] = new DateTime(2005, 1, 5);
			testData.Rows[0]["Column2"] = "1";
			testData.Rows[1]["Column1"] = new DateTime(2005, 2, 1);
			testData.Rows[1]["Column2"] = "2";
			testData.Rows[2]["Column1"] = new DateTime(2005, 1, 5);
			testData.Rows[2]["Column2"] = "2";
			testData.Rows[3]["Column1"] = new DateTime(2005, 2, 1);
			testData.Rows[3]["Column2"] = "1";
			testData.Rows[4]["Column1"] = DBNull.Value;
			testData.Rows[4]["Column2"] = "4";

			ADODataSource testDataSource = GetNewDataSource(testData);
			IDataRowSource[] result = testDataSource.GroupBy(new string[] { "Column1" });
			AssertEquals(3, result.Length);
			AssertEquals(DBNull.Value, ((ADODataSource)result[0]).RowByIndex(0)["Column1"]);
			AssertEquals(new DateTime(2005, 1, 5), ((ADODataSource)result[1]).RowByIndex(0)["Column1"]);
			AssertEquals(new DateTime(2005, 2, 1), ((ADODataSource)result[2]).RowByIndex(0)["Column1"]);
		}

		#region Implementation
		protected DataTable CreateTestTable()
		{
			DataTable testData = new DataTable("SlimeyLimey");
			testData.Columns.Add("Column1", typeof(string));

			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			testData.Rows.Add(testData.NewRow());
			return testData;
		}

		protected abstract ADODataSource GetNewDataSource(DataTable wrappedDataTable);
		#endregion
	}
}
