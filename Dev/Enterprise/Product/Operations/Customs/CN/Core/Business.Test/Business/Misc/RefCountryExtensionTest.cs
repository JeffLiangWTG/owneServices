using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class RefCountryExtensionTest : TestCaseWithFactory
	{
		public void TestRefCountryExtension()
		{
			AssertEquals("CHN", RefCountry.LoadFromCountryCode(Factory, "CN").GetCNCountryCode());
			AssertEquals("USA", RefCountry.LoadFromCountryCode(Factory, "US").GetCNCountryCode());
			AssertEquals("ZZZ", RefCountry.LoadFromCountryCode(Factory, "XXX").GetCNCountryCode());
			AssertEquals("中国", RefCountry.LoadFromCountryCode(Factory, "CN").GetCNCountryName());
			AssertEquals("美国", RefCountry.LoadFromCountryCode(Factory, "US").GetCNCountryName());
			AssertEquals("国(地)别不详", RefCountry.LoadFromCountryCode(Factory, "XXX").GetCNCountryName());
		}

		public void TestGetCodeAndDescriptionWrapper()
		{
			var wrapper = RefCountry.LoadFromCountryCode(Factory, "CN").CreateCodeAndDescriptionWrapper(Factory);
			AssertEquals("CHN", wrapper.Code);
			AssertEquals("中国", wrapper.Description);
			wrapper = RefCountry.LoadFromCountryCode(Factory, "XXX").CreateCodeAndDescriptionWrapper(Factory);
			AssertEquals("ZZZ", wrapper.Code);
			AssertEquals("国(地)别不详", wrapper.Description);
		}
	}
}
