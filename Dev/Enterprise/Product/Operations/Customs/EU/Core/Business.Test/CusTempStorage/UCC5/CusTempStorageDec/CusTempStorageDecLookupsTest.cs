using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclarationStatusList()
		{
			var storageDec = Factory.New<CusTempStorageDec>();
			AssertEquals("OPN, CLS", storageDec.Lookups.DeclarationStatusList.CodesAsString);
		}
	}
}
