using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidationOnAuthorizationHolderLocation()
	{
		var expectedError = "Authorization holder cannot be located in the same country where your company is situated.";
		OrgHeader organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		organisation.OH_RL_NKClosestPort = "NLAMS";

		OrgCusCode cusCode = organisation.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
		cusCode.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode;
		cusCode.OK_CustomsRegNo = "xxxxxxxxxB02";

		authorizationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		authorizationHeader.CPH_OH_PermitHolder = organisation.PK;
		AssertHasErrorContaining(authorizationHeader.CPH_OH_PermitHolderInfo, expectedError);

		OrgHeader organisation2 = Factory.New<OrgHeader>();
		organisation2.OH_Code = "ORGH2";
		organisation2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

		authorizationHeader.CPH_OH_PermitHolder = organisation2.PK;
		AssertNoErrorContaining(authorizationHeader.CPH_OH_PermitHolderInfo, expectedError);
	}

	public void TestValidationOnAuthorizationHolderLFRConfig()
	{
		var expectedError = "Authorization holder must have an LFR number.";
		OrgHeader organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		authorizationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		authorizationHeader.CPH_OH_PermitHolder = organisation.PK;
		AssertHasErrorContaining(authorizationHeader.CPH_OH_PermitHolderInfo, expectedError);

		OrgHeader organisation2 = Factory.New<OrgHeader>();
		organisation2.OH_Code = "ORGH2";
		organisation2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		OrgCusCode cusCode = organisation2.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
		cusCode.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode;
		cusCode.OK_CustomsRegNo = "xxxxxxxxxB02";

		authorizationHeader.CPH_OH_PermitHolder = organisation2.PK;
		AssertNoErrorContaining(authorizationHeader.CPH_OH_PermitHolderInfo, expectedError);
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorizationHeader = Factory.New<CusAuthorisationHeader>();
	}

	CusAuthorisationHeader authorizationHeader;
}
