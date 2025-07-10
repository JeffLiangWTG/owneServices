using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimUserControl : ZUserControl
	{
		public AccQueryClaimUserControl()
		{
			InitializeComponent();
		}

		void ShowAddLogCommentPopupForm()
		{
			if (Claim == null)
			{
				((OrgQueryClaimDependentCollection)DataSource).AddNew();
			}
			ZFormModaliser.ShowDialogAndDispose(new AddLogCommentPopupForm(Claim));
		}

		DialogResult ShowAssignClaimPopupFormWithDefaults()
		{
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(Claim);
			action.SetDefaultsFromClaim();
			return ZFormModaliser.ShowDialogAndDispose(new AssignClaimPopupForm(action));
		}

		public void SetAccountTypeTextAndBinding(string ledgerType)
		{
			OrganisationGuidFindBox.GetExtension<LabelCaptionRenderer>().Caption =
				ledgerType == LedgerTypes.AccountsPayable ? Res.GetString("Accounting|Creditor", "Creditor") : Res.GetString("Accounting|Debtor", "Debtor");
		}

		#region Overrides

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			SetControlsReadOnly();
			SetControlsVisibility();
		}

		#endregion

		#region Handlers

		void AddToClaimLogButton_Click(object sender, EventArgs e)
		{
			ShowAddLogCommentPopupForm();
		}

		void RejectClaimButton_Click(object sender, EventArgs e)
		{
			if (ShowAssignClaimPopupFormWithDefaults() == DialogResult.OK)
			{
				Claim.Reject();
				DisableAprovingAndRejecting();
			}
		}

		void ApproveClaimButton_Click(object sender, EventArgs e)
		{
			ODisplayMode prevDisplayMode = ((ZForm)this.ParentForm).DisplayMode;
			object claimMemento = Claim.SaveToMementoForAssignClaimAction();
			if (ShowAssignClaimPopupFormWithDefaults() == DialogResult.OK)
			{
				try
				{
					Claim.Approve();
					DisableAprovingAndRejecting();
				}
				catch (InvalidOperationException ex)
				{
					Claim.RestoreMementoForAssignClaimAction(claimMemento);
					((ZForm)this.ParentForm).DisplayMode = prevDisplayMode;
					Globals.Message.ShowError(ex.Message, Res.GetString("f227d309-a28b-4288-b5ce-0308e1888725", "Claim Approving Failed."));
				}
				catch (JobCreationException ex)
				{
					Claim.RestoreMementoForAssignClaimAction(claimMemento);
					((ZForm)this.ParentForm).DisplayMode = prevDisplayMode;
					Globals.Message.ShowError(ex.Message, Res.GetString("f227d309-a28b-4288-b5ce-0308e1888725", "Claim Approving Failed."));
				}
			}
		}

		void DisableAprovingAndRejecting()
		{
			ApproveClaimButton.Enabled = false;
			RejectClaimButton.Enabled = false;
		}

		#endregion

		AccQueryClaimBase Claim
		{
			get { return (AccQueryClaimBase)BindingSource.Current; }
		}

		#region Implementation

		void SetControlsReadOnly()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool isIntercompanyClaim = false;

				if (Claim != null)
				{
					isIntercompanyClaim = Claim.IsIntercompanyClaim;

					AddToClaimLogButton.Enabled = !Claim.ReadOnly;
					HoldOptionEdit.Visible = Claim.IsHoldOptionVisible;
				}

				RejectClaimButton.Enabled = isIntercompanyClaim;
				ApproveClaimButton.Enabled = isIntercompanyClaim;
			}
		}

		void SetControlsVisibility()
		{
			IntercompanyClaimDetailsGroupBox.Visible = (Claim == null || Claim.AY_MasterBillNumber.IsEmpty);
			mawbTextBox.Visible = (Claim == null || !Claim.AY_MasterBillNumber.IsEmpty);
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

