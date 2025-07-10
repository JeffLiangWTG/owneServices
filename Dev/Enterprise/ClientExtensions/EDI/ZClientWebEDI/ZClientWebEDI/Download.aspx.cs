using System;
using System.IO;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Download : BasePage, IHttpHandler
	{
		public new bool IsReusable
		{
			get { return false; }
		}

		public new void ProcessRequest(HttpContext context)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var reportPk = context.Request.QueryString["report"];
				if (!string.IsNullOrEmpty(reportPk))
				{
					DownloadReport(context, reportPk);
					return;
				}

				SecureQueryString queryString = null;
				try
				{
					QueryStringData = context.Request.QueryString[SecureQueryString.QueryStringKey];
					queryString = new SecureQueryString(QueryStringData);
				}
				catch (QueryStringException) { }

				if (string.IsNullOrWhiteSpace(QueryStringData) && SiteUser != null && SiteUser.IsLoggedIn)
				{
					var file = WebUtility.UrlDecode(context.Request.QueryString[QueryStringKeyFile]);
					var language = context.Request.QueryString[QueryStringKeyLanguage];
					if (!string.IsNullOrWhiteSpace(file) && !string.IsNullOrWhiteSpace(language))
					{
						context.Response.Redirect(GetFileURL(file, language, context.Request.Url.Host));
					}
				}

				if (IsValidQueryString(queryString) && SiteUser != null)
				{
					var fileUrl = GetFileURL(queryString, context.Request.Url.Host);
					if (!SiteUser.IsLoggedIn)
					{
						if (!TryDoAutoLoginFromQueryString(fileUrl, context))
						{
							return;
						}
					}

					if (SiteUser.IsLoggedIn)
					{
						context.Response.Redirect(fileUrl);
					}
					else
					{
						var path = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value;
						var builder = new UriBuilder(path) { Port = -1 };
						context.Response.Redirect(builder.ToString());
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		protected string GetFileURL(SecureQueryString queryString, string requestUrlHost)
			=> GetFileURL(queryString[QueryStringKeyFile], queryString[QueryStringKeyLanguage], requestUrlHost);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		string GetFileURL(string originalFileUrl, string originalLanguage, string requestUrlHost)
		{
			var result = originalFileUrl;
			var language = (string.IsNullOrEmpty(originalLanguage) || originalLanguage.Contains("EN")) ? string.Empty : originalLanguage;
			if (originalFileUrl.Contains("Documents") && !string.IsNullOrEmpty(language))
			{
				var filePhysicalPath = Regex.Replace(originalFileUrl, @"(.*?)(/Documents/UpdateNotes/)(.*)(\..*)", $"{EDIDataRegistry.Instance.MyAccountPhysicalServerPath.Value}$2{language}/$3{language}$4").Replace('/', Path.DirectorySeparatorChar);
				if (File.Exists(filePhysicalPath))
				{
					var pattern = @"(.*UpdateNotes/)(.*)(\..*)";
					var replacement = $@"$1{$"{language}/"}$2{language}$3";
					result = Regex.Replace(originalFileUrl, pattern, replacement);
				}
			}

			if (!this.IsSafeUrl(result, requestUrlHost))
			{
				result = AppInstance.HostingSiteHomePage;
			}

			return result;
		}

		bool IsValidQueryString(SecureQueryString queryString)
		{
			return queryString != null
				&& !string.IsNullOrEmpty(queryString[StaffContactValueObjectHelper.QueryStringKeys.ContactData])
				&& !string.IsNullOrEmpty(queryString[StaffContactValueObjectHelper.QueryStringKeys.LicenceCode])
				&& !string.IsNullOrEmpty(queryString[QueryStringKeyFile]);
		}

		bool TryDoAutoLoginFromQueryString(string fileUrl, HttpContext context)
		{
			try
			{
				var helper = new StaffContactHelper(QueryStringData);
				if (helper.Database != null && helper.Database.LD_AllowAutoLogin)
				{
					var contactImportResult = helper.FindOrCreateContact();
					var (userAccount, contact) = (contactImportResult.UserAccount, contactImportResult.Contact);

					if (contact != null)
					{
						var redirectUrl = AutoLoginHelper.UserRoutingLogin(fileUrl, userAccount, AppInstance, contact);
						if (SiteUser is MyAccountWebUser myAccountSiteUser && myAccountSiteUser.IsLoggedIn)
						{
							return true;
						}

						context.Response.Redirect(redirectUrl.IsAbsoluteUri ? redirectUrl.AbsoluteUri : redirectUrl.OriginalString);
					}
				}
			}
			catch (QueryStringException) { }

			return true;
		}

		void DownloadReport(HttpContext context, string reportPk)
		{
			var response = context.Response;
			var request = context.Request;
			var user = SiteUser;

			if (user == null || !user.IsLoggedIn) //not logged in, redirect to login page with return url
			{
				var loginUrl = AppInstance.LoginPage + "?ReturnURL=" + WebUtility.UrlEncode(FormattableString.Invariant($"{request.RawUrl}&lg=1&{MyAccountUtility.LoginRedirectKey}=1"));
				response.Redirect(loginUrl);
				return;
			}
			else
			{
				//returned from login page, show blank page and redirect to download via 'Refresh'
				if (request.QueryString["lg"] != null && request.QueryString["dl"] == null)
				{
					response.AddHeader("Refresh", "1;URL=" + request.RawUrl + "&dl=1");
					return;
				}
			}

			ZGuid rptPk;
			var errorMessage = "";

			if (!ZGuid.TryParse(reportPk, out rptPk))
			{
				errorMessage = "Invalid PK.";
			}
			else if (user == null || !user.IsLoggedIn)
			{
				errorMessage = "Invalid User.";
			}
			else
			{
				var org = user.LoggedInOrganisation;
				var contact = user.LoggedInUser;
				var report = Factory.Load<EdiReportingQueue>(rptPk);

				if (report == null)
				{
					errorMessage = "Download Link Expired.";
				}
				else if (!FileExists(report.ERQ_ReportFileFullName, out errorMessage))
				{
				}
				else if (report.ERQ_OH != org.PK || (report.ERQ_OC != contact.PK && report.ERQ_GS_NKSupportStaff != user.SupportStaffCode))
				{
					errorMessage = "The report is not created by the current logged user, please clear the browser cookies and try again.";
				}
				else
				{
					var reportName = report.ERQ_ReportName + Path.GetExtension(report.ERQ_ReportFileFullName);
					response.ContentType = DataContentTypes.Zip;
					var contentDispositionType = nameof(ContentDispositionType.Attachment).ToUpperInvariant();
					response.AddHeader("Content-Disposition", FormattableString.Invariant($"{contentDispositionType}; filename=\"{reportName}\""));

					response.ClearContent();
					using (var file = new FileStream(report.ERQ_ReportFileFullName, FileMode.Open, FileAccess.Read))
					{
						file.CopyTo(response.OutputStream);
					}
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				response.Write(errorMessage);
			}

			response.End();
		}

		static bool FileExists(string fileFullName, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (File.Exists(fileFullName))
			{
				return true;
			}
			else
			{
				try
				{
					using (var file = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
					{
					}
				}
				catch (FileNotFoundException)
				{
					errorMessage = "Download Link Expired.";
				}
				catch (Exception ex) when (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException || ex is SecurityException || ex is IOException)
				{
					ErrorReporter.ReportOnce("ZClientWebCargoWiseEDI.DownloadReport", ex.Message, ex);
					errorMessage = "The required file is currently unavailable. Please try again later.";
				}
				return false;
			}
		}

		const string QueryStringKeyFile = "file";
		const string QueryStringKeyLanguage = "language";

		string QueryStringData { get; set; }
	}
}
