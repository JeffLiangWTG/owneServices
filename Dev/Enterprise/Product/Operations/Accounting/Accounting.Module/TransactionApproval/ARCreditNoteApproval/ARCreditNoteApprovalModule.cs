using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class ARCreditNoteApprovalModule : InvoicingBaseApprovalModule<ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteApprovalModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ARCreditNoteApproval; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ARCreditNoteApproval);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ARCreditNoteApprovalFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ARCreditNoteApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ARCreditNoteApprovalRequestCollection(Factory);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ARCreditNoteApproval; }
		}

		protected override TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params ARCreditNoteApprovalRequest[] approvalRequests)
		{
			return new ARCreditNoteApprovalBulk(factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override InteractiveSecurityOverrideProvider GetSecurityOverrideProvider(BusinessObject[] approvals)
		{
			return new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(approvals);
		}

		protected override ZForm GetNewApprovalBulkForm(TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> bizo, TransactionApprovalFormModes actionMode)
		{
			return new ARCreditNoteApprovalBulkForm((ARCreditNoteApprovalBulk)bizo, actionMode);
		}

		protected override void HandleApproveCore(TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> approvalBulk)
		{
			var creditNoteApprovalBulk = approvalBulk as ARCreditNoteApprovalBulk;
			var allowedApprovals = creditNoteApprovalBulk.SetARCreditNoteApprovalsStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, out bool errorsAlreadyHandled);
			var deniedApprovals = approvalBulk.Approvals.Except(allowedApprovals);
			if (allowedApprovals.Any())
			{
				if (deniedApprovals.Any())
				{
					var stringBuilder = new ZStringBuilder(Res.GetString("3aab7de1-2e65-4f3e-b369-5d537887a39c", "You do not have the required authorization level to approve the following requests:"));
					foreach (var approval in deniedApprovals)
					{
						stringBuilder.Append(FormattableString.Invariant($"{approval.JobNumber} {approval.JobBranch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code} {approval.JobDepartment?.GE_Code ?? GlbDepartment.CurrentDepartment.GE_Code}"));
					}
					stringBuilder.Append(string.Empty);
					stringBuilder.Append(Res.GetString("69f6fc98-1d12-416a-9f23-8db81bbdadc7", "Would you like to approve the remaining requests?"));

					var result = Globals.Message.Show(stringBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("0d0a08bd-83c3-4aad-80e5-6e7e495d2887", "Approve Approval Request"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.No)
					{
						return;
					}
				}

				creditNoteApprovalBulk = GetNewApprovalBulk(creditNoteApprovalBulk.Factory, GetSecurityOverrideProvider(allowedApprovals.ToArray()), allowedApprovals.ToArray()) as ARCreditNoteApprovalBulk;
				var form = GetNewApprovalBulkForm(creditNoteApprovalBulk, TransactionApprovalFormModes.Approve);
				ShowForm(form);
			}
			else if (!errorsAlreadyHandled)
			{
				Globals.Message.ShowError(Res.GetString("0d7fda5b-b9e7-4fda-af78-53b6b5c8ecc1", "You don't have rights to approve selected requests."));
			}
		}

		protected override void HandleRejectCore(TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> approvalBulk)
		{
			var creditNoteApprovalBulk = approvalBulk as ARCreditNoteApprovalBulk;
			var allowedApprovals = creditNoteApprovalBulk.SetARCreditNoteApprovalsStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, out bool errorsAlreadyHandled);
			var deniedApprovals = approvalBulk.Approvals.Except(allowedApprovals);
			if (allowedApprovals.Any())
			{
				if (deniedApprovals.Any())
				{
					var stringBuilder = new ZStringBuilder(Res.GetString("8043ab4d-2410-4872-9b2b-69006e218c91", "You do not have the required authorization level to reject the following requests:"));
					foreach (var approval in deniedApprovals)
					{
						stringBuilder.Append(FormattableString.Invariant($"{approval.JobNumber} {approval.JobBranch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code} {approval.JobDepartment?.GE_Code ?? GlbDepartment.CurrentDepartment.GE_Code}"));
					}
					stringBuilder.Append(string.Empty);
					stringBuilder.Append(Res.GetString("b9ef623b-5a9e-496b-9684-166b07d06f81", "Would you like to reject the remaining requests?"));

					var result = Globals.Message.Show(stringBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("13f2b76a-af84-4bee-952c-f16627a107df", "Reject Approval Request"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.No)
					{
						return;
					}
				}

				creditNoteApprovalBulk = GetNewApprovalBulk(creditNoteApprovalBulk.Factory, GetSecurityOverrideProvider(allowedApprovals.ToArray()), allowedApprovals.ToArray()) as ARCreditNoteApprovalBulk;
				var form = GetNewApprovalBulkForm(creditNoteApprovalBulk, TransactionApprovalFormModes.Reject);
				ShowForm(form);
			}
			else if (!errorsAlreadyHandled)
			{
				Globals.Message.ShowError(Res.GetString("1ee68562-48b3-4aab-be4d-90008a5d0a95", "You don't have rights to reject selected requests."));
			}
		}

		protected override void HandleCancelCore(TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails> approvalBulk)
		{
			var creditNoteApprovalBulk = approvalBulk as ARCreditNoteApprovalBulk;
			var allowedApprovals = creditNoteApprovalBulk.SetARCreditNoteApprovalsStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, out bool errorsAlreadyHandled);
			var deniedApprovals = approvalBulk.Approvals.Except(allowedApprovals);
			if (allowedApprovals.Any())
			{
				if (deniedApprovals.Any())
				{
					var stringBuilder = new ZStringBuilder(Res.GetString("2a890e8f-7bc5-4e05-b774-ddebaa164ab0", "You do not have the required authorization level to cancel the following requests:"));
					foreach (var approval in deniedApprovals)
					{
						stringBuilder.Append(FormattableString.Invariant($"{approval.JobNumber} {approval.JobBranch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code} {approval.JobDepartment?.GE_Code ?? GlbDepartment.CurrentDepartment.GE_Code}"));
					}
					stringBuilder.Append(string.Empty);
					stringBuilder.Append(Res.GetString("472c02da-8d75-4386-be20-58bbd8173374", "Would you like to cancel the remaining requests?"));

					var result = Globals.Message.Show(stringBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("6d1fd1f2-9ceb-4414-b74a-63cbbcbbb956", "Cancel Approval Request"), MessageBoxButtons.YesNo, DialogResult.No);
					if (result == DialogResult.No)
					{
						return;
					}
				}

				creditNoteApprovalBulk = GetNewApprovalBulk(creditNoteApprovalBulk.Factory, GetSecurityOverrideProvider(allowedApprovals.ToArray()), allowedApprovals.ToArray()) as ARCreditNoteApprovalBulk;
				var form = GetNewApprovalBulkForm(creditNoteApprovalBulk, TransactionApprovalFormModes.Cancel);
				ShowForm(form);
			}
			else if (!errorsAlreadyHandled)
			{
				Globals.Message.ShowError(AccountingConstants.CancelRequestsErrorMessage);
			}
		}
	}
}
