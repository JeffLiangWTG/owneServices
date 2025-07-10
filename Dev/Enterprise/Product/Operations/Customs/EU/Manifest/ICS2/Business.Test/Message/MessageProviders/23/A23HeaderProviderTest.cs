using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A23HeaderProviderTest : SendAndAmendHeader23ProviderBaseTest<A23HeaderProvider>
	{
		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;
			AssertNull("Default to null", provider.AmendedItems);
		}

		public override void TestReferralRequestReference()
		{
			AssertNull("Should default to null.", Provider.ReferralRequestReference);

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A23";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			((IAmendedItemsProvider)Provider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals("Should pick the code from the request header.", "A23", Provider.ReferralRequestReference);
		}
	}
}
