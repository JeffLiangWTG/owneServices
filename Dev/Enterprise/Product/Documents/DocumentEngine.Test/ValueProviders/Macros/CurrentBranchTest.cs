using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentBranch))]
	sealed class CurrentBranchTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CurrentBranch >", ValueProviderToTest.IsResponsibleForReplacing("< CurrentBranch >", Passes.FirstPass));
			Assert("should match < Current Branch>", ValueProviderToTest.IsResponsibleForReplacing("< Current Branch>", Passes.FirstPass));
			Assert("should match < Current Branch >", ValueProviderToTest.IsResponsibleForReplacing("< Current Branch >", Passes.FirstPass));
			Assert("should match < CurrentBranch>", ValueProviderToTest.IsResponsibleForReplacing("< CurrentBranch>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, ValueProviderToTest.GetReplacement("<CurrentBranch>", Report));
		}

		public void TestGetReplacement_WhenCompanyIsNull_ShouldNotThrow()
		{
			using (Environment.Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertEquals(Guid.Empty, ValueProviderToTest.GetReplacement("<CurrentBranch>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrentBranch();
		}
	}
}
