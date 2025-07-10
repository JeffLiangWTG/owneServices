using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineDependentCollection))]
sealed class InvoiceLineDependentCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MasterBill = "M";
		declaration.JE_TotalNoOfPieces = 1;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		return new InvoiceLineDependentCollection(invoice);
	}

	protected override Type GetExpectedCollectionType()
	{
		return typeof(InvoiceLineDependentCollection);
	}

	public void TestElementType()
	{
		var collection = (InvoiceLineDependentCollection)GetCollectionToTest();
		var invoiceLine = collection.AddNew();
		AssertType<JobComInvoiceLine>(invoiceLine);
		AssertType<JobComInvoiceLine>(collection[0]);
	}
}
