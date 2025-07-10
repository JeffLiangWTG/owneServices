using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimForm : ZForm
	{
		public AccQueryClaimForm()
		{
		}

		public AccQueryClaimForm(AccQueryClaimBase businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, ButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			if (Claim != null)
			{
				PlugIns.Add(ControllerIDs.ClaimCharges);
				Claim.Canceling += Claim_Cancelling;
			}
			SetupActionMenuItems();
		}

		AccQueryClaimBase Claim
		{
			get { return (AccQueryClaimBase)BusinessEntity; }
		}

		void SetupActionMenuItems()
		{
			if (Claim != null)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|AccQueryClaimForm|ReassignClaimMenuName", "&Reassign Claim"), ReassignClaim_Click);
				CancelClaimChargesMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("Accounting|AccQueryClaimForm|CancelClaimChargesMenuName", "&Cancel Claim Charges"), CancelClaimCharges_Click);
				TabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
				SetActionMenuItemsVisibility(false);
			}
		}

		#region ZForm Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			accQueryClaimUserControl.SetAccountTypeTextAndBinding(Claim.Ledger);
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return TabControl; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("AccQueryClaimForm|FormCaption", "Claim/Query") + " " + Claim.AY_QueryClaimReference; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave saveResult;
			using (Env.StartContextSwitchTrace(new UserContextSwitchLogger()))
			{
				saveResult = base.ValidateAndSave();
			}

			ClaimChargesPlugin plugIn = (ClaimChargesPlugin)PlugIns.GetPlugIn(ControllerIDs.ClaimCharges);
			if (saveResult == ContinueWithSave.Yes)
			{
				var uaCreditNote = Claim.RelatedUnapprovedCreditNote;
				if (uaCreditNote != null && !uaCreditNote.LastWarningMessage.IsEmpty)
				{
					Globals.Message.ShowWarning(uaCreditNote.LastWarningMessage);
				}
				plugIn.ClaimWasSuccessfullySaved();
			}
			return saveResult;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Implementation

		MenuItem CancelClaimChargesMenuItem;

		void CancelClaimCharges_Click(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("99c9cd58-74b2-4227-8143-36f2c9ffdfea", @"The Claim will be saved after canceling Claim Charges.
Do you really want to cancel the Claim Charges and save the Claim?"), Res.GetString("66e4391e-bf64-4d1a-9a53-cd7d9a72c823", "Cancel"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				ZPlugIn plugIn = PlugIns.GetPlugIn(ControllerIDs.ClaimCharges);
				plugIn.Delete();
			}
		}

		void ReassignClaim_Click(object sender, EventArgs e)
		{
			AccQueryClaimReassignAction reassignAction = new AccQueryClaimReassignAction(Claim);
			reassignAction.SetDefaultsFromClaim();
			ZFormModaliser.ShowDialogAndDispose(new AssignClaimPopupForm(reassignAction));
		}

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetActionMenuItemsVisibility(TabControl.SelectedTab == PlugIns.GetPlugIn(ControllerIDs.ClaimCharges).TabPage);
			CancelClaimChargesMenuItem.Enabled = !Claim.ReadOnly && !Claim.IsIntercompanyClaim && Claim.RelatedUnapprovedCreditNote != null;
		}

		void SetActionMenuItemsVisibility(bool isClaimChargesTabSelected)
		{
			foreach (MenuItem menuItem in ActionsMenuItem.MenuItems)
			{
				menuItem.Visible = menuItem == CancelClaimChargesMenuItem ? isClaimChargesTabSelected : !isClaimChargesTabSelected;
			}
		}

		void Claim_Cancelling(object sender, CancelEventArgs e)
		{
			if (Claim.RelatedUnapprovedCreditNote != null)
			{
				e.Cancel = Globals.Message.Show(Res.GetString("cf2df201-4f93-4029-ae1d-8b567dd20174", @"This claim has claim charges that have not yet been approved by your sister company.
Setting the status to 'CRD', 'CLS' or 'CAN' will cancel the claim charges and will close the claim.
Do you want to proceed?"), Res.GetString("71304a7e-4e67-4469-91e9-fb9083aab2b0", "Claim Closing"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No;
				if (!e.Cancel)
				{
					PlugIns.GetPlugIn(ControllerIDs.ClaimCharges).Delete();
				}
			}
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

