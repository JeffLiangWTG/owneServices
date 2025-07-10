using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

abstract class JobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
{
	public const int minScreenWidth = 1366;
	public const int minScreenHeight = 900;
	
	public override void TestMinimumSizeNotTooBig()
	{
		var declaration = Factory.New<JobDeclaration>();

		var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(minScreenWidth);
		var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(minScreenHeight);

		using (var form = GetFormToBashCore())
		{
			Assert("ZA Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
			Assert("ZA Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
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
