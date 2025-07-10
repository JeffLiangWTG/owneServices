using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A14HeaderProviderTest : SendAndAmendHeader14ProviderBaseTest<A14HeaderProvider>
	{
		public override void TestReferralRequestReference()
		{
			AssertNull(Provider.ReferralRequestReference);
			const string expected = "A70";
			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = expected;
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader =
				new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			Provider.AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals(expected, Provider.ReferralRequestReference);
		}

		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;

			AssertNull(provider.AmendedItems);
		}
	}
}
