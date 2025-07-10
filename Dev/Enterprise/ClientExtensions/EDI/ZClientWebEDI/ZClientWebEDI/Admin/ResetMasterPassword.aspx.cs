using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.Web.Admin;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class ResetMasterPassword : RoutingEnabledPage
	{
		public ResetMasterPassword()
		{
			Error += ResetMasterPassword_Error;
		}

		protected void ReportException(Exception exception)
		{
			var key = $"MyAccount_ResetMasterPassword_{(exception.GetType().FullName ?? "").Replace('.', '_')}";
			(new WebExceptionReporter()).ReportWebException(exception, key, exception.Message);
		}

		void ResetMasterPassword_Error(object sender, EventArgs e)
		{
			var error = Server.GetLastError();
			if (error != null)
			{
				ReportException(error);
			}
		}

		protected override string StyleSheetFileName => this.AppInstance.LoginPageStyleSheetPath;

		protected override bool PageRequiresLogin(Uri url) => false;

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var result = new EDIResetMasterPasswordManager(Email, Factory, ResetInfo.OrgCode);
			return result;
		}

		protected EDIResetMasterPasswordManager Manager => DataSource as EDIResetMasterPasswordManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			NewPassword.MaxLength = OrgContact.PasswordMaxLength;
			NewPasswordConfirm.MaxLength = OrgContact.PasswordMaxLength;

			base.OnLoad(e);

			AccountVerification.AccountVerificationCompleted += AccountVerification_AccountVerificationCompleted;

			if (IsPostBack)
			{
				return;
			}

			if (IsInLiteViewMode)
			{
				GoBackLoginLink.NavigateUrl = "../Login/LoginLite.aspx";
			}

			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
			BackLink.NavigateUrl = AppInstance.HostingSiteRoot;

			if (!IsQueryValid)
			{
				ShowInvalidTokenMessage();
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

			var repeaterItems = LoginContactsRepeater.Items.Cast<RepeaterItem>().ToArray();

			var repeaterRadioButtons = repeaterItems.Select(x => (ZRadioButton)x.FindControl("Checked")).ToArray();
			foreach (var radioButton in repeaterRadioButtons)
			{
				radioButton.CheckedChanged += CheckBox_OnCheckedChanged;
			}

			var defaultItem = repeaterItems.FirstOrDefault();
			var defaultRadioButton = (ZRadioButton)defaultItem?.FindControl("Checked");
			if (defaultRadioButton != null)
			{
				defaultRadioButton.Checked = true;
			}

			CheckBox_OnCheckedChanged(null, EventArgs.Empty);
			ResetMasterPasswordHeadingLabel.Visible = Manager.PersonsForBinding.Count > 1;
			PasswordExpiredMessageLabel.Visible = string.Equals(Request.QueryString[PasswordRotationRoutingDescriptor.RefKey], PasswordRotationRoutingDescriptor.RefValue, StringComparison.OrdinalIgnoreCase);
		}

		bool IsQueryValid => !string.IsNullOrEmpty(QueryToken) && !string.IsNullOrEmpty(Email);

		protected override bool ShouldSetupSessionOnLoad => IsQueryValid;

		protected void Update_Click(object sender, EventArgs e)
		{
			var defaultContact = GetSelectedContact();

			if (defaultContact != null)
			{
				if (Guid.TryParse(ResetInfo.EmailTemplateCompanyPk, out var companyPk))
				{
					defaultContact.CompanyPKForEmailTemplate = companyPk;
				}

				var webUserAdminManager = new EDIWebUserAdminManager(defaultContact);

				var message = webUserAdminManager.SetMasterPassword(NewPassword.Text, NewPasswordConfirm.Text);
				PasswordChangeMessage.Text = message;

				if (message == webUserAdminManager.PasswordChangeSuccess)
				{
					Manager.LoginOptionsHelper.WriteLoginHashCookie(defaultContact.OrgCode, defaultContact.OC_Email);

					if (AccessControl.TryConsume(QueryToken, AccessTokenTypes.ResetMasterPassword, out accessToken))
					{
						HideSetPasswordContentAndShowMessage(message);
						Factory.Save();
						PasswordChangeMessage.CssClass = "SuccessMessage";
					}
				}
			}
			else
			{
				if (DataSource == null)
				{
					PasswordChangeMessage.Text = Res.GetString("c89cf7a2-69df-47ca-85f9-37fa1c517542", "The page has expired. Please refresh and try again.");
				}
				else
				{
					var selectedPerson = GetSelectedPerson();
					if (selectedPerson != null)
					{
						var exceptionMessage = new ZStringBuilder();
						var contacts = selectedPerson.ContactCollection;
						exceptionMessage.AppendLine($"selectedPersonPk:{selectedPerson.PK}");
						if (contacts == null)
						{
							exceptionMessage.AppendLine("selectedPerson.ContactCollection==null");
						}
						else
						{
							exceptionMessage.AppendLine($"selectedPerson.ContactCollection:{contacts.Count}");
							foreach (OrgContact contact in contacts)
							{
								exceptionMessage.AppendLine($"    OC_PK:{contact.PK}|OC_IsActive:{contact.OC_IsActive}|OC_WebAccessEnabled:{contact.OC_WebAccessEnabled}");
							}
						}
						var exceptionKey = "ResetMasterPassword.aspx.cs|Update_Click|defaultContact==null";
						ErrorReporter.ReportOnce(exceptionKey, exceptionMessage.ToString());
					}

					PasswordChangeMessage.Text = Res.GetString("ef8e16a9-c9dd-4dde-aae3-8b2ad6d61172", "Please select a contact for login");
				}

				PasswordChangeMessage.Visible = true;
			}
		}

		protected OrgContact GetSelectedContact()
		{
			return GetSelectedPerson()?.ContactCollection?.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && x.OC_WebAccessEnabled);
		}

		protected virtual GlbPerson GetSelectedPerson()
		{
			var checkedItem = LoginContactsRepeater.Items.Cast<RepeaterItem>().FirstOrDefault(item =>
				(item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem) &&
				((ZRadioButton)item.FindControl("Checked")).Checked);

			if (checkedItem == null)
			{
				return null;
			}

			var label = (ZTextLabel)checkedItem.FindControl("RelatedAccounts");
			var result = Manager.PersonsForBinding.FirstOrDefault(x => x.RelatedAccounts.Equals(label.Text))?.Person;
			if (result == null)
			{
				var exceptionKey = "ResetMasterPassword.aspx.cs|GetSelectedPerson|result==null";
				var exceptionMessage = new ZStringBuilder();
				exceptionMessage.AppendLine($"label.Text:{label.Text}");
				foreach (var person in Manager.PersonsForBinding)
				{
					exceptionMessage.AppendLine($"    PER_PK:{person.Person.PK}|RelatedAccounts:{person.RelatedAccounts}");
				}
				ErrorReporter.ReportOnce(exceptionKey, exceptionMessage.ToString());
			}

			return result;
		}

		void ShowInvalidTokenMessage()
		{
			HideSetPasswordContentAndShowMessage(InvalidTokenMessage);
		}

		void HideSetPasswordContentAndShowMessage(string message)
		{
			HeadingMessageDiv.Visible = false;
			ContactsBox.Visible = false;
			NewPassword.Visible = false;
			NewPasswordConfirm.Visible = false;
			Update.Visible = false;
			passwordChangeRequirements.Visible = false;

			PasswordChangeMessage.Text = message;
		}

		#region Properties

		PasswordResetInfo ResetInfo
		{
			get
			{
				if (resetInfo == null)
				{
					ITokenizedAccessControl accessControl = new TokenizedAccessControl();
					peekSuccessful = !string.IsNullOrWhiteSpace(QueryToken) && accessControl.TryPeek(QueryToken, AccessTokenTypes.ResetMasterPassword, out accessToken);
					if (!peekSuccessful || string.IsNullOrEmpty(accessToken.Scope))
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
							resetInfo = new PasswordResetInfo { ContactEmail = accessToken.Scope };
						}
					}
				}
				return resetInfo;
			}
		}

		PasswordResetInfo resetInfo;

		string Email => ResetInfo.ContactEmail;

		ITokenizedAccessControl AccessControl { get; } = new TokenizedAccessControl();

		string QueryToken
		{
			get
			{
				if (string.IsNullOrEmpty(queryToken) || !peekSuccessful)
				{
					queryToken = Request.QueryString[AppInstance.ResetPasswordKey];
				}

				return queryToken;
			}
		}

		string queryToken;

		AccessTokenInfo accessToken;
		bool peekSuccessful;

		string InvalidTokenMessage => Res.GetString("cc019681-2110-40e5-916e-847cb03c130a", "The set link you have followed is invalid or expired.");

		#endregion Properties

		protected void CheckBox_OnCheckedChanged(object sender, EventArgs e)
		{
			AccountVerification.HeaderVisible = false;
			AccountVerification.ShouldRedirectAfterVerification = false;
			PasswordDiv.Visible = true;
			LoginOptionsDiv.Visible = false;

			var selectedPerson = GetSelectedPerson();
			if (selectedPerson != null)
			{
				foreach (var contact in selectedPerson.ContactCollection.Cast<EDIOrgContact>().Where(x => x.OC_Email.EqualsIgnoringCase(Email)))
				{
					var unlinkedUserAccountCandidate = contact.GetMostRecentUnlinkedUserAccount();
					if (unlinkedUserAccountCandidate != null)
					{
						if (LoginOptionsExemptionRoutingDescriptor.CanBypassLoginOptions(unlinkedUserAccountCandidate))
						{
							continue;
						}

						ShowLoginOptions(contact, unlinkedUserAccountCandidate);
						break;
					}

					var linkedUserAccountsQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
					var linkedUserAccounts = contact.Factory.Load<EdiCustomerUserAccount>(linkedUserAccountsQuery);
					var sdaUserAccount = linkedUserAccounts.FirstOrDefault(x => x.EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.SelfDeactivation);

					if (linkedUserAccounts.All(x => !x.EUA_IsActive || !x.EUA_IsContactRelationshipActive) && sdaUserAccount != null)
					{
						ShowLoginOptions(contact, sdaUserAccount);
						break;
					}
				}
			}

			void ShowLoginOptions(EDIOrgContact contact, EdiCustomerUserAccount unlinkedUserAccountCandidate)
			{
				PasswordDiv.Visible = false;
				LoginOptionsDiv.Visible = true;
				Manager.LoginOptionsHelper.SetContactAndUserAccountValues(contact, unlinkedUserAccountCandidate);
				AccountVerification.Refresh();
			}
		}

		void AccountVerification_AccountVerificationCompleted(object sender, EventArgs e)
		{
			PasswordDiv.Visible = true;
			LoginOptionsDiv.Visible = false;
		}
	}
}
