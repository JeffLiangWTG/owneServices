using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	abstract class CellStylizerTest<T> : TestCaseWithFactory where T : RegionCellStylizer
	{
		public abstract void TestCanStylize();

		public abstract void TestStylize();

		ExcelWorkSheet workSheet;
		protected ExcelWorkSheet WorkSheet
		{
			get
			{
				if (workSheet == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.CustomizableDocumentTemplates.TestCellStylizer.xls", "TestCellStylizer.xls");
					var testCellStylizer = new ExcelTemplateForUnitTesting("TestCellStylizer.xls", Path.GetFullPath(tempFileName));
					var excelInterface = new ExcelInterface();
					using (var stream = testCellStylizer.GetAsTemplateStream())
					{
						excelInterface.LoadExcelFile(stream);
					}
					workSheet = excelInterface.WorkSheets[0];
					AssertMultilineASCIIEquals("Pre-condition: WorkSheet",
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[DataContext=GenericFreightJob]
{A}-[#ConfigurableSection:GEN, Document Heading, Page Number Header]
{C}-[Background: Fill(Solid) Color(R:0, G:51, B:102, A:255) - Top: Style(Thin) Color(R:150, G:150, B:150, A:255), Bottom: Style(Thin) Color(R:150, G:150, B:150, A:255), Left: Style(Thin) Color(R:150, G:150, B:150, A:255) - Document Heading]   {D}-[Background: Fill(Solid) Color(R:255, G:102, B:0, A:255) - Top: Style(Thin) Color(R:150, G:150, B:150, A:255), Bottom: Style(Thin) Color(R:150, G:150, B:150, A:255), Right: Style(Thin) Color(R:150, G:150, B:150, A:255) - Page <Current Page> of <TotalPages>]
{A}-[#ConfigurableSection:GEN, Primary Heading, Primary Body]
{C}-[Background: Fill(Solid) Color(R:150, G:150, B:150, A:255) - Top: Style(Thin) Color(R:150, G:150, B:150, A:255), Bottom: Style(Thin) Color(R:150, G:150, B:150, A:255), Left: Style(Thin) Color(R:150, G:150, B:150, A:255), Right: Style(Thin) Color(R:150, G:150, B:150, A:255) - Primary Heading]   {D}-[Background: Fill(Solid) Color(R:204, G:204, B:255, A:255) - Top: Style(Thin) Color(R:150, G:150, B:150, A:255), Bottom: Style(Thin) Color(R:150, G:150, B:150, A:255), Left: Style(Thin) Color(R:150, G:150, B:150, A:255), Right: Style(Thin) Color(R:150, G:150, B:150, A:255) - Primary Body]   {BU}-[Something]
{A}-[#ConfigurableSection:GEN, Secondary Heading, Secondary Body]
{C}-[Background: Fill(Solid) Color(R:51, G:51, B:51, A:255) - Top: Style(Thin) Color(R:51, G:51, B:51, A:255), Bottom: Style(Thin) Color(R:51, G:51, B:51, A:255), Left: Style(Thin) Color(R:51, G:51, B:51, A:255), Right: Style(Thin) Color(R:51, G:51, B:51, A:255) - Secondary Heading]
{C}-[Background: Fill(Solid) Color(R:255, G:204, B:153, A:255) - Top: Style(Thin) Color(R:51, G:51, B:51, A:255), Bottom: Style(Thin) Color(R:51, G:51, B:51, A:255), Left: Style(Thin) Color(R:51, G:51, B:51, A:255), Right: Style(Thin) Color(R:51, G:51, B:51, A:255) - Secondary Body]
{A}-[#EndOfReport]",
workSheet.ToString(new CellFormatterExposingFormatting(WorkSheet.ParentExcelInterface.Xls)));
				}
				return workSheet;
			}
		}

		protected override void TearDown()
		{
			workSheet?.Dispose();
			workSheet?.ParentExcelInterface.Dispose();
			embeddedResourceRetriever?.Dispose();
			base.TearDown();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
