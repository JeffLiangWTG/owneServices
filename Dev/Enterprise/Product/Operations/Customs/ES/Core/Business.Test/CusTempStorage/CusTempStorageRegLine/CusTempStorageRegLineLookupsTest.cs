using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public class CusTempStorageRegLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var list = lookups.CustomsStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "OPN, CLS", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsStatusList);
			});
		}

		public void TestUnionStatusList()
		{
			var unionStatusList = lookups.UnionStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("List values", "COM, NAT, TER", unionStatusList.CodesAsString);
				AssertSame("Cached", unionStatusList, lookups.UnionStatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_LineNumber = 1;
			lookups = new CusTempStorageRegLineLookups(line);
		}
		CusTempStorageRegHeader header;
		CusTempStorageRegLineLookups lookups;
	}
}
