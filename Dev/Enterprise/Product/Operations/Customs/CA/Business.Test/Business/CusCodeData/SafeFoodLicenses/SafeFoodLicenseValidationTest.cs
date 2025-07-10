using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing;

public class SafeFoodLicenseValidationTest :  CusCodeDataValidationTest
{
	public new void TestCheckCY_Code()
	{
		var safeFoodLicense1 = OrgImpAddInfo.SafeFoodLicenses.AddNew();
		ValidationTestHelper.AssertErrorIfNotEntered(safeFoodLicense1.CY_CodeInfo);

		safeFoodLicense1.CY_Code = "123";
		var safeFoodLicense2 = OrgImpAddInfo.SafeFoodLicenses.AddNew();
		safeFoodLicense2.CY_Code = "123";
		AssertHasError(safeFoodLicense2.CY_CodeInfo, "The License No has been duplicated and must be unique.");

		safeFoodLicense1.Validation.ValidateCY_Code();
		AssertHasError(safeFoodLicense1.CY_CodeInfo, "The License No has been duplicated and must be unique.");
	}

	OrgImpAddInfo OrgImpAddInfo
	{
		get { return orgImpAddInfo ??= OrgImpAddInfo.Get(Factory.New<OrgHeader>()); }
	}
	OrgImpAddInfo orgImpAddInfo;
}
