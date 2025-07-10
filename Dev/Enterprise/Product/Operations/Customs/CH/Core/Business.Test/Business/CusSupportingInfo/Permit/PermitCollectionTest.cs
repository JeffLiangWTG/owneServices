using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PermitCollection))]
class PermitCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Factory.New<JobComInvoiceLine>().Permits;
	}

	public void TestMaxCount()
	{
		var collection = GetCollectionToTest();
		AssertEquals(9, collection.MaxCount);
	}
}
