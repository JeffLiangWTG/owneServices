using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class REXDISCusTempStorageReExportLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
				AssertEquals("Codes", "AWB, SIN, ULD, ZZZ", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue("DE|REXDISCusTempStorageReExportLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList()));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<REXDISCusTempStorageReExportLine>();
		}
		REXDISCusTempStorageReExportLine storageLine;
	}
}
