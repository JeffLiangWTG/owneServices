using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class SupportingDocumentValidationTest : TestCaseWithFactory
	{
		public void TestCheckCSI_Procedure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Procedure = "~";
			AssertHasMessageError(supportingDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

			supportingDocument.CSI_Procedure = SupportingDocumentProcedureCodeList.Codes.A;
			AssertNoMessageError(supportingDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateSupportingDocumentNotPersistedForC44AndH1()
		{
			var expectedWarning = "For C44 declaration, NEW Supporting Documents added to Entry Instruction or Misc. will not be sent.";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var declarationDoc = GetSupportingDoc(1, "Ref1");
			declaration.SupportingDocuments.Add(declarationDoc);
			var entryInstructionDoc = GetSupportingDoc(1, "Ref1");
			entryInstruction.SupportingDocuments.Add(entryInstructionDoc);
			var invoiceHeaderDoc = GetSupportingDoc(1, "Ref1");
			invoiceHeader.SupportingDocuments.Add(invoiceHeaderDoc);
			var invoiceLineDoc = GetSupportingDoc(1, "Ref1");
			invoiceLine.SupportingDocuments.Add(invoiceLineDoc);

			var testCases = new[]
			{
				(entrySubStyle: EntrySubStyleList.Codes.Z, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: false, assertMessageText: "If EntryStatus CDP, UCC6 Version and does not exists in EntryHeader but EntryInstruction not A or B or C"),
				(entrySubStyle: EntrySubStyleList.Codes.A, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: true, assertMessageText: "If EntryInstruction A, EntryStatus CDP, UCC6 Version and does not exists in EntryHeader"),
				(entrySubStyle: EntrySubStyleList.Codes.C, entryStatus: EntryStatusCodes.Cleared, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: false, assertMessageText: "If EntryInstruction A or B or C, UCC6 Version and does not exists in EntryHeader but EntryStatus not CDP"),
				(entrySubStyle: EntrySubStyleList.Codes.B, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: true, assertMessageText: "If EntryInstruction B, EntryStatus CDP, UCC6 Version and does not exists in EntryHeader"),
				(entrySubStyle: EntrySubStyleList.Codes.B, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.NoUCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: false, assertMessageText: "EntryInstruction A or B or C, EntryStatus CDP and does not exists in EntryHeader but no UCC6 Version"),
				(entrySubStyle: EntrySubStyleList.Codes.C, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: false, shouldHaveWarning: true, assertMessageText: "If EntryInstruction C, EntryStatus CDP, UCC6 Version and does not exists in EntryHeader"),
				(entrySubStyle: EntrySubStyleList.Codes.B, entryStatus: EntryStatusCodes.ClearedWithPendingDocuments, ucc6Version: UCC6VersionCodes.UCC6, addSupporingDocumentToEntryHeader: true, shouldHaveWarning: false, assertMessageText: "If EntryInstruction A or B or C, EntryStatus CDP, UCC6 Version and exists in EntryHeader"),
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					entryInstruction.CEI_SubStyle = testCase.entrySubStyle;
					entryHeader.CH_EntryStatus = testCase.entryStatus;
					entryHeader.ZG_UCC6Version = testCase.ucc6Version;
					if (testCase.addSupporingDocumentToEntryHeader)
					{
						GetSupportingDoc(1, "Ref1", entryHeader);
					}
					AssertWarningsInDocuments(testCase.assertMessageText, testCase.shouldHaveWarning);
				}
			});

			void AssertWarningsInDocuments(ZString assertMessageText, bool shouldHaveWarning)
			{
				invoiceHeaderDoc.Validation.ValidateAll();
				AssertNoRowWarningContaining(assertMessageText + " InvoiceHeader Document: There should never be a warning", invoiceHeaderDoc, expectedWarning);

				invoiceLineDoc.Validation.ValidateAll();
				AssertNoRowWarningContaining(assertMessageText + " InvoiceLine Document: There should never be a warning", invoiceLineDoc, expectedWarning);

				entryInstructionDoc.Validation.ValidateAll();
				if (shouldHaveWarning)
				{
					AssertHasRowWarningContaining(assertMessageText + " Declaration Document should have a warning", entryInstructionDoc, expectedWarning);
				}
				else
				{
					AssertNoRowWarningContaining(assertMessageText + " Declaration Document should not have a warning", entryInstructionDoc, expectedWarning);
				}

				declarationDoc.Validation.ValidateAll();
				if (shouldHaveWarning)
				{
					AssertHasRowWarningContaining(assertMessageText + "EntryInstruction Document should have a warning", declarationDoc, expectedWarning);
				}
				else
				{
					AssertNoRowWarningContaining(assertMessageText + "EntryInstruction Document should not have a warning", declarationDoc, expectedWarning);
				}
			}
		}

		SupportingDocument GetSupportingDoc(int i, ZString refNumber, CusEntryHeader entryHeader = null)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_ItemNumber = i;

			if (entryHeader != null)
			{
				supDoc.CSI_ParentID = entryHeader.PK;
				supDoc.CSI_ParentTableCode = entryHeader.TablePrefix;
			}

			return supDoc;
		}
	}
}
