using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class JobDeclarationValidationHelperTest : TestCaseWithFactory
{
	public void TestIsAddressFilled()
	{
		CombineAssertions(() =>
		{
			var address = Factory.New<OrgAddress>();
			AssertEquals("No Field in Address filled", false, JobDeclarationValidationHelper.IsAddressFilled(address));

			address.OA_CompanyNameOverride = "XXX";
			address.OA_Address1 = "Address1";
			address.OA_City = "City";
			AssertEquals("Nationality And PostCode required", false, JobDeclarationValidationHelper.IsAddressFilled(address));

			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			address.OA_PostCode = "PostCode";
			AssertEquals("All Required Fields filled",true, JobDeclarationValidationHelper.IsAddressFilled(address));
		});
	}
}
