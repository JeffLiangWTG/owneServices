using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDecider()
		{
			AssertType<CusGoodsLocationTypeDecider>(CusGoodsLocation.TypeDecider);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var location = factory.New<CusGoodsLocation>();
			location.Parent = factory.NewWithValidTestData<TemporaryStorageHeader>();
			location.CGL_LocationUse = "DEP";
			return location;
		}
	}
}
