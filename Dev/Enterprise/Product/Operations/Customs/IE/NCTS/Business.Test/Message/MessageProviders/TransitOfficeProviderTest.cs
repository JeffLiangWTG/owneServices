using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class TransitOfficeProviderTest : DataProviderTestCase<TransitOfficeProvider>
	{
		public void TestOfficeCode()
		{
			transitOffice.CY_Data = "IE001";
			AssertEquals("IE001", provider.OfficeCode);
		}

		public void TestETA()
		{
			transitOffice.CY_Date = new DateTime(new DateTime(2023, 8, 2, 10, 10, 10).Ticks, DateTimeKind.Utc);
			AssertEquals(new DateTime(2023, 8, 2, 10, 10, 10), provider.ETA);
			AssertEquals(DateTimeKind.Unspecified, provider.ETA.Kind);
		}

		protected override TransitOfficeProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			transitOffice = (NctsIEOfficeCode)header.MovementHeader.CustomsOffices.AddNew();
			provider = new TransitOfficeProvider(transitOffice);
		}
		NctsIEOfficeCode transitOffice;
		TransitOfficeProvider provider;
	}
}
