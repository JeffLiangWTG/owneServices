using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AdditionalSupplyChainActorProvider))]
	sealed class AdditionalSupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestRole()
		{
			AssertEquals("FR1", Provider.Role);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("AddSupID", Provider.IdentificationNumber);
		}

		protected override AdditionalSupplyChainActorProvider GetProvider() => provider;
		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			cusSupplyChainActorReference = Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "FIS", "AddSupID");
			provider = new AdditionalSupplyChainActorProvider(cusSupplyChainActorReference, 1);
		}

		CusSupplyChainActorReference cusSupplyChainActorReference;
		AdditionalSupplyChainActorProvider provider;
		NctsHeader header;
	}
}
