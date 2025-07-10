using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ExportDeclarationActivationJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => CHJobMessageTypeList.Codes.ExportDeclarationActivation;

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		declaration.CusContainers.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();
		declaration.Bills.AddNew();
		declaration.CustomsEntryHeaders.AddNew();
		return declaration;
	}
}
