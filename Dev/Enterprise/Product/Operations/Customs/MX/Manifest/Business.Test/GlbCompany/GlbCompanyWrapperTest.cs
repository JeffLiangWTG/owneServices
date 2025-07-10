using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	public class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is MX.", wrapper.IsValidWrapper);
		}
	}
}
