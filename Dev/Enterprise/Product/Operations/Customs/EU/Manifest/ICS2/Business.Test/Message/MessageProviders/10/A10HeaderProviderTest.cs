using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class A10HeaderProviderTest : SendAndAmendHeader10ProviderBaseTest<A10HeaderProvider>
	{
		public void TestReferralRequestReference()
		{
			AssertNull("Should default to null.", Provider.ReferralRequestReference);

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI);
			Provider.AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals("Should pick the code from the request header.", "A70", Provider.ReferralRequestReference);
		}

		public void TestAmendedItems()
		{
			AssertNull(Provider.AmendedItems);
		}
	}
}
