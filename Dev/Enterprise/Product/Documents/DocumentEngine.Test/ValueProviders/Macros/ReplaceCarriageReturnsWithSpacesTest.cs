using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ReplaceCarriageReturnsWithSpaces))]
	sealed class ReplaceCarriageReturnsWithSpacesTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces>");
			AssertNotResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces()>");
			AssertNotResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(,)>");
			AssertNotResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(\"\",)>");
			AssertNotResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(,\"\")>");

			AssertIsResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(\"\")>");
			AssertIsResponsibleForReplacing("< Replace Carriage Returns With Spaces ( \"\" ) >");
			AssertIsResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(\"Count Flatula\")>");
			AssertIsResponsibleForReplacing("<ReplaceCarriageReturnsWithSpaces(\"<Z0_VarCharMax>\")>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Count   Clinton", "<ReplaceCarriageReturnsWithSpaces(\"Count\r\nClinton\")>");
			AssertIsReplacedWith("Co   unt Clin   ton", "<ReplaceCarriageReturnsWithSpaces(\" Co\runt Clin\nton  \")>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ReplaceCarriageReturnsWithSpaces();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_VarCharMax", "Address\r\nProfile"));
		}
	}
}
