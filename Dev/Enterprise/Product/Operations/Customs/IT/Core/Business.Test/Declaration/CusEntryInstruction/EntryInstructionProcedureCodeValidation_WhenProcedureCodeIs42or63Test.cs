using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryInstructionProcedureCodeValidation_WhenProcedureCodeIs42or63Test : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new CusEntryInstructionProcedureValidator(null));
	}

	public void TestMustHaveFR2Code()
	{
		const string expectedMessageErrorFR2 = "When CPC is 42 or 63, 'FR2' code must exist in 'Fiscal References' Tab in Entry Instruction or in all linked 'Invoice Lines' Tab.";

		AssertEquals("[PRE-CONDITION] EntryInstruction-> Fiscal References count", 0, entryInstruction.FiscalReferences.Count);

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "42";
			AssertHasMessageErrorContaining("When Procedure is 42 and No fiscal references (FR2)", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR2);

			entryInstruction.CEI_Procedure = "40";
			AssertNoMessageErrorContaining("When Procedure is not 42 or 63", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR2);

			entryInstruction.FiscalReferences.AddNew().CFR_Code = "FR2";
			entryInstruction.CEI_Procedure = "42";
			AssertNoMessageErrorContaining("When Procedure is 42 and There are at least on FR2 fiscal reference", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR2);

			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR2";
			entryInstruction.CEI_Procedure = "63";
			AssertNoMessageErrorContaining("When Procedure is 63 and there are at least FR2 fiscal reference", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR2);
		});
	}

	public void TestMustHaveFR1orFR3Code()
	{
		const string expectedMessageErrorFR1orFR3 = "When CPC is 42 or 63, 'FR1' or 'FR3' code must exist in 'Fiscal References' Tab in Entry Instruction or in all linked 'Invoice Lines' Tab.";

		AssertEquals("[PRE-CONDITION] EntryInstruction-> Fiscal References count", 0, entryInstruction.FiscalReferences.Count);

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "42";
			AssertHasMessageErrorContaining("When Procedure is 42 and No fiscal references (FR1, FR3)", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR1orFR3);

			entryInstruction.CEI_Procedure = "40";
			AssertNoMessageErrorContaining("When Procedure is not 42 or 63", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR1orFR3);

			entryInstruction.FiscalReferences.AddNew().CFR_Code = "FR1";
			entryInstruction.CEI_Procedure = "42";
			AssertNoMessageErrorContaining("When Procedure is 42, There are at least on FR1 fiscal reference", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR1orFR3);

			entryInstruction.FiscalReferences.RemoveAndDeleteAll();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR3";
			entryInstruction.CEI_Procedure = "63";
			AssertNoMessageErrorContaining("When Procedure is 63 and there are at least on FR1 or FR3 fiscal reference", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR1orFR3);
		});
	}

	public void TestFRxCodeMustNotExistAtBothLevel()
	{
		const string expectedMessageErrorFR2 = "When CPC is 42 or 63, 'FR2' code must exist in 'Fiscal References' Tab in Entry Instruction or in all linked 'Invoice Lines' Tab.";
		const string expectedMessageErrorFR1 = "When CPC is 42 or 63, 'FR1' code must exist in 'Fiscal References' Tab in Entry Instruction or in all linked 'Invoice Lines' Tab";

		entryInstruction.CEI_Procedure = "42";
		entryInstruction.FiscalReferences.AddNew().CFR_Code = "FR1";
		entryInstruction.FiscalReferences.AddNew().CFR_Code = "FR2";

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR1";
		invoiceLine.FiscalReferences.AddNew().CFR_Code = "FR2";

		entryInstruction.Validation.ValidateCEI_Procedure();
		CombineAssertions("When Procedure is 42", () =>
		{
			AssertHasMessageErrorContaining("FR2 fiscal reference exists in both Instruction and Entry Live level", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR2);
			AssertHasMessageErrorContaining("FR3 fiscal reference exists in both Instruction and Entry Live level", entryInstruction.CEI_ProcedureInfo, expectedMessageErrorFR1);
		});
	}

	public void TestFiscalReferenceAtLineLevelMustHaveAtLeastDifferentReference()
	{
		var expectedMessageError = "When CPC is 42 or 63, all linked Invoice Lines must not have the same 'Reference' for 'FR1' code in their 'Fiscal References' Tab.";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			var fr1Code1 = invoiceLine.FiscalReferences.AddNew();
			fr1Code1.CFR_Code = "FR1";
			fr1Code1.CFR_Reference = "FR1R";

			entryInstruction.CEI_Procedure = "42";
			AssertHasMessageErrorContaining("When Procedure code is 42 and all Inv. Lines have the same FR1 code and reference", entryInstruction.CEI_ProcedureInfo, expectedMessageError);

			entryInstruction.CEI_Procedure = "40";
			AssertNoMessageErrorContaining("When Procedure code is not 40 and all Inv. Lines have the same FR1 code and reference", entryInstruction.CEI_ProcedureInfo, expectedMessageError);

			var fr1Code2 = invoiceLine.FiscalReferences.AddNew();
			fr1Code2.CFR_Code = "FR1";
			fr1Code2.CFR_Reference = "FR1T";
			entryInstruction.CEI_Procedure = "42";
			AssertNoMessageErrorContaining("When Procedure code is 42 and all Inv. Lines has have the same FR1 code and reference", entryInstruction.CEI_ProcedureInfo, expectedMessageError);

			fr1Code2.CFR_Reference = "FR1R";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var fr1Code3 = invoiceLine2.FiscalReferences.AddNew();
			fr1Code3.CFR_Code = "FR1";
			fr1Code3.CFR_Reference = "FR1R";
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageErrorContaining("When Procedure code is 42 and all Inv. Lines have the same FR1 code and reference", entryInstruction.CEI_ProcedureInfo, expectedMessageError);

			var fr1Code4 = invoiceLine2.FiscalReferences.AddNew();
			fr1Code4.CFR_Code = "FR1";
			fr1Code4.CFR_Reference = "";
			entryInstruction.Validation.ValidateCEI_Procedure();
			AssertHasMessageErrorContaining("When Procedure code is 42 and all Inv. Lines have the same FR1 code and reference", entryInstruction.CEI_ProcedureInfo, expectedMessageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
}
