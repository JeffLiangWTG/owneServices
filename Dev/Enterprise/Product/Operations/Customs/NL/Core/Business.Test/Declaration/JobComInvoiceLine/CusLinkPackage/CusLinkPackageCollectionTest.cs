using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusLinkPackageCollection))]
class CusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusLinkPackageCollection>
{
	public void TestAllowNew()
	{
		AssertEquals(false, GetCollectionToTest().AllowNew);
	}

	protected override CusLinkPackageCollection GetCollectionToTest()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new CusLinkPackageCollection(invoiceLine);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new CusLinkPackage(invoiceLine);
	}
}
