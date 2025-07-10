using CargoWise.Data;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DatabaseServer))]
	sealed class DatabaseServerTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < DatabaseServer >", ValueProviderToTest.IsResponsibleForReplacing("< DatabaseServer >", Passes.FirstPass));
			Assert("should match < Database Server>", ValueProviderToTest.IsResponsibleForReplacing("< Database Server>", Passes.FirstPass));
			Assert("should match < Database Server >", ValueProviderToTest.IsResponsibleForReplacing("< Database Server >", Passes.FirstPass));
			Assert("should match < DatabaseServer>", ValueProviderToTest.IsResponsibleForReplacing("< DatabaseServer>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(Db.ServerName, ValueProviderToTest.GetReplacement("<DatabaseServer>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DatabaseServer();
		}
	}
}
