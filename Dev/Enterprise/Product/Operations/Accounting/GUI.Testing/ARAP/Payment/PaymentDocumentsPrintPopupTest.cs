using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.ARAP.Payment.PaymentDocumentsPrintPopup;

namespace Enterprise.Accounting.GUI.ARAP.Payment.Testing
{
	[TestedType(typeof(PaymentDocumentsPrintPopup))]
	public class PaymentDocumentsPrintPopupTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestDelegateIsClearedOnPrintButtonClick()
		{
			using (MockPaymentDocumentsPrintPopup formToTest = new MockPaymentDocumentsPrintPopup())
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form) { });
				formToTest.Show();
				formToTest.ExecutePrintButton();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object form) { });
			}
		}

		public void TestDefaultCheckBoxSettings()
		{
			using (MockPaymentDocumentsPrintPopup formToTest = new MockPaymentDocumentsPrintPopup())
			{
				AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				formToTest.IsCheque = true;
				formToTest.Show();
				Assert(formToTest.PaymentVoucherCheckBox_Exposed.Checked);
				Assert(formToTest.PrintRemitAdviceCheckBox_Exposed.Checked);
				Assert(formToTest.PrintChequeCheckBox_Exposed.Checked);
				Assert(!formToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}
		}

		public void TestCheckBoxEnabled()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			using (var formToTest = new MockPaymentDocumentsPrintPopup())
			{
				formToTest.Show();
				MockPaymentPrinter testPrinter = new MockPaymentPrinter();
				formToTest.IsCheque = false;

				Assert(!formToTest.PrintChequeCheckBox_Exposed.Checked);
				Assert(!formToTest.PrintPaymentBatchListingCheckBox_Exposed.Checked);
				Assert(!formToTest.PrintChequeCheckBox_Exposed.Enabled);
				Assert(!formToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}

			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintPaymentBatchListing.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (var formToTest = new MockPaymentDocumentsPrintPopup())
			{
				formToTest.IsCheque = true;
				formToTest.Show();

				Assert(formToTest.PrintChequeCheckBox_Exposed.Checked);
				Assert(formToTest.PrintPaymentBatchListingCheckBox_Exposed.Checked);
				Assert(formToTest.PrintChequeCheckBox_Exposed.Enabled);
				Assert(!formToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}

			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (var formToTest = new MockPaymentDocumentsPrintPopup("BLAH!", ZBool.True))
			{
				formToTest.IsCheque = true;
				formToTest.Show();

				Assert(!formToTest.PrintChequeCheckBox_Exposed.Checked);
				Assert(!formToTest.PrintChequeCheckBox_Exposed.Enabled);
				Assert(formToTest.PrintPaymentBatchListingCheckBox_Exposed.Checked);
				Assert(formToTest.ChequeIsAutoPrintLabel_Exposed.Visible);
			}
		}

		public void TestFormHasTopMostSetToTrue()
		{
			var message = @"TopMost should be set to TRUE as this solves a problem when trying to show this form.
If you don't set TopMost = true, this RemittanceAdvicePrint form will be shown BEHIND the Matching form when posting an AP Payment";

			using (PaymentDocumentsPrintPopup formToTest = new PaymentDocumentsPrintPopup())
			{
				AssertEquals(message, true, formToTest.TopMost);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new PaymentDocumentsPrintPopup();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrintChequeOriginal = AccountingConfigurationRegistry.Instance.PrintCheque.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			PrintRemittanceAdviceOriginal = AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			PrintPaymentVoucherOriginal = AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		protected override void TearDown()
		{
			base.TearDown();
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintChequeOriginal);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintRemittanceAdviceOriginal);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintPaymentVoucherOriginal);
		}

		bool PrintChequeOriginal;
		bool PrintRemittanceAdviceOriginal;
		bool PrintPaymentVoucherOriginal;
	}
}
