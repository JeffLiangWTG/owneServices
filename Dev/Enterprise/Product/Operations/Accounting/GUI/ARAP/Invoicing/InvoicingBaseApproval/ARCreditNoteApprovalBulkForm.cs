using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class ARCreditNoteApprovalBulkForm : ARCreditNoteApprovalBulkFormForDesigner
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ARCreditNoteApprovalBulkForm()
		{
		}

		public ARCreditNoteApprovalBulkForm(ARCreditNoteApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
			var details = bo.Approvals[0].PostingDetails;
			if (details.ApprovingOption != ApprovalCredentialOption.SingleLogin)
			{
				ApprovingUserCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d93b1df6-d078-4287-b655-c3291fced65e", "Approving User 1");
			}

			var isSequentialLogin = details.ApprovingOption == ApprovalCredentialOption.SequentialLogin;
			ApprovingUser2CodeFindBox.Visible = details.ApprovingOption != ApprovalCredentialOption.SingleLogin;
			ApprovingUser3CodeFindBox.Visible = isSequentialLogin && details.MaxAuthorisationLevelRequired > 2;
			ApprovingUser4CodeFindBox.Visible = isSequentialLogin && details.MaxAuthorisationLevelRequired > 3;
			ApprovingUser5CodeFindBox.Visible = isSequentialLogin && details.MaxAuthorisationLevelRequired > 4;
			ApprovingUser6CodeFindBox.Visible = isSequentialLogin && details.MaxAuthorisationLevelRequired > 5;
			MissingResourceStringChecker.ExcludeFromTest(RelatedRequestsLabel);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetFormLabelsAndCaptions();
			ShowSupplyTypeColumn();

			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				DetailsGrid.RemoveFromAvailableColumns("PlaceOfSupply");
			}
		}

		void ShowSupplyTypeColumn()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				DetailsGrid.RemoveFromAvailableColumns("SupplyType");
			}
		}

		void SetFormLabelsAndCaptions()
		{
			var approvals = ((ARCreditNoteApprovalBulk)BusinessEntity).Approvals;
			var approvalRequest = approvals[0];
			JobNumberTextBox.CaptionResourceString = approvalRequest.JobNumberTextBoxResString;
			branchGuidFindBox.CaptionResourceString = approvalRequest.JobBranchTextBoxResString;
			deptGuidFindBox.CaptionResourceString = approvalRequest.JobDepartmentTextBoxResString;
			if (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (approvals.Count == 1)
				{
					RelatedRequestsGroupBox.CaptionResourceString = approvalRequest.RelatedRequestsdGridTitle;
				}
				else
				{
					RelatedRequestsLabel.Visible = false;
				}
				RelatedRequestsGrid.ReadOnly = true;
			}
			else
			{
				RelatedRequestsPanel.Visible = false;
			}
		}
	}

#if DEBUG
	// This ZForm serves as base class only. It's not abstract so that it can be open in designer tool. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public class ARCreditNoteApprovalBulkFormForDesigner : TransactionApprovalBulkForm<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteApprovalBulkFormForDesigner()
		{
		}

		public ARCreditNoteApprovalBulkFormForDesigner(ARCreditNoteApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}
	}
}
