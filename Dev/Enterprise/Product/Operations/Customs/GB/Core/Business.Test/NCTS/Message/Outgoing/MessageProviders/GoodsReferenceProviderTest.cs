using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class GoodsReferenceProviderTest : DataProviderTestCase<GoodsReferenceProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestDeclarationGoodsItemNumber_CusCodeData()
		{
			nctsContainerItemNumber.CY_DataNumeric = 123;
			AssertEquals(123, provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber_NctsDepartureCargoDesc()
		{
			var bill = header.Bills.AddNew();
			var cargoDesc = bill.GoodsItems.AddNew();
			cargoDesc.BY_DeclarationGoodsItemNumber = 987;
			provider = new GoodsReferenceProvider(cargoDesc, 1);

			AssertEquals(987, provider.DeclarationGoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			nctsContainerItemNumber = header.EnRouteIncidents.AddNew().IncidentContainers.AddNew().ItemNumbers.AddNew();
			provider = new GoodsReferenceProvider(nctsContainerItemNumber, 1);
		}

		GoodsReferenceProvider provider;
		NctsContainerItem nctsContainerItemNumber;
		NctsHeader header;

		protected override GoodsReferenceProvider GetProvider() => provider;
	}
}
