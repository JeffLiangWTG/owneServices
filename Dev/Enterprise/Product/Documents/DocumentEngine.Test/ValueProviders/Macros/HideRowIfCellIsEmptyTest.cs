using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HideRowIfCellIsEmpty))]
	sealed class HideRowIfCellIsEmptyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Hide Row If Cell Is Empty >", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< HideRowIfCel lIsEmpty >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIfCellIs Empty>", Report));
		}

		public void TestIsINonVisualisableValueProviderThatModifyDocumentLayout()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProviderThatModifyDocumentLayout;

			AssertNotNull(provider);
			AssertEquals(HideRowIfCellIsEmpty.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new HideRowIfCellIsEmpty();
		}
	}
}
