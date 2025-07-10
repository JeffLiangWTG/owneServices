using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ProductName))]
	sealed class ProductNameTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <ReportName>", !ValueProviderToTest.IsResponsibleForReplacing("<ReportName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("CargoWise", ValueProviderToTest.GetReplacement("<ProductName>", Report));
			AssertEquals("Invalid property name: <ReportName>", ValueProviderToTest.GetReplacement("<ReportName>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ProductName();
		}
	}
}
