using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class NationalAdditionalCodesUserControlTest : TestCaseWithFactory
	{
		public void TestNationalAdditionalCodesEditButton_OnClick()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new NationalAdditionalCodesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var button = control.FindSingle<ZButton>("NationalAdditionalCodesEditButton");
				button.PerformClick();
				AssertType<NationalAdditionalCodesForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestIExtendedControl()
		{
			using (var control = new NationalAdditionalCodesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Host", control, control.Host);
					AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
				});
			}
		}
		public void TestZLabelCaptionRenderer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new NationalAdditionalCodesUserControl())
			{
				form.CaptionRenderingEnabled = true;
				form.Controls.Add(control);
				form.Show();

				var test = control.GetExtension<ZLabelCaptionRenderer>();
				AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
			}
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new NationalAdditionalCodesUserControl())
			{
				AssertEquals(nameof(JobComInvoiceLine.JI_NationalAdditionalCodes), control.ResourceStringBindingMember);
			}
		}
	}
}
