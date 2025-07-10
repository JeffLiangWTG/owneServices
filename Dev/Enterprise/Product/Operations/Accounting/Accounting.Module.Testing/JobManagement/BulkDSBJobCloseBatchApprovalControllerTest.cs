using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BulkDSBJobCloseBatchApprovalController))]
	public class BulkDSBJobCloseBatchApprovalControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(BulkDSBJobCloseBatchApprovalController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BulkDSBJobCloseBatchApproval;
		}

		public void TestView()
		{
			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_BatchNumber = "001";
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.BulkDSBJobCloseBatchApproval) as BulkDSBJobCloseBatchApprovalController;

			Env.Security.DisbursementJobCloseBatchView.IsAllowed = false;
			using (var form = controller.ShowEditForm(batch))
			{
				AssertNull(form);
				AssertEquals(Env.Security.DisbursementJobCloseBatchView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Manage -> Job Costing -> Disbursement Job Close Batch Approval", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.DisbursementJobCloseBatchView.IsAllowed = true;
			using (var form = controller.ShowEditForm(batch) as AccountingZForm)
			{
				AssertEquals("View Disbursement Job Close Batch Approval", form.FormHeading);
				AssertType<BulkDSBJobCloseBatchApprovalForm>(form);
				AssertType<DsbJobCloseBatch>(form.BusinessEntityForPersistingForm);
				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.View, ((BulkDSBJobCloseBatchApprovalForm)form).ActionMode);
			}
		}

		public void TestGetApprovalForm()
		{
			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_BatchNumber = "001";
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.BulkDSBJobCloseBatchApproval) as BulkDSBJobCloseBatchApprovalController;
			using (var form = controller.GetApprovalOrCancelForm(batch, BulkDSBJobCloseBatchApprovalFormModes.Approve) as AccountingZForm)
			{
				AssertEquals("Approve Disbursement Job Close Batch Approval", form.FormHeading);
				AssertType<BulkDSBJobCloseBatchApprovalForm>(form);
				AssertType<DsbJobCloseBatch>(form.BusinessEntityForPersistingForm);
				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.Approve, ((BulkDSBJobCloseBatchApprovalForm)form).ActionMode);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Approve, ((DsbJobCloseBatch)form.BusinessEntityForPersistingForm).JBB_BatchStatus);
			}
		}

		public void TestGetCancelForm()
		{
			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_BatchNumber = "001";
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.BulkDSBJobCloseBatchApproval) as BulkDSBJobCloseBatchApprovalController;
			using (var form = controller.GetApprovalOrCancelForm(batch, BulkDSBJobCloseBatchApprovalFormModes.Cancel) as AccountingZForm)
			{
				AssertEquals("Cancel Disbursement Job Close Batch Approval", form.FormHeading);
				AssertType<BulkDSBJobCloseBatchApprovalForm>(form);
				AssertType<DsbJobCloseBatch>(form.BusinessEntityForPersistingForm);
				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.Cancel, ((BulkDSBJobCloseBatchApprovalForm)form).ActionMode);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Cancel, ((DsbJobCloseBatch)form.BusinessEntityForPersistingForm).JBB_BatchStatus);
			}
		}

		public override void TestNewForm()
		{
			Assert("Creating new batch is not supported", true);
		}
	}
}
