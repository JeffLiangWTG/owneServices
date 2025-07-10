using System.IO;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(UrlHyperlink))]
	sealed class UrlHyperlinkTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< UrlHyperlink (    Tst ) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<UrlHyperlink(www.edi.com.au,blah)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<UrlHyperlink(www.edi.com.au, ediSite, click here)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< UrlHyperlink( www.edi.com.au,ediSite,click here )>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			ExcelHyperlink replacement = (ExcelHyperlink)ValueProviderToTest.GetReplacement("<UrlHyperlink(www.edi.com.au, ediSite, click here)>", Report);
			AssertEquals("www.edi.com.au", replacement.LinkLocation);
			AssertEquals("", replacement.TargetFrame);
			AssertEquals("", replacement.TextMark);
			AssertEquals("ediSite", replacement.TextToShow);
			AssertEquals("click here", replacement.Tooltip);
			AssertEquals(FlexCel.Core.THyperLinkType.URL, replacement.Type);
		}

		public void TestTranslation()
		{
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put(DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.DocLabelKeyPrefix, "The Title"), new ResourceStringData("", "Der Titel"));
				resourceStrings.Put(DocBuilderResourceStrings.GetKey("", DocBuilderResourceStrings.DocLabelKeyPrefix, "My Title"), new ResourceStringData("", "Mein Titel"));

				var replacement = (ExcelHyperlink)ValueProviderToTest.GetReplacement("<UrlHyperlink(www.edi.com.au, \"The Title\", \"My Title\")>", Report);
				AssertEquals("Der Titel", replacement.TextToShow);
				AssertEquals("Mein Titel", replacement.Tooltip);
			}
		}

		public void TestHyperlinkIsEvaluatedCorrectlyWhenPutTogetherWithAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#DocumentHeader]
{B}-[<AutoHeight><UrlHyperlink(www.edi.com.au, ediSite, click here)>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();
						var hyperLink = excelInterface.Xls.GetHyperLink(1);
						var hyperLinkLocation = excelInterface.Xls.GetHyperLinkCellRange(1);

						AssertEquals(@"{B}-[ediSite]", workSheet.ToString());
						AssertEquals("CellValue should be replaced with show text of Hyperlink", hyperLink.Description, workSheet[0, 1].ToString());
						AssertEquals("Hyperlink should be placed in the correct location", "B1:B1", hyperLinkLocation.CellRef);
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new UrlHyperlink();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualResult = ValueProviderToTest.GetReplacement(example, Report);
			AssertType(typeof(ExcelHyperlink), actualResult);
			AssertEquals(((ExcelHyperlink)expectedResult).LinkLocation, ((ExcelHyperlink)actualResult).LinkLocation);
		}
	}
}
