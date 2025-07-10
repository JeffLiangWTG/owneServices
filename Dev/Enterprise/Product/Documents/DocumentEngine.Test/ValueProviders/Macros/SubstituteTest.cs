using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Substitute))]
	sealed class SubstituteTest : ValueProviderTest
	{
		public void TestGetReplacementCoreOnEmptyMatches()
		{
			var processor = new Substitute();
			AssertNoExceptionThrown(() => processor.GetReplacement("<Substitute(\"Hello World\", \"\", \"Goodbye\")>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in Substitute Macro: "
			+ "The character/string to substitute cannot be empty. The original text without substitution will be returned." + "]",
			Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestRemoveDoubleQuotes()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(
				factory,
				"Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<Substitute(""<Z0_VarCharMax>"", """""", ""'"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);

			var documentCommand = factory.New<DocumentCommand>();

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = @"SAL AGENCIES KCT ""B2"" SHED";

			factory.Save();

			AssertMultilineASCIIEquals("End to end test for <Substitute> macro removing double quotes."
				, "{B}-[SAL AGENCIES KCT 'B2' SHED]"
				, DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		public void TestEndToEnd()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(
				factory,
				"Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<Substitute(""Hello World"", ""Hello"", ""Goodbye"")>]
{B}-[<Substitute(""<Z0_VarCharMax>"", "":"", """")>]
{B}-[<Substitute(""<Upper(""<Z0_VarCharMax>"")>"", ""1"", ""2"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);

			var documentCommand = factory.New<DocumentCommand>();

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "Line: 1";

			factory.Save();

			AssertMultilineASCIIEquals("End to end test for <Substitute> macro.",
@"{B}-[Goodbye World]
{B}-[Line 1]
{B}-[LINE: 2]",
				DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<Substitute>");
			AssertNotResponsibleForReplacing("<Substitute()>");
			AssertNotResponsibleForReplacing("<Substitute(,)>");
			AssertNotResponsibleForReplacing("<Substitute(\"\",)>");
			AssertNotResponsibleForReplacing("<Substitute(,\"\")>");

			AssertIsResponsibleForReplacing("<Substitute(\"Hello World\", \"Hello\", \"Goodbye\")>");
			AssertIsResponsibleForReplacing("<Substitute(\"<Z0_VarCharMax>\", \":\", \"\")>");
			AssertIsResponsibleForReplacing("< Substitute ( \"Hello World\" , \"Hello\" , \"Goodbye\" ) >");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Goodbye World", "<Substitute(\"Hello World\", \"Hello\", \"Goodbye\")>");
			AssertIsReplacedWith("Hella Warld", "<Substitute(\"Hello World\", \"o\", \"a\")>");
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Substitute();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_VarCharMax", "He:llo"));
		}
	}
}
