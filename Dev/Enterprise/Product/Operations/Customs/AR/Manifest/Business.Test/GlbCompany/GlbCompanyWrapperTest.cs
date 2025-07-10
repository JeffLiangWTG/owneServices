using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is AR.", wrapper.IsValidWrapper);
		}
	}
}
