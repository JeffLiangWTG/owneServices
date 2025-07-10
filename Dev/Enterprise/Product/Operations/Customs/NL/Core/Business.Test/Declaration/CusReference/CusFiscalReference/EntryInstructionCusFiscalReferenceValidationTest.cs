using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(EntryInstructionCusFiscalReferenceValidation))]
class EntryInstructionCusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Code_F49()
	{
		var expectedErrorMessage = "No fiscal reference is allowed to be filled in due to additional procedure F49. Please remove the fiscal reference or change the additional procedure in the invoice line";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
		additionalProcedureCode.CY_Code = "XXXXF49";
		invoiceLine.JI_CEI = instruction.PK;
		fiscalReference.CFR_Code = "FR1";

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("If there is an additional procedure F49 in one of the linked invoice lines, there should be a message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);

			additionalProcedureCode.CY_Code = "XXXXF50";
			fiscalReference.CFR_Code = "FR2";
			AssertNoMessageErrorContaining("If there is no additional procedure F49 in one of the linked invoice lines, there should be a no message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);
		});
	}

	public void TestCheckCFR_Code_F48()
	{
		var expectedErrorMessage = "When entering role code FR5, all invoice lines must have additional procedure F48";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var additionalProcedureCode1 = invoiceLine1.AdditionalProcedureCodes.AddNew();
		additionalProcedureCode1.CY_Code = "XXXXF50";
		invoiceLine1.JI_CEI = instruction.PK;
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var additionalProcedureCode2 = invoiceLine2.AdditionalProcedureCodes.AddNew();
		additionalProcedureCode2.CY_Code = "XXXXF48";
		invoiceLine1.JI_CEI = instruction.PK;
		fiscalReference.CFR_Code = "FR5";

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("If not all linked invoice lines have additional procedure F48, there should be a message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);

			fiscalReference.CFR_Code = "FR2";
			AssertNoMessageErrorContaining("If the instruction type is not F5, there should be no message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);

			additionalProcedureCode1.CY_Code = "XXXXF48";
			fiscalReference.CFR_Code = "FR5";
			AssertNoMessageErrorContaining("If all linked invoice lines have additional procedure F48, there should be no message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);
		});
	}

	public void TestCheckCFR_Code_FR5_F48()
	{
		var expectedErrorMessage = "When all invoice lines have additional procedure F48, then role code FR5 must be used.";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var additionalProcedureCode1 = invoiceLine1.AdditionalProcedureCodes.AddNew();
		additionalProcedureCode1.CY_Code = "XXXXF48";
		invoiceLine1.JI_CEI = instruction.PK;
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var additionalProcedureCode2 = invoiceLine2.AdditionalProcedureCodes.AddNew();
		additionalProcedureCode2.CY_Code = "XXXXF48";
		invoiceLine1.JI_CEI = instruction.PK;
		fiscalReference.CFR_Code = "FR7";

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("If all linked invoice lines have additional procedure F48, there should be a message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);

			fiscalReference.CFR_Code = "FR5";
			AssertNoMessageErrorContaining("If the instruction type is F5, there should be no message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);

			additionalProcedureCode1.CY_Code = "XXXXF50";
			fiscalReference.CFR_Code = "FR7";
			AssertNoMessageErrorContaining("If not all linked invoice lines have additional procedure F48, there should be no message error.", fiscalReference.CFR_CodeInfo, expectedErrorMessage);
		});
	}

	public void TestCheckCFR_Reference()
	{
		var expectedErrorMessage = "Please fill in a Dutch VAT number starting with NL";
		fiscalReference.CFR_Code = "FR7";
		fiscalReference.CFR_Reference = "BE1234567";

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("If Code is FR7 and refrence does not contain a dutch vat number, an error message must be displayed", fiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			fiscalReference.CFR_Reference = "NL1234567";
			AssertNoMessageErrorContaining("If the reference starts with NL, there should be no message error.", fiscalReference.CFR_ReferenceInfo, expectedErrorMessage);

			fiscalReference.CFR_Code = "FR2";
			fiscalReference.CFR_Reference = "BE1234567";
			AssertNoMessageErrorContaining("If Code is different from FR7, there should be no message error.", fiscalReference.CFR_ReferenceInfo, expectedErrorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		fiscalReference = instruction.FiscalReferences.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction instruction;
	CusFiscalReference fiscalReference;
}
