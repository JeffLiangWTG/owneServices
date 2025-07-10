using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

public class LookupsHelperTest : TestCaseWithFactory
{
	public void TestRegionOrTerritoryOfDestinationList_DropEdit() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
		helper.CreateCusCodeListTerritory(countryCode, "02", "Test 2");
		helper.CreateCusCodeListTerritory(countryCode, "03", "Test 3");
		helper.CreateCusCodeListTerritory(countryCode, "04", "Test 4");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "05", "Test 5");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "06", "Test 6");
		helper.CreateCusCodeListCanaryIsland(countryCode, "07", "Test 7");
		helper.CreateCusCodeListCanaryIsland(countryCode, "08", "Test 8");
		_ = helper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "09", "Test 9", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var lookups = declaration.AddInfoChildLookups;
		var list = LookupsHelper.RegionOfDestinationDropEditList(Factory);

		AssertEquals("Number of codes in list", 10, list.Count);
		Assert(list.ContainsCode("01"));
		Assert(list.ContainsCode("02"));
		Assert(list.ContainsCode("03"));
		Assert(list.ContainsCode("04"));
		Assert(list.ContainsCode("05"));
		Assert(list.ContainsCode("06"));
		Assert(list.ContainsCode("07"));
		Assert(list.ContainsCode("08"));
		Assert(list.ContainsCode("55"));
		Assert(list.ContainsCode("56"));
		AssertEquals("Test 1", list.GetDescriptionFromCode("01"));
	});

	[TestDate(2021, 03, 29)]
	public void TestRegionOrTerritoryOfDestinationList_CodeFindBox() => CombineAssertions(() =>
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EuropeanUnionEUN;
		var esCode = Core.Constants.CountryCodes.Spain;
		var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
		helper.CreateNewOrGetExistingDataGrouping(esCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.CL142, "Region Of Destination");
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "DE", "DE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "FR", "FR", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "IE", "IE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 02, 01));
		helper.CreateCusCodeList(esCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "28", "MADRID", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
		helper.CreateCusCodeList(esCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "29", "MALAGA", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
		var lookups = declaration.AddInfoChildLookups;
		var list = LookupsHelper.RegionOfDestinationCodeFindBoxList(Factory);
		list.Load();
		
		AssertContainsExactElementsInAnyOrder("Elements", new[] { "DE", "FR", "28", "29" }, list.Select(x => x.ZZD_Code));
		AssertSame("Cached", list, lookups.RegionOrTerritoryOfDestinationList);
	});
}
