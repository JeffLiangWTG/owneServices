using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DESREMSealProviderTest : DataProviderTestCase<DESREMSealProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(DESREMSealProvider.NewOrNull(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestIdentifier()
		{
			AssertEquals("S002", Provider.Identifier);
		}

		protected override DESREMSealProvider GetProvider() => DESREMSealProvider.NewOrNull(seal, 1);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalHeaderContainer = header.ArrivalHeaderContainers.AddNew();
			seal = arrivalHeaderContainer.Seals.AddNew();
			seal.BK_SequenceNumber = 2;
			seal.BK_SealNumber = "S002";
		}
		CusSeal seal;
	}
}
