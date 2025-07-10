using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class ImportCusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var instructionResult = instructionLookups.CodeList.CodesAsString;
		var expectedListCodesAsString = "FR5, FR7";
		AssertEquals($"Instruction's cusFiscalReference lookups must be F5, F7", expectedListCodesAsString, instructionResult);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionFiscalReference = instruction.FiscalReferences.AddNew();
		instructionLookups = new ImportCusFiscalReferenceLookups(instructionFiscalReference);

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var invoiceLineFiscalReference = instruction.FiscalReferences.AddNew();
	}
	ImportCusFiscalReferenceLookups instructionLookups;
	CusEntryInstruction instruction;
}
