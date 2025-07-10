using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(NonModifiable))]
	sealed class NonModifiableTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Non Modifiable >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<NonModifiable>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< No nModifiable>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<NonModifiable>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new NonModifiable();
		}

		public override void TestDocumentation()
		{
			Assert(true);
		}
	}
}
