using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PreviousDocumentLookups))]
sealed class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListImport() => CombineAssertions(() =>
	{
		AssertCodeList(Common.Shared.SharedJobMessageTypeList.Codes.Import, RefCusCodeTestHelper.ValidPreviousDocumentsListImport, RefCusCodeTestHelper.InvalidPreviousDocumentsListImport);
		AssertCodeList(Common.Shared.SharedJobMessageTypeList.Codes.Import, RefCusCodeTestHelper.ValidPreviousDocumentsListImportPast, RefCusCodeTestHelper.ValidPreviousDocumentsListImport, new ZDateTime(2023, 12, 31));
	});

	public void TestCodeListExport() => CombineAssertions(() =>
	{
		AssertCodeList(Common.Shared.SharedJobMessageTypeList.Codes.Export, RefCusCodeTestHelper.ValidPreviousDocumentsListExport, RefCusCodeTestHelper.InvalidPreviousDocumentsListExport);
		AssertCodeList(Common.Shared.SharedJobMessageTypeList.Codes.Export, RefCusCodeTestHelper.ValidPreviousDocumentsListExport, RefCusCodeTestHelper.ValidPreviousDocumentsListExportPast, new ZDateTime(2023, 12, 31));
	});

	void AssertCodeList(string messageType, string validPermitAuthorityCode, string invalidPermitAuthorityCode, ZDateTime? assessmentDate = null)
	{
		var effAssessmentDate = assessmentDate ?? ZDateTime.Today;

		RefCusCodeTestHelper.CreatePreviousDocumentsList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var linePreviousDocument = invoiceLine.PreviousDocuments.AddNew();

		if (effAssessmentDate != ZDateTime.Today)
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = effAssessmentDate;
			invoiceLine.JI_CEI = entryInstruction.PK;
		}

		var list = (ZZRefCusCodeListCombinedCollection)linePreviousDocument.Lookups.CodeList;
		list.Load();

		AssertEquals("Valid code", true, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == validPermitAuthorityCode));
		AssertEquals("Invalid code", false, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == invalidPermitAuthorityCode));
	}
}
