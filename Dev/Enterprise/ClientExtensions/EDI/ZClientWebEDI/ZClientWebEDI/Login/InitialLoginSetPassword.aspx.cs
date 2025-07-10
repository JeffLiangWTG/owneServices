using System;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class InitialLoginSetPassword : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override void OnLoad(EventArgs e)
		{
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			base.OnLoad(e);

			var contact = IdentityManager.Contact;

			if (contact != null)
			{
				OrgNameText.Text = contact.OrgAddress?.CompanyName ?? contact.Header.OH_FullName;
				OrgCodeText.Text = contact.Header.OH_Code;
			}
			else
			{
				HideSetPasswordContentAndShowMessage(InvalidLinkMessage);
				return;
			}

			if (contact.HasPassword)
			{
				HideSetPasswordContentAndShowMessage(PasswordAlreadySetMessage);
			}
		}

		protected override bool ShowLoginStatus => false;

		protected void Update_Click(object sender, EventArgs e)
		{
			var contact = IdentityManager.Contact;

			if (contact != null)
			{
				if (contact.HasPassword)
				{
					HideSetPasswordContentAndShowMessage(PasswordAlreadySetMessage);
				}
				else
				{
					var webUserAdminManager = new EDIWebUserAdminManager(contact);
					var result = webUserAdminManager.SetMasterPassword(NewPassword.Text, NewPasswordConfirm.Text, PasswordInstructionType.Set);

					PasswordChangeMessage.Text = result;

					if (result == webUserAdminManager.PasswordChangeSuccess)
					{
						RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
					}
				}
			}
			else
			{
				HideSetPasswordContentAndShowMessage(InvalidLinkMessage);
			}
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			OrgCodeLabel.Visible = false;
			OrgNameText.Visible = false;
			OrgCodeText.Visible = false;

			NewPassword.Visible = false;
			NewPasswordConfirm.Visible = false;

			Update.Visible = false;
			SetPasswordInstructionsLabel.Visible = false;

			PasswordChangeMessage.Text = message;
		}

		string InvalidLinkMessage => "You have been redirected here to set the initial password for your account; however, the link was invalid. Please attempt login again and if this issue is recurring, contact your system administrator.";
		string PasswordAlreadySetMessage => "You have been redirected here to set the initial password for your account; however, your password has already been set. Please attempt login again and if this issue is recurring, contact your system administrator.";
	}
}
