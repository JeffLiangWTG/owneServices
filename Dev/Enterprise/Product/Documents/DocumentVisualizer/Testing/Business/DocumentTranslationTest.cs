using CargoWise.Macros;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentTranslationTest : TestCase
	{
		public void TestMacrosAreRunInDocumentLanguageContext()
		{
			CombineAssertions(() =>
			{
				AssertMacrosAreRunInDocumentLanguageContext(null, 69, "sixty nine");
				AssertMacrosAreRunInDocumentLanguageContext(Enterprise.Core.Constants.Languages.English, 69, "sixty nine");
				AssertMacrosAreRunInDocumentLanguageContext(Enterprise.Core.Constants.Languages.French, 69, "soixante-neuf");
				AssertMacrosAreRunInDocumentLanguageContext(Enterprise.Core.Constants.Languages.German, 69, "neunundsechzig");
			});
		}

		public void TestMacrosAreRunInDocumentLanguageContext_InvalidDocumentLanguageDoesNotThrowException() => AssertMacrosAreRunInDocumentLanguageContext("@#$", 69, "sixty nine");

		void AssertMacrosAreRunInDocumentLanguageContext(string documentLanguage, int numberToTranslate, string expectedTranslation)
		{
			var enableTranslationParameter = !string.IsNullOrWhiteSpace(documentLanguage)
				? $":EnableTranslation"
				: string.Empty;

			var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""Test Document"":DataContext=""Test""{enableTranslationParameter}
#End
#Body
	<Description>: <NumberToWords({numberToTranslate})>
#End");

			IStandardTemplate template = new StandardTemplate(worksheet);

			var dataSource = new { Description = $"Sixty nine in {documentLanguage ?? "default language"}" }.MakeDynamic();
			var scope = new MacroScope(dataSource);

			var context = new IMacroLibrary[]
			{
				new StandardLibrary(),
				new DataLibrary()
			}.CreateContext();

			var languageProvider = !string.IsNullOrWhiteSpace(documentLanguage)
				? new DummyDocumentLanguageProvider(documentLanguage)
				: null;

			var document = template.CreateDocument(template.Name, "Test", scope, context, null, languageProvider);
			AssertEquals("prerequisite: document language is set", documentLanguage ?? SharedConstants.Languages.English, document.Language);

			var documentContent = new DocumentStringBuilder(document).ToString();
			AssertMultilineASCIIEquals("document content",
$@"	{{A}}	{{B}}
--- Page 1 ---
{{1}}		{{B}} - Sixty nine in {documentLanguage ?? "default language"}: {expectedTranslation}",
					documentContent);
		}
	}
}
