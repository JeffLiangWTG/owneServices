using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(AdditionalSupplementaryCodesUserControl))]
	sealed class AdditionalSupplementaryCodesUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl() => CombineAssertions(() =>
		{
			using var control = new AdditionalSupplementaryCodesUserControl();
			AssertEquals("Host", control, control.Host);
			AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
		});

		public void TestAdditionalSupplementaryCodesEditButton_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using var form = new ZForm(invoiceLine);
			using var control = new AdditionalSupplementaryCodesUserControl();
			form.Controls.Add(control);
			form.Show();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var button = control.FindSingle<ZButton>("AdditionalSupplementaryCodesEditButton");
			button.PerformClick();
			AssertType<AdditionalSupplementaryCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestZLabelCaptionRenderer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using var form = new ZForm(invoiceLine);
			using var control = new AdditionalSupplementaryCodesUserControl();
			form.CaptionRenderingEnabled = true;
			form.Controls.Add(control);
			form.Show();

			AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
		}

		public void TestResourceStringBindingMember()
		{
			using var control = new AdditionalSupplementaryCodesUserControl();
			AssertEquals(nameof(JobComInvoiceLine.JI_AdditionalSupplements), control.ResourceStringBindingMember);
		}
	}
}
