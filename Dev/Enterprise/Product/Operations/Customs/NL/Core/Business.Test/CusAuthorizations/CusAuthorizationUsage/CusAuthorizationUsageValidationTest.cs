using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateTypeCodeAndReferenceNumberUnique()
	{
		var errorMessage = "[R9011] Combination of Type and Reference must be unique for authorizations.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var authorization_InvoiceLine1 = invoiceLine.CusAuthorizationUsages.AddNew();
		authorization_InvoiceLine1.AGC_Code = "TES";
		authorization_InvoiceLine1.AGC_Number = "123";

		var authorization_InvoiceLine2 = invoiceLine.CusAuthorizationUsages.AddNew();
		authorization_InvoiceLine2.AGC_Code = "TES";
		authorization_InvoiceLine2.AGC_Number = "123";

		CombineAssertions(() =>
		{
			AssertHasRowMessageError("Combination of Type and Reference under InvoiceLine isn't unique", authorization_InvoiceLine2, errorMessage);
			authorization_InvoiceLine2.AGC_Code = "NMB";
			AssertNoRowMessageError("Combination of Type and Reference under InvoiceLine is unique", authorization_InvoiceLine2, errorMessage);

			var authorization_EntryInstruction1 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorization_EntryInstruction1.AGC_Code = "TES";
			authorization_EntryInstruction1.AGC_Number = "123";

			var authorization_EntryInstruction2 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorization_EntryInstruction2.AGC_Code = "TES";
			authorization_EntryInstruction2.AGC_Number = "123";
			AssertNoRowMessageError("Parent is EntryInstruction, no row error", authorization_EntryInstruction2, errorMessage);
		});
	}

	public void TestCheckAGC_Code()
	{
		var expectedMessage = "DPO authorization and Method of Payment missing. Please fill one of these fields related to Duty payments";
		var expectedWarning = "Both DPO authorization and Method of payment are filled";

		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = prepareInvoiceLine(testDec);
		var cusEntryLineFee = invoiceLine.CusEntryLine.Fees[0];
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		invoiceLine.JI_FormattedProcedure = "000000";
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Number = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";

		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertHasRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		authorizationUsage.AGC_Code = "XXX";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertHasRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		invoiceLine.JI_FormattedProcedure = "510000";
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		invoiceLine.JI_FormattedProcedure = "530000";
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		invoiceLine.JI_FormattedProcedure = "710000";
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = "Test";
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		authorizationUsage.AGC_Number = ZString.Empty;
		cusEntryLineFee.CF_MethodOfPayment = "1";
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = ZString.Empty;
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertNoRowMessageError(authorizationUsage, expectedMessage);
		AssertNoRowWarningContaining(authorizationUsage, expectedWarning);

		authorizationUsage.ClearRowNotifications();

		testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		cusEntryLineFee.CF_MethodOfPayment = "1";
		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = "123";
		authorizationUsage.AGC_Code = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		authorizationUsage.Validation.ValidateAuthorizationUsage_Code();
		AssertHasRowWarning(authorizationUsage, expectedWarning);
	}

	public void TestR9010Validation()
	{
		var errMessage = "[R9010] Combination Type and Authorization number must be unique.";
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();

		var auth_EntryLine1 = entryInstruction.CusAuthorizationUsages.AddNew();
		auth_EntryLine1.AGC_Code = "DPO";
		auth_EntryLine1.AGC_Number = "123";

		var auth_EntryLine2 = entryInstruction.CusAuthorizationUsages.AddNew();
		auth_EntryLine2.AGC_Code = "DPO";
		auth_EntryLine2.AGC_Number = "123";

		CombineAssertions(() =>
		{
			AssertHasRowMessageError("Combination of Type and Reference under Entry Instruction is not unique", auth_EntryLine2, errMessage);

			// Change the AGC_Code to make the combination unique
			auth_EntryLine2.AGC_Code = "XMV";
			AssertNoRowMessageError("Combination of Type and Reference under Entry Instruction is unique", auth_EntryLine2, errMessage);

			// Change the AGC_Number to make the combination unique
			auth_EntryLine1.AGC_Code = "DPO";
			auth_EntryLine1.AGC_Number = "143";
			AssertNoRowMessageError("Combination of Type and Reference under Entry Instruction is unique", auth_EntryLine2, errMessage);
		});
	}

	JobComInvoiceLine prepareInvoiceLine(JobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		entryInstruction.CEI_JE = declaration.PK;
		invoiceLine.JI_CEI = entryInstruction.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.Fees.AddNew();

		return invoiceLine;
	}
}
