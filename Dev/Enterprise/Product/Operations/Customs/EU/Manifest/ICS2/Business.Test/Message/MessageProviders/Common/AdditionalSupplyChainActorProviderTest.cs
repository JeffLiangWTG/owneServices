using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalSupplyChainActorProviderTest : DataProviderTestCase<AdditionalSupplyChainActorProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("CusSupplyChainActorReference missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(supplyChainActor));
			});
		}

		public void TestType()
		{
			supplyChainActor.CFR_Code = "CS";
			AssertEquals("Role", "CS", Provider.Type);
		}

		public void TestIdentifier()
		{
			supplyChainActor.CFR_Reference = "IdentificationNo";
			AssertEquals("IdentificationNumber", "IdentificationNo", Provider.Identifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supplyChainActor = Factory.New<CusSupplyChainActorReference>();
		}
		CusSupplyChainActorReference supplyChainActor;

		AdditionalSupplyChainActorProvider GenerateProvider(CusSupplyChainActorReference supplyChainActor) => new AdditionalSupplyChainActorProvider(supplyChainActor);

		protected sealed override AdditionalSupplyChainActorProvider GetProvider()
		{
			return GenerateProvider(supplyChainActor);
		}
	}
}
