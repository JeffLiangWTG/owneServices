using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocument))]
sealed class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
{
	public void TestValidationType()
	{
		AssertType<PreviousDocumentValidation>(preDoc.Validation);
	}

	protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.PreviousDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.PreviousDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.PreviousDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => preDoc;

	protected override void SetUp()
	{
		base.SetUp();
		var decl = Factory.New<JobDeclaration>();
		var invoiceHeader = decl.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		preDoc = invoiceLine.PreviousDocuments.AddNew();
	}
	PreviousDocument preDoc;
}
