using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CodeType()
		{
			var organization = Factory.New<OrgHeader>();
			var cusCode = organization.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CustomsRegNo = "12345678900";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CustomsRegNo = " 12345-6789-0 ";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CustomsRegNo = "1 23.45/67X890 ";
			AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CustomsRegNo = "1 23.45/67890 ";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.OK_CustomsRegNo = "ABC";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
			cusCode.OK_CustomsRegNo = "123456789";
			AssertNoErrorContaining(cusCode.OK_CustomsRegNoInfo, "Business Number (GBR) must be entered as 10 digits.");
		}

		public void TestCarrierCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.KRForwarderIDs, "Forwarder IDs");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, RefCusCodeListTypes.Codes.KRForwarderIDs, "0001", "운송주선인부호1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var organization = Factory.New<OrgHeader>();
			var cusCode = organization.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "XXXX";
			AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The code entered does not exist in the Korea Forwarder IDs Reference database.");

			cusCode.OK_CustomsRegNo = "0001";
			AssertNoMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, "The code entered does not exist in the Korea Forwarder IDs Reference database.");
		}

		public void TestUnipassID()
		{
			var businesses = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "기업");
			var businessesCusCode = businesses.CustomsCodes.AddNew();
			businessesCusCode.OK_CodeType = IdentificationType.UnipassIDForIndividual;
			AssertHasErrorContaining(businessesCusCode.OK_CodeTypeInfo, "Businesses cannot use code 05.");

			businessesCusCode.OK_CodeType = IdentificationType.UnipassIDForOrganization;
			AssertNoErrors(businessesCusCode.OK_CodeTypeInfo);

			var individuals = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "NAT", "RK2", "개인");
			var individualsCusCode = individuals.CustomsCodes.AddNew();
			individualsCusCode.OK_CodeType = IdentificationType.UnipassIDForOrganization;
			AssertHasErrorContaining(individualsCusCode.OK_CodeTypeInfo, "Individuals cannot use code 06.");

			individualsCusCode.OK_CodeType = IdentificationType.UnipassIDForIndividual;
			AssertNoErrors(individualsCusCode.OK_CodeTypeInfo);
		}
	}
}
