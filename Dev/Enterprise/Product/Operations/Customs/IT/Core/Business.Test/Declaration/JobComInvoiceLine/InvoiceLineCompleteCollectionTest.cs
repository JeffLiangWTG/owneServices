using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceLineCompleteCollection))]
sealed class InvoiceLineCompleteCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MasterBill = "M";
		declaration.JE_TotalNoOfPieces = 1;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		return new InvoiceLineCompleteCollection(declaration);
	}

	protected override Type GetExpectedCollectionType()
	{
		return typeof(InvoiceLineCompleteCollection);
	}
}
