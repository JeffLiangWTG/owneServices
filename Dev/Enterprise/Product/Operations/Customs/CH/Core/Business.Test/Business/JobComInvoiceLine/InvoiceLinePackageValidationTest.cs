using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InvoiceLinePackageValidation))]
sealed class InvoiceLinePackageValidationTest : TestCaseWithFactory
{
	public void TestCheckIsLinked() => CombineAssertions(() =>
	{
		string message = "[NS30003] Containers Information will not be sent to Customs for Simplified Declarations.";
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var package = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		var packing = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		packing.IsLinked = true;
		AssertHasRowWarning(packing, message);

		packing.IsLinked = false;
		AssertNoRowWarnings(packing);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		packing.IsLinked = true;
		AssertNoRowWarnings(packing);

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		packing = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		packing.IsLinked = true;
		AssertNoRowWarnings(packing);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		packing = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		packing.IsLinked = true;
		AssertNoRowWarnings(packing);
	});
}
