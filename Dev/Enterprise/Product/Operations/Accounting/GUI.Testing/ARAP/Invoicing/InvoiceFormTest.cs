using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class InvoiceFormTest : BaseInvoicingFormTest
	{
		public void TestCashInvoiceCheckboxDefault()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();
			invoice.SubmittedFromInvoicingForm = true;

			((Invoice)invoice).IsInvoiceReceiptPayment = true;
			AssertEquals(true, ((Invoice)invoice).IsInvoiceReceiptPayment);

			using (InvoiceForm form = (InvoiceForm)GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				if (invoice is UAInvoice)
				{
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is true and UAInvoice", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is true and UAInvoice", false, form.CashInvoiceOnCheckbox.Checked);
				}
				else
				{
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should be visible if IsInvoiceReceiptPayment is true and not UAInvoice", true, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should be checked if IsInvoiceReceiptPayment is true and not UAInvoice", true, form.CashInvoiceOnCheckbox.Checked);
				}
				form.Close();
			}
		}

		public void TestNonCashInvoiceCheckboxDefault()
		{
			Invoice invoice = (Invoice)GetInvoiceWithValidTestData();
			invoice.SubmittedFromInvoicingForm = true;

			invoice.IsInvoiceReceiptPayment = false;
			AssertEquals(false, invoice.IsInvoiceReceiptPayment);

			using (InvoiceForm form = (InvoiceForm)GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is false", false, form.CashInvoiceOnCheckbox.Checked);
				AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is false", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);

				if (invoice is UAInvoice)
				{
					AssertEquals("ReceiptPaymentOuterPanel should be visible", false, form.ReceiptPaymentOuterPanel.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be visible if IsInvoiceReceiptPayment is false and UAInvoice", false, form.CashInvoiceOnCheckbox.Visible);
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is false", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is false", false, form.CashInvoiceOnCheckbox.Checked);
				}
				else
				{
					AssertEquals("ReceiptPaymentOuterPanel should be visible", true, form.ReceiptPaymentOuterPanel.Visible);
					AssertEquals("CashInvoiceOnCheckbox should be visible if Is InvoiceReceiptPayment is false and not UAInvoice", true, form.CashInvoiceOnCheckbox.Visible);
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is false", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is false", false, form.CashInvoiceOnCheckbox.Checked);
					form.CashInvoiceOnCheckbox.Checked = true;
					AssertEquals("Binding causes IsInvoiceReceiptPayment to be set", true, invoice.IsInvoiceReceiptPayment);
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is true", true, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is true", true, form.CashInvoiceOnCheckbox.Checked);
				}

				form.Close();
			}

			if (!(invoice is UAInvoice))
			{
				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
				using (InvoiceForm form = (InvoiceForm)GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is false", false, form.CashInvoiceOnCheckbox.Checked);
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is false", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);

					AssertEquals("ReceiptPaymentOuterPanel should be visible", false, form.ReceiptPaymentOuterPanel.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be visible if IsInvoiceReceiptPayment is false and UAInvoice", false, form.CashInvoiceOnCheckbox.Visible);
					AssertEquals("ReceiptPaymentPanel_ForTestOnly should not be visible if IsInvoiceReceiptPayment is false", false, form.ReceiptPaymentPanel_ForTestOnly.Visible);
					AssertEquals("CashInvoiceOnCheckbox should not be checked if IsInvoiceReceiptPayment is false", false, form.CashInvoiceOnCheckbox.Checked);

					form.Close();
				}
			}
		}

		public void TestBusinessContextAPInvoiceForm()
		{
			var invoice = (Invoice)GetInvoiceWithValidTestData();
			var isAPInvoice = invoice.GetType() == typeof(APInvoice);

			using (var form = (InvoiceForm)GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				AssertEquals(isAPInvoice, invoice.Factory.HasContext(BusinessContext.APInvoiceForm));
				form.Close();

				AssertEquals(false, invoice.Factory.HasContext(BusinessContext.APInvoiceForm));
			}
		}

		public void TestFormOnClosedWhenLineChargesWereDeletedWithExceptionWhichMayLeaveFormDataInInvalidState()
		{
			var invoice = (Invoice)GetInvoiceWithValidTestData();

			using (var form = (InvoiceForm)GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				Application.DoEvents();

				var consolCost = invoice.ConsolCosting.ConsolCosts.AddNew();
				var charge = consolCost.ApportionmentCharges.AddNew();
				((IBusinessObjectInternals)charge).MarkAsDeleted();

				AssertNoExceptionThrown("A deteched charge in collection should not cause errors when close the form.", () => form.Close());
			}
		}

		public void TestShouldNotShowNegativeCompliancesMessageWhenNoNeeded()
		{
			AssertShouldNotShowNegativeCompliancesMessageWhenNoNeeded(true);
			AssertShouldNotShowNegativeCompliancesMessageWhenNoNeeded(false);
		}

		void AssertShouldNotShowNegativeCompliancesMessageWhenNoNeeded(bool flag)
		{
			var invoice = (Invoice)GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.Debtor.PK;

			if (invoice is APInvoice)
			{
				TestObjectCreator.Debtor.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

				var line = invoice.Lines.AddNew() as APInvoiceLine;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				line.AL_AT = TestObjectCreator.GST1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.CreateComplianceDocumentRecordOnPosting = false;

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
				using (var form = (InvoiceForm)GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					UnitTestUserNotification.Instance.ClearMessages();
					Factory.Save();
					Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestHandleNegativeCompliancesFailedToCreate()
		{
			AssertHandleNegativeCompliancesFailedToCreate(true);
			AssertHandleNegativeCompliancesFailedToCreate(false);
		}

		void AssertHandleNegativeCompliancesFailedToCreate(bool flag)
		{
			var invoice = (Invoice)GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.Debtor.PK;

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				if (invoice is ARInvoice)
				{
					TestObjectCreator.Debtor.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";

					var line = invoice.Lines.AddNew() as ARInvoiceLine;
					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					line.AL_OSExTaxAmount = -100m;
					line.AL_AC = TestObjectCreator.FRT.PK;
					line.AL_AT = TestObjectCreator.GST1.PK;
				}

				if (invoice is APInvoice)
				{
					TestObjectCreator.Debtor.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "PCD";

					var line = invoice.Lines.AddNew() as APInvoiceLine;
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
				using (var form = (InvoiceForm)GetFormByInvoice(invoice))
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

		public abstract void TestPromptToPrintComplianceDocumentWhenEnablePrompt();

		public abstract void TestPromptToPrintComplianceDocumentWhenDisablePrompt();
	}
}
