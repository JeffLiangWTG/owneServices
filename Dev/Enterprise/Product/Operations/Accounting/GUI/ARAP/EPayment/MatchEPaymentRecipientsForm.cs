using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsForm : AccountingZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public MatchEPaymentRecipientsForm()
		{
		}

		public MatchEPaymentRecipientsForm(MatchEPaymentRecipients matchEPaymentRecipients)
			: base(matchEPaymentRecipients)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, PostButton);
		}

		public MatchEPaymentRecipients MatchEPaymentRecipients => base.BusinessEntity as MatchEPaymentRecipients;

		public MatchEPaymentRecipientsControl MatchRecipientsControl_ForTestOnly => matchRecipientsControl;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.matchRecipientsControl.BackColor = BackColor;
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//this is to suppress the ZForm Save changes dialog when close button is clicked.
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = ContinueWithSave.No;
			if (matchRecipientsControl.SelectedBeneficiaries != null && matchRecipientsControl.SelectedBeneficiaries.Count() == 1)
			{
				var beneficiary = matchRecipientsControl.SelectedBeneficiaries.First();
				MatchEPaymentRecipients.SetAccountDetailsValuesFromBeneficiary(beneficiary);
				result = ContinueWithSave.Yes;
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("AA4B808B-9302-49FE-8891-BFD5F80C0B8A", "You must select exactly one recipient from the grid."));
			}
			return result;
		}

		internal void SyncButton_Click(object sender, EventArgs e)
		{
			if (MatchEPaymentRecipientsHelper.RunPreSynchronizeCheck(MatchEPaymentRecipients))
			{
				MatchEPaymentRecipientsHelper.CreateBeneficiaryRequest(MatchEPaymentRecipients);
				matchRecipientsControl.TabControl.SelectTab("SyncRecipientTabPage");
			}
		}
	}
}

