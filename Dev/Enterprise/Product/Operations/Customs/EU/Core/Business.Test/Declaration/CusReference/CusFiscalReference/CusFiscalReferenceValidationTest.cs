using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRuleR0010()
		{
			var duplicateFiscalReferenceMessage = "[R0010] Value can’t be entered in both Entry Instruction and Invoice lines.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_JE = testDeclaration.PK;
			var testInstructionFiscalReference1 = testInstruction1.FiscalReferences.AddNew();
			var testInstructionFiscalReference2 = testInstruction1.FiscalReferences.AddNew();
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine1 = testInvoice.JobComInvoiceLines.AddNew();
			var testInvoiceLine2 = testInvoice.JobComInvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInstruction1.PK;
			testInvoiceLine2.JI_CEI = testInstruction1.PK;
			var testInvoiceLineFiscalReference1 = testInvoiceLine1.FiscalReferences.AddNew();
			var testInvoiceLineFiscalReference2 = testInvoiceLine1.FiscalReferences.AddNew();
			var testInvoiceLineFiscalReference3 = testInvoiceLine2.FiscalReferences.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(testDeclaration, true))
			{
				testInstructionFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				testInstructionFiscalReference1.CFR_Reference = "001";

				testInvoiceLineFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				testInvoiceLineFiscalReference1.CFR_Reference = "001";
				testInvoiceLineFiscalReference1.Validation.ValidateAll();

				testDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				CombineAssertions("R0010 : declaration is UCC6 and Export", () =>
				{
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference1, duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference1.CFR_Reference = "002";
					testInvoiceLineFiscalReference1.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference1, duplicateFiscalReferenceMessage);
				});

				testDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				CombineAssertions("R0010 : declaration is UCC6 and Import", () =>
				{
					testInstructionFiscalReference1.CFR_Reference = "002";
					testInstructionFiscalReference1.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(testInstructionFiscalReference1, duplicateFiscalReferenceMessage);

					testInstructionFiscalReference1.RemoveRowMessageError(duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference3.CFR_Reference = FiscalReferenceCodeList.Codes.FR1_Importer;
					testInvoiceLineFiscalReference3.CFR_Reference = "002";
					testInstructionFiscalReference1.Validation.ValidateAll();

					AssertEquals(testInstructionFiscalReference1.GetMessageErrors().Count(), 1);

					testInvoiceLineFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
					testInvoiceLineFiscalReference2.CFR_Reference = "001";
					testInvoiceLineFiscalReference2.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference2, duplicateFiscalReferenceMessage);

					testInstructionFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
					testInstructionFiscalReference2.CFR_Reference = "001";
					testInstructionFiscalReference2.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(testInstructionFiscalReference2, duplicateFiscalReferenceMessage);

					testInstructionFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
					testInstructionFiscalReference2.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInstructionFiscalReference2, duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
					testInvoiceLineFiscalReference3.CFR_Reference = "001";
					testInvoiceLineFiscalReference3.Validation.ValidateAll();
					AssertHasRowMessageErrorContaining(testInvoiceLineFiscalReference3, duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
					testInvoiceLineFiscalReference3.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference3, duplicateFiscalReferenceMessage);
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(testDeclaration, false))
			{
				testInstructionFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				testInstructionFiscalReference1.CFR_Reference = "001";

				testInvoiceLineFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				testInvoiceLineFiscalReference1.CFR_Reference = "001";
				testInvoiceLineFiscalReference1.Validation.ValidateAll();

				testDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				CombineAssertions("R0010 : declaration is not UCC6 and Export", () =>
				{
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference1, duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference1.CFR_Reference = "002";
					testInvoiceLineFiscalReference1.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference1, duplicateFiscalReferenceMessage);
				});

				testDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				CombineAssertions("R0010 : declaration is not UCC6 and Import", () =>
				{
					testInstructionFiscalReference1.CFR_Reference = "002";
					testInstructionFiscalReference1.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInstructionFiscalReference1, duplicateFiscalReferenceMessage);

					testInstructionFiscalReference1.RemoveRowMessageError(duplicateFiscalReferenceMessage);

					testInvoiceLineFiscalReference3.CFR_Reference = FiscalReferenceCodeList.Codes.FR1_Importer;
					testInvoiceLineFiscalReference3.CFR_Reference = "002";
					testInstructionFiscalReference1.Validation.ValidateAll();

					testInvoiceLineFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
					testInvoiceLineFiscalReference2.CFR_Reference = "001";
					testInvoiceLineFiscalReference2.Validation.ValidateAll();
					AssertNoRowMessageErrorContaining(testInvoiceLineFiscalReference2, duplicateFiscalReferenceMessage);
				});
			}
		}

		public void TestValidateFiscalReferenceAvailability()
		{
			var errorMessageForInstruction = "Entry Instruction Additional Fiscal References are only required when Dataset (Declaration Type) is H1, H6, H7 or I1.";
			var errorMessageForInvLine = "Invoice Line Additional Fiscal References are only required when Dataset (Declaration Type) is H1 or I1.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_JE = testDeclaration.PK;
			var testInstructionFiscalReference = testInstruction1.FiscalReferences.AddNew();
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine1 = testInvoice.JobComInvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInstruction1.PK;
			var testInvoiceLineFiscalReference = testInvoiceLine1.FiscalReferences.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(testDeclaration, true))
			{
				CombineAssertions(() =>
				{
					testInstruction1.CEI_Style = "H2";
					testInstructionFiscalReference.CFR_Code = "FR1";
					testInvoiceLineFiscalReference.CFR_Code = "FR1";
					AssertHasWarningContaining("Declaration Type is H2, Fiscal Reference in Instruction", testInstructionFiscalReference.CFR_CodeInfo, errorMessageForInstruction);
					AssertHasWarningContaining("Declaration Type is H2, Fiscal Reference in Invoice Line", testInvoiceLineFiscalReference.CFR_CodeInfo, errorMessageForInvLine);

					testInstruction1.CEI_Style = "H1";
					testInstructionFiscalReference.Validation.ValidateCFR_Code();
					testInvoiceLineFiscalReference.Validation.ValidateCFR_Code();
					AssertNoWarningContaining("Declaration Type is H1, Fiscal Reference in Instruction", testInstructionFiscalReference.CFR_CodeInfo, errorMessageForInstruction);
					AssertNoWarningContaining("Declaration Type is H1, Fiscal Reference in Invoice Line", testInvoiceLineFiscalReference.CFR_CodeInfo, errorMessageForInvLine);

					testInstruction1.CEI_Style = "H6";
					testInstructionFiscalReference.Validation.ValidateCFR_Code();
					testInvoiceLineFiscalReference.Validation.ValidateCFR_Code();
					AssertNoWarningContaining("Declaration Type is H6, Fiscal Reference in Instruction", testInstructionFiscalReference.CFR_CodeInfo, errorMessageForInstruction);
					AssertHasWarningContaining("Declaration Type is H6, Fiscal Reference in Invoice Line", testInvoiceLineFiscalReference.CFR_CodeInfo, errorMessageForInvLine);

					testInstruction1.CEI_Style = "H7";
					testInstructionFiscalReference.Validation.ValidateCFR_Code();
					testInvoiceLineFiscalReference.Validation.ValidateCFR_Code();
					AssertNoWarningContaining("Declaration Type is H7, Fiscal Reference in Instruction", testInstructionFiscalReference.CFR_CodeInfo, errorMessageForInstruction);
					AssertHasWarningContaining("Declaration Type is H7, Fiscal Reference in Invoice Line", testInvoiceLineFiscalReference.CFR_CodeInfo, errorMessageForInvLine);

					testInstruction1.CEI_Style = "I1";
					testInstructionFiscalReference.Validation.ValidateCFR_Code();
					testInvoiceLineFiscalReference.Validation.ValidateCFR_Code();
					AssertNoWarningContaining("Declaration Type is I1, Fiscal Reference in Instruction", testInstructionFiscalReference.CFR_CodeInfo, errorMessageForInstruction);
					AssertNoWarningContaining("Declaration Type is I1, Fiscal Reference in Invoice Line", testInvoiceLineFiscalReference.CFR_CodeInfo, errorMessageForInvLine);
				});
			}
		}

		public void TestCheckCFR_Code_If_FR3()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false, MessageTypeList.Codes.Import);
			GenerateProcedure("Ye", YesNoList.Codes.No, YesNoList.Codes.Yes, "Yes description", true, MessageTypeList.Codes.Import);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceline = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceline.JI_Procedure = "Ye12367";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = false", !invoiceline.HasAnyProcedureWithSuspendedVat);

			var invoiceline2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline2.JI_Procedure = "No12367";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = true", invoiceline2.HasAnyProcedureWithSuspendedVat);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryheader = declaration.CustomsEntryHeaders[0];
			entryheader.CH_CEI_Instruction = entryInstruction.PK;
			var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();

			Assert(!entryheader.IsAllEntryLinesVatSuspended);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			AssertNoMessageError(cusFiscalReference.CFR_CodeInfo, "VAT is suspended on all entry lines; FR3 is likely not necessary.");

			invoiceline.JI_Procedure = "No12367";
			Assert(entryheader.IsAllEntryLinesVatSuspended);

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			AssertHasMessageError(cusFiscalReference.CFR_CodeInfo, "VAT is suspended on all entry lines; FR3 is likely not necessary.");

			cusFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			AssertNoMessageError(cusFiscalReference.CFR_CodeInfo, "VAT is suspended on all entry lines; FR3 is likely not necessary.");
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, CargoWise.Types.ZBool isCalculeVAT, string messageType)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_ShipmentType = messageType;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Concession = "367";
			procedure.ZZ6_Description = "description";
			procedure.ZZ6_CalculateVAT = isCalculeVAT;
			return procedure;
		}
	}
}
