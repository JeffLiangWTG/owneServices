using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(JobDeclarationDocumentSupporter))]
public class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
{
	public override void TestGetDocBusinessObjects()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
		DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
		AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", "Enterprise.Customs.AE.Business.DocDeclaration", result[0].GetType().ToString());
		declaration.CustomsEntryHeaders.AddNew();
		result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, null);
		AssertEquals("Document wrapper for data context of CusEntryHeader is of type DocDeclaration", "Enterprise.Customs.AE.Business.DocCusEntryHeader", result[0].GetType().ToString());
		declaration.Invoices.AddNew();
		result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ComInvoiceHeader, null);
		AssertEquals("Document wrapper for data context of ComInvoiceHeader is of type DocJobComInvoiceHeader", "Enterprise.Customs.AE.Business.DocJobComInvoiceHeader", result[0].GetType().ToString());
	}

	public override void TestGetFilterValueEXPBKRLIC()
	{
		Assert("This test should never be run for AE because AE do not do Export jobs - all of their jobs are performed as imports, and thus there is no need to check for a Export Broker Licence.", true);
	}
}
