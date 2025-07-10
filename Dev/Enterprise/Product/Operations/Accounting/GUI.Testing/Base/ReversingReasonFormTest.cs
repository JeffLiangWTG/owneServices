using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AccountingMasterFilesRegistry = Enterprise.MasterFiles.Business.AccountingMasterFilesRegistry;

namespace Enterprise.Accounting.GUI.Base.Testing
{
	[TestedType(typeof(TransactionReasonForm))]
	public class ReversingReasonFormTest : ZFormBasherTest
	{
		public void TestReversingReasonCodeAndDescription()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			Factory.Save();

			using (var form = new TransactionReasonForm(new TransactionReasonHolder(arInvoice), "reversing", "reversing"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = string.Empty;
				form.ReasonTextBox_ForTestOnly.Text = "any description";
				UnitTestUserNotification.Instance.ClearMessages();
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals("Enter a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "ABC";
				AssertEquals("selected reason code is not in the drop down list values", -1, form.ReversingReasonCodeDropEdit_ForTestOnly.SelectedIndex);
				form.ReasonTextBox_ForTestOnly.Text = "any description";
				UnitTestUserNotification.Instance.ClearMessages();
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals("Enter a valid reason code", UnitTestUserNotification.Instance.LastMessage.Text);

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = string.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals("Please enter a non-blank reason text!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTransactionReasonForm_Closed()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			Factory.Save();

			var transactionReasonHolder = new TransactionReasonHolder(arInvoice);
			using (var form = new TransactionReasonForm(transactionReasonHolder, "reversing", "reversing"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "ABC";
				AssertEquals("selected reason code is invalid", -1, form.ReversingReasonCodeDropEdit_ForTestOnly.SelectedIndex);
				form.ReasonTextBox_ForTestOnly.Text = "any description";
				form.SupportingDocumentNumberTextBox_ForTestOnly.Text = "test";
				UnitTestUserNotification.Instance.ClearMessages();
				form.Close();
				Assert("form is closed", !form.Visible);
				AssertNull("No validation popup", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("reason value is reset", string.Empty, transactionReasonHolder.Reason);
				AssertEquals("code value is reset", string.Empty, transactionReasonHolder.Code);
				AssertEquals("doc number value is reset", string.Empty, transactionReasonHolder.SupportingDocumentNumber);
			}
		}

		public void TestSupportingDocumentNumberNotEmpty_ReverseReason()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			Factory.Save();

			arCreditNote.OriginalTransaction = arInvoice;
			var creditNoteReversingHolder = new TransactionReasonHolder(arCreditNote);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "reversing", "reversing"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "reversing", "reversing"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.SupportingDocumentNumberTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestSupportingDocumentNumberNotEmpty_AmendmentReason()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			arInvoice.AH_TransactionReference = "001";
			arInvoice.AH_ComplianceSubType = "TXI";
			Factory.Save();

			var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
			arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
			var creditNoteReversingHolder = new TransactionReasonHolder(arCreditNote, TransactionReasonCategory.AmendmentReason);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "amending", "amending"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "amending", "amending"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.SupportingDocumentNumberTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}

			var arAmendment = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.VND, 2m, TestObjectCreator.Debtor);
			arAmendment.AH_TransactionBelongsToGroup = arInvoice.PK;
			var invoiceAmendingHolder = new TransactionReasonHolder(arAmendment, TransactionReasonCategory.AmendmentReason);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(invoiceAmendingHolder, "amending", "amending"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new TransactionReasonForm(invoiceAmendingHolder, "amending", "amending"))
			{
				form.Show();

				form.ReversingReasonCodeDropEdit_ForTestOnly.CodeBox.Text = "IDE";
				form.ReasonTextBox_ForTestOnly.Text = "test";
				form.SupportingDocumentNumberTextBox_ForTestOnly.Text = "test";
				form.OKReasonButton_ForTestOnly.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestSupportingDocumentNumberVisible_ReverseReason()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
			Factory.Save();

			var creditNoteReversingHolder = new TransactionReasonHolder(arCreditNote);

			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "reversing", "reversing"))
			{
				form.Show();

				AssertEquals(false, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(false, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var form = new TransactionReasonForm(creditNoteReversingHolder, "reversing", "reversing"))
			{
				form.Show();

				AssertEquals(true, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(true, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}

			var invoiceReversingHolder = new TransactionReasonHolder(arInvoice);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var form = new TransactionReasonForm(invoiceReversingHolder, "reversing", "reversing"))
			{
				form.Show();

				AssertEquals(false, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(false, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}
		}

		public void TestSupportingDocumentNumberVisible_AmendmentReason()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
			Factory.Save();

			var creditNoteAmendingHolder = new TransactionReasonHolder(arCreditNote, TransactionReasonCategory.AmendmentReason);

			using (var form = new TransactionReasonForm(creditNoteAmendingHolder, "amending", "amending"))
			{
				form.Show();

				AssertEquals(false, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(false, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var form = new TransactionReasonForm(creditNoteAmendingHolder, "amending", "amending"))
			{
				form.Show();

				AssertEquals(true, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(true, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}

			var invoiceAmendingHolder = new TransactionReasonHolder(arInvoice, TransactionReasonCategory.AmendmentReason);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (var form = new TransactionReasonForm(invoiceAmendingHolder, "amending", "amending"))
			{
				form.Show();

				AssertEquals(false, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(false, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (var form = new TransactionReasonForm(invoiceAmendingHolder, "amending", "amending"))
			{
				form.Show();

				AssertEquals(true, form.SupportingDocumentNumLabel_ForTestOnly.Visible);
				AssertEquals(true, form.SupportingDocumentNumberTextBox_ForTestOnly.Visible);
			}
		}

		public void TestIsAmendInFullVisible()
		{
			var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Debtor);
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);

			Factory.Save();

			var creditNoteAmendingHolder = new TransactionReasonHolder(arCreditNote, TransactionReasonCategory.AmendmentReason);

			AssertVisible(Core.Constants.CountryCodes.Malaysia, false);
			AssertVisible(Core.Constants.CountryCodes.Australia, false);

			var amendingTransaction = arInvoice.GenerateAmendingTransaction(typeof(ARCreditNote));
			creditNoteAmendingHolder = new TransactionReasonHolder(amendingTransaction, TransactionReasonCategory.AmendmentReason);

			AssertVisible(Core.Constants.CountryCodes.Malaysia, true);
			AssertVisible(Core.Constants.CountryCodes.Australia, false);

			void AssertVisible(string countryCode, bool visible)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var form = new TransactionReasonForm(creditNoteAmendingHolder, "amending", "amending"))
				{
					form.Show();

					AssertEquals(visible, form.IsAmendInFullCheckBox_ForTestOnly.Visible);
				}
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.ReversingCode = "IDE";
			invoice.ReversingReason = "some reason goes here";

			TransactionReasonHolder reversingholder = new TransactionReasonHolder(invoice);
			reversingholder.HasChanges = false;
			return new TransactionReasonForm(reversingholder, "Please enter some reason", Res.GetString("26107194-7cd0-4a2a-bc55-b3f72d3af0de", "Test form"));
		}

		#endregion
	}
}
