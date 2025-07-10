using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BEGlbCompanyWrapper))]
public class BEGlbCompanyWrapperTest : Enterprise.MasterFiles.Business.Testing.GlbCompanyWrapperTest<BEGlbCompanyWrapper>
{
	public void TestIsValidWrapper()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

		CombineAssertions(() =>
		{
			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is BE.", wrapper.IsValidWrapper);

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals("Should be false as the country code is DE.", false, wrapper.IsValidWrapper);
		});
	}
}
