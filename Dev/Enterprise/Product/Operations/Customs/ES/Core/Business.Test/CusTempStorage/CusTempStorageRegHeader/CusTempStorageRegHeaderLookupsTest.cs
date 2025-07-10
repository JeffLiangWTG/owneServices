using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var list = lookups.StatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "OPN, CLS", list.CodesAsString);
				AssertSame("Cached", list, lookups.StatusList);
			});
		}

		public void TestPreviousReferenceTypeList()
		{
			var list = lookups.PreviousReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "G5, LAM, TSM, NCTS5", list.CodesAsString);
				AssertSame("Cached", list, lookups.PreviousReferenceTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_AppCode = "123";
			header.SRH_Status = "OK";
			header.SRH_Reference = "TEST";
			lookups = new CusTempStorageRegHeaderLookups(header);
		}

		CusTempStorageRegHeader header;
		CusTempStorageRegHeaderLookups lookups;
	}
}
