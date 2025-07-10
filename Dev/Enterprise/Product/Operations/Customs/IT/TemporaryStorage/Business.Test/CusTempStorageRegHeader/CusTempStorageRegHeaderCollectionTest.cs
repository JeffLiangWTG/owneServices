using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderCollection))]
sealed public class CusTempStorageRegHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegHeaderCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var testHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		Factory.Save();
		return testHeader;
	}

	protected override CusTempStorageRegHeaderCollection GetCollectionToTest() => new CusTempStorageRegHeaderCollection(Factory);
}
