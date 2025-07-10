using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestCheckChargeType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var messageError = "Tax type is required";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = prepareInvoiceLineForRelatedIndicator(declaration);
		invoiceLine.RelatedIndicator = false;
		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = false;
		invoiceLine.RelatedIndicator4 = false;
		var cusEntryLineFee = invoiceLine.CusEntryLine.Fees[0];

		cusEntryLineFee.CF_ChargeType = "TAX";
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		cusEntryLineFee.CF_ChargeType = ZString.Empty;
		AssertHasMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		invoiceLine.RelatedIndicator = true;
		invoiceLine.RelatedIndicator2 = true;
		invoiceLine.RelatedIndicator3 = true;
		invoiceLine.RelatedIndicator4 = true;

		cusEntryLineFee.CF_ChargeType = "TAX";
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		cusEntryLineFee.CF_ChargeType = ZString.Empty;
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.RelatedIndicator = false;
		cusEntryLineFee.CF_ChargeType = ZString.Empty;
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		messageError = "Base Amount is required";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		cusEntryLineFee.CF_BaseValue = ZDecimal.Zero;
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertHasMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);
		cusEntryLineFee.CF_ChargeType = "B00";
		AssertHasMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		cusEntryLineFee.CF_BaseValue = 100;
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		cusEntryLineFee.CF_BaseValue = ZDecimal.Zero;
		cusEntryLineFee.CF_ChargeType = "XXX";
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		cusEntryLineFee.CF_BaseValue = ZDecimal.Zero;
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoMessageError(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		messageError = "Tax type A00 is not allowed for this procedure";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.RelatedIndicator = true;
		invoiceLine.RelatedIndicator2 = true;
		invoiceLine.RelatedIndicator3 = true;
		invoiceLine.RelatedIndicator4 = true;
		invoiceLine.JI_FormattedProcedure = "51XXXXX";
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		invoiceLine.JI_FormattedProcedure = "53XXXXX";
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		cusEntryLineFee.CF_ChargeType = "B00";
		AssertNoWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		invoiceLine.JI_FormattedProcedure = "XXXXXXX";
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertHasWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		invoiceLine.RelatedIndicator = false;
		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = false;
		invoiceLine.RelatedIndicator4 = false;
		invoiceLine.JI_FormattedProcedure = "51XXXXX";
		cusEntryLineFee.CF_ChargeType = "B00";
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.RelatedIndicator = true;
		invoiceLine.RelatedIndicator2 = true;
		invoiceLine.RelatedIndicator3 = true;
		invoiceLine.RelatedIndicator4 = true;
		invoiceLine.JI_FormattedProcedure = "51XXXXX";
		cusEntryLineFee.CF_ChargeType = "B00";
		cusEntryLineFee.CF_ChargeType = "A00";
		AssertNoWarning(cusEntryLineFee.CF_ChargeTypeInfo, messageError);
	}

	public void TestCheckMethodOfPayment()
	{
		var expectedMessage = "DPO authorization and Method of Payment missing. Please fill one of these fields related to Duty payments";
		var expectedWarning = "Both DPO authorization and Method of payment are filled";

		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = prepareInvoiceLineForRelatedIndicator(testDec);
		var cusEntryLineFee = invoiceLine.EntryInstruction.EntryHeader.AllEntryLines[0].Fees[0];
		invoiceLine.JI_FormattedProcedure = "000000";
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
		authorizationUsage.AGC_Number = ZString.Empty;
		authorizationUsage.AGC_Code = "DPO";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;

		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertHasRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		cusEntryLineFee.ClearRowNotifications();

		authorizationUsage.AGC_Code = "XXX";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertHasRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		cusEntryLineFee.ClearRowNotifications();

		invoiceLine.JI_FormattedProcedure = "510000";
		authorizationUsage.AGC_Code = "DPO";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		invoiceLine.JI_FormattedProcedure = "530000";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		invoiceLine.JI_FormattedProcedure = "710000";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = "Test";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		authorizationUsage.AGC_Number = ZString.Empty;
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = ZString.Empty;
		cusEntryLineFee.CF_MethodOfPayment = ZString.Empty;
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertNoRowMessageError(cusEntryLineFee, expectedMessage);
		AssertNoRowWarningContaining(cusEntryLineFee, expectedWarning);

		testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_FormattedProcedure = "000000";
		authorizationUsage.AGC_Number = "123";
		cusEntryLineFee.CF_MethodOfPayment = "1";
		cusEntryLineFee.Validation.ValidateMethodOfPayment();
		AssertHasRowWarningContaining(cusEntryLineFee, expectedWarning);

		invoiceLine.EntryInstruction.Delete();
		AssertNoExceptionThrown(() => cusEntryLineFee.Validation.ValidateMethodOfPayment());
	}

	public override void TestCheckCF_ChargeType()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = prepareInvoiceLineForRelatedIndicator(testDec);
		var cusEntryLineFee = invoiceLine.EntryInstruction.EntryHeader.AllEntryLines[0].Fees[0];

		cusEntryLineFee.CF_ChargeType = ZString.Empty;
		AssertHasMessageErrorContaining(cusEntryLineFee.CF_ChargeTypeInfo, "You have not entered a [UCC 4/3] Type.");
		cusEntryLineFee.CF_ChargeType = "INV";
		AssertHasMessageErrorContaining(cusEntryLineFee.CF_ChargeTypeInfo, "The code you have selected is not in the list.");
	}

	JobComInvoiceLine prepareInvoiceLineForRelatedIndicator(JobDeclaration declaration)
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
