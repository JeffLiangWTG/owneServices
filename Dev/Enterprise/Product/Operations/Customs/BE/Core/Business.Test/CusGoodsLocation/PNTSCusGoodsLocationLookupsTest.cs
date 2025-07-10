using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSCusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestQualifierList()
	{
		CombineAssertions(() =>
		{
			var qualifierList = lookups.QualifierList;
			AssertContainsExactElementsInAnyOrder("Elements", new ZString[] { "U", "V", "W", "X", "Y" }, qualifierList.GetAllCodes());
			AssertSame("Cached", qualifierList, lookups.QualifierList);
		});
	}

	public void TestTypeList_Qualfier_U()
	{
		AssertTypeList("U", new ZString[] { "A", "D" });
	}

	public void TestTypeList_Qualfier_V()
	{
		AssertTypeList("V", new ZString[] { "A", "B", "C", "D" });
	}

	public void TestTypeList_Qualfier_W()
	{
		AssertTypeList("W", new ZString[] { "D" });
	}

	public void TestTypeList_Qualfier_X()
	{
		AssertTypeList("X", new ZString[] { "D" });
	}

	public void TestTypeList_Qualfier_Y()
	{
		AssertTypeList("Y", new ZString[] { "D" });
	}

	void AssertTypeList(string qualifier, ZString[] expectedList)
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_Qualifier = qualifier;
			var typeList = lookups.TypeList;
			AssertContainsExactElementsInAnyOrder("Elements", expectedList, typeList.GetAllCodes());
			AssertSame("Cached", typeList, lookups.TypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		cusGoodsLocation = (CusGoodsLocation)temporaryStorageHeader.GoodsLocation;
		lookups = (PNTSCusGoodsLocationLookups)cusGoodsLocation.Lookups;
	}
	PNTSCusGoodsLocationLookups lookups;
	CusGoodsLocation cusGoodsLocation;
}
