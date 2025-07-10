using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

public class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateLFR()
	{
		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "xxxxxxxxxB02";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "LFR VAT number must end with B02");

			cusCode.OK_CustomsRegNo = "xxxxxxxxxb02";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "LFR VAT number must end with B02");

			cusCode.OK_CustomsRegNo = "xxxxxxxxxB01";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "LFR VAT number must end with B02");
		});
	}

	#region Implementation

	OrgCusCode cusCode;
	OrgHeader organisation;

	protected override void SetUp()
	{
		base.SetUp();
		organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "ORGH1";
		organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		cusCode = organisation.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
		cusCode.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode;
	}

	#endregion
}
