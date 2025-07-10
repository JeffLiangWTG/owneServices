using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE015MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE015MessageProvider>
	{
		public void TestTransitOperation()
		{
			AssertType<IE015TransitOperationProvider>("TransitOperation", Provider.TransitOperation);
		}

		protected override IE015MessageProvider GetProvider() => new IE015MessageProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
