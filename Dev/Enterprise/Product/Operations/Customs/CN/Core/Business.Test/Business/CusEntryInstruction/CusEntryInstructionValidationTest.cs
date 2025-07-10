using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2020, 3, 20)]
		public void TestCheckOperationMattersAsString()
		{
			instruction.OperationMatters.AddNew(OperationMatterList.Codes.AssuredInspectClearance);
			instruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.Paperless;
			instruction.Validation.ValidateOperationMattersAsString();
			AssertHasMessageError(instruction.OperationMattersAsStringInfo, "Only '通关无纸化' entry supports 担保验放.");
			instruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms;
			instruction.Validation.ValidateOperationMattersAsString();
			AssertNoMessageErrors(instruction.OperationMattersAsStringInfo);
		}

		public void TestCheckCEI_Style()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure("CN", "", "XX", "", "", "Export", "EXP");
			instruction.Validation.ValidateCEI_Style();
			AssertHasMessageErrorContaining(instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.CEI_Style = "A";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageErrorContaining(instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(instruction.CEI_StyleInfo, ListValidation.InvalidCodeMessageError);
			instruction.CEI_Style = "XX";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageErrorContaining(instruction.CEI_StyleInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(instruction.CEI_StyleInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateEntryMaxCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < 50; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "10010" + idx.ToString().PadLeft(2, '0');
			}

			declaration.DoMerge();
			var maximumEntryLinesAllowedExceededMessage = "There are more than 50 Entry Lines linked to this Entry Instruction, please split it.";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, maximumEntryLinesAllowedExceededMessage);
			childInstruction.Validation.ValidateAll();
			AssertNoRowMessageError(childInstruction, maximumEntryLinesAllowedExceededMessage);
			var invoiceLine51 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine51.JI_CEI = instruction.PK;
			invoiceLine51.JI_Tariff = "1001051";
			declaration.DoMerge();
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, maximumEntryLinesAllowedExceededMessage);
			childInstruction.Validation.ValidateAll();
			AssertNoRowMessageError(childInstruction, maximumEntryLinesAllowedExceededMessage);
		}

		public void TestCheckSpecialBusinessIdentifiersAsString()
		{
			declaration.JE_MessageType = "IMP";
			instruction.SpecialBusinessIdentifiers.AddNew("B01");
			instruction.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.SpecialBusinessIdentifiersAsStringInfo, "Export job does not support");
			instruction.SpecialBusinessIdentifiers.AddNew("C03");
			instruction.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.SpecialBusinessIdentifiersAsStringInfo, "Export job does not support");
			declaration.JE_MessageType = "EXP";
			instruction.Validation.ValidateAll();
			AssertHasMessageErrorContaining(instruction.SpecialBusinessIdentifiersAsStringInfo, "Export job does not support");
			instruction.SpecialBusinessIdentifiers.RemoveAndDeleteAll();
			instruction.SpecialBusinessIdentifiers.AddNew("B01");
			instruction.Validation.ValidateAll();
			AssertNoMessageErrorContaining(instruction.SpecialBusinessIdentifiersAsStringInfo, "Export job does not support");
		}

		public void TestCheckXC_BillOfLading()
		{
			var targetInfo = instruction.BillOfLadingInfo;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var msgAirImp = "B/L No. should be 11 digits followed by \"_\" plus alphanumeric characters, or 11 digits.";
			instruction.BillOfLading = "test";
			instruction.BillOfLading = "";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.BillOfLading = "1234567890+";
			AssertHasMessageError(targetInfo, msgAirImp);
			instruction.BillOfLading = "123456789012";
			AssertHasMessageError(targetInfo, msgAirImp);
			instruction.BillOfLading = "12345678901_1+";
			AssertHasMessageError(targetInfo, msgAirImp);
			instruction.BillOfLading = "12345678901_1AB";
			AssertNoMessageErrors(targetInfo);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var msgAirExp = "B/L No. should be 11 digits followed by \"_\" plus no more than 8 alphanumeric characters, or 11 digits.";
			instruction.BillOfLading = "12345678901_1ABCDEFGH";
			AssertHasMessageError(targetInfo, msgAirExp);
			instruction.BillOfLading = "12345678901_1ABCDEFG";
			AssertNoMessageErrors(targetInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			instruction.BillOfLading = "";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			var msgSea = "B/L No. should consist of alphanumeric characters and asterisk(*), and may not begin or end with asterisk.";
			instruction.BillOfLading = "A123+BC";
			AssertHasMessageError(targetInfo, msgSea);
			instruction.BillOfLading = "*A123BC";
			AssertHasMessageError(targetInfo, msgSea);
			instruction.BillOfLading = "A123BC*";
			AssertHasMessageError(targetInfo, msgSea);
			instruction.BillOfLading = "A123*BC";
			AssertNoMessageErrors(targetInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			instruction.BillOfLading = "";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			var msgMail = "B/L No. should be alphanumeric characters";
			instruction.BillOfLading = "A123_BC";
			AssertHasMessageError(targetInfo, msgMail);
			instruction.BillOfLading = "A123BC";
			AssertNoMessageErrors(targetInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			instruction.BillOfLading = "";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			instruction.BillOfLading = "A123*BC";
			AssertNoMessageErrors(targetInfo);
			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			instruction.BillOfLading = "A123*BC";
			instruction.Validation.ValidateBillOfLading();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.DoNotEntered);
			instruction.BillOfLading = "";
			AssertNoMessageErrors(targetInfo);
		}

		public void TestCheckInvoiceLines()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusProc = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.China, "", "1011", "", "", "Customs Procedure 1011", "IMP,EXP");
			instruction.CEI_Style = cusProc.ZZ6_ProcedureCode;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "ABC";
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = instruction.PK;
			var charge111 = invoiceLine11.Charges.AddNew();
			charge111.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge111.J7_RX_NKCurrency = "CNY";
			var charge112 = invoiceLine11.Charges.AddNew();
			charge112.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge112.J7_RX_NKCurrency = "CNY";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "DEF";
			var invoiceLine21 = invoice2.InvoiceLines.AddNew();
			invoiceLine21.JI_CEI = instruction.PK;
			var charge211 = invoiceLine21.Charges.AddNew();
			charge211.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge211.J7_RX_NKCurrency = "AUD";
			var charge212 = invoiceLine21.Charges.AddNew();
			charge212.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge212.J7_RX_NKCurrency = "AUD";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError(instruction, "The Invoice Headers linked to this Entry Instruction have different Incoterms.");
			AssertHasRowMessageError(instruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for OFT charges.");
			AssertHasRowMessageError(instruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for ONS charges.");
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_CEI_Parent = instruction.PK;
			childInstruction.Validation.ValidateAll();
			AssertHasRowMessageError(childInstruction, "The Invoice Headers linked to this Entry Instruction have different Incoterms.");
			AssertHasRowMessageError(childInstruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for OFT charges.");
			AssertHasRowMessageError(childInstruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for ONS charges.");
			invoice2.JZ_IncoTerm = "ABC";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, "The Invoice Headers linked to this Entry Instruction have different Incoterms.");
			charge211.J7_RX_NKCurrency = "CNY";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for OFT charges.");
			charge212.J7_RX_NKCurrency = "CNY";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError(instruction, "The Invoice Lines linked to this Entry Instruction have multiply Currencies for ONS charges.");
		}

		public void TestCheckCertificateOfOrigin()
		{
			var messageText = "The Invoice Lines linked to this Entry Instruction have multiply Certificate of Origin or Preferential Code.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_PrimaryPreference = "NORMAL";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_PrimaryPreference = "NORMAL";
			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine2.CertificateOfOrigin = "222";
			invoiceLine1.TradeAgreementCode = "01";
			invoiceLine2.TradeAgreementCode = "01";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("None of the lines is CertificateOfOrigin Applicable, should NOT add message.", instruction, messageText);

			invoiceLine1.JI_PrimaryPreference = "FTA";
			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine2.CertificateOfOrigin = "222";
			invoiceLine1.TradeAgreementCode = "01";
			invoiceLine2.TradeAgreementCode = "01";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("None of the lines is CertificateOfOrigin Applicable, should NOT add message.", instruction, messageText);

			invoiceLine2.JI_PrimaryPreference = "FTA";
			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine2.CertificateOfOrigin = "222";
			invoiceLine1.CertificateOfOriginType = "C";
			invoiceLine2.CertificateOfOriginType = "D";
			invoiceLine1.TradeAgreementCode = "01";
			invoiceLine2.TradeAgreementCode = "01";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Both are CertificateOfOrigin Applicable, multiple CertificateOfOrigin and multiple CertificateOfOriginType, should add message.", instruction, messageText);

			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine2.CertificateOfOrigin = "111";
			invoiceLine1.TradeAgreementCode = "01";
			invoiceLine2.TradeAgreementCode = "02";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Both are CertificateOfOrigin Applicable, multiple CertificateOfOrigin and multiple CertificateOfOriginType, should add message.", instruction, messageText);

			invoiceLine1.CertificateOfOrigin = "111";
			invoiceLine2.CertificateOfOrigin = "111";
			invoiceLine1.TradeAgreementCode = "01";
			invoiceLine2.TradeAgreementCode = "01";
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Both are CertificateOfOrigin Applicable, multiple CertificateOfOriginType, should add message.", instruction, messageText);

			invoiceLine1.CertificateOfOriginType = "C";
			invoiceLine2.CertificateOfOriginType = "C";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Both are CertificateOfOrigin Applicable, multiple CertificateOfOriginType, should add message.", instruction, messageText);
		}

		public void TestCheckItemNoOnCertOfOrigin()
		{
			var invHeader = declaration.Invoices.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var invLine2 = invHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			invLine1.JI_CEI = instruction.PK;
			invLine2.JI_CEI = instruction.PK;
			invLine1.JI_CL = entryLine1.PK;
			invLine2.JI_CL = entryLine2.PK;
			invLine1.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invLine2.JI_PrimaryPreference = Constants.PrimaryPreferenceCodes.FreeTradeAgreement;
			invLine1.CertificateOfOrigin = "1";
			invLine2.CertificateOfOrigin = "1";
			invLine1.ItemNoOnCertOfOrigin = 1;
			invLine2.ItemNoOnCertOfOrigin = 1;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(instruction, "The Invoice Lines which have the same Certificate Of Origin Item No. should be merged to one Entry Line");
			invLine1.JI_CL = entryLine2.PK;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(instruction, "The Invoice Lines which have the same Certificate Of Origin Item No. should be merged to one Entry Line");
			invLine1.JI_CL = entryLine1.PK;
			invLine2.ItemNoOnCertOfOrigin = 2;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(instruction, "The Invoice Lines which have the same Certificate Of Origin Item No. should be merged to one Entry Line");
			invLine2.ItemNoOnCertOfOrigin = 1;
			invLine2.CertificateOfOrigin = "2";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(instruction, "The Invoice Lines which have the same Certificate Of Origin Item No. should be merged to one Entry Line");
		}

		public void TestValidationModeProvider()
		{
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, instruction.Validation.ValidationModeProvider);

			var instruction2 = Factory.New<CusEntryInstruction>();
			AssertNull(instruction2.Validation.ValidationModeProvider);
		}

		public void TestCheckCEI_SubStyle()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(instruction.CEI_SubStyleInfo, new ZString[] { "X" }, new ZString[] { "0", "1", "2" });

			var message = "This is the initial submission, please check the Intelligent Declaration Type.";

			instruction.CEI_SubStyle = ZString.Empty;
			AssertNoWarning(instruction.CEI_SubStyleInfo, message);

			instruction.CEI_SubStyle = IntelligentDeclarationTypeList.Codes.Non;
			AssertNoWarning(instruction.CEI_SubStyleInfo, message);

			instruction.CEI_SubStyle = IntelligentDeclarationTypeList.Codes.Add;
			AssertNoWarning(instruction.CEI_SubStyleInfo, message);

			instruction.CEI_SubStyle = IntelligentDeclarationTypeList.Codes.Cancel;
			AssertHasWarning(instruction.CEI_SubStyleInfo, message);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasWarning(instruction.CEI_SubStyleInfo, message);

			entryHeader.CH_EntryStatus = "01";
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoWarning(instruction.CEI_SubStyleInfo, message);
		}

		#region AddInfo properties

		public void TestCheckCEI_Packages()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.Validation.ValidateCEI_Packages();
			AssertHasMessageErrorContaining(entryInstruction.CEI_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			entryInstruction.CEI_Packages = -10;
			AssertHasMessageErrorContaining(entryInstruction.CEI_PackagesInfo, MandatoryValidation.ValueCannotBeNegative);
			entryInstruction.CEI_Packages = 10;
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackagesInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackagesInfo, MandatoryValidation.ValueCannotBeNegative);
			entryInstruction.CEI_PackageUQ = PackageType.Codes.Naked;
			AssertHasMessageError(entryInstruction.CEI_PackagesInfo, CusEntryInstructionValidation.PackTypeMessageError);
			entryInstruction.CEI_PackageUQ = PackageType.Codes.Bulk;
			AssertHasMessageError(entryInstruction.CEI_PackagesInfo, CusEntryInstructionValidation.PackTypeMessageError);
			entryInstruction.CEI_PackageUQ = PackageType.Codes.Bag;
			AssertNoMessageError(entryInstruction.CEI_PackagesInfo, CusEntryInstructionValidation.PackTypeMessageError);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = "BTH";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Packages = 11;
			instruction.CEI_PackageUQ = "01";
			childInstruction.CEI_Packages = 12;
			childInstruction.CEI_PackageUQ = "02";
			AssertHasMessageErrorContaining(childInstruction.CEI_PackagesInfo, "should equal to the value on its related Instruction.");
			AssertHasMessageErrorContaining(childInstruction.CEI_PackageUQInfo, "should equal to the value on its related Instruction.");
		}

		public void TestCheckCEI_PackageUQ()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.Validation.ValidateCEI_PackageUQ();
			AssertHasMessageErrorContaining(entryInstruction.CEI_PackageUQInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PackageUQ = "X";
			AssertHasMessageErrorContaining(entryInstruction.CEI_PackageUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageUQInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PackageUQ = PackageType.Codes.Bag;
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(entryInstruction.CEI_PackageUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckCEI_LevyType()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerCCD = importer.CustomsCodes.AddNew("CCD", "1111111111", "CN");
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.CustomsCodes.AddNew("CCD", "9999999999", "CN");
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_OH_Importer = importer.PK;
			testDeclaration.JE_OH_Supplier = supplier.PK;
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var targetInfo = testInst.CEI_LevyTypeInfo;
			testInst.Validation.ValidateCEI_LevyType();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testInst.CEI_Style = "118";
			testInst.Validation.ValidateCEI_LevyType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testDeclaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			testInst.Validation.ValidateCEI_LevyType();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testInst.CEI_Style = CNRefCusProcedure.Codes._0245;
			testInst.Validation.ValidateCEI_LevyType();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testInst.CEI_LevyType = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testInst.CEI_LevyType = "101";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testInst.CEI_LevyType = "506";
			testDeclaration.JE_CNTransportMode = "5";
			testInst.Validation.ValidateCEI_LevyType();
			AssertHasMessageErrorContaining(targetInfo, "The selected Levy Type is not suitable for Transport Mode Air");
			testDeclaration.JE_CNTransportMode = "6";
			testInst.Validation.ValidateCEI_LevyType();
			AssertNoMessageErrors(targetInfo);
			testDeclaration.JE_CNTransportMode = "5";
			testInst.CEI_LevyType = "118";
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_LevyType = ZString.Empty;
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_Style = CNRefCusProcedure.Codes._0255;
			testInst.CEI_LevyType = "118";
			AssertHasMessageErrorContaining(targetInfo, "Levy Type should be blank when Procedure Code is");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0255;
			testInst.CEI_LevyType = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, "Levy Type should be blank when Procedure Code is");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "307";
			AssertNoWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "139";
			AssertHasWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			AssertNoWarningContaining(targetInfo, "or blank");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "101";
			AssertNoWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0200;
			testInst.CEI_LevyType = "139";
			AssertHasWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			AssertHasWarningContaining(targetInfo, "or blank");
			importerCCD.OK_CustomsRegNo = "1111111111";
			testInst.CEI_LevyType = "601";
			testInst.Validation.ValidateCEI_LevyType();
			AssertHasMessageError(targetInfo, "When Levy Type is 601, the sixth digit of the CCD number of Importer should be 3.");
			importerCCD.OK_CustomsRegNo = "1111131111";
			testInst.Validation.ValidateCEI_LevyType();
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_LevyType = "602";
			AssertHasMessageError(targetInfo, "When Levy Type is 602, the sixth digit of the CCD number of Importer should be 2.");
			testInst.CEI_LevyType = "603";
			AssertHasMessageError(targetInfo, "When Levy Type is 603, the sixth digit of the CCD number of Importer should be 4.");
			importerCCD.OK_CustomsRegNo = "1111111111";
			testInst.CEI_LevyType = "799";
			AssertHasMessageError(targetInfo, "When Levy Type is 799, the sixth digit of the CCD number of Importer should be 2,3,4.");
			importerCCD.OK_CustomsRegNo = "1111141111";
			testInst.Validation.ValidateCEI_LevyType();
			AssertNoMessageErrors(targetInfo);
			testDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testInst.CEI_LevyType = "602";
			AssertHasMessageError(targetInfo, "When Levy Type is 602, the sixth digit of the CCD number of Supplier should be 2.");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "307";
			AssertHasWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			AssertNoWarningContaining(targetInfo, "or blank");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "139";
			AssertHasWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			AssertNoWarningContaining(targetInfo, "or blank");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0110;
			testInst.CEI_LevyType = "101";
			AssertNoWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			testInst.CEI_Style = CNRefCusProcedure.Codes._0200;
			testInst.CEI_LevyType = "139";
			AssertHasWarningContaining(targetInfo, "The selected levy type might not suitable for the selected procedure code.\r\nThe suitable levy types:");
			AssertHasWarningContaining(targetInfo, "or blank");
			testDeclaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var testInstRec = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstRec.CEI_CEI_Parent = testInst.PK;
			testInst.CEI_LevyType = "139";
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.DoNotEntered);
			testInst.CEI_LevyType = "";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.DoNotEntered);
		}

		[TestDate(2016, 5, 5)]
		public void TestCheckCEI_ManualNo()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var testInst = testItems.EntryInstruction;
			testInst.CEI_ManualNo = ZString.Empty;
			var targetInfo = testInst.CEI_ManualNoInfo;
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "12AArr33";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "12AArr33DDD";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "2AArr33~D!4";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "112345678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "AZ2345678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A1234!678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A12345!78901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A123456!8901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A1234567A901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A12345678901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A1234D678901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_ManualNo = "A1234ABC8901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_LevyType = "201";
			testInst.CEI_ManualNo = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testInst.CEI_Style = CNRefCusProcedure.Codes._0815;
			testInst.CEI_LevyType = "201";
			testInst.CEI_ManualNo = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			testInst.CEI_ManualNo = "A12345678901";
			AssertHasMessageErrorContaining(targetInfo, "Manual Number should start with Z");
			testInst.CEI_ManualNo = "Z12345678901";
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_LevyType = "501";
			testInst.CEI_ManualNo = "A12345678901";
			AssertHasMessageErrorContaining(targetInfo, "Manual Number should start with D");
		}

		public void TestCheckCEI_RelatedMRN()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_RelatedMRN = ZString.Empty;
			var targetInfo = testInst.CEI_RelatedMRNInfo;
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_RelatedMRN = "12345678901234567";
			AssertHasMessageErrorContaining(targetInfo, "Customs Entry Number should be 18 digit (0-9) characters.");
			testInst.CEI_RelatedMRN = "123456789012345678";
			AssertNoMessageErrors(targetInfo);
			testInst.CEI_RelatedMRN = "12^567890A2345678";
			AssertHasMessageErrorContaining(targetInfo, "Customs Entry Number should be 18 digit (0-9) characters.");
		}

		public void TestRequiresRelatedEntryNumber()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			var instruction = testItems.EntryInstruction;
			var testInfo = instruction.CEI_RelatedMRNInfo;
			instruction.Validation.ValidateCEI_RelatedMRN();
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_MessageType = "EXP";
				instruction.Validation.ValidateCEI_RelatedMRN();
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_Style = "0265";
				instruction.Validation.ValidateCEI_RelatedMRN();
				AssertHasMessageErrorContaining("WillGenerateExitingEntry should have message error", testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_RelatedMRN = "MRN001";
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.JE_MessageSubType = "BTH";
				instruction.CEI_RelatedMRN = "";
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_Style = "1427";
				instruction.Validation.ValidateCEI_RelatedMRN();
				AssertHasMessageErrorContaining("WillGenerateEnteringEntry", testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_RelatedMRN = "MRN001";
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_Style = "2600";
				instruction.CEI_RelatedMRN = "";
				AssertHasMessageErrorContaining("CEI_Style in list", testInfo, MandatoryValidation.YouHaveNotEntered);
				instruction.CEI_RelatedMRN = "MRN001";
				AssertNoMessageErrorContaining(testInfo, MandatoryValidation.YouHaveNotEntered);
				var childInstruction = declaration.CustomsEntryInstructions.AddNew();
				childInstruction.Validation.ValidateCEI_RelatedMRN();
				AssertHasMessageErrorContaining("WillGenerateBothEntries", childInstruction.CEI_RelatedMRNInfo, MandatoryValidation.YouHaveNotEntered);
				childInstruction.CEI_RelatedMRN = "MRN001";
				AssertNoMessageErrorContaining(childInstruction.CEI_RelatedMRNInfo, MandatoryValidation.YouHaveNotEntered);
			}

			);
		}

		public void TestCheckCEI_RelatedManualNo()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_RelatedManualNo = ZString.Empty;
			var targetInfo = testInst.CEI_RelatedManualNoInfo;
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "12AArr33";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "12AArr33DDD";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "2AArr33~D!4";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "112345678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "AZ2345678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A1234!678901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A12345!78901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A123456!8901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A1234567A901";
			AssertHasMessageErrorContaining(targetInfo, "Customs Manual Number should have 12 characters and in the format: ANNNNXXXNNNN, where A is a letter, X is alphanumeric and N is a number.");
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A12345678901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A1234D678901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
			testInst.CEI_RelatedManualNo = "A1234ABC8901";
			AssertNoMessageErrors(targetInfo);
			AssertNoErrors(targetInfo);
		}

		public void TestCheckCEI_CIQRelatedReason()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instrunction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instrunction.Validation.ValidateCEI_CIQRelatedReason();
			AssertNoMessageErrors(instrunction.CEI_CIQRelatedReasonInfo);
			instrunction.CEI_CIQRelatedNum = "CUS1234567890";
			instrunction.Validation.ValidateCEI_CIQRelatedReason();
			AssertHasMessageErrorContaining(instrunction.CEI_CIQRelatedReasonInfo, MandatoryValidation.YouHaveNotEntered);
			instrunction.CEI_CIQRelatedReason = "X";
			AssertHasMessageErrorContaining(instrunction.CEI_CIQRelatedReasonInfo, ListValidation.InvalidCodeMessageError);
			instrunction.CEI_CIQRelatedReason = "1";
			AssertNoMessageErrors(instrunction.CEI_CIQRelatedReasonInfo);
		}

		public void TestCheckCEI_CEI_Parent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var instruction1 = Factory.New<CusEntryInstruction>();
			var instruction2 = Factory.New<CusEntryInstruction>();
			var instruction3 = Factory.New<CusEntryInstruction>();
			var instruction4 = Factory.New<CusEntryInstruction>();
			declaration.CustomsEntryInstructions.Add(instruction1);
			declaration.CustomsEntryInstructions.Add(instruction2);
			declaration.CustomsEntryInstructions.Add(instruction3);
			declaration.CustomsEntryInstructions.Add(instruction4);
			instruction2.Validation.ValidateCEI_CEI_Parent();
			AssertNoNotifications(instruction2.CEI_CEI_ParentInfo);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			instruction2.Validation.ValidateCEI_CEI_Parent();
			AssertHasMessageError(instruction2.CEI_CEI_ParentInfo, "This Entry Instruction is isolated.");
			instruction2.CEI_CEI_Parent = instruction1.PK;
			AssertNoMessageError(instruction2.CEI_CEI_ParentInfo, "This Entry Instruction is isolated.");
			instruction4.CEI_CEI_Parent = ZGuid.NewZGuid();
			AssertHasErrorContaining(instruction4.CEI_CEI_ParentInfo, "Please select a valid Parent Instruction.");
			instruction4.CEI_CEI_Parent = instruction1.PK;
			AssertHasError(instruction4.CEI_CEI_ParentInfo, "There are two Entry Instructions have the same parent.");
			instruction4.CEI_CEI_Parent = instruction3.PK;
			AssertNoError(instruction4.CEI_CEI_ParentInfo, "There are two Entry Instructions have the same parent.");
			AssertNoError(instruction4.CEI_CEI_ParentInfo, "Please select a valid Parent Instruction.");
		}

		public void TestCheckXC_DocumentSubmissionType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var instruction1 = Factory.New<CusEntryInstruction>();
			instruction1.Validation.ValidateCEI_DocumentSubmissionType();
			AssertHasMessageErrorContaining(instruction1.CEI_DocumentSubmissionTypeInfo, MandatoryValidation.YouHaveNotEntered);
			instruction1.CEI_DocumentSubmissionType = "X";
			AssertHasMessageErrorContaining(instruction1.CEI_DocumentSubmissionTypeInfo, ListValidation.InvalidCodeMessageError);
			instruction1.CEI_DocumentSubmissionType = "M";
			AssertNoMessageErrors(instruction1.CEI_DocumentSubmissionTypeInfo);
			declaration.CustomsEntryInstructions.Add(instruction1);
			instruction1.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			instruction1.CEI_DocumentSubmissionType = "X";
			AssertHasMessageErrorContaining(instruction1.CEI_DocumentSubmissionTypeInfo, "Doc. Submission should be M – 通关无纸化 for two-step declaration clearance mode.");
			instruction1.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.Integrated;
			instruction1.CEI_DocumentSubmissionType = "X";
			AssertNoMessageErrorContaining(instruction1.CEI_DocumentSubmissionTypeInfo, "Doc. Submission should be M – 通关无纸化 for two-step declaration clearance mode.");
			instruction1.JobDeclaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
			instruction1.CEI_DocumentSubmissionType = "M";
			AssertNoMessageErrorContaining(instruction1.CEI_DocumentSubmissionTypeInfo, "Doc. Submission should be M – 通关无纸化 for two-step declaration clearance mode.");
		}

		public void TestCheckCEI_CIQRequires()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeType.ZZK_CodeType, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			anotherFactory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "B", tariff1);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "A", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005101000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "MED", tariff2);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "3005109000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "CFCS", tariff3);
			anotherFactory.Save();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoNotifications(instruction.CEI_CIQRequiresInfo);
			instruction.CEI_CIQRequires = true;
			declaration.JE_RL_NKPortOfLoading = "CNSHA";
			declaration.JE_RL_NKPortOfArrival = "CNBJS";
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.DomesticNotRequireCIQMessage);
			instruction.CEI_CIQRequires = false;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoNotifications(instruction.CEI_CIQRequiresInfo);
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			invoiceLine.JI_Tariff = "8476900000";
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasWarningContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasWarningContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyRequirementB.DocumentBRequireCIQMessage);
			invoiceLine.JI_Tariff = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoNotifications(instruction.CEI_CIQRequiresInfo);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_Tariff = "8476900000";
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyRequirementA.DocumentARequireCIQMessage);
			invoiceLine.JI_Tariff = ZString.Empty;
			instruction.CEI_Style = CNRefCusProcedure.Codes._4561;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyProcedureCode4561.Procedure4561RequireCIQMessage);
			invoiceLine.JI_Tariff = "3005101000";
			instruction.CEI_Style = CNRefCusProcedure.Codes._3612;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategy3612MED.Procedure3612MEDRequireCIQMessage);
			var cargoAttribute = Factory.New<CargoAttribute>();
			cargoAttribute.CY_ParentID = invoiceLine.PK;
			cargoAttribute.CY_ParentTableCode = invoiceLine.TablePrefix;
			cargoAttribute.CY_Type = Constants.CusCodeDataTypes.Codes.CargoAttribute;
			invoiceLine.CargoAttributes.Add(cargoAttribute);
			cargoAttribute.CY_Code = CargoAttributeList.Codes._21;
			invoiceLine.JI_Tariff = ZString.Empty;
			instruction.CEI_Style = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyCargoAttributes.CargoAttributesRequireCIQMessage);
			cargoAttribute.CY_Code = CargoAttributeList.Codes._11;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			cargoAttribute.CY_Code = CargoAttributeList.Codes._22;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyCargoAttributes.CargoAttributesRequireCIQMessage);
			cargoAttribute.CY_Code = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			invoiceLine.JI_Tariff = "3005109000";
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyCFCS.CFCSRequireCIQMessage);
			invoiceLine.JI_Tariff = ZString.Empty;
			var otherPackage = instruction.OtherPackages.AddNew();
			otherPackage.CY_Code = PackageType.Codes.WoodBox;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			otherPackage.CY_Code = PackageType.Codes.WoodBarrel;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			otherPackage.CY_Code = PackageType.Codes.NaturalWood;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			otherPackage.CY_Code = PackageType.Codes.PlantAuxiliaryPadMaterial;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			otherPackage.CY_Code = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			instruction.CEI_PackageUQ = PackageType.Codes.WoodBox;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			instruction.CEI_PackageUQ = PackageType.Codes.WoodBarrel;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			instruction.CEI_PackageUQ = PackageType.Codes.NaturalWood;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			instruction.CEI_PackageUQ = PackageType.Codes.PlantAuxiliaryPadMaterial;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyPackageType.PackageTypeRequireCIQMessage);
			instruction.CEI_PackageUQ = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.CusContainers.AddNew();
			var container = instruction.EntryHeader.Containers[0];
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyLCL.LCLRequireCIQMessage);
			container.CO_FCL_LCL_AIR = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			invoiceLine.JI_NameOfGoods = "氨";
			instruction.CEI_CIQRequires = false;
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, CusEntryInstructionValidation.GoodsRequireCIQMessage);
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, EntryRequiresCIQStrategyDangerousChemical.DangerousChemicalRequireCIQMessage);
			invoiceLine.JI_NameOfGoods = ZString.Empty;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertNoMessageErrors(instruction.CEI_CIQRequiresInfo);
			invoiceLine.JI_Tariff = "8476900000";
			otherPackage.CY_Code = PackageType.Codes.WoodBox;
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			instruction.Validation.ValidateCEI_CIQRequires();
			AssertHasMessageErrorContaining(instruction.CEI_CIQRequiresInfo, @"This Entry Instruction requires inspection and quarantine as,
       some tariffs requires supporting document A
       the type of package or other packages include wooden packaging
       some contains are LCL");
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}
}
