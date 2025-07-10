using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A17HeaderProviderTest : SendAndAmendHeader17ProviderBaseTest<A17HeaderProvider>
	{
		public override void TestReferralRequestReference()
		{
			AssertNull("Should default to null.", Provider.ReferralRequestReference);

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			((IAmendedItemsProvider)Provider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals("Should pick the code from the request header.", "A70", Provider.ReferralRequestReference);
		}

		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;
			AssertNull("Default to null", provider.AmendedItems);
		}
	}
}
