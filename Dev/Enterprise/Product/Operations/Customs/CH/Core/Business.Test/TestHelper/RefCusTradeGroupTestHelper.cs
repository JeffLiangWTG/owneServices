using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class RefCusTradeGroupTestHelper
{
	const string DataGrouping = Core.Constants.CountryCodes.Switzerland;

	internal const string EFTATradeGroup = UniversalReferenceConstants.TradeGroup.EFTACountries;
	internal const string CountryInEFTATradeGroup = "IS";
	internal const string CountryNotInEFTATradeGroup = "AI";

	internal const string DevelopingCountriesTradeGroup = UniversalReferenceConstants.TradeGroup.DevelopingCountries;
	internal const string CountryInDevelopingCountriesTradeGroup = "AI";
	internal const string CountryNotInDevelopingCountriesTradeGroup = "SA";

	internal const string CountriesOutsideSecurityZoneTradeGroup = UniversalReferenceConstants.TradeGroup.CountriesOutsideSecurityZone;
	internal const string CountryInOutsideSecurityZoneTradeGroup = "AU";
	internal const string CountryNotInOutsideSecurityZoneTradeGroup = "CH";

	internal const string CountryInNCL0147CountryList = "AT";
	internal const string CountryInEUNButNotInEUSEC = "AU";
	internal const string CountryInEUSECButNotInEUN = "CH";

	internal static void CreateTradeGroups(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		var eftaTradeGroup = helper.CreateTradeGroup(DataGrouping, EFTATradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(eftaTradeGroup, CountryInEFTATradeGroup);

		var developmentCountriesTradeGroup = helper.CreateTradeGroup(DataGrouping, DevelopingCountriesTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(developmentCountriesTradeGroup, CountryInDevelopingCountriesTradeGroup);

		var countriesOutsideSecurityZoneTradeGroup = helper.CreateTradeGroup(DataGrouping, CountriesOutsideSecurityZoneTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(countriesOutsideSecurityZoneTradeGroup, CountryInOutsideSecurityZoneTradeGroup);

		factory.Save();
	}

	public static void CreateTestNCL0147CountryList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var startDate = ZDate.Today.AddMonths(-1);
		var endDate = ZDate.Today.AddMonths(1);
		var eusec = Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUForSafetyAndSecurity;
		var eun = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		var tradeGroupEUNEUSEC = helper.LoadOrCreateTradeGroup(eun, eusec, startDate, endDate);
		helper.AddCountry(tradeGroupEUNEUSEC, CountryInNCL0147CountryList, startDate, endDate);

		var tradeGroupEUNEUCTP = helper.CreateTradeGroup(eun, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, startDate, endDate);
		helper.AddCountry(tradeGroupEUNEUCTP, CountryInEUNButNotInEUSEC, startDate, endDate);

		var tradeGroupZZEUSEC = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, eusec, startDate, endDate);
		helper.AddCountry(tradeGroupZZEUSEC, CountryInEUSECButNotInEUN, startDate, endDate);
		factory.Save();
	}
}
