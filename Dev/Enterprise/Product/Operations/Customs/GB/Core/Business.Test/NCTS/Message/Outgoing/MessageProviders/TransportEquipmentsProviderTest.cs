using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class TransportEquipmentsProviderTest : DataProviderTestCase<TransportEquipmentsProvider>
	{
		public void TestGoodsReferences()
		{
			AssertEquals(0, Provider.GoodsReferences.Count);
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestContainerIdentificationNumber()
		{
			AssertEquals("contnr", Provider.ContainerIdentificationNumber);
		}

		public void TestNumberOfSeals()
		{
			AssertEquals(2, Provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			AssertNotNull(Provider.Seals);
		}

		protected sealed override TransportEquipmentsProvider GetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var container = header.DepartureHeaderContainers.AddNew();
			container.BC_ContainerNum = "contnr";
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			return new TransportEquipmentsProvider(container, 1);
		}
	}
}
