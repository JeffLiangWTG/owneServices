using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE015TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE015TransitOperationProvider>
	{
		protected override IE015TransitOperationProvider GetProvider() => new IE015TransitOperationProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
