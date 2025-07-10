using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class ResetPassword : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			base.OnLoad(e);

			if (IsInLiteViewMode)
			{
				GoBackLoginLink.NavigateUrl = "../Login/LoginLite.aspx";
			}

			if (!IsPostBack)
			{
				if (!IsQueryValid)
				{
					HideResetPasswordContentAndShowMessage(InvalidTokenMessage);
					CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
					BackLink.NavigateUrl = AppInstance.HostingSiteRoot;
					return;
				}

				if (!string.IsNullOrWhiteSpace(ResetInfo.Product) && ResetInfo.Product == ProductTypes.Codes.BorderWise)
				{
					if (ResetInfo.NavigateUrl != null)
					{
						GoBackLoginLink.NavigateUrl = ResetInfo.NavigateUrl.ToString();
					}
					BackLink.Visible = false;
				}
			}

			AccountVerification.AccountVerificationCompleted += AccountVerification_AccountVerificationCompleted;
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
			BackLink.NavigateUrl = AppInstance.HostingSiteRoot;
			PasswordExpiredMessageLabel.Visible = string.Equals(Request.QueryString[PasswordRotationRoutingDescriptor.RefKey], PasswordRotationRoutingDescriptor.RefValue, StringComparison.OrdinalIgnoreCase);
		}

		bool IsQueryValid => !string.IsNullOrEmpty(QueryToken) && !string.IsNullOrEmpty(Email);

		protected override bool ShouldSetupSessionOnLoad => IsQueryValid;

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);

			if (!IsPostBack)
			{
				OrgCodeDropDownList_SelectedIndexChanged(null, null);
			}
		}

		protected override bool PageRequiresLogin(Uri url) => false;

		protected override void OnInit(EventArgs e)
		{
			if (!string.IsNullOrEmpty(QueryToken) && !string.IsNullOrEmpty(Email) && OrgContacts != null)
			{
				foreach (var contact in OrgContacts)
				{
					var longItemName = FormattableString.Invariant($"{contact.OrgCode} - {(contact as IGlbPersonPrimarySource)?.CompanyName ?? ""}");
					var shortItemName = longItemName.Length > 45 ? longItemName.Substring(0, 42).PadRight(45, '.') : longItemName;
					var item = new ListItem(shortItemName, contact.PK.ToString());
					item.Attributes.Add("Title", longItemName);
					OrgCodeDropDownList.Items.Add(item);
				}
			}

			base.OnInit(e);
		}

		public override void Dispose()
		{
			passwordChangeRequirements?.Dispose();
			base.Dispose();
		}

		protected void Update_Click(object sender, EventArgs e)
		{
			var selectedWebUser = GetSelectedWebUser();
			if (selectedWebUser != null)
			{
				if (Guid.TryParse(ResetInfo.EmailTemplateCompanyPk, out var companyPk))
				{
					selectedWebUser.CompanyPKForEmailTemplate = companyPk;
				}

				var webUserAdminManager = new EDIWebUserAdminManager(selectedWebUser, new HttpClient(GetHttpClientHandler()));
				var result = webUserAdminManager.ChangePassword(NewPassword.Text, NewPasswordConfirm.Text);
				PasswordChangeMessage.Text = result.Message;

				if (result.IsSuccess)
				{
					Helper.WriteLoginHashCookie(selectedWebUser.OrgCode, selectedWebUser.OC_Email);

					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					if (accessControl.TryConsume(QueryToken, AccessTokenTypes.ResetPassword, out accessToken))
					{
						Factory.Save();
						HideResetPasswordContentAndShowMessage(result.Message, true);
						PasswordChangeMessage.CssClass = "SuccessMessage";
					}
				}
			}
			else
			{
				HideResetPasswordContentAndShowMessage(InvalidTokenMessage);
			}
		}

		protected virtual HttpClientHandler GetHttpClientHandler() => new HttpClientHandler();

		void HideResetPasswordContentAndShowMessage(string message, bool savedSuccessfully = false)
		{
			this.OrgCodeLabel.Visible = false;
			this.OrgCodeDropDownList.Visible = false;
			this.NewPassword.Visible = false;
			this.NewPasswordConfirm.Visible = false;

			this.Update.Visible = false;
			this.BackLink.Visible = false;
			this.HeadingMessageDiv.Visible = false;
			passwordChangeRequirements.Visible = false;

			PasswordChangeMessage.Text = message;
			GoBackMessage.Visible = savedSuccessfully;
		}

		internal OrgContact GetSelectedWebUser()
		{
			if (ZGuid.TryParse(OrgCodeDropDownList.SelectedValue, out var contactPK))
			{
				return OrgContacts?.FirstOrDefault(x => x.PK == contactPK);
			}

			return null;
		}

		PasswordResetInfo ResetInfo
		{
			get
			{
				if (resetInfo == null)
				{
					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					var peekSuccessfully = accessControl.TryPeek(QueryToken, AccessTokenTypes.ResetPassword, out accessToken);
					if (!peekSuccessfully || string.IsNullOrEmpty(accessToken.Scope))
					{
						resetInfo = new PasswordResetInfo();
					}
					else
					{
						if (accessToken.Scope.StartsWith("{", StringComparison.OrdinalIgnoreCase) && accessToken.Scope.EndsWith("}", StringComparison.OrdinalIgnoreCase))
						{
							try
							{
								resetInfo = JsonConvert.DeserializeObject<PasswordResetInfo>(accessToken.Scope);
							}
							catch (JsonReaderException)
							{
								resetInfo = new PasswordResetInfo();
								ErrorReporter.ReportOnce($"The PasswordResetInfo:{accessToken.Scope} should be valid json string");
							}
						}
						else
						{
							resetInfo = new PasswordResetInfo() { ContactEmail = accessToken.Scope };
						}
					}
				}
				return resetInfo;
			}
		}

		PasswordResetInfo resetInfo;

		string Email => ResetInfo.ContactEmail;

		IEnumerable<OrgContact> OrgContacts
		{
			get
			{
				if (orgContacts == null && !string.IsNullOrWhiteSpace(Email))
				{
					var query = new ZDBOnlyQuery(typeof(OrgContact));
					query.AddToFilter(OrgContactSchema.OC_Email, Email);
					query.AddToFilter(OrgContactSchema.OC_IsActive, true);
					query.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
					if (!string.IsNullOrWhiteSpace(ResetInfo.OrgCode))
					{
						var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, OrgContactSchema.OC_OH);
						orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, ResetInfo.OrgCode);
						query.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
					}
					orgContacts = Factory.Load<OrgContact>(query).OrderBy(x => x.OrgCode);
				}
				return orgContacts;
			}
		}
		IEnumerable<OrgContact> orgContacts;

		string QueryToken => HttpContext.Current.Request.QueryString[AppInstance.ResetPasswordKey];

		string InvalidTokenMessage => "The reset link you have followed is invalid or expired.";

		AccessTokenInfo accessToken;

		#region LoginOptions

		protected override BusinessObject GetNewDataSource() => new LoginOptionsHelper(Factory);

		protected LoginOptionsHelper Helper => (LoginOptionsHelper)DataSource;

		protected void OrgCodeDropDownList_SelectedIndexChanged(object sender, EventArgs e)
		{
			AccountVerification.HeaderVisible = false;
			AccountVerification.ShouldRedirectAfterVerification = false;
			PasswordDiv.Visible = true;
			LoginOptionsDiv.Visible = false;

			var selectedContact = (EDIOrgContact)GetSelectedWebUser();
			if (selectedContact != null)
			{
				var unlinkedUserAccount = selectedContact.GetMostRecentUnlinkedUserAccount();
				if (unlinkedUserAccount != null && !LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(unlinkedUserAccount))
				{
					PasswordDiv.Visible = false;
					LoginOptionsDiv.Visible = true;
					Helper.SetContactAndUserAccountValues(selectedContact, unlinkedUserAccount);
					AccountVerification.Refresh();
				}
			}
		}

		void AccountVerification_AccountVerificationCompleted(object sender, EventArgs e)
		{
			PasswordDiv.Visible = true;
			LoginOptionsDiv.Visible = false;
		}

		#endregion Binding
	}
}
