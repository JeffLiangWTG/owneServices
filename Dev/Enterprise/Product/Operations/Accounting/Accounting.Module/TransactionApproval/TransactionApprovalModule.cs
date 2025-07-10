using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public abstract class TransactionApprovalModule<TransactionType, RequestType, DetailsType> : ZFilterGridModule
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public TransactionApprovalModule()
		{
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TransactionApprovalFilterBusinessObject();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		void HandleApprove(object sender, EventArgs e)
		{
			BusinessObject[] approvals = this.Grid.SelectedElements;

			int failedCount = 0;
			ZStringBuilder jobNumberBuilder = new ZStringBuilder();
			if (approvals.Length == 0)
			{
				ShowNoSelectedMessage();
			}
			else
			{
				int validationErrorStatusCount = 0;
				var allowedStatuses = new[] { Constants.GenApprovalRequestApprovalStatus.Requested };

				var latestApprovals = GetLatestStatus(approvals, out BusinessObjectFactory newFactory);
				foreach (RequestType approval in latestApprovals)
				{
					if (!allowedStatuses.Contains(approval.XP_ApprovalStatus.ToString()))
					{
						failedCount++;
						jobNumberBuilder.Append(approval.ReferenceID);
					}
					if (approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Error)
					{
						validationErrorStatusCount++;
					}
				}

				if (failedCount > 0)
				{
					if (IsRelatedTransactionEditSupported && validationErrorStatusCount > 0)
					{
						var result = Globals.Message.Show(Res.GetString("543A02C3-535A-45E4-A145-6FFF48E5342C",
@"One or more of the selected requests have a status of '{0}'. This means the transaction pending allocation has errors that must be corrected before the transaction can be approved for allocation. You must edit the related transaction pending allocation and save it. This will cancel the current request and create a new request with a valid approval status.

Do you want to edit the related transaction pending allocation now?", Constants.GenApprovalRequestApprovalStatus.Error),
								this.Description, MessageBoxButtons.YesNo, DialogResult.No);

						if (result == DialogResult.Yes)
						{
							foreach (RequestType approval in latestApprovals)
							{
								if (approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Error)
								{
									ShowEditFormForRelatedTransaction(approval);
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("565F8EF0-86C3-4444-AC76-40DE09F79616", "Can't Approve - These approvals are NOT in allowed {0} status: {1}",
							GetAllowedStatusList(allowedStatuses), jobNumberBuilder.ToStringWithDelimiterBetweenAppends(", ")));
					}
				}
				else
				{
					var formBizo = GetNewApprovalBulk(newFactory, GetSecurityOverrideProvider(approvals), latestApprovals);
					var notAllowedToApproveMessage = formBizo.NotAllowedToApproveMessage;
					if (!string.IsNullOrEmpty(notAllowedToApproveMessage))
					{
						Globals.Message.ShowError(notAllowedToApproveMessage);
					}
					else
					{
						HandleApproveCore(formBizo);
					}
				}
			}
		}

		protected virtual void HandleApproveCore(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk)
		{
			if (approvalBulk.Approve())
			{
				var form = GetNewApprovalBulkForm(approvalBulk, TransactionApprovalFormModes.Approve);
				ShowForm(form);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("0d7fda5b-b9e7-4fda-af78-53b6b5c8ecc1", "You don't have rights to approve selected requests."));
			}
		}

		#region Handle Reject

		void HandleReject(object sender, EventArgs e)
		{
			BusinessObject[] approvals = this.Grid.SelectedElements;
			if (approvals.Length == 0)
			{
				ShowNoSelectedMessage();
			}
			else
			{
				var latestApprovals = GetLatestStatus(approvals, out BusinessObjectFactory newFactory);
				if (CheckApprovalsEligibilityAndShowErrorMessageWhenInvalid(latestApprovals))
				{
					var formBizo = GetNewApprovalBulk(newFactory, GetSecurityOverrideProvider(approvals), latestApprovals);
					HandleRejectCore(formBizo);
				}
			}
		}

		bool CheckApprovalsEligibilityAndShowErrorMessageWhenInvalid(RequestType[] latestApprovals)
		{
			var jobNumberBuilderForStatusFailures = new ZStringBuilder();
			var jobNumberBuilderForPostedFailures = new ZStringBuilder();
			var jobNumberBuilderForHavingRejectionFailures = new ZStringBuilder();

			var allowedStatuses = new List<string>() { Constants.GenApprovalRequestApprovalStatus.Requested };
			if (IsErrorStatusSupported)
			{
				allowedStatuses.Add(Constants.GenApprovalRequestApprovalStatus.Error);
			}

			foreach (RequestType approval in latestApprovals)
			{
				if (!allowedStatuses.Contains(approval.XP_ApprovalStatus.ToString()))
				{
					jobNumberBuilderForStatusFailures.Append(approval.ReferenceID);
				}
				else if (approval.TransactionIsAlreadyPosted && !PostedTransactionCanBeRejected)
				{
					jobNumberBuilderForPostedFailures.Append(approval.ReferenceID);
				}
				else if (!IsTransactionEligibleToCreateRejectionRequest(approval))
				{
					jobNumberBuilderForHavingRejectionFailures.Append(approval.ReferenceID);
				}
			}

			if (jobNumberBuilderForStatusFailures.Length > 0)
			{
				Globals.Message.ShowError(Res.GetString("DA249CF3-0F29-42E5-B702-90B215BCF8A3", "Can't Reject - These approvals are NOT in allowed {0} status: {1}",
					GetAllowedStatusList(allowedStatuses), jobNumberBuilderForStatusFailures.ToStringWithDelimiterBetweenAppends(", ")));
			}
			else if (jobNumberBuilderForPostedFailures.Length > 0)
			{
				Globals.Message.ShowError(Res.GetString("ef3d56cd-c0f5-411e-aa34-6d12d9c15446", "Can't Reject - Transactions for these approvals have already been posted. Please Cancel these approval requests: {0}",
					jobNumberBuilderForPostedFailures.ToStringWithDelimiterBetweenAppends(", ")));
			}
			else if (jobNumberBuilderForHavingRejectionFailures.Length > 0)
			{
				Globals.Message.ShowError(Res.GetString("8F4CE601-61BD-4B0C-8F2E-D545CB1E6471", "Can't Reject - Transactions for these approvals are not eligible for rejecting or they have already created rejection request. You can reject only commercial invoices. Please Cancel these approval requests: {0}",
					jobNumberBuilderForHavingRejectionFailures.ToStringWithDelimiterBetweenAppends(", ")));
			}
			return
				jobNumberBuilderForStatusFailures.Length == 0
				&& jobNumberBuilderForPostedFailures.Length == 0
				&& jobNumberBuilderForHavingRejectionFailures.Length == 0;
		}

		protected virtual bool DoRejectAction(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk) => approvalBulk.Reject();

		protected virtual bool IsTransactionEligibleToCreateRejectionRequest(RequestType approvalRequest) => true;

		protected virtual void HandleRejectCore(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk)
		{
			if (DoRejectAction(approvalBulk))
			{
				ShowApprovalBulkForm(approvalBulk, TransactionApprovalFormModes.Reject);
			}
			else
			{
				ShowHasNoRightsErrorMessage();
			}
		}

		protected void ShowApprovalBulkForm(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk, TransactionApprovalFormModes actionMode)
		{
			var form = GetNewApprovalBulkForm(approvalBulk, TransactionApprovalFormModes.Reject);
			ShowForm(form);
		}

		protected void ShowHasNoRightsErrorMessage() => Globals.Message.ShowError(Res.GetString("1ee68562-48b3-4aab-be4d-90008a5d0a95", "You don't have rights to reject selected requests."));

		#endregion

		#region Handle Cancel

		protected void HandleCancel(object sender, EventArgs e)
		{
			var securityCheckPoint = GetNewController(null).GetCheckPointForDelete(null);
			if (!securityCheckPoint.IsAllowed)
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				BusinessObject[] approvals = this.Grid.SelectedElements;
				int failedCount = 0;
				int toFixCount = 0;

				var failedRefIDSet = new SortedSet<string>();
				var fixedRefIDSet = new SortedSet<string>();
				var skippedRefIDSet = new SortedSet<string>();

				if (approvals.Length == 0)
				{
					ShowNoSelectedMessage();
				}
				else
				{
					var latestApprovals = GetLatestStatus(approvals, out BusinessObjectFactory newFactory);
					foreach (RequestType approval in latestApprovals)
					{
						var fixResult = AutoFixInvalidCancelApprovals(approval);
						if (fixResult != CancelStatus.Valid)
						{
							toFixCount++;

							if (fixResult == CancelStatus.Fixed)
							{
								fixedRefIDSet.Add(approval.ReferenceID);
							}
							else if (fixResult == CancelStatus.InvalidButSkipped)
							{
								skippedRefIDSet.Add(approval.ReferenceID);
							}
						}
						else if (toFixCount <= 0 && IsApprovalInvalidToCancel(approval))
						{
							failedCount++;
							failedRefIDSet.Add(approval.ReferenceID);
						}
					}

					if (toFixCount > 0)
					{
						if (latestApprovals.Length > 1)
						{
							Globals.Message.ShowWarning(CreateCancelMessage(latestApprovals, fixedRefIDSet, skippedRefIDSet));
						}
					}
					else if (failedCount > 0)
					{
						Globals.Message.ShowError(
							Res.GetString("541b9906-656c-4d47-9f7e-97afb158c595", "Can't Cancel - These approvals are already posted, canceled, or have unfinished Electronic Invoicing process: {0}",
							new ZStringBuilder(failedRefIDSet).ToStringWithDelimiterBetweenAppends(", ")));
					}
					else
					{
						var formBizo = GetNewApprovalBulk(newFactory, GetSecurityOverrideProvider(approvals), latestApprovals);
						var notAllowedToCancelMessage = formBizo.NotAllowedToCancelMessage;
						if (!string.IsNullOrEmpty(notAllowedToCancelMessage))
						{
							Globals.Message.ShowError(notAllowedToCancelMessage);
						}
						else
						{
							HandleCancelCore(formBizo);
						}
					}
				}
			}
		}

		protected virtual void HandleCancelCore(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk)
		{
			if (approvalBulk.Cancel())
			{
				var form = GetNewApprovalBulkForm(approvalBulk, TransactionApprovalFormModes.Cancel);
				ShowForm(form);
			}
			else
			{
				Globals.Message.ShowError(AccountingConstants.CancelRequestsErrorMessage);
			}
		}

		protected bool IsApprovalInvalidToCancel(RequestType approval) =>
			approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled ||
			approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted ||
			approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.ApprovalRequested ||
			approval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.RejectionRequested;

		protected virtual CancelStatus AutoFixInvalidCancelApprovals(RequestType approval) => CancelStatus.Valid;

		string CreateCancelMessage(RequestType[] latestApprovals, SortedSet<string> fixedRefIDSet, SortedSet<string> skippedRefIDSet)
		{
			var strBuilder = new ZStringBuilder();

			strBuilder.AppendLine(Res.GetString("2350C8C7-4DB7-4ca9-A538-E76B02AB2A62", "Batch Cancel Aborted."));
			if (fixedRefIDSet.Count > 0)
			{
				strBuilder.AppendLine(Res.GetString("08D68B10-178C-4972-B190-C6132A476873", "Some approvals are invalid and fixed: {0}", new ZStringBuilder(fixedRefIDSet).ToStringWithDelimiterBetweenAppends(", ")));
			}
			if (skippedRefIDSet.Count > 0)
			{
				strBuilder.AppendLine(Res.GetString("44B6D4E6-524F-4A99-9F97-FFEDFCFD7FD5", "Some approvals are invalid but remain unchanged: {0}", new ZStringBuilder(skippedRefIDSet).ToStringWithDelimiterBetweenAppends(", ")));
			}
			if (latestApprovals.Length > fixedRefIDSet.Count + skippedRefIDSet.Count)
			{
				strBuilder.AppendLine(Res.GetString("B9AB6E04-F514-404D-B79F-D805C77A5FD7", "You may select the rest approvals in the batch, and cancel them manually."));
			}

			return strBuilder.ToString();
		}

		#endregion

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			MenuItem approveMenuItem = new ZMenuItem(ResString.GetMultilingualString("7b43acae-6ecc-4f48-9621-3c0536bc6818", "&Approve"), new EventHandler(HandleApprove));
			menuItems.Add(approveMenuItem);
			MenuItem rejectMenuItem = new ZMenuItem(ResString.GetMultilingualString("20d5f83d-c9c0-4d6e-9330-938ad50d700d", "&Reject"), new EventHandler(HandleReject));
			menuItems.Add(rejectMenuItem);
			MenuItem cancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("ddfaba42-d45c-4163-9560-49bce0c8255f", "&Cancel"), new EventHandler(HandleCancel));
			menuItems.Add(cancelMenuItem);
			return menuItems.ToArray();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected abstract TransactionApprovalBulk<TransactionType, RequestType, DetailsType> GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params RequestType[] approvalRequests);
		protected abstract ZForm GetNewApprovalBulkForm(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> bizo, TransactionApprovalFormModes actionMode);

		protected virtual InteractiveSecurityOverrideProvider GetSecurityOverrideProvider(BusinessObject[] approvals)
		{
			return new InteractiveSecurityOverrideProvider();
		}

		protected virtual bool IsErrorStatusSupported
		{
			get { return false; }
		}

		protected virtual bool IsRelatedTransactionEditSupported
		{
			get { return false; }
		}

		protected virtual bool PostedTransactionCanBeRejected => true;

		protected virtual void ShowEditFormForRelatedTransaction(RequestType request)
		{
		}

		protected void ShowForm(ZForm form)
		{
			var parentForm = ParentModalForm ?? LocateMainForm();
#if DEBUG
			if (Globals.IsTest && parentForm == null)
			{
				ZFormModaliser.LastFormShownForTest = form;
				form.Show();
			}
			else
#endif
			{
				ZFormModaliser.Show(form, parentForm);
			}
		}

		static string GetAllowedStatusList(IEnumerable<string> allowedStatuses)
		{
			return string.Format(allowedStatuses.Count() > 1 ? "({0})" : "{0}", new ZStringBuilder(allowedStatuses).ToStringWithDelimiterBetweenAppends(", "));
		}

		RequestType[] GetLatestStatus(BusinessObject[] approvals, out BusinessObjectFactory newFactory)
		{
			newFactory = new BusinessObjectFactory();
			return newFactory.Load<RequestType>(new ZQuery(GenApprovalRequestSchema.PK, approvals.Select(x => x.PK)));
		}

		protected class DecisionProvider : DefaultModuleDecisionProvider
		{
			public DecisionProvider(TransactionApprovalModule<TransactionType, RequestType, DetailsType> module)
				: base(module)
			{
			}

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				((TransactionApprovalModule<TransactionType, RequestType, DetailsType>)Module).HandleViewClick(null, null);
			}
		}
	}

	//Move enum here to avoid CA1000
	public enum CancelStatus
	{
		Valid,
		Fixed,
		InvalidButSkipped
	}
}
