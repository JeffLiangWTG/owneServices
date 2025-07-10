using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	[TestedType(typeof(BulkDSBJobCloseBatchApprovalForm))]
	public class BulkDSBJobCloseBatchApprovalFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var batch = SetUpBatch();
			return new BulkDSBJobCloseBatchApprovalForm(batch);
		}

		public void TestFormWithModeView()
		{
			var batch = SetUpBatch();

			using (var form = new BulkDSBJobCloseBatchApprovalForm(batch))
			{
				form.Show();

				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.View, form.ActionMode);

				Assert(!form.PostingButtonsUserControl.SaveButton.Visible);
				Assert(!form.PostingButtonsUserControl.SaveAndCloseButton.Visible);

				Assert(!batch.JBB_BatchStatusInfo.HasChanges);
				Assert(!batch.JBB_ApprovalTimeUtcInfo.HasChanges);
				Assert(!batch.JBB_GS_NKApprovingUserInfo.HasChanges);
			}
		}

		[TestDate(2019, 10, 20, 0, 0, 0)]
		public void TestFormWithModeApprove()
		{
			var batch = SetUpBatch();

			using (var form = new BulkDSBJobCloseBatchApprovalForm(batch, BulkDSBJobCloseBatchApprovalFormModes.Approve))
			{
				form.Show();

				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.Approve, form.ActionMode);

				Assert(!form.PostingButtonsUserControl.SaveButton.Visible);
				Assert(form.PostingButtonsUserControl.SaveAndCloseButton.Visible);

				Assert(batch.JBB_BatchStatusInfo.HasChanges);
				Assert(batch.JBB_ApprovalTimeUtcInfo.HasChanges);
				Assert(batch.JBB_GS_NKApprovingUserInfo.HasChanges);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Approve, batch.JBB_BatchStatus);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, batch.JBB_GS_NKApprovingUser);
				AssertEquals(ZDateTime.UtcNow, batch.JBB_ApprovalTimeUtc);

				Assert(!form.IsDisposed);
				form.PostingButtonsUserControl.SaveAndCloseButton.PerformClick();
				Assert(form.IsDisposed);

				ReleaseFactory();
				var batchAfterApproval = Factory.Load<DsbJobCloseBatch>(batch.PK);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Approve, batchAfterApproval.JBB_BatchStatus);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, batchAfterApproval.JBB_GS_NKApprovingUser);
				AssertEquals(ZDateTime.UtcNow, batchAfterApproval.JBB_ApprovalTimeUtc);
				AssertEquals(batch.TransactionLines.Count, batchAfterApproval.TransactionLines.Count);
			}
		}

		[TestDate(2019, 10, 20, 0, 0, 0)]
		public void TestFormWithModeCancel()
		{
			var batch = SetUpBatch();

			using (var form = new BulkDSBJobCloseBatchApprovalForm(batch, BulkDSBJobCloseBatchApprovalFormModes.Cancel))
			{
				form.Show();

				AssertEquals(BulkDSBJobCloseBatchApprovalFormModes.Cancel, form.ActionMode);

				Assert(!form.PostingButtonsUserControl.SaveButton.Visible);
				Assert(form.PostingButtonsUserControl.SaveAndCloseButton.Visible);

				Assert(batch.JBB_BatchStatusInfo.HasChanges);
				Assert(batch.JBB_ApprovalTimeUtcInfo.HasChanges);
				Assert(batch.JBB_GS_NKApprovingUserInfo.HasChanges);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Cancel, batch.JBB_BatchStatus);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, batch.JBB_GS_NKApprovingUser);
				AssertEquals(ZDateTime.UtcNow, batch.JBB_ApprovalTimeUtc);

				Assert(!form.IsDisposed);
				form.PostingButtonsUserControl.SaveAndCloseButton.PerformClick();
				Assert(form.IsDisposed);

				ReleaseFactory();
				var batchAfterCancel = Factory.Load<DsbJobCloseBatch>(batch.PK);
				AssertEquals(AccountingConstants.DsbJobBatchStatus.Cancel, batchAfterCancel.JBB_BatchStatus);
				AssertNotEquals(batch.TransactionLines, batchAfterCancel.TransactionLines);
				AssertEquals(0, batchAfterCancel.TransactionLines.Count);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, batchAfterCancel.JBB_GS_NKApprovingUser);
				AssertEquals(ZDateTime.UtcNow, batchAfterCancel.JBB_ApprovalTimeUtc);
			}
		}

		DsbJobCloseBatch SetUpBatch()
		{
			TestObjectCreator.CC1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			TestObjectCreator.CC1.AC_AG_DisbursementSurplusAccount = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.CC1.AC_AG_DisbursementShortfallAccount = TestObjectCreator.GLHeader2.PK;

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J001";
			var charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Agent);
			charge.JR_APInvoiceNum = "INV001";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			new InvoicingPostManager(testJob).CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			var batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			var lines = Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, testJob.PK));
			lines.ForEach(x => x.AL_JBB = batch.PK);
			batch.JBB_BatchNumber = "B001";
			Factory.Save();

			return batch;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}
