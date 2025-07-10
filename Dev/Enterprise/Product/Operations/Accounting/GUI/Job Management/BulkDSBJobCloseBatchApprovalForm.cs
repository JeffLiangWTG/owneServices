using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public enum BulkDSBJobCloseBatchApprovalFormModes
	{
		Approve,
		Cancel,
		View
	}

	public partial class BulkDSBJobCloseBatchApprovalForm : AccountingZForm
	{
		public BulkDSBJobCloseBatchApprovalForm(DsbJobCloseBatch jobCloseBatch, BulkDSBJobCloseBatchApprovalFormModes mode = BulkDSBJobCloseBatchApprovalFormModes.View)
			: base(jobCloseBatch)
		{
			ActionMode = mode;

			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);

			InitJobCloseBatchForApproveAndCancel();
		}

		public override string FormVerb
		{
			get
			{
				var verb = "";
				if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.View)
				{
					verb = FormVerbs.View;
				}
				else if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Approve)
				{
					verb = Res.GetString("c99ef3d9-2436-4adf-8736-df7c81f287e9", "Approve");
				}
				else if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Cancel)
				{
					verb = Res.GetString("65a4cb04-40cc-40c1-923d-3bbf9117858b", "Cancel");
				}

				return verb;
			}
		}

		public override string FormCaption
		{
			get { return Res.GetString("b5459345-3d27-4938-8646-746c342dff99", "Disbursement Job Close Batch Approval"); }
		}

		void InitJobCloseBatchForApproveAndCancel()
		{
			if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Approve ||
				ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Cancel)
			{
				BusinessEntity.JBB_ApprovalTimeUtc = EnvProxy.Instance.Time.CurrentUtcDateTime;
				BusinessEntity.JBB_GS_NKApprovingUser = GlbStaff.CurrentUser.GS_Code;
				BusinessEntity.JBB_BatchStatus = ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Approve
					? AccountingConstants.DsbJobBatchStatus.Approve
					: AccountingConstants.DsbJobBatchStatus.Cancel;
			}
		}

		public readonly BulkDSBJobCloseBatchApprovalFormModes ActionMode;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.View)
			{
				PostingButtonsUserControl.SaveAndCloseButton.Visible = false;
			}
			else
			{
				PostingButtonsUserControl.SaveAndCloseButton.Visible = true;
			}
		}

		void ApprovalOrCancelBatch()
		{
			if (ActionMode == BulkDSBJobCloseBatchApprovalFormModes.Cancel)
			{
				BusinessEntity.TransactionLines.AsEnumerable().Cast<AccTransactionLines>().ForEach(x => x.AL_JBB = Guid.Empty);
			}

			ValidateAndSave();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
		}

		protected override void OnPostButtonClick(object sender, EventArgs e)
		{
			ApprovalOrCancelBatch();
			this.Close();
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			ApprovalOrCancelBatch();
		}

		new DsbJobCloseBatch BusinessEntity
		{
			get { return base.BusinessEntity as DsbJobCloseBatch; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}

