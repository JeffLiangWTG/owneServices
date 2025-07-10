using CargoWise.BrandManager;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginName))]
	sealed class LoginNameTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", BrandingFactory.Instance.ProductName, ValueProviderToTest.GetReplacement("<LoginName>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < LoginName >", ValueProviderToTest.IsResponsibleForReplacing("< LoginName >", Passes.FirstPass));
			Assert("should match < Login Name>", ValueProviderToTest.IsResponsibleForReplacing("< Login Name>", Passes.FirstPass));
			Assert("should match < Login Name >", ValueProviderToTest.IsResponsibleForReplacing("< Login Name >", Passes.FirstPass));
			Assert("should match < LoginName>", ValueProviderToTest.IsResponsibleForReplacing("< LoginName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_LoginName.ToString(), ValueProviderToTest.GetReplacement("<LoginName>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_LoginName = "john.doe";
		}
	}
}
