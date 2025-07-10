using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	sealed class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is IL.", wrapper.IsValidWrapper);
		}

		public void TestPasswordType()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Israel;
			var wrapper = GetWrapper(company);
			AssertEquals("Password Type should be ILC", "ILC", wrapper.GlbExternalPassword.GP_PasswordType);
		}
	}
}
