using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Admin : BasePage
	{
		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = ChangeMasterPasswordPerson.New(SiteUser?.LoggedInUser?.Person);
			return result;
		}

		protected ChangeMasterPasswordPerson PersonWrapper => DataSource as ChangeMasterPasswordPerson;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			CurrentPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			base.OnLoad(e);
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
			BackLink.NavigateUrl = AppInstance.HostingSiteRoot;

			if (SiteUser == null || !SiteUser.IsLoggedIn)
			{
				HideSetPasswordContentAndShowMessage(Res.GetString("efffb72f-3bf0-4f21-8ee7-60655456cb6c", "Please log in before attempting to change your password."));
			}
			else if (!SiteUser.LoggedInUser.Person.HasPassword)
			{
				ContactsBox.Visible = false;
				ChangePasswordInstructionsLabel.Visible = false;
			}
			else
			{
				RelatedAccounts.Text = PersonWrapper.RelatedAccounts;
			}
		}

		protected void Update_Click(object sender, EventArgs e)
		{
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				if (SiteUser.IsSuperUser)
				{
					PasswordChangeMessage.Text = "Cannot change password for Support";
				}
				else
				{
					var contact = SiteUser.LoggedInUser;
					var manager = new EDIWebUserAdminManager(contact);
					PasswordChangeMessage.Text = contact.Person.HasPassword
						? manager.ChangeMasterPassword(CurrentPassword.Text, NewPassword.Text, NewPasswordConfirm.Text)
						: manager.ChangePassword(CurrentPassword.Text, NewPassword.Text, NewPasswordConfirm.Text);

					if (PasswordChangeMessage.Text == manager.PasswordChangeSuccess)
					{
						HideSetPasswordContent();
					}
				}
			}
			else
			{
				HideSetPasswordContentAndShowMessage(Res.GetString("db91b457-3945-44e8-ba30-ac1bfb830a16", "Your session has expired. Please login again before attempting to change your password."));
				Logout();
				Session.Abandon();
			}
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			HideSetPasswordContent();

			PasswordChangeMessage.Text = message;
		}

		void HideSetPasswordContent()
		{
			ContactsBox.Visible = false;
			ChangePasswordInstructionsLabel.Visible = false;
			CurrentPassword.Visible = false;
			NewPassword.Visible = false;
			NewPasswordConfirm.Visible = false;
			Update.Visible = false;
			passwordChangeRequirements.Visible = false;
		}

		protected override bool ShowLoginStatus => false;
	}
}
