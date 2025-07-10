using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(SignOffText))]
	sealed class SignOffTextTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new SignOffText();
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertEquals("IsResponsibleForReplacing(\"<>\")", false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing(\"< SignOff >\")", false, ValueProviderToTest.IsResponsibleForReplacing("< SignOff >", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing(\"< SignOffText >\")", true, ValueProviderToTest.IsResponsibleForReplacing("< SignOffText >", Passes.FirstPass));
			AssertEquals("IsResponsibleForReplacing(\"< Salu tation>\")", false, ValueProviderToTest.IsResponsibleForReplacing("< Salu tation>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert("Precondition: The registry sign off text should not be empty.", Env.Registry.SignOffText.Length > 0);
			AssertEquals("GetReplacement()", Env.Registry.SignOffText, ValueProviderToTest.GetReplacement("<SignOff>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var registry = RawDataRegistry.Instance.FindByName("SignOffText");
			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Best Regards,");
		}
	}
}
