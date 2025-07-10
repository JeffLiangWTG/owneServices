using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CountrySpecificTypeStructTest : TestCase
	{
		public void TestIt()
		{
			CountrySpecificTypeDecider.CountrySpecificType countrySpecificType = new CountrySpecificTypeDecider.CountrySpecificType(Enterprise.Core.Constants.CountryCodes.France, delegate
			{ return typeof(string); });
			AssertEquals("Should be assigned in the constructor", Enterprise.Core.Constants.CountryCodes.France, countrySpecificType.CountryCode);
			AssertEquals("Should be assigned in the constructor", typeof(string), countrySpecificType.BusinessObjectType);
		}
	}
}
