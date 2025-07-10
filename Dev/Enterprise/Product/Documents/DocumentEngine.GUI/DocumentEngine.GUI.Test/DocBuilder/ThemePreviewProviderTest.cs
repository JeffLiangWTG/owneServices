using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.DocumentEngine.GUI.DocBuilder.Testing
{
	sealed class ThemePreviewProviderTest : TestCaseWithFactory
	{
		public void TestGetPreview()
		{
			var provider = new ThemePreviewProvider();
			var themeRegistry = new DocBuilderThemeRegistry();
			var theme = themeRegistry.FindTheme("CargoWise Blues");
			var preview = provider.GetPreview(theme);

			AssertNotNull(preview);
			AssertEquals("preview.Width", 429, preview.Width);
			AssertEquals("preview.Height", 184, preview.Height);
		}

		public void TestTranslation()
		{
			var provider = new ThemePreviewProvider();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var excelInterface = new ExcelInterface())
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				mockChs.Put("36AACA72-113C-42E1-B0D6-F4535C5D4A39", new ResourceStringData("36AACA72-113C-42E1-B0D6-F4535C5D4A39", "文档表头"));
				mockChs.Put("61A7471E-441C-485C-A94D-A58C57E803F1", new ResourceStringData("61A7471E-441C-485C-A94D-A58C57E803F1", "主要表头"));
				mockChs.Put("DA51A174-2774-4886-B729-B52D159ACBB4", new ResourceStringData("DA51A174-2774-4886-B729-B52D159ACBB4", "主要主体"));
				mockChs.Put("8562394A-1163-4322-BE22-A3194C713AAA", new ResourceStringData("8562394A-1163-4322-BE22-A3194C713AAA", "第二表头"));
				mockChs.Put("C02703D8-BCC6-4023-9DB1-AA5F8D7876E1", new ResourceStringData("C02703D8-BCC6-4023-9DB1-AA5F8D7876E1", "第二主体 {0}"));
				mockChs.Put("46718FD4-760F-4219-B50A-8B31232B5453", new ResourceStringData("46718FD4-760F-4219-B50A-8B31232B5453", "合计"));
				mockChs.Put("FACC9D71-1D72-46CF-BC66-29356F85986D", new ResourceStringData("FACC9D71-1D72-46CF-BC66-29356F85986D", "账户"));
				mockChs.Put("3C262CEF-8EF4-4CBF-AF8E-BBC71D09E9F4", new ResourceStringData("3C262CEF-8EF4-4CBF-AF8E-BBC71D09E9F4", "第1页"));
				mockChs.Put("8934534A-7320-4DB3-A922-6316D7670E47", new ResourceStringData("8934534A-7320-4DB3-A922-6316D7670E47", "日期"));

				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.GUI.Test.DocBuilder.Preview.xls");
				excelInterface.LoadExcelFile(tempFileName);
				var workSheet = excelInterface.WorkSheets[0];
				provider.TranslateContent(workSheet);

				var expectedResult = @"{A}-[文档表头]   {C}-[第1页]

{A}-[主要表头]

{B}-[日期]   {C}-[39668]

{B}-[账户]   {C}-[2681286]

{B}-[主要表头]   {C}-[主要主体]

{A}-[第二表头]   {C}-[合计]

{A}-[第二主体 1]   {C}-[268]
{A}-[第二主体 2]   {C}-[236]
{A}-[第二主体 3]   {C}-[789]
{A}-[第二主体 4]   {C}-[123]
";

				AssertMultilineASCIIEquals("The template xls should be translated", expectedResult,
					workSheet.ToString());
			}
		}
	}
}
