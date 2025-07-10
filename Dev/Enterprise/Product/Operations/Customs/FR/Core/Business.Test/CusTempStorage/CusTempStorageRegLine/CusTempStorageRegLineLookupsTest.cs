using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageRegLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			var list = lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "AWB, CTR, HWB, MBL, TRK", list.CodesAsString);
				AssertSame("Cached", list, lookups.OwnerReferenceTypeList);
			});
		}

		public void TestPackageTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list = lookups.PackageTypeList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "VG", "NE" }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.PackageTypeList);
			});
		}

		public void TestUnionStatusList()
		{
			var list = lookups.UnionStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "C, T1, T2, T2F, TD, TF, X", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnionStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			lookups = new CusTempStorageRegLineLookups(line);
		}
		CusTempStorageRegLineLookups lookups;
	}
}
