using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DocumentManager9015Test : TestCaseWithFactory
	{
		public void TestSupportingDocument_ConditionsMet()
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

			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);

			var docManager = new DocumentManager9015(declaration, GetMessageBoxProvider(true));

			CombineAssertions(() =>
			{
				AssertEquals("Declaration has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));

				docManager.Remove9015DocumentsIfNeeded();
				AssertEquals("Declaration has supporting document after calling manager because conditions are met", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document after calling manager because conditions are met", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document after calling manager because conditions are met", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
			});
		}

		public void TestSupportingDocument_ConditionsNotMet()
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

			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine, docType);

			var docManager = new DocumentManager9015(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				AssertEquals("Declaration has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));

				docManager.Remove9015DocumentsIfNeeded();
				AssertEquals("Declaration has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));

				docManager = new DocumentManager9015(declaration, GetMessageBoxProvider(true));
				docManager.Remove9015DocumentsIfNeeded();
				AssertEquals("Declaration has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine.SupportingDocuments, docType));
			});
		}

		public void TestSupportingDocument_MultipleEntries()
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

			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "AA";
			declaration.Declarant.OA_OH = declarant.PK;
			var authorisation = SupDocTestHelper.AddAuthorisationWithHolder(Factory, declarant.PK, ESCusAuthorisationHeaderTypeList.Codes.AuthorizedEconomicOperatorCustomsSimplifications);

			GuaranteesTestHelper.CreateGuaranteesHeaderDetail(Factory, declarant.PK, "Guarantee", false);
			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction1.PK, "Guarantee"), (entryInstruction2.PK, "Guarantee"));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.ZG_MethodOfPayment = "R";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.ZG_MethodOfPayment = "A";

			declaration.DoMerge();

			SupDocTestHelper.AddSuportingDocumentToDeclaration(declaration, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceHeader(invoice, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine1, docType);
			SupDocTestHelper.AddSuportingDocumentToInvoiceLine(invoiceLine2, docType);

			var docManager = new DocumentManager9015(declaration, GetMessageBoxProvider(false));

			CombineAssertions(() =>
			{
				AssertEquals("Declaration has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line 1 has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
				AssertEquals("Invoice line 2 has supporting document before calling manager", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));

				docManager.Remove9015DocumentsIfNeeded();
				AssertEquals("Declaration has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line 1 has supporting document after calling manager because conditions are met", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
				AssertEquals("Invoice line 2 has supporting document after calling manager because conditions are not met but MessageBoxProvider returns false", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));

				docManager = new DocumentManager9015(declaration, GetMessageBoxProvider(true));
				docManager.Remove9015DocumentsIfNeeded();
				AssertEquals("Declaration has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(declaration.SupportingDocuments, docType));
				AssertEquals("Invoice header has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(invoice.SupportingDocuments, docType));
				AssertEquals("Invoice line 1 has supporting document after calling manager because conditions are met", 1, SupDocTestHelper.GetTypeDocumentCount(invoiceLine1.SupportingDocuments, docType));
				AssertEquals("Invoice line 2 has no supporting document after calling manager because conditions are not met and MessageBoxProvider returns true", 0, SupDocTestHelper.GetTypeDocumentCount(invoiceLine2.SupportingDocuments, docType));
			});
		}

		IDocument9015MessageBoxProvider GetMessageBoxProvider(bool response)
		{
			var mockProvider = new Mock<IDocument9015MessageBoxProvider>();
			mockProvider.Setup(m => m.AskIfShouldRemove9015Documents(It.IsAny<ZString>())).Returns(response);
			return mockProvider.Object;
		}

		readonly string docType = SupportingDocumentType.VATReductionCode;
	}
}
