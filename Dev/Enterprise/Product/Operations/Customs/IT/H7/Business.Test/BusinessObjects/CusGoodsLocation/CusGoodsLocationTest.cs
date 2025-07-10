using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed public class CusGoodsLocationTest : EU.H7.Business.Testing.CusGoodsLocationTest
{
	public void TestAuthorization()
	{
		SetUpTests();
		goodsLocation.CGL_Authorization = "authorization";

		AssertEquals("authorization", goodsLocation.CGL_Authorization);
	}

	void SetUpTests()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		goodsLocation = bill.CusGoodsLocation;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateCusGoodsLocation();

	BusinessObject CreateCusGoodsLocation()
	{
		SetUpTests();
		return goodsLocation;
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
	CusGoodsLocation goodsLocation;
}
