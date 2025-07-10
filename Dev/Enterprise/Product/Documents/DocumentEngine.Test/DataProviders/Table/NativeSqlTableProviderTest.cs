using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class NativeSqlTableProviderTest : TransactionedTestCase
	{
		public void TestFillDataTableShouldIgnoreXmlPlanTable()
		{
			using (RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestReport.stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
				TestReport.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				var table = provider.GetDataTable("Dummy Table Name", $"exec ep_DbConfigInfo @dbname='{Db.DatabaseName}'", TestReport, true);

				var columnsExpected = new List<string>
				{
					"MAXDOP",
					"CostOfParallelism",
					"OptimizeForAdHocWorkloads",
					"MinMemory",
					"MaxMemory",
					"TotalAvailableMemory",
					"TraceFlags",
					"DbAlwaysOn",
					"ParameterizationForced",
					"AutoCreate",
					"AutoUpdate",
					"AutoUpdateAsync",
					"CompatibilityLevel",
					"Cardinality"
				};

				var realColumns = new List<string>();
				table.Columns.OfType<DataColumn>().ForEach(c => realColumns.Add(c.ColumnName));

				AssertContainsExactElementsInAnyOrder(columnsExpected, realColumns);
			}
		}

		public void TestFillDataTable_UseStatisticAndRecompileQueryHint()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
			"Test", string.Empty,
			@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[Data:ShipmentData=SELECT '1' as Column1]
{A}-[Data:ReportData=SELECT <ShipmentData.Column1> as Column2]
{A}-[#SectionBody:Data=ReportData]
{A}-[<ReportData.Column2>]
{A}-[#EndOfReport]");

			SqlParameterNameGenerator.Reset();

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			using (RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.UseRecompileQueryHint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				report.stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
				report.PrepareForRender();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);
				}

				Assert("Report should have no error.", !report.ErrorManager.HasErrors);
				AssertNotNull(report.stmReportRun);
				AssertNullOrEmpty(report.stmReportRun.RRI_ExecutionPlanText);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFillDataTable_ExcludesParametersByDefault()
		{
			try
			{
				SqlParameterNameGenerator.Reset();
				SetupDummyReport();

				var expectedParameterComments = "--@p0, VarChar, \"AU\";";
				TestReport.stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
				TestReport.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				provider.IncludeParameterOverrideForTest = false;
				provider.GetDataTable("Dummy Table Name", "SELECT Column1 FROM Report_DummyReport(<CompanyCountryCode>)", TestReport, true);

				AssertNotContains("Should not contain parameter comments", expectedParameterComments, TestReport.stmReportRun.RRI_QueryText);
			}
			finally
			{
				TearDownDummyReport();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFillDataTable_IncludesParametersWhenRequired()
		{
			try
			{
				SqlParameterNameGenerator.Reset();
				SetupDummyReport();

				var expectedParameterComments = "--@p0, VarChar, \"AU\";";
				TestReport.stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
				TestReport.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				provider.IncludeParameterOverrideForTest = true;
				provider.GetDataTable("Dummy Table Name", "SELECT Column1 FROM Report_DummyReport(<CompanyCountryCode>)", TestReport, true);

				AssertContains("Should contain parameter comments", expectedParameterComments, TestReport.stmReportRun.RRI_QueryText);
			}
			finally
			{
				TearDownDummyReport();
			}
		}

		[DeveloperOnlyTest]
		public void TestExecutionPlanCodeByItself()
		{
			var dataSet = new DataSet();
			DataTable result = null;
			var connection = Db.Connection;
			string executionPlan = null;

			var sql = "select * from dbo.dummybizo where Z0_PK = 'DEADBEEF-5EA1-4684-B1D5-9E7C57E6C97E';";
			using (var adapter = connection.Command(sql).NewDataAdapter())
			{
				connection.ExecuteNonQuery("SET STATISTICS XML ON");
				adapter.Fill(dataSet);
				connection.ExecuteNonQuery("SET STATISTICS XML OFF");
				result = dataSet.Tables[0];
				AssertEquals(2, dataSet.Tables.Count);
				executionPlan = (string)dataSet.Tables[1].Rows[0][0];
			}
			AssertContains("<SHOWPLANXML ", executionPlan.ToUpperInvariant());
			AssertContains("</SHOWPLANXML>", executionPlan.ToUpperInvariant());
			//the exact number of columns is not particularly important, just making sure it's not busted
			Assert(result.Columns.Count.ToString(), result.Columns.Count >= 28 && result.Columns.Count <= 29);
			AssertEquals(0, result.Rows.Count);
		}

		public void TestQueryText()
		{
			MakeBehaviourWithPK();
			TestReport.stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
			new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName, TestReport, true);

			AssertContains("--Udf Parameters:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--WhereClause Parameters:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--Report Name:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--Staff Name:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--Time Out:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--Time Consumed:", TestReport.stmReportRun.RRI_QueryText);
			AssertContains("--Server:", TestReport.stmReportRun.RRI_QueryText);
		}

		public void TestReplaceSelectDataSourceWithSelectListMacro()
		{
			var provider = new NativeSqlTableProviderForTesting();

			AssertExceptionThrown<InvalidOperationException>(() =>
				provider.GetValidSelectStatement("ReportData", "Table1", null));

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
						"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[#SectionBody:Data=ReportData]
{A}-[<ReportData.Column1>]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals("SELECT [COLUMN1] FROM Table1", provider.GetValidSelectStatement("ReportData", "Table1", report));
			}
		}

		public void TestReplaceSelectDataSourceWithSelectListMacroAndMaximumrows()
		{
			var provider = new NativeSqlTableProviderForTesting();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
						"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[#SectionBody:Data=ReportData]
{A}-[<ReportData.Column1>]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals("SELECT TOP 10 [COLUMN1] FROM Table1", provider.GetValidSelectStatement("ReportData", "Table1", report, 10));
			}
		}

		public void TestErroneousSQLDoesThrowSQLExecutionException()
		{
			MakeEmptyEmptyBehaviour();
			try
			{
				DataTable tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT 1/0", TestReport, true);
			}
			catch (SQLExecutionException exception)
			{
				Assert(exception.Message.StartsWith("Error loading table [Dummy Table Name]. Error: [Divide by zero error encountered.]"));
			}
		}

		public void TestSimpleTableName()
		{
			MakeEmptyEmptyBehaviour();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(TestData.DocEngineTestTableRowCount, tbl.Rows.Count);
		}

		public void TestSimpleSQL()
		{
			MakeEmptyEmptyBehaviour();
			DataTable tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 10 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(10, tbl.Rows.Count);
		}

		public void TestFillTimeout()
		{
			var query2 = @"select top(50000) * from dbo.orgheader
join dbo.OrgContact on OC_PK != OH_PK
join dbo.OrgAddress on OA_PK != OC_PK";

			var provider = new NativeSqlTableProviderForTesting();

			TestReport.TimeOut = 120;
			DataTable result = null;
			try
			{
				result = provider.GetDataTable("Dummy Table Name", query2, TestReport, true);
				AssertNotNull(result);
			}
			catch (ReportSQLTimeoutException ex)
			{
				AssertEquals("Report query time out, not fill in data set time out", ex.Message, "Report query timed out");
			}

			TestReport.ResetTableProvidersTimesForTimeout();

			TestReport.TimeOut = 1;

			AssertExceptionThrown<ReportSQLTimeoutException>($"{nameof(ReportSQLTimeoutException)} should have been thrown.", () => provider.GetDataTable("Dummy Table Name", query2, TestReport, true));
		}

		public void TestTableNameWithWhereClause()
		{
			MakeBehaviourWithWhereClause();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
		}

		public void TestSQLWithWhereClause()
		{
			MakeBehaviourWithWhereClause();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
		}

		public void TestSQLWithSubqueryWhereClause()
		{
			MakeBehaviourWithWhereClause();
			var tbl = new NativeSqlTableProvider().GetDataTable(
				"Dummy Table Name",
				"SELECT *, (select count(*) FROM " + TestData.DocEngineTestTableName + " where CharField1='a') " +
				"FROM " + TestData.DocEngineTestTableName,
				TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
		}

		public void TestSQLWithSubqueryWhereClause2()
		{
			MakeBehaviourWithWhereClause();
			var tbl = new NativeSqlTableProvider().GetDataTable(
				"Dummy Table Name",
				"SELECT * FROM " + TestData.DocEngineTestTableName + " where CharField1 in (Select CharField1 from " + TestData.DocEngineTestTableName + ") ",
				TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
		}

		public void TestSQLWithGuid()
		{
			MakeBehaviourWithPK();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(1, tbl.Rows.Count);
		}

		public void TestSQLWithStoredProc()
		{
			MakeBehaviourWithPK();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "Exec sp_columns StmData", TestReport, true);
			Assert(tbl.Rows.Count > 0);
		}

		public void TestTableNameWithGuid()
		{
			MakeBehaviourWithPK();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(1, tbl.Rows.Count);
		}

		public void TestSimpleTableNameWithSortOrder()
		{
			MakeBehaviourWithSortOrder();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals("0                                       ", tbl.Rows[0]["CharField3"].ToString());
		}

		public void TestTimeoutError()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (Db.Connection.TemporarySetLockTimeout(DbConnection.LockTimeout.SqlDefault))
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				anotherConnection.BeginTransaction();

				try
				{
					var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
						"Test", string.Empty,
						@"{A}-[#Config]
	{A}-[Name=TestTemplate]
	{A}-[Version=1.3]
	{A}-[PageStyle=Portrait]
	{A}-[SqlTimeout=30]
			
	{A}-[#EndOfReport]");

					using (var documentPack = new DocumentPack())
					using (var report = new Report(documentPack, excelTemplate))
					{
						report.TimeOut = new ZInt(1);
						report.PrepareForRender();
						anotherConnection.ExecuteNonQuery("ALTER TABLE dbo.RefUnloco ADD RL_DummyColumn int null;");
						MakeBehaviourWithSortOrder();
						var provider = new NativeSqlTableProvider();
						AssertExceptionThrown<ReportSQLTimeoutException>(() => provider.GetDataTable("Dummy Table Name", "refUnLoco", report, true));
					}
				}
				finally
				{
					anotherConnection.RollbackTransaction();
				}
			}
		}

		[SnailTest]
		public void TestTimeoutErrorWithSeveralDataProviders()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[SqlTimeout=30]
{A}-[Data:ReportData1=select '1' column1 WAITFOR DELAY <<DelayString>>]
{A}-[Data:ReportData2=select '2' column1 WAITFOR DELAY <<DelayString>>]
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (DocumentsDataRegistry.Instance.UseRecompileQueryHint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.MacroTranslator.RegisterValueProvider(new DelegateValueProvider("DelayString", (macro, r) => { return "'00:00:20'"; }));
				report.PrepareForRender();
				AssertExceptionThrown<ReportSQLTimeoutException>(() => report.Renderer.Render());
			}
		}

		[ExpectNoExceptions]
		public void TestSQLParametersShouldNotBeTranslated()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[Data:ReportData= exec ('select 1 as ' + <Selected GroupBy Option>)]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Branch>]
{A}-[#GroupBy:ReportData.Branch:GroupTitle]
{B}-[<ReportData.Branch>]
{A}-[#EndOfReport]");

			using (DocumentsDataRegistry.Instance.UseRecompileQueryHint.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				try
				{
					var selectedGroupBy = new GroupBy("Branch", "ReportData.Branch")
					{
						DisplayNameLocalizedData = new ResourceStringData("key", "Niederlassung"), // 'Niederlassung' means 'Branch' in German.
						Selected = true
					};
					report.GroupByCollection.Add(selectedGroupBy);
					using (var stream = new MemoryStream())
					{
						report.Save(stream);
					}
				}
				catch (Exception e)
				{
					AssertNotContains("Should not throw specific exception.", "Severity: [Fatal] Message: [Error processing GroupBy columns - Could not find column [Branch].] Cell Content: []  Cell: [N/A] ", e.Message);
				}
			}
		}

		public void TestSQLQueryHints()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (recompile)"));
				}
			}
		}

		public void TestSQLQueryHintsWithHintAndRegistryItemSetToFalse()
		{
			DocumentsDataRegistry.Instance.UseRecompileQueryHint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					Assert("Query Hint is Appended", e.Message.Contains("SELECT * FROM dongisagenius option (recompile)"));
				}
			}
		}

		public void TestSQLQueryHintsForStoredProcedures()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with recompile"));
				}
			}
		}

		public void TestRemoveRecompileFromHints()
		{
			var provider = new NativeSqlTableProviderForTesting();

			string hints = "";
			AssertEquals("", provider.RemoveRecompileFromHintsForTest(hints));

			CombineAssertions(delegate
			{
				hints = "fast 10";
				AssertEquals("fast 10", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "recompile";
				AssertEquals("", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "recompile, fast 10";
				AssertEquals("fast 10", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "fast 10, recompile";
				AssertEquals("fast 10", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "fast 10, RECOMPILE";
				AssertEquals("fast 10", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "hint1 , recompile , hint2";
				AssertEquals("hint1, hint2", provider.RemoveRecompileFromHintsForTest(hints));

				hints = "recompile, OPTIMIZE FOR (@field = 'recompile', @recompile = 'bar'), Arecompile, fast 10, recompileB, MAXRECURSION 2, recompile, blabla, recompile";
				AssertEquals("OPTIMIZE FOR (@field = 'recompile', @recompile = 'bar'), Arecompile, fast 10, recompileB, MAXRECURSION 2, blabla", provider.RemoveRecompileFromHintsForTest(hints));
			});
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithHint()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile, fast 10]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (recompile, fast 10)"));
				}
			}
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithHintForStoredProcedures()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile, RESULT SETS UNDEFINED]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with recompile, RESULT SETS UNDEFINED"));
				}
			}
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithoutHint1()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=fast 10]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (fast 10, recompile)"));
				}
			}
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithoutHintForStoredProcedures1()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=RESULT SETS UNDEFINED]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with RESULT SETS UNDEFINED, recompile"));
				}
			}
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithoutHint2()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (recompile)"));
				}
			}
		}

		public void TestSQLQueryHintsWithoutIgnoreRecompileWithoutHintForStoredProcedures2()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query Hint Appended with RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with recompile"));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithHint()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile, fast 10]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query options appended without RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (fast 10)"));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithHintForStoredProcedures()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=recompile, RESULT SETS UNDEFINED]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query hints appended without RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with RESULT SETS UNDEFINED"));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithoutHint1()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=fast 10]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query hints appended without RECOMPILE?", true, e.Message.Contains("SELECT * FROM dongisagenius option (fast 10)"));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithoutHintForStoredProcedures1()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHints=RESULT SETS UNDEFINED]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Query options appended without RECOMPILE?", true, e.Message.Contains("Exec sp_fake stmdata with RESULT SETS UNDEFINED"));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithoutHint2()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "dongisagenius", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Error message contains query?", true, e.Message.Contains("SELECT * FROM dongisagenius"));
					AssertEquals("Any query hints appended?", false, e.Message.Contains("option ("));
				}
			}
		}

		public void TestSQLQueryHintsWithIgnoreRecompileWithoutHintForStoredProcedures2()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3	]
{A}-[PageStyle=Portrait]
{A}-[SqlQueryHintsIgnoreRecompile]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				var provider = new NativeSqlTableProvider();
				try
				{
					var tbl = provider.GetDataTable("Dummy Table Name", "Exec sp_fake stmdata", report, true);
				}
				catch (Exception e)
				{
					AssertEquals("Error message contains exec command?", true, e.Message.Contains("Exec sp_fake stmdata"));
					AssertEquals("Any query hints appended?", false, e.Message.Contains("Exec sp_fake stmdata with"));
				}
			}
		}

		public void TestSimpleSQLWithSortOrder()
		{
			MakeBehaviourWithSortOrder();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals("0                                       ", tbl.Rows[0]["CharField3"].ToString());
		}

		public void TestSQLWithWhereClauseAndSortOrder()
		{
			MakeBehaviourWithWhereClauseAndSortOrder();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
			AssertEquals("8                                       ", tbl.Rows[0]["CharField3"].ToString());
		}

		public void TestSQLWithWhereClauseAndSortOrderAndStoredProc()
		{
			MakeBehaviourWithWhereClauseAndSortOrder();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "Exec sp_columns StmNumberSequence", TestReport, true);
			AssertEquals(1, tbl.Rows.Count);
		}

		public void TestTableNameWithWhereClauseAndSortOrder()
		{
			MakeBehaviourWithWhereClauseAndSortOrder();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", TestData.DocEngineTestTableName, TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
			AssertEquals("8                                       ", tbl.Rows[0]["CharField3"].ToString());
		}

		public void TestSQLWithMacroReplacement()
		{
			TestReport.PrepareForRender();
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test", "1"));
			MakeEmptyEmptyBehaviour();
			var tbl = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT * FROM " + TestData.DocEngineTestTableName + " WHERE CharField1 = <Test>", TestReport, true);
			AssertEquals(40, tbl.Rows.Count);
		}

		public void TestFilterAppendWhere()
		{
			var dataSourceString = "SELECT * FROM " + TestData.DocEngineTestTableName + System.Environment.NewLine + "WHERE CharField1 = 'test'";
			TestReport.PrepareForRender();
			var testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField2";
			testTextField.Value = "2";
			TestReport.FilterCollection.Add(testTextField);

			var mockTableProvider = new MockTableProvider();
			_ = mockTableProvider.GetDataTable("Dummy Table Name", dataSourceString, TestReport, true);

			AssertEquals("Should only be a single where in the SQL", 1, HowManyWheresInThis(mockTableProvider.SQLString));
		}

		public void TestGetDataTable_AddWhereAndOrderByClause()
		{
			var mockTableProvider = new MockTableProvider();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
						"Test", string.Empty,
	@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[#SectionBody:Data=ReportData]
{A}-[<ReportData.Column1)>]
		
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();

				var testTextField = new TextField(new BusinessObjectFactory());
				testTextField.FieldName = "CharField";
				testTextField.Value = "2";
				report.FilterCollection.Add(testTextField);

				AssertNoExceptionThrown(() => mockTableProvider.GetDataTable("ReportData", "Table1 WHERE CharField != (SELECT 'AA' AS code) AND (1=1", report, true));
				AssertEquals("SELECT [COLUMN1)] FROM Table1 WHERE CharField != (SELECT 'AA' AS code) AND (1=1", mockTableProvider.SQLString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSQLWithTVPMacro()
		{
			var template = new ExcelTemplateForUnitTesting("MultipleSelectionLookup.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();

				var lookup = report.FilterCollection[1] as MultipleSelectionLookup;
				AssertNotNull("Precondition: MultipleSelectionLookup is chosen.", lookup);
				AssertEquals("Precondition: DisplayName", "Orgs", lookup.DisplayName);

				var factory = lookup.Factory;
				var collectionProvider = lookup.CollectionProvider;
				AssertEquals("Precondition: collectionProvider.Collection is empty", 0, collectionProvider.Collection.Count);

				var org1 = factory.New<OrgHeader>();
				org1.OH_Code = "1";
				collectionProvider.Collection.Add(org1);
				var org2 = factory.New<OrgHeader>();
				org2.OH_Code = "3";
				collectionProvider.Collection.Add(org2);

				string dataSourceString = string.Format("SELECT * FROM {0} JOIN <Orgs.CodesAsTVP> ON CharField1 = Value", TestData.DocEngineTestTableName);
				var tableProvider = new NativeSqlTableProvider();
				var table = tableProvider.GetDataTable("Dummy Table Name", dataSourceString, report, false);
				AssertEquals(80, table.Rows.Count);
			}
		}

		public void TestFilterWithMacro()
		{
			TestReport.PrepareForRender();
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test", 1));
			var mockTableProvider = new NativeSqlTableProvider();
			var tbl = mockTableProvider.GetDataTable("Dummy Table Name", "SELECT '<<NUMBER TO WORDS(<<Test>>)>>' as CharField2", TestReport, true);

			AssertEquals("one", tbl.Rows[0]["CharField2"].ToString());
		}

		[UseSnapshotProtection]
		public void TestQueriesAreRunUnderReaderAccount()
		{
			try
			{
				new NativeSqlTableProvider().GetDataTable("OrgHeader", "exec ('update dbo.ORGHEADER Set OH_Code = ''X''')", TestReport, false);
			}
			catch (SQLExecutionException ex)
			{
				Assert(ex.Message.Contains("229"));
			}
		}

		public void TestGetValidSelectStatementWithMaximumRowsSet()
		{
			MakeEmptyEmptyBehaviour();
			var provider = new NativeSqlTableProvider();
			var sql = provider.GetValidSelectStatement("SectionBody", "SELECT OH_CODE FROM dbo.OrgHeader", null, 10);
			AssertEquals("SELECT TOP 10 OH_CODE FROM dbo.OrgHeader", sql);

			sql = provider.GetValidSelectStatement("SectionBody", "SELECT DISTINCT OH_CODE FROM dbo.OrgHeader", null, 10);
			AssertEquals("SELECT DISTINCT TOP 10 OH_CODE FROM dbo.OrgHeader", sql);

			provider.GetDataTable("OrgHeader", "SELECT  OH_CODE FROM dbo.OrgHeader", TestReport, false, 10);
			Assert("No error message is added in errorManager", !TestReport.ErrorManager.HasErrors);

			provider.GetDataTable("OrgHeader", "SELECT TOP 10 OH_CODE FROM dbo.OrgHeader", TestReport, false, 10);
			Assert("Error message is added in errorManager", TestReport.ErrorManager.HasErrors);
			AssertEquals("Error message for using MaximumNumberOfRowsToShow macro and SELECT TOP at the same time is added",
				"Severity: [Error] Message: [You cannot use 'MaximumNumberOfRowsToShow' macro and a data source string including 'SELECT TOP' at the same time. Please check the data source string(SELECT TOP 10 OH_CODE FROM dbo.OrgHeader) and the section body area] Cell: [N/A] Sheetname: [(unknown)]"
				, TestReport.ErrorManager.ToString());
		}

		public void TestGetValidSelectStatementWithLFBehindSelect()
		{
			MakeEmptyEmptyBehaviour();
			var provider = new NativeSqlTableProvider();
			var sql = provider.GetValidSelectStatement("ReportData", "SELECT\r\nTestColumn FROM TestTable", TestReport);
			AssertEquals("SELECT  TestColumn FROM TestTable", sql);
		}

		public void TestDoesSqlHaveWhereClause()
		{
			var provider = new NativeSqlTableProviderForTesting();
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT * FROM dbo.GlbBranch WHERE GB_Code in ('ABC','DEF') /* whatever)) (*/ AND GB_IsValid = 1 "));
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT (SELECT 1 FROM dbo.GLBCOMPANY WHERE GC_CODE != ')'),* FROM dbo.GLBBRANCH WHERE GB_CODE IN ( SELECT GS_CODE FROM dbo.GLBSTAFF WHERE GS_CODE != 'HFG)' ) AND GB_BRANCHNAME != 'TEST)'"));
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT (SELECT 1 FROM dbo.GLBCOMPANY WHERE GC_CODE != '('),* FROM dbo.GLBBRANCH WHERE GB_CODE IN ( SELECT GS_CODE FROM dbo.GLBSTAFF WHERE GS_CODE != 'HFG)' ) AND GB_BRANCHNAME != 'TEST)'"));
			AssertEquals(false, provider.ExposedDoesSqlHaveWhereClause("SELECT *, (SELECT TOP 1 column FROM Table2 WHERE column = (SELECT column FROM Table3)) FROM Table1"));
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT * FROM Table1 WHERE column = (SELECT TOP 1 column FROM Table2 WHERE column = (SELECT TOP 1 FROM Table3))"));

			AssertEquals(false, provider.ExposedDoesSqlHaveWhereClause("SELECT *, (SELECT TOP 1 column FROM Table2 WHERE column = SELECT column FROM Table3)) FROM Table1"));
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT * FROM Table1 WHERE column = (SELECT TOP 1 column FROM Table2 WHERE column = (SELECT TOP 1 FROM Table3"));
			AssertEquals(true, provider.ExposedDoesSqlHaveWhereClause("SELECT * FROM Table1 WHERE column = SELECT TOP 1 column FROM Table2 WHERE column = (SELECT TOP 1 FROM Table3))"));
		}

		public void TestMaxDopOption()
		{
			MakeEmptyEmptyBehaviour();
			var provider = new NativeSqlTableProvider();
			var execProcedure = "EXEC " + TestData.DocEngineTestProcedure;

			using (DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, -2))
			{
				TestReport.MaxDop = -3;
				AssertEquals(false, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(-2, provider.GetMaxDopValue(TestReport));
				var table1 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table1.Rows.Count);

				TestReport.MaxDop = 0;
				AssertEquals(false, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(-2, provider.GetMaxDopValue(TestReport));
				var table2 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table2.Rows.Count);

				TestReport.MaxDop = 65;
				AssertEquals(true, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(65, provider.GetMaxDopValue(TestReport));
				var table3 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table3.Rows.Count);
				var table4 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", execProcedure, TestReport, true);
				AssertEquals(1, table4.Rows.Count);
			}

			using (DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				TestReport.MaxDop = -3;
				AssertEquals(false, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(0, provider.GetMaxDopValue(TestReport));
				var table1 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table1.Rows.Count);

				TestReport.MaxDop = 0;
				AssertEquals(false, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(0, provider.GetMaxDopValue(TestReport));
				var table2 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table2.Rows.Count);

				TestReport.MaxDop = 65;
				AssertEquals(true, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(65, provider.GetMaxDopValue(TestReport));
				var table3 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table3.Rows.Count);
				var table4 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", execProcedure, TestReport, true);
				AssertEquals(1, table4.Rows.Count);
			}

			using (DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 32))
			{
				TestReport.MaxDop = -3;
				AssertEquals(false, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(32, provider.GetMaxDopValue(TestReport));
				var table1 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table1.Rows.Count);

				TestReport.MaxDop = 0;
				AssertEquals(true, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(32, provider.GetMaxDopValue(TestReport));
				var table2 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table2.Rows.Count);

				TestReport.MaxDop = 65;
				AssertEquals(true, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(65, provider.GetMaxDopValue(TestReport));
				var table3 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table3.Rows.Count);
				var table4 = new NativeSqlTableProvider().GetDataTable("Dummy Table Name", execProcedure, TestReport, true);
				AssertEquals(1, table4.Rows.Count);
			}
		}

		public void TestMaxDopOption_WhenAnalyzerIsNull()
		{
			var provider = new NativeSqlTableProviderForTesting();

			using (DocumentsDataRegistry.Instance.ReportDBCommandMaximumDegreeOfParallelism.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			{
				AssertNull(TestReport.Analyser);
				AssertEquals(true, provider.ShouldAddMaxDopOption(TestReport));
				AssertEquals(4, provider.GetMaxDopValue(TestReport));
				var table = provider.GetDataTable("Dummy Table Name", "SELECT TOP 5 * FROM " + TestData.DocEngineTestTableName, TestReport, true);
				AssertEquals(5, table.Rows.Count);
			}
		}

		[UseSnapshotProtection]
		public void TestGetDataTable_WhenUseStatisticsWithoutPermission_ShouldNotThrowException()
		{
			using (RawDataRegistry.Instance.ReportStatisticsLogExecutionPlan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (new DisposableAction(
			   () => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
			   () =>
			   {
				   Db.ConnectionOverrideForTest.Dispose();
				   Db.ConnectionOverrideForTest = null;
			   }))
			{
				Db.Connection.ExecuteNonQuery("REVOKE SHOWPLAN FROM " + Db.Connection.UserLogin);
				using (var adminConnection = Db.NewAdminConnection())
				{
					((ICurrentDbControl)adminConnection).UseDatabase(RefDbTableNameResolver.SingleRefDatabaseName);
					try
					{
						adminConnection.ExecuteNonQuery("REVOKE SHOWPLAN FROM GUEST");
						var stmReportRun = new BusinessObjectFactory().New<StmReportRun>();
						TestReport.stmReportRun = stmReportRun;
						var provider = new NativeSqlTableProvider();
						AssertNoExceptionThrown("GetDataTable should retry rather than throw Exception without SHOWPLAN permission.", () => provider.GetDataTable("Dummy Table Name", "SELECT * FROM RefDatabase_RefExchangeRateZZ", TestReport, false));
						var allNotes = stmReportRun.Notes.GetAllNotes();
						AssertEquals(1, allNotes.Count);
						var note = allNotes.First() as StmNote;
						AssertEquals("Permission denied in database", note.ST_Description);
						AssertEquals(nameof(StmNoteVisibility.PUB), note.ST_NoteType);
						Assert(note.ST_NoteDataAsText.Contains($"SHOWPLAN permission denied in database '{RefDbTableNameResolver.SingleRefDatabaseName}'."));
					}
					finally
					{
						adminConnection.ExecuteNonQuery("GRANT SHOWPLAN TO GUEST");
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestData.CreateDocEngineTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateJobTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestProcedure();

			filterCollectionItem = new CollectionOfIFilter();
			sortOrderCollectionItem = new SortOrderCollection();
			dataSourceParameters = new StringCollection();
			pk = TestData.GetFirstGuidInTestTable();
			pack = new DocumentPack();

			registry.ReportingDbServerNames = Array.Empty<string>();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			testReport?.Dispose();
			embeddedResourceRetriever?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;
		readonly SystemDataRegistryForTest registry = SystemDataRegistryForTest.Get();
		EmbeddedResourceRetriever embeddedResourceRetriever;
		CollectionOfIFilter filterCollectionItem;
		SortOrderCollection sortOrderCollectionItem;
		StringCollection dataSourceParameters;
		Guid pk;
		DocumentPack pack;

		Report testReport;
		Report TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					var excelTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
					testReport = new Report(pack, excelTemplate);
				}
				return testReport;
			}
		}

		void MakeEmptyEmptyBehaviour()
		{
			TestReport.PrepareForRender();
		}

		void TearDownDummyReport()
		{
			var reportSql = "DROP FUNCTION Report_DummyReport";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void SetupDummyReport()
		{
			var reportSql = @"
CREATE FUNCTION Report_DummyReport(@parameter as VARCHAR(3))
RETURNS TABLE
AS
RETURN
(
	SELECT 'AU' AS Column1
)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.ExecuteNonQuery();
			}

			var excelTemplate = new ExcelTemplateForUnitTesting("ReportWithParameterInData2.xls", TestFilesSubFolder.ReportTestFiles);
			testReport = new Report(pack, excelTemplate);
		}

		void MakeBehaviourWithWhereClause()
		{
			TestReport.PrepareForRender();
			var testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField1";
			testTextField.Value = "2";
			TestReport.FilterCollection.Add(testTextField);
		}

		void MakeBehaviourWithPK()
		{
			TestReport.PrepareForRender();
			var pKFilter = new PrimaryKeyFilter("UnitTestID", TestData.GetFirstGuidInTestTable());
			TestReport.FilterCollection.Add(pKFilter);
		}

		void MakeBehaviourWithSortOrder()
		{
			TestReport.PrepareForRender();
			var so = new RuntimeOptions.SortOrder("Test", "CharField3");
			TestReport.SortOrderCollection.Add(so);
		}

		void MakeBehaviourWithWhereClauseAndSortOrder()
		{
			TestReport.PrepareForRender();
			var testTextField = new TextField(new BusinessObjectFactory());
			testTextField.FieldName = "CharField1";
			testTextField.Value = "2";
			TestReport.FilterCollection.Add(testTextField);
			var so = new RuntimeOptions.SortOrder("Test", "CharField3");
			TestReport.SortOrderCollection.Add(so);
		}

		int HowManyWheresInThis(string dataSourceString)
		{
			var whereCount = 0;
			var dataSourceStringList = dataSourceString.Split(' ');
			foreach (var dataSourceStringPart in dataSourceStringList)
			{
				if (dataSourceStringPart.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase) > -1)
				{
					whereCount++;
				}
			}
			return whereCount;
		}

		sealed class MockTableProvider : NativeSqlTableProviderForTesting
		{
			public string SQLString = "";
			public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool isFirstTable, int maximumNumberOfRows = -1)
			{
				SQLString = GetValidSelectStatement(tableName, dataSourceString, report, maximumNumberOfRows);
				AddWhereAndOrderByClause(SQLString, report);

				return null;
			}

			public DataTable GetDataTableWithoutOrderAndWhereCaluses(string tableName, string dataSourceString, Report report, bool isFirstTable)
			{
				return base.GetDataTable(tableName, dataSourceString, report, isFirstTable);
			}
		}
	}
}
