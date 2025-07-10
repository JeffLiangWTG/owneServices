using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE013MessageProvider>
	{
		public void TestTransitOperation()
		{
			AssertType<IE013TransitOperationProvider>("TransitOperation", Provider.TransitOperation);
		}

		protected override IE013MessageProvider GetProvider() => new IE013MessageProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
