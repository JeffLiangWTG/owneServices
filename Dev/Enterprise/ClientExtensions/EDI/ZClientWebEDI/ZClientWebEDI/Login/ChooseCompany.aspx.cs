using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class ChooseCompany : RoutingEnabledPage
	{
		protected override bool ShowLoginStatus => false;

		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsPostBack)
			{
				return;
			}

			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);

			if (!IsQueryValid)
			{
				ShowInvalidTokenMessage();
				return;
			}

			var repeaterItems = LoginContactRepeater.Items.Cast<RepeaterItem>().ToArray();
			var defaultItem = repeaterItems.FirstOrDefault();
			var defaultRadioButton = (ZRadioButton)defaultItem?.FindControl("Checked");

			if (defaultRadioButton != null)
			{
				defaultRadioButton.Checked = true;
			}
		}

		bool IsQueryValid => !string.IsNullOrEmpty(QueryToken) && ContactCandidatePKs.Count > 1;

		protected override bool ShouldSetupSessionOnLoad => IsQueryValid;

		string InvalidTokenMessage => Res.GetString("08930591-11ed-47c6-84cf-458afdc0bf90", "The link is invalid. Please attempt login again and if this issue is recurring, raise an incident.");

		void ShowInvalidTokenMessage()
		{
			LoginContactsDiv.Visible = false;
			SignInButton.Visible = false;
			ShowError(InvalidTokenMessage);
		}

		void ShowError(string errorMessage)
		{
			Message.Text = errorMessage;
			Message.Visible = true;
		}

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var loginMan = new ChooseCompanyManager(AppInstance, ContactCandidatePKs);
			return loginMan;
		}

		protected ChooseCompanyManager ChooseCompanyManager => (ChooseCompanyManager)DataSource;

		List<Guid> ContactCandidatePKs
		{
			get
			{
				if (contactCandidatePKs == null)
				{
					contactCandidatePKs = new List<Guid>();

					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					peekSuccessful = accessControl.TryPeek(QueryToken, AccessTokenTypes.LoginRouterMultiContactIdentity, out accessToken);
					if (!peekSuccessful || string.IsNullOrEmpty(accessToken.Scope))
					{
						return contactCandidatePKs;
					}

					var splitScope = accessToken.Scope.Split(':');

					if (splitScope.Length != 2 || !ZBool.TryParse(splitScope[0], out var rememberMe))
					{
						return contactCandidatePKs;
					}

					RememberMe = rememberMe;

					var pkStrings = splitScope[1].Split(',');

					foreach (var pkString in pkStrings)
					{
						if (Guid.TryParse(pkString, out var pk))
						{
							contactCandidatePKs.Add(pk);
						}
					}
				}

				return contactCandidatePKs;
			}
		}

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
						catch (QueryStringException) { }

						if (queryString != null)
						{
							queryToken = queryString[LoginRouter.IdentityTokenQueryStringKey];
							originalUrl = queryString[LoginRouter.OriginalUrlQueryStringKey];
						}
					}
				}

				return queryToken;
			}
		}

		string queryToken;
		string originalUrl;

		protected bool RememberMe { get; private set; }
		List<Guid> contactCandidatePKs;
		AccessTokenInfo accessToken;
		bool peekSuccessful;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected void SignInButton_Click(object sender, EventArgs e)
		{
			var checkedItem = LoginContactRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
				(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				((ZRadioButton)item.FindControl("Checked")).Checked);

			if (checkedItem == null)
			{
				ShowError(Res.GetString("df906dd8-ea8a-4a5c-9647-b3770969a444", "Please select a contact"));
				return;
			}

			var companyCodeControl = (ZTextLabel)checkedItem.FindControl("CompanyCode");
			var companyCode = companyCodeControl.Attributes["value"];
			var contactForLogin = ChooseCompanyManager.LoginContacts.Cast<OrgContact>().FirstOrDefault(x => x.OrganisationCode.EqualsIgnoringCase(companyCode));

			if (contactForLogin != null)
			{
				if (RememberMe)
				{
					ChooseCompanyManager.SetRememberMe(contactForLogin);
				}

				ITokenizedAccessControl accessControl = new TokenizedAccessControl();
				accessControl.TryConsume(QueryToken, AccessTokenTypes.LoginRouterMultiContactIdentity, out _);
				RedirectViaLoginRouter(originalUrl, contactForLogin);
			}
			else
			{
				var loginContacts = ChooseCompanyManager.LoginContacts.Cast<OrgContact>();
				ErrorReporter.ReportOnce("ChooseCompany.aspx contact not found", FormattableString.Invariant($"Mismatched Organization Code: {companyCodeControl.Text}, Login Contact Organization Codes: {string.Join(",", loginContacts.Select(x => x.OrganisationCode))}"));
				ShowError(Res.GetString("60a56a04-56be-4eb0-96ba-390319778fe2", "There was an error while logging in. Please attempt to login again. If the issue persists, please raise an incident."));
			}
		}
	}
}
