using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class SingleSectionBodyDataAreaReportRendererTest : TestCaseWithFactory
	{
		public void TestRender()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				report.Renderer = new SingleSectionBodyDataAreaReportRenderer(report);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("The document header should of been removed.",
@"{B}-[1]   {C}-[One]
{B}-[2]   {C}-[Two]
{B}-[3]   {C}-[Three]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestRender_NoSectionBody()
		{
			CreateDummyBusinessObject(1, "One");
			CreateDummyBusinessObject(2, "Two");
			CreateDummyBusinessObject(3, "Three");

			Factory.Save();

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#DocumentHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#EndOfReport]");

			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				report.Renderer = new SingleSectionBodyDataAreaReportRenderer(report);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("The document should be empty.", string.Empty,
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestRemovedAreasWithFields()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT * FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_VarCharMax>]
{A}-[#GroupBy:ReportData.Z0_Number:GroupTitle]
{B}-[<ReportData.Z0_Number>(<ReportData.Z0_VarCharMax>)]
{A}-[#EndOfReport]");

			using var report = new Report(new DocumentPack(), excelTemplate);
			var renderer = new SingleSectionBodyDataAreaReportRenderer(report);
			report.Renderer = renderer;

			using var stream = new MemoryStream();
			report.Save(stream);

			AssertEquals(1, renderer.RemovedAreasWithFields.Count());
			var area = renderer.RemovedAreasWithFields.First();
			AssertType<GroupByArea>(area);
			var fields = area.AllDatafields["REPORTDATA"].ToArray();
			var expected = new string[] { "Z0_NUMBER", "Z0_VARCHARMAX" };
			AssertArrayEqualsByElements("it should have the correct data field.", expected, fields);
		}

		void CreateDummyBusinessObject(int number, string text)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = number;
			dummy.Z0_VarCharMax = text;
		}
	}
}
