using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizationsFindBoxList()
		{
			var storageLine = Factory.New<CusTempStorageLine>();
			AssertNotNull(storageLine.Lookups.OrganizationsFindBoxList);
			AssertEquals(typeof(OrganisationsFindBoxCollection), storageLine.Lookups.OrganizationsFindBoxList.GetType());
		}
	}
}
