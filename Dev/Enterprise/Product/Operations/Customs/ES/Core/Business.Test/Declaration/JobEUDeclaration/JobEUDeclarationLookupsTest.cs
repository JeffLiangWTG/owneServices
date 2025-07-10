using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

public class JobEUDeclarationLookupsTest : TestCaseWithFactory
{
	public void TestRegionOrTerritoryOfDestinationList_DropEdit() => CombineAssertions(() =>
	{
		var helper = new ESUniversalReferenceTestDataHelper(Factory);
		var countryCode = Core.Constants.CountryCodes.Spain;
		helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
		helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "02", "Test 2");
		helper.CreateCusCodeListCanaryIsland(countryCode, "03", "Test 3");
		_ = helper.CreateCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "04", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var lookups = declaration.AddInfoChildLookups;
		AssertContainsExactElementsInExactOrder("Same list as helper", LookupsHelper.RegionOfDestinationDropEditList(Factory), lookups.RegionOrTerritoryOfDestinationList);
		AssertSame("List is cached", LookupsHelper.RegionOfDestinationDropEditList(Factory), lookups.RegionOrTerritoryOfDestinationList);
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
		helper.CreateCusCodeList(eunCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "IE", "IE", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 02, 01));
		helper.CreateCusCodeList(esCode, UniversalReferenceConstants.RefCusCodeListTypes.CL142, "28", "MADRID", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
		var lookups = declaration.AddInfoChildLookups;

		AssertContainsExactElementsInAnyOrder("Elements", LookupsHelper.RegionOfDestinationCodeFindBoxList(Factory), lookups.RegionOrTerritoryOfDestinationList);
		AssertSame("List is cached", LookupsHelper.RegionOfDestinationCodeFindBoxList(Factory), lookups.RegionOrTerritoryOfDestinationList);
	});
}
