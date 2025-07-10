using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOk_CustomsRegNoDANAndDAT()
	{
		cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
		cusCode.OK_CustomsRegNo = "1234";

		var cusCode2 = organisation.CustomsCodes.AddNew();
		cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
		cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
		cusCode2.OK_CustomsRegNo = "4321";

		AssertNoError(cusCode.OK_CustomsRegNoInfo, "Each 'DAN' and 'DAT' must have an unique code.");
		AssertNoError(cusCode2.OK_CustomsRegNoInfo, "Each 'DAN' and 'DAT' must have an unique code.");

		cusCode.OK_CustomsRegNo = "4321";

		AssertHasError(cusCode.OK_CustomsRegNoInfo, "Each 'DAN' and 'DAT' must have an unique code.");

		cusCode2.Validation.ValidateOK_CustomsRegNo();
		AssertHasError(cusCode2.OK_CustomsRegNoInfo, "Each 'DAN' and 'DAT' must have an unique code.");
	}

	public void TestCheckOk_CustomsRegCcpHasValidAuthorisationRule()
	{
		var expectedWarning = "This CCP code does not have an Authorization. Please consider adding this code as LOC in Authorizations for this Organization";

		cusCode.OK_CodeType = "CCP";
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
		cusCode.OK_CustomsRegNo = "C123456XIT ";
		AssertHasWarningContaining("When no authorisations have a LOC rule equal equal to entered CCP", cusCode.OK_CustomsRegNoInfo, expectedWarning);

		var authorisationHeader = Factory.New<CusAuthorisationHeader>();
		authorisationHeader.CPH_Type = "CWP";
		authorisationHeader.CPH_Number = "123456X";
		authorisationHeader.CPH_OH_PermitHolder = organisation.PK;
		cusCode.Validation.ValidateOK_CustomsRegNo();
		AssertHasWarningContaining("When no authorisations have a LOC rule equal equal to entered CCP", cusCode.OK_CustomsRegNoInfo, expectedWarning);

		var locRule = authorisationHeader.CusAuthorisationRules.AddNew();
		locRule.CPR_RuleCode = "LOC";

		locRule.CPR_ValueFrom = "X654321";
		cusCode.Validation.ValidateOK_CustomsRegNo();
		AssertHasWarningContaining("When no authorisations have a LOC rule equal to entered CCP", cusCode.OK_CustomsRegNoInfo, expectedWarning);

		locRule.CPR_ValueFrom = "123456X";
		Factory.Save();
		cusCode.Validation.ValidateOK_CustomsRegNo();
		AssertNoWarningContaining("When at least one authorisation have at least one LOC rule equal to entered CCP", cusCode.OK_CustomsRegNoInfo, expectedWarning);
	}

	#region Implementation

	OrgCusCode cusCode;
	OrgHeader organisation;

	protected override void SetUp()
	{
		base.SetUp();
		organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
		cusCode = organisation.CustomsCodes.AddNew();
	}

	#endregion
}
