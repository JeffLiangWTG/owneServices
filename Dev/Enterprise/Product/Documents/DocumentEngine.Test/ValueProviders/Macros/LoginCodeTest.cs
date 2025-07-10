using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginCode))]
	sealed class LoginCodeTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginCode>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <logine code>", !ValueProviderToTest.IsResponsibleForReplacing("<logine code>", Passes.FirstPass));
			Assert("should match < login     code       >", ValueProviderToTest.IsResponsibleForReplacing("< login     code       >", Passes.FirstPass));
			Assert("should match <LoginCode>", ValueProviderToTest.IsResponsibleForReplacing("<LoginCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_Code, ValueProviderToTest.GetReplacement("<LoginCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_Code = "WTG";
		}
	}
}
