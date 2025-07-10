using System;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZClientWebCargoWiseEDI;
using Enterprise.ZClientWebCargoWiseEDI.Testing;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using static Enterprise.ZClientWebCargoWiseEDI.LoginV2;

namespace ZClientWebEDI.Test.Login
{
	[HttpContextEnabledTest]
	[TestedType(typeof(LoginV2))]
	public class LoginV2Test : TestCaseWithFactory
	{
		public void TestRedirectToV1Page()
		{
			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>());
			Assert(!OIDCLoginHelper.IsOIDCReady());
			var queryStringTextProperty = typeof(HttpRequest).GetProperty("QueryStringText", BindingFlags.NonPublic | BindingFlags.Instance);

			var dataString = "ABC .123.123.123.123 %%%% +++++";
			var originalQueryString = $"ReturnUrl={WebUtility.UrlEncode("Dummy.aspx")}&data={WebUtility.UrlEncode(dataString)}";
			AssertNotEquals(dataString, WebUtility.UrlEncode(dataString));

			queryStringTextProperty.SetValue(HttpContext.Current.Request, originalQueryString);
			using (var page = GetNewZPage() as LoginV2ForTest)
			{
				AssertEquals("The data should be decoded value", dataString, page.Request.QueryString["data"]);
				AssertEquals("The data should be decoded value", "Dummy.aspx", page.Request.QueryString["ReturnUrl"]);
				Assert("QueryString.ToString() should return the encoded value", originalQueryString.Equals(page.Request.QueryString.ToString(), StringComparison.OrdinalIgnoreCase));

				page.Page_PreInitTestOnly();
				Assert(HttpContext.Current.Response.RedirectLocation.EndsWith($"Login.aspx?{page.Request.QueryString}"));
			}
		}

		public void TestAuthorizeUrl()
		{
			Assert(OIDCLoginHelper.IsOIDCReady());
			using (var server = OIDCLoginHelperTest.SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{server.Port}",
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var targetSite = "https://DummySite.com/main?path=product&target=cargowise&type=update%20note&filename=更新文档";
				var queryStringTextProperty = typeof(HttpRequest).GetProperty("QueryStringText", BindingFlags.NonPublic | BindingFlags.Instance);
				queryStringTextProperty.SetValue(HttpContext.Current.Request, $"ReturnUrl={WebUtility.UrlEncode(targetSite)}");
				using (EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "C27126A1-AEF5-4D90-A2BA-CA54D3E35B9E"))
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				using (var page = GetNewZPage() as LoginV2ForTest)
				{
					page.DoPageLoad();
					page.LoginNameTextBoxForTest.Text = "user@123.com";
					Assert(OIDCLoginHelper.ShouldRedirectToIDP(Factory, "user@123.com"));

					AssertEquals(targetSite, page.Request.QueryString["ReturnUrl"]);
					page.SigninBtn_ClickForTest();
					Assert(page.Response.RedirectLocation.StartsWith(oidcConfig.AuthorityURL));
				}
			}
		}

		public void TestShouldNotRedirectToPasswordPage_WhenRedirectFailsOnTheServerSide()
		{
			Assert(OIDCLoginHelper.IsOIDCReady());
			using (var server = OIDCLoginHelperTest.SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = null,
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var targetSite = "https://DummySite.com/main?path=product&target=cargowise&type=update%20note&filename=更新文档";
				var queryStringTextProperty = typeof(HttpRequest).GetProperty("QueryStringText", BindingFlags.NonPublic | BindingFlags.Instance);
				queryStringTextProperty.SetValue(HttpContext.Current.Request, $"ReturnUrl={WebUtility.UrlEncode(targetSite)}");
				using (EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "C27126A1-AEF5-4D90-A2BA-CA54D3E35B9E"))
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				using (var page = GetNewZPage() as LoginV2ForTest)
				{
					page.DoPageLoad();
					page.LoginNameTextBoxForTest.Text = "user@123.com";
					Assert(OIDCLoginHelper.ShouldRedirectToIDP(Factory, "user@123.com"));
					AssertEquals("Precondition: PageStatus Should be UserNameAndCompanyCode", PageStatusList.UserNameAndCompanyCode, page.PageStatus);
					AssertEquals(targetSite, page.Request.QueryString["ReturnUrl"]);
					page.SigninBtn_ClickForTest();
					AssertEquals("when user inputs email and org code that should be redirected and redirect fails on the server side, user should stay on the OIDC Landing Page", OIDCLoginHelper.ErrorList.FailedToRedirectToIDP, page.MessageForTest.Text);
					AssertEquals(PageStatusList.UserNameAndCompanyCode, page.PageStatus);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestSigninBtn_ClickWhenAllContactsMatchOrg_ShouldLoginFailed()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TRAINS";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BUSES";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "CABS";

			const string commonEmail = "fly@guy.com";
			const string password = "ChangeMe123!";
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_OH = org1.PK;
			contact1.OC_Email = commonEmail;
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword(password);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_OH = org2.PK;
			contact2.OC_Email = commonEmail;
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword(password);

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_OH = org3.PK;
			contact3.OC_Email = commonEmail;
			contact3.OC_WebAccessEnabled = true;
			contact3.SetHashedPassword(password);
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid() });
			Factory.Save();

			Assert(OIDCLoginHelper.IsOIDCReady());
			using (var server = OIDCLoginHelperTest.SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{server.Port}",
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var targetSite = "https://DummySite.com/main?path=product&target=cargowise&type=update%20note&filename=更新文档";
				var queryStringTextProperty = typeof(HttpRequest).GetProperty("QueryStringText", BindingFlags.NonPublic | BindingFlags.Instance);
				queryStringTextProperty.SetValue(HttpContext.Current.Request, $"ReturnUrl={WebUtility.UrlEncode(targetSite)}");
				using (EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "C27126A1-AEF5-4D90-A2BA-CA54D3E35B9E"))
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				using (var page1 = GetNewZPage() as LoginV2ForTest)
				{
					page1.DoPageLoad();
					page1.LoginNameTextBoxForTest.Text = commonEmail;
					Assert(!OIDCLoginHelper.ShouldRedirectToIDP(Factory, commonEmail));

					page1.SigninBtn_ClickForTest();
					AssertEquals(false, page1.SiteUser.IsLoggedIn);
					page1.LoginManForTest.UserName = commonEmail;
					page1.LoginManForTest.Password = password;
					page1.LoginManForTest.RememberMe = true;
					page1.SigninBtn_ClickForTest();
					AssertEquals("No non-OIDC contact", "Login Failed", page1.MessageForTest.Text);
				}
			}
		}

		public void TestShouldNotShowPasswordPage_WhenShouldRedirectToIDPIsTrue()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "TRAINS";
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { org1.PK.ToGuid() });
			Factory.Save();

			Assert(OIDCLoginHelper.IsOIDCReady());
			using (var server = OIDCLoginHelperTest.SetupMockServer())
			{
				var oidcConfig = new OIDCConfig()
				{
					IsOIDCEnabled = false,
					OIDCServerType = OIDCServerTypes.Azure,
					AuthorityURL = $"https://{MockIdentityServerBase.HostName}:{server.Port}",
					ClientIdentifier = "testid"
				};
				oidcConfig.ClaimsMappings.Add(new OIDCClaimsMapping() { ClaimName = "unique_name", Identifier = "GlbStaff.GS_LoginName" });
				oidcConfig.Scopes.Add(new OIDCScope() { ScopeName = "testid" });
				oidcConfig.IsVerified = true;

				var targetSite = "https://DummySite.com/main?path=product&target=cargowise&type=update%20note&filename=更新文档";
				var queryStringTextProperty = typeof(HttpRequest).GetProperty("QueryStringText", BindingFlags.NonPublic | BindingFlags.Instance);
				queryStringTextProperty.SetValue(HttpContext.Current.Request, $"ReturnUrl={WebUtility.UrlEncode(targetSite)}");
				using (EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "C27126A1-AEF5-4D90-A2BA-CA54D3E35B9E"))
				using (SystemDataRegistry.Instance.OIDCConfig.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig))
				using (var page1 = GetNewZPage() as LoginV2ForTest)
				{
					page1.DoPageLoad();
					page1.LoginNameTextBoxForTest.Text = "user@cc.com";
					Assert(!OIDCLoginHelper.ShouldRedirectToIDP(Factory, "user@cc.com"));

					page1.SigninBtn_ClickForTest();
					AssertEquals(false, page1.SiteUser.IsLoggedIn);
					AssertEquals("Precondition: PageStatus Should be password", PageStatusList.Password, page1.PageStatus);
					page1.LoginManForTest.CompanyCode = "TRAINS";
					page1.SigninBtn_ClickForTest();
					AssertEquals("Users matching OIDC cannot login using passwords, should redirect to IDP", OIDCLoginHelper.ErrorList.UserNotRedirectedToIDP, page1.MessageForTest.Text);
					AssertEquals("Users matching OIDC cannot login using passwords, should redirect to IDP", PageStatusList.UserNameAndCompanyCode, page1.PageStatus);
					ErrorReporter.Clear();
				}
			}
		}

		protected ZPage GetNewZPage()
		{
			var page = new LoginV2ForTest();

			var setIntrinsicsMethod = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			setIntrinsicsMethod.Invoke(page, new object[] { HttpContext.Current });

			return page;
		}

		protected override void SetUp()
		{
			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "@123.com" });
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { Guid.NewGuid() });
			Assert(OIDCLoginHelper.IsOIDCReady());
			base.SetUp();
		}
	}

	class LoginV2ForTest : LoginV2
	{
		public void Page_PreInitTestOnly()
		{
			base.Page_PreInit(null, EventArgs.Empty);
		}

		public LoginManager LoginManForTest => base.LoginMan;

		public StateBag ViewStateForTest => ViewState;

		public void SigninBtn_ClickForTest() => base.SigninBtn_Click(null, EventArgs.Empty);

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForTest();
			result.OnCustomSessionStart();
			return result;
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}

		public void DoPageLoad()
		{
			try
			{
				Message = new ZTextLabel();
				CompanyCodeDiv = new HtmlGenericControl();
				ShowCompanyCodeLabelDiv = new HtmlGenericControl();

				CopyrightYear = new ZTextLabel();
				LoginForm = new HtmlForm();
				LoginNameTextLabel = new ZTextLabel();
				CompanyCodeTextLabel = new ZTextLabel();
				PasswordTextBox = new ZTextBox();
				ForgotPasswordSpan = new HyperLink();
				LoginNameTextBox = new ZTextBox();
				CompanyCodeTextBox = new ZTextBox();
				GoBackLoginLink = new HyperLink();

				base.OnLoad(EventArgs.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException)
				{
					throw;
				}
			}
		}

		public HtmlForm LoginFormForTest => LoginForm;

		public ZTextBox LoginNameTextBoxForTest => LoginNameTextBox;

		public ZTextBox PasswordTextBoxForTest => PasswordTextBox;

		public ZTextBox CompanyCodeTextBoxForTest => CompanyCodeTextBox;

		public ZTextLabel MessageForTest => Message;
	}
}
