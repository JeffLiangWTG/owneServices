using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLineValidation))]
class JobComInvoiceLineValidationBaseOnlyTest : JobComInvoiceLineValidationAbstractTest<JobComInvoiceLineValidation>
{
	protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

	protected override JobComInvoiceLineValidation GetValidation() => new JobComInvoiceLineValidation(invoiceLine);

	public void TestCheckJI_CEI()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageError(invoiceLine.JI_CEIInfo, "Invoice Line is not linked to an Entry Instruction.");
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageError(invoiceLine.JI_CEIInfo, "Invoice Line is not linked to an Entry Instruction.");
		});
	}

	public void TestCheckJI_CountryOfOriginMandatoryValidation()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();

		CombineAssertions(() =>
		{
			additionalProcedureCode.CY_Code = "";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, "[C0871] If additional procedure starts with E, then ‘Country/Region of Origin’ is required.");

			additionalProcedureCode.CY_Code = "K1";
			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, "[C0871] If additional procedure starts with E, then ‘Country/Region of Origin’ is required.");

			additionalProcedureCode.CY_Code = "E1";
			invoiceLine.JI_CountryOfOrigin = "11";
			AssertNoMessageError(invoiceLine.JI_CountryOfOriginInfo, "[C0871] If additional procedure starts with E, then ‘Country/Region of Origin’ is required.");

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			AssertHasMessageError(invoiceLine.JI_CountryOfOriginInfo, "[C0871] If additional procedure starts with E, then ‘Country/Region of Origin’ is required.");
		});
	}

	public void TestRuleNR9024()
	{
		var errorMessage = "[NR9024]  If additional information type 00100 is completed it must be present for all line items.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = "B1";
		entryInstruction2.CEI_Style = "B2";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		invoiceLine3.JI_CEI = entryInstruction2.PK;
		var addInfo1 = invoiceLine1.AdditionalInfos.AddNew();
		var addInfo2 = invoiceLine2.AdditionalInfos.AddNew();
		addInfo1.CSI_SubType = "INF";
		addInfo2.CSI_SubType = "INF";

		CombineAssertions(() =>
		{
			addInfo1.CSI_Code = "00100";
			invoiceLine2.Validation.ValidateAll();
			AssertHasRowMessageError("Line 1 has 00100 Additional Info Type but not Line 2 and both belong to the same entry instruction.", invoiceLine2, errorMessage);
			AssertNoRowMessageError("Line 3 belongs to a different entry instruction", invoiceLine3, errorMessage);

			addInfo2.CSI_Code = "00100";
			invoiceLine2.Validation.ValidateAll();
			AssertNoRowMessageError("00100 Additional Info Type also added to Line 2", invoiceLine2, errorMessage);
		});
	}

	public void TestRuleNR9025()
	{
		var errorMessage = "[NR9025]  If additional information type 00100 is completed the first 2 digits of the procedure must be equal for all line items.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_Style = "B1";
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine2.JI_CEI = entryInstruction1.PK;
		var addInfo = invoiceLine1.AdditionalInfos.AddNew();
		addInfo.CSI_SubType = "INF";
		addInfo.CSI_Code = "00100";

		CombineAssertions(() =>
		{
			invoiceLine1.JI_FormattedProcedure = "1000";
			invoiceLine2.JI_FormattedProcedure = "2000";
			AssertHasMessageError("Line 1 has 00100 Additional Info Type, Procedure code is 1000 but Line 2 for the same entry instruction has Procedure code 2000.", invoiceLine2.JI_FormattedProcedureInfo, errorMessage);

			invoiceLine2.JI_FormattedProcedure = "1001";
			AssertNoMessageError("First two digits of the procedure are same for Line 1 and Line 2 now.", invoiceLine2.JI_FormattedProcedureInfo, errorMessage);
		});
	}
}
