using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNLOCOs()
		{
			(var arrivalGoodsLocation, _, _) = CusGoodsLocationTest.GetNewBusinessObject(Factory);
			AssertType<RefUNLOCOCollection>(arrivalGoodsLocation.Lookups.UNLOCOs);
		}

		public void TestTypeOfLocationList()
		{
			(var arrivalGoodsLocation, _, _) = CusGoodsLocationTest.GetNewBusinessObject(Factory);
			AssertSame(Factory.GetCachedValue<CusGoodsLocationTypeList>(), arrivalGoodsLocation.Lookups.TypeOfLocationList);
		}
	}
}
