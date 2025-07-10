using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Truncate))]
	sealed class TruncateTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<Truncate>");
			AssertNotResponsibleForReplacing("<Truncate>()>");
			AssertNotResponsibleForReplacing("<Truncate>(\"\",0,0)>");
			AssertNotResponsibleForReplacing("<Truncate>(\"HELLO\")>");
			AssertNotResponsibleForReplacing("<Truncate>(\"\",HELLO)>");
			AssertNotResponsibleForReplacing("<Truncate>(\"\",1.3)>");
			AssertNotResponsibleForReplacing("<Truncate>(\"\",-1)>");

			AssertIsResponsibleForReplacing("<Truncate(\"\",0)>");
			AssertIsResponsibleForReplacing("<Truncate(\"\",7)>");
			AssertIsResponsibleForReplacing("<Truncate(\"Moooooooooooooooooooooooooooooo\",7)>");
			AssertIsResponsibleForReplacing("<Truncate   (\"\",    0)>");
			AssertIsResponsibleForReplacing("< Truncate(\"\",0)>");
			AssertIsResponsibleForReplacing("<Truncate(\"Captain Fantastic Faster Than Superman Spiderman Batman Wolverine Hulk And The Flash Combined\",20)>");
		}

		public void TestZeroLength()
		{
			AssertIsReplacedWith("", "<Truncate(\"Hello\", 0)>");
		}

		public void TestStringShorterThanMaximum()
		{
			AssertIsReplacedWith("Hello", "<Truncate(\"Hello\", 6)>");
		}

		public void TestStringEqualToMaximum()
		{
			AssertIsReplacedWith("Hello", "<Truncate(\"Hello\", 5)>");
		}

		public void TestStringLongerThanMaximum()
		{
			AssertIsReplacedWith("H...", "<Truncate(\"Hello\", 4)>");
		}

		public void TestStringLongerThanMaximum2()
		{
			AssertIsReplacedWith("...", "<Truncate(\"Hello\", 3)>");
		}

		public void TestStringLongerThanMaximum3()
		{
			AssertIsReplacedWith("..", "<Truncate(\"Hello\", 2)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("Captain Fantastic...", "<Truncate(\"Captain Fantastic is a totally awesome first name\", 20)>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Truncate();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("JobDescription", "The port where the shipment is intended to go."));
		}
	}
}
