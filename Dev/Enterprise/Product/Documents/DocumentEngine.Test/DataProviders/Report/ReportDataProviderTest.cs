using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class ReportDataProviderTest : TestCaseWithFactory
	{
		public void TestAddDataSource()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:##HeaderTest");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestAddDataSourceWithColonInQuery()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:select * from ##HeaderTest where MultiLineText = '1: ABC'");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestAddDataSourceWithMultipleColonsInQuery()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:select * from ##HeaderTest where MultiLineText = '1: ABC' OR MultiLineText = '2: DFG' OR MultiLineText = '3: HIJ'");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestAddDataSourceWithMultipleColonsInQueryAndNoWhereClause()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:select * from ##HeaderTest where MultiLineText = '1: ABC' OR MultiLineText = '2: DFG' OR MultiLineText = '3: HIJ' :                    NoWhereClause");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestAddDataSourceWithMultipleColonsInQueryAndNoWhereClauseAndExceptionalColumnName()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:select * from ##HeaderTest where MultiLineText = '1: ABC' OR MultiLineText = '2: DFG' OR MultiLineText = '3: HIJ' order by NoWhereClause:    NoWhereClause");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestAddDataSourceWithMultipleLineQuery()
		{
			TestData.CreateHeaderTestTable();
			AssertEquals(0, DataProvider.DS.Tables.Count);
			DataProvider.AddDataSource("Header:select " + System.Environment.NewLine +
																"* from " + System.Environment.NewLine +
																"##HeaderTest " + System.Environment.NewLine +
																"where MultiLineText = '1: ABC' OR MultiLineText = '2: DFG' OR MultiLineText = '3: HIJ' " + System.Environment.NewLine +
																"order by NoWhereClause:    NoWhereClause");
			AssertEquals(1, DataProvider.DS.Tables.Count);
			AssertEquals(0, DataProvider.DS.Tables.IndexOf("Header"));
		}

		public void TestGetDataRows()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.AddDataSource("Lines:##LinesTest");
			AssertEquals(TestData.DocEngineTestTableRowCount, DataProvider.GetDataElements("Lines").Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDataRowsWithFilter()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.Report.Analyser.ReportSQLSources.Clear();
			DataProvider.Report.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName));
			TextField testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField1";
			testTextField.Value = "2";
			DataProvider.Report.FilterCollection.Add(testTextField);

			DataProvider.AddDataSource(TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName, true);
			AssertEquals(40, DataProvider.GetDataElements(TestData.DocEngineTestTableName).Count);

			DataProvider.DS.Tables.Remove(TestData.DocEngineTestTableName);
			DataProvider.AddDataSource(TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName, true, 20);
			AssertEquals(20, DataProvider.GetDataElements(TestData.DocEngineTestTableName).Count);
		}

		public void TestGetDataRowsWithFilterAndNoWhereClause()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.Report.Analyser.ReportSQLSources.Clear();
			DataProvider.Report.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName));
			TextField testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField1";
			testTextField.Value = "2";
			DataProvider.Report.FilterCollection.Add(testTextField);

			DataProvider.AddDataSource(TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName + ":NoWhereClause");
			AssertEquals(TestData.DocEngineTestTableRowCount, DataProvider.GetDataElements(TestData.DocEngineTestTableName).Count);
		}

		public void TestGetDataRowsForSecondDataStatementWithFilter()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.Report.Analyser.ReportSQLSources.Clear();
			DataProvider.Report.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, TestData.HeaderTempTableName + ":" + TestData.HeaderTempTableName));
			DataProvider.Report.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, TestData.DocEngineTestTableName + "1:" + TestData.DocEngineTestTableName));
			TextField testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField1";
			testTextField.Value = "2";
			DataProvider.Report.FilterCollection.Add(testTextField);

			DataProvider.AddDataSource(TestData.HeaderTempTableName + ":" + TestData.HeaderTempTableName);
			DataProvider.AddDataSource(TestData.DocEngineTestTableName + "1:" + TestData.DocEngineTestTableName);
			AssertEquals(TestData.DocEngineTestTableRowCount, DataProvider.GetDataElements(TestData.DocEngineTestTableName + "1").Count);
		}

		[ExpectException(typeof(DataProviderException))]
		public void TestGetDataRowsNonExistingTable()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.AddDataSource("Lines:##LinesTest");
			AssertEquals(0, DataProvider.GetDataElements("Test1").Count);
		}

		public void TestGetColumnValue()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.AddDataSource(TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName);
			AssertEquals("0                                       ", DataProvider.GetColumnValue(null, 0, TestData.DocEngineTestTableName + ".CharField1"));
			AssertEquals((byte)'b', ((byte[])DataProvider.GetColumnValue(null, 0, TestData.DocEngineTestTableName + ".UncompressedImageField"))[1]);
			AssertEquals((byte)'b', ((byte[])DataProvider.GetColumnValue(null, 0, TestData.DocEngineTestTableName + ".CompressedImageField"))[1]);
			AssertEquals("", DataProvider.GetColumnValue(null, 0, TestData.DocEngineTestTableName + ".ChrFld"));
			AssertStartsWith("Errors Reported", "Column 'ChrFld' is not found in Table '##DocEngineTest'. Columns: ", DataProvider.Report.ErrorManager.ToString("{1}", false));
			DataProvider.Report.ErrorManager.ClearErrors();
			AssertEquals("", DataProvider.GetColumnValue(null, 0, "qewew.ChrFld"));
			AssertEquals("Errors Reported", "Table 'qewew' is not found in data set.", DataProvider.Report.ErrorManager.ToString("{1}", false));
		}

		public void TestGetColumnValueWithInvalidDataSource()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.AddDataSource(TestData.DocEngineTestTableName + ":" + TestData.DocEngineTestTableName);
			var invalidDataSource = new OneRowDataSource();
			AssertEquals("", DataProvider.GetColumnValue(invalidDataSource, 0, TestData.DocEngineTestTableName + ".CharField1"));
			AssertEquals("Errors Reported", "The Data Source used in this cell is invalid. Please specify a valid Data Source in the Data option of your Section Body.", DataProvider.Report.ErrorManager.ToString("{1}", false));
			DataProvider.Report.ErrorManager.ClearErrors();
		}

		public void TestGetColumnValueDoesntThrowExceptionWhenIndexOutOfBounds()
		{
			TestData.CreateDocEngineTestTable();
			DataProvider.AddDataSource(TestData.LinesTempTableName + ":" + TestData.LinesTempTableName);

			IDataRowSource rowSource = DataProvider.GetDataRowSource(TestData.LinesTempTableName, false).GetFirstNRows(3);
			AssertEquals("Precondition: rowSource.RowCount", 3, rowSource.RowCount);

			string columnName = TestData.LinesTempTableName + ".Description";
			AssertEquals("", DataProvider.GetColumnValue(rowSource, -1, columnName));
			AssertEquals("Unit test Line number 0                 ", DataProvider.GetColumnValue(rowSource, 0, columnName));
			AssertEquals("Unit test Line number 1                 ", DataProvider.GetColumnValue(rowSource, 1, columnName));
			AssertEquals("Unit test Line number 2                 ", DataProvider.GetColumnValue(rowSource, 2, columnName));
			AssertEquals("Unit test Line number 0                 ", DataProvider.GetColumnValue(rowSource, 3, columnName));
			AssertEquals("Unit test Line number 0                 ", DataProvider.GetColumnValue(rowSource, 4, columnName));
		}

		public void TestTrySwitchToEdwConnectionDuringTryAddDataSource()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[EDWData:DBName=SELECT DB_NAME() DBName]
{A}-[#EndOfReport]");

			var testDataProvider = new DataProviderForUnitTest(excelTemplate);
			using (var report = testDataProvider.Report)
			{
				report.IsEdwDataSource = true;
				Report.useMainConnection = false;
				report.PrepareForRender();

				testDataProvider.GetDataRowSource("DBName", false);
				AssertEquals(1, testDataProvider.DS.Tables.Count);
				AssertEquals(1, testDataProvider.DS.Tables[0].Rows.Count);
				Assert("This data is retrieved from EDW database", testDataProvider.DS.Tables[0].Rows[0][0].ToString().Contains("EDW"));
			}
		}

		public void TestTrySwitchToEdwConnectionDuringTryAddDataSource_EDWOnlyData()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[EDWOnlyData:DBName=SELECT DB_NAME() DBName]
{A}-[#EndOfReport]");

			var testDataProvider = new DataProviderForUnitTest(excelTemplate);
			using (var report = testDataProvider.Report)
			{
				Report.useMainConnection = false;
				report.PrepareForRender();

				testDataProvider.GetDataRowSource("DBName", false);
				AssertEquals(1, testDataProvider.DS.Tables.Count);
				AssertEquals(1, testDataProvider.DS.Tables[0].Rows.Count);
				Assert("This data is retrieved from EDW database", testDataProvider.DS.Tables[0].Rows[0][0].ToString().Contains("EDW"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProviderForDataSections()
		{
			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("MultipleDataSectionsWithFilters.xls", TestFilesSubFolder.ReportTestFiles)))
			{
				report.PrepareForRender();
				((TextField)report.FilterCollection[1]).Value = "JobShipmentPreplanning";
				var provider = report.DataProvider as ReportDataProvider;
				AssertNotNull("Provider for this report should be ADodataprovider", provider);

				var source1 = provider.GetDataRowSource("test1", false);
				Assert("Not a datasection so no filtering shoudl be applied should be serveral records", source1.RowCount > 1);
			}

			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("MultipleDataSectionsWithFilters.xls", TestFilesSubFolder.ReportTestFiles)))
			{
				report.PrepareForRender();
				((TextField)report.FilterCollection[1]).Value = "JobShipmentPreplanning";
				var provider = report.DataProvider as ReportDataProvider;
				AssertNotNull("Provider for this report should be ADodataprovider", provider);

				IDataRowSource source1 = provider.GetDataRowSource("test1", true);
				AssertEquals("is a data section so filtering should be applied and there should be only one record", 1, source1.RowCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataSet()
		{
			TestData.CreateDocEngineTestTable();
			var excelTemplate = new ExcelTemplateForUnitTesting("DataProviderTester.xls", TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(new DocumentPack(), excelTemplate))
			{
				rpt.PrepareForRender();
				var testDataProvider = rpt.DataProvider;

				AssertEquals(6, testDataProvider.GetDataRowSource("Country").RowCount);
				AssertEquals(TestData.DocEngineTestTableRowCount, testDataProvider.GetDataRowSource("Test").RowCount);
				AssertEquals(23, testDataProvider.GetDataRowSource("Loco").RowCount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCSharpDataProvider()
		{
			TestData.CreateDocEngineTestTable();
			var excelTemplate = new ExcelTemplateForUnitTesting("DataProviderTester.xls", TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(new DocumentPack(), excelTemplate))
			{
				rpt.PrepareForRender();
				var testDataProvider = (ReportDataProvider)rpt.DataProvider;

				var testDataProviderType = Type.GetType("Enterprise.DocumentEngine.DataProviders.Testing.ReportDataProviderTest+UnitTestingDocumentDataProvider", true);
				testDataProvider.tableProviderFactoryForTesting.TableProviderTypesForTesting["UnitTesting"] = testDataProviderType.FullName;
				rpt.FilterCollection.Add(new PrimaryKeyFilter("Test", Guid.NewGuid()));
				testDataProvider.AddDataSource("UnitTesting:UnitTesting");

				AssertEquals(4, testDataProvider.GetDataRowSource("UnitTesting").RowCount);
			}
		}

		public void TestProviderFactoryDataTableProvider()
		{
			AssertProviderFactory(typeof(UnitTestingDocumentDataProvider), "MyDataProvider");
			AssertProviderFactory(typeof(UnitTestingDocumentDataProvider), "  MyDataProvider   ");
			AssertProviderFactory(typeof(UnitTestingDocumentDataProvider), "  MyDataProvider ( foo, <bar> 42! )");
			AssertProviderFactory(typeof(UnitTestingDocumentDataProvider), "MyDataProvider()");
			AssertProviderFactory(typeof(UnitTestingDocumentDataProvider), "MyDataProvider()");
		}

		public void TestProviderFactoryNativeSQL()
		{
			AssertProviderFactory(typeof(NativeSqlTableProvider), "SELECT * FROM MyDataProvider");
			AssertProviderFactory(typeof(NativeSqlTableProvider), " sElEcT * FROM MyDataProvider");
			AssertProviderFactory(typeof(NativeSqlTableProvider), "  SELECT * FROM sys.objects");
			AssertProviderFactory(typeof(NativeSqlTableProvider), "sys.objects");
			AssertProviderFactory(typeof(NativeSqlTableProvider), "this doesn't exist anywhere!");
			AssertProviderFactory(typeof(NativeSqlTableProvider), " DELIVERANCEDELIVER:");
			AssertProviderFactory(typeof(NativeSqlTableProvider), " dElIvErAnCeDeLiVer:blahblah");
		}

		public void TestDocengineHandlesSort()
		{
			Report rpt = DataProvider.Report;
			rpt.Analyser.ReportSQLSources.Clear();
			rpt.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, "data:MyDataProviderWithNoSortHandling"));

			PrimaryKeyFilter flt = new PrimaryKeyFilter("", Guid.Empty);
			DataProvider.Report.FilterCollection.Add(flt);
			DataProvider.Report.SortOrderCollection.Add("test", "col1").Selected = true;
			DataProvider.AddDataSource("data:MyDataProviderWithNoSortHandling", true);
			AssertEquals(1, DataProvider.GetColumnValue(DataProvider.GetDataRowSource("data"), 0, "data.col1"));
		}

		public void TestDocengineLeavesSort()
		{
			Report rpt = DataProvider.Report;
			rpt.Analyser.ReportSQLSources.Clear();
			rpt.Analyser.ReportSQLSources.Add((DataSourceTypes.NormalData, "data:MyDataProviderWithSortHandling"));

			PrimaryKeyFilter flt = new PrimaryKeyFilter("", Guid.Empty);
			DataProvider.Report.FilterCollection.Add(flt);
			DataProvider.Report.SortOrderCollection.Add("test", "col1").Selected = true;
			DataProvider.AddDataSource("data:MyDataProviderWithSortHandling");
			AssertEquals(4, DataProvider.GetColumnValue(DataProvider.GetDataRowSource("data"), 0, "data.col1"));
		}

		public void TestAddDatasource_NoExtraCommandAfterCacheHit()
		{
			using (ReportDataProvider.EnableReportSqlResultCache(true))
			{
				var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
		@"{A}-[#Config]
{A}-[Data:ReportData=select top 1 * from dbo.GlbCompany]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.GC_PK>]
{A}-[#EndOfReport]");
				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, template))
				{
					report.PrepareForRender();
					var provider1 = new ReportDataProvider(report);
					provider1.AddDataSource("ReportData:select top 1 * from dbo.GlbCompany");
					var previousCommanCount = Db.Connection.ExecutedCommandCount;
					var provider2 = new ReportDataProvider(report);
					provider2.AddDataSource("ReportData:select top 1 * from dbo.GlbCompany");

					AssertEquals("Should not execute extra Db Command.", 0, Db.Connection.ExecutedCommandCount - previousCommanCount);
				}
			}
		}

		public void TestAddDataSource_MultipleThreadWithSameReportShouldDBQueryOneByOne()
		{
			using (ReportDataProvider.EnableReportSqlResultCache(true))
			{
				var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Data:ReportData=select top 1 * from dbo.GlbCompany]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.GC_PK>]
{A}-[#EndOfReport]");
				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, template))
				{
					report.PrepareForRender();
					var task1 = new Thread(() =>
					{
						AddDataSourceForTest(report);
					});
					task1.Start();
					task1.Join();

					var dbHitCount = 0;
					var task2 = new Thread(() =>
					{
						dbHitCount += AddDataSourceForTest(report);
					});
					task2.Start();
					task2.Join();

					Assert("Task2 should have db hit.", dbHitCount > 0);
				}
			}

			int AddDataSourceForTest(Report reportForTest)
			{
				using (Db.DisposableActionForDbConnection())
				{
					var previousCommanCount = Db.Connection.ExecutedCommandCount;
					var provider1 = new ReportDataProvider(reportForTest);
					provider1.AddDataSource("ReportData:select top 1 * from dbo.GlbCompany");
					return Db.Connection.ExecutedCommandCount - previousCommanCount;
				}
			}
		}

		public void TestAddDatasource_NoExceptionThrownWhenDeliveryWithCsvAndXlsx()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			var contents = @"{A}-[#Config]
{A}-[Data:ReportData=select top 1 <ReportData.SelectList> from dbo.GlbCompany]
{A}-[#DocumentHeader]
{A}-[#PageHeader]
{A}-[#SectionPageHeader]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.GC_Name>]
{A}-[#GroupBy:ReportData.GC_Name:GroupTitle]
{B}-[<ReportData.GC_Name>(<ReportData.GC_Code>)]
{A}-[#LastPageFooter]
{A}-[#EndOfReport]";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report2", contents);
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report2";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using var documentPack = new DocumentPack(reportCommand);
			documentPack.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
			var recipient1 = documentPack.DeliveryInstructions.Recipients.AddNew();
			recipient1.DeliveryMethod = ContactNotifyModes.Email;
			recipient1.AttachmentType = AttachmentTypeList.Codes.CsvWithHeadings;
			recipient1.Email = @"unit.test@cargowise.com";
			var recipient2 = documentPack.DeliveryInstructions.Recipients.AddNew();
			recipient2.DeliveryMethod = ContactNotifyModes.Email;
			recipient2.AttachmentType = AttachmentTypeList.Codes.Xlsx;
			recipient2.Email = @"unit.test@cargowise.com";

			var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduledReport.PopulateDefaultsFromDeliveryInstructions(documentPack.DeliveryInstructions);
			AssertEquals("Pre-condition: There should be 2 recipient.", 2, scheduledReport.Recipients.Count);
			AssertNoExceptionThrown("No Exception Thrown When Delivery with csv and xlsx", () => scheduledReport.Run());
		}

		public void TestAddDatasource_ContentOfReportShouldBeSameForMultipleRecipients()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			var contents = @"{A}-[#Config]
							{A}-[Name=Test Report]
							{A}-[Data:TestData=Select CONVERT(varchar(100), GETDATE(), 25) as Date]
							{A}-[#SectionBody:Data=TestData]
							{B}-[<TestData.Date>]
							{A}-[#endofreport]";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Report1", contents);
			var reportCommand = Factory.New<ReportCommand>();
			reportCommand.SU_MenuName = "Test Report1";
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;
			using var documentPack = new DocumentPack(reportCommand);
			documentPack.DeliveryInstructions.Recipients.RemoveAndDeleteAll();
			var recipient1 = documentPack.DeliveryInstructions.Recipients.AddNew();
			recipient1.DeliveryMethod = ContactNotifyModes.Email;
			recipient1.AttachmentType = AttachmentTypeList.Codes.Xlsx;
			recipient1.Email = @"unit.test@cargowise.com";
			var recipient2 = documentPack.DeliveryInstructions.Recipients.AddNew();
			recipient2.DeliveryMethod = ContactNotifyModes.Email;
			recipient2.AttachmentType = AttachmentTypeList.Codes.CsvWithHeadings;
			recipient2.Email = @"unit.test@cargowise.com";

			var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduledReport.PopulateDefaultsFromDeliveryInstructions(documentPack.DeliveryInstructions);
			AssertEquals("Pre-condition: There should be 2 recipient.", 2, scheduledReport.Recipients.Count);

			scheduledReport.Run();

			var deserializedValue = scheduledReport.CreateReportFromTask();
			var deserializedReport = deserializedValue.Report;
			var jobs = Factory.Load<StmPrintJob>(new ZQuery());

			AssertEquals("Pre-condition: There should be 2 jobs.", 2, jobs.Length);

			var jobOfCsv = jobs.Where(j => j.BlobType.Equals("CSV")).FirstOrDefault();
			var jobOfXlsx = jobs.Where(j => j.BlobType.Equals("XLSX")).FirstOrDefault();
			var contentOfCsv = jobOfCsv.SP_CustomProperties.ToUTF8();
			using var excelInterface = new ExcelInterface();
			using var stream = new MemoryStream(jobOfXlsx.SP_CustomProperties);
			excelInterface.LoadExcelFile(stream);
			var contentOfXlsx = excelInterface.WorkSheets[0][0, 1].ToString();
			//Xls report: {B}-[2024-11-01 14:12:07.833]
			//Csv report: "2024-11-01 14:12:07.833"\r\n
			var pattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}";
			var csvTestDate = Regex.Match(contentOfCsv, pattern).Value;
			var xlsTestDate = Regex.Match(contentOfXlsx, pattern).Value;
			AssertNotNullOrEmpty("Content of CSV should not be empty.", csvTestDate);
			AssertNotNullOrEmpty("Content of XLSX should not be empty.", xlsTestDate);
			AssertEquals("data of CSV report should be the same as Xls report",csvTestDate, xlsTestDate);
		}

		public void TestAddDatasource_DataTableCanBeChangedWhenCacheIsOff()
		{
			AssertEquals(0, DataProvider.DS.Tables.Count);
			AssertEquals("AUD                                     ", DataProvider.GetColumnValue(DataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));

			UpdateLinesCurrency("USD");
			DataProvider.DS.Tables.RemoveAt(0);
			AssertEquals(0, DataProvider.DS.Tables.Count);
			AssertEquals("Currency should be changed because the cache is off.", "USD                                     ", DataProvider.GetColumnValue(DataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));
		}

		public void TestAddDatasource_DataTableCannotBeChangedWhenCacheIsOn()
		{
			using (ReportDataProvider.EnableReportSqlResultCache(true))
			{
				AssertEquals(0, DataProvider.DS.Tables.Count);
				AssertEquals("AUD                                     ", DataProvider.GetColumnValue(DataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));

				UpdateLinesCurrency("USD");
				AssertEquals("Currency should not be changed because the cache is on.", "AUD                                     ", DataProvider.GetColumnValue(DataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddDatasource_ShouldNotHitCacheWhenSheetNameIsDifferent()
		{
			var testDataProvider = new DataProviderForUnitTest(new ExcelTemplateForUnitTesting("NewStyleMulti-Sheet.xls", TestFilesSubFolder.ReportTestFiles));
			using (var report = testDataProvider.Report)
			using (ReportDataProvider.EnableReportSqlResultCache(true))
			{
				report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[0]);

				AssertEquals(0, testDataProvider.DS.Tables.Count);
				AssertEquals("AUD                                     ", testDataProvider.GetColumnValue(testDataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));

				UpdateLinesCurrency("USD");
				AssertEquals("Should hit cache for the same sheet.", "AUD                                     ", testDataProvider.GetColumnValue(testDataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));

				report.SetWorkSheetCurrentlyBeingProcessedForTestOnly(report.XlInterface.WorkSheets[1]);
				testDataProvider.DS.Tables.RemoveAt(0);
				AssertEquals(0, DataProvider.DS.Tables.Count);
				AssertEquals("Should not hit cache for different sheet.", "USD                                     ", testDataProvider.GetColumnValue(testDataProvider.GetDataRowSource("Lines"), 0, "Lines.Currency"));
			}
		}

		public void TestReportDataProviderShouldHaveIndependentCacheForDifferentReport()
		{
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "JTOTST1";
			org1.OH_FullName = "Jerry Test Organisation 1";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "JTOTST2";
			org2.OH_FullName = "Jerry Test Organisation 2";

			Factory.Save();

			var content = @"{A}-[#Config]
{A}-[Name=Jerry Test]
{A}-[#EndOfReport]";
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Jerry Test", string.Empty, content);
			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Jerry Simple Test", template, Factory);

			using (ReportDataProvider.EnableReportSqlResultCache(true))
			{
				var docPack1 = new DocumentPack(reportCommand);
				var valueInReport1 = "";
				using (var report = (Report)docPack1[0])
				{
					var dataProvider = report.DataProvider as ReportDataProvider;
					dataProvider?.AddDataSource(string.Format("ReportData:SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = '{0}'", org1.PK), true);
					valueInReport1 = report.DataProvider.GetColumnValue(null, 0, "ReportData.OH_FullName") as string;
				}

				var docPack2 = new DocumentPack(reportCommand);
				var valueInReport2 = "";
				using (var report = (Report)docPack2[0])
				{
					var dataProvider = report.DataProvider as ReportDataProvider;
					dataProvider?.AddDataSource(string.Format("ReportData:SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = '{0}'", org2.PK), true);
					valueInReport2 = report.DataProvider.GetColumnValue(null, 0, "ReportData.OH_FullName") as string;
				}

				AssertEquals("Jerry Test Organisation 1", valueInReport1);
				AssertEquals("Jerry Test Organisation 2", valueInReport2);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateDocEngineTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateJobTestTable();
			TestData.CreateLinesTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
			dataProvider?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}

		DataProviderForUnitTest dataProvider;
		DataProviderForUnitTest DataProvider => dataProvider ?? (dataProvider = new DataProviderForUnitTest(NewStyleTemplate));

		void UpdateLinesCurrency(string currency)
		{
			var sb = new StringBuilder();
			sb.Append(@"UPDATE " + TestData.LinesTempTableName + @"
SET Currency = '" + currency + "'" + @"
WHERE Description = 'Unit test Line number 0                 '");
			Db.Connection.ExecuteNonQuery(sb.ToString());
		}

		void AssertProviderFactory(Type expectedProviderType, string dataSource)
		{
			AssertNotNull("Set ExpectedProviderType before calling this", expectedProviderType);
			Assert("Expected " + expectedProviderType.Name + " for " + dataSource, expectedProviderType.IsAssignableFrom(DataProvider.GetProvider(dataSource).GetType()));
		}

		[CodeAlive(@"This is used by reflection in ReportDataProvider.cs.")]
		sealed class UnitTestingDocumentDataProviderWithNoSortHandling : DocumentDataTableProvider
		{
			public override bool HandlesSortInternally
			{
				get { return false; }
			}

			public static TableProvider New()
			{
				return new UnitTestingDocumentDataProviderWithNoSortHandling();
			}

			protected override DataTable GetDataTable(Guid pK)
			{
				DataTable tbl = new DataTable();
				tbl.Columns.Add("col1", typeof(int));
				tbl.Columns.Add("col2", typeof(string));
				tbl.Rows.Add(new object[] { 4, "test4" });
				tbl.Rows.Add(new object[] { 3, "test3" });
				tbl.Rows.Add(new object[] { 2, "test2" });
				tbl.Rows.Add(new object[] { 1, "test1" });
				return tbl;
			}
		}

		[CodeAlive(@"This is used by reflection in ReportDataProvider.cs.")]
		sealed class UnitTestingDocumentDataProviderWithInternalSortHandling : DocumentDataTableProvider
		{
			public static TableProvider ConstructorWrapper()
			{
				return new UnitTestingDocumentDataProviderWithInternalSortHandling();
			}

			protected override DataTable GetDataTable(Guid pK)
			{
				DataTable tbl = new DataTable();
				tbl.Columns.Add("col1", typeof(int));
				tbl.Columns.Add("col2", typeof(string));
				tbl.Rows.Add(new object[] { 4, "test4" });
				tbl.Rows.Add(new object[] { 3, "test3" });
				tbl.Rows.Add(new object[] { 2, "test2" });
				tbl.Rows.Add(new object[] { 1, "test1" });
				return tbl;
			}
		}

		sealed class UnitTestingDocumentDataProvider : DocumentDataTableProvider
		{
			public static TableProvider New()
			{
				return new UnitTestingDocumentDataProvider();
			}

			protected override DataTable GetDataTable(Guid pK)
			{
				DataTable tbl = new DataTable();
				tbl.Columns.Add("col1", typeof(int));
				tbl.Columns.Add("col2", typeof(string));
				tbl.Rows.Add(new object[] { 1, "test1" });
				tbl.Rows.Add(new object[] { 2, "test2" });
				tbl.Rows.Add(new object[] { 3, "test3" });
				tbl.Rows.Add(new object[] { 4, "test4" });

				return tbl;
			}
		}

		sealed class DataProviderForUnitTest : ReportDataProvider, IDisposable
		{
			public DataProviderForUnitTest(ExcelTemplate templateForTesting)
				: base(new Report(new DocumentPack(), templateForTesting))
			{
				var typeDataProvider = Type.GetType("Enterprise.DocumentEngine.DataProviders.Testing.ReportDataProviderTest+UnitTestingDocumentDataProvider", true);
				var typeDataProviderWithNoSort = Type.GetType("Enterprise.DocumentEngine.DataProviders.Testing.ReportDataProviderTest+UnitTestingDocumentDataProviderWithNoSortHandling", true);
				var typeDataProviderWithSort = Type.GetType("Enterprise.DocumentEngine.DataProviders.Testing.ReportDataProviderTest+UnitTestingDocumentDataProviderWithInternalSortHandling", true);

				tableProviderFactoryForTesting.TableProviderTypesForTesting["MyDataProvider"] = typeDataProvider.FullName;
				tableProviderFactoryForTesting.TableProviderTypesForTesting["MyDataProviderWithNoSortHandling"] = typeDataProviderWithNoSort.FullName;
				tableProviderFactoryForTesting.TableProviderTypesForTesting["MyDataProviderWithSortHandling"] = typeDataProviderWithSort.FullName;

				Report.PrepareForRender();
			}

			public new DataSet DS
			{
				get { return base.DS; }
			}

			public new Report Report
			{
				get { return base.Report; }
			}

			public new object FormatColumnValue(object columnValue)
			{
				return base.FormatColumnValue(columnValue);
			}

			public new TableProvider GetProvider(string dataSource)
			{
				return base.GetProvider(dataSource);
			}

			public void Dispose()
			{
				if (Report != null)
				{
					Report.Dispose();
				}
			}
		}
	}
}
