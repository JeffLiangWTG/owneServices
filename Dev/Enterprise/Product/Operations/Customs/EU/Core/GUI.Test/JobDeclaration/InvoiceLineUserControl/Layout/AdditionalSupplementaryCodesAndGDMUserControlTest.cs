using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class AdditionalSupplementaryCodesAndGDMUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new AdditionalSupplementaryCodesAndGDMUserControl())
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
			using (var control = new AdditionalSupplementaryCodesAndGDMUserControl())
			{
				form.CaptionRenderingEnabled = true;
				form.Controls.Add(control);
				form.Show();

				AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
			}
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new AdditionalSupplementaryCodesAndGDMUserControl())
			{
				AssertEquals(nameof(JobComInvoiceLine.JI_AdditionalSupplements), control.ResourceStringBindingMember);
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

				using (var control = new AdditionalSupplementaryCodesAndGDMUserControlForTest(invoiceLinesUserControl))
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

		public void TestGDMLinkBehaviourInMultiLineMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			var line2 = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = brokerageControl.InvoiceLinesUserControl;
				using (var control = new AdditionalSupplementaryCodesAndGDMUserControlForTest(invoiceLinesUserControl))
				{
					invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
					invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.Select(1);
					CombineAssertions(() =>
					{
						var link = control.FindSingle<ZLinkLabel>("GDMLink");
						AssertEquals("GDMLink is visible", true, link.Visible);
						AssertEquals("GDMLink should be shown as GDM", "GDM", link.Text);

						line1.JI_Tariff = "1234";
						line2.JI_Tariff = "1234";
						link.PerformClick_ForTest();
						AssertType("LastFormShown type", typeof(GuidedDecisionMakingForm), ZFormModaliser.LastFormShownDialogForTest);

						line2.JI_Tariff = "5678";
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
						link.PerformClick_ForTest();
						AssertEquals("ExpectedMessage", "The selected Invoice Lines have different Commodity Codes. Only the current Invoice Line will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertType("LastFormShown type", typeof(GuidedDecisionMakingForm), ZFormModaliser.LastFormShownDialogForTest);
					});
				}
			}
		}

		sealed class AdditionalSupplementaryCodesAndGDMUserControlForTest : AdditionalSupplementaryCodesAndGDMUserControl
		{
			public AdditionalSupplementaryCodesAndGDMUserControlForTest(Customs.GUI.BaseInvoiceLineUserControl invoiceLineUserControl)
			{
				this.invoiceLineUserControl = invoiceLineUserControl;
			}
			readonly Customs.GUI.BaseInvoiceLineUserControl invoiceLineUserControl;

			protected override Control GetInvoiceLineUserControl() => invoiceLineUserControl;
		}
	}
}
