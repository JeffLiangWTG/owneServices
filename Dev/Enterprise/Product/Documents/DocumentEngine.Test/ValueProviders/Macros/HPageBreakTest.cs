using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HPageBreak))]
	sealed class HPageBreakTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new HPageBreak();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< HPageBreak>", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertNotNull(ValueProviderToTest.GetReplacement("< HPageBreak>", Report));

			object result = ValueProviderToTest.GetReplacement("< HPageBreak>", Report);
			Assert(result is FlexHPageBreak);
		}

		public void TestIsINonVisualisableValueProvider()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(HPageBreak.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}
	}
}
