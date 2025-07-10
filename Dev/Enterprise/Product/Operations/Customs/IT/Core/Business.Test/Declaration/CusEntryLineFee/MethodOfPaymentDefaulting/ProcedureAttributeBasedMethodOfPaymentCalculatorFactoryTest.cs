using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ProcedureAttributeBasedMethodOfPaymentCalculatorFactoryTest : TestCaseWithFactory
{
	public void TestGetNew_WhenLineFeeIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When lineFee is null", () => ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(lineFee: null));
	}

	public void TestGetNew_WhenDeclarationIsNull()
	{
		var lineFee = Factory.New<CusEntryLine>().Fees.AddNew();
		var methodOfPaymentCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(lineFee);
		CombineAssertions(() =>
		{
			AssertEquals("Calculator Type", EmptyMethodOfPaymentCalculatorFullName, methodOfPaymentCalculator.GetType().FullName);
			AssertEquals("Default Value", "", methodOfPaymentCalculator.GetDefaultValue());
		});
	}

	public void TestGetNew_WhenDeclarationIsNotImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "XXX";
		var lineFee = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew().Fees.AddNew();
		var methodOfPaymentCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(lineFee);
		CombineAssertions(() =>
		{
			AssertEquals("Calculator Type", EmptyMethodOfPaymentCalculatorFullName, methodOfPaymentCalculator.GetType().FullName);
			AssertEquals("Default Value", "", methodOfPaymentCalculator.GetDefaultValue());
		});
	}

	public void TestGetNew_WhenDeclarationIsImport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var lineFee = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew().Fees.AddNew();
		lineFee.CF_ChargeType = "ANN";
		var methodOfPaymentCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(lineFee);
		AssertType<ProcedureAttributeBasedMethodOfPaymentCalculator>("Calculator Type", methodOfPaymentCalculator);
	}

	public void TestGetNew_WhenDeclarationIsImportAndChargeTypeIsNotApplicable()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		var notApplicableChargeTypes = new string[] { "", "A35", "A45", "423", "430", "445", "9AA", "9AB" };

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		CombineAssertions(() =>
		{
			foreach (var chargeType in notApplicableChargeTypes)
			{
				var lineFee = entryLine.Fees.AddNew();
				lineFee.CF_ChargeType = chargeType;
				var methodOfPaymentCalculator = ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(lineFee);
				AssertEquals($"For charge type '{chargeType}', Calculator Type", EmptyMethodOfPaymentCalculatorFullName, methodOfPaymentCalculator.GetType().FullName);
				AssertEquals($"For charge type '{chargeType}', Default Value", "", methodOfPaymentCalculator.GetDefaultValue());
			}
		});
	}

	public void TestGetDefaultValue_DutyPaymentMethod()
	{
		SetUpProceduresWithAttributes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4500";
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		CombineAssertions(() =>
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "A00";
			AssertEquals("For DTYPaymentMethod, Defaulting MethodOfPayment", "D", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4500F06";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "A00";
			AssertEquals("For DTYPaymentMethod, when concession is not empty in Procedure, Defaulting MethodOfPayment", "D", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4522";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "A00";
			AssertEquals("For DTYPaymentMethod, when no Attribute with PreviousProcedureCode match, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "7100";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "A00";
			AssertEquals("When no attributes present, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);
		});
	}

	public void TestGetDefaultValue_VatPaymentMethod()
	{
		SetUpProceduresWithAttributes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4500";
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		CombineAssertions(() =>
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For VATPaymentMethod, Defaulting MethodOfPayment", "V", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4522";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For VATPaymentMethod, Defaulting MethodOfPayment", "V", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4500F06";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For VATPaymentMethod, Defaulting MethodOfPayment", "V", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "7100";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("When no attributes present, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);
		});
	}

	public void TestGetDefaultValue_OtherFeePaymentMethod()
	{
		SetUpProceduresWithAttributes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4500";
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		CombineAssertions(() =>
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "123";
			AssertEquals("For OTHPaymentMethod, Defaulting MethodOfPayment", "O", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4522";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "123";
			AssertEquals("For OTHPaymentMethod, Defaulting MethodOfPayment", "O", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "4500F06";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "123";
			AssertEquals("For OTHPaymentMethod, Defaulting MethodOfPayment", "O", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "7100";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "123";
			AssertEquals("When no attributes present, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);
		});
	}

	public void TestGetDefaultValue_Procedure()
	{
		SetUpProceduresWithAttributes();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "4500";
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);

		CombineAssertions(() =>
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For valid Procedure, Defaulting MethodOfPayment", "V", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "9900";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For invalid Procedure, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);

			invoiceLine.JI_Procedure = "";
			lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "B00";
			AssertEquals("For empty Procedure, Defaulting MethodOfPayment", "A", lineFee.CF_MethodOfPayment);
		});
	}

	void SetUpProceduresWithAttributes()
	{
		var testDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure4500 = testDataHelper.CreateRefCusProcedure("IT", "IM", "45", "00", "", "Procedure 4500", "IMP");
		var procedure4500F06 = testDataHelper.CreateRefCusProcedure("IT", "IM", "45", "00", "F06", "Procedure 4500F06", "IMP");
		testDataHelper.CreateRefCusProcedure("IT", "IM", "45", "22", "", "Procedure 4522", "IMP");
		testDataHelper.CreateRefCusProcedure("IT", "IM", "71", "00", "", "Procedure 7100", "IMP");
		testDataHelper.CreateRefCusProcedureAttribute(procedure4500.PK, "DTYPaymentMethod", "D");
		testDataHelper.CreateRefCusProcedureAttribute(procedure4500.PK, "VATPaymentMethod", "V");
		testDataHelper.CreateRefCusProcedureAttribute(procedure4500.PK, "OTHPaymentMethod", "O");
		testDataHelper.CreateRefCusProcedureAttribute(procedure4500F06.PK, "DTYPaymentMethod", "F");
	}

	const string EmptyMethodOfPaymentCalculatorFullName = "Enterprise.Customs.IT.Business.Declaration.ProcedureAttributeBasedMethodOfPaymentCalculator+EmptyMethodOfPaymentCalculator";
}
