using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

public class RefCusTradeGroupLoaderTest : TestCaseWithFactory
{
	public void TestGetTradeGroup()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);

		var tradeGroup = RefCusTradeGroupLoader.GetTradeGroup(Factory, RefCusTradeGroupTestHelper.DevelopingCountriesTradeGroup, ZDateTime.Now);
		CombineAssertions(() =>
		{
			AssertEquals("Country in group", true, tradeGroup.TradeGroupCountries.Any(c => c.ZZB_RN_NKTradeGroupCountryCode == RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup));
			AssertEquals("Country not in group", false, tradeGroup.TradeGroupCountries.Any(c => c.ZZB_RN_NKTradeGroupCountryCode == RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup));

			AssertNull("Unknown group", RefCusTradeGroupLoader.GetTradeGroup(Factory, "@@@", ZDateTime.Now));
		});
	}

	public void TestIsAnyCountryPartOfTradeGroup()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Country in group", true, RefCusTradeGroupLoader.IsAnyCountryPartOfTradeGroup(Factory, RefCusTradeGroupTestHelper.EFTATradeGroup, new ZString[] { RefCusTradeGroupTestHelper.CountryInEFTATradeGroup, RefCusTradeGroupTestHelper.CountryNotInEFTATradeGroup }, ZDateTime.Now));
			AssertEquals("Country not in group", false, RefCusTradeGroupLoader.IsAnyCountryPartOfTradeGroup(Factory, RefCusTradeGroupTestHelper.EFTATradeGroup, new ZString[] { RefCusTradeGroupTestHelper.CountryNotInEFTATradeGroup }, ZDateTime.Now));
		});
	}

	public void TestIsAnyCountryPartOfDevelopingCountries()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Country in group", true, RefCusTradeGroupLoader.IsAnyCountryPartOfDevelopingCountries(Factory, new ZString[] { RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup, RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup }, ZDateTime.Now));
			AssertEquals("Country not in group", false, RefCusTradeGroupLoader.IsAnyCountryPartOfDevelopingCountries(Factory, new ZString[] { RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup }, ZDateTime.Now));
		});
	}

	public void TestIsCountryPartOfDevelopingCountries()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);
		var countryInGroup = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup);
		var countryNotInGroup = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup);

		CombineAssertions(() =>
		{
			AssertEquals("Country in group", true, RefCusTradeGroupLoader.IsCountryPartOfDevelopingCountries(Factory, countryInGroup, ZDateTime.Now));
			AssertEquals("Country not in group", false, RefCusTradeGroupLoader.IsCountryPartOfDevelopingCountries(Factory, countryNotInGroup, ZDateTime.Now));
			AssertEquals("Country is null", false, RefCusTradeGroupLoader.IsCountryPartOfDevelopingCountries(Factory, null, ZDateTime.Now));
		});
	}
}
