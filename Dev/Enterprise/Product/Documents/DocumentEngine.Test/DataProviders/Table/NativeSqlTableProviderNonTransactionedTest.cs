using System;
using CargoWise.Data;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class NativeSqlTableProviderNonTransactionedTest : TestCase
	{
		public void TestMetadataChangedDuringSnapshotIsolationThrowsMetadataHasChangedException()
		{
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
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
					var provider = new NativeSqlTableProviderForTesting();
					provider.ShouldProduceSnapshotIsolationError = true;
					provider.GetDataTable("Dummy Table Name", "select  * from dbo.DummyBizo", report, true);
					anotherConnection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo REBUILD WITH (ONLINE = ON);");
					AssertExceptionThrown<MetadataHasChangedException>(() => provider.GetDataTable("Dummy Table Name", "DummyBizo", report, true));
				}
			}
		}

		public void TestReplaceParametersForSQLQueryHintsTable()
		{
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
					"Test", string.Empty,
	@"{A}-[#Config]
	{A}-[Name=TestTemplate]
	{A}-[Version=1.3	]
	{A}-[PageStyle=Portrait]
	{A}-[Data:ReportData=select * from dbo.DummyBizo where Z0_Number=<Test1>]
	{A}-[SqlQueryHints=OPTIMIZE FOR (<Test1> UNKNOWN)]
	{A}-[#SectionBody:Data=ReportData]	
	{A}-[#EndOfReport]");
				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate))
				{
					SqlParameterNameGenerator.Reset();
					report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Test1", "AB<"));
					report.PrepareForRender();
					var provider = new NativeSqlTableProvider();
					try
					{
						var tbl = provider.GetDataTable("Dummy Table Name", "select * from dbo.DummyBizo where Z0_Number=<Test1>", report, true);
					}
					catch (Exception e)
					{
						var expectedString = "option (OPTIMIZE FOR (@p0 UNKNOWN), recompile)";
						AssertEquals(true, e.Message.Contains(expectedString));
					}
				}
			}
		}
	}
}
