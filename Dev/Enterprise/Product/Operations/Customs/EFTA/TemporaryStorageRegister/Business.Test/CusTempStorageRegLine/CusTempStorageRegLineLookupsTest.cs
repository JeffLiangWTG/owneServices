using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineLookups))]
sealed class CusTempStorageRegLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOwnerReferenceTypeList()
	{
		AssertSame(lookups.OwnerReferenceTypeList, lookups.OwnerReferenceTypeList);
	}

	public void TestPackageTypeList()
	{
		SetUpRefData();
		Factory.Save();

		CombineAssertions(() =>
		{
			var packageTypeList = lookups.PackageTypeList;
			AssertEquals("Count", 5, packageTypeList.Count);
			AssertEquals("Contains VQ?", expected: true, packageTypeList.ContainsCode("VQ"));
			AssertEquals("Contains VG?", expected: true, packageTypeList.ContainsCode("VG"));
			AssertEquals("Contains NE?", expected: true, packageTypeList.ContainsCode("NE"));
			AssertEquals("Contains NF?", expected: true, packageTypeList.ContainsCode("NF"));
			AssertEquals("Contains AA?", expected: true, packageTypeList.ContainsCode("AA"));
			AssertSame("Cached", packageTypeList, lookups.PackageTypeList);
		});
	}

	public void TestCustomsStatusList()
	{
		AssertSame(lookups.CustomsStatusList, lookups.CustomsStatusList);
	}

	public void TestUnionStatusList()
	{
		AssertSame(lookups.UnionStatusList, lookups.UnionStatusList);
	}

	public void TestWeightUQLookup()
	{
		CombineAssertions(() =>
		{
			var lookup = lookups.WeightUQList;
			var list = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

			AssertSame(list, lookup);
			AssertEquals("Lookup should contain KG for Kilograms", expected: true, lookup.ContainsCode("KG"));
		});
	}

	public void TestBulkPackageUnitTypeList()
	{
		SetUpRefData();
		Factory.Save();

		CombineAssertions(() =>
		{
			var unitTypesList = lookups.BulkPackageUnitTypeList;
			AssertEquals("Count", 2, unitTypesList.Count);
			AssertEquals("Contains VQ?", expected: true, unitTypesList.ContainsCode("VQ"));
			AssertEquals("Contains VG?", expected: true, unitTypesList.ContainsCode("VG"));
			AssertSame("Cached", unitTypesList, lookups.BulkPackageUnitTypeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "AAA";
		header.SRH_Reference = "reference";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		lookups = line.Lookups;
	}
	CusTempStorageRegLineLookups lookups;

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		_ = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
	}
}
