using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class CreditNoteFormTest : BaseInvoicingFormTest
	{
		[TestDate(2020, 2, 2)]
		public void TestOriginalTransactionRefFieldsReadOnlyAP()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "ABC000125", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDate.Today.AddDays(-4));
			Factory.Save();

			var creditNote = Factory.New<APCreditNote>();
			using (var testForm = new CreditNoteForm(creditNote))
			{
				testForm.Show();
				Application.DoEvents();
				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);

				creditNote.AH_OriginalInvoiceDate = ZDate.Today.AddDays(-5);
				creditNote.AH_OriginalTransactionNum = "ABC000123";

				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);
				AssertEquals(null, creditNote.OriginalReferenceTransaction);
				AssertEquals(ZDate.Today.AddDays(-5), creditNote.AH_OriginalInvoiceDate);
				AssertEquals("ABC000123", creditNote.AH_OriginalTransactionNum);

				creditNote.OriginalTransactionReference = invoice.PK;

				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);
				AssertEquals(invoice, creditNote.OriginalReferenceTransaction);
				AssertEquals(ZDate.Today.AddDays(-4), creditNote.AH_OriginalInvoiceDate);
				AssertEquals("ABC000125", creditNote.AH_OriginalTransactionNum);
			}
		}

		[TestDate(2020, 2, 2)]
		public void TestOriginalTransactionRefFieldsReadOnlyAR()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ABC000125", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDate.Today.AddDays(-4));
			Factory.Save();

			var creditNote = Factory.New<ARCreditNote>();
			using (var testForm = new CreditNoteForm(creditNote))
			{
				testForm.Show();
				Application.DoEvents();
				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);

				creditNote.AH_OriginalInvoiceDate = ZDate.Today.AddDays(-5);
				creditNote.AH_OriginalTransactionNum = "ABC000123";
				Application.DoEvents();

				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(!testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);
				AssertEquals(null, creditNote.OriginalReferenceTransaction);
				AssertEquals(ZDate.Today.AddDays(-5), creditNote.AH_OriginalInvoiceDate);
				AssertEquals("ABC000123", creditNote.AH_OriginalTransactionNum);
				AssertNotEquals(invoice.AH_TransactionNum, creditNote.AH_OriginalTransactionNum);

				creditNote.OriginalTransactionReference = invoice.PK;
				Application.DoEvents();

				Assert(!testForm.InvoiceDetails.TransactionGuidFindBox.ReadOnly);
				Assert(testForm.InvoiceDetails.AH_OriginalInvoiceDateEdit.ReadOnly);
				Assert(testForm.InvoiceDetails.AH_OriginalTransactionNumTextBox.ReadOnly);
				AssertEquals(invoice, creditNote.OriginalReferenceTransaction);
				AssertEquals(ZDate.Today.AddDays(-4), creditNote.AH_OriginalInvoiceDate);
				AssertEquals(invoice.AH_TransactionNum, creditNote.AH_OriginalTransactionNum);
			}
		}

		public void TestHandleNegativeCompliancesFailedToCreate()
		{
			AssertHandleNegativeCompliancesFailedToCreate(true);
			AssertHandleNegativeCompliancesFailedToCreate(false);
		}

		public void TestOriginalTransactionRefContols()
		{
			void assertVisibility(BaseInvoicingForm form, bool visible)
			{
				AssertEquals(visible, form.InvoiceDetails.TransactionGuidFindBox.Visible);
				AssertEquals(visible, form.InvoiceDetails.TransactionGuidFindBox.GetExtension<LabelCaptionRenderer>().Visible);
				AssertEquals(visible, form.InvoiceDetails.AH_OriginalInvoiceDateEdit.Visible);
				AssertEquals(visible, form.InvoiceDetails.AH_OriginalInvoiceDateEdit.GetExtension<LabelCaptionRenderer>().Visible);
				AssertEquals(visible, form.InvoiceDetails.AH_OriginalTransactionNumTextBox.Visible);
				AssertEquals(visible, form.InvoiceDetails.AH_OriginalTransactionNumTextBox.GetExtension<LabelCaptionRenderer>().Visible);
			}

			void assertFor<T>(bool visible) where T : InvoicingBase
			{
				using (var testForm = new CreditNoteForm(Factory.New<T>()))
				{
					testForm.Show();
					Application.DoEvents();
					assertVisibility(testForm, visible);
				}
			}

			assertFor<ARCreditNote>(true);
			assertFor<APCreditNote>(true);
			assertFor<APInvoice>(false);
			assertFor<UACreditNote>(false);
		}

		void AssertHandleNegativeCompliancesFailedToCreate(bool flag)
		{
			var invoice = (CreditNote)GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.Debtor.PK;

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (invoice is ARCreditNote)
				{
					TestObjectCreator.Debtor.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";

					var line = invoice.Lines.AddNew() as ARCreditNoteLine;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					line.AL_OSExTaxAmount = -100m;
					line.AL_AC = TestObjectCreator.FRT.PK;
					line.AL_AT = TestObjectCreator.GST1.PK;
				}

				if (invoice is APCreditNote)
				{
					TestObjectCreator.Debtor.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

					var line = invoice.Lines.AddNew() as APCreditNoteLine;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_OSExTaxAmount = -100m;
					line.CreateComplianceDocumentRecordOnPosting = true;
					line.ComplianceDocumentNumber = "AA00000001";
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (var form = (CreditNoteForm)GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					Factory.Save();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestHandleSaveException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ABC000125", TestObjectCreator.AUD, 1M, TestObjectCreator.Debtor, ZDate.Today.AddDays(-4));
				Factory.Save();

				var creditNote = Factory.New<ARCreditNote>();
				using (var testForm = new CreditNoteForm(creditNote))
				{
					testForm.Show();
					Application.DoEvents();
					testForm.HandleSaveException_ForTestOnly(new InvoiceDateLessThanPreviousException());

					AssertEquals("Invoice date must be equal or higher than previous document.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public abstract void TestPromptToPrintComplianceDocumentWhenEnablePrompt();

		public abstract void TestPromptToPrintComplianceDocumentWhenDisablePrompt();
	}
}
