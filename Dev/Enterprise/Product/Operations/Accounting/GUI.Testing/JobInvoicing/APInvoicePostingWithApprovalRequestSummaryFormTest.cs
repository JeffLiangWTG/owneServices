using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(APInvoicePostingWithApprovalRequestSummaryForm))]
	public class APInvoicePostingWithApprovalRequestSummaryFormBasherTest : ZFormBasherTest
	{
		public void TestVerb()
		{
			var formBizo = new APInvoiceChargesCollection();
			formBizo.Add(new APInvoiceCharges("", "", ZGuid.Empty, "", null));

			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, "S001"))
			{
				form.Show();

				AssertEquals("S001 AP Invoice Posting Summary", form.Text);
			}
		}

		public void TestButtonsSetup()
		{
			var formBizo = new APInvoiceChargesCollection();
			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, ""))
			{
				form.Show();

				AssertNotNull("AcceptButton is defined", form.AcceptButton);
				AssertEquals("AcceptButton.Text", "Con&tinue", ((Button)form.AcceptButton).Text);
				AssertEquals("AcceptButton.DialogResult", DialogResult.OK, ((Button)form.AcceptButton).DialogResult);
				AssertNotNull("CancelButton is defined", form.CancelButton);
				AssertEquals("CancelButton.Text", "&Cancel", ((Button)form.CancelButton).Text);
				AssertEquals("CancelButton.DialogResult", DialogResult.Cancel, ((Button)form.CancelButton).DialogResult);
			}
		}

		public void TestValidationOnClosing()
		{
			var formBizo = new APInvoiceChargesCollection();
			var invoiceCharges = new APInvoiceCharges("", "", ZGuid.Empty, "", null);
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			invoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(request);
			formBizo.Add(invoiceCharges);

			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, ""))
			{
				form.Show();

				Assert("Precondition: from Visible.", form.Visible);
				form.AcceptButton.PerformClick();
				form.Close();
				Assert("Validation have been run.", formBizo.HasErrors());
				AssertEquals("Validation errors are shown to a user.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("From is still opened.", form.Visible);
				AssertEquals("form.DialogResult", DialogResult.OK, form.DialogResult);

				form.CancelButton.PerformClick();
				form.Close();
				Assert("Form closed even with validation errors.", !form.Visible);
				AssertEquals("form.DialogResult", DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestHandlePrint()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodManagementTestHelper.SetupPeriods();
			var formBizo = new APInvoiceChargesCollection();

			Env.Security.APInvoiceApproval_Print.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, "S20012"))
			{
				form.Show();
				var grid = form.Controls.Find("grid", true)[0] as ZGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Preview");
				menuItem.PerformClick();
				AssertStartsWith("LastMessage", Env.Security.APInvoiceApproval_Print.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.APInvoiceApproval_Print.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, "S20012"))
			{
				form.Show();
				var grid = form.Controls.Find("grid", true)[0] as ZGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Preview");
				menuItem.PerformClick();
				AssertStartsWith("LastMessage", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipment = TestObjectCreator.CreateShipment("S20012", "AUSYD", "AUMEL");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", null, 100m, TestObjectCreator.AALSHI, "123456", null, 100m, TestObjectCreator.Debtor);
			Factory.Save();

			var apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.AALSHI.OH_Code, "123456", ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.Add(charge);
			var approvalRequest = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			approvalRequest.XP_ReasonDescription = "Desc";
			approvalRequest.InitializeJobRelated(apInvoiceCharges, job.PK, job.TablePrefix);
			apInvoiceCharges.PostingGUIProvider.ShowApprovalFormToSetDescription(approvalRequest);
			formBizo.Add(apInvoiceCharges);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail);
			bool printTaskRan = false;
			InvoicePrintHelper.ResetPrintTaskRun_ForTestOnly();
			InvoicePrintHelper.PrintTaskRun_ForTestOnly += delegate(InvoicePrintTask printTask)
			{
				printTaskRan = true;
				AssertEquals("There should be 1 reports in the pack", 1, printTask[0].Count);
				AssertEquals("Cost Confirmation Document", printTask[0][0].Name);
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, "S20012"))
			{
				form.Show();
				var grid = form.Controls.Find("grid", true)[0] as ZGrid;
				var menuItem = grid.ContextMenu.MenuItems.FindByText("Preview");
				menuItem.PerformClick();
				Assert("Print task should be run", printTaskRan);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var formBizo = new APInvoiceChargesCollection();
			formBizo.Add(new APInvoiceCharges("", "", ZGuid.Empty, "", null));
			var cancelledRequest = new BusinessObjectFactory().New<APInvoiceChargesApprovalRequest>();
			cancelledRequest.PostingDetails.Charges.AddNew();
			formBizo.Add(new APInvoiceCharges(cancelledRequest));

			return new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, "");
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
