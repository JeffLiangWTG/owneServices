using CargoWise.Data;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DatabaseName))]
	sealed class DatabaseNameTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < DatabaseName >", ValueProviderToTest.IsResponsibleForReplacing("< DatabaseName >", Passes.FirstPass));
			Assert("should match < Database Name>", ValueProviderToTest.IsResponsibleForReplacing("< Database Name>", Passes.FirstPass));
			Assert("should match < Database Name >", ValueProviderToTest.IsResponsibleForReplacing("< Database Name >", Passes.FirstPass));
			Assert("should match < DatabaseName>", ValueProviderToTest.IsResponsibleForReplacing("< DatabaseName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(Db.DatabaseName, ValueProviderToTest.GetReplacement("<DatabaseName>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DatabaseName();
		}
	}
}
