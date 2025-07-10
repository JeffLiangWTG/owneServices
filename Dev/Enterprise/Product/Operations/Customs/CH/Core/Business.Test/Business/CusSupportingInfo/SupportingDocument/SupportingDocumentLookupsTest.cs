using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PreviousDocumentLookups))]
sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateSupportingDocumentCodes(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var headerSupportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		var lineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		var validExportCodes = new[] { RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateN, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateY, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceN, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceY, RefCusCodeTestHelper.ValidExportSupportingDocumentCode };
		AssertCodeList(headerSupportingDocument, validExportCodes, "Export InvoiceHeader");
		AssertCodeList(lineSupportingDocument, validExportCodes, "Export InvoiceLine");

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		AssertCodeList(headerSupportingDocument, new[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCode }, "Import InvoiceHeader");
		AssertCodeList(lineSupportingDocument, new[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCode }, "Import InvoiceLine");

		var effAssessmentDate = new ZDateTime(2023, 12, 31);
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = effAssessmentDate;
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertCodeList(headerSupportingDocument, new[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCode }, "Import InvoiceHeader Past");
		AssertCodeList(lineSupportingDocument, new[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCodePast }, "Import InvoiceLine Past");
	});

	void AssertCodeList(SupportingDocument supportingDocument, IEnumerable<string> expectedCodes, string assertionMessage)
	{
		var collection = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
		collection.Load();
		AssertContainsExactElementsInAnyOrder(assertionMessage, expectedCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(c => c.ZZD_Code));
	}

	public void TestValidOriginDocumentCodes_Import()
	{
		RefCusCodeTestHelper.CreateOriginDocumentCodes(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

		string[] expectedCodes = ["3", "861", "862", "865", "866", "872", "873", "874", "954", "964"];
		string[] expectedCodesImpPast = ["861", "862", "865", "866", "872", "873", "874", "954", "964"];

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("IMP Valid Origin Document Codes", expectedCodes, supportingDocument.Lookups.ValidOriginDocumentCodes.GetAllCodes());

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("IMP Valid Origin Document Codes - Past", expectedCodesImpPast, supportingDocument.Lookups.ValidOriginDocumentCodes.GetAllCodes());
		});
	}
}
