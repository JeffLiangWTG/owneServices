using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InvoiceLinePackagePivotCollection))]
class InvoiceLinePackagePivotCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		return new InvoiceLinePackagePivotCollection(invoiceLine);
	}

	public void TestGetTypeOfElementsFromPK()
	{
		var invoiceLinePackagePivotCollection = Factory.New<JobComInvoiceLine>().PackagesPivot as InvoiceLinePackagePivotCollection;
		var invoiceLinePackagePivot = invoiceLinePackagePivotCollection.AddNew();
		AssertEquals("Expected type", typeof(InvoiceLinePackagePivot), invoiceLinePackagePivotCollection.GetTypeOfElementsFromPK(invoiceLinePackagePivot.PK));
	}
}
