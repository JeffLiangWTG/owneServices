using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FallbackIfEmpty))]
	sealed class FallbackIfEmptyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertNotResponsibleForReplacing("<FallbackIfEmpty>");
			AssertNotResponsibleForReplacing("<FallbackIfEmpty()>");
			AssertNotResponsibleForReplacing("<FallbackIfEmpty(,)>");
			AssertNotResponsibleForReplacing("<FallbackIfEmpty(\"\",)>");
			AssertNotResponsibleForReplacing("<FallbackIfEmpty(,\"\")>");

			AssertIsResponsibleForReplacing("<FallbackIfEmpty(\"\",\"\")>");
			AssertIsResponsibleForReplacing("< FallbackIfEmpty(\"\",\"\")>");
			AssertIsResponsibleForReplacing("<Fallback IfEmpty(\"\",\"\")>");
			AssertIsResponsibleForReplacing("<FallbackIf Empty(\"\",\"\")>");
			AssertIsResponsibleForReplacing("<FallbackIfEmpty (\"\",\"\")>");
			AssertIsResponsibleForReplacing("< Fallback If Empty (\"\",\"\") >");

			AssertIsResponsibleForReplacing("<FallbackIfEmpty(\"Text\",\"Fallback\")>");
			AssertIsResponsibleForReplacing("<FallbackIfEmpty(\"\",\"Fallback\")>");
			AssertIsResponsibleForReplacing("<FallbackIfEmpty(\"<Z0_VarCharMax><Z0_Number>\",\"Fallback\")>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("", "<FallbackIfEmpty(\"\",\"\")>");
			AssertIsReplacedWith("blah", "<FallbackIfEmpty(\"\",\"blah\")>");
			AssertIsReplacedWith("zero", "<FallbackIfEmpty(\"0\",\"zero\")>");
			AssertIsReplacedWith("boo gfehofe hfeoifo", "<FallbackIfEmpty(\"\",\"boo gfehofe hfeoifo\")>");
			AssertIsReplacedWith("W)NRVOY#N%$L(*(@B*N($YNB(*@Y$ *Y%$YBNY@", "<FallbackIfEmpty(\"\",\"W)NRVOY#N%$L(*(@B*N($YNB(*@Y$ *Y%$YBNY@\")>");

			AssertIsReplacedWith("blah", "<FallbackIfEmpty(\"blah\",\"shouldnotbereplacedwiththis\")>");
			AssertIsReplacedWith("gwer agag ", "<FallbackIfEmpty(\"gwer agag \",\"shouldnotbereplacedwiththis\")>");
			AssertIsReplacedWith("BN%#U()UB#VM#$(U", "<FallbackIfEmpty(\"BN%#U()UB#VM#$(U\",\"shouldnotbereplacedwiththis\")>");
			AssertIsReplacedWith("ALW HFHW FOWYRL(%*$@* &$:YE@ E@(&", "<FallbackIfEmpty(\"ALW HFHW FOWYRL(%*$@* &$:YE@ E@(&\",\"shouldnotbereplacedwiththis\")>");
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FallbackIfEmpty();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_VarCharMax", ""));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_Number", ""));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_Fallback", "Fallback"));
		}
	}
}
