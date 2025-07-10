using System;
using System.Globalization;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class SetPassword : BasePage
	{
		protected override void OnLoad(EventArgs e)
		{
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			accessControl = new TokenizedAccessControl();
			var peekSuccessfully = !string.IsNullOrEmpty(QueryToken) && accessControl.TryPeek(QueryToken, AccessTokenTypes.SetPassword, out accessToken);

			base.OnLoad(e);

			if (IsInLiteViewMode)
			{
				GoBackLoginLink.NavigateUrl = "../Login/LoginLite.aspx";
			}

			if (!IsPostBack)
			{
				if (!peekSuccessfully || !IsQueryValid)
				{
					HideSetPasswordContentAndShowMessage(InvalidTokenMessage);
					return;
				}

				OrgCodeText.Text = WebContact.Header.OH_Code;
				OrgNameText.Text = (WebContact as IGlbPersonPrimarySource)?.CompanyName ?? "";
			}

			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}

		protected override bool PageRequiresLogin(Uri url) => false;

		bool IsQueryValid
		{
			get
			{
				return !string.IsNullOrEmpty(QueryToken) && WebContact != null;
			}
		}

		protected override bool ShouldSetupSessionOnLoad => IsQueryValid;

		protected void Update_Click(object sender, EventArgs e)
		{
			if (WebContact != null)
			{
				if (Guid.TryParse(ResetInfo.EmailTemplateCompanyPk, out var companyPk))
				{
					WebContact.CompanyPKForEmailTemplate = companyPk;
				}

				var webUserAdminManager = new EDIWebUserAdminManager(WebContact);
				var result = webUserAdminManager.ChangePassword(NewPassword.Text, NewPasswordConfirm.Text, PasswordInstructionType.Set);

				PasswordChangeMessage.Text = result.Message;

				if (result.IsSuccess)
				{
					if (accessControl.TryConsume(QueryToken, AccessTokenTypes.SetPassword, out accessToken))
					{
						Factory.Save();
						HideSetPasswordContentAndShowMessage(result.Message, true);
						PasswordChangeMessage.CssClass = "SuccessMessage";
					}
				}
			}
			else
			{
				HideSetPasswordContentAndShowMessage(InvalidTokenMessage);
			}
		}

		void HideSetPasswordContentAndShowMessage(string message, bool savedSuccessfully = false)
		{
			this.SetPasswordInstructionsLabel.Visible = false;
			this.OrgCodeLabel.Visible = false;
			this.OrgCodeText.Visible = false;
			this.OrgNameText.Visible = false;
			this.NewPassword.Visible = false;
			this.NewPasswordConfirm.Visible = false;
			passwordChangeRequirements.Visible = false;

			this.Update.Visible = false;

			PasswordChangeMessage.Text = message;
			GoBackMessage.Visible = savedSuccessfully;
		}

		internal OrgContact WebContact
		{
			get
			{
				if (webContact == null && accessToken.ParentId != Guid.Empty)
				{
					webContact = Factory.Load<OrgContact>(accessToken.ParentId);
				}

				return webContact;
			}
		}

		OrgContact webContact;

		PasswordResetInfo ResetInfo
		{
			get
			{
				if (resetInfo == null)
				{
					if (!string.IsNullOrEmpty(accessToken.Scope) &&
						accessToken.Scope.StartsWith("{", StringComparison.OrdinalIgnoreCase) && accessToken.Scope.EndsWith("}", StringComparison.OrdinalIgnoreCase))
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
						resetInfo = new PasswordResetInfo();
					}
				}
				return resetInfo;
			}
		}
		PasswordResetInfo resetInfo;

		string QueryToken => HttpContext.Current.Request.QueryString[AppInstance.SetPasswordKey];

		string InvalidTokenMessage => "The set link you have followed is invalid or expired.";

		AccessTokenInfo accessToken;

		ITokenizedAccessControl accessControl;
	}
}
