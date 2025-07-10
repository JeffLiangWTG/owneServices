using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Testing
{
	class RefCountryHelperTest : TestCaseWithFactory
	{
		public void TestIsCountryPartOfEuropeanUnion()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty country should be false", false, RefCountryHelper.IsCountryPartOfEuropeanUnion(Factory, ZString.Empty));
				AssertEquals("Non existant country should be false", false, RefCountryHelper.IsCountryPartOfEuropeanUnion(Factory, "!$"));

				foreach (var country in Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, new[] { "IE", "FR", "DE", "IT", "US", "GB", "AU", "ZA" })))
				{
					var code = country.RN_Code;
					AssertEquals(code + " - Result should match RefCountry.IsPartOfEuropeanUnion", country.IsPartOfEuropeanUnion, RefCountryHelper.IsCountryPartOfEuropeanUnion(Factory, code));
				}
			});
		}
	}
}
