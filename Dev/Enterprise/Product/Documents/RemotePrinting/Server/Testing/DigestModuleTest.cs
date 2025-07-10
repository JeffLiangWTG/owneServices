using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWiseOne.WebInfrastructure;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class DigestModuleTest : TestCase
	{
		[HttpContextEnabledTest]
		public void TestApplicationErrorHandler_RelatedDBConnectionException()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();
			digestModule.HandleApplicationException_ExposedForTest(new InvalidOperationException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached."));

			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals(app.Response.StatusCode, 538);
			AssertEquals(app.Response.StatusDescription, "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.");
		}

		[HttpContextEnabledTest]
		public void TestDenyAccessWithError_DatabaseUpgradedException()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();
			digestModule.IsDatabaseVersionOlder = true;
			digestModule.OnAuthenticateRequest(HttpContext.Current.ApplicationInstance, new EventArgs());

			AssertEquals(app.Response.StatusCode, 535);
			AssertEquals(app.Response.StatusDescription, new DatabaseUpgradedException().Message);

			digestModule.IsDatabaseVersionOlder = false;
			digestModule.OnAuthenticateRequest(HttpContext.Current.ApplicationInstance, new EventArgs());

			AssertEquals(app.Response.StatusCode, 536);
			AssertEquals(app.Response.StatusDescription, new DatabaseUpgradedException(false).Message);
		}

		public void TestApplicationErrorHandler_SendReports()
		{
			// In DigestModule.Init(), new event handler should be subscribed to application.Error
			// Check that it is subscribed to and reports errors

			var digestModule = new DigestModuleForTest();

			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.BadRequest, "Test"));
			AssertEquals("Bad Request 400 Error in WebPrint Http Application", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.NotFound, "Test"));
			AssertEquals("Not Found 404 Error in WebPrint Http Application", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			digestModule.HandleApplicationException_ExposedForTest(new InvalidOperationException("Test"));
			AssertEquals("Exception in WebPrint Http Application", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			digestModule.HandleApplicationException_ExposedForTest(new HttpException(418, "Test"));
			AssertEquals("418 Error in WebPrint Http Application", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestApplicationErrorHandler_ShouldNotReports()
		{
			var digestModule = new DigestModuleForTest();

			//Should not report error if the exception is HttpException with missing remoteprintingservice.asmx with proper url
			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.NotFound, "The file '/webprint/remoteprintingservice.asmx' does not exist."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.BadRequest, "A potentially dangerous Request.Path value was detected from the client (<)."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.InternalServerError, "Request timed out."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(new HttpException((int)HttpStatusCode.InternalServerError, "The client disconnected."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(CreateInvalidOperationExceptionWithUnhandledXmlSchemaExceptionForTest("The global attribute 'http://www.w3.org/XML/1998/namespace:lang' has already been declared."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(CreateInvalidOperationExceptionWithUnhandledXmlSchemaExceptionForTest("The global element 'http://www.w3.org/2001/XMLSchema:schema' has already been declared."));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(new SoapException("Server was unable to read request.", SoapException.ClientFaultCode, new XmlException("Root element is missing.")));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			digestModule.HandleApplicationException_ExposedForTest(new SoapException("Server was unable to read request.", SoapException.ClientFaultCode, new XmlException("There is an unclosed literal string. Line 1, position 134.")));
			Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();

			InvalidOperationException CreateInvalidOperationExceptionWithUnhandledXmlSchemaExceptionForTest(string errorMessage)
			{
				var xmlSchemaException = new XmlSchemaExceptionForTest(errorMessage);
				var httpUnhandledException = new HttpUnhandledException("Exception of type 'System.Web.HttpUnhandledException' was thrown.", xmlSchemaException);
				var invalidOperationException = new InvalidOperationException("The XML Web service help page encountered an internal error.", httpUnhandledException);
				return invalidOperationException;
			}
		}

		public void TestApplicationErrorHandler_ShouldNotReportForSignalRProllingException()
		{
			var digestModule = new DigestModuleForTest();

			digestModule.HandleApplicationException_ExposedForTest(new ArgumentNullException());
			AssertEquals("Should report error", false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();

			digestModule.HandleApplicationException_ExposedForTest(new ArgumentNullExceptionForTest());
			AssertEquals("Should report error", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			ErrorReporter.Clear();
		}

		[HttpContextEnabledTest]
		[UseSnapshotProtection]
		public void TestApplicationErrorHandler_ShouldNotReportIfDatabaseIsUpgraded()
		{
			var digestModule = new DigestModuleForTest();
			using (Db.DisposableUpgrade_ForTest(acquireLockOut: false))
			{
				// Arrange Cause a DatabaseUpgradedException
				AssertExceptionThrown<DatabaseUpgradedException>(() =>
					((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection());
				digestModule.OnAuthenticateRequest(HttpContext.Current.ApplicationInstance, new EventArgs());
				//Act
				digestModule.HandleApplicationException_ExposedForTest(new Exception("This is a test exception"));
				//Assert
				Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
		}

		public void TestApplicationErrorHandler_ShouldNotReportIfIsUpgrateRunning()
		{
			var digestModule = new DigestModuleForTest();
			var sqlContext = new WebUpgradeSqlContext(Db.ServerName, Db.DatabaseName, () => ((IDbConnectionInternals)Db.Connection).ADOConnection);
			var upgradeManager = new WebUpgradeManager(sqlContext);
			var webUpdaterMutexName = WebUpgradeManager.GetUpdaterMutexName(sqlContext.DatabaseName);
			using (var manualResetEventForRunning = new ManualResetEvent(false))
			using (var manualResetEventForWaitting = new ManualResetEvent(false))
			{
				Task task = null;
				try
				{
					task = Task.Run(() =>
					{
						using (var mutex = new UpgraderMutex(webUpdaterMutexName))
						{
							manualResetEventForWaitting.Set();
							manualResetEventForRunning.WaitOne();
						}
					});
					manualResetEventForWaitting.WaitOne();

					AssertEquals("It's running upgrade", true, WebUpgradeManager.IsUpgrateRunning());
					digestModule.HandleApplicationException_ExposedForTest(new Exception("Jerry Test"));
					Assert("Should not report error", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				}
				finally
				{
					manualResetEventForRunning.Set();
					task.Wait();
				}
			}
		}

		[HttpContextEnabledTest]
		public void TestCWSupportLogin()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();

			//response is hashed password and nonce is generated for current time
			//With correct username expired support token
			var invalidToken = CWSupportLoginToken.GenerateTokenForTest("XYZ", "", "", new CargoWise.Types.ZDateTime(2023, 1, 1));
			var authString = GetAuthString(Authentication.SupportUserPrefix + invalidToken, app.Request.HttpMethod, "");
			var shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out _);
			AssertEquals(true, shouldDenyAccess);

			//With incorrect support prefix
			var validToken = CWSupportLoginToken.TokenForTest;
			authString = GetAuthString("CWSupport" + validToken, app.Request.HttpMethod, "");
			shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out _);
			AssertEquals(true, shouldDenyAccess);

			//With correct username and correct support token
			authString = GetAuthString(Authentication.SupportUserPrefix + validToken, app.Request.HttpMethod, "");
			shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out _);
			AssertEquals(false, shouldDenyAccess);
		}

		[HttpContextEnabledTest]
		[TestDate(2021, 08, 27, 12, 56, 24)]
		public void TestRolesForSupportUser()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();

			var validToken = CWSupportLoginToken.TokenForTest;
			var authString = GetAuthString(Authentication.SupportUserPrefix + validToken, app.Request.HttpMethod, "");
			var denied = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out var roles);
			AssertEquals(false, denied);
			AssertCollectionContains(Authentication.SupportRole, roles);
			AssertCollectionContains(Authentication.UserRole, roles);

			authString = GetAuthString("user1", app.Request.HttpMethod, "test1");

			denied = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out roles);
			AssertEquals(true, denied);
			AssertCollectionNotContains(Authentication.SupportRole, roles);
			AssertCollectionNotContains(Authentication.UserRole, roles);

			digestModule.ApplicationUserForTest = "user1";
			digestModule.ApplicationPwdForTest = "test1";

			denied = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out roles);
			digestModule.ApplicationUserForTest = null;
			AssertEquals(false, denied);
			AssertCollectionNotContains(Authentication.SupportRole, roles);
			AssertCollectionContains(Authentication.UserRole, roles);
		}

		[HttpContextEnabledTest]
		public void TestOutputUser()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();

			var validToken = CWSupportLoginToken.TokenForTest;
			var authString = GetAuthString(Authentication.SupportUserPrefix + validToken, app.Request.HttpMethod, "");
			digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out var userName, out _);
			AssertEquals("CWSupport-" + validToken, userName);

			authString = GetAuthString("user1", app.Request.HttpMethod, "test1");
			digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out userName, out _);
			AssertEquals("user1", userName);
		}

		[HttpContextEnabledTest]
		public void TestShouldNotDenyAccessWithoutAuthInfoInRequest()
		{
			var app = HttpContext.Current.ApplicationInstance;
			var digestModule = new DigestModuleForTest();

			string authString = null;
			var shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out var roles);
			AssertEquals("Should not deny access if auth info is not provided", false, shouldDenyAccess);
			AssertEquals("There should be no roles listed", 0, roles.Length);

			authString = string.Empty;
			shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out roles);
			AssertEquals("Should not deny access if auth info is not provided", false, shouldDenyAccess);
			AssertEquals("There should be no roles listed", 0, roles.Length);

			authString = "Digest abc= 1, xyz= 2";
			shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out roles);
			AssertEquals("Should not deny access if auth info is not provided", false, shouldDenyAccess);
			AssertEquals("There should be no roles listed", 0, roles.Length);

			authString = GetAuthString("abc", app.Request.HttpMethod, "xyz");
			shouldDenyAccess = digestModule.ShouldDenyAccess(authString, app.Request.HttpMethod, app.Context, out _, out roles);
			AssertEquals("Should deny access for incorrect login/password", true, shouldDenyAccess);
			AssertEquals("There should be no roles listed", 0, roles.Length);
		}

		string GetAuthString(string userName, string httpMethod, string password)
		{
			var authorizationString = string.Format("Digest username= {0}, realm=RemotePrinting,nonce=MjcvMDgvMjAyMSAxMjo1NzoxMSBQTQ," +
								"uri='/',algorithm=MD5,cnonce=94eb99aecec40108e950a2a831c19da5," +
								"nc=00000003,qop=auth,response= cdf31cd30a19a5e69c575524b1787d99, " +
								"opaque=0000000000000000", userName);

			var requestInfo = Authentication.Instance.GetDigestAuthorizationRequestInfo(authorizationString);

			var hashedDigest = Authentication.Instance.GetDigestHash(requestInfo, httpMethod, password);
			authorizationString = string.Format("Digest username= {0}, realm=RemotePrinting,nonce=MjcvMDgvMjAyMSAxMjo1NzoxMSBQTQ," +
								"uri='/',algorithm=MD5,cnonce=94eb99aecec40108e950a2a831c19da5," +
								"nc=00000003,qop=auth,response= {1}, " +
								"opaque=0000000000000000", userName, hashedDigest);

			return authorizationString;
		}

		class DigestModuleForTest : DigestModule
		{
			public bool IsDatabaseVersionOlder;
			protected override void ThrowExceptionForTesting()
			{
				throw new DatabaseUpgradedException(IsDatabaseVersionOlder);
			}
		}

		[Serializable]
		class XmlSchemaExceptionForTest : XmlSchemaException
		{
			public XmlSchemaExceptionForTest(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			protected XmlSchemaExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

			public override string StackTrace => "at ASP.defaultwsdlhelpgenerator_aspx.Page_Load(Object sender, EventArgs e)";
		}

		[Serializable]
		class ArgumentNullExceptionForTest : ArgumentNullException
		{
			public ArgumentNullExceptionForTest() : base() { }

#if NETFRAMEWORK
			protected ArgumentNullExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

			public override string StackTrace => "at Microsoft.AspNet.SignalR.Transports.LongPollingTransport";
		}
	}
}
