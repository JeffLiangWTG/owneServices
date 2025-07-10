using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AddTitleIfNotEmpty))]
	sealed class AddTitleIfNotEmptyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<AddTitleIfNotEmpty(\"Consignor: \",\"FRED'S FAT HATS\", \", \")>");
			AssertIsResponsibleForReplacing("< AddTitleIfNotEmpty ( \"Consignor: \",\"FRED'S FAT HATS\" , \"blah\" ) >");

			AssertNotResponsibleForReplacing("<AddTitleIfNotEmpty(\"Consignor: \")>");
			AssertNotResponsibleForReplacing("<AddTitleIfNotEmpty(Consignor: , FRED'S FAT HATS)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Consignor: FRED'S FAT HATS, ", "<AddTitleIfNotEmpty(\"Consignor: \",\"FRED'S FAT HATS\", \", \")>");
			AssertIsReplacedWith("", "<AddTitleIfNotEmpty(\"Consignor: \",\"\", \", \")>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals("ValueProviderToTest.ComponentType", VisualiserComponentTypes.TextEdit, ValueProviderToTest.ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new AddTitleIfNotEmpty();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ConsignorName", "Test Organization"));
		}
	}
}
