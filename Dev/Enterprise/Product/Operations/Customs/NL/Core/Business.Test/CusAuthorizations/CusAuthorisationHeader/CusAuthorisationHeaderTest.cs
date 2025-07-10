using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusAuthorisationHeader))]
class CusAuthorisationHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestAuthorizationDescription()
	{
		authorisationHeader.CPH_Type = "";
		AssertEquals("Empty Description", "", authorisationHeader.AuthorizationTypeDescription);

		authorisationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		AssertEquals("LFR Description", NLCusAuthorisationHeaderTypeList.Descriptions.LFR, authorisationHeader.AuthorizationTypeDescription);

		authorisationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.C501;
		AssertEquals("AEOC Description", NLCusAuthorisationHeaderTypeList.Descriptions.C501, authorisationHeader.AuthorizationTypeDescription);

		authorisationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.C503;
		AssertNotEquals("AEOF Description should not equal", NLCusAuthorisationHeaderTypeList.Descriptions.C501, authorisationHeader.AuthorizationTypeDescription);
	}

	public void TestIsLFRConfigItemCopiedToAuthorizationNumber()
	{
		OrgHeader organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		OrgCusCode cusCode = organisation.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
		cusCode.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode;
		cusCode.OK_CustomsRegNo = "xxxxxxxxxB02";

		authorisationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.LFR;
		authorisationHeader.CPH_OH_PermitHolder = organisation.PK;

		AssertEquals("Authorization Number", cusCode.OK_CustomsRegNo, authorisationHeader.CPH_Number);
	}

	public void TestCusAuthorisationHeaderValidationType()
	{
		AssertType<CusAuthorisationHeaderValidation>("CusAuthorisationHeaderValidation", authorisationHeader.GetNewValidation());
	}

	public void TestCusAuthorisationRules()
	{
		AssertType<CusAuthorisationRuleCollection>(authorisationHeader.CusAuthorisationRules);
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorisationHeader = Factory.New<CusAuthorisationHeader>();
	}

	CusAuthorisationHeader authorisationHeader;
}
