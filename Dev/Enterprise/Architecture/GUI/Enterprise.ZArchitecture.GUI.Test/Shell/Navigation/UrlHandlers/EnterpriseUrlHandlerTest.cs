using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class EnterpriseUrlHandlerTest : TestCaseWithDummy
	{
#if !WINZOR
		#region RegisterRemotingServerAndEdiLoadLocation

		public void TestRegisterInstance()
		{
			var licenceKeyIdentifier = StaticCurrentFetcher.Instance.CurrentCompany.LicenceKeyIdentifier;
			var currentBranchCode = StaticCurrentFetcher.Instance.CurrentBranch.GB_Code;
			DeleteEdiLoadLocationRegistryKey(licenceKeyIdentifier);

			EnterpriseUrlHandlerService.Instance.RegisterInstance();
			using (var licenceRegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes\EdiEnterprise.edient\shell\open\command\" + licenceKeyIdentifier, true))
			{
				AssertEquals(Db.ServerName + " " + Db.DatabaseName, licenceRegistryKey.GetValue("StartParameters"));
				AssertEquals("BranchCode value = current branch code", currentBranchCode, licenceRegistryKey.GetValue("BranchCode"));
				AssertEquals(ApplicationType.CargoWiseRDP, licenceRegistryKey.GetValue("ApplicationType"));
			}
			DeleteEdiLoadLocationRegistryKey(licenceKeyIdentifier);
		}

		[ExpectNoExceptions]
		public void TestRegisterInstance_WhenRegistryAccessIsRestricted()
		{
			var licenceKeyIdentifier = StaticCurrentFetcher.Instance.CurrentCompany.LicenceKeyIdentifier;
			var currentBranchCode = StaticCurrentFetcher.Instance.CurrentBranch.GB_Code;
			DeleteEdiLoadLocationRegistryKey(licenceKeyIdentifier);

			using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\EdiEnterprise.edient\shell\open\command"))
			{
				var registrySecurity = key.GetAccessControl();
				var rule = new RegistryAccessRule(WindowsIdentity.GetCurrent().Name, RegistryRights.CreateSubKey, AccessControlType.Deny);
				registrySecurity.AddAccessRule(rule);
				key.SetAccessControl(registrySecurity);

				try
				{
					EnterpriseUrlHandlerService.Instance.RegisterInstance();
				}
				finally
				{
					registrySecurity.RemoveAccessRule(rule);
					key.SetAccessControl(registrySecurity);
				}
			}
		}

		#endregion
#endif

		#region UrlHandlers

		public void TestUrlHandlers()
		{
			AssertEquals("Should include ShowEditFormUrlHandler", true, ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ShowEditFormUrlHandler.Instance));
			AssertEquals("Should include ShowNewFormUrlHandler", true, ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ShowNewFormUrlHandler.Instance));
			AssertEquals("Should include ShowViewFormUrlHandler", true, ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ShowViewFormUrlHandler.Instance));
			AssertEquals("Should include ShowDeleteFormUrlHandler", true, ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ShowDeleteFormUrlHandler.Instance));
			AssertEquals("Should include ShowModuleUrlHandler", true, ((IList<UrlHandler>)EnterpriseUrlHandlerService.UrlHandlers).Contains(ShowModuleUrlHandler.Instance));
		}

		#endregion

		#region GetEnterpriseUrlQueryString

		public void TestGetEnterpriseUrlQueryString_LicenceCode()
		{
			Dummy.Factory.Save();

			var enterpriseCode = StaticCurrentFetcher.Instance.CurrentCompany.LicenceEnterpriseCode;
			var serverCode = StaticCurrentFetcher.Instance.CurrentCompany.LicenceServerID;
			var licenceCode = $"{enterpriseCode}XXX{serverCode}";

			var url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, licenceCode);
			AssertNotNull("Matches Enterprise Code and Server Code", EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url));

			var companyCode = StaticCurrentFetcher.Instance.CurrentCompany.GC_Code;
			licenceCode = $"{enterpriseCode}{companyCode}{serverCode}";

			url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, licenceCode);

			AssertNotNull("Exact Match", EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url));

			licenceCode = $"XXX{companyCode}{serverCode}";
			url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, licenceCode);

			AssertExceptionThrown<EnterpriseUrlHandlerException>(
				"Enterprise Code Different",
				() => EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url));

			licenceCode = $"{enterpriseCode}{companyCode}XXX";
			url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, licenceCode);

			AssertExceptionThrown<EnterpriseUrlHandlerException>(
				"Server Code Different",
				() => EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url));
		}

		#endregion

		#region ExecuteUrl

		public void TestGetEnterpriseUrlQueryString_WhenApplicationInstanceNameEmptyAndUrlInstanceNameFull()
		{
			string url;

			using (SetTemporaryInstanceName("instance"))
			{
				url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			}

			using (SetTemporaryInstanceName(null))
			{
				AssertNoExceptionThrown(() => EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(url));
			}
		}

		public void TestExecuteUrlForWindowPersister()
		{
			Dummy.Factory.Save();
			AssertEquals("Form should not be shown initially for the test", false, CheckIfDummyFormOpen());

			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			EnterpriseUrlHandlerService.Instance.ExecuteUrlForWindowPersister(url, false);
			AssertEquals("Form should be opened for edit", true, CheckIfDummyFormOpen());
		}

#if !WINZOR    // Winzor version doesn't support this feature, nor have an interface for it.
		public void TestLicenceKeyIdentifier()
		{
			var actualLicenceKeyIdentifier = StaticCurrentFetcher.Instance.CurrentCompany.LicenceKeyIdentifier;
			AssertEquals(actualLicenceKeyIdentifier, ((IEnterpriseUrlHandlerService)EnterpriseUrlHandlerService.Instance).LicenceKeyIdentifier);
		}

		public void TestLicenceKeyIdentifier_UsesDbSafelyFromAnotherThread()
		{
			var licenceKeyIdentifier1 = "";

			var threadstart1 = new ThreadStart(delegate
			{
				licenceKeyIdentifier1 = ((IEnterpriseUrlHandlerService)new EnterpriseUrlHandlerService()).LicenceKeyIdentifier;
			});

			var thread1 = new Thread(threadstart1);
			thread1.Start();
			thread1.Join(1000);

			var actualLicenceKeyIdentifier = StaticCurrentFetcher.Instance.CurrentCompany.LicenceKeyIdentifier;
			AssertEquals(actualLicenceKeyIdentifier, licenceKeyIdentifier1);
		}
#endif

		public void TestExecuteUrl_ForEmailHyperlink()
		{
			Dummy.Factory.Save();
			AssertEquals("Form should not be shown initially for the test", false, CheckIfDummyFormOpen());

			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace("-", "%2D");
			AssertEquals("Should contain some url encoding for the test", true, url.Contains("%2D"));
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, CheckIfDummyFormOpen());
		}

		public void TestExecuteUrl_ForInternetExplorerHyperlink()
		{
			Dummy.Factory.Save();
			AssertEquals("Form should not be shown initially for the test", false, CheckIfDummyFormOpen());

			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = WebUtility.UrlDecode(url); // IE does this before passing the url to the application!

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, CheckIfDummyFormOpen());
		}

		public void TestCanExecuteUrl_ServerNameDatabaseNameNoCaseSensitive()
		{
			// Arrange
			Dummy.Factory.Save();

			var urlWithUpperServerNameAndDatabaseName = CustomizeUrl(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK), $"ServerName={InstanceDetails.Current.ServerName.ToUpper()}&", $"DatabaseName={InstanceDetails.Current.DatabaseName.ToUpper()}&");
			var urlWithLowerServerNameAndDatabaseName = CustomizeUrl(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK), $"ServerName={InstanceDetails.Current.ServerName.ToLower()}&", $"DatabaseName={InstanceDetails.Current.DatabaseName.ToLower()}&");

			// Act, Assert
			ExecuteUrlAndOpenForm(urlWithUpperServerNameAndDatabaseName); //Upper
			ExecuteUrlAndOpenForm(urlWithLowerServerNameAndDatabaseName); //Lower

			void ExecuteUrlAndOpenForm(string url)
			{
				AssertEquals("Form should not be shown initially for the test", false, CheckIfDummyFormOpen());
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				AssertEquals("Form should be opened for edit", true, CheckIfDummyFormOpen());
			}
		}

#if !WINZOR    // Winzor version does NOT throw an exception.
		public void TestExecuteUrl_WaitForAppToStart()
		{
			using (var dispatcherRemoval = ClearDispatcher())
			{
				Dummy.Factory.Save();
				var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);

				var ex = AssertExceptionThrown<EnterpriseUrlHandlerException>(
					"Expected an exception due to the main form not being available and waitForAppToStart=false",
					() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
				AssertEquals($"{Enterprise.Core.Constants.ProductName} cannot handle your request, it may be opening or closing at this time. Try again later.", ex.Message);

				var thread = new Thread(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, true));
				thread.Start();
				Thread.Sleep(100);
				dispatcherRemoval.Dispose();
				while (!thread.Join(TimeSpan.FromMilliseconds(100)))
				{
					Application.DoEvents();
				}
				AssertEquals("After waiting for the main form to re-appear, the url should be executed", true, CheckIfDummyFormOpen());
			}
		}
#endif

#if !WINZOR    // Winzor version throws a different message and doesn't start.
		public void TestExecuteUrl_WaitForAppToStartWhichNeverHappens()
		{
			using (var dispatcherRemoval = ClearDispatcher())
			{
				Dummy.Factory.Save();
				var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);

				var threadstart = new ThreadStart(delegate
				{
					AssertExceptionThrown<EnterpriseUrlHandlerException>(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, true));
				});

				var thread = new Thread(threadstart);
				thread.Start();
				Assert(thread.Join(150000));
			}
		}
#endif

#if !WINZOR    // Winzor version throws a different message and doesn't start.
		public void TestExecuteUrl_WaitForLogin()
		{
			ExecuteUrl_WaitForLogin(true);
		}

		public void TestExecuteUrl_WaitForTwoFactorLogin()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");
			ExecuteUrl_WaitForLogin(false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void ExecuteUrl_WaitForLogin(bool waitForAppToStart)
		{
			Dummy.Factory.Save();
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			var task = new Task(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, waitForAppToStart));
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.SetUserContext(null);
			task.Start();
			Thread.Sleep(100);
			Application.DoEvents();
			EnvProxy.Instance.SetUserContext(userContext);
			Thread.Sleep(100);
			while (!task.Wait(100))
			{
				Application.DoEvents();
			}
			AssertEquals(true, CheckIfDummyFormOpen());
		}
#endif

		[SnailTest]
		public void TestExecuteUrl_WaitForLoginWhichNeverHappens()
		{
			ExecuteUrl_WaitForLoginWhichNeverHappens(true);
		}

		[SnailTest]
		public void TestExecuteUrl_WaitForTwoFactorLoginWhichNeverHappens()
		{
			SystemDataRegistry.Instance.TwoFactorAuthenticationTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");
			ExecuteUrl_WaitForLoginWhichNeverHappens(false);
		}

		void ExecuteUrl_WaitForLoginWhichNeverHappens(bool waitForAppToStart)
		{
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			using (EnvProxy.Instance.SetTemporaryUserContext(string.Empty, Guid.Empty, Guid.Empty))
			{
				AssertExceptionThrown<EnterpriseUrlHandlerException>(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, waitForAppToStart));
			}
		}

#if !WINZOR    // Winzor version does not throw an exception
		public void TestExecuteUrlThrowsExceptionWhenCannotMatchUserCredentials()
		{
			var handler = EnterpriseUrlHandlerService.Instance;
			var urlAuthenticationKey = Guid.NewGuid().ToString();
			var urlAuthenticationSemaphore = new UrlAuthenticationSemaphore(urlAuthenticationKey);
			var otherUserPk = GetOtherUser().PK;
			var otherUserProvider = (ISemaphoreProvider)new SemaphoreProviderWithMockUserIdForTesting(otherUserPk.ToGuid());
			using (otherUserProvider.CreateSemaphoreHandle(urlAuthenticationSemaphore))
			{
				AssertExceptionThrown<InvalidUrlCredentialsException>(
					"Should throw exception when invalid authentication key",
					() => handler.ExecuteUrl(GetUrl(urlAuthenticationKey: Guid.NewGuid().ToString()), false));

				var ex = AssertExceptionThrown<InvalidUrlCredentialsException>(
					"Should throw exception when cannot match user credentials",
					() => handler.ExecuteUrl(GetUrl(urlAuthenticationKey: urlAuthenticationKey), false));
				AssertEquals("Invalid user credentials.", ex.Message);
				AssertNotEquals("Should not re-login as other user when cannot match user credentials and url authentication requested", otherUserPk, EnvInstance.CurrentUser.PK);
			}

			using (EnvInstance.SemaphoreProvider.CreateSemaphoreHandle(urlAuthenticationSemaphore))
			{
				Assert("Should execute url when user credentials in url match current user", handler.ExecuteUrl(GetUrl(urlAuthenticationKey: urlAuthenticationKey), false));
				Assert("Url should be executed", CheckIfDummyFormOpen());
			}
		}
#endif

#if !WINZOR    // Winzor version throws a different message.
		public void TestExecuteUrlThrowsExceptionWhenCannotMatchLicenceKey()
		{
			Dummy.Factory.Save();

			var url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, "BLABLABLA");
			var ex = AssertExceptionThrown<NotSupportedLicenceKeyException>(
				"Should throw exception when cannot match licence key",
				() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
			AssertEquals("Cannot process url for the licence key specified.", ex.Message);

			url = ShowEditFormUrlHandler.Instance.CreateWithoutApplicationContext(DummyControllerIDs.Dummy, Dummy.PK);
			Assert("Should not throw exception when no licence key specified", EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
			Assert("Url should be executed", CheckIfDummyFormOpen());
		}
#endif

#if !WINZOR    // Winzor version does not have this feature
		public void TestExecuteUrlWhenCannotMatchInstanceName()
		{
			// Arrange
			string url;
			using (SetTemporaryInstanceName("invalid-instance"))
			{
				Dummy.Factory.Save();

				url = CustomizeUrl(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK), string.Empty, String.Empty);
			}

			using (SetTemporaryInstanceName("correct-instance"))
			{
				// Act
				var result = EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);

				// Assert
				AssertEquals("Should return false when cannot match instance name.", false, result);
				Assert("Url should not be executed", !CheckIfDummyFormOpen());
			}
		}
#endif

#if !WINZOR    // Winzor version throws a different message and/or exception.
		[ExpectExceptionMessage(typeof(EnterpriseUrlHandlerException), "User did not log in within 1 minute when trying to execute the url.")]
		public void TestLoggedOut_AccessErrorLogForm_Issue00223736()
		{
			var url = "edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=53bea36d-a4fd-4d39-9061-1e0ebfd4eea5";
			using (EnvProxy.Instance.SetTemporaryUserContext(String.Empty, Guid.Empty, Guid.Empty))
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			}
		}
#endif

#if !WINZOR    // Winzor version throws a different message and/or exception.
		public void TestExecuteUrl_CorrectGlobalMessageShown_UrlAuthentication_NoUserLogin()
		{
			var url = "edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Organisation&BusinessEntityPK=53bea36d-a4fd-4d39-9061-1e0ebfd4eea5";

			using (EnvProxy.Instance.SetTemporaryUserContext(String.Empty, Guid.Empty, Guid.Empty))
			{
				AssertExceptionThrown(typeof(EnterpriseUrlHandlerException), "User did not log in within 1 minute when trying to execute the url.", () => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
			}
		}
#endif

		public void TestCanHandleLegacyUrl()
		{
			// Arrange
			string legacyUrlWithDomainAndInstance;
			string legacyUrlWithoutDomainAndInstance;

			TestCanAccessActiveDirectory();
			TestCannotAccessActiveDirectory();

			void TestCanAccessActiveDirectory()
			{
				// Act,Assert
				using (SetTemporaryInstanceName("instance"))
				{
					legacyUrlWithDomainAndInstance = CustomizeUrl(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK), string.Empty, string.Empty);

					AssertNoExceptionThrown(() => EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(legacyUrlWithDomainAndInstance));
				}
			}

			void TestCannotAccessActiveDirectory()
			{
				// Act
				legacyUrlWithoutDomainAndInstance = CustomizeUrl(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK), string.Empty, string.Empty);

				// Assert
				AssertNoExceptionThrown(() => EnterpriseUrlHandlerService.GetEnterpriseUrlQueryString(legacyUrlWithoutDomainAndInstance));
			}
		}

		string CustomizeUrl(string url, string firstReplacement, string secondReplacement)
		{
			var domainValue = Regex.Match(url, @"ServerName=([^&]+)&").Groups[1].Value;
			var databaseValue = Regex.Match(url, @"DatabaseName=([^&]+)&").Groups[1].Value;

			return url.Replace($"ServerName={domainValue}&", firstReplacement)
				.Replace($"DatabaseName={databaseValue}&", secondReplacement);
		}

		internal static IDisposable ClearDispatcher()
		{
			var dispatcher = ApplicationDispatcher.Current;
			ApplicationDispatcher.Current = null;
			return new DisposableAction(() => ApplicationDispatcher.Current = dispatcher);
		}

#if !WINZOR    // Helper method for testing that are not used by Winzor version.
		IGlbStaff GetOtherUser()
		{
			var staff = Factory.New<IGlbStaff>();
			staff.GS_LoginName = "OtherUser";
			Factory.Save();
			return staff;
		}

		string GetUrl(string branch = null, string department = null, string urlAuthenticationKey = null)
		{
			var builder = new ZStringBuilder(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK));
			builder.AppendIfNotEmpty("&Branch=", branch);
			builder.AppendIfNotEmpty("&Department=", department);
			builder.AppendIfNotEmpty("&UrlAuthenticationKey=", urlAuthenticationKey);
			return builder.ToString();
		}
#endif

		internal static bool CheckIfDummyFormOpen()
		{
			var dummyForm = Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault();
			var isDummyFormOpen = dummyForm != null;
			if (isDummyFormOpen)
			{
				Assert("Dummy form should be open in main form thread", !dummyForm.InvokeRequired && Thread.CurrentThread == ApplicationDispatcher.MainThread);
				dummyForm.Dispose();
			}
			return isDummyFormOpen;
		}

		static IDisposable SetTemporaryInstanceName(string instanceName)
		{
			var searchResultMock = new Mock<ICargoWiseOneInstanceSearchResult>();
			searchResultMock.SetupGet(s => s.Name).Returns(instanceName);

			var cwInstanceClassMock = new Mock<ICargoWiseOneInstanceClass>();
			cwInstanceClassMock.Setup(i => i.ExistsInCurrentSchema()).Returns(true);
			cwInstanceClassMock.Setup(i => i.FindInstanceByDatabase(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DirectorySearcherWrapper>(), It.IsAny<DirectorySearchOptions>())).Returns(searchResultMock.Object);

			var substitute = ObjectFactory.Substitute(cwInstanceClassMock.Object);
			var recalculate = InstanceDetails.ReCalculateForTest();

			return new DisposableAction(() =>
			{
				substitute.Dispose();
				recalculate.Dispose();
			});
		}

		#endregion

		#region CreateAndVerifyQueryString

#if !WINZOR		// Winzor doesn't appear to support this feature.
		public void TestVerifyQueryString_InvalidUrlSecurityHash()
		{
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace(Dummy.PK.ToString(), Guid.NewGuid().ToString());

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Form should not be opened due to an invalid url", false, CheckIfDummyFormOpen());
				AssertEquals($"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", ex.Message);
			}
		}

		public void TestVerifyQueryString_InvalidUrlSecurityHash_HashIsNotAValidBase64String()
		{
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace("Hash=", "Hash=Tampered");

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Form should not be opened due to an invalid url", false, CheckIfDummyFormOpen());
				AssertEquals($"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", ex.Message);
			}
		}
#endif
		#endregion

		#region Implementation

#if !WINZOR        // Helper methods not used by Winzor version of the tests                                                  
		void DeleteEdiLoadLocationRegistryKey(ZString testLicenceKeyIdentifier)
		{
			using (var commandKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes\edient\shell\open\command", true))
			{
				if (commandKey != null)
				{
					if (((IList<string>)commandKey.GetSubKeyNames()).Contains(testLicenceKeyIdentifier))
					{
						commandKey.DeleteSubKey(testLicenceKeyIdentifier);
					}
				}
			}
		}

		IWinFormsEnvironment EnvInstance
		{
			get { return (IWinFormsEnvironment)EnvProxy.Instance; }
		}
#endif

		#endregion
	}
}
