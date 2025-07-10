using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportCusEntryLineFeeMethodOfPaymentDefaultingTest : TestCaseWithFactory
{
	public void TestDefaulting_WhenChargeTypeIsDutyStartingWithA_ProcedureAttributeIsFound()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "ANN";
		AssertEquals("Method of payment", "X", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsDutyStartingWith27_ProcedureAttributeIsFound()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "27N";
		AssertEquals("Method of payment", "X", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsVatStartingWithB_ProcedureAttributeIsFound()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "BXX";
		AssertEquals("Method of payment", "Y", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsVatStartingWith4_ProcedureAttributeIsFound()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "4XX";
		AssertEquals("Method of payment", "Y", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsOtherFee_ProcedureAttributeIsFound()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "Z", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsExcludedVat()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "423";
		AssertEquals("Even though a procedure attribute is found, Method of payment", "A", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsExcludedPortTax()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "9AA";
		AssertEquals("Even though a procedure attribute is found, Method of payment", "A", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsTemporaryAntiDumping()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "A35";
		AssertEquals("Even though a procedure attribute is found, Method of payment", "R", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsTemporaryCountervailing()
	{
		SetUpProcedureWithAttributes();
		Factory.Save();

		invoiceLine.JI_Procedure = "0000";
		lineFee.CF_ChargeType = "A45";
		AssertEquals("Even though a procedure attribute is found, Method of payment", "R", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsEmpty()
	{
		declaration.JE_DefermentAccountNumber = "";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "A", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsEqualToDatCode()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("DAT", "DAT123", "IT");
		declaration.JE_OH_Importer = orgHeader.PK;
		declaration.JE_DefermentAccountNumber = "DAT123";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "D", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmptyAndChargeTypeIsDuty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "ANN";
		AssertEquals("Method of payment", "E", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmptyAndChargeTypeIsSanMarinoDuty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "27N";
		AssertEquals("Method of payment", "E", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmpty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "G", lineFee.CF_MethodOfPayment);
	}

	void SetUpProcedureWithAttributes()
	{
		var testDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = testDataHelper.CreateRefCusProcedure("IT", "IM", "00", "00", "", "Procedure with attribute", "IMP");
		testDataHelper.CreateRefCusProcedureAttribute(procedure.PK, "DTYPaymentMethod", "X");
		testDataHelper.CreateRefCusProcedureAttribute(procedure.PK, "VATPaymentMethod", "Y");
		testDataHelper.CreateRefCusProcedureAttribute(procedure.PK, "OTHPaymentMethod", "Z");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		lineFee = entryLine.Fees.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLineFee lineFee;
}
