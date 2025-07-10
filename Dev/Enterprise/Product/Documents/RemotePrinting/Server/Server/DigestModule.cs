using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = RemotePrinting.Server.Res;

namespace Enterprise.RemotePrinting.Server
{
	public class DigestModule : IHttpModule
	{
		public DigestModule()
		{
		}

		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			application.AuthenticateRequest += new EventHandler(this.OnAuthenticateRequest);
			application.EndRequest += new EventHandler(this.OnEndRequest);
			application.Error += OnError;
		}

#if DEBUG
		protected virtual void ThrowExceptionForTesting() { }
#endif

		public void OnAuthenticateRequest(object source, EventArgs eventArgs)
		{
			HttpApplication httpApp = (HttpApplication)source;

			try
			{
#if DEBUG
				ThrowExceptionForTesting();
#endif
				var authorizationString = httpApp.Request.Headers["Authorization"];
				var shouldDenyAccess = ShouldDenyAccess(authorizationString, httpApp.Request.HttpMethod, httpApp.Context, out var userName, out var roles);

				if (!shouldDenyAccess)
				{
					if (roles != null && roles.Length > 0)
					{
						httpApp.Context.User = new GenericPrincipal(new GenericIdentity(userName, "Enterprise.RemotePrinting.Server.Digest"), roles);
					}
				}
				else
				{
					DenyAccess(httpApp);
				}
			}
			catch (DatabaseUpgradedException ex)
			{
				var statusCode = ex.IsDatabaseVersionOlder ? 535 : 536;
				if (!Globals.IsTest)
				{
					WebUpgradeManager.NotifyUpgradeRequired();
				}
				DenyAccessWithError(httpApp, statusCode, ex.Message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool ShouldDenyAccess(string authorizationString, string httpMethod, HttpContext httpContext, out string userName, out string[] roles)
		{
			var shouldDenyAccess = false;
			userName = string.Empty;
			var userRoles = new List<string>();

			var requestInfo = Authentication.Instance.GetDigestAuthorizationRequestInfo(authorizationString);

			if (requestInfo.ContainsKey("username"))
			{
				shouldDenyAccess = true;

				userName = requestInfo["username"];

				string userPwdToUse = null;

#if DEBUG
				if (!string.IsNullOrEmpty(ApplicationUserForTest) && userName == ApplicationUserForTest)
				{
					userPwdToUse = ApplicationPwdForTest;
				}
				else
#endif

				if (userName == Authentication.Instance.ApplicationUser)
				{
					userPwdToUse = Authentication.Instance.ApplicationPwd;
				}
				else if (Authentication.Instance.AlternativeCredentials.ContainsCode(userName))
				{
					userPwdToUse = Authentication.Instance.AlternativeCredentials.GetDescriptionFromCode(userName);
				}
				else if (userName.StartsWith(Authentication.SupportUserPrefix, StringComparison.InvariantCultureIgnoreCase))
				{
					var supportToken = userName.Substring(Authentication.SupportUserPrefix.Length);
					var validateResult = CWSupportLoginToken.Validate(supportToken);
					if (validateResult.IsValid)
					{
						shouldDenyAccess = false;
						userRoles.Add(Authentication.UserRole);
						userRoles.Add(Authentication.SupportRole);
					}
				}

				if (!string.IsNullOrEmpty(userPwdToUse))
				{
					var hashedDigest = Authentication.Instance.GetDigestHash(requestInfo, httpMethod, userPwdToUse);

					var isNonceStale = !Authentication.Instance.IsValidNonce(requestInfo["nonce"]);
					if (httpContext != null)
					{
						httpContext.Items["staleNonce"] = isNonceStale;
					}

					if ((requestInfo["response"] == hashedDigest) && (!isNonceStale))
					{
						shouldDenyAccess = false;
						userRoles.Add(Authentication.UserRole);
					}
				}
			}

			roles = userRoles.ToArray();
			return shouldDenyAccess;
		}

#if DEBUG
		public string ApplicationUserForTest { get; set; }
		public string ApplicationPwdForTest { get; set; }
#endif

		/// <summary>
		/// WWW-Authenticate header is added here, 
		/// so if an authorization fails elsewhere than in this module, 
		/// we can still request authentication from the client.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="eventArgs"></param>
		public void OnEndRequest(object source, EventArgs eventArgs)
		{
			HttpApplication app = (HttpApplication)source;

			if (app.Response.StatusCode == 401)
			{
				object staleObj = app.Context.Items["staleNonce"];
				bool isNonceStale = staleObj != null && (bool)staleObj;
				string challengeString = Authentication.Instance.GetChallengeString(isNonceStale);

				app.Response.AppendHeader("WWW-Authenticate", challengeString);
				app.Response.StatusCode = 401;
			}
		}

		void DenyAccess(HttpApplication app)
		{
			DenyAccessWithError(app, 401, Res.GetString("12966fa3-7110-46c5-8ba3-35dc4acc209f", "Access Denied"));
		}

		void DenyAccessWithError(HttpApplication app, int statusCode, string description)
		{
			app.Response.StatusCode = statusCode;
			app.Response.StatusDescription = description;
			app.CompleteRequest();
		}

		void OnError(object sender, EventArgs e)
		{
			if (sender is HttpApplication httpApp)
			{
				var lastError = httpApp.Server?.GetLastError();
				if (lastError != null)
				{
					HandleApplicationException(lastError);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Name")]
		void HandleApplicationException(Exception ex)
		{
			if(Db.IsDatabaseUpgraded || WebUpgradeManager.IsUpgrateRunning())
			{
				//If database is upgraded, then do not report any exception
				return;
			}
			if (ex is HttpException httpException)
			{
				var handled = false;

				var httpCode = httpException.GetHttpCode();

				if (httpCode == (int)HttpStatusCode.BadRequest && ex.Message.StartsWith(DANGEROUS_REQUEST_PATH_MESSAGE))
				{
					// Might be external attempt to hack into system. If it's legitimate error in our client, it will be reported.
					handled = true;
				}
				else if (httpCode == (int)HttpStatusCode.NotFound && ex.Message.Contains("remoteprintingservice.asmx", StringComparison.OrdinalIgnoreCase))
				{
					// Missing remoteprintingservice.asmx, maybe due to Web application being upgraded
					handled = true;
				}
				else if (httpCode == (int)HttpStatusCode.InternalServerError && (ex.Message.Equals("Request timed out.") || ex.Message.Equals("The client disconnected.")))
				{
					// Connection with client may have been dropped, or database is not accessible or being upgraded.
					handled = true;
				}

				if (!handled)
				{
					string httpDescription = "";
					switch (httpCode)
					{
						case (int)HttpStatusCode.BadRequest: // 400
							httpDescription = "Bad Request";
							break;
						case (int)HttpStatusCode.NotFound: // 404
							httpDescription = "Not Found";
							break;
						default:
							break;
					}
					if (!string.IsNullOrEmpty(httpDescription))
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"{httpDescription} {httpCode} Error in WebPrint Http Application"), ex);
					}
					else
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"{httpCode} Error in WebPrint Http Application"), ex);
					}
				}
			}
			else if (ex is InvalidOperationException invalidOperationException &&
				invalidOperationException.InnerException is HttpUnhandledException httpUnhandledException &&
				httpUnhandledException.InnerException is XmlSchemaException schemaException &&
				(schemaException.Message.StartsWith("The global element", StringComparison.OrdinalIgnoreCase) || schemaException.Message.StartsWith("The global attribute", StringComparison.OrdinalIgnoreCase)) &&
				schemaException.Message.EndsWith("has already been declared.", StringComparison.OrdinalIgnoreCase) &&
				schemaException.StackTrace != null && schemaException.StackTrace.Contains("defaultwsdlhelpgenerator_aspx.Page_Load", StringComparison.OrdinalIgnoreCase))
			{
				//Just ignore this exception
			}
			else if (ex is SoapException && ex.Message.Equals("Server was unable to read request.", StringComparison.OrdinalIgnoreCase)
				&& ex.InnerException is XmlException xmlException &&
				(xmlException.Message.Equals("Root element is missing.", StringComparison.OrdinalIgnoreCase) || xmlException.Message.StartsWith("There is an unclosed literal string", StringComparison.OrdinalIgnoreCase)))
			{
				//There should be similar error on client side, and client will likely retry same operation and may succeed with it.
			}
			else if (ExceptionHandlingExtension.IsRelatedDBConnectionException(ex))
			{
				//Do not report the Exception which is related to db connection
				ExceptionHandlingExtension.ReportErrorToClient(538, ex.Message);
			}
			else if (ex is ArgumentNullException argNullException && argNullException.StackTrace != null &&
				argNullException.StackTrace.Contains("Microsoft.AspNet.SignalR.Transports"))
			{
				//Do nothing
			}
			else if (ex != null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Exception in WebPrint Http Application"), ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string DANGEROUS_REQUEST_PATH_MESSAGE = "A potentially dangerous Request.Path value";

#if DEBUG
		public void HandleApplicationException_ExposedForTest(Exception ex)
		{
			HandleApplicationException(ex);
		}
#endif
	}
}
