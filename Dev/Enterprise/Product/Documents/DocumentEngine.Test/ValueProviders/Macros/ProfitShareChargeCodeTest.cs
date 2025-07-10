using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Integration.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ProfitShareChargeCode))]
	sealed class ProfitShareChargeCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < ProfitShareChargeCode >", ValueProviderToTest.IsResponsibleForReplacing("< ProfitShareChargeCode >", Passes.FirstPass));
			Assert("should match < ProfitShare Chargecode>", ValueProviderToTest.IsResponsibleForReplacing("< ProfitShare Chargecode>", Passes.FirstPass));
			Assert("should match < Profit Share Charge Code >", ValueProviderToTest.IsResponsibleForReplacing("< Profit Share Charge Code >", Passes.FirstPass));
			Assert("should match < ProfitShare Charge Code>", ValueProviderToTest.IsResponsibleForReplacing("< ProfitShare Charge Code>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.ProfitShareChargeCode).Returns(ZGuid.Empty);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				AssertEquals(DBNull.Value, ValueProviderToTest.GetReplacement("<ProfitShareChargeCode>", Report));

				ZGuid guid = ZGuid.NewZGuid();
				mockSupporter.Reset();
				mockSupporter.Setup(m => m.ProfitShareChargeCode).Returns(guid);
				AssertEquals(guid, ValueProviderToTest.GetReplacement("<ProfitShareChargeCode>", Report));
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ProfitShareChargeCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var registryItem = ((ProfitShareChargeCode)ValueProviderToTest).GetProfitShareChargeCodeRegistryItem();
			registryItem.Inner.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid("65BBEAF1-B62C-4837-B45B-B069FB0A5ECB"));
		}
	}
}
