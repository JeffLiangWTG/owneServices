using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookupsType()
	{
		var address = GetCusGoodsLocationAddressForTesting(Factory);
		AssertType<CusGoodsLocationAddressLookups>($"{typeof(CusGoodsLocationAddress)}.Lookups should be of type {typeof(CusGoodsLocationAddressLookups).FullName}.", address.Lookups);
	}

	protected override BusinessObject GetNewBusinessObject() => GetCusGoodsLocationAddressForTesting(Factory);

	static CusGoodsLocationAddress GetCusGoodsLocationAddressForTesting(BusinessObjectFactory factory)
	{
		var location = CusGoodsLocationTest.GetCusGoodsLocationForTesting(factory);
		var address = location.Address;
		return address;
	}
}
