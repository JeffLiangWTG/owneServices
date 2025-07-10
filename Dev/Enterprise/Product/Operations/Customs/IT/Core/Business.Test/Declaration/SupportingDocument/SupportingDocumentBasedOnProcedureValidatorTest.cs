using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentBasedOnProcedureValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new SupportingDocumentBasedOnProcedureValidator(null));
	}

	public void TestCheckCode_WhenProcedureIs42_63_AtJobLevel()
	{
		const string expectedMessageError = "When at least one Entry Instruction with Procedure Code is '42' or '63' and Declaration Type is 'H1' or 'H5', the document types 'Y040', 'Y041' and 'Y042' must not be used";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var supportingDocument = declaration.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = "Y040";
			AssertNoMessageErrorContaining("When Declaration has not Entry Instructions, Code: Y040", supportingDocument.CSI_CodeInfo, expectedMessageError);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_Procedure = "42";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("When Declaration has at least one Instruction (Style: H1, Procedure: 42), Code: Y040", supportingDocument.CSI_CodeInfo, expectedMessageError);

			supportingDocument.CSI_Code = "Z040";
			AssertNoMessageErrorContaining("When Declaration has at least one Instruction (Style: H1, Procedure: 42), Code: Z040", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Procedure = "40";
			supportingDocument.CSI_Code = "Y040";
			AssertNoMessageErrorContaining("When Declaration has at least one Instruction (Style: H1, Procedure: 40), Code: Y040", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H2";
			entryInstruction.CEI_Procedure = "42";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining("When Declaration has at least one Instruction (Style: H2, Procedure: 42), Code: Y040", supportingDocument.CSI_CodeInfo, expectedMessageError);
		});
	}

	public void TestCheckCode_WhenProcedureIs42_63_AtInvoiceLevel()
	{
		const string expectedMessageError = "When at least one Entry Instruction with Procedure Code is '42' or '63' and Declaration Type is 'H1' or 'H5', the document types 'Y040', 'Y041' and 'Y042' must not be used";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var supportingDocument = invoice.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = "Y041";
			AssertNoMessageErrorContaining("When Invoice has not related Entry Instructions, Code: Y041", supportingDocument.CSI_CodeInfo, expectedMessageError);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			entryInstruction.CEI_Style = "H5";
			entryInstruction.CEI_Procedure = "63";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("When Invoice has at least one Instruction (Style: H5, Procedure: 63), Code: Y041", supportingDocument.CSI_CodeInfo, expectedMessageError);

			supportingDocument.CSI_Code = "Z041";
			AssertNoMessageErrorContaining("When Invoice has at least one Instruction (Style: H5, Procedure: 63), Code: Z041", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Procedure = "60";
			supportingDocument.CSI_Code = "Y041";
			AssertNoMessageErrorContaining("When Invoice has at least one Instruction (Style: H5, Procedure: 60), Code: Y041", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H4";
			entryInstruction.CEI_Procedure = "63";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining("When Invoice has at least one Instruction (Style: H5, Procedure: 60), Code: Y041", supportingDocument.CSI_CodeInfo, expectedMessageError);
		});
	}

	public void TestCheckCode_WhenProcedureIs42_63_AtInvoiceLineLevel()
	{
		const string expectedMessageError = "When related Entry Instruction with Procedure Code is '42' or '63' and Declaration Type is 'H1' or 'H5', the document types 'Y040', 'Y041' and 'Y042' must not be used";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = "Y042";
			AssertNoMessageErrorContaining("When Invoice Line has not related Entry Instructions, Code: Y042", supportingDocument.CSI_CodeInfo, expectedMessageError);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			entryInstruction.CEI_Style = "H5";
			entryInstruction.CEI_Procedure = "63";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("When related Entry Instruction (Style: H5, Procedure: 63), Code: Y042", supportingDocument.CSI_CodeInfo, expectedMessageError);

			supportingDocument.CSI_Code = "Z042";
			AssertNoMessageErrorContaining("When related Entry Instruction (Style: H5, Procedure: 63), Code: Z042", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Procedure = "60";
			supportingDocument.CSI_Code = "Y042";
			AssertNoMessageErrorContaining("When related Entry Instruction (Style: H5, Procedure: 60), Code: Y042", supportingDocument.CSI_CodeInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H4";
			entryInstruction.CEI_Procedure = "63";
			supportingDocument.Validation.ValidateCSI_Code();
			AssertNoMessageErrorContaining("When related Entry Instruction (Style: H5, Procedure: 60), Code: Y042", supportingDocument.CSI_CodeInfo, expectedMessageError);
		});
	}
}
