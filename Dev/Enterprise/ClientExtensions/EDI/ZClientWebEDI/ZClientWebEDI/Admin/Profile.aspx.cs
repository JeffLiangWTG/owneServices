using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Environment;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Profile : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				if (SiteUser?.LoggedInOrgContact?.Person == null)
				{
					MessageLabel.Text = "The page has expired. Please login again.";
					return;
				}

				if (IsInLiteViewMode)
				{
					Breadcrumb.Visible = false;
				}

				PopulatePersonalInfo();

				if ((SiteUser.IsLoggedIn || SiteUser.LoggedInOrgContact.Person != null) && SiteUser.LoggedInOrgContact.Person.HasPassword)
				{
					return;
				}

				TabContainer.Visible = false;
			}
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return new UserAccountDeactivationManager(SiteUser?.LoggedInOrgContact?.Person);
		}

		protected UserAccountDeactivationManager Manager => DataSource as UserAccountDeactivationManager;

		#endregion

		void PopulatePersonalInfo()
		{
			if (SiteUser.IsLoggedIn)
			{
				this.UserNameLabel.Text = SiteUser.LoggedInUserName;
				this.CompanyNameLabel.Text = SiteUser.AffiliationName;
				this.EmailLabel.Text = SiteUser.LoggedInOrgContact.Email;

				var hasPersonalEmailAddress = false;
				if (SiteUser.LoggedInOrgContact.Person != null)
				{
					this.PersonalEmailLabel.Text = SiteUser.LoggedInOrgContact.Person.PER_EmailAddress;
					this.RegisteredPersonalEmail.Value = this.PersonalEmailLabel.Text;
					this.PersonalEmailTextBox.Attributes["style"] = "display:none";
					this.SaveEmailButton.Attributes["style"] = "display:none";
					hasPersonalEmailAddress = !SiteUser.LoggedInOrgContact.Person.PER_EmailAddress.IsEmpty;
				}

				SetPersonalEmailControlVisible(hasPersonalEmailAddress);

				footer.InnerHtml = EDIDataRegistry.Instance.RegisterPersonalEmailPageFooterText.Value.Replace("\r\n", "<br>");
			}
		}

		void SetPersonalEmailControlVisible(bool hasPersonalEmailAddress)
		{
			this.PersonalEmailLabel.Visible = hasPersonalEmailAddress;
			this.RegisterPersonalEmailButton.Visible = !hasPersonalEmailAddress;
			this.ChangePersonalEmailButton.Visible = hasPersonalEmailAddress;
		}

		protected void RegisterPersonalEmailButton_OnClick(object sender, EventArgs e)
		{
			ShowPersonalEmailTextBoxAndSaveEmailButton();
		}

		protected void ChangePersonalEmailButton_OnClick(object sender, EventArgs e)
		{
			ShowPersonalEmailTextBoxAndSaveEmailButton();
			this.PersonalEmailTextBox.Text = SiteUser.LoggedInOrgContact.Person.PER_EmailAddress;
		}

		void ShowPersonalEmailTextBoxAndSaveEmailButton()
		{
			this.PersonalEmailLabel.Visible = false;
			this.ChangePersonalEmailButton.Visible = false;
			this.RegisterPersonalEmailButton.Visible = false;

			this.PersonalEmailTextBox.Attributes["style"] = "display:inline";
			this.SaveEmailButton.Attributes["style"] = "display:inline";
		}

		protected void SaveEmailButton_OnClick(object sender, EventArgs e)
		{
			if (Env.CurrentUser == null)
			{
				AppInstance.SetupSession(null, EventArgs.Empty, false);
			}

			var emailAddress = this.PersonalEmailTextBox.Text;

			var setPersonalPasswordHelper = new SetPersonalEmailHelper(Factory, this);
			var result = setPersonalPasswordHelper.SaveEmail(SiteUser.LoggedInOrgContact, emailAddress);
			if (result.Item1)
			{
				SaveEmailButton.Attributes["style"] = "display:none";
				PersonalEmailTextBox.Attributes["style"] = "display:none";
				PersonalEmailLabel.Visible = true;

				if (SiteUser.LoggedInOrgContact.Person == null)
				{
					return;
				}

				var personalEmailAddress = SiteUser.LoggedInOrgContact.Person.PER_EmailAddress;
				PersonalEmailLabel.Text = personalEmailAddress;

				if (personalEmailAddress.IsEmpty)
				{
					RegisterPersonalEmailButton.Visible = true;
				}
				else
				{
					ChangePersonalEmailButton.Visible = true;
				}
			}

			MessageLabel.Text = result.Item2;
		}

		protected void SwitchPage_Click(object sender, EventArgs e)
		{
			if (TabPersonalEmail.Visible)
			{
				SwitchToUserAccountsPage();
			}
			else
			{
				if (UserAccountRelationship.HasChanges)
				{
					ConfirmationDiv.CssClass = CiModalOn;
				}
				else
				{
					SwitchToPersonalEmailPage();
				}
			}
		}

		void SwitchToPersonalEmailPage()
		{
			TabPersonalEmail.Visible = true;
			TabUserAccounts.Visible = false;
			ButtonUserAccounts.Enabled = true;
			ButtonPersonalEmail.Enabled = false;
		}

		void SwitchToUserAccountsPage()
		{
			TabPersonalEmail.Visible = false;
			TabUserAccounts.Visible = true;
			ButtonUserAccounts.Enabled = false;
			ButtonPersonalEmail.Enabled = true;
		}

		protected void ConfirmationYes_Click(object sender, EventArgs e)
		{
			UserAccountRelationship.SaveChangesButton_Click(sender, e);
			SwitchToPersonalEmailPage();
			ConfirmationDiv.CssClass = CiModalOff;
		}

		protected void ConfirmationNo_Click(object sender, EventArgs e)
		{
			SwitchToPersonalEmailPage();
			ConfirmationDiv.CssClass = CiModalOff;
		}

		protected void ConfirmationCancel_Click(object sender, EventArgs e)
		{
			ConfirmationDiv.CssClass = CiModalOff;
		}

		const string CiModalOff = "CiModalOff";
		const string CiModalOn = "CiModalOn";
	}
}
