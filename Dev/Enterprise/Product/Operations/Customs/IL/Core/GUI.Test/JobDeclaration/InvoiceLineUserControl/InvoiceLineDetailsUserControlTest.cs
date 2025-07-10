using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsUserControl))]
	sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using var control = new InvoiceLineDetailsUserControl();
			_ = control.AssertContainsControl<ZDropEdit>(nameof(InvoiceLineDetailsUserControl.InvoiceNumberDropEdit), x => x
				.WithBindTo(nameof(JobComInvoiceLine.JI_Calc_Invoice))
				.WithCaption("Invoice Number")
				.WithSizeScaled(334, 20)
			);

			_ = control.AssertContainsControl<ZTextBox>(nameof(InvoiceLineDetailsUserControl.PreferenceDocNumberTextBox), x => x
				.WithBindTo(nameof(JobComInvoiceLine.JI_PreferenceDocNumber))
				.WithSizeScaled(175, 20)
			);
		}
	}
}
