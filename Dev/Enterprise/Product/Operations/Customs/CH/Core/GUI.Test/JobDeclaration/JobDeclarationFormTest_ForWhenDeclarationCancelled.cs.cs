using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

abstract class JobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
{
	public override void TestMinimumSizeNotTooBig()
	{
		var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(JobDeclarationFormTest.minScreenWidth);
		var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(JobDeclarationFormTest.minScreenHeight);

		using (var form = GetFormToBashCore())
		{
			AssertLessThanOrEqualTo("Declaration Form min size too wide", form.MinimumSize.Width, minScreenWidthSupported);
			AssertLessThanOrEqualTo("Declaration Form min size too high", form.MinimumSize.Height, minScreenHeightSupported);
		}
	}

	protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		declaration.CusContainers.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();
		declaration.Bills.AddNew();
		var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		cusEntryHeader.MergedLines.AddNew();
		return declaration;
	}
}
