using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Upper))]
	sealed class UpperTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<Upper>");
			AssertNotResponsibleForReplacing("<Upper()>");
			AssertNotResponsibleForReplacing("<Upper(,)>");
			AssertNotResponsibleForReplacing("<Upper(\"\",)>");
			AssertNotResponsibleForReplacing("<Upper(,\"\")>");
			AssertNotResponsibleForReplacing("<Upper(\"\",0,3)>");

			AssertIsResponsibleForReplacing("<Upper(\"\")>");
			AssertIsResponsibleForReplacing("< Upper ( \"\" ) >");

			AssertIsResponsibleForReplacing("<Upper(\"Count Flatula\")>");
			AssertIsResponsibleForReplacing("<Upper(\"Dexter\r\nMorgan\")>");
			AssertIsResponsibleForReplacing("<Upper(\"Abc\r\ndefg\r\n\r\nhij\")>");
			AssertIsResponsibleForReplacing("<Upper(\"<Z0_VarCharMax>\")>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("COUNT CLINTON", "<Upper(\"Count Clinton\")>");
			AssertIsReplacedWith("DEXTER\r\nMORGAN", "<Upper(\"Dexter\r\nMorgan\")>");
			AssertIsReplacedWith("ABC\r\nDEFG\r\n\r\nHIJ", "<Upper(\"Abc\r\ndefg\r\n\r\nhij\")>");
			AssertIsReplacedWith("RAKAKA", "<Upper(\"RaKaKa\")>");
			AssertIsReplacedWith("", "<Upper(\"\")>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Upper();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_VarCharMax", "Containers"));
		}
	}
}
