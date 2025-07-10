using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2AmendedItemsHeader))]
	sealed class ICS2AmendedItemsHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmendedItems()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;
			requestHeader1.EUS_TransportDocumentType = "TEST";
			requestHeader1.EUS_MemberState = "DE";
			requestHeader1.EUS_Status = "SNT";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "B01";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;
			requestHeader2.EUS_TransportDocumentType = "RTR";
			requestHeader2.EUS_MemberState = "IE";
			requestHeader2.EUS_Status = "AWA";
			requestHeader2.RequestResponses.AddNew();

			var requestHeader3 = manifestHeader.RequestHeaders.AddNew();
			requestHeader3.EUS_Identifier = "B02";
			requestHeader3.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			requestHeader3.EUS_TransportDocumentType = "RTR";
			requestHeader3.EUS_MemberState = "IE";
			requestHeader3.EUS_Status = string.Empty;
			requestHeader3.RequestResponses.AddNew();

			var requestHeader4 = manifestHeader.RequestHeaders.AddNew();
			requestHeader4.EUS_Identifier = "C85";
			requestHeader4.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			requestHeader4.EUS_TransportDocumentType = "RTR";
			requestHeader4.EUS_MemberState = "IE";
			requestHeader4.EUS_Status = string.Empty;

			CombineAssertions(() =>
			{
				var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				var amendedItems = amendedItemsHeader.AmendedItems;

				var item = (ICS2AmendedItem)amendedItems.Single();
				AssertEquals("A70", item.Identifier);
				AssertEquals("A70 - Should default to false as there are not any RequestResponse data.", false, item.IsSelected);

				amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFS, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				amendedItems = amendedItemsHeader.AmendedItems;

				AssertContainsExactElementsInAnyOrder(new string[] { "B01", "A70" }, amendedItemsHeader.AmendedItems.Select(c => c.Identifier));
				AssertEquals("B01 - Should default to false as the status is not empty.", false, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().First(c => c.Identifier == "B01").IsSelected);

				amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				amendedItems = amendedItemsHeader.AmendedItems;
				AssertContainsExactElementsInAnyOrder(new string[] { "B02", "A70", "C85" }, amendedItemsHeader.AmendedItems.Select(c => c.Identifier));
				AssertEquals("B02 - Should default to true as the status of request is empty.", true, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().First(c => c.Identifier == "B02").IsSelected);
				AssertEquals("C85 - Should default to true as the status of request is empty.", true, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().First(c => c.Identifier == "C85").IsSelected);

				amendedItemsHeader = new ICS2AmendedItemsHeader(MessageTypes.Codes.R02, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
				amendedItems = amendedItemsHeader.AmendedItems;
				AssertContainsExactElementsInAnyOrder(new string[] { "B02", "A70", "C85" }, amendedItemsHeader.AmendedItems.Select(c => c.Identifier));
				AssertEquals("B02 - Should default to true.", true, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().First(c => c.Identifier == "B02").IsSelected);
				AssertEquals("C85 - Should default to false as there are not any RequestResponse data.", false, amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>().First(c => c.Identifier == "C85").IsSelected);
			});
		}

		public void TestHasAmendedItems()
		{
			var header = (ICS2AmendedItemsHeader)GetNewBusinessObject();
			Assert("Should default to false.", !header.HasAmendedItems());

			header = (ICS2AmendedItemsHeader)GetNewBusinessObject();
			var manifestHeader = header.ManifestHeader;

			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			Assert("Should be true for AMD header as there is at least one request header which type is AMD.", header.HasAmendedItems());
			Assert("Should be false for RFI header.", !new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI).HasAmendedItems());
		}

		public void TestRunPreSaveValidation()
		{
			var error = "Please select at least one valid referral request to send the message.";

			var header = (ICS2AmendedItemsHeader)GetNewBusinessObject();
			header.RunPreSaveValidation();

			AssertHasRowError(header, error);

			var manifestHeader = header.ManifestHeader;
			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			header = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			header.AmendedItems[0].IsSelected = false;
			header.RunPreSaveValidation();

			AssertHasRowError(header, error);

			header.AmendedItems[0].IsSelected = true;
			header.RunPreSaveValidation();

			AssertNoRowError(header, error);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			return new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
		}
	}
}
