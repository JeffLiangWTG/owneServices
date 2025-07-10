#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace Enterprise.URLHandler.Testing
{
	[UseSnapshotProtection]
	sealed class EnterpriseUrlHandlerClientTest : TestCase
	{
		// Some of these tests must be run through the CW1 test runner instead of through visual studio.
		#region ExecuteUrl

		[GuiTest]
		public void TestExecuteUrl()
		{
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(true, ShowEditFormUrl);
			AssertEquals("No messages should be shown to the user", null, client.LastShownMessage);
			AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
		}

		[GuiTest, TestSemaphoreProvider]
		public void TestExecuteUrl_WhenEnterpriseIsRunningForUserProvidedInUrl()
		{
			var urlAuthenticationKey = Guid.NewGuid().ToString();
			using (EnvInstance.SemaphoreProvider.CreateSemaphoreHandle(new UrlAuthenticationSemaphore(urlAuthenticationKey)))
			{
				var url = ShowEditFormUrl + "&UrlAuthenticationKey=" + urlAuthenticationKey;
				client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(true, url);
				AssertNull("No new process is started", client.LastProcessCreated);
				AssertNull("No messages should be shown to the user", client.LastShownMessage);
				AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
			}
		}

		[GuiTest, TestSemaphoreProvider]
		public void TestExecuteUrl_WhenEnterpriseIsRunningButNotForUserProvidedInUrl()
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var staff = factory.New<IGlbStaff>();
				staff.GS_LoginName = "OtherUser";

				var security1 = factory.New<IGlbSecurity>();
				security1.GU_GS = staff.PK;
				security1.GU_SecurityItemIsAllowed = true;
				security1.GU_SecurityRight = "Login";

				factory.Save();

				var urlAuthenticationKey = Guid.NewGuid().ToString();
				var otherUserProvider = (ISemaphoreProvider)new SemaphoreProviderWithMockUserIdForTesting(staff.PK.ToGuid());
				using (otherUserProvider.CreateSemaphoreHandle(new UrlAuthenticationSemaphore(urlAuthenticationKey)))
				{
					client.OnProcessStarting += () => Assert(
						"Should create url authentication mutex before starting new enterprise when UrlAuthenticationKey provided",
						UrlAuthenticationMutex.IsUrlAuthenticationRequired);

					var url = ShowEditFormUrl + "&Branch=SYD&Department=BRN&UrlAuthenticationKey=" + urlAuthenticationKey;
					AssertStartCW(url, Db.ServerName + " " + Db.DatabaseName + " -Branch:SYD -Department:BRN -ShowLogin", false, false);
					AssertEquals("Should login as other user", staff.PK, EnvInstance.CurrentUser.PK);
					AssertEquals("Should login to provided branch", "SYD", EnvInstance.CurrentBranch.Code);
					AssertEquals("Should login to provided department", "BRN", EnvInstance.CurrentDepartment.Code);
					Assert("Should release url authentication mutex after started new enterprise", !UrlAuthenticationMutex.IsUrlAuthenticationRequired);
				}
			}
			finally
			{
				EnvInstance.LoginController.Logout();
			}
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEnterpriseIsRunningButNotForLicenceKeyProvidedInUrl()
		{
			try
			{
				URLHandlerServiceRegistration.RegisterInstance(
					OtherLicenseKeyIdentifer,
					EnvInstance.CurrentBranch.Code,
					Db.ServerName + " " + Db.DatabaseName,
					ApplicationType.CargoWiseRDP, null, null, null);

				var url = ShowEditFormUrlHandler.Instance.CreateWithSpecifiedLicenceCode(
					DummyControllerIDs.Dummy,
					GetDummy().PK,
					OtherLicenseKeyIdentifer);
				url = url.Substring(0, url.IndexOf("&Hash", StringComparison.InvariantCulture));

				AssertStartCW(url, Db.ServerName + " " + Db.DatabaseName + " -Branch:BNE -ShowLogin", false, false);
			}
			finally
			{
				Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(@"EdiEnterprise.edient\shell\open\command\" + OtherLicenseKeyIdentifer);
			}
		}

		[GuiTest]
		public void TestExecuteUrlOnPublishedEnterpriseProcess_ShouldNotLookForOtherHandlerEvenIfNotHandled()
		{
			service.ExecuteUrlOverride = () => false;
			client.PublishedEnterpriseFound = true;
			ExecuteUrlOnMultipleHandlers();
			AssertNull("Published url handler found, no new process is started", client.LastProcessCreated);
			AssertNull("No messages should be shown to the user", client.LastShownMessage);
			AssertEquals("Should NOT go through all handlers as published found", service.UrlsExecuted.Count, 0);
		}

		[GuiTest]
		public void TestExecuteUrlOnAnyRunningEnterpriseProcess_ShouldKeepLookingForGoodHandlerWhenHandlerWhichNotAbleToHandleUrlIsFound()
		{
			var executeUrlCalls = 0;
			service.ExecuteUrlOverride = () =>
			{
				executeUrlCalls++;
				switch (executeUrlCalls)
				{
					case 1:
						throw new EnterpriseUrlHandlerException(string.Empty);
					case 2:
						throw new RDPRemoteException(new EnterpriseUrlHandlerException(string.Empty));
					case 3:
						return false;
					case 4:
						return true;
				}
				return true;
			};

			ExecuteUrlOnMultipleHandlers(4);
			AssertNull("Good url handler found, no new process is started", client.LastProcessCreated);
			AssertNull("No messages should be shown to the user", client.LastShownMessage);
			AssertEquals("Should go through all handlers in order to find the good one", executeUrlCalls, 4);
		}

		[GuiTest]
		public void TestExecuteUrlOnAnyRunningEnterpriseProcess_ShouldIgnoreAllExceptionsIfGoodHandlerIsFoundEvenIfReturnsFalse()
		{
			var executeUrlCalls = 0;
			service.ExecuteUrlOverride = () =>
			{
				executeUrlCalls++;
				switch (executeUrlCalls)
				{
					case 1:
						throw new NotSupportedLicenceKeyException(string.Empty);
					case 2:
						throw new RDPRemoteException(new NotSupportedLicenceKeyException(string.Empty));
					case 4:
						throw new EnterpriseUrlHandlerException(CannotHandleUrlExceptionMessage);
				}
				return false;
			};

			ExecuteUrlOnMultipleHandlers(4);
			AssertNull("No new process is started", client.LastProcessCreated);
			AssertEquals("Good url handler found but it cannot handle url provided", UnknownUrlRequestType, client.LastShownMessage);
			AssertEquals("Should go through all handlers in attempt to find better one", executeUrlCalls, 4);
		}

		[GuiTest]
		public void TestExecuteUrlOnAnyRunningEnterpriseProcess_AuthenticationExceptionsShouldBeIgnoredIfOtherHandlerFound()
		{
			var executeUrlCalls = 0;
			service.ExecuteUrlOverride = () =>
			{
				executeUrlCalls++;
				switch (executeUrlCalls)
				{
					case 1:
						throw new NotSupportedLicenceKeyException(string.Empty);
					case 2:
						throw new EnterpriseUrlHandlerException(CannotHandleUrlExceptionMessage);
					case 3:
						throw new InvalidUrlCredentialsException(string.Empty);
				}
				return true;
			};

			ExecuteUrlOnMultipleHandlers();
			AssertNull("No new process is started", client.LastProcessCreated);
			AssertEquals("Should ignore handlers running for other licence or user but should report error from found one", CannotHandleUrlExceptionMessage, client.LastShownMessage);
			AssertEquals("Should go through all handlers in order to find good one", executeUrlCalls, 3);
		}

		[GuiTest]
		public void TestExecuteUrlOnAnyRunningEnterpriseProcess_ShouldReportLastUrlHandlerExceptionIfNoGoodHandlerFound()
		{
			var executeUrlCalls = 0;
			service.ExecuteUrlOverride = () =>
			{
				executeUrlCalls++;
				switch (executeUrlCalls)
				{
					case 1:
						throw new EnterpriseUrlHandlerException("First exception");
					case 2:
						throw new EnterpriseUrlHandlerException(CannotHandleUrlExceptionMessage);
					case 3:
						throw new InvalidUrlCredentialsException(string.Empty);
				}
				return true;
			};

			ExecuteUrlOnMultipleHandlers();
			AssertNull("No new process is started", client.LastProcessCreated);
			AssertEquals("Should report last url handler error ignoring authentication exceptions", CannotHandleUrlExceptionMessage, client.LastShownMessage);
			AssertEquals("Should go through all handler in order to find good one", executeUrlCalls, 3);
		}

		[GuiTest]
		public void TestExecuteUrl_WhenExceptionErrorRaised()
		{
			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, ZGuid.NewZGuid());
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(true, url);
			AssertEquals($"The system has searched all open {BrandingFactory.Instance.ProductName} programs and could not find the record", client.LastShownMessage);
		}

		[GuiTest]
		public void TestExecuteUrl_WithInvalidUrl()
		{
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(true, "Invalid");
			AssertEquals(UnknownUrlRequestType, client.LastShownMessage);
		}

		[GuiTest]
		public void TestExecuteUrl_GlowIntegration()
		{
			var integrationKey = Guid.NewGuid().ToString();
			var eventName = "EnterpriseUrlHandled:" + integrationKey;
			using (EnvInstance.SemaphoreProvider.CreateSemaphoreHandle(new UrlAuthenticationSemaphore(integrationKey)))
			using (var urlHandledEvent = new EventWaitHandle(false, EventResetMode.ManualReset, eventName))
			{
				client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(true, ShowEditFormUrl + "&UrlAuthenticationKey=" + integrationKey);

				AssertEquals("EnterpriseUrlHandled event raised", true, urlHandledEvent.WaitOne());
				AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
			}
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEdiEnterpriseNotRunning_NoInstance()
		{
			Microsoft.Win32.Registry.CurrentUser.DeleteSubKey(@"SOFTWARE\Classes\EdiEnterprise.edient\shell\open\command\EDIEDIDAT", false);

			client.SimulateEdiEnterpriseNotRunning = true;
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, ShowEditFormUrl);

			AssertEquals(
				@"This application is either not running or is not running in the correct company or
licensed server installation directory. Please restart and try again.

If you are running the application on a Terminal Server or Citrix client,
you must open the shortcut or hyperlink from within the remote server.",
				client.LastShownMessage);
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEdiEnterpriseNotRunning_CargoWiseOneExeNotFound()
		{
			client.SimulateEdiEnterpriseNotRunning = true;
			client.ProcessStartExceptionToThrow = new FileNotFoundException();

			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, ShowEditFormUrl);
			AssertEquals(true, client.LastShownMessage.StartsWith("This application is either not running"));
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEdiEnterpriseNotRunning_CargoWiseOneExeCouldNotBeStarted()
		{
			client.SimulateEdiEnterpriseNotRunning = true;
			client.ProcessStartExceptionToThrow = new InvalidOperationException();

			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, ShowEditFormUrl);
			AssertEquals(true, client.LastShownMessage.StartsWith("This application is either not running"));
		}

		[GuiTest]
		public void TestExecuteMultipleUrls_WhenEdiEnterpriseNotRunning()
		{
			var urls = new[] { ShowEditFormUrl, ShowEditFormUrl };

			client.SimulateEdiEnterpriseNotRunning = true;
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, urls);

			AssertNotNull("New Enterprise process created", client.LastProcessCreated);
			AssertEquals("New process created", @"C:\Program Files\WiseTech Global\CargoWise\CargoWise.Start.exe", client.LastProcessCreatedFileName);
			AssertEquals("process arguments", Db.ServerName + " " + Db.DatabaseName + " -Branch:BNE -ShowLogin", client.LastProcessCreatedArguments);
			AssertNull("No messages should be shown to the user", client.LastShownMessage);
			AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
			Assert("Should execute all urls", urls.SequenceEqual(service.UrlsExecuted));
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEdiEnterpriseNotRunning_SpawnNewEnterpriseProcess()
		{
			client.OnProcessStarting += () => Assert(
				"Should NOT create url authentication mutex if no UrlAuthenticationKey provided",
				!UrlAuthenticationMutex.IsUrlAuthenticationRequired);
			AssertStartCW(ShowEditFormUrl, Db.ServerName + " " + Db.DatabaseName + " -Branch:BNE -ShowLogin");
		}

		[GuiTest]
		public void TestExecuteUrl_WhenEdiEnterpriseNotRunning_ShouldOverrideBranchAndDepartmentIfProvidedInUrl()
		{
			AssertStartCW(ShowEditFormUrl + "&Branch=TSB&Department=TSD", Db.ServerName + " " + Db.DatabaseName + " -Branch:TSB -Department:TSD -ShowLogin");
		}

		[TestRequiresAdministrativePrivileges("Modifies system registry")]
		[GuiTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestExecuteUrlUsingWCA()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var directoryName = tempDirectory.DirectoryName;
				File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "MockProgram.exe"), Path.Combine(directoryName, "WiseCloudClient.exe"));
				var domainName = Guid.NewGuid().ToString();
				var instance = Guid.NewGuid().ToString();

				using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient\" + domainName))
				{
					wcaClientReg.SetValue("Path", Path.Combine(directoryName, "WiseCloudClient.exe"));
				}
				try
				{
					using (SetTemporaryInstanceName(instance))
					using (var watcher = new FileSystemWatcher(directoryName, "run"))
					{
						watcher.EnableRaisingEvents = true;
						watcher.Changed += (object sender, FileSystemEventArgs e) =>
							{
								Process.GetProcesses().Single(p => IsProcess(p, Path.Combine(directoryName, "WiseCloudClient.exe"))).Kill();
								client.SimulateEdiEnterpriseNotRunning = false;
							};

						var url = ReplaceDomainInUrl(ShowEditFormUrl, domainName);
						client.SimulateEdiEnterpriseNotRunning = true;
						client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, url);

						AssertEquals(string.Join("\r\n", new[] { directoryName + "\\", Path.Combine(directoryName, "WiseCloudClient.exe"), "run", "RDPV1", "CW", instance }) + "\r\n", File.ReadAllText(Path.Combine(directoryName, "run")));
						AssertNull("No messages should be shown to the user", client.LastShownMessage);
						AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
					}
				}
				finally
				{
					using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
					using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient"))
					{
						wcaClientReg.DeleteSubKey(domainName);
					}
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Modifies system registry")]
		[GuiTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestExecuteUrlUsingWCAWithoutLicenseCode()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var directoryName = tempDirectory.DirectoryName;
				File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "MockProgram.exe"), Path.Combine(directoryName, "WiseCloudClient.exe"));
				var domainName = Guid.NewGuid().ToString();
				var instance = Guid.NewGuid().ToString();

				using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient\" + domainName))
				{
					wcaClientReg.SetValue("Path", Path.Combine(directoryName, "WiseCloudClient.exe"));
				}
				try
				{
					using (SetTemporaryInstanceName(instance))
					using (var watcher = new FileSystemWatcher(directoryName, "run"))
					{
						watcher.EnableRaisingEvents = true;
						watcher.Changed += (object sender, FileSystemEventArgs e) =>
						{
							Process.GetProcesses().Single(p => IsProcess(p, Path.Combine(directoryName, "WiseCloudClient.exe"))).Kill();
							client.SimulateEdiEnterpriseNotRunning = false;
						};

						var url = ReplaceDomainInUrl(ShowEditFormUrlHandler.Instance.CreateWithoutApplicationContext(DummyControllerIDs.Dummy, GetDummy().PK), domainName);
						client.SimulateEdiEnterpriseNotRunning = true;
						client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, url);

						AssertEquals(string.Join("\r\n", new[] { directoryName + "\\", Path.Combine(directoryName, "WiseCloudClient.exe"), "run", "RDPV1", "CW", instance }) + "\r\n", File.ReadAllText(Path.Combine(directoryName, "run")));
						AssertNull("No messages should be shown to the user", client.LastShownMessage);
						AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
					}
				}
				finally
				{
					using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
					using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient"))
					{
						wcaClientReg.DeleteSubKey(domainName);
					}
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Modifies system registry")]
		[GuiTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Testing")]
		public void TestExecuteUrlUsingWCA_AfterOpenInstanceRaisesException()
		{
			using (var tempDirectory = new TempDirectory())
			{
				var directoryName = tempDirectory.DirectoryName;
				File.Copy(Path.Combine(AssemblyLoader.GetBinPath(), "MockProgram.exe"), Path.Combine(directoryName, "WiseCloudClient.exe"));
				var domainName = Guid.NewGuid().ToString();
				var instance = Guid.NewGuid().ToString();

				using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient\" + domainName))
				{
					wcaClientReg.SetValue("Path", Path.Combine(directoryName, "WiseCloudClient.exe"));
				}
				try
				{
					var dummy = new BusinessObjectFactory() { RefreshEnabled = false }.New<DummyBusinessObject>();

					using (SetTemporaryInstanceName(instance))
					using (var watcher = new FileSystemWatcher(directoryName, "run"))
					{
						watcher.EnableRaisingEvents = true;
						watcher.Changed += (object sender, FileSystemEventArgs e) =>
						{
							Process.GetProcesses().Single(p => IsProcess(p, Path.Combine(directoryName, "WiseCloudClient.exe"))).Kill();
							ApplicationDispatcher.Current.Invoke(dummy.Factory.Save);
						};

						var url = ReplaceDomainInUrl(ShowEditFormUrlHandler.Instance.CreateWithoutApplicationContext(DummyControllerIDs.Dummy, dummy.PK), domainName);
						client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, url);

						AssertEquals(string.Join("\r\n", new[] { directoryName + "\\", Path.Combine(directoryName, "WiseCloudClient.exe"), "run", "RDPV1", "CW", instance }) + "\r\n", File.ReadAllText(Path.Combine(directoryName, "run")));
						AssertNull("No messages should be shown to the user", client.LastShownMessage);
						AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
					}
				}
				finally
				{
					using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
					using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient"))
					{
						wcaClientReg.DeleteSubKey(domainName);
					}
				}
			}
		}

		static string ReplaceDomainInUrl(string url, string domainName)
		{
			return url.Replace($"&Domain={Domain.GetCurrentDomain()}", $"&Domain={domainName}");
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

		bool IsProcess(Process p, string path)
		{
			try
			{
				return p != null && p.MainModule.FileName == path;
			}
			catch (Win32Exception)
			{
				return false;
			}
		}

		void AssertStartCW(string url, string expectedArguments, bool simulateEdiEnterpriseNotRunning = true, bool isRunningTests = false)
		{
			client.SimulateEdiEnterpriseNotRunning = simulateEdiEnterpriseNotRunning;
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(isRunningTests, url);

			AssertNotNull("New CargoWise.Start.exe process created", client.LastProcessCreated);
			AssertEquals("New process created", @"C:\Program Files\WiseTech Global\CargoWise\CargoWise.Start.exe", client.LastProcessCreatedFileName);
			AssertEquals("CargoWise.Start.exe process arguments", expectedArguments, client.LastProcessCreatedArguments);
			AssertNull("No messages should be shown to the user", client.LastShownMessage);
			AssertEquals("Form opened for edit", true, CheckIfDummyFormOpen());
			Assert($"{service.UrlsExecuted.Aggregate((x, y) => x + ", " + y)} should contain {url}", service.UrlsExecuted.Contains(url));
		}

		static bool CheckIfDummyFormOpen()
		{
			var openDummyForm = Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault();
			var isDummyFormOpen = openDummyForm != null;
			if (isDummyFormOpen)
			{
				openDummyForm.Invoke((MethodInvoker)delegate ()
				{ openDummyForm.Close(); });
			}
			return isDummyFormOpen;
		}

		#endregion

		#region Test Classes

		class TestEnterpriseUrlHandlerClient : EnterpriseUrlHandlerClient
		{
			// To prevent deadlock because both the test and the remote server require execution on the UI thread.
			public void ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(bool isRunningTests, params string[] urls)
			{
				try
				{
					if (isRunningTests)
					{
						TestingState.Setup(); //new AppDomain, so we have to set up IsRunningTests
					}

					var completedList = new bool[urls.Length];
					Exception ex = null;

					for (var i = 0; i < urls.Length; i++)
					{
						var index = i;
						ThreadPool.QueueUserWorkItem(delegate
						{
							try
							{
								ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew(urls[index]);
							}
							catch (Exception unexpectedException) when (!unexpectedException.IsCriticalException())
							{
								ex = unexpectedException;
							}

							completedList[index] = true;
						});
					}

					while (completedList.Any(completed => !completed && ex == null))
					{
						Application.DoEvents();
						Thread.Sleep(1);
					}

					if (ex != null)
					{
						Assert(string.Format("An unexpected exception was thrown: {0}", ex.Message), false);
					}
				}
				finally
				{
					if (isRunningTests)
					{
						TestingState.TearDown();
					}
				}
			}

			#region Handlers / GetUrlHandlerService

			public bool SimulateEdiEnterpriseNotRunning;

			internal IEnumerable<IEnterpriseUrlHandlerService> HandlersOverride { private get; set; }

			protected override IEnumerable<IEnterpriseUrlHandlerService> Handlers
			{
				get { return HandlersOverride ?? base.Handlers; }
			}

			public override IEnterpriseUrlHandlerService GetUrlHandlerService(int processId)
			{
				IEnterpriseUrlHandlerService result;

				if (LastProcessCreated != null && processId == LastProcessCreated.Id)
				{
					result = handlerServiceForLastProcessCreated;
				}
				else
				{
					result = base.GetUrlHandlerService(!SimulateEdiEnterpriseNotRunning ? processId : 0);
				}
				return result;
			}

			internal void SetHandlerServiceForLastProcessCreated(IEnterpriseUrlHandlerService service)
			{
				handlerServiceForLastProcessCreated = service;
			}

			IEnterpriseUrlHandlerService handlerServiceForLastProcessCreated;

			#endregion

			#region Process.Start

			public Process LastProcessCreated { get; set; }
			public Exception ProcessStartExceptionToThrow { private get; set; }

			public string LastProcessCreatedFileName { get; private set; }
			public string LastProcessCreatedArguments { get; private set; }
			public event Action OnProcessStarting;

			protected override Process ProcessStart(string fileName, string arguments = null)
			{
				lock (this)
				{
					if (OnProcessStarting != null)
					{
						OnProcessStarting();
					}

					if (ProcessStartExceptionToThrow != null)
					{
						throw ProcessStartExceptionToThrow;
					}

					if (LastProcessCreated != null)
					{
						throw new InvalidOperationException("Should not start multiple processes");
					}

					LastProcessCreatedFileName = fileName;
					LastProcessCreatedArguments = arguments;
					LastProcessCreated = new Process { StartInfo = new ProcessStartInfo("notepad.exe") };
					LastProcessCreated.Start();
					Thread.Sleep(3000);
					LastProcessCreated = ProcessLocator.Instance.GetCurrentUserVisibleProcessesByName("Notepad")[0];
					_ = Task.Run(async () =>
					{
						using (var namedPipeServerStream = new NamedPipeServerStream("edientUrlHandlerService_" + LastProcessCreated.Id, PipeDirection.InOut))
						{
							await namedPipeServerStream.WaitForConnectionAsync();
						}
					});
				}

				return LastProcessCreated;
			}

			#endregion

			#region ShowMessage

			public string LastShownMessage;

			protected override void ShowMessage(string text)
			{
				LastShownMessage = text;
			}

			#endregion

			protected override bool ExecuteUrlOnPublishedEnterpriseProcess(string licenceKeyIdentifier, string url, out bool ediEnterpriseFound)
			{
				ediEnterpriseFound = PublishedEnterpriseFound;
				return false;
			}

			internal bool PublishedEnterpriseFound { private get; set; }
		}

		class EnterpriseUrlHandlerServiceForTest : MarshalByRefObject, IEnterpriseUrlHandlerService
		{
			internal EnterpriseUrlHandlerServiceForTest(IWinFormsEnvironment env)
			{
				this.env = env;
				UrlsExecuted = new List<string>();
			}

			public bool ExecuteUrl(string url, bool wait)
			{
				using (Db.DisposableActionForDbConnection())
				{
					UrlsExecuted.Add(url);

					if (ExecuteUrlOverride != null)
					{
						return ExecuteUrlOverride();
					}

					Assert("Should wait for enterprise to start when executing url first time on the new instance", wait || UrlsExecuted.Count > 1);

					var urlAuthenticationKey = new QueryString(url)["UrlAuthenticationKey"];
					if (!string.IsNullOrWhiteSpace(urlAuthenticationKey))
					{
						env.SetTemporaryUserContext(string.Empty, Guid.Empty, Guid.Empty);
						ApplicationDispatcher.Current.BeginInvoke(new Action(() =>
							{
								var loginAction = UrlHandlerServiceUtils.GetLoginAction();
								loginAction();
							}));
					}

					return EnterpriseUrlHandlerService.Instance.ExecuteUrl(url.Replace(OtherLicenseKeyIdentifer, LicenceKeyIdentifier), wait);
				}
			}

			public string LicenceKeyIdentifier
			{
				get { return "EDIEDIDAT"; }
			}

#if NET8_0_OR_GREATER
			[Obsolete("This Remoting API is not supported and throws PlatformNotSupportedException.")]
#endif
			public override object InitializeLifetimeService()
			{
				return null;
			}

			internal Func<bool> ExecuteUrlOverride { private get; set; }
			internal List<string> UrlsExecuted { get; private set; }
			readonly IWinFormsEnvironment env;
		}

		const string OtherLicenseKeyIdentifer = "BLABLABLA";

		#endregion

		#region Implementation

		void ExecuteUrlOnMultipleHandlers(int numberOfHandlers = 3)
		{
			client.HandlersOverride = Enumerable.Repeat(service, numberOfHandlers).ToArray();
			client.ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew_OnOtherThread(false, ShowEditFormUrl);
		}

		string ShowEditFormUrl
		{
			get { return showEditFormUrl ?? (showEditFormUrl = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, GetDummy().PK)); }
		}

		static DummyBusinessObject GetDummy()
		{
			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			dummy.Factory.Save();
			AssertEquals("Form not shown initially for the test", false, CheckIfDummyFormOpen());
			return dummy;
		}

		string showEditFormUrl;

		static IWinFormsEnvironment EnvInstance
		{
			get { return (IWinFormsEnvironment)EnvProxy.Instance; }
		}

		protected override void SetUp()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			base.SetUp();

			initialUserContext = EnvInstance.CurrentUserContext;
			EnterpriseUrlHandlerService.Instance.RegisterInstance();

			appDomain = AppDomain.CreateDomain("EnterpriseUrlHandlerClientTest");
			appDomain.DoCallBack(delegate
			{
				Db.InitializeDatabaseDetails("WeDontKnowWhatServerToConnectToInThisContext", "WeDontKnowWhatDatabaseToConnectToInThisContext");
			});

			service = new EnterpriseUrlHandlerServiceForTest(EnvInstance);

			client = (TestEnterpriseUrlHandlerClient)appDomain.CreateInstanceAndUnwrap(typeof(TestEnterpriseUrlHandlerClient).Assembly.FullName, typeof(TestEnterpriseUrlHandlerClient).FullName);
			client.SetHandlerServiceForLastProcessCreated(service);

			TaskTestListener.Instance.ExpectTask("await pipeService.WaitForConnectionAsync in URLHandlerServiceRegistration");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		protected override void TearDown()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var lastProcessCreated = client.LastProcessCreated;
			if (lastProcessCreated != null)
			{
				using (var pipeClient = new NamedPipeClientStream("edientUrlHandlerService_" + lastProcessCreated.Id))
				{
					pipeClient.ConnectAsync();
				}

				lastProcessCreated.Kill();
			}

			AppDomain.Unload(appDomain);

			EnvInstance.SetUserContext(initialUserContext);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		AppDomain appDomain;
		// EnterpriseUrlHandlerClient can be used before main initialization code, test without any initialization using a seperate app domain

		EnterpriseUrlHandlerServiceForTest service;
		TestEnterpriseUrlHandlerClient client;
		IUserContext initialUserContext;
		const string CannotHandleUrlExceptionMessage = "Cannot handle url";
		const string UnknownUrlRequestType = "Unable to open form. Please check that CargoWise is running and that the URL is correct.";

		#endregion
	}
}
#endif
