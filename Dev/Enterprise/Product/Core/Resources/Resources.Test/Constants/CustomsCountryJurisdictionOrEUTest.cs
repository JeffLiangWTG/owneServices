using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class CustomsCountryJurisdictionOrEUTest : TestCase
	{
		public void TestGetCustomsCountryOfJurisdictionOrEU()
		{
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("Puerto Rico should be under US Customs Jurisdiction", Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.PuertoRico));
			AssertEquals(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Switzerland));
			AssertEquals("Liechtenstein should be under CH Customs Jurisdiction", Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Liechtenstein));
			AssertEquals(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Australia));

			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Austria));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Belgium));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Bulgaria));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Croatia));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Cyprus));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.CzechRepublic));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Denmark));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Germany));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Estonia));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Finland));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.France));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Greece));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Hungary));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Ireland));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Italy));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Latvia));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Lithuania));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Luxembourg));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Malta));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Netherlands));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Poland));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Portugal));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Romania));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Spain));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Sweden));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Slovakia));
			AssertEquals(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Slovenia));

			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.FrenchGuyana));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Guadeloupe));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Martinique));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Mayotte));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.Reunion));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.SaintBarthelemy));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(Core.Constants.CountryCodes.SaintMartin));
		}
	}
}
