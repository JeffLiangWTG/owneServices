#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public enum TransactionApprovalFormModes
	{
		SetDescription,
		Approve,
		Reject,
		Cancel,
		View
	}

	public abstract partial class TransactionApprovalBulkForm<TransactionType, RequestType, DetailsType> : ZForm, IButtonPostTextOverride, IButtonApplyTextOverride
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public TransactionApprovalBulkForm()
		{
		}

		public TransactionApprovalBulkForm(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> bo, TransactionApprovalFormModes actionMode)
			: base(bo)
		{
			ActionMode = actionMode;
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, IsSaveButtonHidden);
			ReasonDescriptionTextBox.ReadOnly = IsReasonDescriptionReadOnly;
			isSingleRequestMode = bo.Approvals.Count == 1;
			TopGridPanel.Visible = !isSingleRequestMode;
			TopSingleRequestPanel.Visible = isSingleRequestMode;
			DisplayMode = ODisplayMode.Edit;
		}

		protected readonly bool isSingleRequestMode;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected virtual bool IsPostingSupported => false;

		protected virtual bool IsSaveButtonHidden => !IsPostingSupported || ActionMode != TransactionApprovalFormModes.Approve;

		protected readonly TransactionApprovalFormModes ActionMode;

		TransactionApprovalBulk<TransactionType, RequestType, DetailsType> ApprovalBulk => DataSource as TransactionApprovalBulk<TransactionType, RequestType, DetailsType>;

		protected override ContinueWithSave ValidateAndSave()
		{
			try
			{
				var result = base.ValidateAndSave();

				if (result == ContinueWithSave.Yes
					&& isApplyButtonClicked)
				{
					PostApprovalsAndRemovePosted(ApprovalBulk);

					if (ApprovalBulk.Approvals.Count > 0)
					{
						if (isSingleRequestMode)
						{
							var userReplyToEditSingleErroredRow = Globals.Message.Show(
								Res.GetString("84760D3D-F992-4838-BA5F-475DDA7456D9", "Error(s) have been found during posting. They can be fixed possibly by editing the related {0}. Do you want to edit it now?\r\nErrors:\r\n{1}", string.Join(" / ", ApprovalBulk.Approvals.Select(x => x.ReferenceType.ToLower()).Distinct()), string.Join("\r\n", ApprovalBulk.Approvals[0].RowErrors.Select(rowError => rowError.Message))),
								Res.GetString("98C781D7-97EC-4B71-9B3C-5BD9C59D6B91", "Posting Errors"),
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Error);

							if (userReplyToEditSingleErroredRow == DialogResult.Yes)
							{
								Edit(ApprovalBulk.Approvals.ToArray());
							}

							Close();
						}
						else
						{
							AddEditMenuItemToApprovals();

							Globals.Message.ShowError(Res.GetString("C8A4D818-0C10-48EC-8C87-EC989AB024EC", "Requests that remained in the Approval Requests grid are not posted due to errors found. They can be fixed possibly by editing the related {0}. Right click > select Edit on the Approval Requests grid to edit the {0}.", string.Join(" / ", ApprovalBulk.Approvals.Select(x => x.ReferenceType.ToLower()).Distinct())));
						}
					}
					else
					{
						Close();
					}
				}

				return result;
			}
			finally
			{
				isApplyButtonClicked = false;
			}
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			if (ActionMode != TransactionApprovalFormModes.SetDescription && ActionMode != TransactionApprovalFormModes.View)
			{
				base.Save(factories);
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		public virtual bool IsReasonDescriptionReadOnly => ActionMode != TransactionApprovalFormModes.SetDescription;

		public override string FormVerb
		{
			get
			{
				string text = base.FormVerb;
				switch (ActionMode)
				{
					case TransactionApprovalFormModes.Approve:
						text = Res.GetString("CE604929-8063-4C1C-A183-8C953D8FD8E3", "Approve");
						break;
					case TransactionApprovalFormModes.Reject:
						text = Res.GetString("E15AF72D-5E8A-48F5-B6F3-D38205605C39", "Reject");
						break;
					case TransactionApprovalFormModes.Cancel:
						text = Res.GetString("281BB911-FE4D-46E2-9197-7DBD074542B6", "Cancel");
						break;
					case TransactionApprovalFormModes.SetDescription:
						text = string.Empty;
						break;
				}

				return text;
			}
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//Do nothing to prevent asking user to save data
		}

		protected override bool AllowNew => false;

		string IButtonPostTextOverride.PostButtonText
		{
			get
			{
				string text = string.Empty;
				if (ActionMode == TransactionApprovalFormModes.SetDescription)
				{
					text = Res.GetString("E82F8037-5718-4952-8D85-E0D7A3FB4F1D", "Continue");
				}

				return text;
			}
		}
		string IButtonApplyTextOverride.ApplyButtonText => Res.GetString("5AB45C65-DEA5-4BCC-B728-E04625B2EFA0", "Save && Post");

		#region Posting

		void Edit(RequestType[] approvals)
		{
			if (approvals.Length == 0)
			{
				Globals.Message.Show(Res.GetString("5E35E5D9-81F2-49D2-81F7-EF43C5EB4D87", "Please select at least one approval."));
			}
			else
			{
				var errorMessages = new List<string>();

				foreach (RequestType approval in approvals)
				{
					var newFactory = new BusinessObjectFactory();
					SetPostContext(newFactory);
					var reloadedApproval = newFactory.Load<RequestType>(approval.PK);

					if (reloadedApproval != null)
					{
						if (reloadedApproval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
						{
							ZString errorMessage;
							var requestParent = GetRequestParent(reloadedApproval, out errorMessage);
							if (requestParent != null)
							{
								BusinessObject objectToEdit;
								var controllerID = GetControllerIDForEditing(requestParent, out objectToEdit);
								if (controllerID != null)
								{
									var controller = ZControllerFactory.Create(controllerID);
									var lastEditForm = controller.ShowEditForm(objectToEdit);
#if DEBUG
									OnEditFormShown_ForTestOnly?.Invoke(lastEditForm, null);
#endif
									ApprovalBulk.Approvals.RemoveFromRelationship(approval);
								}
								else
								{
									errorMessages.Add(Res.GetString("288CC897-B3A4-440E-AEDA-B7FEA75B0FF1", "Cannot open corresponding {0} for request ({1}).",
										reloadedApproval.ReferenceType.ToLower(),
										reloadedApproval.ReferenceID));
								}
							}
							else
							{
								errorMessages.Add(Res.GetString("97FC266E-8D08-4153-94D7-70FFAE9C77D5", "A {0} can’t be found for request ({1}).{2}",
									reloadedApproval.ReferenceType.ToLower(),
									reloadedApproval.ReferenceID,
									errorMessage.IsEmpty ? "" : System.Environment.NewLine + errorMessage));
							}
						}
						else if (reloadedApproval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted
							|| reloadedApproval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled)
						{
							errorMessages.Add(Res.GetString("FFAB9945-3078-4438-8A13-F18C96449C2B", "Request ({0}) is already {1}.",
								reloadedApproval.ReferenceID,
								reloadedApproval.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted
									? Res.GetString("143baa1b-4908-4ed1-bc3e-75f09d34ec19", "Posted")
									: Res.GetString("892a6e11-186c-4538-b172-deef69d1ecfe", "Canceled")));

							ApprovalBulk.Approvals.RemoveFromRelationship(approval);
						}
						else
						{
							errorMessages.Add(Res.GetString("5984244E-1305-4131-BE8E-C486551B8979", "Can’t Edit request ({0}) - only approved request can be edited here. Its status now is ‘{1}’.", reloadedApproval.ReferenceID, reloadedApproval.XP_ApprovalStatus));
						}
					}
					else
					{
						errorMessages.Add(Res.GetString("1F849647-8DD0-4BC5-9E35-CAD114AD25AD", "Request ({0}) cannot be found.", approval.ReferenceID));
					}
				}

				if (errorMessages.Count > 0)
				{
					Globals.Message.ShowWarning(string.Join("\r\n", errorMessages));
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021: Avoid out parameters")]
		protected virtual BusinessObject GetRequestParent(RequestType request, out ZString errorMessage)
		{
			throw new NotImplementedException();
		}

		protected virtual void SetPostContext(BusinessObjectFactory newFactory)
		{
			throw new NotImplementedException();
		}

		[SuppressMessage("Microsoft.Design", "CA1021: Avoid out parameters")]
		protected virtual ControllerID GetControllerIDForEditing(BusinessObject requestParent, out BusinessObject objectToEdit)
		{
			throw new NotImplementedException();
		}

		protected virtual void PostApprovalsAndRemovePosted(TransactionApprovalBulk<TransactionType, RequestType, DetailsType> approvalBulk)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		public Action<object, EventArgs> OnEditFormShown_ForTestOnly;
#endif

		void AddEditMenuItemToApprovals()
		{
			if (!isEditMenuItemAdded)
			{
				TopGrid.ContextMenu.MenuItems.Add(ResString.GetMultilingualString("2D31EC19-3161-4B56-913E-11BB96FE120C", "Edit"), HandleEdit);
				isEditMenuItemAdded = true;
			}
		}

		bool isEditMenuItemAdded;

		void HandleEdit(object sender, EventArgs e)
		{
			var seletedApprovals = TopGrid.SelectedElements.Cast<RequestType>();
			Edit(seletedApprovals.ToArray());

			if (ApprovalBulk.Approvals.Count == 0)
			{
				Close();
			}
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			isApplyButtonClicked = true;

			base.OnApplyButtonClick(sender, e);
		}

		bool isApplyButtonClicked;

		#endregion
	}
}
