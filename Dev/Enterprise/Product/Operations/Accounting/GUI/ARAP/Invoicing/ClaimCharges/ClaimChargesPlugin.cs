using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.InvoicingApproval;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.ARAP
{
	public class ClaimChargesPlugin : ZPlugIn
	{
		public ClaimChargesPlugin(IBusiness hostEntity)
			: base(hostEntity)
		{
		}

		#region Overrides

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new ClaimChargesUserControl();
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return (NoResString)"Claim Charges"; } // Hard-coded constant
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return InternalCreditNote;
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return InternalCreditNote != null;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			CoveringLabelText = String.Empty;
			if (ClaimMustBeSaved)
			{
				CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|TheClaimIsNotSavedYouMustSaveTheClaimBeforeClaimChargesAllocation", "The Claim is not saved. You must save the Claim before claim charges allocation.");
			}
			else if (Claim.IsIntercompanyClaim || Claim.Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|ThisClaimDoesntHaveAnyChargesAllocated", "The claim doesn't have any charges allocated.");
			}
			else
			{
				if (Claim.IsClosed || Claim.IsClosedByOriginalValue)
				{
					CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|ClaimIsClosedItDoesNotAllowClaimChargesAllocation", "Claim is closed. It does not allow claim charges allocation.");
				}
				else if (Claim.Debtor == null || Claim.AY_OH_DebtorInfo.HasErrors())
				{
					CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|ClaimCreditorMustBeEnteredBeforeClaimChargesAllocation", "Claim Creditor must be entered before claim charges allocation.");
				}
				else if (Claim.Debtor.IsProxyOrgOfAnyCompany(true))
				{
					if (InternalCreditNote == null)
					{
						var apClaim = (APAccQueryClaim)Claim;
						var relatedCreditNoteAllowance = apClaim.CheckIsRelatedCreditNoteAllowed(returnReason: true);
						if (!relatedCreditNoteAllowance.IsAllowed)
						{
							CoveringLabelText = relatedCreditNoteAllowance.ReasonWhyNoAllowed;
						}
						else if (Claim.TransactionHeader == null || Claim.AY_AHInfo.HasErrors())
						{
							CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|ClaimInvoiceNumberMustBeEnteredBeforeClaimChargesAllocation", "Claim Invoice Number must be entered before claim charges allocation.");
						}
						else if (QueryUser(Res.GetString("b686f82a-3a04-4aed-9945-4804b4422dc2", "Do you want to allocate claim charges now?"), Res.GetString("a3644b77-ef5e-476b-9cd6-8e9bb75f1c67", "Allocate Claim Charges"), false))
						{
							InternalCreditNote = apClaim.CreateAndAttachRelatedCreditNote();
						}
						else
						{
							CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|YouHaveChosenNotToAllocateClaimChargesNoteNowPleaseChangeToAnotherTabThenClickBackToThisTabToCreateACreditNoteForThisClaim", "You have chosen not to allocate claim charges note now.\r\nChange to another tab, then click back to this tab to create a Credit Note for this Claim.");
						}
					}
				}
				else
				{
					CoveringLabelText = Res.GetString("Accounting|ClaimChargesPlugin|ClaimChargesCanOnlyBeAllocatedAndAnUnapprovedAPCreditNoteCreatedWhenTheCreditorOnTheClaimIsAnOrganisationProxyForAnotherLoginCompanyOrBranch", "Claim charges can only be allocated and an unapproved AP credit note created when the creditor on the claim is an organization proxy for another login company or branch.");
				}
			}
			return InternalCreditNote != null;
		}

		public override void Delete()
		{
			TabPage.ClearNotificationImage();
			Claim.CancelRelatedUnapprovedCreditNote();
			InternalCreditNote = null;
			ClaimMustBeSaved = true;
			DiscardCurrentUserControl();
			SynchroniseIfTabPageVisible();
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();

			BindUserControlIfRequired();

			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				((ClaimChargesUserControl)this.UserControl).AH_OSExtraTaxAmountCalcEdit.CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var continueWithSave = base.ShowPreSaveDialogsCore();

			if (continueWithSave == ContinueWithSave.Yes)
			{
				var invoicingbase = GetBusinessEntityForPlugIn() as InvoicingBase;
				SecurityOverrideProviderSource.Get(invoicingbase).Provider = new InvoicingSecurityOverrideProvider();
				var preSaveActionResult = new InvoicingPreSaveHelper().PreSaveActions(invoicingbase, ApprovalGUIProvider, false);

				if (!preSaveActionResult.CanProceed)
				{
					Globals.Message.ShowError(preSaveActionResult.ErrorMessage);
					continueWithSave = ContinueWithSave.No;
				}
			}
			return continueWithSave;
		}

		BaseInvoicingFormApprovalGUIProvider ApprovalGUIProvider
		{
			get { return approvalGUIProvider ?? (approvalGUIProvider = new BaseInvoicingFormApprovalGUIProvider(this.Form)); }
		}
		BaseInvoicingFormApprovalGUIProvider approvalGUIProvider;

		#endregion

		public void ClaimWasSuccessfullySaved()
		{
			ClaimMustBeSaved = false;
			SynchroniseIfTabPageVisible();
		}

		#region Implementation

		ZString CoveringLabelText;
		bool ClaimMustBeSaved { get; set; }

		bool QueryUser(string question, string caption, bool showInTest)
		{
			DialogResult result = DialogResult.Yes;
			if (!Globals.IsTest || showInTest)
			{
				result = Globals.Message.Show(question, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			}
			return result == DialogResult.Yes;
		}

		AccQueryClaimBase Claim
		{
			get { return HostBusinessEntity as AccQueryClaimBase; }
		}

		UACreditNote InternalCreditNote
		{
			get
			{
				if (internalCreditNote == null)
				{
					UnboundUserControl();
					internalCreditNote = Claim.RelatedUnapprovedCreditNote;
					if (internalCreditNote != null && internalCreditNote.IsInDatabase)
					{
						internalCreditNote.SetReadOnlyIncludingChildren(true);
						if (Claim.IsIntercompanyClaim)
						{
							internalCreditNote.SuspendValidation();
						}
					}
				}
				return internalCreditNote;
			}
			set
			{
				if (internalCreditNote != value)
				{
					UnboundUserControl();
					internalCreditNote = value;
					if (internalCreditNote == null)
					{
						ResetBusinessEntityToNull();
					}
					Claim.RelatedUnapprovedCreditNote = value;
				}
			}
		}
		UACreditNote internalCreditNote;

		void BindUserControlIfRequired()
		{
			if (!isUserControlBound)
			{
				if (!TabPage.Controls.Contains(UserControl))
				{
					TabPage.AddPlugInUserControl();
				}

				((ZUserControl)UserControl).SetDataBinding(InternalCreditNote, "");
				isUserControlBound = true;
			}
		}

		void UnboundUserControl()
		{
			((ZUserControl)UserControl).SetDataBinding(null, "");
			isUserControlBound = false;
		}

		bool isUserControlBound;

		#endregion
	}
}
