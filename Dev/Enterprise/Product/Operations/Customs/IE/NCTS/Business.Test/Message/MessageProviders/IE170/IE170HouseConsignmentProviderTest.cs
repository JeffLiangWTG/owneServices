using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE170HouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE170HouseConsignmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE170HouseConsignmentProvider(null));
		}

		public void TestDepartureTransportMeans()
		{
			AssertType<ITransportMeans[]>("Departure Transport Means", Provider.DepartureTransportMeans);
		}

		protected override IE170HouseConsignmentProvider GetProvider() => new IE170HouseConsignmentProvider(nctsHeader.MovementHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
