using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocBuilderTemplateMerge;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class TemplateStylizerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStylize()
		{
			using (var excelInterface = new ExcelInterface())
			{
				var theme = new DocBuilderTheme("Test", true);
				theme.DocumentHeading.Color1 = FromArgb(SystemDocumentElementsMerger.PrimaryHeadingMagicColor);
				theme.DocumentHeading.Color2 = Color.LightBlue;
				theme.DocumentHeading.FontColor = Color.PaleTurquoise;
				theme.DocumentHeading.Font = new Font("Courier New", 15, FontStyle.Bold | FontStyle.Italic);
				theme.PageNumberHeading.Color1 = Color.LightPink;
				theme.PageNumberHeading.Color2 = Color.DarkGreen;
				theme.PageNumberHeading.FontColor = Color.LimeGreen;
				theme.PageNumberHeading.Font = new Font("Courier New", 10, FontStyle.Italic);
				theme.PrimaryHeading.Color1 = Color.Aquamarine;
				theme.PrimaryHeading.Color2 = Color.Pink;
				theme.PrimaryHeading.FontColor = Color.Blue;
				theme.PrimaryHeading.Font = new Font("Times New Roman", 9, FontStyle.Bold | FontStyle.Italic);
				theme.PrimaryBody.Color1 = Color.LightGray;
				theme.PrimaryBody.Color2 = Color.HotPink;
				theme.SecondaryHeading.Color1 = Color.Lavender;
				theme.SecondaryHeading.Color2 = Color.LightCoral;
				theme.SecondaryHeading.FontColor = Color.LimeGreen;
				theme.SecondaryHeading.Font = new Font("Arial", 8, FontStyle.Bold);
				theme.SecondaryBody.Color1 = Color.LightSkyBlue;
				theme.SecondaryBody.Color2 = Color.PeachPuff;

				excelInterface.LoadExcelFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine.GUI\DocumentEngine.GUI\DocBuilder\Preview.xls"));

				var workSheet = excelInterface.WorkSheets[0];
				var stylizer = new TemplateStylizer(excelInterface, theme);

				stylizer.Stylize();

				var expectedResult =
@"{A}-[Background: Fill(Solid) Color(R:150, G:150, B:150, A:255) - Top: Style(Thin) Color(R:173, G:216, B:230, A:255), Bottom: Style(Thin) Color(R:173, G:216, B:230, A:255), Left: Style(Thin) Color(R:173, G:216, B:230, A:255) - Document Heading]   {B}-[Background: Fill(Solid) Color(R:150, G:150, B:150, A:255) - Top: Style(Thin) Color(R:173, G:216, B:230, A:255), Bottom: Style(Thin) Color(R:173, G:216, B:230, A:255)]   {C}-[Background: Fill(Solid) Color(R:255, G:182, B:193, A:255) - Top: Style(Thin) Color(R:0, G:100, B:0, A:255), Bottom: Style(Thin) Color(R:0, G:100, B:0, A:255), Left: Style(Thin) Color(R:0, G:100, B:0, A:255), Right: Style(Thin) Color(R:0, G:100, B:0, A:255) - Page 1 of 1]

{A}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255), Left: Style(Thin) Color(R:255, G:192, B:203, A:255) - Primary Heading]   {B}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255)]   {C}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255), Right: Style(Thin) Color(R:255, G:192, B:203, A:255)]

{B}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255), Left: Style(Thin) Color(R:255, G:192, B:203, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - Date]   {C}-[Top: Style(Thin) Color(R:255, G:105, B:180, A:255), Bottom: Style(Thin) Color(R:255, G:105, B:180, A:255), Left: Style(Thin) Color(R:255, G:105, B:180, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - 39668 - [$-C09]dd\-mmm\-yy;@]

{B}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255), Left: Style(Thin) Color(R:255, G:192, B:203, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - Account]   {C}-[Top: Style(Thin) Color(R:255, G:105, B:180, A:255), Bottom: Style(Thin) Color(R:255, G:105, B:180, A:255), Left: Style(Thin) Color(R:255, G:105, B:180, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - 2681286]
{B}-[@]   {C}-[@]
{B}-[Background: Fill(Solid) Color(R:127, G:255, B:212, A:255) - Top: Style(Thin) Color(R:255, G:192, B:203, A:255), Bottom: Style(Thin) Color(R:255, G:192, B:203, A:255), Left: Style(Thin) Color(R:255, G:192, B:203, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - Primary Heading]   {C}-[Top: Style(Thin) Color(R:255, G:105, B:180, A:255), Bottom: Style(Thin) Color(R:255, G:105, B:180, A:255), Left: Style(Thin) Color(R:255, G:105, B:180, A:255), Right: Style(Thin) Color(R:255, G:105, B:180, A:255) - Primary Body]

{A}-[Background: Fill(Solid) Color(R:230, G:230, B:250, A:255) - Top: Style(Thin) Color(R:240, G:128, B:128, A:255), Bottom: Style(Thin) Color(R:240, G:128, B:128, A:255), Left: Style(Thin) Color(R:240, G:128, B:128, A:255) - Secondary Heading]   {B}-[Background: Fill(Solid) Color(R:230, G:230, B:250, A:255) - Top: Style(Thin) Color(R:240, G:128, B:128, A:255), Bottom: Style(Thin) Color(R:240, G:128, B:128, A:255)]   {C}-[Background: Fill(Solid) Color(R:230, G:230, B:250, A:255) - Top: Style(Thin) Color(R:240, G:128, B:128, A:255), Bottom: Style(Thin) Color(R:240, G:128, B:128, A:255), Right: Style(Thin) Color(R:240, G:128, B:128, A:255) - Amount]
{A}-[Top: Style(Thin) Color(R:240, G:128, B:128, A:255), Left: Style(Thin) Color(R:255, G:218, B:185, A:255)]   {B}-[Top: Style(Thin) Color(R:240, G:128, B:128, A:255)]   {C}-[Top: Style(Thin) Color(R:240, G:128, B:128, A:255), Right: Style(Thin) Color(R:255, G:218, B:185, A:255)]
{A}-[Left: Style(Thin) Color(R:255, G:218, B:185, A:255) - Secondary Body 1]   {B}-[#,##0.00]   {C}-[Right: Style(Thin) Color(R:255, G:218, B:185, A:255) - 268 - #,##0.00]
{A}-[Left: Style(Thin) Color(R:255, G:218, B:185, A:255) - Secondary Body 2]   {B}-[#,##0.00]   {C}-[Right: Style(Thin) Color(R:255, G:218, B:185, A:255) - 236 - #,##0.00]
{A}-[Left: Style(Thin) Color(R:255, G:218, B:185, A:255) - Secondary Body 3]   {B}-[#,##0.00]   {C}-[Right: Style(Thin) Color(R:255, G:218, B:185, A:255) - 789 - #,##0.00]
{A}-[Left: Style(Thin) Color(R:255, G:218, B:185, A:255) - Secondary Body 4]   {B}-[#,##0.00]   {C}-[Right: Style(Thin) Color(R:255, G:218, B:185, A:255) - 123 - #,##0.00]
{A}-[Bottom: Style(Thin) Color(R:255, G:218, B:185, A:255), Left: Style(Thin) Color(R:255, G:218, B:185, A:255)]   {B}-[Bottom: Style(Thin) Color(R:255, G:218, B:185, A:255)]   {C}-[Bottom: Style(Thin) Color(R:255, G:218, B:185, A:255), Right: Style(Thin) Color(R:255, G:218, B:185, A:255)]";

				AssertMultilineASCIIEquals("stylizer.Stylize(excelInterface)", expectedResult,
					workSheet.ToString(new CellFormatterExposingFormatting(workSheet.ParentExcelInterface.Xls)));

				//Stylize a second time, result should remain the same
				stylizer.Stylize();

				AssertMultilineASCIIEquals("stylizer.Stylize(excelInterface)", expectedResult,
					workSheet.ToString(new CellFormatterExposingFormatting(workSheet.ParentExcelInterface.Xls)));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestStylizeDoesNotCrashOnTemplateWithEmptyWorksheet()
		{
			using (var excelInterface = new ExcelInterface())
			{
				var theme = new DocBuilderTheme("Test", true);
				theme.DocumentHeading.Color1 = Color.Red;
				theme.DocumentHeading.Color2 = Color.LightBlue;
				theme.DocumentHeading.FontColor = Color.PaleTurquoise;
				theme.DocumentHeading.Font = new Font("Courier New", 15, FontStyle.Bold | FontStyle.Italic);

				excelInterface.LoadExcelFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\TemplateWithEmptyWorksheet.xls"));

				var workSheet = excelInterface.WorkSheets[0];
				var stylizer = new TemplateStylizer(excelInterface, theme);

				stylizer.Stylize();
			}
		}

		Color FromArgb(long argb)
		{
			return Color.FromArgb((byte)((argb & 0xff000000) >> 24),
									(byte)((argb & 0xff0000) >> 16),
									(byte)((argb & 0xff00) >> 8),
									(byte)(argb & 0xff));
		}
	}
}
