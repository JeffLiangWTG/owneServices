using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Authentication.Glow.Client;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;
using WTG.Foundation.FrameworkExtensions.IO;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class PersistentSingleSignOnAuthenticationControllerTest : TestCase
	{
		public void TestAjaxAsync_LoginErrorAccountLocked()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.AccountLocked;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: AccountLocked").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorLogonDetailsIncorrect()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.LogonDetailsIncorrect;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: LogonDetailsIncorrect").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorLogonDetailsIncorrectWithCorrectDatabaseConfiguration()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.LogonDetailsIncorrect;
			var responseHeaders = new Dictionary<string, IEnumerable<string>>();

			using (var command = Db.Connection.Command("GetInstanceId"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddOutputParameter("@InstanceId", SqlDbType.VarChar, 10, 0, 0, null);
				command.ExecuteNonQuery();

				var instanceId = (string)command.GetParameterValue("@InstanceId");
				responseHeaders.Add("wtg-instid", new List<string>() { instanceId });
			}

			beginSessionDetails.ResponseHeaders = responseHeaders.ToLookup(a => a.Key, a => a.Value, StringComparer.OrdinalIgnoreCase);
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: LogonDetailsIncorrect").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorLogonDetailsIncorrectWithDatabaseConfigurationError()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.LogonDetailsIncorrect;
			var responseHeaders = new Dictionary<string, IEnumerable<string>>();
			responseHeaders.Add("wtg-instid", new List<string>() { "blah" });
			beginSessionDetails.ResponseHeaders = responseHeaders.ToLookup(a => a.Key, a => a.Value, StringComparer.OrdinalIgnoreCase);
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: LogonDetailsIncorrect").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorPasswordChangeRequired()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.PasswordChangeRequired;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: PasswordChangeRequired").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorWebAccessNotEnabled()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.WebAccessNotEnabled;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: WebAccessNotEnabled").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorCouldNotCalculateEnterpriseContext()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.CouldNotCalculateEnterpriseContext;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: CouldNotCalculateEnterpriseContext").GetAwaiter().GetResult();
		}

		public void TestAjaxAsync_LoginErrorThirdPartyUserValidationRequired()
		{
			beginSessionDetails.AuthenticationResult = AuthenticationResult.ThirdPartyUserValidationRequired;
			AssertAsyncException<AuthorizationFailureException>(async () => await controller.AuthenticateAsync(clientSessionServiceMock.Object), "Unexpected authentication result: ThirdPartyUserValidationRequired").GetAwaiter().GetResult();
		}

		async Task AssertAsyncException<T>(Func<Task> codeToRun, string expectedExceptionMessage) where T : Exception
		{
			try
			{
				await codeToRun();
				throw new Exception($"Exception expected to be thrown with message: '{expectedExceptionMessage}', but none was thrown.");
			}
			catch (T ex)
			{
				AssertEquals(ex.Message, expectedExceptionMessage);
			}
		}

		public void TestAuthenticateAsync_SuccessfulLogin()
		{
			var guid = Guid.NewGuid();
			beginSessionDetails.AuthenticationResult = AuthenticationResult.Success;
			beginSessionDetails.SessionId = guid;

			mockUserSession.SetupProperty(x => x.SessionId);
			mockUserSession.SetupProperty(x => x.AuthenticationToken);

			controller.AuthenticateAsync(clientSessionServiceMock.Object).Wait();
			clientSessionServiceMock.Verify(s => s.BeginSessionWithSingleSignOnTokenAsync(), Times.Once);

			mockUserSession.VerifySet(x => x.AuthenticationToken = "sometoken", Times.Once);
			mockUserSession.VerifySet(x => x.SessionId = guid, Times.Once);

			AssertEquals(mockUserSession.Object.AuthenticationToken, "sometoken");
			AssertEquals(mockUserSession.Object.SessionId, guid);
		}

		public void TestLoadAuthenticationStateAsync_ReturnsDefaultWhenNoFileExists()
		{
			var task = controller.LoadAuthenticationStateAsync();
			Assert(task.Result.IsDefault);
		}

		public void TestLoadAuthenticationStateAsync_ReturnsDefaultWhenDifferentUserLoads()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();

			var factory = new BusinessObjectFactory();
			var tempStaff = factory.New<IGlbStaff>();

			var controllerSameUser = new PersistentSingleSignOnAuthenticationController(mockUserSession.Object, GetCurrentUserEnv(), true) { TestPath = TempForTest.TempPath };
			var controllerDifferentUser = new PersistentSingleSignOnAuthenticationController(
				mockUserSession.Object,
				new() { UserPK = tempStaff.PK.ToGuid(), Branch = Env.CurrentBranchPK, Department = Env.CurrentDepartmentPK },
				true)
			{ TestPath = TempForTest.TempPath };

			var task = controllerSameUser.LoadAuthenticationStateAsync();
			AssertEquals(task.Result, session);

			task = controllerDifferentUser.LoadAuthenticationStateAsync();
			Assert(task.Result.IsDefault);

			// The same user can still load after logging back in
			CheckStateIsCorrect();
		}

		public void TestLoadAuthenticationStateAsync_ReturnsSessionDetailsWhenFileExists()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();

			var task = controller.LoadAuthenticationStateAsync();
			AssertEquals(task.Result, session);
		}

		public void TestLoadAuthenticationStateAsync_ReturnsDefaultWhenCorruptedSmallFileExists()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();
			File.WriteAllText(filePath, "o");

			var task = controller.LoadAuthenticationStateAsync();
			Assert(task.Result.IsDefault);
		}

		public void TestLoadAuthenticationStateAsync_ReturnsDefaultWhenCorruptedHugeFileExists()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();
			File.WriteAllText(filePath, "massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted massive corrupted ");

			var task = controller.LoadAuthenticationStateAsync();
			Assert(task.Result.IsDefault);
		}

		public void TestSaveAuthenticationStateAsync_CreatesFileWhenNoneExists()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();
			CheckStateIsCorrect();
		}

		public void TestSaveAuthenticationStateAsync_UpdatesExistingStateFile()
		{
			controller.SaveAuthenticationStateAsync(session).Wait();
			File.WriteAllText(filePath, "overriding to garbage");

			controller.SaveAuthenticationStateAsync(session).Wait();
			CheckStateIsCorrect();
		}

		public void TestSaveAuthenticationStateAsync_DoNothingWhenPersistentToFileIsFalse()
		{
			controller = new PersistentSingleSignOnAuthenticationController(mockUserSession.Object, GetCurrentUserEnv(), false);
			controller.TestPath = TempForTest.TempPath;
			controller.SaveAuthenticationStateAsync(session).Wait();

			Assert(!File.Exists(filePath));
		}

		void CheckStateIsCorrect()
		{
			Assert(File.Exists(filePath));
			var savedState = controller.LoadAuthenticationStateAsync().Result;
			AssertEquals(savedState, session);
		}

		void DeleteStateFile()
		{
			if (Directory.Exists(cleanupRootDirectory))
			{
				DirectoryExtensions.Delete(cleanupRootDirectory, true, true);
			}
		}

		PersistentSingleSignOnAuthenticationController controller;
		ClientBeginSessionDetails beginSessionDetails;
		Mock<IClientSessionService> clientSessionServiceMock;
		Guid sessionGuid;
		SessionAuthenticationDetails session;
		Mock<IUserSession> mockUserSession;
		string testRoot;
		string cleanupRootDirectory;
		string filePath;

		protected override void SetUp()
		{
			base.SetUp();

			testRoot = Path.Combine(TempForTest.TempPath, Db.ServerName);
			cleanupRootDirectory = Path.Combine(TempForTest.TempPath, Db.ServerName.Split('\\', '/')[0]);
			var fileLocation = Path.Combine(testRoot, Db.DatabaseName);

			filePath = Path.Combine(fileLocation, "glowauth.dat");

			DeleteStateFile();

			beginSessionDetails = new ClientBeginSessionDetails();
			clientSessionServiceMock = new Mock<IClientSessionService>();
			clientSessionServiceMock.Setup(s => s.BeginSessionWithSingleSignOnTokenAsync()).Returns(Task.FromResult(beginSessionDetails));

			var ssoHelperMock = new Mock<IGlowSingleSignOnTokenProvider>();
			ssoHelperMock.Setup(c => c.CreateLimitedToken(It.IsAny<Guid>(), It.IsAny<string>(), It.Is<GlowSingleSignOnTokenOptions>(o => o.IsCaptiveSession))).Returns("sometoken");

			ObjectFactory.Substitute(ssoHelperMock.Object);

			mockUserSession = new Mock<IUserSession>();

			sessionGuid = Guid.NewGuid();
			session = new SessionAuthenticationDetails
			{
				AuthenticationToken = "sometoken",
				AuthenticationTicketHeader = "someheader",
				SessionId = sessionGuid,
			};

			controller = new PersistentSingleSignOnAuthenticationController(mockUserSession.Object, GetCurrentUserEnv(), true);
			controller.TestPath = TempForTest.TempPath;
		}

		internal static UserEnv GetCurrentUserEnv()
			=> new() { UserPK = Env.CurrentUserPK, Branch = Env.CurrentBranchPK, Department = Env.CurrentDepartmentPK };

		protected override void TearDown()
		{
			base.TearDown();
			DeleteStateFile();
		}
	}

	class PersistentSingleSignOnAuthenticationControllerCommitTest : TransactionedTestCase
	{
		public void TestAuthenticateAsync_ThreadSafety()
		{
			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					controller.AuthenticateAsync(clientSessionServiceMock.Object).Wait();
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join();

			exceptionInfo?.Throw();

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		PersistentSingleSignOnAuthenticationController controller;
		Mock<IClientSessionService> clientSessionServiceMock;

		protected override void SetUp()
		{
			base.SetUp();

			var beginSessionDetails = new ClientBeginSessionDetails();
			clientSessionServiceMock = new Mock<IClientSessionService>();
			clientSessionServiceMock.Setup(s => s.BeginSessionWithSingleSignOnTokenAsync()).Returns(Task.FromResult(beginSessionDetails));

			var mockUserSession = new Mock<IUserSession>();
			controller = new PersistentSingleSignOnAuthenticationController(mockUserSession.Object, PersistentSingleSignOnAuthenticationControllerTest.GetCurrentUserEnv(), true);
		}
	}
}
