using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestFacilitiesList()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000001", "BE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000002", "BE000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000003", "BE000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000004", "BE000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		Factory.Save();

		CombineAssertions(() =>
		{
			((ZZRefCusCodeListCombinedCollection)lookups.UnlocodeList).Load();
			var codes = lookups.UnlocodeList;
			AssertContainsExactElementsInAnyOrder("Elements", new ZString[] { "BE000001", "BE000002", "BE000003", "BE000004" }, ((ZZRefCusCodeListCombinedCollection)lookups.UnlocodeList).Select(x => x.ZZD_Code));
			AssertSame("Cached", codes, lookups.UnlocodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var cusGoodsLocation = (CusGoodsLocation)instruction.GoodsLocation;
		lookups = (CusGoodsLocationLookups)cusGoodsLocation.Lookups;
	}
	CusGoodsLocationLookups lookups;
}
