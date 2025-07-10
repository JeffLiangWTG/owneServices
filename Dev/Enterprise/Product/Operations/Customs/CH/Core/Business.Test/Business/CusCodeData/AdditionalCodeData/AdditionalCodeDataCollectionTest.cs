using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalCodeDataCollection))]
class AdditionalCodeDataCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new AdditionalCodeDataCollection(invoiceLine);
	}
}
