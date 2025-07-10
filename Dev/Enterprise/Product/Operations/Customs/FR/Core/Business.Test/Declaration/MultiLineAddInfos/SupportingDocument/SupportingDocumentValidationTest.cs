using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var message = "Maximum length of this field has been exceeded. Only the first 35 characters will be sent in message to Customs.";
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = new string('A', 35);
			AssertNoWarning("No warning expected when CSI_ReferenceNumber doesn't exceed 35 chars.", supportingDocument.CSI_ReferenceNumberInfo, message);
			supportingDocument.CSI_ReferenceNumber = new string('A', 36);
			AssertHasWarning("A warning expected when CSI_ReferenceNumber exceeds 35 chars.", supportingDocument.CSI_ReferenceNumberInfo, message);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			supportingDocument.Validation.ValidateAll();
			AssertNoWarning("No warning expected for DeltaIE.", supportingDocument.CSI_ReferenceNumberInfo, message);
		}

		public void TestCheckCSI_AdditionalDescription()
		{
			var message = "Maximum length of this field has been exceeded. Only the first 35 characters will be sent in message to Customs.";
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_AdditionalDescription = new string('A', 35);
			AssertNoWarning("No warning expected when CSI_AdditionalDescription doesn't exceed 35 chars.", supportingDocument.CSI_AdditionalDescriptionInfo, message);
			supportingDocument.CSI_AdditionalDescription = new string('A', 36);
			AssertHasWarning("A warning expected when CSI_AdditionalDescription exceeds 35 chars.", supportingDocument.CSI_AdditionalDescriptionInfo, message);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			supportingDocument.Validation.ValidateAll();
			AssertNoWarning("No warning expected for DeltaIE.", supportingDocument.CSI_AdditionalDescriptionInfo, message);
		}

		public void TestCheckCSI_Description()
		{
			var message = "Maximum length of this field has been exceeded. Only the first 260 characters will be sent in message to Customs.";
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Description = new string('A', 260);
			AssertNoWarning("No warning expected when CSI_ReferenceNumber doesn't exceed 260 chars.", supportingDocument.CSI_DescriptionInfo, message);
			supportingDocument.CSI_Description = new string('A', 261);
			AssertHasWarning("A warning expected when CSI_ReferenceNumber exceeds 260 chars.", supportingDocument.CSI_DescriptionInfo, message);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			supportingDocument.Validation.ValidateAll();
			AssertNoWarning("No warning expected for DeltaIE.", supportingDocument.CSI_DescriptionInfo, message);
		}

		public void TestCheckUnitOfQuantity()
		{
			SupportingDocumentTest.SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = string.Empty;
				supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

				supportingDocument.CSI_Code = "CODE1";
				supportingDocument.CSI_UnitOfQuantity = "LTR";
				supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "The code you have selected is not in the list.");
				supportingDocument.CSI_UnitOfQuantity = "KGM";
				AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);

				supportingDocument.CSI_Code = "CODE2";
				supportingDocument.CSI_UnitOfQuantity = "LTR";
				supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);
				supportingDocument.CSI_UnitOfQuantity = "KGM";
				supportingDocument.Validation.ValidateCSI_UnitOfQuantity();
				AssertHasMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "The code you have selected is not in the list.");

				supportingDocument.CSI_Code = "CODE3";
				supportingDocument.CSI_UnitOfQuantity = "XXX";
				AssertNoMessageErrors(supportingDocument.CSI_UnitOfQuantityInfo);
			});
		}

		public void TestAllowDuplicatesIfCodeIsAPermit()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("0001", isImport: true, hasPermitAttribute: false));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var supDoc1 = invoiceLine.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "0001";

			var supDoc2 = invoiceLine.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = "0001";
			AssertHasMessageErrorContaining(supDoc2.CSI_CodeInfo, "already exists");

			supDoc1.CSI_Code = "2700";
			supDoc2.CSI_Code = "2700";
			AssertNoMessageErrorContaining(supDoc2.CSI_CodeInfo, "already exists");
		}

		public void TestInconsistentUnitOrCurrency()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2700", isImport: true, hasPermitAttribute: true));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var supDoc1 = declaration.SupportingDocuments.AddNew();
			var supDoc2 = declaration.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "2700";
			supDoc1.CSI_Code = "2700";

			supDoc1.CSI_ReferenceNumber = "PermitA";
			supDoc2.CSI_ReferenceNumber = "PermitA";

			supDoc1.CSI_UnitOfQuantity = "KG";
			supDoc2.CSI_UnitOfQuantity = "CM";
			supDoc2.RunPreSaveValidation();
			AssertHasRowMessageError("Same reference, but different units (KG & CM).", supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc2.CSI_UnitOfQuantity = "KG";
			supDoc2.RunPreSaveValidation();
			AssertNoRowMessageError(supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc1.CSI_RX_NKCurrency = "GBP";
			supDoc2.CSI_RX_NKCurrency = "FRF";
			supDoc2.RunPreSaveValidation();
			AssertHasRowMessageError("Same reference, but different currencies (GBP & FRF).", supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc2.CSI_ReferenceNumber = "PermitB";
			supDoc2.RunPreSaveValidation();
			AssertNoRowMessageError(supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc1.CSI_Code = "0001";
			supDoc1.CSI_Code = "0001";
			supDoc1.CSI_UnitOfQuantity = "KG";
			supDoc2.CSI_UnitOfQuantity = "CM";
			supDoc2.RunPreSaveValidation();
			AssertNoRowMessageError(supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);
		}

		public void TestCheckCSI_DateOfIssue()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_ReferenceNumber = "1";

			supportingDocument.CSI_IsDTP = false;
			supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument.Validation.ValidateCSI_DateOfIssue();
			AssertHasMessageError(supportingDocument.CSI_DateOfIssueInfo, MessageBuilderHelper.MessageSupportingDocNeedDate("1"));

			supportingDocument.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument.Validation.ValidateCSI_DateOfIssue();
			AssertNoMessageError(supportingDocument.CSI_DateOfIssueInfo, MessageBuilderHelper.MessageSupportingDocNeedDate("1"));

			supportingDocument.CSI_IsDTP = true;
			supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument.Validation.ValidateCSI_DateOfIssue();
			AssertNoMessageError(supportingDocument.CSI_DateOfIssueInfo, MessageBuilderHelper.MessageSupportingDocNeedDate("1"));

			supportingDocument.CSI_DateOfIssue = ZDateTime.Today;
			supportingDocument.Validation.ValidateCSI_DateOfIssue();
			AssertNoMessageError(supportingDocument.CSI_DateOfIssueInfo, MessageBuilderHelper.MessageSupportingDocNeedDate("1"));
		}

		public void TestCheckCSI_DateOfIssue_ATVAI_SupportingDoc()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_VATDeferType = VATProcedureList.Codes.L;
			frOrgImpAddInfo.ZO_VATProcedureDateLimit = ZDate.Today.AddYears(-1);
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;

			var errorMessage = $"This type of VAT procedure is not valid yet";
			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode;

			supportingDocument.CSI_DateOfIssue = ZDate.Today.AddMonths(-1);
			AssertNoMessageErrorContaining(supportingDocument.CSI_DateOfIssueInfo, errorMessage);

			supportingDocument.CSI_DateOfIssue = ZDate.Today;
			AssertNoMessageErrorContaining(supportingDocument.CSI_DateOfIssueInfo, errorMessage);

			supportingDocument.CSI_DateOfIssue = ZDate.Today.AddMonths(1);
			AssertHasMessageErrorContaining(supportingDocument.CSI_DateOfIssueInfo, errorMessage);

			supportingDocument.CSI_DateOfIssueInfo.ClearValue();
			AssertHasMessageErrorContaining(supportingDocument.CSI_DateOfIssueInfo, "must have a date");
		}

		public void TestCheckCSI_DateOfIssue_DateShouldNotBePresent_WhenCodeIsATVAIAndDeferTypeIsNotL()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			declaration.ZG_VATDeferType = VATProcedureList.Codes.S;
			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode;
			ValidationTestHelper.AssertIfIsEnteredMessageError(supportingDocument1.CSI_DateOfIssueInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode;
			ValidationTestHelper.AssertIfIsEnteredMessageError(supportingDocument2.CSI_DateOfIssueInfo);

			declaration.ZG_VATDeferType = VATProcedureList.Codes.L;
			var supportingDocument3 = declaration.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatAndDutiesAdditionalCode;
			AssertNoMessageErrorContaining(supportingDocument3.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			var supportingDocument4 = declaration.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = VATDeferStrategyCodeList.Codes.VisaFreeAi2VatOnlyAdditionalCode;
			AssertNoMessageErrorContaining(supportingDocument4.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckCSI_DateOfIssueWhenCodeIsAi2_TVAOrAi2_WithoutTVA()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			supportingDocument1.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument1.Validation.ValidateCSI_DateOfIssue();
			AssertHasMessageError(supportingDocument1.CSI_DateOfIssueInfo, "The date should be declaration submission date.");

			supportingDocument1.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			supportingDocument1.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument1.Validation.ValidateCSI_DateOfIssue();
			AssertHasMessageError(supportingDocument1.CSI_DateOfIssueInfo, "The date should be declaration submission date.");

			supportingDocument1.CSI_DateOfIssue = ZDateTime.Today;
			AssertNoMessageError(supportingDocument1.CSI_DateOfIssueInfo, "The date should be declaration submission date.");

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			supportingDocument1.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument1.CSI_Code = VATDeferStrategyCodeList.Codes.IdentifiedVATNumber;
			supportingDocument1.Validation.ValidateCSI_DateOfIssue();
			AssertNoMessageError(supportingDocument1.CSI_DateOfIssueInfo, "The date should be declaration submission date.");

			supportingDocument1.CSI_Code = VATDeferStrategyCodeList.Codes.UnidentifiedVATNumber;
			supportingDocument1.CSI_DateOfIssue = ZDateTime.Empty;
			supportingDocument1.Validation.ValidateCSI_DateOfIssue();
			AssertHasMessageError(supportingDocument1.CSI_DateOfIssueInfo, "The date should be declaration submission date.");
		}

		public void TestCheckCSI_Quantity3()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var info = supportingDocument.CSI_Quantity3Info;
			string errorNumberNotNegative = "Please enter a 'D48 Duration in Months' greater than or equal to 0.";
			supportingDocument.CSI_Quantity3 = -1;
			AssertHasErrorContaining(info, errorNumberNotNegative);
			supportingDocument.CSI_Quantity3 = 1;
			AssertNoErrorContaining(info, errorNumberNotNegative);
			supportingDocument.CSI_Quantity3 = 0;
			AssertNoErrorContaining(info, errorNumberNotNegative);
		}

		public void TestCheckMixedStatuses()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = "G2";
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();

			entry1.CH_EntryStatus = "100";
			entry2.CH_EntryStatus = "101";

			var suppDoc = declaration.SupportingDocuments.AddNew();
			suppDoc.Validation.ValidateAll();
			AssertHasRowMessageError(suppDoc, "This declaration has entries with multiple statuses");

			entry2.CH_EntryStatus = "100";
			suppDoc.Validation.ValidateAll();
			AssertNoRowMessageError(suppDoc, "This declaration has entries with multiple statuses");

			var invoiceHeader = declaration.Invoices.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;
			invLine2.JI_CEI = entryInstruction2.PK;

			var suppDoc2 = invoiceHeader.SupportingDocuments.AddNew();
			suppDoc2.Validation.ValidateAll();
			AssertHasRowMessageError(suppDoc2, "This invoice contains lines which are used over multiple entry instructions");

			invLine2.JI_CEI = entryInstruction1.PK;
			suppDoc2.Validation.ValidateAll();
			AssertNoRowMessageError(suppDoc2, "This invoice contains lines which are used over multiple entry instructions");
		}

		public void TestCheckCSI_Reference_WithG1Declaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = "G1";
			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			Assert(declaration.IsDeltaC);

			entry1.CH_EntryStatus = "100";
			entry2.CH_EntryStatus = "101";

			var suppDoc = declaration.SupportingDocuments.AddNew();
			suppDoc.CSI_IsDTP = false;

			suppDoc.CSI_ReferenceNumber = "TEST";
			AssertNoMessageErrorContaining(suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			suppDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			suppDoc.CSI_IsDTP = true;

			suppDoc.CSI_ReferenceNumber = "TEST";
			AssertNoMessageErrorContaining(suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			suppDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining(suppDoc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestDateOfIssueIsOlderThan25Years()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var supDoc1 = invoiceLine.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = "0001";

			supDoc1.CSI_DateOfIssue = ZDateTime.Today;
			AssertNoMessageErrorContaining(supDoc1.CSI_DateOfIssueInfo, "is more than 25 years old and thus is not valid.");

			supDoc1.CSI_DateOfIssue = ZDateTime.Today.AddYears(-26);
			AssertHasMessageErrorContaining(supDoc1.CSI_DateOfIssueInfo, "is more than 25 years old and thus is not valid.");

			AssertNoErrors(supDoc1.CSI_DateOfIssueInfo);
		}

		public void TestCheckCSI_Quantity3_IsD48()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.No);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			AssertValidateCSI_Quantity3(supportingDocument);

			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			var headerSupportingDocument = jobComInvoiceHeader.SupportingDocuments.AddNew();
			AssertValidateCSI_Quantity3(headerSupportingDocument);

			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();
			AssertValidateCSI_Quantity3(lineSupportingDocument);
		}

		void AssertValidateCSI_Quantity3(SupportingDocument sd)
		{
			sd.CSI_Quantity3 = 0m;
			sd.CSI_DateOfIssue = ZDateTime.Today;
			sd.CSI_Code = "0001";
			AssertNoMessageErrors("CSI_Quantity3 = 0 no error", sd.CSI_Quantity3Info);

			sd.CSI_Quantity3 = 1m;
			AssertHasMessageError("CSI_Quantity3 > 0 and Is not D48", sd.CSI_Quantity3Info, "D48 Duration in Months is only available for D48 document.");

			sd.CSI_Code = "0003";
			AssertNoMessageErrors("CSI_Quantity3 > 0 and Is D48", sd.CSI_Quantity3Info);

			sd.CSI_Code = "2044";
			AssertHasMessageError("CSI_Code changed and fire re-validation", sd.CSI_Quantity3Info, "D48 Duration in Months is only available for D48 document.");

			sd.CSI_Quantity3 = 0m;
			AssertNoMessageErrors("CSI_Quantity3 = 0", sd.CSI_Quantity3Info);
		}
	}
}
