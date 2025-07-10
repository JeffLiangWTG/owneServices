using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class CusEntryNumberValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCE_IssueDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var addInfo = invoiceLine.AdditionalInfos.AddNew();
		var entryline = entryHeader.MergedLines.AddNew();
		entryline.InvoiceLines.Add(invoiceLine);

		addInfo.CSI_Type = "INF";
		addInfo.CSI_Code = "NP500";

		entryHeader.EntryNumber = "123456789012";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = "D";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "E";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "F";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "R";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		entryInstruction.CEI_SubStyle = "V";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		entryInstruction.CEI_SubStyle = "Z";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		addInfo.CSI_Code = "TEST";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "R";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "V";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertNoMessageErrors(entryHeader.CusEntryNumber.CE_IssueDateInfo);
		entryInstruction.CEI_SubStyle = "D";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		entryInstruction.CEI_SubStyle = "E";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		entryInstruction.CEI_SubStyle = "F";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
		entryInstruction.CEI_SubStyle = "A";
		entryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Today;
		AssertHasMessageError(entryHeader.CusEntryNumber.CE_IssueDateInfo, "Acceptance date must be empty");
	}
}
