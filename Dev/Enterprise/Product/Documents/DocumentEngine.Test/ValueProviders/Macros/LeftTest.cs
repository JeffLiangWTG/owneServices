using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Left))]
	sealed class LeftTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<Left>");
			AssertNotResponsibleForReplacing("<Left()>");
			AssertNotResponsibleForReplacing("<Left(,)>");
			AssertNotResponsibleForReplacing("<Left(\"\",)>");
			AssertNotResponsibleForReplacing("<Left(,\"\")>");
			AssertNotResponsibleForReplacing("<Left(\"\",0,3)>");

			AssertIsResponsibleForReplacing("<Left(\"\",0)>");
			AssertIsResponsibleForReplacing("<Left(\"\",1)>");
			AssertIsResponsibleForReplacing("< Left ( \"\" , 2 ) >");

			AssertIsResponsibleForReplacing("<Left(\"Count Flatula\", 1)>");
			AssertIsResponsibleForReplacing("<Left(\"Count Flatula\", 3)>");
			AssertIsResponsibleForReplacing("<Left(\"<Z0_VarCharMax>\", 12)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Coun", "<Left(\"Count Clinton\", 4)>");
			AssertIsReplacedWith("Co", "<Left(\"Count Clinton\", 2)>");
			AssertIsReplacedWith("", "<Left(\"Count Clinton\", 0)>");
		}

		public void TestReplacementWithLineBreak()
		{
			CombineAssertions(() =>
			{
				AssertIsReplacedWith("Count Clint", "<Left(\"Count Clinton\r\nand Flatula\", 11)>");
				AssertIsReplacedWith("Count Clinton\r", "<Left(\"Count Clinton\r\nand Flatula\", 14)>");
				AssertIsReplacedWith("Count Clinton\r\n", "<Left(\"Count Clinton\r\nand Flatula\", 15)>");
				AssertIsReplacedWith("Count Clinton\r\nand", "<Left(\"Count Clinton\r\nand Flatula\", 18)>");
				AssertIsReplacedWith("Count Clinton\r\nand\r", "<Left(\"Count Clinton\r\nand\r\nFlatula\", 19)>");
				AssertIsReplacedWith("Count Clinton\r\nand\r\n", "<Left(\"Count Clinton\r\nand\r\nFlatula\", 20)>");
				AssertIsReplacedWith("Count Clinton\r\nand\r\nFla", "<Left(\"Count Clinton\r\nand\r\nFlatula\", 23)>");
			});
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Left();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OH_FullName", "WiseTech Global"));
		}
	}
}
