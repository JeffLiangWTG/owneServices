using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing;

public abstract class JobDeclarationFormTest_ForWhenDeclarationCancelled<TBusinessObject> : BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<TBusinessObject>
	where TBusinessObject : JobDeclaration
{
	public override void TestMinimumSizeNotTooBig()
	{
		var declaration = Factory.New<JobDeclaration>();
		var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
		var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(900);
		using (var form = new JobDeclarationForm(declaration))
		{
			Assert("EU Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
			Assert("EU Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
		}
	}

	protected override TBusinessObject GetPopulatedDeclarationForFormBashingCore()
	{
		var declaration = base.GetPopulatedDeclarationForFormBashingCore();

		declaration.SupportingDocuments.AddNew();

		var invoice = (JobComInvoiceHeader)declaration.Invoices.First();
		invoice.SupportingDocuments.AddNew();

		var invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.First();
		invoiceLine.SupportingDocuments.AddNew();

		return declaration;
	}
}
