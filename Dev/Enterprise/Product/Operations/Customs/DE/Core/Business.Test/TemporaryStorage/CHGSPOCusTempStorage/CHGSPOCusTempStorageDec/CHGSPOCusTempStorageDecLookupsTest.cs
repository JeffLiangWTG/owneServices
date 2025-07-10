using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CHGSPOCusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIdentificationIndicatorList()
		{
			var storageDec = Factory.New<CHGSPOCusTempStorageDec>();
			var indicatorList = storageDec.Lookups.IdentificationIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.New<CHGSPOCusTempStorageDec>().Lookups.IdentificationIndicatorList, indicatorList);
				AssertEquals("Codes", "REG", indicatorList.CodesAsString);
			});
		}
	}
}
