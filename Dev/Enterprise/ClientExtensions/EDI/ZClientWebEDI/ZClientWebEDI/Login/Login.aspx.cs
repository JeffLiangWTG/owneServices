using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Login : BasePage
	{
		protected override bool ShowLoginStatus => false;

		public Login()
		{
			LoginHelper = GetNewLoginHelper();
			Load += Login_Load;
		}

		protected virtual void Page_PreInit(object sender, EventArgs e)
		{
			if (OIDCLoginHelper.IsOIDCReady())
			{
				var uri = "~/Login/LoginV2.aspx";
				if (Request.QueryString.Count > 0)
				{
					uri += $"?{Request.QueryString}";
				}

				Response.Redirect(uri);
				return;
			}
		}

		protected virtual void Page_Load(object sender, EventArgs e)
		{
			if (Request.QueryString["data"] != null)
			{
				try
				{
					Message.Text = WebUtility.HtmlDecode(PageSecureQueryString["message"]);
				}
				catch (InvalidQueryStringException)
				{
				}
				catch (ExpiredQueryStringException)
				{
				}
			}
		}

		protected virtual void Login_Load(object sender, EventArgs e)
		{
			LoginHelper.OnPageLoad();

			if (!LoginMan.CompanyCode.IsEmpty)
			{
				ShowCompanyCodeLabelDiv.Style["display"] = "none";
				CompanyCodeDiv.Style["display"] = "block";
			}
			else
			{
				ShowCompanyCodeLabelDiv.Attributes.Add("onClick", "showCompanyCode();");
			}
		}

		protected MyAccountLoginHelper LoginHelper;

		protected virtual MyAccountLoginHelper GetNewLoginHelper()
		{
			return new MyAccountLoginHelper(this, false);
		}

		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			var loginMan = new LoginManager { IsCompanyCodeRequired = false };
			PresetUserNameAndCompany(loginMan);

			if (CompanyCodeTextBox != null && LoginNameTextBox != null && PasswordTextBox != null && RememberMeCheckBox != null
				&& (!CompanyCodeTextBox.Text.IsNullOrEmpty() || !LoginNameTextBox.Text.IsNullOrEmpty() || !PasswordTextBox.Text.IsNullOrEmpty() || RememberMeCheckBox.Checked))
			{
				using (loginMan.GetValidationSuspender())
				{
					loginMan.CompanyCode = CompanyCodeTextBox.Text;
					loginMan.UserName = LoginNameTextBox.Text;
					loginMan.Password = PasswordTextBox.Text;
					loginMan.RememberMe = RememberMeCheckBox.Checked;
				}
			}

			return loginMan;
		}

		protected void PresetUserNameAndCompany(LoginManager loginMan)
		{
			var sessionRefKey = HttpContext.Current.Request.QueryString[MyAccountLoginHelper.RefKey];
			if (!string.IsNullOrEmpty(sessionRefKey))
			{
				var session = HttpContext.Current.Session;
				if (session != null && session[sessionRefKey] is LoginManager sessionLoginMan)
				{
					loginMan.CompanyCode = sessionLoginMan.CompanyCode;
					loginMan.UserName = sessionLoginMan.UserName;
				}
			}
		}

		protected LoginManager LoginMan => DataSource as LoginManager;

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		#endregion

		#region Loading

		protected override void OnInitComplete(EventArgs e)
		{
			base.OnInitComplete(e);
			LoginHelper.RedirectIfLoggedIn();
		}

		#endregion

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}

		void InitializeComponent()
		{
			CompanyCodeTextBox.BindTo = "CompanyCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((LoginManager)(null)).CompanyCodeInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((LoginManager)(null)).CompanyCode)));
			LoginNameTextBox.BindTo = "UserName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((LoginManager)(null)).UserNameInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((LoginManager)(null)).UserName)));
			PasswordTextBox.BindTo = "Password";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((LoginManager)(null)).PasswordInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((LoginManager)(null)).Password)));
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Web.GUI.LoginManager";
			CopyrightYear.Text = ZDateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		}
		#endregion

		#region Helpers

		protected virtual void SigninBtn_Click(object sender, EventArgs e)
		{
			LoginHelper.IsRedirectAfterSignIn = true;

			if (!LoginMan.HasErrors)
			{
				if (LoginMan.CompanyCode.IsEmpty)
				{
					var (result, candidates) = LoginHelper.GetContactsFromCredentials();
					switch (result)
					{
						case LoginContactsResult.Success:
							{
								IEnumerable<OrgContact> contacts = [];
								if (OIDCLoginHelper.IsOIDCReady())
								{
									contacts = candidates.Where(x => !OIDCLoginHelper.IsMatchOrganisation(Factory, x.Header.OH_Code));
									if (contacts.IsNullOrEmpty())
									{
										Message.Text = GeneralLoginFailedMessage;
										return;
									}
								}
								else
								{
									contacts = candidates;
								}

								LoginHelper.LoginContactCandidates.AddRange(contacts);

								if (LoginHelper.LoginContactCandidates.Count > 1)
								{
									LoginHelper.RedirectToChooseCompany(new Uri(AppInstance.ChooseCompanyPage), AppInstance);
									return;
								}
								break;
							}
						case LoginContactsResult.TooManyContacts:
							{
								Message.Text = GeneralLoginFailedMessage;
								SendAmbiguousLoginContactsEmail(candidates);
								return;
							}
					}
				}

				LoginHelper.SignInViaRouting();
			}

			var org = new BusinessObjectFactory().LoadFromNaturalKey<EDIOrgHeader>(OrgHeaderSchema.OH_Code, LoginMan.CompanyCode);
			if (org != null && !org.HasCurrentSupportContractOrNoActiveLicence)
			{
				Message.Text = SupportExpiredMesage;
			}
			else if (SiteUser?.IsLockedOut ?? false)
			{
				Message.Text = AccountLockedMessage;
			}
			else
			{
				Message.Text = CredentialsNotMatchMessage;
			}
		}

		internal const string SupportExpiredMesage = @"We have encountered a problem with your account. Please email licensemanagement@wisetechglobal.com for further information.";
		const string AccountLockedMessage = "Account Locked";
		const string CredentialsNotMatchMessage = "Credentials do not match";
		const string GeneralLoginFailedMessage = "Login Failed";

		void SendAmbiguousLoginContactsEmail(OrgContact[] candidates)
		{
			var companyName = new ZString(Env.Registry.MailboxDisplayName);

			var template = EDIDataRegistry.Instance.AmbiguousContactLoginNotificationMessageTemplate.Value;
			var factory = candidates[0].Factory;
			var wrapper = new MyAccountAmbiguousLogin(candidates);
			var parser = new DocumentParser<MyAccountAmbiguousLogin, DocMyAccountAmbiguousLogin>(factory);

			var email = new HtmlEmailDef
			{
				FromDisplayName = companyName,
				Subject = parser.Parse(wrapper, template.EmailSubject),
				ContentType = EmailContentTypes.HTML
			};

			email.LoadHtmlUsingTemplate(parser.Parse(wrapper, template.EmailBody));

			if (companyName.IsValid && !companyName.IsEmpty)
			{
				email.FromDisplayName = companyName;
			}

			email.AddRecipientForUserCommunication(LoginMan.UserName);

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Login.aspx: Failed to send multiple contact notification email", e.Message, e);
			}
		}

		#endregion
	}
}
