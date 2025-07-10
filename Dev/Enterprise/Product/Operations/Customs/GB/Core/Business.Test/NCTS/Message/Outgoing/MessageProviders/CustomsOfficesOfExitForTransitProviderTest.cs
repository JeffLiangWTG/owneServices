using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class CustomsOfficesOfExitForTransitProviderTest : DataProviderTestCase<CustomsOfficesOfExitForTransitProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ExtID", provider.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var customsOfficeOfDestination = header.MovementHeader.CustomsOffices.AddNew();
			customsOfficeOfDestination.CY_Code = "EXT";
			customsOfficeOfDestination.CY_Data = "ExtID";
			provider = new CustomsOfficesOfExitForTransitProvider(customsOfficeOfDestination.CY_Data, 1);
		}

		CustomsOfficesOfExitForTransitProvider provider;

		protected override CustomsOfficesOfExitForTransitProvider GetProvider() => provider;
	}
}
