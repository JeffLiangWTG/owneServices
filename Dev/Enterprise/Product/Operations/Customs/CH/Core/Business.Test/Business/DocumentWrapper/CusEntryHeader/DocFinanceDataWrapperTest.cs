using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocFinanceDataWrapper))]
sealed class DocFinanceDataWrapperTest : DocumentWrapperTestCase
{
	public void TestFinanceData()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceHeader.JZ_IncoTerm = "FOB";
		var supplier = Factory.New<OrgHeader>();
		jobDeclaration.JE_OH_Supplier = supplier.PK;
		supplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "1000088059", "CH");
		supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "CHE-123.456.788", "CH");

		var docFinanceDataWrapper = DocFinanceDataWrapper.New(invoiceHeader, Factory);

		CombineAssertions(() =>
		{
			AssertNotNull(docFinanceDataWrapper);
			AssertType<DocFinanceDataWrapper>(docFinanceDataWrapper);
			AssertEquals("Incoterms should match", "FOB", docFinanceDataWrapper.Incoterms);
			AssertEquals("VatNumber should match", "CHE-123.456.788", docFinanceDataWrapper.VatNumber);
			AssertEquals("CustomsInvoiceRecipient should match", "1000088059", docFinanceDataWrapper.CustomsInvoiceRecipient);
		});
	}

	public override DocumentWrapper[] GetDocumentWrappers()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		return new DocumentWrapper[] { DocFinanceDataWrapper.New(invoiceHeader, Factory) };
	}
}
