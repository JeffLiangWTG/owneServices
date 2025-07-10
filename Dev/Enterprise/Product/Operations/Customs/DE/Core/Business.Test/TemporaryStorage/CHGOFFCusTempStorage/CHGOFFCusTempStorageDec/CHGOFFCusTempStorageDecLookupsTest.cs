using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CHGOFFCusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIdentificationIndicatorList()
		{
			var storageDec = Factory.New<CHGOFFCusTempStorageDec>();
			var indicatorList = storageDec.Lookups.IdentificationIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", storageDec.GetIdentificationIndicatorListExcludingSIN(), indicatorList);
				AssertEquals("Codes", "AWB, REG", indicatorList.CodesAsString);
			});
		}
	}
}
