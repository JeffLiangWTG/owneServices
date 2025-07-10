using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	/// <summary>
	/// RegisterMasterPassword page for the web site
	/// </summary>
	public partial class RegisterMasterPassword : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			return new MasterPasswordManager(IdentityManager?.Contact?.Person);
		}

		protected MasterPasswordManager Manager => DataSource as MasterPasswordManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IdentityManager.Contact == null || Manager.Person == null || Manager.Person.HasPassword)
			{
				HideLabelsAndShowError(Res.GetString("41262508-69b4-44db-8007-3be7f1dd8907", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator."));
				return;
			}

			ErrorMessage.Visible = false;

			var defaultItem = PasswordSourceContactRepeater.Items.Cast<RepeaterItem>().FirstOrDefault();
			var defaultRadioButton = (ZRadioButton)defaultItem?.FindControl("Checked");
			if (defaultRadioButton != null)
			{
				defaultRadioButton.Checked = true;
			}
		}

		void HideLabelsAndShowError(string errorMessage)
		{
			ContactsBox.Visible = false;
			ErrorMessage.Text = errorMessage;
			ErrorMessage.Visible = true;
		}

		protected void UseExistingButton_Click(object sender, EventArgs e)
		{
			var checkedItem = PasswordSourceContactRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
				(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				((ZRadioButton)item.FindControl("Checked")).Checked);

			if (checkedItem != null)
			{
				var companyCode = (ZTextLabel)checkedItem.FindControl("CompanyCode");
				var email = (ZTextLabel)checkedItem.FindControl("Email");
				var passwordHoldingContacts = Manager.PasswordHoldingContacts.Cast<OrgContact>().ToArray();
				var passwordSource = passwordHoldingContacts.FirstOrDefault(x => x.OrganisationCode.EqualsIgnoringCase(companyCode.Text) && x.OC_Email.EqualsIgnoringCase(email.Text));

				if (passwordSource == null)
				{
					ErrorReporter.ReportOnce("Organization code or email mismatch", FormattableString.Invariant($"Mismatched Organization Code: {companyCode.Text}, Mismatched Email: {email.Text}, Login Contact Organization Codes: {string.Join(",", passwordHoldingContacts.Select(x => x.OrganisationCode))}, Login Contact Emails: {string.Join(",", passwordHoldingContacts.Select(x => x.OC_Email))}"));
					ShowContactError(Res.GetString("67059f9a-551f-45e0-a369-864657283560", "There was an error while loading this contact. Please refresh the page. If the issue persists, please raise an incident."));
					return;
				}

				if (Env.CurrentUser == null)
				{
					AppInstance.SetupSession(sender, e, false);
				}

				var passwordManager = new EDIWebUserAdminManager(IdentityManager.Contact);
				passwordManager.CopyMasterPasswordFromContact(passwordSource);

				RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
			}
			else
			{
				if (IdentityManager.Contact == null || Manager.Person == null)
				{
					ShowContactError(PageExpiredMessage);
				}
				else
				{
					ShowContactError(Res.GetString("9efb0eb2-5ae9-484a-bcd8-d97c88037db6", "Please select a contact"));
				}
			}

			void ShowContactError(string text)
			{
				ErrorMessage.Text = text;
				ErrorMessage.Visible = true;
			}
		}

		protected void SetNewPasswordButton_Click(object sender, EventArgs e)
		{
			try
			{
				var contact = IdentityManager.Contact;
				var token = IdentityManager.Token;
				if (contact == null)
				{
					if (string.IsNullOrEmpty(token))
					{
						HideLabelsAndShowError(SessionExpiredMessage);
						return;
					}

					var newFactory = new BusinessObjectFactory();
					var newIdentityManager = GetNewIdentityManager(newFactory);
					newIdentityManager.PopulatePropertiesFromToken(token);
					contact = newIdentityManager.Contact;

					if (contact == null)
					{
						var expiredTokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Token, token);
						expiredTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ExpiresAt, SQLComparisonOperator.LessThan, ZDateTime.UtcNow);
						var isTokenExpired = newFactory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, expiredTokenQuery);

						if (isTokenExpired)
						{
							HideLabelsAndShowError(SessionExpiredMessage);
						}
						else
						{
							ITokenizedAccessControl accessControl = new TokenizedAccessControl();
							var peekSuccessful = accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out var accessToken);
							ReportError(peekSuccessful, accessToken);
							HideLabelsAndShowError(Res.GetString("1f729199-8005-4fdd-8be1-6d128def2baa", "There was a problem loading your account. Please try to login again."));
						}

						return;
					}
				}

				var masterPasswordPageUrl = new Uri(AppInstance.SetMasterPasswordPage);
				var originalRequestUrl = LoginRouter.GetOriginalUrlFromRequest(Request) ?? new Uri(EDIDataRegistry.Instance.MyAccountIndexPage.Value);

				var setMasterPasswordUrl = SetMasterPasswordHelper.GenerateSetMasterPasswordUrl(contact, masterPasswordPageUrl, originalRequestUrl, token);
				Response.Redirect(setMasterPasswordUrl.IsAbsoluteUri ? setMasterPasswordUrl.AbsoluteUri : setMasterPasswordUrl.OriginalString);
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("RegisterMasterPassword.SetNewPasswordButton_Click()", ex);
			}
		}

		protected virtual void ReportError(bool peekSuccessful, AccessTokenInfo accessToken)
		{
			(new WebExceptionReporter()).ReportWebException(new DeveloperNotificationException("IdentityManager.Contact is null"), "IdentityManager.Contact is null", FormattableString.Invariant($"IdentityManager.Token value: {IdentityManager.Token}, Peek Successful: {peekSuccessful}, Token Parent ID: {accessToken.ParentId}, Token Parent Code: {accessToken.ParentTableCode}"));
		}
	}
}
