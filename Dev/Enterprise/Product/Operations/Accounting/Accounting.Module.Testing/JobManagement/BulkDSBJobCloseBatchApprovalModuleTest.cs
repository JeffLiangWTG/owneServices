using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BulkDSBJobCloseBatchApprovalModule))]
	public class BulkDSBJobCloseBatchApprovalModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BulkDSBJobCloseBatchApproval;
		}

		public void TestMenuItems()
		{
			using (var module = new BulkDSBJobCloseBatchApprovalModule())
			{
				var items = module.FormActionMenu;

				AssertNull(items.FindByText("&New"));
				AssertNull(items.FindByText("&Edit"));
				AssertNull(items.FindByText("&Delete"));
				AssertNotNull(items.FindByText("&View"));

				AssertNotNull(items.FindByText("Approve", true));
				AssertNotNull(items.FindByText("Cancel", true));
			}
		}

		public void TestApproval()
		{
			TestApprovalOrCancel(true);
		}

		public void TestCancel()
		{
			TestApprovalOrCancel(false);
		}

		public void TestShowFormWhenApproval()
		{
			TestFormWhenApprovalOrCancel(true);
		}

		public void TestShowFormWhenCancel()
		{
			TestFormWhenApprovalOrCancel(false);
		}

		void TestFormWhenApprovalOrCancel(bool isApproval)
		{
			var actionMode = isApproval ? BulkDSBJobCloseBatchApprovalFormModes.Approve : BulkDSBJobCloseBatchApprovalFormModes.Cancel;
			var batchStatus = isApproval ? AccountingConstants.DsbJobBatchStatus.Approve : AccountingConstants.DsbJobBatchStatus.Cancel;

			var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch1.JBB_BatchNumber = "001";
			batch1.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
			Factory.Save();

			using (var module = new BulkDSBJobCloseBatchApprovalModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 1, module.DisplayGrid.SelectedElements.Length);
				using (var formToShow = module.ApprovalOrCancelBatch(isApproval))
				{
					AssertNotNull(isApproval);
					AssertType<BulkDSBJobCloseBatchApprovalForm>(formToShow);
					AssertType<DsbJobCloseBatch>(formToShow.BusinessEntityForPersistingForm);
					AssertEquals(actionMode, ((BulkDSBJobCloseBatchApprovalForm)formToShow).ActionMode);
					AssertEquals(batchStatus, ((DsbJobCloseBatch)formToShow.BusinessEntityForPersistingForm).JBB_BatchStatus);
				}
			}
		}

		void TestApprovalOrCancel(bool isApproval)
		{
			var menuItemText = isApproval ? "Approve" : "Cancel";
			var securityCheckpoint = isApproval ? Env.Security.DisbursementJobCloseBatchApprove : Env.Security.DisbursementJobCloseBatchCancel;
			var noSecurityRightErrorMessage = isApproval
				? "Manage -> Job Costing -> Disbursement Job Close Batch Approval -> Approve"
				: "Manage -> Job Costing -> Disbursement Job Close Batch Approval -> Cancel";

			using (var module = new BulkDSBJobCloseBatchApprovalModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();

				//no Security Right
				securityCheckpoint.IsAllowed = false;
				module.FormActionMenu.FindByText(menuItemText, true).PerformClick();
				AssertEquals(securityCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(noSecurityRightErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				securityCheckpoint.IsAllowed = true;

				//no Batch
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 0, module.DisplayGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.FormActionMenu.FindByText(menuItemText, true).PerformClick();
				AssertEquals("Please select one record", UnitTestUserNotification.Instance.LastMessage.Text);

				//one Batch without status REQ 
				var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
				batch1.JBB_BatchNumber = "001";
				Factory.Save();
				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 1, module.DisplayGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.FormActionMenu.FindByText("Approve", true).PerformClick();
				AssertEquals("You can only Approval/Cancel record with status REQ", UnitTestUserNotification.Instance.LastMessage.Text);

				//one Batch with status REQ 
				batch1.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
				Factory.Save();
				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 1, module.DisplayGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.FormActionMenu.FindByText(menuItemText, true).PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				//two Batchs
				var batch2 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
				batch2.JBB_BatchNumber = "002";
				Factory.Save();
				module.PerformSearch_ForTest();
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 2, module.DisplayGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.FormActionMenu.FindByText(menuItemText, true).PerformClick();
				AssertEquals("Please select one record", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
