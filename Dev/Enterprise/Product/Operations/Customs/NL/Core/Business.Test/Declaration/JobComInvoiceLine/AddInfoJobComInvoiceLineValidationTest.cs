using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckZG_TransNature()
	{
		invoiceLine.ZG_TransNature = "ZZ";
		AssertHasMessageError(invoiceLine.ZG_TransNatureInfo, "The code you have selected is not in the list.");

		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

		invoiceHeader.JZ_ValuationCode = ZString.Empty;
		invoiceLine.ZG_TransNature = ZString.Empty;

		AssertNoMessageError("Not set on Invoice Line but entry instruction is null", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");

		invoiceLine.JI_CEI = cusEntryInstruction.PK;
		AssertZG_TransNatureForAllTypes(cusEntryInstruction, invoiceHeader, invoiceLine);
	}

	void AssertZG_TransNatureForAllTypes(CusEntryInstruction cusEntryInstruction, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine)
	{
		cusEntryInstruction.CEI_Style = "B4";
		invoiceLine.AddInfoValidation.ValidateZG_TransNature();
		AssertNoMessageError("Not set on the Invoice Line but Declaration type is not one of B1, B2, C1, H1, H3, H4, H5 or I", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");

		var validDeclarationTypesForMessage = new ZString[] { DeclarationTypeList.Codes.B1, DeclarationTypeList.Codes.B2, DeclarationTypeList.Codes.C1, DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.H3, DeclarationTypeList.Codes.H4, DeclarationTypeList.Codes.H5, DeclarationTypeList.Codes.I1 };
		foreach (var style in validDeclarationTypesForMessage)
		{
			cusEntryInstruction.CEI_Style = style;
			cusEntryInstruction.ZG_TransNature = ZString.Empty;
			invoiceHeader.JZ_ValuationCode = ZString.Empty;
			invoiceLine.ZG_TransNature = ZString.Empty;
			AssertHasMessageError("Not set on the Invoice Line and Declaration type is one of B1, B2, C1, H1, H3, H4, H5 or I", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");

			cusEntryInstruction.ZG_TransNature = "72";
			invoiceLine.AddInfoValidation.ValidateZG_TransNature();
			AssertNoMessageError("Not set on the Invoice Line but Declaration type is one of B1, B2, C1, H1, H3, H4, H5 or I and transaction Nature is filled for the entry instruction", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");

			cusEntryInstruction.ZG_TransNature = ZString.Empty;
			invoiceHeader.JZ_ValuationCode = "72";
			invoiceLine.AddInfoValidation.ValidateZG_TransNature();
			AssertNoMessageError("Not set on the Invoice Line but Declaration type is one of B1, B2, C1, H1, H3, H4, H5 or I and transaction Nature is filled for the invoice header", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");

			invoiceHeader.JZ_ValuationCode = ZString.Empty;
			invoiceLine.ZG_TransNature = "72";
			AssertNoMessageError("Set on the Invoice Line for Declaration type as one of B1, B2, C1, H1, H3, H4, H5 or I", invoiceLine.ZG_TransNatureInfo, "Transaction Nature is required on either Invoice header OR Invoice Item level OR Entry Instruction Level");
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
}
