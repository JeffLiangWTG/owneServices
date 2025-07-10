using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	public class AuthorityToActValidatorTest : TestCaseWithFactory
	{
		public void TestCountrySpecificNameForPOA()
		{
			AssertEquals("written authority to act", new AuthorityToActValidator().CountrySpecificNameForPOA);
		}
	}
}
