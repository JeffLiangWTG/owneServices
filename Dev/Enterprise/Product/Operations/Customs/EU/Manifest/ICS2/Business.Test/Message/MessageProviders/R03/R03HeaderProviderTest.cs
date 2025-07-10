using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class R03HeaderProviderTest : ICS2BaseMessageProviderTest<R03HeaderProvider>
	{
		public void TestResponsibleMemberStateCountry()
		{
			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;
			requestHeader1.EUS_MemberState = "DE";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "A70";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;
			requestHeader2.EUS_MemberState = "DE";

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFS);
			((IAmendedItemsProvider)Provider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertEquals("ResponsibleMemberStateCountry", "DE", Provider.ResponsibleMemberStateCountry);
		}

		public void TestRepresentativeIdentificationNumber()
		{
			AssertEquals("Representative Identification Number is empty when shipping agent is not set for manifest header", string.Empty, Provider.RepresentativeIdentificationNumber);

			var shippingAgent = Factory.NewWithValidTestData<OrgAddress>();
			shippingAgent.OA_RN_NKCountryCode = CountryCodes.Germany;
			var orgHeader = shippingAgent.Header;
			var cusCodeEori = orgHeader.CustomsCodes.AddNew();
			cusCodeEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCodeEori.OK_RN_NKCodeCountry = CountryCodes.Germany;
			cusCodeEori.OK_CustomsRegNo = "EORI456";

			manifestHeader.AMA_OA_ShippingAgent = shippingAgent.PK;

			AssertEquals("Should get representative identification number from shipping agent header with contry code concatenated", "DEEORI456", new R03HeaderProvider(manifestHeader).RepresentativeIdentificationNumber);
		}

		public void TestTransportDocument()
		{
			AssertNull(Provider.TransportDocument);

			var newHeaderProvider = GetProvider();
			manifestHeader.AMA_MasterBill = null;
			AssertNull(newHeaderProvider.TransportDocument);

			newHeaderProvider = GetProvider();
			manifestHeader.AMA_MasterBill = "bill123";
			manifestHeader.MasterBill.TransportDocumentType = "ABCD";
			AssertEquals("TransportDocument DocumentNumber From Manifest Header", "bill123", newHeaderProvider.TransportDocument.Identifier);
			AssertEquals("TransportDocument Type From Manifest Header", "ABCD", newHeaderProvider.TransportDocument.Type);
		}

		public void TestDeclarantIdentificationNumber()
		{
			AssertEquals("Declarant IdentificationNumber", "DE654321", Provider.DeclarantIdentificationNumber);
		}

		public void TestHrcmScreeningResults()
		{
			AssertEquals(0, Provider.HrcmScreeningResults.Count);

			var hrcmScreeningOnHeader = manifestHeader.BillScreenings.AddNew();
			hrcmScreeningOnHeader.ASR_Result = "H01";

			var newProvider = GetProvider();
			AssertEquals("H01", newProvider.HrcmScreeningResults.Single().Result);

			var bill1 = manifestHeader.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.TransportDocumentType = "TD1";

			var hrcmScreeningOnBill1 = bill1.BillScreenings.AddNew();
			hrcmScreeningOnBill1.ASR_Result = "B01";

			var hrcmScreeningOnBill2 = bill1.BillScreenings.AddNew();
			hrcmScreeningOnBill2.ASR_Result = "B02";

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_HouseBillNumber = "HB001";

			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.TransportDocumentType = "TD2";

			var hrcmScreeningOnBill3 = bill2.BillScreenings.AddNew();
			hrcmScreeningOnBill3.ASR_Result = "B03";

			var hrcmScreeningOnBill4 = bill2.BillScreenings.AddNew();
			hrcmScreeningOnBill4.ASR_Result = "B04";

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;
			requestHeader2.EUS_Identifier = "B56";
			requestHeader2.EUS_HouseBillNumber = "HB002";

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFS);

			newProvider = GetProvider();
			((IAmendedItemsProvider)newProvider).AmendedItems = amendedItemsHeader.AmendedItems.ToArray<ICS2AmendedItem>();

			AssertContainsExactElementsInAnyOrder(new[] { "B01", "B02", "B03", "B04" }, newProvider.HrcmScreeningResults.Select(c => c.Result));
		}

		public void TestAmendedItems()
		{
			IAmendedItemsProvider provider = Provider;
			AssertNull("Default to null", provider.AmendedItems);
		}

		protected override IEnumerable<Expression<System.Func<R03HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.TransportDocument;
		}
	}
}
