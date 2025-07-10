using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class REXDISCusTempStorageDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIdentificationIndicatorList()
		{
			var storageDec = Factory.New<REXDISCusTempStorageDec>();
			var indicatorList = storageDec.Lookups.IdentificationIndicatorList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", storageDec.GetIdentificationIndicatorListIncludingSIN(), indicatorList);
				AssertEquals("Codes", "AWB, REG, SIN", indicatorList.CodesAsString);
			});
		}

		public void TestProcedureTypeList()
		{
			var storageDec = Factory.New<REXDISCusTempStorageDec>();
			var procedureTypeList = storageDec.Lookups.ProcedureTypeList;
			CombineAssertions(() =>
			{
				AssertSame("Cached", Factory.GetCachedValue<TemporaryStorageProcedureTypeList>(), procedureTypeList);
				AssertEquals("Codes", "1, 2, 4, 6, 7, 8", procedureTypeList.CodesAsString);
			});
		}
	}
}
