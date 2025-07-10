using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.CustomerService.Business;
#if NETFRAMEWORK
using Enterprise.CustomerService.MyAccountLogin;
#endif
using Enterprise.MasterFiles.Business;
#if NETFRAMEWORK
using Enterprise.Registry.Business;
#endif
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.CustomerService.GUI.Res;

namespace Enterprise.UserPortal
{
	public class UserPortalLauncher
	{
		public void LaunchUserPortal()
		{
			LaunchUserPortal(MyAccountPageName, null);
		}

		public void LaunchUserPortal(string pageName, string url)
		{
			GoToCargoWiseUrl(pageName, url);
		}

		public void LaunchWiseTechAcademy(string path, string target)
		{
			if (path != null)
			{
				if (target != null)
				{
					GoToCargoWiseUrl((NoResString)"WiseTech Academy", FormattableString.Invariant($"~/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx?path={path}&target={target}"));
					return;
				}

				GoToCargoWiseUrl((NoResString)"WiseTech Academy", FormattableString.Invariant($"~/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx?path={path}"));
				return;
			}

			GoToCargoWiseUrl((NoResString)"WiseTech Academy", (NoResString)"~/WiseTechAcademy/WiseTechAcademyAutoLogin.aspx");
		}

		const string MyAccountPageName = "myaccount.cargowise.com";

		public void GoToNewIncident(string module, string subModule, string referenceId)
		{
			if (ShouldEnableTrustedMessaging)
			{
				var licenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				GoToPortalV3(eRequestNewLandingPageId, module: module, subModule: subModule, referenceId: referenceId, licenceCode: licenceCode);
			}
			else
			{
				var queryString = LandingPageToSecuredQueryString(eRequestNewLandingPageId);
				queryString[StaffContactValueObjectHelper.QueryStringKeys.Module] = module;
				queryString[StaffContactValueObjectHelper.QueryStringKeys.SubModule] = subModule;
				queryString[StaffContactValueObjectHelper.QueryStringKeys.ReferenceId] = referenceId;
				GoToPortalV2(queryString);
			}
		}

		public void GoToExistingIncident(string incidentNumber)
		{
			if (ShouldEnableTrustedMessaging)
			{
				GoToPortalV3(eRequestEditLandingPageId, incidentNumber: incidentNumber);
			}
			else
			{
				var queryString = LandingPageToSecuredQueryString(eRequestEditLandingPageId);
				queryString[StaffContactValueObjectHelper.QueryStringKeys.IncidentNumber] = incidentNumber;
				GoToPortalV2(queryString);
			}
		}

		public void GoToIncidentPortal()
		{
			if (ShouldEnableTrustedMessaging)
			{
				GoToPortalV3(eRequestPortalLandingPageId);
			}
			else
			{
				var queryString = LandingPageToSecuredQueryString(eRequestPortalLandingPageId);
				GoToPortalV2(queryString);
			}
		}

		SecureQueryString LandingPageToSecuredQueryString(string landingPageId)
		{
			var queryString = new StaffContactValueObjectHelper().CurrentStaffAndRegistrationToSecuredQueryString();
			queryString[StaffContactValueObjectHelper.QueryStringKeys.LandingPageId] = landingPageId;
			return queryString;
		}

		void GoToPortalV2(SecureQueryString queryString)
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			using (new ZWaitCursorChanger())
			{
				var cancelToken = cancelTokenSource.Token;

				var requestUri = PortalAuthServiceClient.AutoLoginUrl;
				string url = null;

				try
				{
					const int TimeoutMs = 60000;
					var client = new PortalAuthServiceClient();
					var result = client.GetUrl(requestUri, queryString, cancelToken, TimeoutMs);
					var statusCode = result.Item2;

					if (statusCode == HttpStatusCode.OK)
					{
						url = result.Item1.ToString();
					}
					else
					{
						ShowAutologinUnavailable(MyAccountPageName);
						url = myAccountDefaultUrl;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ShowAutologinUnavailable(MyAccountPageName, ex);
					url = myAccountDefaultUrl;
				}

				if (url != null)
				{
					SafeStartProcess(url);
				}
			}
		}

		#region TrustedMessaging

		async void GoToPortalV3(string landingPageId, string incidentNumber = null, string module = null, string subModule = null, string referenceId = null, string licenceCode = null)
		{
			using (new ZWaitCursorChanger())
			{
				string url = null;

				try
				{
					if (!await ObjectFactory.Get<ISystemUserAccountCollectionTermChecker>().CheckTermAcknowledged())
					{
						ShowAutologinUnavailable(MyAccountPageName);
						url = myAccountDefaultUrl;
					}
					else
					{
						var autoLoginTaskResult = await ObjectFactory.Get<IUserPortalClient>().ERequestPortalAutoLoginAsync(landingPageId, incidentNumber, module, subModule, referenceId, licenceCode);
						if (autoLoginTaskResult.Success)
						{
							url = autoLoginTaskResult.Response.AutoLoginUrl.ToString();
						}
						else
						{
							ShowAutologinUnavailable(MyAccountPageName);
							url = myAccountDefaultUrl;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ShowAutologinUnavailable(MyAccountPageName, ex);
					url = myAccountDefaultUrl;
				}

				if (url != null)
				{
					SafeStartProcess(url);
				}
			}
		}

		bool ShouldEnableTrustedMessaging =>
#if NETFRAMEWORK // In .NET Core and Winzor, we use TrustedMessaging
			WebDataRegistry.Instance.EnableTrustedMessaging.Value;
#else
			true;
#endif

		#endregion

		async protected virtual void GoToCargoWiseUrl(string pageName, string url)
		{
			using (new ZWaitCursorChanger())
			{
				try
				{
					if (ShouldEnableTrustedMessaging)
					{
						if (!await ObjectFactory.Get<ISystemUserAccountCollectionTermChecker>().CheckTermAcknowledged())
						{
							url = myAccountDefaultUrl;
							ShowAutologinUnavailable(pageName);
						}
						else
						{
							var autoLoginTaskResult = await ObjectFactory.Get<IUserPortalClient>().MyAccountAutoLoginAsync(string.IsNullOrWhiteSpace(url) ? null : new Uri(url, UriKind.RelativeOrAbsolute));
							if (autoLoginTaskResult.Success)
							{
								url = autoLoginTaskResult.Response.AutoLoginUrl.ToString();
							}
							else
							{
								url = myAccountDefaultUrl;
								ShowAutologinUnavailable(pageName);
							}
						}
					}
					else
					{
#if NETFRAMEWORK
						var loginService = GetAutoLoginService();
						string staffSecuredQueryString = new StaffContactValueObjectHelper().CurrentStaffAndRegistrationToSecuredQueryString().ToString();
						url = loginService.GetAutoLoginUrlWithReturnUrl(staffSecuredQueryString, url);
#endif
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (string.IsNullOrEmpty(url))
					{
						url = myAccountDefaultUrl;
					}
					ShowAutologinUnavailable(pageName, ex);
				}

				SafeStartProcess(url);
			}
		}

		void ShowAutologinUnavailable(string pageName, Exception ex = null)
		{
			string msg = Res.GetString("df0a9239-266a-4576-9ccd-ea6f7af5f509", "Automatic login is currently unavailable. Press OK to open your web browser on the {0} login page.", pageName);
#if DEBUG
			if (ex != null)
			{
				msg += "\r\n\r\n" + ex;
			}
#endif
			Globals.Message.ShowInformation(msg);
		}

		void SafeStartProcess(string url)
		{
			try
			{
				StartProcess(url);
			}
			catch (Win32Exception ex)
			{
				ErrorReporter.ReportOnce("HelpUrl", "Failed to open web address for: " + url, ex);
				Globals.Message.ShowInformation(Res.GetString("b5bd0cce-2284-492b-8ef9-0063e1744c40", "The web address '{0}' could not be opened. Please try copy and pasting it into your web browser instead.", url));
			}
		}

#if NETFRAMEWORK // In .NET Core and Winzor, we use TrustedMessaging instead
		protected virtual ILoginService GetNewAutoLoginService()
		{
			return new LoginService();
		}

		ILoginService GetAutoLoginService()
		{
			const int twentySeconds = 20000;

			var loginService = GetNewAutoLoginService();
			loginService.Url = string.Format("{0}/Login/LoginService.asmx", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/'));
			loginService.Timeout = twentySeconds;
			SetProxy(loginService);
			return loginService;
		}

		protected virtual void SetProxy(ILoginService loginService)
		{
			loginService.Proxy = WebRequest.DefaultWebProxy;
			if (loginService.Proxy != null && loginService.Proxy is WebProxy && !((WebProxy)loginService.Proxy).UseDefaultCredentials)
			{
				loginService.Proxy.Credentials = CredentialCache.DefaultCredentials;
			}
		}
#endif

		protected virtual void StartProcess(string url)
		{
			WebUrlLauncher.Launch(url);
		}

		const string myAccountDefaultUrl = "https://www.cargowise.com/my-account/index.shtml";

		public const string eRequestNewLandingPageId = "eRequest";
		public const string eRequestEditLandingPageId = "eRequestEdit";
		public const string eRequestPortalLandingPageId = "eRequestPortal";
		public const string AccreditationAttemptPortalLadingPageId = "AccreditationPortal";
	}
}
