using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CHGSPOCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.OwnerReferenceTypeList;
				AssertEquals("CodesAsString", "AWB, ULD, ZZZ", list.CodesAsString);
				AssertSame("Cached", list, lookups.OwnerReferenceTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CHGSPOCusTempStorageLineLookups(Factory.New<CHGSPOCusTempStorageLine>());
		}
		CHGSPOCusTempStorageLineLookups lookups;
	}
}
