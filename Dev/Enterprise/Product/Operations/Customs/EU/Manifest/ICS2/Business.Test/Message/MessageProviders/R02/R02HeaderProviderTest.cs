using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class R02HeaderProviderTest : ICS2BaseMessageProviderTest<R02HeaderProvider>
	{
		public void TestResponsibleMemberStateCountry()
		{
			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			requestHeader1.EUS_MemberState = "DE";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "A70";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;
			requestHeader2.EUS_MemberState = "DE";

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI);
			((IAmendedItemsProvider)Provider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();
			AssertEquals("ResponsibleMemberStateCountry", "DE", Provider.ResponsibleMemberStateCountry);
		}

		public void TestTransportDocumentMasterLevel()
		{
			AssertNull(Provider.TransportDocumentMasterLevel);

			var newHeaderProvider = GetProvider();
			manifestHeader.AMA_MasterBill = null;
			AssertNull(newHeaderProvider.TransportDocumentMasterLevel);

			newHeaderProvider = GetProvider();
			manifestHeader.AMA_MasterBill = "bill123";
			manifestHeader.MasterBill.TransportDocumentType = "N722";
			AssertEquals("TransportDocument DocumentNumber", "bill123", newHeaderProvider.TransportDocumentMasterLevel.Identifier);
			AssertEquals("TransportDocument Type", "N722", newHeaderProvider.TransportDocumentMasterLevel.Type);
		}

		public void TestDeclarantIdentificationNumber()
		{
			AssertEquals("Declarant IdentificationNumber", "DE654321", Provider.DeclarantIdentificationNumber);
		}

		public void TestAdditionalInformationResponses()
		{
			AssertEquals(0, Provider.AdditionalInformationResponses.Count);

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "DEYTZYIBTLRO4AG8S";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			requestHeader1.EUS_MemberState = "AT";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "4E253A6613394633B";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;
			requestHeader2.EUS_MemberState = "AT";

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI);

			var newHeaderProvider = GetProvider();
			((IAmendedItemsProvider)newHeaderProvider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertContainsExactElementsInAnyOrder(new[] { "DEYTZYIBTLRO4AG8S", "4E253A6613394633B" }, newHeaderProvider.AdditionalInformationResponses.Select(c => c.ReferralResponseReference));
		}

		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;
			AssertNull("Default to null", provider.AmendedItems);
		}
	}
}
