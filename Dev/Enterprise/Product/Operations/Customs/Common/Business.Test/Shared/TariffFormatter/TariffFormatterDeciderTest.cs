using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing
{
	class TariffFormatterDeciderTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetByCountryCode()
		{
			AssertGetByCountryCode(Core.Constants.CountryCodes.UnitedArabEmirates, "Enterprise.Customs.Business.TariffFormatter");
			AssertGetByCountryCode(Core.Constants.CountryCodes.Canada, "Enterprise.Customs.CA.Business.TariffFormatter");
			AssertGetByCountryCode("EUN", "Enterprise.Customs.EU.Business.TariffFormatterThirteen");
			AssertGetByCountryCode(Core.Constants.CountryCodes.Germany, "Enterprise.Customs.EU.Business.TariffFormatterEleven");
			AssertGetByCountryCode(Core.Constants.CountryCodes.NewZealand, "Enterprise.Customs.NZ.Business.TariffValidation.NZTariffFormatter");
			AssertGetByCountryCode("Shared", "Enterprise.Customs.Business.TariffFormatter");
			AssertGetByCountryCode(Core.Constants.CountryCodes.Eritrea, "Enterprise.Customs.Business.TariffFormatter");
			AssertGetByCountryCode(Core.Constants.CountryCodes.UnitedStates, "Enterprise.Customs.US.Business.TariffFormatter");
		}

		[ExpectNoExceptions]
		void AssertGetByCountryCode(string country, string fullname)
		{
			NUnit.Framework.Assert.That(TariffFormatterDecider.GetByCountryCode(country).GetType().FullName, Is.EqualTo(fullname));
		}
	}
}
