using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusEntryHeaderValidationTest : EU.Business.Declaration.Testing.CusEntryHeaderValidationTest
	{
		public new void TestParent()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckT2LReceptionEntry()
		{
			var messageErrorText = "Cannot send T2L Reception message without MRN or Issue Date; please enter those fields under the Entries grid";
			var messageErrorTextPOUS2 = "Cannot send T2L Reception message without MRN; please enter those fields under the Entries grid";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is not t2l", entryHeader, messageErrorText);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is not t2l reception (import)", entryHeader, messageErrorText);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is t2l reception (import) without mrn or issue date", entryHeader, messageErrorText);

				entryHeader.MovementReferenceNumber = "Test";
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is t2l reception (import) without issue date", entryHeader, messageErrorText);

				entryHeader.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No Message error because entry is t2l reception (import) with mrn and issue date", entryHeader, messageErrorText);

				entryHeader.ZG_POUSVersion = 2;
				entryHeader.MovementReferenceNumber = "";
				entryHeader.MovementReferenceNumberIssueDate = ZDateTime.Empty;
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is t2l reception (import) without mrn or issue date when POUS2", entryHeader, messageErrorTextPOUS2);

				entryHeader.MovementReferenceNumber = "Test";
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No Message error because entry is t2l reception (import) without issue date when POUS2", entryHeader, messageErrorTextPOUS2);
			});
		}

		public void TestCheckAnnexesInEntry_H1()
		{
			var messageErrorText = "At least one annex should be added when sending a message";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_UCC6Version = 0;
			CombineAssertions(() =>
			{
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is not H1", entryHeader, messageErrorText);

				entryHeader.ZG_UCC6Version = 1;
				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is H1 without annexes and CH_EntryStatus = CDP", entryHeader, messageErrorText);

				entryHeader.CH_EntryStatus = "JPB";
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is H1 without annexes CH_EntryStatus = JPB", entryHeader, messageErrorText);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);

				entryHeader.CH_EntryStatus = EntryStatusCodes.ClearedWithPendingDocuments;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("Message error because entry is H1 with annexes and CH_EntryStatus = CDP", entryHeader, messageErrorText);

				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is H1, has CEI_Style = H2 and annexes are declared", entryHeader, messageErrorText);

				entryInstruction.CEI_Style = ZString.Empty;
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is H1 and is export", entryHeader, messageErrorText);
			});
		}

		public void TestCheckAnnexesInEntry_AES()
		{
			var messageErrorText = "At least one annex should be added when sending a message";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var supDoc = invoiceLine.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "AA";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is not AES", entryHeader, messageErrorText);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryHeader.ZG_UCC6Version = 1;
				entryHeader.MovementReferenceNumber = "MRN-TEST";
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is AES", entryHeader, messageErrorText);

				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("Message error because entry is AES without annexes", entryHeader, messageErrorText);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is AES and annexes are declared", entryHeader, messageErrorText);

				entryHeader.SetCSVClearanceNum("CSV-TEST");
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				Factory.Save();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("No message error because entry is AES and has Clearance Number and no annexes", entryHeader, messageErrorText);
			});
		}

		public void TestCheckAnnexesInEntry_SeeOnlyInT2LNoPous()
		{
			var messageErrorText = "At least one annex should be added when sending a message";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var supDoc = invoiceLine.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "AA";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ZG_POUSVersion = POUSVersionCodes.POUS;
			Factory.Save();

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("POUSVersion: Message error because entry is t2l", entryHeader, messageErrorText);

				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("POUSVersion: Message error because entry is t2l without supporting documents and no annexes", entryHeader, messageErrorText);
			});

			entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;
			Factory.Save();

			CombineAssertions(() =>
			{
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("NOPOUSVersion: No message error because entry is not t2l", entryHeader, messageErrorText);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("NOPOUSVersion: Message error because entry is t2l", entryHeader, messageErrorText);

				supDoc = invoiceLine.SupportingDocuments.AddNew();
				supDoc.CSI_Code = "9010";
				Factory.Save();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("NOPOUSVersion: No message error because entry is t2l export with supporting document 9010", entryHeader, messageErrorText);

				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				entryHeader.EDocPivotCollection.RemoveAndDeleteAll();
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("NOPOUSVersion: Message error because entry is t2l without supporting documents and no annexes", entryHeader, messageErrorText);

				var docPivot = Factory.NewWithValidTestData<CusStorageDocPivot>();
				entryHeader.EDocPivotCollection.Add(docPivot);
				Factory.Save();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("NOPOUSVersion: No message error because entry is t2l and annexes are declared", entryHeader, messageErrorText);
			});
		}

		public void TestCheckCH_CEI_Instruction()
		{
			var messageErrorText = "You have not entered an Entry Instruction, please check Inv. Lines.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var supDoc = invoiceLine.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "AA";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.Validation.ValidateAll();
				AssertHasRowMessageError("There is error when entry has no entry instruction", entryHeader, messageErrorText);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				Factory.Save();
				entryHeader.Validation.ValidateAll();
				AssertNoRowMessageError("There is no error when entry has entry instruction", entryHeader, messageErrorText);
			});
		}

		public void TestCheckMovementReferenceNumber()
		{
			var messageErrorText = "T2L(F) MRN is mandatory";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			Factory.Save();

			CombineAssertions(() =>
			{
				entryHeader.Validation.ValidateAll();
				AssertNoMessageError("No message error because entry is not T2C", entryHeader.MovementReferenceNumberInfo, messageErrorText);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				entryHeader.Validation.ValidateAll();
				AssertHasMessageError("Message error because entry is T2C and MRN is empty", entryHeader.MovementReferenceNumberInfo, messageErrorText);

				entryHeader.MovementReferenceNumber = "Test";
				entryHeader.Validation.ValidateAll();
				AssertNoMessageError("No message error because entry is T2C and MRN is not empty", entryHeader.MovementReferenceNumberInfo, messageErrorText);
			});
		}
	}
}
