using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AutoHeight))]
	sealed class AutoHeightTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Auto Height >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< Aut oHeight>", Passes.FirstPass));

			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<AutoHeight(3)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< AutoHeight(6) >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Auto Height(32)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (1)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (1, RemoveLineBreaksToFit)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (100,RemoveLineBreaksToFit)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (RemoveLineBreaksToFit)>", Passes.FirstPass));

			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< Aut oHeight(x)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< AutoHeiht (12)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<AutoHeight(18>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (1, )>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (100,Re moveLineBreaksToFit)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Auto Height (Re moveLineBreaksToFit)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<AutoHeight>", Report));
		}

		public void TestShouldRemoveLineBreaksToFit()
		{
			var trueMacros = new[] { "<Auto Height (1,RemoveLineBreaksToFit)>", "<Auto Height (RemoveLineBreaksToFit)>" };
			var falseMacros = new[] { "<Auto Height (1)>", "<Auto Height>" };

			foreach (var trueMacro in trueMacros)
			{
				Assert("Should remove line breaks for macro " + trueMacro, AutoHeight.ShouldRemoveLineBreaksToFit(trueMacro));
			}
			foreach (var falseMacro in falseMacros)
			{
				Assert("Should NOT remove line breaks for macro " + falseMacro, !AutoHeight.ShouldRemoveLineBreaksToFit(falseMacro));
			}
		}

		public void TestIsINonVisualisableValueProviderThatModifyDocumentLayout()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProviderThatModifyDocumentLayout;

			AssertNotNull(provider);
			AssertEquals(AutoHeight.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new AutoHeight();
		}
	}
}
