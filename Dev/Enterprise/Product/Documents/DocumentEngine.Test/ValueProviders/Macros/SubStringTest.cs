using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SubString))]
	sealed class SubStringTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<SubString>");
			AssertNotResponsibleForReplacing("<SubString()>");
			AssertNotResponsibleForReplacing("<SubString(,)>");
			AssertNotResponsibleForReplacing("<SubString(\"\",)>");
			AssertNotResponsibleForReplacing("<SubString(,\"\")>");

			AssertIsResponsibleForReplacing("<SubString(\"\",0)>");
			AssertIsResponsibleForReplacing("<SubString(\"\",1)>");
			AssertIsResponsibleForReplacing("<SubString(\"\",0,3)>");
			AssertIsResponsibleForReplacing("<SubString(\"\",1,2)>");
			AssertIsResponsibleForReplacing("< SubString(\"\",1)>");
			AssertIsResponsibleForReplacing("<Sub String (\"\",12)>");

			AssertIsResponsibleForReplacing("<SubString(\"Count Flatula\", 1, 2)>");
			AssertIsResponsibleForReplacing("<SubString(\"Count Flatula\", 3)>");
			AssertIsResponsibleForReplacing("<SubString(\"<Z0_VarCharMax>\", 12)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("ou", "<SubString(\"Count Clinton\", 1, 2)>");
			AssertIsReplacedWith("unt Clinton", "<SubString(\"Count Clinton\", 2)>");
		}

		public void TestReplacement_WithLineBreaks()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(
				factory,
				"Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[<SubString(""<Z0_VarCharMax>"", 0, 12)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);

			var documentCommand = factory.New<DocumentCommand>();

			var pivot = factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "WiseTech Global";
			factory.Save();

			AssertMultilineASCIIEquals("{B}-[WiseTech Glo]", DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));

			dummy.Z0_VarCharMax = "WiseTech\nGlobal";
			factory.Save();

			AssertMultilineASCIIEquals("{B}-[WiseTech|>Glo]", DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new SubString();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_VarCharMax", "WiseTech Global"));
		}
	}
}
