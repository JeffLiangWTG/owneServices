using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ExportJobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => EU.Business.MessageTypeList.Codes.Export;

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.Bills.AddNew();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = cusEntryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return declaration;
	}
}
