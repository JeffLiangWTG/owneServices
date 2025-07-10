using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class PermitItemDetailLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCY_CodeList()
	{
		RefCusCodeTestHelper.CreatePermitItemDetailsKeyCodeListAndFrenchLanguage(Factory);

		AssertEquals(2, lookups.CY_CodeList.Count);
		CombineAssertions("PermitItemDetailsKeyCodeList should contain these codes", () =>
		{
			Assert(lookups.CY_CodeList.ContainsCode("1"));
			Assert(lookups.CY_CodeList.ContainsCode("2"));
		});
	}

	public void TestCITESCommodityTypeList()
	{
		RefCusCodeTestHelper.CreateCITESCommodityTypeList(Factory);
		var list = lookups.CITESCommodityTypeList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.ValidCITESCommodityTypeListCode).Any());
			AssertEquals("invalid code", false, list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.InvalidCITESCommodityTypeListCode).Any());
		});
	}

	public void TestCITESScientificNameList()
	{
		RefCusCodeTestHelper.CreateCITESScientificNameList(Factory);
		var list = lookups.CITESScientificNameList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertEquals("valid code", true, list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.ValidCITESScientificNameListCode).Any());
			AssertEquals("invalid code", false, list.Find(c => c.ZZD_Code == RefCusCodeTestHelper.InvalidCITESScientificNameListCode).Any());
		});
	}

	public void TestCITESCodeList()
	{
		CombineAssertions(() =>
		{
			permitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key3;
			AssertSame($"Key {permitItemDetail.CY_Code}", lookups.CITESCommodityTypeList, lookups.CITESCodeList);
			permitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key4;
			AssertSame($"Key {permitItemDetail.CY_Code}", lookups.CITESScientificNameList, lookups.CITESCodeList);

			permitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key1;
			AssertNull($"Key {permitItemDetail.CY_Code}", lookups.CITESCodeList);
			permitItemDetail.CY_Code = PermitItemDetailKeyList.Codes.Key2;
			AssertNull($"Key {permitItemDetail.CY_Code}", lookups.CITESCodeList);
			permitItemDetail.CY_Code = "9";
			AssertNull($"Key {permitItemDetail.CY_Code}", lookups.CITESCodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var permit = Factory.New<Permit>();
		permitItemDetail = permit.PermitItemDetails.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.SingleEPermit;
		permit.CSI_DataModel = "CH";
		lookups = permitItemDetail.Lookups;
	}
	PermitItemDetail permitItemDetail;
	PermitItemDetailLookups lookups;
}
