using System;
#if NETFRAMEWORK
using System.Net;
#endif
using System.Threading.Tasks;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
#if NETFRAMEWORK
using Enterprise.CustomerService.MyAccountLogin;
#endif
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Environment;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.UserPortal.Testing
{
	internal class UserPortalLauncherTest : TestCaseWithFactory
	{
		public void TestLaunchUserPortal()
		{
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UserPortalLauncherForTest launcher = new UserPortalLauncherForTest();
#if NETFRAMEWORK
			launcher.ShouldReturnError = true;
#endif
			launcher.LaunchUserPortal();
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertStartsWith("LastMessage", "Automatic login is currently unavailable. Press OK to open your web browser on the myaccount.cargowise.com login page.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);

#if NETFRAMEWORK
			Assert("Proxy is not set", !launcher.IsProxySet);

			launcher.LastUrl = "";
			ErrorReporter.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			launcher.ShouldReturnError = false;
			launcher.LaunchUserPortal();
			AssertEquals("", ErrorReporter.LastMessageReported);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals("meh", launcher.LastUrl);
			Assert("Proxy is set", launcher.IsProxySet);

			launcher.ShouldReturnError = true;
#endif
			launcher.LaunchUserPortal("Wise Learning", "http://www.cargowise.com/eLearning.aspx");
			AssertEquals("", ErrorReporter.LastMessageReported);
			AssertStartsWith("LastMessage", "Automatic login is currently unavailable. Press OK to open your web browser on the Wise Learning login page.", UnitTestUserNotification.Instance.LastMessage.Text);

#if NETFRAMEWORK
			AssertEquals("http://www.cargowise.com/eLearning.aspx", launcher.LastUrl);
#else
			AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);
#endif
		}

		public void TestERequest()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var launcher = new UserPortalLauncherForTest();
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var checker = new SystemUserAccountCollectionTermCheckerForTest();

			using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
			{
				using (ObjectFactory.Substitute<IUserPortalClient>(new UserPortalClientForTest()))
				{
					AssertEquals(false, checker.TermAcknowledged);
					launcher.GoToIncidentPortal();
					AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);

					checker.TermAcknowledged = true;
					AssertEquals(true, checker.TermAcknowledged);
					launcher.GoToIncidentPortal();
					AssertEquals("http://www.cw1.com/eRequestPortal/////", launcher.LastUrl);
					launcher.GoToNewIncident("M01", "S02", "R03");
					AssertEquals("http://www.cw1.com/eRequest//M01/S02/R03/EDIEDIDAT", launcher.LastUrl);
					launcher.GoToExistingIncident("INC_005");
					AssertEquals("http://www.cw1.com/eRequestEdit/INC_005////", launcher.LastUrl);
					launcher.GoToNewIncident("M01", "S02", "R03");
					AssertEquals("http://www.cw1.com/eRequest//M01/S02/R03/EDIEDIDAT", launcher.LastUrl);
				}
			}
		}

		public void TestMyAccountAutoLogin()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var launcher = new UserPortalLauncherForTest();
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var checker = new SystemUserAccountCollectionTermCheckerForTest();

			using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
			{
				using (ObjectFactory.Substitute<IUserPortalClient>(new UserPortalClientForTest()))
				{
					AssertEquals(false, checker.TermAcknowledged);
					launcher.LaunchUserPortal("Page1", "http://www.cw1.com/page1");
					AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);

					checker.TermAcknowledged = true;
					AssertEquals(true, checker.TermAcknowledged);
					launcher.LaunchUserPortal("Page1", "http://www.cw1.com/page1");
					AssertEquals("http://www.cw1.com/MyAccountAutoLoginUrl.aspx?return=http%3a%2f%2fwww.cw1.com%2fpage1", launcher.LastUrl);
				}
			}
		}

		public void TestMyAccountAutoLogin_AutoLoginNotAvailable()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var launcher = new UserPortalLauncherForTest();
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var checker = new SystemUserAccountCollectionTermCheckerForTest();

			using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
			using (ObjectFactory.Substitute<IUserPortalClient>(new UserPortalClientForTest()))
			{
				AssertEquals(false, checker.TermAcknowledged);
				launcher.LaunchUserPortal("Page1", "http://www.cw1.com/page1");

				AssertStartsWith("LastMessage", "Automatic login is currently unavailable. Press OK to open your web browser on the Page1 login page.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);
			}
		}

		public void TestLaunchWiseTechAcademy()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var launcher = new UserPortalLauncherForTest();
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var checker = new SystemUserAccountCollectionTermCheckerForTest();

			using (ObjectFactory.Substitute<ISystemUserAccountCollectionTermChecker>(checker))
			{
				using (ObjectFactory.Substitute<IUserPortalClient>(new UserPortalClientForTest()))
				{
					AssertEquals(false, checker.TermAcknowledged);
					launcher.LaunchUserPortal("Page1", "http://www.cw1.com/page1");
					AssertEquals("https://www.cargowise.com/my-account/index.shtml", launcher.LastUrl);

					checker.TermAcknowledged = true;
					AssertEquals(true, checker.TermAcknowledged);
					launcher.LaunchWiseTechAcademy(null, null);
					AssertEquals("http://www.cw1.com/MyAccountAutoLoginUrl.aspx?return=~%2fWiseTechAcademy%2fWiseTechAcademyAutoLogin.aspx", launcher.LastUrl);
					launcher.LaunchWiseTechAcademy("product", "cargo");
					AssertEquals("http://www.cw1.com/MyAccountAutoLoginUrl.aspx?return=~%2fWiseTechAcademy%2fWiseTechAcademyAutoLogin.aspx%3fpath%3dproduct%26target%3dcargo", launcher.LastUrl);
				}
			}
		}

		class UserPortalLauncherForTest : UserPortalLauncher
		{
#if NETFRAMEWORK
			public bool ShouldReturnError
			{
				get;
				set;
			}

			protected override ILoginService GetNewAutoLoginService()
			{
				return new LoginServiceForTest(this);
			}

			protected override void SetProxy(ILoginService loginService)
			{
				IsProxySet = true;
			}
			public bool IsProxySet;
#endif
			protected override void StartProcess(string url)
			{
				LastUrl = url;
			}

			public string LastUrl;
		}

#if NETFRAMEWORK
		class LoginServiceForTest : ILoginService
		{
			public LoginServiceForTest(UserPortalLauncherForTest userPortalLauncher)
				: base()
			{
				this.userPortalLauncher = userPortalLauncher;
			}

			string _url;
			string ILoginService.Url
			{
				get => _url;
				set
				{
					if (userPortalLauncher.ShouldReturnError)
					{
						throw new Exception("meh");
					}
					else
					{
						_url = "meh";
					}
				}
			}

			public int Timeout { get; set; }

			//Proxy property is not used in the LoginServiceForTest as the UserPortalLauncher.SetProxy is overloaded by UserPortalLauncherForTest and is not calling base
			public IWebProxy Proxy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			readonly UserPortalLauncherForTest userPortalLauncher;

			public string GetAutoLoginUrlWithReturnUrl(string staffSecuredQueryString, string url)
			{
				return _url;
			}
		}
#endif

		class SystemUserAccountCollectionTermCheckerForTest : ISystemUserAccountCollectionTermChecker
		{
			public bool TermAcknowledged { get; set; }
			public Task<bool> CheckTermAcknowledged() => Task.FromResult(TermAcknowledged);
		}

		class UserPortalClientForTest : IUserPortalClient
		{
			public Task<TrustedResponse<bool>> AcknowledgeAgreementAsync(string userAgreementType, bool shouldSendCopy)
			{
				return Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });
			}

			public Task<TrustedResponse<bool>> SignAgreementAsync(IUserAgreementSignerDetails deets)
			{
				return Task.FromResult(new TrustedResponse<bool>() { Success = true, Response = true });
			}

			public Task<TrustedResponse<AutoLoginResponse>> ERequestPortalAutoLoginAsync(string landingPageId, string incidentNumber, string module, string subModule, string referenceId, string licenceCode)
				=> Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse(new Uri($"http://www.cw1.com/{landingPageId}/{incidentNumber}/{module}/{subModule}/{referenceId}/{licenceCode}")) });

			public Uri GetMyAccountAutoLoginUrl(Uri returnUrl)
			{
				return new Uri($"http://www.cw1.com/MyAccountAutoLoginUrl.aspx?return={HttpUtility.UrlEncode(returnUrl.ToString())}");
			}

			public Task<TrustedResponse<OAuthLoginResponse>> OAuthAutoLoginAsync(Uri returnUrl)
			{
				return Task.FromResult(new TrustedResponse<OAuthLoginResponse>()
				{
					Success = true,
					Response = new OAuthLoginResponse() { RedirectUrl = new Uri(returnUrl + "?token=123"), Token = "123" }
				});
			}

			public Task<TrustedResponse<UserAgreementResponseData>> GetUserAgreementAsync(string userAgreementType)
				=> Task.FromResult(new TrustedResponse<UserAgreementResponseData>()
				{
					Success = true,
					Response = new UserAgreementResponseData()
					{
						Required = false,
						Title = "",
						Content = "",
						Level = "",
						VersionNumber = 0,
					}
				});

			public Task<TrustedResponse<EnterpriseAgreementResponseData>> GetEnterpriseAgreementUrlAsync(string userAgreementType)
				=> Task.FromResult(new TrustedResponse<EnterpriseAgreementResponseData>()
				{
					Success = true,
					Response = new EnterpriseAgreementResponseData()
					{
						Required = false,
						Url = string.Empty,
					}
				});

			public Task<TrustedResponse<AutoLoginResponse>> MyAccountAutoLoginAsync(Uri returnUrl)
			{
				var rsp = GetMyAccountAutoLoginUrl(returnUrl);
				return Task.FromResult(new TrustedResponse<AutoLoginResponse>() { Success = true, Response = new AutoLoginResponse() { AutoLoginUrl = rsp } });
			}

			public Task<TrustedResponse<GetAcceptancesResponse>> GetAcceptancesAsync(string userAgreementType)
			{
				return Task.FromResult(new TrustedResponse<GetAcceptancesResponse>() { Success = true, Response = new GetAcceptancesResponse() });
			}
		}
	}
}
