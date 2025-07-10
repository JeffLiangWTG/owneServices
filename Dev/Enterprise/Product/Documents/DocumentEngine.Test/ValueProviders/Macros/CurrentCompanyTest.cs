using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrentCompany))]
	sealed class CurrentCompanyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CurrentCompany >", ValueProviderToTest.IsResponsibleForReplacing("< CurrentCompany >", Passes.FirstPass));
			Assert("should match < Current Company>", ValueProviderToTest.IsResponsibleForReplacing("< Current Company>", Passes.FirstPass));
			Assert("should match < Current Company >", ValueProviderToTest.IsResponsibleForReplacing("< Current Company >", Passes.FirstPass));
			Assert("should match < CurrentCompany>", ValueProviderToTest.IsResponsibleForReplacing("< CurrentCompany>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, ValueProviderToTest.GetReplacement("<CurrentCompany>", Report));
		}

		public void TestGetReplacement_WhenCompanyIsNull_ShouldNotThrow()
		{
			using (Environment.Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertEquals(Guid.Empty, ValueProviderToTest.GetReplacement("<CurrentCompany>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrentCompany();
		}
	}
}
