using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class CusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		CombineAssertions(() =>
		{
			AssertLookups(string.Empty,
				ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing,
				ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission,
				ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing);

			AssertLookups("FR1, FR2, FR4, FR5",
				ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation);

			AssertLookups("FR1, FR2, FR3, FR4, FR5, FR7",
				ImportCusEntryInstructionsDeclarationTypeList.Codes.IntroductionOfGoods,
				ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired,
				ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified);
		});
	}

	void AssertLookups(string expectedListCodesAsString, params string[] styles)
	{
		foreach (var style in styles)
		{
			instruction.CEI_Style = style;

			var instructionResult = instructionLookups.CodeList.CodesAsString;
			AssertEquals($"Instruction's cusFiscalReference lookups when instruction.CEI_Style = {style}", expectedListCodesAsString, instructionResult);

			var invoiceLineResult = invoiceLineLookups.CodeList.CodesAsString;
			AssertEquals($"Invoice Line's cusFiscalReference lookups when instruction.CEI_Style = {style}", expectedListCodesAsString, invoiceLineResult);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		var instructionFiscalReference = instruction.FiscalReferences.AddNew();
		instructionLookups = new ImportCusFiscalReferenceLookups(instructionFiscalReference);

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		var invoiceLineFiscalReference = instruction.FiscalReferences.AddNew();
		invoiceLineLookups = new ImportCusFiscalReferenceLookups(invoiceLineFiscalReference);
	}
	ImportCusFiscalReferenceLookups instructionLookups;
	ImportCusFiscalReferenceLookups invoiceLineLookups;
	CusEntryInstruction instruction;
}
