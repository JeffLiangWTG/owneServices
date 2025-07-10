using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class DistinctEmailRequired : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override bool ShowLoginStatus => false;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				DistinctEmailRequiredMessage.InnerHtml = EDIDataRegistry.Instance.DistinctEmailRequiredPageFooterText.Value.Replace("\r\n", "<br>");
			}
		}

		#region Binding

		protected override BusinessObject GetNewDataSource() => new LoginOptionsHelperForDistinctEmail(Factory, IdentityManager.UserAccount?.WebAccessContact, IdentityManager.UserAccount);

		protected LoginOptionsHelperForDistinctEmail Helper => DataSource as LoginOptionsHelperForDistinctEmail;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion Binding

		protected override void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			AccountVerification.AccountVerificationCompleted += AccountVerification_AccountVerificationCompleted;
			AccountVerification.ShouldRedirectAfterVerification = false;

			if (!IdentityManager.IsValidID())
			{
				HideSetPasswordContentAndShowInvalidLinkMessage();
			}

			if (InstructionLabel.Text.Contains("{0}"))
			{
				if (Helper.IsValid)
				{
					InstructionLabel.Text = string.Format(CultureInfo.CurrentCulture, InstructionLabel.Text, Helper.UserAccountForDistinctEmail.EUA_Email);
				}
				else
				{
					HideSetPasswordContentAndShowInvalidLinkMessage();
				}
			}
		}

		void HideSetPasswordContentAndShowInvalidLinkMessage()
		{
			InstructionLabel.Text = Res.GetString("FCB6216F-5AA7-4DFB-BF90-B3012D5F6127", "The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.");
			EmailTextbox.Visible = false;
		}

		void InitializeComponent()
		{
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}

		protected void RegisterEmailButton_Click(object sender, EventArgs e)
		{
			RegisterEmail();
		}

		protected void RegisterEmail()
		{
			var newEmail = EmailTextbox.Text;
			if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(newEmail))
			{
				MessageLabel.Text = "Please enter a valid email address.";
				return;
			}

			var originalUrl = LoginRouter.GetOriginalUrlFromRequest(Request);
			if (Helper.RegisterNewEmail(newEmail, originalUrl))
			{
				MessageLabel.Text = string.Format(CultureInfo.CurrentCulture, "An email was sent to {0} to verify your work email address. Please verify the email to complete the update.", newEmail);
				if (EmailTextbox != null)
				{
					EmailTextbox.Visible = false;
				}

				if (RegisterEmailButton != null)
				{
					RegisterEmailButton.Visible = false;
				}

				if (InstructionLabel != null)
				{
					InstructionLabel.Visible = false;
				}
			}
			else
			{
				var userAccount = IdentityManager.UserAccount;

				if (userAccount == null || userAccount.WebAccessContact == null)
				{
					MessageLabel.Text = "The page has expired. Please login again.";
					return;
				}

				var email = EmailTextbox.Text;
				var duplicateContact = ContactImporter.GetDuplicateContactFromOrg(userAccount.WebAccessContact.OC_OH, email, userAccount.WebAccessContact, userAccount.Factory);
				var duplicateUserAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, duplicateContact.PK);
				duplicateUserAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, userAccount.EUA_LD);

				if (!userAccount.Factory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, duplicateUserAccountQuery))//if contact has any other user accounts from the same database then we should not let them in.
				{
					Helper.SetContactAndUserAccountValues(duplicateContact, userAccount);
					AccountVerification.Refresh();
					LoginOptionsDiv.Visible = true;
				}

				MessageLabel.Text = "Work Email is not unique, please enter another email.";
			}
		}

		protected void AccountVerification_AccountVerificationCompleted(object sender, EventArgs e)
		{
			LoginOptionsDiv.Visible = false;
			var userAccount = IdentityManager.UserAccount;
			var duplicateContact = Helper.Contact;

			if (duplicateContact != null)
			{
				CustomerUserAccountRelationshipStatusResolver.MergeToContact(userAccount, duplicateContact);
			}

			MessageLabel.Text = "Your web access has been extended to this account.";
			RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), userAccount);
		}
	}
}
