using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ServerVersion))]
	sealed class SQLServerVersionTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <sever venison>", !ValueProviderToTest.IsResponsibleForReplacing("<sever venison>", Passes.FirstPass));
			Assert("should match < server    version       >", ValueProviderToTest.IsResponsibleForReplacing("< server    version       >", Passes.FirstPass));
			Assert("should match <ServerVersion>", ValueProviderToTest.IsResponsibleForReplacing("<ServerVersion>", Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ServerVersion();
		}
	}
}
