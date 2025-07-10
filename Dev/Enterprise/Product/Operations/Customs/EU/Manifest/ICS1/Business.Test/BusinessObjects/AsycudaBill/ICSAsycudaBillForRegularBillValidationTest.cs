using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	class AsycudaBillForRegularBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_OA_Consignee()
		{
			var consol = Factory.New<ForwardingConsol>();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentTableCode = "JK";
			header.AMA_ParentId = consol.PK;
			header.SpecialMentions = ICSAsycudaBillValidationForRegularBill.NegotiableBOL;
			var bill = header.Bills.AddNew();

			var validation = bill.Validation;

			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);

			header.SpecialMentions = "";
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "You have not entered");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Consignee = org.MainAddress.PK;
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);

			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "An EORI or TCUIN number must be set up against the organization");

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "US");
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);

			org.CustomsCodes.RemoveAndDeleteAll();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "US");
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "An EORI or TCUIN number must be set up against the organization");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "LV");
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);
		}

		public void TestCheckABL_OA_Shipper()
		{
			var consol = Factory.New<ForwardingConsol>();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentTableCode = "JK";
			header.AMA_ParentId = consol.PK;
			var bill = header.Bills.AddNew();

			var validation = bill.Validation;

			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrors(bill.ABL_OA_ShipperInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, "You have not entered");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_Shipper = org.MainAddress.PK;
			validation.ValidateABL_OA_Shipper();
			AssertHasWarningContaining(bill.ABL_OA_ShipperInfo, "If the organization has a valid EORI Trader Identification number or a Third Country Unique Identification Number (TCUIN) which has been made available to the Union by the third country concerned, then the EORI or TCUIN must be supplied");

			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;
			validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, "Organization must have a valid EORI Trader Identification number or a Third Country Unique Identification Number(TCUIN)");

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "US");
			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrors(bill.ABL_OA_ShipperInfo);

			org.CustomsCodes.RemoveAndDeleteAll();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "US");
			validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, "Organization must have a valid EORI Trader Identification number or a Third Country Unique Identification Number(TCUIN)");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "LV");
			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrors(bill.ABL_OA_ShipperInfo);
		}

		public void TestCheckABL_OA_NotifyParty()
		{
			var consol = Factory.New<ForwardingConsol>();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentTableCode = "JK";
			header.AMA_ParentId = consol.PK;
			header.SpecialMentions = "";
			var bill = header.Bills.AddNew();

			var validation = bill.Validation;

			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);

			header.SpecialMentions = ICSAsycudaBillValidationForRegularBill.NegotiableBOL;
			validation.ValidateABL_OA_NotifyParty();
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "You have not entered");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			bill.ABL_OA_NotifyParty = org.MainAddress.PK;
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);

			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;
			validation.ValidateABL_OA_NotifyParty();
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "An EORI or TCUIN number must be set up against the organization");

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "US");
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);

			org.CustomsCodes.RemoveAndDeleteAll();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "US");
			validation.ValidateABL_OA_NotifyParty();
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "An EORI or TCUIN number must be set up against the organization");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", "LV");
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);
		}
	}
}
