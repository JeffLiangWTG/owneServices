using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class CustomsCountryJurisdictionTest : TestCase
	{
		public void TestGetCustomsCountryOfJurisdiction()
		{
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("Puerto Rico should be under US Customs Jurisdiction", Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.PuertoRico));
			AssertEquals(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Switzerland));
			AssertEquals("Liechtenstein should be under CH Customs Jurisdiction", Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Liechtenstein));
			AssertEquals(Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Australia));

			AssertEquals(Core.Constants.CountryCodes.Austria, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Austria));
			AssertEquals(Core.Constants.CountryCodes.Belgium, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Belgium));
			AssertEquals(Core.Constants.CountryCodes.Bulgaria, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Bulgaria));
			AssertEquals(Core.Constants.CountryCodes.Croatia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Croatia));
			AssertEquals(Core.Constants.CountryCodes.Cyprus, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Cyprus));
			AssertEquals(Core.Constants.CountryCodes.CzechRepublic, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.CzechRepublic));
			AssertEquals(Core.Constants.CountryCodes.Denmark, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Denmark));
			AssertEquals(Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Germany));
			AssertEquals(Core.Constants.CountryCodes.Estonia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Estonia));
			AssertEquals(Core.Constants.CountryCodes.Finland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Finland));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.France));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.FrenchGuyana));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Guadeloupe));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Martinique));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Mayotte));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Reunion));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.SaintBarthelemy));
			AssertEquals(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.SaintMartin));
			AssertEquals(Core.Constants.CountryCodes.Greece, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Greece));
			AssertEquals(Core.Constants.CountryCodes.Hungary, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Hungary));
			AssertEquals(Core.Constants.CountryCodes.Ireland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Ireland));
			AssertEquals(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Italy));
			AssertEquals(Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Latvia));
			AssertEquals(Core.Constants.CountryCodes.Lithuania, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Lithuania));
			AssertEquals(Core.Constants.CountryCodes.Luxembourg, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Luxembourg));
			AssertEquals(Core.Constants.CountryCodes.Malta, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Malta));
			AssertEquals(Core.Constants.CountryCodes.Netherlands, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Netherlands));
			AssertEquals(Core.Constants.CountryCodes.Poland, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Poland));
			AssertEquals(Core.Constants.CountryCodes.Portugal, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Portugal));
			AssertEquals(Core.Constants.CountryCodes.Romania, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Romania));
			AssertEquals(Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Spain));
			AssertEquals(Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Sweden));
			AssertEquals(Core.Constants.CountryCodes.Slovakia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Slovakia));
			AssertEquals(Core.Constants.CountryCodes.Slovenia, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.Slovenia));
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes));
		}
	}
}
