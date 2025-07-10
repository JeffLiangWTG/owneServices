using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.CDSCashPayments.Testing
{
	internal class CDSCashPaymentsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var form = CDSCashPaymentsHelper.GetFormForTest(Factory))
			{
				CombineAssertions(() =>
				{
					AssertControl<ZOrganisationControl>(form, "Importer", "Importer", "EntryHeader.Declaration.JE_OH_Importer", 249);
					AssertControl<ZTextBox>(form, "PaymentAmount", "Payment Amount", "C9_PaymentAmount", 100);
					AssertControl<ZDropEdit>(form, "TransactionType", "Transaction Type", "C9_TransactionType", 193);
					AssertControl<ZDateEdit>(form, "PaymentDate", "Payment Date", "C9_PaymentDate");
					AssertControl<ZTextBox>(form, "PaymentReference", "Payment Reference", "C9_PaymentReference", 140);
					AssertControl<ZTextBox>(form, "IncomingPayResponseNo", "Incoming Pay Response No.", "C9_IncomingPayResponseNo", 140);
					AssertControl<ZDropEdit>(form, "PaymentStatus", "Payment Status", "C9_PaymentStatus", 193);
					AssertControl<ZTextBox>(form, "MRN", "MRN", "MRN", 171);
					AssertControl<ZTextBox>(form, "BGMReference", "DUCR", "LRN", 171);
					AssertControl<ZDateEdit>(form, "ReceiptDate", "Receipt Date", "C9_ReceiptDate");
				});
			}
		}

		public void TestLinkLabel()
		{
			using (var form = CDSCashPaymentsHelper.GetFormForTest(Factory))
			{
				form.Show();
				var declarationReference = ((Business.CusEntryPayInfo)((ZForm)form).DataSource).DeclarationReference;
				var link = AssertControl<ZLinkLabel>(form, "DeclarationReference", null, "EntryHeader.Declaration.HumanReadableName");
				AssertEquals("DeclarationReference link text", $"Declaration {declarationReference}", link.Text);
				AssertEquals("DeclarationReference link expected to have exactly one link", 1, link.Links.Count);
				link.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(link.Links[0]));

				var uc = form.FindSingle<CDSCashPaymentsUserControl>("CDSCashPaymentsUserControl");
				var caption = (uc.JobDeclarationController?.LastShownForm as JobDeclarationForm)?.FormCaption;
				(uc.JobDeclarationController?.LastShownForm as ZForm)?.Close();
				AssertContains($"Customs Declaration - {declarationReference}", caption);
			}
		}

		public void TestOpenButton()
		{
			using (var form = CDSCashPaymentsHelper.GetFormForTest(Factory))
			{
				form.Show();
				var declarationReference = ((Business.CusEntryPayInfo)((ZForm)form).DataSource).DeclarationReference;
				var button = AssertControl<ZButton>(form, "OpenDeclarationButton", "Open", string.Empty);
				button.PerformClick();

				var uc = form.FindSingle<CDSCashPaymentsUserControl>("CDSCashPaymentsUserControl");
				var caption = (uc.JobDeclarationController?.LastShownForm as JobDeclarationForm)?.FormCaption;
				(uc.JobDeclarationController?.LastShownForm as ZForm)?.Close();
				AssertContains($"Customs Declaration - {declarationReference}", caption);
			}
		}

		T AssertControl<T>(Form form, string controlName, string expectedCaption, string expectedBindingMember, int expectedWidth = -1) where T : Control
		{
			var control = CDSCashPaymentsHelper.GetSingleControlOrNull<T>(form, controlName);
			AssertNotNull(controlName, control);
			if (control != null)
			{
				AssertEquals($"{controlName}.Caption", expectedCaption, control.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals($"{controlName}.BindingMember", expectedBindingMember, control.GetBindingMember());
				if (expectedWidth != -1)
				{
					AssertEquals($"{controlName}.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), control.Width);
				}
			}
			return control;
		}
	}
}
