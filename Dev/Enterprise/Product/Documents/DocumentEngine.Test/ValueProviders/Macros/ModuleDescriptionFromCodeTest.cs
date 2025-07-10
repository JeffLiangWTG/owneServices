using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ModuleDescriptionFromCode))]
	sealed class ModuleDescriptionFromCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<moduledescriptionfromcode>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   Module Description From Code   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   ModuleDescriptionFromCode somefield   >", Passes.FirstPass));
			Assert("should match <ModuleDescriptionFromCode(AField)>", ValueProviderToTest.IsResponsibleForReplacing("<ModuleDescriptionFromCode(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   ModuleDescriptionFromCode(AField, other)   >", Passes.SecondPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<ModuleDescriptionFromCode()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("Core", ValueProviderToTest.GetReplacement("<ModuleDescriptionFromCode(COR)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<ModuleDescriptionFromCode(ZUB)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<ModuleDescriptionFromCode()>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ModuleDescriptionFromCode();
		}
	}
}
