using System;
using System.Globalization;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class AccountVerificationControl : BaseUserControl
	{
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
			DataSourceAssemblyName = "ZClientWebEDI";
			DataSourceTypeName = "Enterprise.ZClientWebCargoWiseEDI.LoginOptionsHelper";
		}

		protected LoginOptionsHelper Helper => Page.DataSource as LoginOptionsHelper ?? (Page.DataSource as EDIResetMasterPasswordManager)?.LoginOptionsHelper;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!(Helper is LoginOptionsHelperForDistinctEmail))
			{
				Refresh();
			}
		}

		public void Refresh()
		{
			MessageLabel.Text = "";

			if (Helper.IsValid)
			{
				if (!Helper.HasPassword)
				{
					PasswordMessageLabel.Visible = false;
					PasswordTextbox.Visible = false;
					VerifyPasswordButton.Visible = false;
				}
				else
				{
					PasswordMessageLabel.Visible = true;
					PasswordTextbox.Visible = true;
					VerifyPasswordButton.Visible = true;
					Page.ZClientScript.RegisterStartupScript(GetType(), "SetupToggleButtonAvailabilityEvent",
						FormattableString.Invariant($@"
<script type=""text/javascript"" src=""../Scripts/buttontoggler.js""></script>
<script>
	setupToggleButtonAvailabilityEventListener(document.getElementById('{VerifyPasswordButton.ClientID}'), [document.getElementById('{PasswordTextbox.ClientID}')]);
</script>"), false);
				}

				ActionInstructionAccountReactivatedLabel.Visible =
					Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.AccountReactivated
					|| Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.ProductDeactivation
					|| Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.SelfDeactivation
					|| (!Helper.UserAccount.EUA_IsContactRelationshipActive && Helper.UserAccount.EUA_ContactRelationshipStatus.IsEmpty && Helper.UserAccount.EUA_IsActive && Helper.Contact.OC_IsActive);
				ActionInstructionEmailChangedLabel.Visible = Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.EmailChanged;
				ActionInstructionMultipleUIDLinkedLabel.Visible = Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.MultipleUserAccountsLinked;
				ActionInstructionContactMovedLabel.Visible = Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.DissolvedContactWithPassword;

				if (Helper.ContactRelationshipStatus == ContactRelationshipStatusList.Codes.DissolvedContactWithPassword)
				{
					ActionInstructionContactMovedLabel.Text = ActionInstructionContactMovedLabel.Text.Replace("(*retainedOrgCode*)", Helper.Contact.OrganisationCode);
				}

				if (!Helper.MaskedEmailAddress.IsEmpty)
				{
					EmailLabel.Text = Helper.MaskedEmailAddress;
					EmailMessageLabel.Visible = true;
					EmailLabel.Visible = true;
					SendVerificationEmailButton.Visible = true;
				}
				else
				{
					EmailMessageLabel.Visible = false;
					EmailLabel.Visible = false;
					SendVerificationEmailButton.Visible = false;
				}

				ERequestMessageLabel.Visible = true;

				return;
			}

			MessageLabel.Text = Res.GetString("a1b3005b-7df0-4b97-96bd-e1c81d723543", "The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.");
			ActionInstructionAccountReactivatedLabel.Visible = false;
			ActionInstructionEmailChangedLabel.Visible = false;
			ActionInstructionMultipleUIDLinkedLabel.Visible = false;
			PasswordMessageLabel.Visible = false;
			PasswordTextbox.Visible = false;
			VerifyPasswordButton.Visible = false;
			EmailMessageLabel.Visible = false;
			EmailLabel.Visible = false;
			SendVerificationEmailButton.Visible = false;
			ERequestMessageLabel.Visible = false;
		}

		protected void VerifyPasswordButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(PasswordTextbox.Text))
			{
				MessageLabel.Text = "Please input password!";
			}
			else
			{
				if (Helper.VerifyPassword(PasswordTextbox.Text))
				{
					if (ShouldRedirectAfterVerification)
					{
						var originalRequestUrl = LoginRouter.GetOriginalUrlFromRequest(Request);
						var redirectUrl = AutoLoginHelper.UserRoutingLogin(originalRequestUrl, Helper.UserAccount, Page.AppInstance, Helper.Contact);
						Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
					}

					AccountVerificationCompleted?.Invoke(null, null);
				}
				else
				{
					MessageLabel.Text = "Invalid Password!";
				}
			}
		}

		protected void SendVerificationEmailButton_Click(object sender, EventArgs e)
		{
			var originalUrl = LoginRouter.GetOriginalUrlFromRequest(Request);
			if (Helper.SendEmailVerification(originalUrl))
			{
				MessageLabel.Text = string.Format(CultureInfo.CurrentCulture, "Email Verification has been sent to {0}.", Helper.MaskedEmailAddress);
			}
			else
			{
				MessageLabel.Text = "Couldn't send an Email Verification.";
			}
		}

		public bool HeaderVisible
		{
			get => HeaderDiv.Visible;
			set => HeaderDiv.Visible = value;
		}

		public bool ShouldRedirectAfterVerification { get; set; } = true;

		public event EventHandler AccountVerificationCompleted;
	}
}
