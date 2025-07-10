using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(JPGlbCompanyWrapper))]
	sealed class JPGlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<JPGlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is JP.", wrapper.IsValidWrapper);
		}
	}
}
