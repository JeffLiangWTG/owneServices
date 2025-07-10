using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecGoodsitemProducedDocumentDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("Argument == null", EdecGoodsitemProducedDocumentDataProvider.New(null));
			AssertNotNull("Argument != null", EdecGoodsitemProducedDocumentDataProvider.New(supportingDocument));
		});
	}

	public void TestProvider()
	{
		string code = "ABCD";
		string referenceNumber = "1234";
		ZDate dateOfIssue = new ZDate(2022, 1, 1);
		string referenceNumber2 = "456";

		supportingDocument.CSI_Code = code;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		supportingDocument.CSI_DateOfIssue = dateOfIssue;
		supportingDocument.CSI_ReferenceNumber2 = referenceNumber2;

		var dataProvider = EdecGoodsitemProducedDocumentDataProvider.New(supportingDocument);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.DocumentType), code, dataProvider.DocumentType);
			AssertEquals(nameof(dataProvider.DocumentReferenceNumber), referenceNumber, dataProvider.DocumentReferenceNumber);
			AssertEquals(nameof(dataProvider.IssueDate), dateOfIssue, dataProvider.IssueDate);
			AssertEquals(nameof(dataProvider.AdditionalInformation), referenceNumber2, dataProvider.AdditionalInformation);
		});
	}

	public void TestNewCollection()
	{
		string code = "ABCD";
		string referenceNumber = "1234";
		ZDate dateOfIssue = new ZDate(2022, 1, 1);
		string referenceNumber2 = "456";

		SetSupportingDocument(supportingDocument, code, referenceNumber, dateOfIssue, referenceNumber2);
		SetSupportingDocument(invoice.SupportingDocuments.AddNew(), code, referenceNumber, dateOfIssue, referenceNumber2);
		SetSupportingDocument(invoiceLine.SupportingDocuments.AddNew(), code, referenceNumber, dateOfIssue, referenceNumber2);
		SetSupportingDocument(invoice.SupportingDocuments.AddNew(), "EFGH", referenceNumber, dateOfIssue, referenceNumber2);
		SetSupportingDocument(invoiceLine.SupportingDocuments.AddNew(), "IJKL", referenceNumber, dateOfIssue, referenceNumber2);

		var dataProviders = EdecGoodsitemProducedDocumentDataProvider.NewCollection(entryLine).OrderBy(x => x.DocumentType).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("DocumentType", code, dataProviders[0].DocumentType);
			AssertEquals("DocumentReferenceNumber", referenceNumber, dataProviders[0].DocumentReferenceNumber);
			AssertEquals("IssueDate", dateOfIssue, dataProviders[0].IssueDate);
			AssertEquals("AdditionalInformation", referenceNumber2, dataProviders[0].AdditionalInformation);

			AssertEquals("DocumentType", "EFGH", dataProviders[1].DocumentType);
			AssertEquals("DocumentType", "IJKL", dataProviders[2].DocumentType);
		});
	}

	void SetSupportingDocument(SupportingDocument supportingDocument, string code, string referenceNumber, ZDate dateOfIssue, string referenceNumber2)
	{
		supportingDocument.CSI_Code = code;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		supportingDocument.CSI_DateOfIssue = dateOfIssue;
		supportingDocument.CSI_ReferenceNumber2 = referenceNumber2;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
		supportingDocument = invoiceLine.SupportingDocuments.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	SupportingDocument supportingDocument;
}

