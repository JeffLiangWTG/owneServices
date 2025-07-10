using CargoWise.Data;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(UserRepository))]
	sealed class UserRepositoryTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <userErpository>", !ValueProviderToTest.IsResponsibleForReplacing("<userErpository>", Passes.FirstPass));
			Assert("should match < UserRepository       >", ValueProviderToTest.IsResponsibleForReplacing("< UserRepository       >", Passes.FirstPass));
			Assert("should match <UserRepository>", ValueProviderToTest.IsResponsibleForReplacing("<UserRepository>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(Db.DatabaseName + DbUserRepository.RepositoryDbSuffix, ValueProviderToTest.GetReplacement("<UserRepository>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new UserRepository();
		}
	}
}
