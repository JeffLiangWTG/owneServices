using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginTitle))]
	sealed class LoginTitleTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginTitle>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<LoginTitle>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_Title, ValueProviderToTest.GetReplacement("<LoginTitle>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginTitle();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_Title = "Developer";
		}
	}
}
