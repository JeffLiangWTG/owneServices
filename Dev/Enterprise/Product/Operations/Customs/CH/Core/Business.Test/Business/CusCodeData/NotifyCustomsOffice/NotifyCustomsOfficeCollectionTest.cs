using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NotifyCustomsOfficeCollection))]
class NotifyCustomsOfficeCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCount() => AssertEquals(9, GetCollectionToTest().MaxCount);

	protected override BusinessObjectCollection GetCollectionToTest() => Factory.New<JobComInvoiceLine>().NotifyCustomsOffices;
}
