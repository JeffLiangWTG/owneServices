using CargoWise.BrandManager;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginFullName))]
	sealed class LoginFullNameTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() => AssertEquals("Replaced Result when User is Null", BrandingFactory.Instance.ProductName, ValueProviderToTest.GetReplacement("<LoginFullName>", Report)));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<LoginFullName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, ValueProviderToTest.GetReplacement("<LoginFullName>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginFullName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_FullName = "John Doe";
		}
	}
}
