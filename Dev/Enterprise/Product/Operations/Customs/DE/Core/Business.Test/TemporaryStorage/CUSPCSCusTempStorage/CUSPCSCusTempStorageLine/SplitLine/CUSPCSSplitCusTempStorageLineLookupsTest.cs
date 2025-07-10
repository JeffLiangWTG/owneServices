using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CUSPCSSplitCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			var result = storageLine.Lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AWB, ULD, ZZZ", result.CodesAsString);
				AssertSame("Cached", result, Factory.GetCachedValue("DE|CUSPCSSplitCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList()));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CUSPCSSplitCusTempStorageLine>();
		}
		CUSPCSSplitCusTempStorageLine storageLine;
	}
}
