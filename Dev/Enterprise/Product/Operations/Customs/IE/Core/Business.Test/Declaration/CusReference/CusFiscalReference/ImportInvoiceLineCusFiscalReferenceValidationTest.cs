using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using FiscalReferenceCodeList = Enterprise.Customs.EU.Business.FiscalReferenceCodeList;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportInvoiceLineCusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRuleBR600010()
		{
			var fiscalReferenceErrorMessage = "[BR600010] An invoice line can have none or just one IOSS (FR5) Fiscal Reference declared.";
			(var testInvoiceLine, var testInvoice, var testDeclaration) = SetupData(IEJobMessageTypeList.Codes.Import);
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_JE = testDeclaration.PK;
			testInvoiceLine.JI_CEI = testInstruction.PK;

			var testInvoiceLineFiscalReference1 = testInvoiceLine.FiscalReferences.AddNew();
			var testInvoiceLineFiscalReference2 = testInvoiceLine.FiscalReferences.AddNew();
			var testInvoiceLineFiscalReference3 = testInvoiceLine.FiscalReferences.AddNew();

			testInvoiceLineFiscalReference1.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			testInvoiceLineFiscalReference2.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			testInvoiceLineFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;

			CombineAssertions("When only one FiscalReference has CFR_Code == FR5", () =>
			{
				testInvoiceLineFiscalReference1.Validation.ValidateCFR_Code();
				AssertNoMessageError("testInvoiceLineFiscalReference1 should not have error msg", testInvoiceLineFiscalReference1.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertNoMessageError("testInvoiceLineFiscalReference2 should not have error msg", testInvoiceLineFiscalReference2.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertNoMessageError("testInvoiceLineFiscalReference3 should not have error msg", testInvoiceLineFiscalReference3.CFR_CodeInfo, fiscalReferenceErrorMessage);
			});

			testInvoiceLineFiscalReference3.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			CombineAssertions("When more than one FiscalReferences have CFR_Code == FR5", () =>
			{
				testInvoiceLineFiscalReference1.Validation.ValidateCFR_Code();
				AssertHasMessageError("testInvoiceLineFiscalReference1 should have error msg", testInvoiceLineFiscalReference1.CFR_CodeInfo, fiscalReferenceErrorMessage);
				testInvoiceLineFiscalReference2.Validation.ValidateCFR_Code();
				AssertNoMessageError("testInvoiceLineFiscalReference2 should not have error msg", testInvoiceLineFiscalReference2.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertHasMessageError("testInvoiceLineFiscalReference3 should have error msg", testInvoiceLineFiscalReference3.CFR_CodeInfo, fiscalReferenceErrorMessage);
			});
		}

		public void TestValidateRuleBR600011()
		{
			var fiscalReferenceErrorMessage = "[BR600011] Either all invoice lines have just one IOSS (FR5) Fiscal Reference declared or none.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_JE = testDeclaration.PK;

			var testInvoice = testDeclaration.Invoices.AddNew();

			var testInvoiceLine1 = testInvoice.JobComInvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInstruction.PK;
			var testInvoiceLine2 = testInvoice.JobComInvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInstruction.PK;

			var testInvoiceLine1FiscalReference = testInvoiceLine1.FiscalReferences.AddNew();
			var testInvoiceLine2FiscalReference = testInvoiceLine2.FiscalReferences.AddNew();

			testInvoiceLine1FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
			testInvoiceLine2FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			CombineAssertions("When no invoice line linked to same instruction has FiscalReference with CFR_Code == FR5", () =>
			{
				testInvoiceLine1FiscalReference.Validation.ValidateCFR_Code();
				AssertNoMessageError("testInvoiceLine1FiscalReference should not have error message", testInvoiceLine1FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertNoMessageError("testInvoiceLine2FiscalReference should not have error message", testInvoiceLine2FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
			});

			testInvoiceLine1FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			testInvoiceLine2FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			CombineAssertions("When all invoice lines linked to same instruction doesn't have one FiscalReference with CFR_Code == FR5", () =>
			{
				testInvoiceLine1FiscalReference.Validation.ValidateCFR_Code();
				AssertHasMessageError("testInvoiceLine1FiscalReference should have error message", testInvoiceLine1FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertNoMessageError("testInvoiceLine2FiscalReference should not have error message", testInvoiceLine2FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
			});

			testInvoiceLine1FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			testInvoiceLine2FiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR5_Vendor;
			CombineAssertions("When all invoice lines linked to same instruction have one FiscalReference with CFR_Code == FR5", () =>
			{
				testInvoiceLine1FiscalReference.Validation.ValidateCFR_Code();
				testInvoiceLine2FiscalReference.Validation.ValidateCFR_Code();
				AssertNoMessageError("testInvoiceLine1FiscalReference should not have error message", testInvoiceLine1FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
				AssertNoMessageError("testInvoiceLine2FiscalReference should not have error message", testInvoiceLine2FiscalReference.CFR_CodeInfo, fiscalReferenceErrorMessage);
			});
		}

		(JobComInvoiceLine invoiceLine, JobComInvoiceHeader invoice, JobDeclaration declaration) SetupData(string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return (invoiceLine, invoice, declaration);
		}
	}
}
