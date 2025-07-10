using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(OverFlowToFollowPage))]
	sealed class OverFlowToFollowPageTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Over Flow To Follow Page (\"Test\")>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< OverF lowTo Follo wPage>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< OverFlowToFollowPage>", Passes.FirstPass));

			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<OverFlowToFollowPage(\"Test\")>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< OverFlow   To Follow  Page   (  \"Test\" ) >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Over Flow To Follow Page (\"Test\", 25, Test)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<Over Flow To Follow Page (\"Test\", 25,     Test)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Over Flow To Follow Page (\"Test\", Test)>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Over Flow To Follow Page (\"Test\", 25)>", Passes.FirstPass));

			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< Ove RfloW To fOlloW pAge (x, x , x)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<OverFlowToFollowPage(18>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<OverFlowToFollowPage>", Report));
		}

		public void TestGetFieldName()
		{
			AssertEquals("Robert", OverFlowToFollowPage.GetFieldTitle("< Over Flow To Follow Page (\"Robert\", 25, Test)>"));
		}

		public void TestGetMinimumRows()
		{
			AssertEquals(5, OverFlowToFollowPage.GetMaximumRows("< Over Flow To Follow Page (\"Test\", 5, Test)>"));
		}

		public void TestGetOverflowBehavior()
		{
			AssertEquals("Default OverflowBehaviour", OverflowBehaviour.WrapOverflowWithContinued, OverFlowToFollowPage.GetOverflowBehavior("<OverFlowToFollowPage(\"Andrew\", 25)>"));
			AssertEquals("Defined OverflowBehaviour", OverflowBehaviour.MoveAllContentWithContinued, OverFlowToFollowPage.GetOverflowBehavior("<OverFlowToFollowPage(\"Andrew\", 25, MoveAllContentWithContinued)>"));
		}

		public void TestIsINonVisualisableValueProvider()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(OverFlowToFollowPage.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new OverFlowToFollowPage();
		}
	}
}
