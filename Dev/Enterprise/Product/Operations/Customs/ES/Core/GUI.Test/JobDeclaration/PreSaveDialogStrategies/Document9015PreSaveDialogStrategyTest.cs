using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class Document9015PreSaveDialogStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => new Document9015PreSaveDialogStrategy(null));
				AssertNoExceptionThrown(() => new Document9015PreSaveDialogStrategy(Factory.New<JobDeclaration>()));
			});
		}

		public void TestRunPreSaveActionDoesNothing_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			declaration.DoMerge();

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertEquals("When no documents in declaration and conditions are not met for a VAT reduction nothing is done", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("When no documents in invoice header and conditions are not met for a VAT reduction nothing is done", 0, SupDocTestHelper.GetTypeDocumentCount(header.SupportingDocuments, docType));
				AssertEquals("When no documents in invoice line and conditions are not met for a VAT reduction nothing is done", 0, SupDocTestHelper.GetTypeDocumentCount(line.SupportingDocuments, docType));
				AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionDoesNothing_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			declaration.DoMerge();

			CombineAssertions(() =>
			{
				SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
				AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				ShowPreSaveDialog(declaration);
				AssertNull("When there are documents in declaration and conditions are not met for a VAT reduction but declaration is export no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration still has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
			});
		}

		public void TestRunPreSaveActionShouldNotAsk()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = "DIR";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "AA";
			declaration.Declarant.OA_OH = declarant.PK;
			var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction.PK, "Guarantee"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.ZG_MethodOfPayment = "R";

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertNull("When no documents in declaration/invoice header/invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
				AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				ShowPreSaveDialog(declaration);
				AssertNull("When there are documents in declaration and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				ShowPreSaveDialog(declaration);
				AssertNull("When there are documents in invoice header and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				invoice.SupportingDocuments.RemoveAndDeleteAll();
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
				ShowPreSaveDialog(declaration);
				AssertNull("When there are documents in invoice line and conditions are met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRunPreSaveActionShouldAskAnswerNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = entryNumber;

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
				AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
				invoice.SupportingDocuments.RemoveAndDeleteAll();
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line 2 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice line 2 and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is NO, the documents are not removed", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
			});
		}

		public void TestRunPreSaveActionShouldAskAnswerYes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = entryNumber;

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
				AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice line and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents", GetPopUpMessage(entryNumber), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("When dialog answer is YES, the documents are removed", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
			});
		}

		public void TestRunPreSaveActionShouldAsk_MultipleEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = "DIR";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.C;

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "AA";
			declaration.Declarant.OA_OH = declarant.PK;
			var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, "Guarantee"), (entryInstruction2.PK, "Guarantee"), (entryInstruction3.PK, "Guarantee"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.ZG_MethodOfPayment = "A";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.ZG_MethodOfPayment = "S";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction3.PK;
			invoiceLine3.ZG_MethodOfPayment = "R";

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
			var entryLine3 = entryHeader3.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;

			Factory.Save();

			var firstEntryNumber = entryHeader1.CH_BGMReference;
			var secondEntryNumber = entryHeader2.CH_BGMReference;
			var thirdEntryNumber = entryHeader3.CH_BGMReference;

			CombineAssertions(() =>
			{
				ShowPreSaveDialog(declaration);
				AssertNull("When no documents in declaration/invoice header/invoice line and conditions are not met for a VAT reduction no pop up is shown", UnitTestUserNotification.Instance.LastMessage.Text);

				SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
				AssertEquals("Declaration has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in declaration and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents for the first entry", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(firstEntryNumber)));
				AssertEquals("When there are no documents in declaration (has been deleted when accepted first pop up) and conditions are not met for a VAT reduction  no pop up is shown for the second entry", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(secondEntryNumber)));
				AssertEquals("When there are no documents in declaration (has been deleted when accepted first pop up) and conditions are met for a VAT reduction no pop up is shown for the third entry", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(thirdEntryNumber)));
				AssertEquals("When dialog answer is YES, the documents are removed from declaration", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice header and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents for the first entry", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(firstEntryNumber)));
				AssertEquals("When there are no documents in invoice header (has been deleted when accepted first pop up) and conditions are not met for a VAT reduction no pop up is shown for the second entry", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(secondEntryNumber)));
				AssertEquals("When there are no documents in invoice header (has been deleted when accepted first pop up) and conditions are met for a VAT reduction no pop up is shown for the third entry", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(thirdEntryNumber)));
				AssertEquals("When dialog answer is YES, the documents are removed from invoice header", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));

				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine1, docType);
				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine2, docType);
				SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine3, docType);
				AssertEquals("Declaration has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting documents", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line 1 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
				AssertEquals("Invoice line 2 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
				AssertEquals("Invoice line 3 has supporting document", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine3.SupportingDocuments, docType));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ShowPreSaveDialog(declaration);
				AssertEquals("When there are documents in invoice line 1 and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents for the first entry", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(firstEntryNumber)));
				AssertEquals("When there are documents in invoice line 2 and conditions are not met for a VAT reduction a pop up is shown to ask if we want to remove the existing documents for the second entry", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(secondEntryNumber)));
				AssertEquals("When there are documents in invoice line 3 and conditions are met for a VAT reduction no pop up is shown for the third entry", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(GetPopUpMessage(thirdEntryNumber)));
				AssertEquals("When dialog answer is YES, the documents are removed from invoice line 1", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
				AssertEquals("When dialog answer is YES, the documents are removed from invoice line 2", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
				AssertEquals("Since there is no pop up nothing is done to the docuemtn in invoice line 3", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine3.SupportingDocuments, docType));
			});
		}

		void ShowPreSaveDialog(JobDeclaration declaration) => new Document9015PreSaveDialogStrategy(declaration).ShowPreSaveDialogs(ContinueWithSave.Yes);

		string GetPopUpMessage(string entryNumber) => "For Entry " + entryNumber + " there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

		readonly string docType = SupportingDocumentType.VATReductionCode;

		readonly string entryNumber = "ES00001";
	}
}
