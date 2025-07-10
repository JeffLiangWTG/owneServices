using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Reference_EntryInstruction_WithProceduresNotRequiringVAT_ForImportDeclaration()
	{
		const string expectedErrorMessage = "For Procedure 42, Reference field must not start with 'IT'.";
		SetUpRefData();

		CombineAssertions("Import declaration, with entry instruction", () =>
		{
			cusEntryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.ReleaseForFreeCirculationOfGoodsForConsumption42;

			cusInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Buyer;
			cusInstructionFiscalReference.CFR_Reference = "IT2345";
			AssertHasMessageError("When procedure = 42, code = 'FR2', reference starts with IT", cusInstructionFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			cusInstructionFiscalReference.CFR_Reference = "AU2345";
			AssertNoMessageError("When procedure = 42, code = 'FR2', reference starts with AU", cusInstructionFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			cusInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Importer;
			cusInstructionFiscalReference.CFR_Reference = "IT2345";
			AssertNoMessageError("When procedure = 42, code = 'FR1', reference starts with IT", cusInstructionFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			cusEntryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.IntoTemporaryImport;

			cusInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Buyer;
			cusInstructionFiscalReference.CFR_Reference = "IT2345";
			AssertNoNotifications(cusInstructionFiscalReference.CFR_ReferenceInfo);
		});
	}

	public void TestCheckCFR_Reference_InvoiceLines_WithProceduresNotRequiringVAT_ForImportDeclaration()
	{
		const string expectedErrorMessage = "For Procedure 63, Reference field must not start with 'IT'.";
		SetUpRefData();

		CombineAssertions("Import declaration, with invoice lines", () =>
		{
			cusEntryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.ReimportationWithSimultaneousReleaseForConsumption63;

			invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Buyer;
			invoiceLineFiscalReference.CFR_Reference = "IT2345";
			AssertHasMessageError("When procedure = 63, code = 'FR2', reference starts with IT", invoiceLineFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			invoiceLineFiscalReference.CFR_Reference = "AU2345";
			AssertNoMessageError("When procedure = 63, code = 'FR2', reference starts with AU", invoiceLineFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Importer;
			invoiceLineFiscalReference.CFR_Reference = "IT2345";
			AssertNoMessageError("When procedure = 63, code = 'FR1', reference starts with IT", invoiceLineFiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			cusEntryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.IntoTemporaryImport;

			invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.Buyer;
			invoiceLineFiscalReference.CFR_Reference = "IT2345";
			AssertNoNotifications(cusInstructionFiscalReference.CFR_ReferenceInfo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

		cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
		cusInstructionFiscalReference = cusEntryInstruction.FiscalReferences.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLineFiscalReference = invoiceLine.FiscalReferences.AddNew();
	}

	void SetUpRefData()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.SetUpRefDataWithVATRequirement();
	}

	CusEntryInstruction cusEntryInstruction;
	CusFiscalReference cusInstructionFiscalReference;
	CusFiscalReference invoiceLineFiscalReference;
}

