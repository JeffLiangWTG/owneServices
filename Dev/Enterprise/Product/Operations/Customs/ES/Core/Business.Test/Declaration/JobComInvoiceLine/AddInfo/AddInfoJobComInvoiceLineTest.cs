using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

[TestedType(typeof(AddInfoJobComInvoiceLine))]
class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
	}

	public void TestZG_RegionOfDestination_MaxLegnth()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertEquals(3, invoiceLine.ZG_RegionOfDestinationInfo.MaxLength);
	}
}
