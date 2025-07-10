using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsBulkForm : AccountingZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MatchEPaymentRecipientsBulkForm()
		{
		}

		public MatchEPaymentRecipientsBulkForm(MatchEPaymentRecipientsBulk matchEPaymentRecipientsBulk)
			: base(matchEPaymentRecipientsBulk)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, MatchButton, CloseButton, UnmatchButton);
			HookEvents();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!this.IsDesignMode())
			{
				this.matchRecipientsControl.FilteredRecipientsGridCurrentItemChanged += RecipientsFilteredGrid_CurrentSelectionChanged;
				SetApplyButtonText();
			}
		}

		public MatchEPaymentRecipientsBulk MatchEPaymentRecipients => base.BusinessEntity as MatchEPaymentRecipientsBulk;

		public MatchEPaymentRecipientsControl MatchRecipientsControl_ForTestOnly => matchRecipientsControl;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.matchRecipientsControl.BackColor = BackColor;
			CheckBoxOverrideDefault.AllowOverlap(BankAccountGuidFindBox);
			CreditorGuidFindBox.AllowOverlap(CloseButton);
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//this is to suppress the ZForm Save changes dialog when close button is clicked.
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.No;
		}

		void RecipientsFilteredGrid_CurrentSelectionChanged(object sender, EventArgs e)
		{
			if (matchRecipientsControl.SelectedBeneficiaries != null)
			{
				var currentIndex = matchRecipientsControl.FilteredRecipientsGrid.CurrentRowIndex;
				if (currentIndex >= 0)
				{
					var beneficiary = matchRecipientsControl.FilteredRecipientsGrid.ListManager.List[currentIndex] as AccEPaymentBeneficiary;
					MatchEPaymentRecipients.CurrentBeneficiary = beneficiary;
					MatchEPaymentRecipients.CreditorPK = beneficiary.LinkedOrgHeader?.PK ?? ZGuid.Empty;
					MatchEPaymentRecipients.DefaultPaymentReason = beneficiary.LinkedAccountDetails?.A1_EPaymentReasonCode ?? ZString.Empty;
					MatchEPaymentRecipients.PaymentReferenceType = beneficiary.LinkedAccountDetails?.A1_EPaymentReferenceType ?? ZString.Empty;
					MatchEPaymentRecipients.PaymentReference = beneficiary.LinkedAccountDetails?.A1_EPaymentReference ?? ZString.Empty;
					MatchEPaymentRecipients.DefaultBankAccountPK = beneficiary.LinkedOrgBankAccountPK;
					MatchEPaymentRecipients.AgreedPaymentMethod = beneficiary.LinkedOrgAgreedPaymentMethod;
					MatchEPaymentRecipients.AllowOverrideDefault = false;
				}
				else
				{
					MatchEPaymentRecipients.CurrentBeneficiary = null;
					MatchEPaymentRecipients.CreditorPK = ZGuid.Empty;
					MatchEPaymentRecipients.DefaultPaymentReason = ZString.Empty;
					MatchEPaymentRecipients.DefaultBankAccountPK = ZGuid.Empty;
					MatchEPaymentRecipients.AgreedPaymentMethod = ZString.Empty;
					MatchEPaymentRecipients.AllowOverrideDefault = false;
					MatchEPaymentRecipients.PaymentReferenceType = ZString.Empty;
					MatchEPaymentRecipients.PaymentReference = ZString.Empty;
				}
				MatchEPaymentRecipients.CreditorPKInfo.RefreshBinding();
				MatchEPaymentRecipients.DefaultBankAccountPKInfo.RefreshBinding();
				MatchEPaymentRecipients.AgreedPaymentMethodInfo.RefreshBinding();
			}
		}

		internal void SyncButton_Click(object sender, EventArgs e)
		{
			if (MatchEPaymentRecipientsHelper.RunPreSynchronizeCheck(MatchEPaymentRecipients))
			{
				MatchEPaymentRecipientsHelper.CreateBeneficiaryRequest(MatchEPaymentRecipients);
				matchRecipientsControl.TabControl.SelectTab("SyncRecipientTabPage");
			}
		}

		internal void UnmatchButton_Click(object sender, EventArgs e)
		{
			if (MatchEPaymentRecipients.CurrentBeneficiary != null)
			{
				var selectedRecipient = MatchEPaymentRecipients.CurrentBeneficiary;
				if (selectedRecipient.IsMatchedWithOrg)
				{
					try
					{
						selectedRecipient.UnMatchOrg();
						Globals.Message.Show(Res.GetString("046CEEF3-0EB9-4F3E-B542-E21B360D0D08", "Payables Organization successfully unmatched"));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
					matchRecipientsControl.MatchEPaymentRecipientsFilterControl.FirePerformSearch();
					MatchEPaymentRecipients.RefreshBindingIncludingChildren();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("58C676A7-9F65-4BFE-B1EE-52E66EC5E8DA", "Please choose a matched recipient first"));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("138DB15B-6344-4845-9EF1-6ECF563BA988", "Please choose a recipient first"));
			}
		}

		internal void MatchButton_Click(object sender, EventArgs e)
		{
			if (MatchEPaymentRecipients.CurrentBeneficiary == null)
			{
				Globals.Message.ShowError(Res.GetString("138DB15B-6344-4845-9EF1-6ECF563BA988", "Please choose a recipient first"));
				return;
			}

			if (!MatchEPaymentRecipients.CreditorPK.IsValid)
			{
				Globals.Message.ShowError(Res.GetString("ED4DE5FF-C9C5-459B-9525-3513425C596F", "Please choose a valid creditor first"));
				return;
			}

			MatchEPaymentRecipients.ValidateAll();
			if (MatchEPaymentRecipients.HasErrors)
			{
				Globals.Message.ShowError(new ZNotificationCollector(MatchEPaymentRecipients, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString());
				return;
			}

			var selectedRecipient = MatchEPaymentRecipients.CurrentBeneficiary;
			if (selectedRecipient.IsMatchedWithOrg)
			{
				Globals.Message.ShowError(Res.GetString("3B9EBD47-AD23-4E15-8EC5-1E498B435195", "Please choose an unmatched recipient first"));
				return;
			}

			var creditorPK = MatchEPaymentRecipients.CreditorPK;
			var defaultBankAccountPK = MatchEPaymentRecipients.DefaultBankAccountPK;
			var agreedPaymentMethod = MatchEPaymentRecipients.AgreedPaymentMethod;
			var allowOverrideDefault = MatchEPaymentRecipients.AllowOverrideDefault;
			var defaultPaymentReason = MatchEPaymentRecipients.DefaultPaymentReason;
			var paymentReferenceType = MatchEPaymentRecipients.PaymentReferenceType;
			var paymentReference = MatchEPaymentRecipients.PaymentReference;

			var canContinueMatching = !defaultPaymentReason.IsEmpty || EPaymentHelper.GetUserConfirmationToContinueWithEmptyPaymentReason();
			if (canContinueMatching)
			{
				try
				{
					selectedRecipient.MatchOrg(creditorPK, defaultPaymentReason, defaultBankAccountPK, agreedPaymentMethod, allowOverrideDefault, selectedRecipient, paymentReferenceType, paymentReference);
					Globals.Message.Show(Res.GetString("979112F2-D91C-40DA-B62F-B1103C53E504", "Payables Organization successfully matched"));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
				}
				matchRecipientsControl.MatchEPaymentRecipientsFilterControl.FirePerformSearch();
				MatchEPaymentRecipients.RefreshBindingIncludingChildren();
			}
		}

		void PaymentReferenceType_OnChanged(object sender, EventArgs e)
		{
			TextPaymentReference.Visible = MatchEPaymentRecipients.PaymentReferenceType == EPaymentReferenceTypes.FreeText;
		}

		#region Hook and Unhook Events

		void HookEvents()
		{
			if (MatchEPaymentRecipients != null)
			{
				MatchEPaymentRecipients.HasChangesChanged += MatchEPaymentRecipients_HasChangesChanged;
				MatchEPaymentRecipients.OnPaymentReferenceTypeChanged += PaymentReferenceType_OnChanged;
			}
		}

		void UnhookEvents()
		{
			if (MatchEPaymentRecipients != null)
			{
				MatchEPaymentRecipients.HasChangesChanged -= MatchEPaymentRecipients_HasChangesChanged;
				MatchEPaymentRecipients.OnPaymentReferenceTypeChanged -= PaymentReferenceType_OnChanged;
			}
		}

		#endregion

		void SetApplyButtonText()
		{
			fApplyButton.Text = Res.GetString("CBEACA94-5C6C-423A-8890-B47523E5C526", "Unmatch");
			fPostButton.Text = Res.GetString("B9206AAE-89AF-4D5D-926D-9FA97DC146DE", "Match");
		}

		void MatchEPaymentRecipients_HasChangesChanged(object sender, EventArgs e)
		{
			SetApplyButtonText();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
			}
			base.Dispose(disposing);
		}
	}
}

