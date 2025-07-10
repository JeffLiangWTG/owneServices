using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.WebInfrastructure;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.GlobalBase;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Moq;
using NUnit.Framework;
using WTG.AppDomainWrappers.Net;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	/// <summary>
	/// Global test class for all non-descending tests that need only run once
	/// </summary>
	[HttpContextEnabledTest]
	public sealed class TestGlobal : TransactionedTestCase
	{
		public void TestGetSiteUser()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			AssertNull("Pre-condition", global.SiteUser);

			global.OnCustomSessionStartForTesting(global, EventArgs.Empty);
			AssertEquals(typeof(OrgContactWebUser), global.SiteUser.GetType());
		}

		public void TestOnCustomSessionStart()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			AssertNull("Pre-condition", HttpContext.Current.Session["SiteUser"]);

			global.OnCustomSessionStartForTesting(this, EventArgs.Empty);
			AssertEquals(typeof(OrgContactWebUser), HttpContext.Current.Session["SiteUser"].GetType());
		}

		public void TestGetNewSiteUser()
		{
			var global = new ZGlobalForTesting();
			AssertEquals(typeof(OrgContactWebUser), global.GetNewSiteUser().GetType());
		}

		//Region name / test locations ouaght to change
		#region newTests

		public void TestApplicationCookie()
		{
			var global = new ZGlobalForTesting();
			var cookieName = global.ApplicationCookieNameForTesting;
			var appCookie = global.ApplicationCookie;

			AssertEquals("CW1Application", cookieName);
			AssertEquals("CW1Application", appCookie.CookieName);
		}

		public void TestGlobalConfig()
		{
			var global = new ZGlobalForTesting();
			var config = global.GlobalConfig;

			AssertType<ZGlobalConfig>(config);
		}

		public void TestSetupSiteUser()
		{
			using (var global = new ZGlobalForTesting())
			{
				const string key = "SiteUser";
				global.SetupSiteUser();
				AssertEquals(1, HttpContext.Current.Session.Count);
				AssertEquals(key, HttpContext.Current.Session.Keys.Get(0));
				Assert(HttpContext.Current.Session.IsNewSession);
				AssertType<OrgContactWebUser>(HttpContext.Current.Session[key]);
			}
		}

		public void TestTryToReloadPage()
		{
			var global = new ZGlobalForTesting();
			AssertNotNull("precondition", HttpContext.Current);
			AssertNotNull("precondition", HttpContext.Current.Response);
			Assert(global.TryToReloadPage());
		}

		#endregion

		public void TestFileNotFoundError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.AddError(new FileNotFoundException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Page not found", qs["title"]);
			AssertContains("The page you requested was not found.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestSQLExceptionError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var sqlException = SqlExceptionBuilder.CreateSqlException(1, "testSqlException");

			HttpContext.Current.AddError(sqlException);
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Web Application Error", qs["title"]);
			AssertContains("The Web Application you attempted to access is currently unavailable", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestHttpExceptionRequestTimedOutError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new HttpException("Request timed out"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Request Timed Out", qs["title"]);
			AssertContains("Your request has timed out.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestHttpExceptionWithSearchIndexerRequestTimedOutError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.Session.Add(SearchControl.SearchControlIsSearchingIndexer, true);

			HttpContext.Current.AddError(new HttpException("Request timed out"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Request Timed Out", qs["title"]);
			AssertContains("Your request has timed out. Please try to add more filtering to make your search more specific.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestShowExceptionWhenUserVisible()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new UserVisibleException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);

			string url = HttpContext.Current.Response.RedirectLocation;

			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);

			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Error", qs["title"]);
			AssertEquals("An error has occurred and we couldn't process your request, please try again or contact us if the problem persists.<br><br>Exception of type 'Enterprise.ZArchitecture.Web.GUI.Testing.UserVisibleException' was thrown.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestShowErrorWhenDBTimeoutExpire()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new InvalidOperationException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);

			string url = HttpContext.Current.Response.RedirectLocation;

			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);

			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("DB connection error", qs["title"]);
			AssertEquals("All pooled database connections are in use, please try again later or contact us if the problem persists.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestSendEmailToWebAdminWhenWebFormExceedsLimit()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			HttpContext.Current.AddError(new InvalidOperationException("Operation is not valid due to the current state of the object."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			AssertEquals("Email should contain error message", global.EmailsNotSentBecauseInTestMode[0].TextBody, "The web form has exceeded its limit of 1000 items to handle. This is due to a limitation of older versions of the .NET Framework. Please upgrade the web server to the latest .NET Framework version.");
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
			ErrorReporter.Clear();
		}

		public void TestShowOutOfMemoryError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			var factory = new BusinessObjectFactory();
			var admin = factory.NewWithValidTestData<GlbStaff>();
			admin.GS_EmailAddress = "admin@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			HttpContext.Current.AddError(new OutOfMemoryException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Out of Memory Error", qs["title"]);
			AssertContains("An error has occurred because this website is running low on memory.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestShowErrorToUserWhenNeatUploadThrowsNullPointerException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.AddError(new NullReferenceException() { Source = "Brettle.Web.NeatUpload" });

			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("File Upload Error", qs["title"]);
			AssertContains("An error occurred while processing this request. Please try again or contact your Administrator if the error persists.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestApplicationError_WhenRegistryJsonDeserializationFailed_RedirectsToErrorPage()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new RegistryJsonException("Error Message", "RegistryName", "RegistryCaption"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Invalid JSON Registry!", qs["title"]);
			AssertEquals("The value for registry item (RegistryName - RegistryCaption) contains invalid JSON. Please contact your system administrator.", qs["message"]);
			AssertExceptionWasHandled();
		}

		public void TestApplicationError_WhenRegistryJsonDeserializationFailedOnRenderingErrorPage_SendsEmail_RenderSeriousErrorPage()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			global.RequestUrlOverride = new Uri("http://www.wtg.zone.com/Tracking/Error.aspx?data=SomeDummySecureData", UriKind.Absolute);
			HttpContext.Current.AddError(new RegistryJsonException("Error Message", "RegistryName", "RegistryCaption"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertNull(HttpContext.Current.Response.RedirectLocation);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<string> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
			AssertContains("Email should contain error message", "", global.EmailsNotSentBecauseInTestMode[0].HtmlBody);
		}

		public void TestShowErrorToUserWhenSpecialDirectory()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new HttpException("The file '/App_Themes/Standard/admin.aspx' is in the special directory 'App_Themes', which is not allowed."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Page not found", qs["title"]);
			AssertContains("The page you requested was not found.", qs["message"]);
			AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
			AssertExceptionWasHandled();
		}

		public void TestApplicationErrorReportsHttpException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new HttpException("TestHttpExceptionReportedFromWeb"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			var result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport WHERE QER_SystemCreateUser = 'ZZ' ORDER BY QER_SystemCreateTimeUtc DESC");
			Assert(result.Contains("HttpException"));
			Assert(result.Contains("TestHttpExceptionReportedFromWeb"));
		}

		public void TestViewStateExceptionErrorPage()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.AddError(new ViewStateException());

			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Logged Off", qs["title"]);
			AssertContains("You logged off in another window.", qs["message"]);
			AssertExceptionWasHandled();
		}

		void AssertExceptionWasHandled()
		{
			CombineAssertions(() =>
			{
				AssertNotEquals((int)HttpStatusCode.InternalServerError, HttpContext.Current.Response.StatusCode);
				AssertNotEquals($"{HttpStatusCode.InternalServerError}", HttpContext.Current.Response.StatusDescription);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
			});
		}

		#region TestExceptionShouldBeReported

		public void TestExceptionShouldBeReported()
		{
			CombineAssertions(() =>
			{
				ExceptionReport(new System.Security.Cryptography.CryptographicException(), true);
				ExceptionReport(new InvalidOperationException("Collection was modified; enumeration operation may not execute."), true);
				ExceptionReport(new XmlSchemaException("The global attribute 'http://www.w3.org/XML/1998/namespace:lang' has already been declared."), true);
				ExceptionReport(new XmlSchemaException("The global attribute 'http://www.w3.org/XML/2001/namespace:lang' has already been declared."), true);
				ExceptionReport(new XmlSchemaException("The global element 'http://www.w3.org/XML/2001/namespace:schema' has already been declared."), true);
				ExceptionReport(new ExternalException(), true);
				ExceptionReport(new DirectoryNotFoundException("Could not find a part of the path 'SomeOtherPath'"), true);
				ExceptionReport(new FileLoadException("Could not load file or assembly 'CargoWise.ComponentModel' or one of its dependencies. The media is write protected."), true);
				ExceptionReport(new NullReferenceException("Object reference not set to an instance of an object."), true);
			});
		}

		public void TestExceptionShouldNotBeReported()
		{
			var global = new ZGlobalForTesting();
			CombineAssertions(() =>
			{
				ExceptionReport(new System.Security.Cryptography.CryptographicException("Padding is invalid and cannot be removed."));
				ExceptionReport(new Exception("get_aspx_ver.aspx"));
				ExceptionReport(new InvalidOperationException("No web service found at: /Tracking/WebService/WebServiceShared.asmx."));
				ExceptionReport(new InvalidOperationException("Request format is unrecognized for URL unexpectedly ending in '/Execute'."));
				ExceptionReport(new InvalidOperationException("ExecuteReader requires an open and available Connection. The connection's current state is closed."));
				ExceptionReport(new HttpException("A potentially dangerous Request.Path value was detected from the client (<)."));
				ExceptionReport(new HttpRequestValidationException("A potentially dangerous Request.QueryString value was detected from the client (t='... </SCRIPT><script src...')."));
				ExceptionReport(new HttpRequestValidationException("A potentially dangerous Request.Form value was detected from the client (ctl02$ctl05$AvailabilityDate$ctl00$TextBox='<a href='http://sexf...')."));
				ExceptionReport(new PathTooLongException("The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters."));
				ExceptionReport(new HttpException("Client disconnected after receiving 142112 of 2010758 bytes in 6 secs -- user probably cancelled upload."));
				ExceptionReport(new FormatException("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or a non-white space character among the padding characters."));
				ExceptionReport(new ArgumentException("Invalid postback or callback argument. Event validation is enabled using <pages enableEventValidation=\"true\"/> in configuration or <%@ Page EnableEventValidation=\"true\" %> in a page."));
				ExceptionReport(new HttpException("The length of the URL for this request exceeds the configured maxUrlLength value."));
				ExceptionReport(new HttpException("Session state has created a session id, but cannot save it because the response was already flushed by the application."));
				ExceptionReport(new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x80070057.", unchecked((int)0x80070057)));
				ExceptionReport(new SqlLockLostException("A db reconnect was attempted while undisposed SqlLocks existed", new List<string>()));
				ExceptionReport(new Exception("Tried to read a line. No data received."));
				ExceptionReport(new Win32Exception(1326, "Test Error"));
				ExceptionReport(new Win32Exception(258, "Test Error"));
				ExceptionReport(new HttpException("Invalid viewstate."));
				ExceptionReport(new HttpException("Failed to load viewstate"));
				ExceptionReport(new Win32Exception(53, "The network path was not found."));
				ExceptionReport(new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x80072746.", unchecked((int)0x80072746)));
				ExceptionReport(new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800703E3.", unchecked((int)0x800703E3)));
				ExceptionReport(new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800703E3.", new COMException("Error", unchecked((int)0x800703E3))));
				ExceptionReport(new HttpException(500, "An error occurred while communicating with the remote host. The error code is 0x800704CD.", unchecked((int)0x800704CD)));
				ExceptionReport(new HttpException(500, "The remote host closed the connection. The error code is 0x800703E3.", unchecked((int)0x800703E3)));
				ExceptionReport(new HttpException(500, "The remote host closed the connection. The error code is 0x80070016.", unchecked((int)0x80070016)));
				ExceptionReport(new HttpException(500, "The remote host closed the connection. The error code is 0x80070006.", unchecked((int)0x80070006)));
				ExceptionReport(new Win32Exception(64, "The specified network name is no longer available."));
				ExceptionReport(new InvalidOperationExceptionOnIISStartupForTest("Collection was modified; enumeration operation may not execute."));
				ExceptionReport(new XmlSchemaExceptionOnIISStartupForTest("The global attribute 'http://www.w3.org/XML/1998/namespace:lang' has already been declared."));
				ExceptionReport(new XmlSchemaExceptionOnIISStartupForTest("The global attribute 'http://www.w3.org/XML/2001/namespace:lang' has already been declared."));
				ExceptionReport(new XmlSchemaExceptionOnIISStartupForTest("The global element 'http://www.w3.org/XML/2001/namespace:schema' has already been declared."));
				ExceptionReport(new ExternalException("Timed out waiting for a program to execute. The command being executed was \"C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\csc.exe\\\"")); // An exception message we don't want to send
				ExceptionReport(new DirectoryNotFoundException("Could not find a part of the path 'C:\\ProgramData\\WiseTech Global\\CargoWiseOneWeb\\20.5.8.68\\Forwarding'")); // Exception message we dont want to send
				ExceptionReport(new ArgumentException("Unknown web method blah blah. Parameter name: methodName"));
				ExceptionReport(new BadImageFormatException("Bad IL range."));
				ExceptionReport(new HttpException("Exception of type 'System.Web.HttpException' was thrown."), requestUrlString: "http://www.wtg.zone.com/Tracking/WebService/Trace.axd");
				ExceptionReport(new HttpException("Exception of type 'System.Web.HttpException' was thrown."), requestUrlString: "http://www.wtg.zone.com/Tracking/admin/Trace.axd");
				ExceptionReport(new HttpException("Exception of type 'System.Web.HttpException' was thrown."), requestUrlString: "http://www.wtg.zone.com/Trace.axd");
				ExceptionReport(new HttpException("Exception of type 'System.Web.HttpException' was thrown."), requestUrlString: "http://www.wtg.zone.com/webservice/downloads/Trace.axd");
				ExceptionReport(new SystemWebCachingNullReferenceExceptionForTest("Object reference not set to an instance of an object."));
				ExceptionReport(new NullReferenceExceptionOnIISStartupForTest("Object reference not set to an instance of an object."));
				ExceptionReport(new ArgumentException("Font '?' cannot be found."));
				ExceptionReport(new ArgumentException("Illegal characters in path."));
				ExceptionReport(new ArgumentException("Illegal characters in path."), shouldBeReported: true, uriReferer: "http://www.wtg.zone.com/Tracking/Login.aspx");
				ExceptionReport(new Win32Exception(19, "The media is write protected"));
			});
		}

		public void TestHandleException()
		{
			AssertHandleException(new HttpException(204, "upload cancelled by user"));
			AssertHandleException(new HttpException(404, "Seems like something happened"));
			AssertHandleException(new HttpException(405, "Request format is unrecognized."));
			AssertHandleException(new InvalidProgramException());

			using (SetWarmupUserAgent())
			{
				AssertHandleException(new FileLoadException("The given assembly name or codebase, some-path-blah-blah.dll, was invalid."));
				AssertHandleException(new HttpException("Unable to validate data."));
				AssertHandleException(new BadImageFormatException("Could not load file or assembly 'PtxSdkCommon.wm' or one of its dependencies. The module was expected to contain an assembly manifest."));
				AssertHandleException(new BadImageFormatException("Could not load file or assembly 'dbghelp' or one of its dependencies. The module was expected to contain an assembly manifest."));
				AssertHandleException(new FileLoadException("Could not load file or assembly 'CargoWise.ComponentModel' or one of its dependencies. The media is write protected."));
				AssertHandleException(new HttpCompileException());
			}

			void AssertHandleException(Exception exception)
			{
				var global = new ZGlobalForTesting();
				InitZGlobalWithHttpContext(global);
				HttpContext.Current.AddError(exception);

				global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

				AssertExceptionWasHandled();
			}
		}

		IDisposable SetWarmupUserAgent()
		{
			var userAgentKey = "HTTP_USER_AGENT";
			var cachedUserAgent = HttpContext.Current.Request.ServerVariables[userAgentKey];

			HttpContext.Current.Request.SetServerVariableValue(userAgentKey, "IIS Application Initialization Warmup");

			return new DisposableAction(() => HttpContext.Current.Request.SetServerVariableValue(userAgentKey, cachedUserAgent));
		}

		static void ExceptionReport(Exception exception, bool shouldBeReported = false, string requestUrlString = null, string uriReferer = null)
		{
			var global = new ZGlobalForTesting();
			if (requestUrlString != null)
			{
				global.RequestUrlOverride = new Uri(requestUrlString, UriKind.Absolute);
			}
			if (uriReferer != null)
			{
				var refererField = HttpContext.Current.Request.GetType().GetField("_referrer", BindingFlags.NonPublic | BindingFlags.Instance);
				refererField.SetValue(HttpContext.Current.Request, new Uri(uriReferer));
			}
			AssertEquals(exception.GetType().Name, shouldBeReported, global.ExceptionShouldBeHandledForTesting(exception));
		}

		public void TestExceptionShouldBeReported_ReloadInvalidViewstate()
		{
			AssertReloadPage(new HttpException("Invalid viewstate."));
		}

		public void TestExceptionShouldBeReported_ReloadFailedViewstate()
		{
			AssertReloadPage(new HttpException("Failed to load viewstate"));
		}

		public void TestExceptionShouldBeReported_ReloadInvalidLength()
		{
			AssertReloadPage(new FormatException("Invalid length for a Base-64 char array"));
		}

		public void TestExceptionShouldBeReported_ReloadInvalidChar()
		{
			AssertReloadPage(new FormatException("Invalid character in a Base-64 string"));
		}

		public void TestExceptionShouldBeReported_ReloadNotBase64()
		{
			AssertReloadPage(new FormatException("The input is not a valid Base-64 string"));
		}

		void AssertReloadPage(Exception ex)
		{
			var global = new ZGlobalForTesting();
			AssertReloadPage(() => global.ExceptionShouldBeHandledForTesting(ex));
		}

		public static void AssertReloadPage(Action action)
		{
			using (var memoryStream = new MemoryStream())
			using (var reader = new StreamReader(memoryStream))
			{
				var responseFilter = new TestResponseFilter(HttpContext.Current.Response.Filter, memoryStream);
				HttpContext.Current.Response.Filter = responseFilter;

				action();

				memoryStream.Seek(0, SeekOrigin.Begin);
				AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
				var responseOutput = reader.ReadToEnd();
				var expectedOutput = "<script language=\"javascript\">\nalert(\"An error occurred during information submission. Please try again.\");\njavascript:history.go(-1);\n</script>\n";

				AssertEquals(expectedOutput, responseOutput);
			}
		}

		#endregion

		#region TestDBVersionCheck

		public void TestDBVersionCheck()
		{
			ZGlobal global = new ZGlobalForTesting();

			AssertEquals("DBVersions should initially match", true, global.DBVersionMatches);
			int originalVersion = Env.Registry.DatabaseMajorSchemaVersion;
			int newVersion = originalVersion + 1;
			ZString updateSQLFormatString = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DATABASE_SCHEMA_VERSION'";

			Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, newVersion));
			AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);

			Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, originalVersion));
			AssertEquals("DBVersions should match. The value should not be cached.", true, global.DBVersionMatches);

			originalVersion = Env.Registry.DatabaseSystemDataVersionMajor;
			newVersion = originalVersion + 1;
			updateSQLFormatString = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DatabaseSystemDataVersionMajor'";

			Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, newVersion));
			AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);

			Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, originalVersion));
			AssertEquals("DBVersions should match. The value should not be cached.", true, global.DBVersionMatches);
		}

		public void TestDbVersionAndStructureChangeOnKeyGlbTables()
		{
			Exception caughtException = null;

			int originalVersion = Env.Registry.DatabaseMajorSchemaVersion;
			int newVersion = originalVersion + 1;
			ZString updateSchemaVersion = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DATABASE_SCHEMA_VERSION'";

			Db.Connection.ExecuteNonQuery(ZString.Format(updateSchemaVersion, newVersion));
			Db.Connection.ExecuteNonQuery("DROP FUNCTION IF EXISTS [hrm].[HRMStaffHistory_WithManaged]");
			Db.Connection.ExecuteNonQuery("DROP FUNCTION IF EXISTS [hrm].[HRMRemHistory_WithManaged]");
			Db.Connection.ExecuteNonQuery("DROP FUNCTION IF EXISTS GlbStaff_WithManaged");
			Db.Connection.ExecuteNonQuery("DROP FUNCTION GetStaffWithSecurityContext");
			Db.Connection.ExecuteNonQuery("DROP INDEX NR_RX__GS_SystemLastEditTimeUtc ON GlbStaff");
			Db.Connection.ExecuteNonQuery("exec sp_rename 'GlbStaff.GS_SystemLastEditTimeUtc','GS_SystemLastEditTimeUtcTesting'");

			try
			{
				using (var global = new ZGlobalForTesting())
				{
					global.Session_Start_ForTesting(this, EventArgs.Empty);
				}
			}
			catch (Exception ex)
			{
				caughtException = ex;
			}

			AssertNull("No exceptions should be caught, should be redirected", caughtException);
		}

		#endregion

		#region TestDBVersionMismatchEmailsAdminOnceOnly

		[TestDate(2020, 03, 27, 7, 25, 00)]
		public void TestDBVersionMismatchEmailsAdminOnceOnly()
		{
			using (ZGlobal global = new ZGlobalForTesting())
			{
				try
				{
					var factory = new BusinessObjectFactory();
					var admin1 = factory.NewWithValidTestData<GlbStaff>();
					admin1.GS_EmailAddress = "homer.simpson@cargowise.com";
					var admin2 = factory.NewWithValidTestData<GlbStaff>();
					admin2.GS_EmailAddress = "peter.griffin@cargowise.com";
					var group = factory.NewWithValidTestData<GlbGroup>();
					group.Staff.Add(admin1);
					group.Staff.Add(admin2);
					WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
					factory.Save();

					AssertEquals("DBVersions should initially match", true, global.DBVersionMatches);
					int originalVersion = Env.Registry.DatabaseMajorSchemaVersion;
					int newVersion = originalVersion + 1;
					ZString updateSQLFormatString = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DATABASE_SCHEMA_VERSION'";

					global.EmailsNotSentBecauseInTestMode.Clear();
					AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

					Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, newVersion));
					AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);
					global.RedirectToDBVersionPage();

					AssertEquals("RedirectLocation should be error page", true, HttpContext.Current.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
					AssertEquals("First minute is not reported", 0, global.EmailsNotSentBecauseInTestMode.Count);

					TestDateAttribute.AddSeconds(30);
					ReinitialiseRequest();
					global.RedirectToDBVersionPage();
					AssertEquals("First minute is not reported", 0, global.EmailsNotSentBecauseInTestMode.Count);

					TestDateAttribute.AddMinutes(2);
					ReinitialiseRequest();
					global.RedirectToDBVersionPage();
					AssertEquals("An email should have been sent to the DBAdmin", 1, global.EmailsNotSentBecauseInTestMode.Count);

					var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
					AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
					IEnumerable<String> addressesList = actualRecipients.Mailboxes.Select(x => x.Address);
					Assert("Expected recipient homer.simpson@cargowise.com", addressesList.Contains(@"homer.simpson@cargowise.com"));
					Assert("Expected recipient peter.griffin@cargowise.com", addressesList.Contains(@"peter.griffin@cargowise.com"));

					ReinitialiseRequest();

					AssertEquals("Database version should not match", false, global.DBVersionMatches);
					global.RedirectToDBVersionPage();

					AssertEquals("RedirectLocation should be error page", true, HttpContext.Current.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
					AssertEquals("An email should have been sent to the DBAdmin", 1, global.EmailsNotSentBecauseInTestMode.Count);

					Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, originalVersion));
					AssertEquals("Database version should now match", true, global.DBVersionMatches);

					ReinitialiseRequest();
					global.EmailsNotSentBecauseInTestMode.Clear();
					AssertEquals("Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

					Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, newVersion));
					AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);
					global.RedirectToDBVersionPage();
					AssertEquals("First minute is not reported", 0, global.EmailsNotSentBecauseInTestMode.Count);

					TestDateAttribute.AddMinutes(2);
					ReinitialiseRequest();
					global.RedirectToDBVersionPage();

					AssertEquals("RedirectLocation should be error page", true, HttpContext.Current.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
					AssertEquals("Another email should have been sent to the DBAdmin", 1, global.EmailsNotSentBecauseInTestMode.Count);
					actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
					AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
					addressesList = actualRecipients.Mailboxes.Select(x => x.Address);
					Assert("Expected recipient homer.simpson@cargowise.com", addressesList.Contains(@"homer.simpson@cargowise.com"));
					Assert("Expected recipient peter.griffin@cargowise.com", addressesList.Contains(@"peter.griffin@cargowise.com"));
					Assert("Override message not contained", !global.EmailsNotSentBecauseInTestMode[0].TextBody.Contains("This message was redirected to"));
				}
				finally
				{
					global.EmailsNotSentBecauseInTestMode.Clear();
				}
			}
		}

		public void TestEmailsAreSentToCorrectRecipients()
		{
			using (ZGlobal global = new ZGlobalForTesting())
			{
				var originalHostedLocation = EnvProxy.HostedLocation;
				EnvProxy.SetHostedLocationForTest("SYD");
				EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

				var emailSubject = "Email Subject";
				var emailBody = "Email Body";

				global.SendEmail(emailSubject, emailBody);

				AssertEquals("Number of emails", 1, global.EmailsNotSentBecauseInTestMode.Count);

				var mail = global.EmailsNotSentBecauseInTestMode[0];

				AssertEquals("Email subject", mail.Subject, emailSubject);
				AssertEquals("Email body", mail.TextBody, emailBody);

				var groupEmails = new EmailGroupUtility().GetHostedNotificationsEmailOverride();

				AssertEquals("Number of recipients", 1, mail.To.Count);

				var emailAddress = mail.To.Mailboxes.ElementAt(0).Address;

				AssertEquals("Email is sent to", emailAddress, groupEmails[0]);
				AssertEquals("Email is sent Hosting Notifications", "Hosting.Notifications@wisetechglobal.com", emailAddress);

				EnvProxy.SetHostedLocationForTest(originalHostedLocation);
				global.EmailsNotSentBecauseInTestMode.Clear();

				Guid allGuid = (Guid)Db.Connection.ExecuteScalar("SELECT GG_PK from dbo.GlbGroup where GG_CODE = 'ALL'");
				Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = LOWER(LTRIM(RTRIM((GS_Code)))) + '@b.com', GS_IsActive = 1, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E'");
				Guid groupGuid = WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.Value;
				AssertEquals(allGuid, groupGuid);

				global.SendEmail(emailSubject, emailBody);
				AssertEquals("Number of emails", 1, global.EmailsNotSentBecauseInTestMode.Count);
				mail = global.EmailsNotSentBecauseInTestMode[0];
				AssertEquals("Number of recipients", 3, mail.To.Count);

				var emailAddresses = new List<string>();
				emailAddresses.Add(mail.To.Mailboxes.ElementAt(0).Address);
				emailAddresses.Add(mail.To.Mailboxes.ElementAt(1).Address);
				emailAddresses.Add(mail.To.Mailboxes.ElementAt(2).Address);

				Assert("Recipient's email address", emailAddresses.Contains("c@b.com"));
				Assert("Recipient's email address", emailAddresses.Contains("pm@b.com"));
				Assert("Recipient's email address", emailAddresses.Contains("x@b.com"));
			}
		}
		void ReinitialiseRequest()
		{
			foreach (TestSetupAttribute testAttribute in TestSetupAttributes)
			{
				if (testAttribute is HttpContextEnabledTestAttribute)
				{
					testAttribute.SetUp(this);
				}
			}
		}

		#endregion

		#region TestDBVersionMismatchEmailsShouldNotBeSentInHostedSystem

		public void TestDbVersionMismatchEmailsShouldNotBeSentInHostedSystem()
		{
			ZGlobal global = new ZGlobalForTesting();
			EnvProxy.SetHostedLocationForTest("SYD");
			var originalIsWiseTechGlobalDatabase = DataUtils.IsWiseTechGlobalDatabaseServerForTest;
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;

			try
			{
				AssertEquals("DBVersions should initially match", true, global.DBVersionMatches);
				int originalVersion = Env.Registry.DatabaseMajorSchemaVersion;
				int newVersion = originalVersion + 1;
				ZString updateSqlFormatString = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DATABASE_SCHEMA_VERSION'";

				global.EmailsNotSentBecauseInTestMode.Clear();
				AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

				Db.Connection.ExecuteNonQuery(ZString.Format(updateSqlFormatString, newVersion));
				AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);
				global.RedirectToDBVersionPage();

				AssertEquals("RedirectLocation should be error page", true, HttpContext.Current.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
				AssertEquals("An email should not be sent to the group if is hosted system", 0, global.EmailsNotSentBecauseInTestMode.Count);
			}
			finally
			{
				global.EmailsNotSentBecauseInTestMode.Clear();
				EnvProxy.SetHostedLocationForTest(null);
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = originalIsWiseTechGlobalDatabase;
			}
		}

		#endregion

		#region TestDBVersionMismatchWithEmailDestinationOverride

		[TestDate(2020, 03, 27, 7, 25, 00)]
		public void TestDBVersionMismatchWithEmailDestinationOverride()
		{
			ZGlobal global = new ZGlobalForTesting();

			try
			{
				var factory = new BusinessObjectFactory();
				var admin1 = factory.NewWithValidTestData<GlbStaff>();
				admin1.GS_EmailAddress = "homer.simpson@cargowise.com";
				var admin2 = factory.NewWithValidTestData<GlbStaff>();
				admin2.GS_EmailAddress = "peter.griffin@cargowise.com";
				var group = factory.NewWithValidTestData<GlbGroup>();
				group.Staff.Add(admin1);
				group.Staff.Add(admin2);
				WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
				Env.Registry.EmailDestinationOverride = "override@cargowise.com";
				factory.Save();

				AssertEquals("DBVersions should initially match", true, global.DBVersionMatches);
				int originalVersion = Env.Registry.DatabaseMajorSchemaVersion;
				int newVersion = originalVersion + 1;
				ZString updateSQLFormatString = "UPDATE dbo.StmData set SD_BinaryValue = CAST(CAST('{0}' as nvarchar(60)) as varbinary(8000)) where SD_Name = 'DATABASE_SCHEMA_VERSION'";

				global.EmailsNotSentBecauseInTestMode.Clear();
				AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

				Db.Connection.ExecuteNonQuery(ZString.Format(updateSQLFormatString, newVersion));
				AssertEquals("DBVersion should not match. The value should not be cached.", false, global.DBVersionMatches);

				global.RedirectToDBVersionPage();
				ReinitialiseRequest();
				TestDateAttribute.AddMinutes(2);
				global.RedirectToDBVersionPage();

				AssertEquals("RedirectLocation should be error page", true, HttpContext.Current.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
				AssertEquals("An email should have been sent to the DBAdmin", 1, global.EmailsNotSentBecauseInTestMode.Count);
				var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
				AssertEquals("Should be 1 recipients", 1, actualRecipients.Count);
				AssertEquals("Expected recipient", @"override@cargowise.com", actualRecipients.Mailboxes.ElementAt(0).Address);
				//the order changes randomly, so we accept either ordering
				Assert("Override message contained",
					global.EmailsNotSentBecauseInTestMode[0].TextBody.Contains(System.Environment.NewLine + "(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to homer.simpson@cargowise.com, peter.griffin@cargowise.com.)")
					||
					global.EmailsNotSentBecauseInTestMode[0].TextBody.Contains(System.Environment.NewLine + "(This message was redirected to override@cargowise.com as it was sent from a non-production system. Originally the email was addressed to peter.griffin@cargowise.com, homer.simpson@cargowise.com.)")
					);
			}
			finally
			{
				global.EmailsNotSentBecauseInTestMode.Clear();
			}
		}

		#endregion

		#region TestSendEmailToWebAdminMaxHttpCollectionKey

		public void TestSendEmailToWebAdminMaxHttpCollectionKey()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "homer.simpson@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "peter.griffin@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			HttpContext.Current.AddError(new InvalidOperationException("Operation is not valid due to the current state of the object."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			AssertEquals("An email should have been sent to the DBAdmin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> addressesList = actualRecipients.Mailboxes.Select(x => x.Address);
			Assert("Expected recipient homer.simpson@cargowise.com", addressesList.Contains(@"homer.simpson@cargowise.com"));
			Assert("Expected recipient peter.griffin@cargowise.com", addressesList.Contains(@"peter.griffin@cargowise.com"));
			AssertExceptionWasHandled();
		}

		#endregion

		#region TestSendEmailToWebAdminWhenProxyError

		public void TestShowErrorToUserWhenProxyError()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			HttpContext.Current.AddError(new InvalidOperationException("The request is not supported. (Exception from HRESULT: 0x80070032)"));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);

			string url = HttpContext.Current.Response.RedirectLocation;

			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);

			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Proxy Error, HRESULT: 0x80070032", qs["title"]);
			AssertEquals("The application has encountered a proxy error, HRESULT: 0x80070032. Please contact your administrator.", qs["message"]);
			AssertExceptionWasHandled();
		}

		#endregion

		#region TestSendEmailToWebAdminAndReporForUnauthorizedAccessException

		public void TestSendEmailToWebAdminAndReporForUnauthorizedAccessException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			var exceptionMessage = @"Access to the path (wwwroot) denied.";
			HttpContext.Current.AddError(new UnauthorizedAccessException(exceptionMessage));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
			AssertContains("Email should contain error message", exceptionMessage, global.EmailsNotSentBecauseInTestMode[0].HtmlBody);

			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			string url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Permission error on system", qs["title"]);
			AssertEquals("The application has encountered permission error. Please contact your administrator.", qs["message"]);
		}

		#endregion

		#region TestSendEmailToWebAdminForEnpointNotFoundException

		public void TestSendEmailToWebAdminForEnpointNotFoundException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			string exceptionMessage = "The service 'someservice.svc' does not exist.";
			HttpContext.Current.AddError(new System.ServiceModel.EndpointNotFoundException(exceptionMessage));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			Assert("Email should contain error message", global.EmailsNotSentBecauseInTestMode[0].HtmlBody.IndexOf(exceptionMessage) != -1);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
		}

		#endregion

		#region TestReportError

		public void TestReportErrorUrlEncodesQueryString()
		{
			var global = new ZGlobalForTesting();
			global.ReportError("Configuration Error", "The site is currently experiencing a misconfiguration. We regret any inconvenience caused. Please try again later.");
			string redirectURL = HttpContext.Current.Response.RedirectLocation;
			Assert("RedirectURL", redirectURL.StartsWith("/Error.aspx?data="));

			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(redirectURL.Substring(17)));
			AssertEquals("title", "Configuration Error", qs["title"]);
			AssertEquals("message", "The site is currently experiencing a misconfiguration. We regret any inconvenience caused. Please try again later.", qs["message"]);
		}

		public void TestDBUpgradeErrorEncodesQueryString()
		{
			var global = new ZGlobalForTesting();
			global.ReportError("Under Maintenance", "This site is currently under maintenance. We regret any inconvenience caused. Please try again later.");
			string redirectURL = HttpContext.Current.Response.RedirectLocation;
			Assert("RedirectURL", redirectURL.StartsWith("/Error.aspx?data="));

			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(redirectURL.Substring(17)));
			AssertEquals("title", "Under Maintenance", qs["title"]);
			AssertEquals("message", "This site is currently under maintenance. We regret any inconvenience caused. Please try again later.", qs["message"]);
		}

		public void TestReportErrorForSeriousErrorDoesNotRedirect()
		{
			ZGlobal global = new ZGlobalForSeriousErrorTesting();
			global.ReportError("Serious Error", "Unknown serious error has occured. We regret any inconvenience caused. Please try again later.");
			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertNull(HttpContext.Current.Response.RedirectLocation);
		}

		public void TestReportErrorDoesNotRedirectIfClientCancelled()
		{
			var dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
			var dummyWorkerRequest = dummyApplication.WorkerRequest;
			dummyWorkerRequest.IsClientConnectedOverride = false;

			var global = new ZGlobalForTesting();
			global.ReportError("Under Maintenance", "This site is currently under maintenance. We regret any inconvenience caused. Please try again later.");

			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertNull(HttpContext.Current.Response.RedirectLocation);
		}

		#endregion

		#region TestServerTimeout

		public void TestServerTimeout()
		{
			/* Something about Server.ScriptTimeout:
			 *
			 * 1. The timeout applies only if the debug attribute in the compilation element in web.config is False
			 * Source: http://msdn.microsoft.com/en-us/library/e1f13641.aspx
			 * Section: executionTimeout (analog of Server.ScriptTimeout in web.config)
			 *
			 * 2. Custom Server.ScriptTimeout value can't be less then default value for IIS:
			 * Source: http://msdn.microsoft.com/en-us/library/ms524831.aspx
			 */

			using (WebDataRegistry.Instance.RequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1234))
			{
				var global = new ZGlobalForTesting();
				InitZGlobalWithHttpContext(global);

				AssertNotEquals("Precondition: default server timeout differs from registry setting", WebDataRegistry.Instance.RequestTimeout.Value, HttpContext.Current.Server.ScriptTimeout);

				global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
				AssertEquals("Server timeout equals to registry setting", WebDataRegistry.Instance.RequestTimeout.Value, HttpContext.Current.Server.ScriptTimeout);
			}
		}

		public void TestServerTimeout_Warmup()
		{
			using (WebDataRegistry.Instance.RequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1234))
			using (SetWarmupUserAgent())
			{
				var global = new ZGlobalForTesting();
				InitZGlobalWithHttpContext(global);

				global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);

				AssertEquals("Server timeout is extended to 1 hour for IIS warmup", 3600, HttpContext.Current.Server.ScriptTimeout);
			}
		}

		#endregion

		#region TestWebConfig

		public void TestBeginRequestWhenWebConfigIsNotOk()
		{
			var globalMock = new Mock<ZGlobalForTesting>() { CallBase = true };
			globalMock.Setup(x => x.ConfigurationOK).Returns(false);
			globalMock.Setup(x => x.ErrorPage).Returns("Error.aspx");
			var global = globalMock.Object;

			try
			{
				var factory = new BusinessObjectFactory();
				var admin1 = factory.NewWithValidTestData<GlbStaff>();
				admin1.GS_EmailAddress = "admin1@cargowise.com";
				var admin2 = factory.NewWithValidTestData<GlbStaff>();
				admin2.GS_EmailAddress = "admin2@cargowise.com";
				var group = factory.NewWithValidTestData<GlbGroup>();
				group.Staff.Add(admin1);
				group.Staff.Add(admin2);
				WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
				factory.Save();

				global.EmailsNotSentBecauseInTestMode.Clear();
				AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

				InitZGlobalWithHttpContext(global);
				global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
				string url = HttpContext.Current.Response.RedirectLocation;

				SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
				AssertEquals("Configuration Error", qs["title"]);
				AssertEquals("The site is currently experiencing a misconfiguration. We regret any inconvenience caused. Please try again later.<br /><br />Current settings:<br />EnterpriseCode: EDI<br />ServerCode: DAT<br />Branch: <br />Home page: <br />Company name: <br />", qs["message"]);

				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
				var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
				AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
				IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
				AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
			}
			finally
			{
				global.EmailsNotSentBecauseInTestMode.Clear();
			}
		}

		#endregion

		#region TestSignOut

		public void TestSignOut()
		{
			ZGlobalForTesting global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			FormsAuthentication.SetAuthCookie("homer", false);
			global.ApplicationCookie.WriteCookie("meh");
			global.OnCustomSessionStartForTesting(global, EventArgs.Empty);
			string originalId = global.Session.SessionID;
			Assert(((IList<string>)global.Response.Cookies.AllKeys).Contains(FormsAuthentication.FormsCookieName));
			AssertEquals(DateTime.MinValue, global.Response.Cookies.Get(FormsAuthentication.FormsCookieName).Expires);
			Assert(global.Response.Cookies.Get(global.ApplicationCookie.CookieName).Expires > ZDateTime.Now);

			global.SignOut();
			HttpCookie authCookie = global.Response.Cookies.Get(FormsAuthentication.FormsCookieName);
			bool authCookieExpired = DateTime.MinValue != authCookie.Expires && authCookie.Expires < ZDateTime.Now;
			Assert("Should be expired", authCookieExpired);
			HttpCookie appCookie = global.Response.Cookies.Get(global.ApplicationCookie.CookieName);
			bool appCookieExpired = appCookie.Expires < ZDateTime.Now;
			Assert("Should be expired", appCookieExpired);
		}

		#endregion

		#region TestCryptographicExceptionReport

		public void TestCryptographicExceptionReported()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			string exceptionMessage = "Length of the data to decrypt is invalid.";
			HttpContext.Current.AddError(new System.Security.Cryptography.CryptographicException(exceptionMessage));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			string url = HttpContext.Current.Response.RedirectLocation;
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Web Application Error", qs["title"]);
			AssertEquals(exceptionMessage, qs["message"]);
			AssertExceptionWasHandled();
		}

		#endregion

		#region TestFailedToMapThePathErrorPageAndEmail

		public void TestFailedToMapThePathErrorPageAndEmail()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			string exceptionMessage = "Failed to map the path '/Base/PageHeaderWithNavigation.ascx'.";

			HttpContext.Current.AddError(new InvalidOperationException(exceptionMessage));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			string url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Intermittent Error", qs["title"]);
			AssertEquals("An intermittent error has occurred. Please try again in a few minutes. If problems persist, contact web hosting.", qs["message"]);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			Assert("Email should contain error message", global.EmailsNotSentBecauseInTestMode[0].TextBody.IndexOf(exceptionMessage) != -1);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
			AssertExceptionWasHandled();
		}

		#endregion

		public void TestDatabaseUpgradedExceptionIsHandledProperly()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new DatabaseUpgradedException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals((int)HttpStatusCode.ServiceUnavailable, HttpContext.Current.Response.StatusCode);
			AssertEquals($"{HttpStatusCode.ServiceUnavailable}", HttpContext.Current.Response.StatusDescription);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
		}

		public void TestDatabaseUpgradeInProgressExceptionIsHandledProperly()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new DatabaseUpgradeInProgressException());
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals((int)HttpStatusCode.ServiceUnavailable, HttpContext.Current.Response.StatusCode);
			AssertEquals($"{HttpStatusCode.ServiceUnavailable}", HttpContext.Current.Response.StatusDescription);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
		}

		#region TestNullExceptionsRedirectToErrorPage

		public void TestNullExceptionsRedirectToErrorPage()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.AddError(new ArgumentNullException("Value cannot be null."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			string url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Web Application Error", qs["title"]);
			AssertEquals("The Web Application you attempted to access is currently unavailable", qs["message"]);
			AssertExceptionWasHandled();
		}

		#endregion

		#region TestIOExceptionRedirectToErrorPage

		public void TestIOExceptionRedirectToErrorPage()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			HttpContext.Current.AddError(new IOException("The process cannot access the file because it is being used by another process."));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			string url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("File Access Error", qs["title"]);
			AssertEquals("The web application cannot access an internal file because it is being used by another process. Please Contact your Web Administrator to Restart the IIS.", qs["message"]);
		}

		#endregion

		#region TestUriFormatException

		public void TestUriFormatException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var factory = new BusinessObjectFactory();
			var admin1 = factory.NewWithValidTestData<GlbStaff>();
			admin1.GS_EmailAddress = "admin1@cargowise.com";
			var admin2 = factory.NewWithValidTestData<GlbStaff>();
			admin2.GS_EmailAddress = "admin2@cargowise.com";
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(admin1);
			group.Staff.Add(admin2);
			WebDataRegistry.Instance.WebAdminsEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			factory.Save();

			global.EmailsNotSentBecauseInTestMode.Clear();
			AssertEquals("PreCondition: Email count should be zero", 0, global.EmailsNotSentBecauseInTestMode.Count);

			string exceptionMessage = "Invalid URI: The hostname could not be parsed.";

			HttpContext.Current.AddError(new UriFormatException(exceptionMessage));
			global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			string url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			SecureQueryString qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Uri Format Error", qs["title"]);
			AssertEquals("Invalid URI: The hostname could not be parsed.\r\nRequest Referrer: ", qs["message"]);

			AssertEquals("An email should have been sent to the Web Admin", 1, global.EmailsNotSentBecauseInTestMode.Count);
			Assert("Email should contain error message", global.EmailsNotSentBecauseInTestMode[0].HtmlBody.IndexOf(exceptionMessage) != -1);
			var actualRecipients = global.EmailsNotSentBecauseInTestMode[0].To;
			AssertEquals("Should be 2 recipients", 2, actualRecipients.Count);
			IEnumerable<String> emailsCollection = actualRecipients.Mailboxes.Select(x => x.Address);
			AssertContainsExactElementsInAnyOrder("Expected recipients", emailsCollection, new string[] { @"admin1@cargowise.com", @"admin2@cargowise.com" });
		}

		#endregion

		#region TestRefreshRegistryItemCache

		public void TestRefreshRegistryItemCache()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://one.com");
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			Db.Connection.ExecuteNonQuery("update dbo.StmData set SD_BinaryValue = convert(varbinary(max), N'http://two.com') where SD_Name = '" + WebDataRegistry.Instance.CargoWiseUserPortalUrl.Name + "'");
			Thread.Sleep(1100);
			InitZGlobalWithHttpContext(global);
			global.RefreshRegistryItemCache();
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			Db.Connection.ExecuteNonQuery("insert into dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) values (newid(), '" + RawDataRegistry.RegistryUserUpdateVersionItemName + "', convert(varbinary(max), N'2'))");
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			global.RefreshRegistryItemCache();
			AssertEquals("http://one.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
			Thread.Sleep(1100);
			global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
			AssertEquals("http://two.com", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value);
		}

		#endregion

		#region TestApplicationInitializationWithException

		[ExpectNoExceptions]
		[SuppressMessage("CargoWiseOne", "CW1018A:HttpApplicationDbAppSettingsRule", Justification = "Testing")]
		public void TestApplicationInitializationWithException()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName },
				{ ".appDomain", "*" },
				{ ".domainId", ZGuid.NewZGuid().ToString() },
				{ ".appPath", @"C:\inetpub\wwwroot\webapp\" },
				{ ".appVPath", "/webapp" },
				{ ".hostingVirtualPath", HttpRuntime.AppDomainAppVirtualPath },
				{ ".hostingInstallDir", HttpRuntime.AspInstallDirectory },
			};

			try
			{
				using (var appDomainWrapper = new AppDomainWrapper("/LM/W3SVC/1234/ROOT-1-130600647879179810"))
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						EnterpriseApplicationConfiguration.ConfigureObjectFactory();

						HttpRuntime theRuntime = (HttpRuntime)typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
						typeof(HttpRuntime).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(theRuntime, null);
						DummyWorkerRequest workerRequest = new DummyWorkerRequest("default.aspx", "", new StringWriter());
						HttpContext.Current = new HttpContext(workerRequest);
						HttpApplication testApplication = new DummyHttpApplication(workerRequest);
						typeof(HttpApplication).InvokeMember("InitInternal", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, testApplication, new object[] { HttpContext.Current, testApplication.Application, Array.Empty<MethodInfo>() });
						typeof(HttpApplication).InvokeMember("_context", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, testApplication, new object[] { HttpContext.Current });
						HttpContext.Current.ApplicationInstance = testApplication;
						HttpSessionStateContainer container = new HttpSessionStateContainer("DummySession", new SessionStateItemCollection(), new HttpStaticObjectsCollection(), 60, true, HttpCookieMode.AutoDetect, SessionStateMode.InProc, false);
						SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, container);
						HttpContext.Current.Request.Browser = new HttpBrowserCapabilities();
						HttpContext.Current.Request.Browser.Capabilities = new HybridDictionary();

						TestingState.SuspendIsRunningTests().Dispose();
						ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
						ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
#pragma warning disable HttpApplicationDbAppSettingsRule
						Db.InitializeDatabaseDetails(
							ConfigurationManager.AppSettings["ServerName"],
							ConfigurationManager.AppSettings["DatabaseName"],
							CargoWise.DataProtection.ApplicationType.Web);
#pragma warning restore HttpApplicationDbAppSettingsRule

						var global = new ZGlobalForApplicationStartTesting();
						TestGlobal.InitZGlobalWithHttpContext(global);

						try
						{
							global.Application_Start_Internal(null, EventArgs.Empty);
						}
						catch
						{ }
						Assert("Does not suppress exception before initialisation", !global.WebExceptionReporterForTesting.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions);

						global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty);
						Assert("Supress exception after initialisation", global.WebExceptionReporterForTesting.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions);
					}, domainData);
				}
			}
			finally
			{
				CultureInfo.CurrentCulture = DefaultCulture.Instance;
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[ExpectNoExceptions]
		public void TestInitializeApplicationExceptionReporter()
		{
			// Arrange
			using (TemporaryAppDomain(appDomain => { }, () =>
			{
				var globalMock = new Mock<ZGlobalForTesting> { CallBase = true };
				var webUpgradeManagerStartedEvent = new ManualResetEventSlim(false);

				// Act
				using (WebUpgradeBootstrapper.DisposableWebUpgradeManagerStartedAction_ForTest((webUpgradeManager) => webUpgradeManagerStartedEvent.Set()))
				using (var global = globalMock.Object)
				using (global.StartApplicationDisposable())
				{
					AssertNotNull(global.WebExceptionReporterForTesting);
					Assert(global.WebExceptionReporterForTesting.SuppressExceptionsThatOccurWhilstReportingDeveloperExceptions);
					AssertType(typeof(WebExceptionReporter), global.WebExceptionReporterForTesting);
					AssertType(typeof(WebExceptionReporter), ErrorReporter.Instance);
				}
			}))
			{ }
		}

		[ExpectNoExceptions]
		public void TestApplicationStartedWithoutExceptions()
		{
			// Arrange
			using (TemporaryAppDomain(appDomain => { }, () =>
			{
				var globalMock = new Mock<ZGlobal> { CallBase = true };
				var webUpgradeManagerStartedEvent = new ManualResetEventSlim(false);

				// Act
				using (WebUpgradeBootstrapper.DisposableWebUpgradeManagerStartedAction_ForTest((webUpgradeManager) => webUpgradeManagerStartedEvent.Set()))
				using (var global = globalMock.Object)
				using (global.StartApplicationDisposable())
				{
					AssertType<WebEnvProvider>(Env.GetCurrentProvider());
					AssertNotNull(NotificationHandler.Instance);
					AssertType<ZWebNotificationHandler>(NotificationHandler.Instance);
					Assert(!Directory.Exists(CommonProgramData.GetCargoWiseDirectory("ResourceStringUsageData", Db.ServerName, Db.DatabaseName)));
					Assert(!Globals.IsUserInteractive);
				}
			}))
			{ }
		}

		[ExpectNoExceptions]
		[TestRequiresAdministrativePrivileges("Saving to the system registry")]
		public void TestInitializeDatabase()
		{
			// Arrange
			using (TemporaryAppDomain(appDomain => { }, () =>
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var globalMock = new Mock<ZGlobal> { CallBase = true };
				var webUpgradeManagerStartedEvent = new ManualResetEventSlim(false);

				TestingState.SuspendIsRunningTests().Dispose();
				ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
				ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

				// Act
				using (WebUpgradeBootstrapper.DisposableWebUpgradeManagerStartedAction_ForTest((webUpgradeManager) => webUpgradeManagerStartedEvent.Set()))
				using (var global = globalMock.Object)
				using (global.StartApplicationDisposable())
				{
					AssertEquals(AppDomain.CurrentDomain.GetData("ServerName"), Db.ServerName);
					AssertEquals(AppDomain.CurrentDomain.GetData("DatabaseName"), Db.DatabaseName);
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}))
			{ }

			// Arrange
			using (TemporaryAppDomain(appDomain => { }, () =>
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				var globalMock = new Mock<ZGlobal> { CallBase = true };
				var webUpgradeManagerStartedEvent = new ManualResetEventSlim(false);

				TestingState.SuspendIsRunningTests().Dispose();
				ConfigurationManager.AppSettings["ServerName"] = (string)AppDomain.CurrentDomain.GetData("ServerName");
				ConfigurationManager.AppSettings["DatabaseName"] = (string)AppDomain.CurrentDomain.GetData("DatabaseName");

				WebDbConfiguration.SaveConfiguration(new WebDbConfigurationInfo() { ApplicationPath = WebAppPath.ForCurrentAppDomain(), ServerName = (string)AppDomain.CurrentDomain.GetData("ServerName"), DatabaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName") });
				TestingState.SuspendIsRunningTests().Dispose();

				// Act
				using (WebUpgradeBootstrapper.DisposableWebUpgradeManagerStartedAction_ForTest((webUpgradeManager) => webUpgradeManagerStartedEvent.Set()))
				using (var global = globalMock.Object)
				using (global.StartApplicationDisposable())
				{
					try
					{
						AssertEquals(AppDomain.CurrentDomain.GetData("ServerName"), Db.ServerName);
						AssertEquals(AppDomain.CurrentDomain.GetData("DatabaseName"), Db.DatabaseName);
					}
					finally
					{
						WebDbConfiguration.DeleteAllConfigurations(WebAppPath.ForCurrentAppDomain());
					}
				}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
			}))
			{ }
		}

		public static IDisposable TemporaryAppDomain(Action<AppDomain> appDomainSetData, Action appDomainCallBack)
		{
			var domainData = new Dictionary<string, object>
			{
				{ ".appDomain", "*" },
				{ ".domainId", ZGuid.NewZGuid().ToString() },
				{ ".appPath", @"C:\inetpub\wwwroot\webapp\" },
				{ ".appVPath", "/webapp" },
				{ ".hostingVirtualPath", HttpRuntime.AppDomainAppVirtualPath },
				{ ".hostingInstallDir", HttpRuntime.AspInstallDirectory },
				{ "appDomainCallBack", appDomainCallBack },
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName },
			};

			using (var appDomainWrapper = new AppDomainWrapper(FormattableString.Invariant($"/LM/W3SVC/1234/ROOT-1-130600647879179810")))
			{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
				appDomainWrapper.RunActionInAppDomain(() =>
				{
					AppDomain currentDomain = AppDomain.CurrentDomain;
					var action = (Action)currentDomain.GetData("appDomainCallBack");
					ConfigurationManager.AppSettings["ServerName"] = (string)currentDomain.GetData("ServerName");
					ConfigurationManager.AppSettings["DatabaseName"] = (string)currentDomain.GetData("DatabaseName");

					var hostingEnvironment = new HostingEnvironment();
					var waitCallback = new WaitCallback(state => { });
					typeof(HostingEnvironment)
						?.GetField("_initiateShutdownWorkItemCallback", BindingFlags.NonPublic | BindingFlags.Instance)
						?.SetValue(hostingEnvironment, waitCallback);

					action?.Invoke();
				}, domainData);
			}

			return new DisposableAction(delegate
			{
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public static void InitZGlobalWithHttpContext(ZGlobal global)
		{
			typeof(HttpApplication).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(global, HttpContext.Current);
		}

		#endregion

		public void TestGetValidRedirectUrl()
		{
			var global = new ZGlobalForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(global.DefaultPage, global.GetValidRedirectURL("not a url"));
				AssertEquals(global.DefaultPage, global.GetValidRedirectURL("http://www.openredirectionvulnerability.com"));
				AssertEquals(global.DefaultPage, global.GetValidRedirectURL("wss:openredirectionvulnerability.com"));
				AssertEquals(global.DefaultPage, global.GetValidRedirectURL(global.DefaultPage));
				AssertEquals("/Shipments/Shipments.aspx", global.GetValidRedirectURL("/Shipments/Shipments.aspx"));
				AssertEquals("Should not allow .. to exit above the top directory", global.DefaultPage, global.GetValidRedirectURL("/../../.."));
			});
		}

		public void TestBeginRequest_DbUpgradeAlreadyThrown()
		{
			// Arrange
			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (new DbEnvironmentWithMockGuiPlugin())
			using (new DisposableAction(
				() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
				() =>
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
					Db.ResetDatabaseUpgraded_ForTest();
				}))
			{
				var global = new ZGlobalForTesting();
				InitZGlobalWithHttpContext(global);
				var isSchemaVersionCheckDisabled = Db.IsSchemaVersionCheckDisabled;

				// Cause a DatabaseUpgradedException
				AssertExceptionThrown<DatabaseUpgradedException>(() =>
					((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());

				// Act/Assert
				CombineAssertions(() =>
				{
					AssertExceptionThrown<DatabaseUpgradedException>(() => global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty));
					Assert("SchemaVersionCheck was disabled before test started, please check Test Sequence File to find out the pollution", !isSchemaVersionCheckDisabled);
				});
			}
		}

		public void TestBeginRequestShouldThrowDatabaseUpgradedException_DbUpgradedInMultipleThreads()
		{
			// Arrange
			var global = new ZGlobalForTesting();
			TestGlobal.InitZGlobalWithHttpContext(global);
			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
			using (var disposable = new DisposableAction(
					() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
					() =>
					{
						Db.ConnectionOverrideForTest.Dispose();
						Db.ConnectionOverrideForTest = null;
						Db.ResetDatabaseUpgraded_ForTest();
					}))
			{
				var currentContext = HttpContext.Current;
				var thread1 = new Thread(() =>
				{
					// Act
					AssertNoExceptionThrown(() => global.Application_BeginRequest_ForTesting(currentContext.Application, EventArgs.Empty));
					Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);
				});

				thread1.Start();
				thread1.Join();

				// Assert
				AssertExceptionThrown<DatabaseUpgradedException>(() => global.Application_BeginRequest_ForTesting(HttpContext.Current.Application, EventArgs.Empty));
			}

			Assert("Db.IsDatabaseUpgraded is false", !Db.IsDatabaseUpgraded);
		}

		public void TestApplicationErrorShouldNotReportConfigurationErrorsException_DbUpgradedThrownInMultipleThreads()
		{
			// Arrange
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new ConfigurationErrorsException("This is a test configuration exception"));

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
			using (var disposable = new DisposableAction(
					() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
					() =>
					{
						Db.ConnectionOverrideForTest.Dispose();
						Db.ConnectionOverrideForTest = null;
						Db.ResetDatabaseUpgraded_ForTest();
					}))
			{
				var currentContext = HttpContext.Current;
				var thread1 = new Thread(() =>
				{
					//Act
					AssertExceptionThrown<DatabaseUpgradedException>(() =>
						global.Application_Error_ForTesting(currentContext.ApplicationInstance, EventArgs.Empty));
					Assert("Db.IsDatabaseUpgraded is true", Db.IsDatabaseUpgraded);
				});

				thread1.Start();
				thread1.Join();
				AssertNoExceptionThrown(() => global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty));

				//Assert
				AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals((int)HttpStatusCode.OK, HttpContext.Current.Response.StatusCode);
				AssertEquals($"{HttpStatusCode.OK}", HttpContext.Current.Response.StatusDescription);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		[UseSnapshotProtection]
		public void TestApplicationErrorShouldReportOtherException_DbUpgradedThrownInMultipleThreads()
		{
			// Arrange
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);
			WebInitialiser.Initialise();

			HttpContext.Current.AddError(new Exception("This is a test exception"));

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			using (ObjectFactory.Substitute(versionMock.Object))
			using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
			using (var disposable = new DisposableAction(
					() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
					() =>
					{
						Db.ConnectionOverrideForTest.Dispose();
						Db.ConnectionOverrideForTest = null;
						Db.ResetDatabaseUpgraded_ForTest();
					}))
			{
				var currentContext = HttpContext.Current;
				var thread1 = new Thread(() =>
				{
					//Act
					AssertExceptionThrown<DatabaseUpgradedException>(() =>
						global.Application_Error_ForTesting(currentContext.ApplicationInstance, EventArgs.Empty));
				});

				thread1.Start();
				thread1.Join();
				AssertNoExceptionThrown(() => global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty));

				//Assert
				AssertEquals(false, HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals((int)HttpStatusCode.InternalServerError, HttpContext.Current.Response.StatusCode);
				AssertEquals($"{HttpStatusCode.InternalServerError}", HttpContext.Current.Response.StatusDescription);
				AssertEquals(1, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestReportRuntimeFileAccessException()
		{
			var global = new ZGlobalForTesting();
			InitZGlobalWithHttpContext(global);

			var exception = new HttpException("The file '/XXX/Runtime/An-insanely-long-path-with-assembly-name-and-version-and-class-hierarchy-split-up-into-subfolders/A-random-user-control.ascx' does not exist.");
			HttpContext.Current.AddError(exception);

			AssertNoExceptionThrown(() => global.Application_Error_ForTesting(HttpContext.Current.ApplicationInstance, EventArgs.Empty));
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			var url = HttpContext.Current.Response.RedirectLocation;
			AssertStartsWith("Should redirect to error page", "/Error.aspx", url);
			var qs = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?data=") + "?data=".Length)));
			AssertEquals("Runtime file extraction error", qs["title"]);
			AssertContains("The web application cannot access a file that was extracted by the runtime. This is very likely due to the extraction path becoming too long", qs["message"]);
			AssertExceptionWasHandled();
		}
	}
}
