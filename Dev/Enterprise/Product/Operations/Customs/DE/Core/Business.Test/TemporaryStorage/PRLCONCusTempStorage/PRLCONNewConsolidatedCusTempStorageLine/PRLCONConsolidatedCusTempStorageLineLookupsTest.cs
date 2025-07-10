using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class PRLCONConsolidatedCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AWB, ULD, ZZZ", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue("DE|PRLCONNewConsolidatedCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList()));
			});
		}

		public void TestUnionStatusList()
		{
			var unionStatusList = storageLine.Lookups.UnionStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "F, N", unionStatusList.CodesAsString);
				AssertSame("Cached", unionStatusList, Factory.GetCachedValue("DE|PRLCONNewConsolidatedCusTempStorageLineLookups|UnionStatusList", () => new CodeDescriptionPairList()));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<PRLCONConsolidatedCusTempStorageLine>();
		}
		PRLCONConsolidatedCusTempStorageLine storageLine;
	}
}
