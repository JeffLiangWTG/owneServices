using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	public static class MessageDataProviderTestHelper
	{
		public static MessageSendingObject SetUpMessageSendingObject(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "TestOffice";
			header.PresentationOffice = "Office";
			header.AMA_AgentType = "DIR";
			header.AMA_PaymentAccountNumber = "12345";
			header.AMA_PaymentMethod = IE.Business.PaymentMethodList.Codes.E;

			var declarantHeader = factory.New<OrgHeader>();
			declarantHeader.OH_FullName = "TestDeclarant";

			var declarantContact = declarantHeader.AllocatedContacts.AddNew();
			declarantContact.OC_ContactName = "declarantContactName";
			declarantContact.OC_Phone = "1234567";
			declarantContact.OC_Email = "123@test.com";

			var declarantContactAllocation = declarantContact.Allocations.AddNew();
			declarantContactAllocation.PC_Type = "CUS";

			var declarant = declarantHeader.Addresses.AddNew();
			declarant.OA_CompanyNameOverride = "declarant";
			declarant.OA_Address1 = "declarantAddress1";
			declarant.OA_Address2 = "declarantAddress2";
			declarant.OA_PostCode = "233333";
			declarant.City = "declarantCity";
			declarant.OA_RN_NKCountryCode = "AU";

			header.AMA_OA_Declarant = declarant.PK;

			var representativeHeader = factory.New<OrgHeader>();
			var representativeContact = representativeHeader.AllocatedContacts.AddNew();
			representativeContact.OC_ContactName = "declarantContactName";
			representativeContact.OC_Phone = "1234567";
			representativeContact.OC_Email = "123@test.com";

			var representativeContactAllocation = representativeContact.Allocations.AddNew();
			representativeContactAllocation.PC_Type = "CUS";

			var representative = representativeHeader.Addresses.AddNew();
			representative.OA_CompanyNameOverride = "representative";
			representative.OA_RN_NKCountryCode = "US";

			var customsCode = representative.CustomsCodes.AddNew();
			customsCode.OK_CodeType = "EOR";
			customsCode.OK_CustomsRegNo = "1234412";

			header.AMA_OA_Representative = representative.PK;

			var bill = header.Bills.AddNew();
			bill.ABL_ConsigneeName = "Consignee";
			bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			bill.ABL_ConsigneePostcode = "CPostcode";
			bill.ABL_ConsigneeCity = "ConsigneeCity";
			bill.ABL_RN_NKConsigneeCountry = "US";
			bill.ABL_GoodsValue = 100;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";

			bill.LocalReferenceNumber = "TestLRN";
			bill.MovementReferenceNumber = "TestMRN";

			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_Tariff = "TestTariff1";
			packedItem1.API_GoodsValue = 45;
			packedItem1.API_RX_NKGoodsValueCurrency = "USD";
			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_GoodsValue = 55;
			packedItem2.API_Tariff = "TestTariff2";
			packedItem2.API_RX_NKGoodsValueCurrency = "USD";

			var messageSendingObject = new MessageSendingObject(bill);
			return messageSendingObject;
		}
	}
}
