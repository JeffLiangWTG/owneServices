using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginEmail))]
	sealed class LoginEmailTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() => AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginEmail>", Report)));
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertIsResponsibleForReplacing("<LoginEmail>");
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_EmailAddress, ValueProviderToTest.GetReplacement("<LoginEmail>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginEmail();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "mail@mail.com";
		}
	}
}
