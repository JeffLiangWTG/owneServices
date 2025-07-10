using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueReplacers.Testing
{
	sealed class DelegateValueProviderTest : TestCase
	{
		object Replacer(string macro, Report rpt)
		{
			return 12;
		}

		public void TestIsEditableWhenVisualised()
		{
			DelegateValueProvider valueProvider = new DelegateValueProvider("Test", new ReplacementProviderMethod(Replacer));
			AssertEquals("valueProvider.ComponentType", Enterprise.DocumentEngine.Visualisation.VisualiserComponentTypes.TextEdit, valueProvider.ComponentType);
		}

		public void TestIsResponsible()
		{
			DelegateValueProvider testDelegateValueProvider = new DelegateValueProvider("Test", new ReplacementProviderMethod(Replacer));
			Assert(testDelegateValueProvider.IsResponsibleForReplacing("<test>", Passes.FirstPass));
			Assert(!testDelegateValueProvider.IsResponsibleForReplacing("<test1>", Passes.FirstPass));
			Assert(!testDelegateValueProvider.IsResponsibleForReplacing("<te st>", Passes.FirstPass));
		}

		public void TestIsResponsible_MacroNameWithRegexMetaCharacters()
		{
			DelegateValueProvider testDelegateValueProvider = new DelegateValueProvider("Money$", new ReplacementProviderMethod(Replacer));
			AssertEquals(true, testDelegateValueProvider.IsResponsibleForReplacing("<Money$>", Passes.FirstPass));

			testDelegateValueProvider = new DelegateValueProvider("Test (haha)", new ReplacementProviderMethod(Replacer));
			AssertEquals(true, testDelegateValueProvider.IsResponsibleForReplacing("<test (haha)>", Passes.FirstPass));

			testDelegateValueProvider = new DelegateValueProvider("Warehouse DAN******", new ReplacementProviderMethod(Replacer));
			AssertEquals(true, testDelegateValueProvider.IsResponsibleForReplacing("<Warehouse DAN******>", Passes.FirstPass));
		}

		public void TestIsResponsibleSecondPass()
		{
			DelegateValueProvider testDelegateValueProvider = new DelegateValueProvider("Test", new ReplacementProviderMethod(Replacer), Passes.SecondPass);
			Assert(!testDelegateValueProvider.IsResponsibleForReplacing("<test>", Passes.FirstPass));
			Assert(testDelegateValueProvider.IsResponsibleForReplacing("<test>", Passes.SecondPass));
		}

		public void TestGetReplacement()
		{
			DelegateValueProvider testDelegateValueProvider = new DelegateValueProvider("Test", new ReplacementProviderMethod(Replacer));
			AssertEquals(12, testDelegateValueProvider.GetReplacement("<test>", new Report(null, null)));
		}
	}
}
