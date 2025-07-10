using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACountryPreference))]
	sealed class CACountryPreferenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			((CACountryPreference)BusinessObject).CA_CountryCode = "12";
			AssertEquals("HumanReadableName", "Country/Region Code: '12'", BusinessObject.HumanReadableName);
		}
	}
}
