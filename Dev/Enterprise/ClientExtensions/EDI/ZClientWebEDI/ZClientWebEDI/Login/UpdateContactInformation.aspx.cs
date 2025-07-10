using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class UpdateContactInformation : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PopulatePersonalInfo();
		}

		protected override bool ShowLoginStatus => false;

		void PopulatePersonalInfo()
		{
			if (!IdentityManager.IsValidID())
			{
				HideSaveEmailLabelsTextBoxesAndButtons(Res.GetString("5f8a6dcc-6a0c-4c39-998b-782e713d394f", "The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists."));
				return;
			}

			StoreTokenDebugInfo();

			if (IdentityManager.Contact.Person == null)
			{
				HideSaveEmailLabelsTextBoxesAndButtons(Res.GetString("73de8580-43a9-4d19-8f73-0e5cc3cb73a8", "Your personal account has an error. Please contact your system administrator if this persists."));
				return;
			}

			if (!IdentityManager.Contact.Person.PER_EmailAddress.IsEmpty)
			{
				HideSaveEmailLabelsTextBoxesAndButtons(Res.GetString("ede73607-a604-430c-9f2a-bfb7879ea05c", "Your personal recovery email has already been setup. To change it, go to your MyAccount Profile Menu => Contact Information."));
				return;
			}

			if (!IsPostBack)
			{
				ContinueButton.Attributes["style"] = "display:none";
				GreetingLabel.Text = Res.GetString("a56873a2-4f39-4da0-bb5d-63dd2b432f50", "Hi {0},", IdentityManager.Contact.OC_ContactName);
				RegisterPersonalEmailMessage.InnerHtml = EDIDataRegistry.Instance.RegisterPersonalEmailPageFooterText.Value.Replace("\r\n", "<br>");
			}
		}

		void StoreTokenDebugInfo()
		{
			if (TokenCreationTime != ZDateTime.MinSmallDateTimeValue)
			{
				return;
			}

			var dbToken = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, IdentityManager.Token));
			if (dbToken != null)
			{
				TokenCreationTime = dbToken.SAT_SystemCreateTimeUtc;
				TokenExpiryTime = dbToken.SAT_ExpiresAt;
			}
		}

		ZDateTime TokenCreationTime
		{
			get
			{
				return Session["TokenCreationTime"] != null ? (ZDateTime)Session["TokenCreationTime"] : ZDateTime.MinSmallDateTimeValue;
			}
			set
			{
				Session["TokenCreationTime"] = value;
			}
		}

		ZDateTime TokenExpiryTime
		{
			get
			{
				return Session["TokenExpiryTime"] != null ? (ZDateTime)Session["TokenExpiryTime"] : ZDateTime.MinSmallDateTimeValue;
			}
			set
			{
				Session["TokenExpiryTime"] = value;
			}
		}

		void HideSaveEmailLabelsTextBoxesAndButtons(string message)
		{
			ContactInformationBox.Visible = false;
			ErrorMessage.Text = message;
		}

		protected void SkipButton_OnClick(object sender, EventArgs e)
		{
			var ediPerson = IdentityManager.Contact?.Person as EDIGlbPerson;
			if (ediPerson == null)
			{
				HideSaveEmailLabelsTextBoxesAndButtons(PageExpiredMessage);
			}
			else
			{
				ediPerson.StorePersonalEmailPromptSkip(DoNotAskMeAgainCheckBox.Checked);
				ediPerson.Factory.Save();

				RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
			}
		}

		protected void SaveEmailButton_OnClick(object sender, EventArgs e)
		{
			var contact = IdentityManager.Contact;
			if (contact == null)
			{
				var token = IdentityManager.Token;
				if (string.IsNullOrEmpty(token))
				{
					HideSaveEmailLabelsTextBoxesAndButtons(SessionExpiredMessage);
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
						HideSaveEmailLabelsTextBoxesAndButtons(SessionExpiredMessage);
					}
					else
					{
						ITokenizedAccessControl accessControl = new TokenizedAccessControl();
						var peekSuccessful = accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out var accessToken);

						if (peekSuccessful || TokenCreationTime != ZDateTime.MinSmallDateTimeValue || TokenExpiryTime != ZDateTime.MinSmallDateTimeValue)
						{
							ReportError(accessToken);
						}

						HideSaveEmailLabelsTextBoxesAndButtons(Res.GetString("457839d5-2cdc-4f98-9e2b-7e9315884ffa", "There was a problem loading your account. Please try to login again."));
					}

					return;
				}
			}

			var setPersonalPasswordHelper = new SetPersonalEmailHelper(Factory, this);
			var (success, errorMessage) = setPersonalPasswordHelper.SaveEmail(contact, PersonalEmailTextBox.Text);
			if (success)
			{
				UpdateContactInformationHeadingLabel.Text = "Account Verification";
				UpdateContactInformationInstructionsLabel.Text = Res.GetString("56737f15-ff2f-477c-a543-c8aeccb35056", "An email was sent to '{0}' to verify your personal recovery email. Please verify the email to complete the update.", PersonalEmailTextBox.Text);
				GreetingLabel.Visible = false;
				PersonalEmailTextBox.Visible = false;
				RegisterPersonalEmailMessage.Visible = false;
				DoNotAskMeAgainCheckBox.Visible = false;
				PersonalEmailTextBox.Visible = false;
				SaveEmailButton.Visible = false;
				SkipButton.Visible = false;
				DoNotAskMeAgainCheckBox.Visible = false;
				ContinueButton.Attributes["style"] = "display:inline";
				MessageLabel.Text = string.Empty;
			}
			else
			{
				MessageLabel.Text = errorMessage;
			}
		}

		void ReportError(AccessTokenInfo accessToken)
		{
			ReportErrorCore(FormattableString.Invariant($"IdentityManager.Token value: {IdentityManager.Token}, Token Parent ID: {accessToken.ParentId}, Token Parent Code: {accessToken.ParentTableCode}, Created at: {TokenCreationTime.ToBestReadableDateTimeString()}, Expires at: {TokenExpiryTime.ToBestReadableDateTimeString()}"));
		}

		protected virtual void ReportErrorCore(string message)
		{
			(new WebExceptionReporter()).ReportWebException(new DeveloperNotificationException("IdentityManager.Contact is null"), "IdentityManager.Contact is null", message);
		}

		protected void ContinueButton_OnClick(object sender, EventArgs e)
		{
			RedirectViaLoginRouter(LoginRouter.GetOriginalUrlFromRequest(Request), IdentityManager.Token);
		}
	}
}
