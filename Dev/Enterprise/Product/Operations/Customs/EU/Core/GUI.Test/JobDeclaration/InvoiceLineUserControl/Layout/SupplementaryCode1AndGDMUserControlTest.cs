using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class SupplementaryCode1AndGDMUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new SupplementaryCode1AndGDMUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Host", control, control.Host);
					AssertType<DefaultControlExtensionCollection>("Extensions", control.Extensions);
				});
			}
		}

		public void TestGDMLink()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = brokerageControl.InvoiceLinesUserControl;

				using (var control = new SupplementaryCode1AndGDMUserControlForTest(invoiceLinesUserControl))
				{
					CombineAssertions(() =>
					{
						var link = control.FindSingle<ZLinkLabel>("GDMLink");
						AssertEquals("GDMLink is visible", true, link.Visible);
						AssertEquals("GDMLink should be shown as GDM", "GDM", link.Text);
						link.PerformClick_ForTest();

						AssertType("LastFormShown type", typeof(GuidedDecisionMakingForm), ZFormModaliser.LastFormShownDialogForTest);
					});

					ZFormModaliser.LastFormShownDialogForTest = null;
					ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				}
			}
		}

		sealed class SupplementaryCode1AndGDMUserControlForTest : SupplementaryCode1AndGDMUserControl
		{
			public SupplementaryCode1AndGDMUserControlForTest(Customs.GUI.BaseInvoiceLineUserControl invoiceLineUserControl)
			{
				this.invoiceLineUserControl = invoiceLineUserControl;
			}
			readonly Customs.GUI.BaseInvoiceLineUserControl invoiceLineUserControl;

			protected override Control GetInvoiceLineUserControl() => invoiceLineUserControl;
		}
	}
}
