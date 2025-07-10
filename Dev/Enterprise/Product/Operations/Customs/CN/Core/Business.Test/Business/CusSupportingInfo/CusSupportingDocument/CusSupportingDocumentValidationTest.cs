using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusSupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCusSupportingDocumentsAcrossInstruction()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var invoiceLine1 = testItem.InvoiceLine;
			invoiceLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			var invoiceLine2 = (JobComInvoiceLine)testItem.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testItem.EntryInstruction.PK;
			invoiceLine2.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			var invLn1Doc1 = invoiceLine1.CusSupportingDocuments.AddNew();
			invLn1Doc1.CSI_Code = "1K";
			invLn1Doc1.CSI_ReferenceNumber = "DOC001";
			var invLn2Doc1 = invoiceLine2.CusSupportingDocuments.AddNew();
			invLn2Doc1.CSI_Code = "1K";
			invLn2Doc1.CSI_ReferenceNumber = "DOC002";

			var message = "This Document Type with a different Number has already been specified for the selected Entry Instruction.\nPlease select a different Entry Instruction for this Invoice Line if it is a different document.";
			AssertHasMessageError(invLn2Doc1.CSI_CodeInfo, message);

			invLn2Doc1.CSI_ReferenceNumber = "DOC001";
			AssertNoMessageError(invLn2Doc1.CSI_CodeInfo, message);
		}

		public void TestCheckCSI_LineNoAndCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("RequiresLineNumber", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew("RequiresLineNumber", "Mandatory");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("RequiresLineNumber", "Optional");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD3", "Code 3");
			Factory.Save();
			var testItem = GetNewBusinessObject();
			testItem.CSI_LineNo = -1;
			AssertHasErrorContaining(testItem.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
			testItem.CSI_LineNo = 0;
			AssertNoErrorContaining(testItem.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);
			testItem.CSI_Code = "CD1";
			testItem.Validation.ValidateCSI_LineNo();
			AssertHasMessageErrorContaining(testItem.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
			testItem.CSI_Code = "CD2";
			testItem.Validation.ValidateCSI_LineNo();
			AssertNoMessageErrorContaining(testItem.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
			testItem.CSI_Code = "CD3";
			testItem.Validation.ValidateCSI_LineNo();
			AssertNoMessageErrorContaining(testItem.CSI_LineNoInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckDocumentType()
		{
			var testItem = GetNewBusinessObject();
			testItem.CSI_Code = "1Y";
			AssertNoErrors(testItem.DocumentTypeInfo);
			testItem.CSI_Code = "01";
			AssertHasError("Should have error if Document Type not selected", testItem.DocumentTypeInfo, MandatoryValidation.MustBeEnteredMessage(testItem.DocumentTypeInfo.Description));
		}

		public void TestCheckDocumentTypeSupportingDocuments()
		{
			var testItem = GetNewBusinessObject();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew(Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported, ZString.Empty);
			Factory.Save();
			testItem.Parent.Declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			testItem.CSI_Code = "GG";
			AssertHasMessageErrorContaining(testItem.CSI_CodeInfo, "This Document Type is not supported in two-step declaration clearance mode.");
			testItem.CSI_Code = "CD1";
			AssertNoMessageErrorContaining(testItem.CSI_CodeInfo, "This Document Type is not supported in two-step declaration clearance mode.");
			testItem.Parent.Declaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			testItem.CSI_Code = "GG";
			AssertNoMessageErrorContaining(testItem.CSI_CodeInfo, "This Document Type is not supported in two-step declaration clearance mode.");
		}

		public void TestCheckDocumentType_DuplicatedTypesAndIsLicense()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("Export", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "X");
			code1.Attributes.AddNew("IsLicense", ZString.Empty);
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("Export", ZString.Empty);
			code2.Attributes.AddNew("Import", ZString.Empty);
			code2.Attributes.AddNew("DisplayCode", "Y");
			code2.Attributes.AddNew("IsLicense", ZString.Empty);
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD3", "Code 3");
			code3.Attributes.AddNew("Import", ZString.Empty);
			code3.Attributes.AddNew("Export", ZString.Empty);
			code3.Attributes.AddNew("DisplayCode", "Z");
			var code4 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "1Y", "Certificate of Origin");
			code4.Attributes.AddNew("Import", ZString.Empty);
			code3.Attributes.AddNew("DisplayCode", "Y");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;

			var inst = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = inst.PK;
			invoiceLine2.JI_CEI = inst.PK;

			var supportingDocs1 = invoiceLine.CusSupportingDocuments;
			var supportingDocs2 = invoiceLine2.CusSupportingDocuments;

			var tester_1 = supportingDocs1.AddNew();
			tester_1.DocumentType = "X.Code 1";
			tester_1.CSI_ReferenceNumber = "D00001";
			AssertEquals("CD1", tester_1.CSI_Code);
			AssertNoMessageErrors(tester_1.DocumentTypeInfo);
			var tester_2 = supportingDocs1.AddNew();
			tester_2.DocumentType = "Z.Code 3";
			AssertNoMessageErrors(tester_2.DocumentTypeInfo);
			tester_2.CSI_ReferenceNumber = "D00001";
			tester_2.DocumentType = "Y.Code 2";
			AssertEquals("CD2", tester_2.CSI_Code);
			AssertHasMessageErrorContaining(tester_2.DocumentTypeInfo, "Only one License is allowed.");
			tester_2.DocumentType = "Z.Code 3";

			var tester_3 = supportingDocs2.AddNew();
			tester_3.CSI_ReferenceNumber = "D00001";
			tester_3.DocumentType = "X.Code 1";
			AssertNoMessageErrorContaining(tester_3.DocumentTypeInfo, "Only one License is allowed.");

			tester_3.CSI_ReferenceNumber = "D00002";
			tester_3.Validation.ValidateCSI_Code();
			AssertEquals("CD1", tester_3.CSI_Code);
			AssertHasMessageErrorContaining(tester_3.DocumentTypeInfo, "has already been specified for the selected Entry");
			AssertHasMessageErrorContaining(tester_3.DocumentTypeInfo, "Only one License is allowed.");

			tester_2.DocumentType = "Y.Code 2";
			tester_3.DocumentType = "Z.Code 3";
			AssertNoMessageErrors(tester_3.DocumentTypeInfo);

			tester_2.DocumentType = "X.Code 1";
			tester_2.CSI_ReferenceNumber = "D00001";
			AssertHasMessageErrorContaining(tester_2.DocumentTypeInfo, "The Document Type has been duplicated and must be unique.");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);

			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("Export", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "X");
			code1.Attributes.AddNew("IsLicense", ZString.Empty);

			Factory.Save();

			var testItem = GetNewBusinessObject();
			testItem.CSI_ReferenceNumber = ZString.Empty;
			var targetInfo = testItem.CSI_ReferenceNumberInfo;
			testItem.Validation.ValidateAll();

			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			testItem.CSI_ReferenceNumber = "D0100000001";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			testItem.CSI_ReferenceNumber = "!@#$%^&*";
			AssertHasMessageErrorContaining(targetInfo, "should contain at least one alphanumeric character.");

			testItem.CSI_ReferenceNumber = "!@#$%^&*1";
			AssertNoMessageErrorContaining(targetInfo, "should contain at least one alphanumeric character.");

			testItem.CSI_ReferenceNumber = "!@#$x%^&*";
			AssertNoMessageErrorContaining(targetInfo, "should contain at least one alphanumeric character.");

			testItem.CSI_ReferenceNumber = "Y!@#$%^&*";
			AssertNoMessageErrorContaining(targetInfo, "should contain at least one alphanumeric character.");

			testItem.CSI_ReferenceNumber = "12345678901234567890123456789012";
			AssertNoMessageErrorContaining(targetInfo, "exceeds the maximum allowed");

			testItem.DocumentType = "X.Code 1";
			testItem.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining(targetInfo, "exceeds the maximum allowed");
			testItem.CSI_ReferenceNumber = "12345678901234567890";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckDocumentType_CodeNotForJobType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "X");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CDB", "Code Both");
			code3.Attributes.AddNew("Export", ZString.Empty);
			code3.Attributes.AddNew("Import", ZString.Empty);
			code3.Attributes.AddNew("DisplayCode", "b");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("Export", ZString.Empty);
			code2.Attributes.AddNew("DisplayCode", "Y");
			var codeIgnore = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CUSAB", "CD3", "Code 3");
			codeIgnore.Attributes.AddNew("Import", ZString.Empty);
			codeIgnore.Attributes.AddNew("DisplayCode", "Z");
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var testDocument = invoiceLine.CusSupportingDocuments.AddNew();
			testDocument.CSI_Code = "CDB";
			AssertNoMessageErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
			testDocument.CSI_Code = "CD2";
			AssertHasErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
			testDocument.CSI_Code = "CD1";
			AssertNoMessageErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			testDocument.Validation.ValidateAll();
			AssertHasErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
			testDocument.CSI_Code = "CD2";
			AssertNoMessageErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
			testDocument.CSI_Code = "CDB";
			AssertNoMessageErrorContaining(testDocument.CSI_CodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidationModeProvider()
		{
			var document = GetNewBusinessObject();
			ValidationExtensionsTest.AssertValidationModeProvider(document.Parent.Declaration, document.Validation.ValidationModeProvider);

			document = Factory.New<CusSupportingDocument>();
			AssertNull(document.Validation.ValidationModeProvider);
		}

		protected CusSupportingDocument GetNewBusinessObject()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			return invoiceLine.CusSupportingDocuments.AddNew();
		}
	}
}
