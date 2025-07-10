using System;
using System.Net;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class SetMasterPassword : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new MasterPasswordManager(Person);
			return result;
		}

		protected MasterPasswordManager Manager => DataSource as MasterPasswordManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			if (string.IsNullOrEmpty(QueryToken))
			{
				ShowInvalidTokenMessage();
				return;
			}

			if (!IsPostBack)
			{
				if (Person == null || !AccessControl.TryPeek(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken))
				{
					ShowInvalidTokenMessage();
				}
			}

			base.OnLoad(e);
		}

		bool IsQueryValid => !string.IsNullOrEmpty(QueryToken) && Person != null;

		protected override bool ShouldSetupSessionOnLoad => IsQueryValid;

		protected void Update_Click(object sender, EventArgs e)
		{
			if (Contact != null && Person != null)
			{
				var webUserAdminManager = new EDIWebUserAdminManager(Contact);

				var message = webUserAdminManager.SetMasterPassword(NewPassword.Text, NewPasswordConfirm.Text, PasswordInstructionType.Set);
				PasswordChangeMessage.Text = message;

				if (message == webUserAdminManager.PasswordChangeSuccess)
				{
					if (AccessControl.TryConsume(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken))
					{
						HideSetPasswordContentAndShowMessage(message);
						Factory.Save();
						PasswordChangeMessage.CssClass = "SuccessMessage";

						if (!SiteUser.IsLoggedIn)
						{
							RedirectViaLoginRouter(RedirectUrl, Contact);
						}
						else
						{
							if (!string.IsNullOrEmpty(RedirectUrl))
							{
								Response.Redirect(RedirectUrl);
							}
						}
					}
				}
			}
			else
			{
				ShowInvalidTokenMessage();
			}
		}

		void ShowInvalidTokenMessage()
		{
			HideSetPasswordContentAndShowMessage(InvalidTokenMessage);
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			SetMasterPasswordHeadingLabel.Visible = false;
			ContactsBox.Visible = false;
			NewPassword.Visible = false;
			NewPasswordConfirm.Visible = false;
			Update.Visible = false;
			passwordChangeRequirements.Visible = false;

			PasswordChangeMessage.Text = message;
		}

		#region Properties

		internal GlbPerson Person
		{
			get
			{
				if (person == null)
				{
					PopulateContactAndPersonFromToken();
				}

				return person;
			}
		}

		GlbPerson person;

		internal OrgContact Contact
		{
			get
			{
				if (contact == null)
				{
					PopulateContactAndPersonFromToken();
				}

				return contact;
			}
		}

		OrgContact contact;

		void PopulateContactAndPersonFromToken()
		{
			if (AccessToken.ParentId != Guid.Empty && AccessToken.ParentTableCode == OrgContactSchema.Constants.Prefix)
			{
				contact = Factory.Load<OrgContact>(AccessToken.ParentId);
				person = contact.Person;
			}
		}

		string RedirectUrl => AccessToken.Scope;

		ITokenizedAccessControl AccessControl { get; } = new TokenizedAccessControl();

		string QueryToken
		{
			get
			{
				if (string.IsNullOrEmpty(queryToken) || !peekSuccessful)
				{
					var queryStringData = Request.QueryString[SecureQueryString.QueryStringKey];
					if (!string.IsNullOrEmpty(queryStringData))
					{
						SecureQueryString queryString = null;

						try
						{
							queryString = new SecureQueryString(WebUtility.UrlDecode(queryStringData));
						}
						catch (QueryStringException)
						{
						}

						if (queryString != null)
						{
							queryToken = queryString[WebUserAdminManager.SetMasterPasswordKey];
						}
					}
					else
					{
						queryToken = Request.QueryString[WebUserAdminManager.SetMasterPasswordKey];
					}
				}

				return queryToken;
			}
		}

		string queryToken;

		AccessTokenInfo AccessToken
		{
			get
			{
				if (!peekSuccessful && !string.IsNullOrEmpty(QueryToken))
				{
					peekSuccessful = AccessControl.TryPeek(QueryToken, AccessTokenTypes.SetMasterPassword, out accessToken);
				}

				return accessToken;
			}
		}
		AccessTokenInfo accessToken;
		bool peekSuccessful;

		string InvalidTokenMessage => Res.GetString("cc019681-2110-40e5-916e-847cb03c130a", "The set link you have followed is invalid or expired.");

		#endregion Properties
	}
}
