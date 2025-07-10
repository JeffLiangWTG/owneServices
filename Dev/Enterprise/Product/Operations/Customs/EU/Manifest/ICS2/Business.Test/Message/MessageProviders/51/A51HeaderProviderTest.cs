using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A51HeaderProviderTest : SendAndAmendHeader51ProviderBaseTest<A51HeaderProvider>
	{
		public override void TestReferralRequestReference()
		{
			AssertNull("Should default to null.", Provider.ReferralRequestReference);

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A51";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI);
			Provider.AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals("Should pick the code from the request header.", "A51", Provider.ReferralRequestReference);
		}

		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;

			AssertNull(provider.AmendedItems);
		}
	}
}
