using System;
using System.Net;
using System.Text;
using CargoWise.Common;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class LoginComplete : RoutingEnabledPage
	{
		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			bool isRedirected = false;

			if (SiteUser != null)
			{
				var originalUrl = LoginRouter.GetOriginalUrlFromRequest(Request);
				var originalUrlString = originalUrl != null ? originalUrl.IsAbsoluteUri ? originalUrl.AbsoluteUri : originalUrl.OriginalString : string.Empty;

				if (SiteUser.IsLoggedIn && IsAuthenticated)
				{
					if (IdentityManager.IsValidID())
					{
						IdentityManager.ConsumeToken();
					}

					if (!string.IsNullOrEmpty(originalUrlString))
					{
						Response.Redirect(originalUrlString);
						isRedirected = true;
					}
				}
				else
				{
					if (IdentityManager.IsValidID())
					{
						var contact = IdentityManager.Contact;
						var router = new MyAccountLoginRouter(originalUrl, Token);
						IdentityManager.ConsumeToken();
						var webTransientPassword = ZArchitecture.Environment.User.WebTransientPassword;

						if (!router.HasAnyRoutingRequired && contact.IsValidForWebLogin)
						{
							SiteUser.Login(contact.OrgCode, contact.OC_Email, webTransientPassword);

							if (SiteUser.IsLoggedIn)
							{
								AutoLoginHelper.SetAuthCookie(this, SiteUser);
								if (Env.CurrentUser == null)
								{
									AppInstance.SetupSession(sender, e, false);
								}

								if (!AutoLoginHelper.CheckPreviousLoginStatus(Request.QueryString))
								{
									var eventLogHelper = new EventLogHelper();
									eventLogHelper.CreateLogForUserLoggedIn(SiteUser);
								}

								if (!string.IsNullOrEmpty(originalUrlString))
								{
									Response.Redirect(originalUrlString);
									isRedirected = true;
								}
							}
							else if (!EDIOrgHeader.HasCurrentSupport(contact.OC_OH.ToGuid()))
							{
								var secureQueryString = new SecureQueryString
								{
									{ "message",  Login.SupportExpiredMesage }
								};
								Response.Redirect($"~/Login/LoginLite.aspx?data={WebUtility.UrlEncode(secureQueryString.ToString())}");
								isRedirected = true;
							}
							else if (!SiteUser.IsLockedOut)
							{
								var reportStringBuilder = new StringBuilder();
								reportStringBuilder.AppendLine($"Org: {contact.OrgCode}");
								reportStringBuilder.AppendLine($"Name: {contact.OC_ContactName}");
								reportStringBuilder.AppendLine($"Email: {contact.OC_Email}");
								reportStringBuilder.AppendLine($"Jump To: {LoginRouter.GetOriginalUrlFromRequest(Request)}");
								ReportErrorCore("Login failed in LoginComplete", reportStringBuilder.ToString());
							}
						}
					}
				}
			}

			if (!isRedirected)
			{
				Response.Redirect(EDIDataRegistry.Instance.MyAccountIndexPage.Value);
			}
		}

		protected virtual void ReportErrorCore(string reportKey, string reportMessage)
		{
			(new WebExceptionReporter()).ReportWebException(null, reportKey, reportMessage);
		}

		protected virtual bool IsAuthenticated => Request.IsAuthenticated;
	}
}
