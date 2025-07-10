using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageJobHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusTempStorageJobHeaderLookups()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			AssertType(typeof(CusTempStorageJobHeaderLookups), header.Lookups);
			AssertEquals(8, header.Lookups.TransportModeList.Count);
		}
	}
}
