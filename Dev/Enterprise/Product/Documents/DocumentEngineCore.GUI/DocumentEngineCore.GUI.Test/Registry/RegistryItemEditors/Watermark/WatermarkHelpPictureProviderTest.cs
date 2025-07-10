using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	sealed class WatermarkHelpPictureProviderTest : TestCaseWithFactory
	{
		public void TestGetPreview()
		{
			var provider = new WatermarkHelpPictureProvider();
			var preview = provider.GetPreview();

			AssertNotNull(preview);
			AssertEquals("preview.Width", 135, preview.Width);
			AssertEquals("preview.Height", 188, preview.Height);
		}

		public void TestNoExceptionThrownWhenGetPreview()
		{
			const string PreviewResourceName = "Enterprise.DocumentEngineCore.GUI.Registry.RegistryItemEditors.Watermark.HelpPicture.xls";
			var assembly = typeof(WatermarkHelpPictureProvider).Assembly;
			Assert(assembly.GetManifestResourceNames().Contains(PreviewResourceName));
			using (var stream = assembly.GetManifestResourceStream(PreviewResourceName))
			{
				AssertNotNull(stream);
			}

			var provider = new WatermarkHelpPictureProvider();
			AssertNoExceptionThrown(() => provider.GetPreview());
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTranslation()
		{
			var provider = new WatermarkHelpPictureProvider();
			using (var excelInterface = new ExcelInterface())
			using (var mockChs = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			{
				mockChs.Put("34BA9490-A244-4800-8941-B9F525A957BB", new ResourceStringData("34BA9490-A244-4800-8941-B9F525A957BB", "顶部/左边"));
				mockChs.Put("4C622669-E2FC-4F83-BA53-7126322DF155", new ResourceStringData("4C622669-E2FC-4F83-BA53-7126322DF155", "中间"));
				mockChs.Put("0351F594-B0D6-46E8-BB8C-E5263D238339", new ResourceStringData("0351F594-B0D6-46E8-BB8C-E5263D238339", "右边"));
				mockChs.Put("6A3B4269-A648-403F-881D-EBF087D99392", new ResourceStringData("6A3B4269-A648-403F-881D-EBF087D99392", "中部"));
				mockChs.Put("41E46FFF-82AC-4A96-A3E6-A0713700CB99", new ResourceStringData("41E46FFF-82AC-4A96-A3E6-A0713700CB99", "底部"));

				excelInterface.LoadExcelFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngineCore.GUI\DocumentEngineCore.GUI\Registry\RegistryItemEditors\Watermark\HelpPicture.xls"));
				var workSheet = excelInterface.WorkSheets[0];
				provider.TranslateContent(workSheet);

				var expectedResult = @"{A}-[顶部/左边]   {B}-[中间]   {C}-[右边]



{A}-[中部]

{A}-[底部]";

				AssertMultilineASCIIEquals("The template xls should be translated", expectedResult,
					workSheet.ToString());
			}
		}
	}
}
