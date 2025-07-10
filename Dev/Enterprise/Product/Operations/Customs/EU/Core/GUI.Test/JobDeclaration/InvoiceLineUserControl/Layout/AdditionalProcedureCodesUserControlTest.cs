using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class AdditionalProcedureCodesUserControlTest : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new AdditionalProcedureCodesUserControl())
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
			using (var control = new AdditionalProcedureCodesUserControl())
			{
				form.CaptionRenderingEnabled = true;
				form.Controls.Add(control);
				form.Show();

				AssertNotNullOrEmpty(control.GetExtension<ZLabelCaptionRenderer>().Caption);
			}
		}

		public void TestResourceStringBindingMember()
		{
			using (var control = new AdditionalProcedureCodesUserControl())
			{
				AssertEquals(nameof(JobComInvoiceLine.AdditionalProcedureCodesAsString), control.ResourceStringBindingMember);
			}
		}

		public void TestAdditionalProcedureCodesEditButton_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new ZForm(invoiceLine))
			using (var control = new AdditionalProcedureCodesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;

				var button = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");
				button.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("To select additional procedure codes "));
			}
		}

		public void TestAdditionalProcedureCodesEditButton_Click_JI_FormattedProcedureIsNotEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_FormattedProcedure = "40000C9";

			using (var form = new ZForm(invoiceLine))
			using (var control = new AdditionalProcedureCodesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.ShowDialogsInTest = true;

				var button = control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton");
				button.PerformClick();
				AssertType<AdditionalProcedureCodeForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
