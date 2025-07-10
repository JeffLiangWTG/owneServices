using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestQualifierList()
	{
		var list = lookups.QualifierList;
		CombineAssertions(() =>
		{
			AssertEquals("List", "U, V", list.CodesAsString);
			AssertSame("Cached", list, lookups.QualifierList);
		});
	}

	public void TestQualifierListPhase5ArrivalIncident()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var cusGoodsLocation = (CusGoodsLocation)nctsHeader.EnRouteIncidents.AddNew().GoodsLocation;
		var lookups = cusGoodsLocation.Lookups;

		var list = lookups.QualifierList;
		CombineAssertions(() =>
		{
			AssertEquals("List", "U, W, Z", list.CodesAsString);
			AssertSame("Cached", list, lookups.QualifierList);
		});
	}

	public void TestTypeList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("List when Qualifier is empty", string.Empty, lookups.TypeList.CodesAsString);
			goodsLocation.CGL_Qualifier = "U";
			AssertEquals("List when Qualifier is U", "C", lookups.TypeList.CodesAsString);
			goodsLocation.CGL_Qualifier = "V";
			AssertEquals("List when Qualifier is V", "A", lookups.TypeList.CodesAsString);
			AssertSame("Cached", lookups.TypeList, lookups.TypeList);
		});
	}

	public void TestUnlocodeList()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000001", "BE000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, BE.Business.UniversalReferenceConstants.Role, BE.Business.UniversalReferenceConstants.Export);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000002", "BE000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, BE.Business.UniversalReferenceConstants.Role, "SUP");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BE000003", "BE000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BE000004", "BE000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, BE.Business.UniversalReferenceConstants.Role, BE.Business.UniversalReferenceConstants.Export);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
		Factory.Save();

		var codes = lookups.UnlocodeList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", goodsLocation.Lookups.UnlocodeList, codes);
			AssertCollectionContains("BE000001", "BE000001", codes, false);
			AssertCollectionContains("BE000002", "BE000002", codes, false);
			AssertCollectionContains("BE000003", "BE000003", codes, false);
			AssertCollectionContains("BE000004", "BE000004", codes, false);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		lookups = goodsLocation.Lookups;
	}

	NctsHeader nctsHeader;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationLookups lookups;
}
