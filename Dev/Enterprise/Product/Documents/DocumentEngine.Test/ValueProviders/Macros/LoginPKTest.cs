using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginPK))]
	sealed class LoginPKTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", "00000000-0000-0000-0000-000000000000", ValueProviderToTest.GetReplacement("<LoginPK>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < LoginPK >", ValueProviderToTest.IsResponsibleForReplacing("< LoginPK >", Passes.FirstPass));
			Assert("should match < Login PK>", ValueProviderToTest.IsResponsibleForReplacing("< Login PK>", Passes.FirstPass));
			Assert("should match < Login PK >", ValueProviderToTest.IsResponsibleForReplacing("< Login PK >", Passes.FirstPass));
			Assert("should match < LoginPK>", ValueProviderToTest.IsResponsibleForReplacing("< LoginPK>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.PK.ToString(), ValueProviderToTest.GetReplacement("<LoginPK>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginPK();
		}
	}
}
