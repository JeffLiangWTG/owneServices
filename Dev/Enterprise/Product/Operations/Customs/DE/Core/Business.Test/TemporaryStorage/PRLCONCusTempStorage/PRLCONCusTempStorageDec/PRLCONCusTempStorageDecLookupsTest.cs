using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class PRLCONCusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIdentificationIndicatorList()
		{
			var storageDec = Factory.New<PRLCONCusTempStorageDec>();
			var indicatorList = storageDec.Lookups.IdentificationIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", storageDec.GetIdentificationIndicatorListExcludingSIN(), indicatorList);
				AssertEquals("Codes", "AWB, REG", indicatorList.CodesAsString);
			});
		}
	}
}
