using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FreightChargeCode))]
	sealed class FreightChargeCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < FreightChargeCode >", ValueProviderToTest.IsResponsibleForReplacing("< FreightChargeCode >", Passes.FirstPass));
			Assert("should match < Freight Chargecode>", ValueProviderToTest.IsResponsibleForReplacing("< Freight Chargecode>", Passes.FirstPass));
			Assert("should match < Freight Charge Code >", ValueProviderToTest.IsResponsibleForReplacing("< Freight Charge Code >", Passes.FirstPass));
			Assert("should match < FreightCharge Code>", ValueProviderToTest.IsResponsibleForReplacing("< FreightCharge Code>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(Env.Registry.FreightChargeCode.ToString(), ValueProviderToTest.GetReplacement("<FreightChargeCode>", Report).ToString());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FreightChargeCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Env.Registry.FreightChargeCode = new Guid("466D1DF3-BF79-41F1-8B46-4561FC7C669B");
		}
	}
}
