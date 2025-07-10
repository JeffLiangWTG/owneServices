using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentEngineTestHelperTest : TestCase
	{
		public void TestCreateExcelWorkSheetFromString()
		{
			var expected = string.Empty;

			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.ActiveWorksheet = 0;
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				workSheet[0, 0] = "#Config";
				workSheet[1, 0] = "Name=Test";
				workSheet[2, 0] = "#SectionBody";
				workSheet[3, 1] = "<CurrentLanguage>";
				workSheet[4, 0] = "";
				workSheet[5, 1] = "<IsDraft>";
				workSheet[5, 3] = "<MultiLanguageRegistryItemCastOutToZString>";
				workSheet[6, 0] = "#EndOfReport";

				AssertMultilineASCIIEquals("Pre-condition",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<CurrentLanguage>]

{B}-[<IsDraft>]   {D}-[<MultiLanguageRegistryItemCastOutToZString>]
{A}-[#EndOfReport]", workSheet.ToString());

				expected = workSheet.ToString();
			}

			using (var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(expected))
			{
				AssertMultilineASCIIEquals("Created worksheet should be the same.", expected, workSheet.ToString());
			}
		}
	}
}
