using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderLookups))]
sealed class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCustomsOffficeList()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		AssertSame(header.Lookups.CustomsOfficeList.Factory, header.Lookups.CustomsOfficeList.Factory);
	}
}
