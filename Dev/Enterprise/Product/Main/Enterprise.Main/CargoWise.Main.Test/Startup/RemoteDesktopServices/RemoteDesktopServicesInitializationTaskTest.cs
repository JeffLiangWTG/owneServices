using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

#if NETFRAMEWORK
using System.Threading.Tasks;
#endif

namespace Enterprise.Startup.RemoteDesktopServices.Testing
{
	sealed class RemoteDesktopServicesInitializationTaskTest : AbstractApplicationStartupTaskTest<RemoteDesktopServicesInitializationTask>
	{
#if NETFRAMEWORK // Should be fixed in WI00669071: Remove usages of AppDomains
		[ExpectNoExceptions]
		public void TestExecute()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			appDomain.DoCallBack(() =>
			{
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
					new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, ClientVersion.Version.ToString());
				var task = new RemoteDesktopServicesInitializationTask();
				AssertEquals(true, task.ShouldExecute(new ApplicationArguments(Array.Empty<string>())));
				AssertEquals(true, task.Execute(new ApplicationArguments(Array.Empty<string>())));
				AssertBasicRemoteDesktopServicesInitialized("initialize channel");
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[ExpectNoExceptions]
		public void TestExecuteWithRegistrySettings()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			appDomain.DoCallBack(() =>
			{
				try
				{
					ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
					EnvProxy.Instance.Registry.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ServerOnly;
					InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
						new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, ClientVersion.Version.ToString());
					var task = new RemoteDesktopServicesInitializationTask();
					AssertEquals(true, task.ShouldExecute(new ApplicationArguments(Array.Empty<string>())));
					AssertEquals(true, task.Execute(new ApplicationArguments(Array.Empty<string>())));
					Assert("Channel initialization should be skipped.", !RemoteDesktopServicesInitializationTask.ChannelInitialized);
				}
				finally
				{
					EnvProxy.Instance.Registry.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOrServer;
				}
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[ExpectNoExceptions]
		public void TestClientNotInstalled()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			appDomain.DoCallBack(() =>
			{
				try
				{
					ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
					EnvProxy.Instance.Registry.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOrServer;
					var task = new RemoteDesktopServicesInitializationTask();
					AssertEquals(true, task.Execute(new ApplicationArguments(Array.Empty<string>())));
					Assert("No error message showed if eDocs Access is allowed when RDS not installed.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
					EnvProxy.Instance.Registry.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOnly;
					AssertEquals(true, task.Execute(new ApplicationArguments(Array.Empty<string>())));
					Assert("An error message showed if eDocs Access is not allowed when RDS not installed.", UnitTestUserNotification.Instance.LastMessage.Text == ZTerminalService.ClientPluginApplicationNotInstalledWarning);
				}
				finally
				{
					EnvProxy.Instance.Registry.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOrServer;
					EnterpriseChannel.Instance.Close();
				}
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		[ExpectNoExceptions]
		public void TestClientOutOfDateAndCancel()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			appDomain.DoCallBack(() =>
			{
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
					new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, new Version(1, 0, 0, 0).ToString());
				var task = new RemoteDesktopServicesInitializationTask();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals(true, task.Execute(new ApplicationArguments(Array.Empty<string>())));
				AssertNotNull(EnterpriseChannel.Instance);
				Assert(!EnterpriseChannel.Instance.IsConnected);
				Assert(InitializationMessageHandler.RegisteredRemoteMessageTypes.Length == 0);
				Assert(EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal.Length == 0);
				AssertEquals("A new version of CargoWise Remote Desktop Services is available and will be installed now.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			});
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}
#endif

		[GuiTest]
		public void TestInitCheck_LicenceLogin()
		{
			var originalMainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = null;

				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
					new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, ClientVersion.Version.ToString());

				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				new RemoteDesktopServicesInitializationTask().Execute(new ApplicationArguments(Array.Empty<string>()));
				Assert("Pre", !Env.Licence.RemoteDesktopServices.IsLoggedIn);
				new RemoteDesktopServicesPostLoginTask().Execute();
				AssertAllRemoteDesktopServicesInitialized();
				Assert("Login", Env.Licence.RemoteDesktopServices.IsLoggedIn);
			}
			finally
			{
				Env.Licence.RemoteDesktopServices.ForceLogout();
				StartupOpenMainFormTask.MainFormInstance = originalMainForm;
			}
		}

		public void TestReconnectionInitializeEnterpriseChannel()
		{
			EnterpriseChannel enterpriseChannel1 = null;
			EnterpriseChannel enterpriseChannel2 = null;

			try
			{
				var task = new RemoteDesktopServicesInitializationTask();
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, new Version(1, 0, 0, 0).ToString());
				var executed = task.Execute(new ApplicationArguments(Array.Empty<string>()));
				enterpriseChannel1 = EnterpriseChannel.Instance;

				AssertEquals(true, executed);

				executed = task.Execute(new ApplicationArguments(new[] { "-Reconnect" }));
				enterpriseChannel2 = EnterpriseChannel.Instance;

				AssertEquals(false, executed);
				AssertNotEquals(enterpriseChannel1, enterpriseChannel2);

				AssertEquals(1, enterpriseChannel1.ConnectedCount);
				AssertEquals(0, enterpriseChannel2.ConnectedCount);
			}
			finally
			{
				enterpriseChannel1?.Close();
				enterpriseChannel2?.Close();
			}
		}

		public void TestInitializeCitrixChannel()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			try
			{
				var task = new RemoteDesktopServicesInitializationTask();

				DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOrServer;
				Assert("Citrix Channel Initialized Without Plugin And ANY", task.InitializeChannel(true));
				Assert(!RemoteDesktopServicesInitializationTask.ChannelInitialized);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOnly;
				Assert("Citrix Channel Initialized Without Plugin And CONN", task.InitializeChannel(true));
				Assert(!RemoteDesktopServicesInitializationTask.ChannelInitialized);
				AssertEquals(ZTerminalService.ClientPluginApplicationNotInstalledWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
					new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, new Version(1, 0, 0, 0).ToString());
				Assert("Citrix Channel Initialized With Previous Plugin", task.InitializeChannel(true));
				Assert(!RemoteDesktopServicesInitializationTask.ChannelInitialized);
				AssertEquals(ZTerminalService.ClientPluginApplicationNotInstalledWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
					new[] { EnterpriseChannelMessageTypes.SaveFileDialog }, ClientVersion.Version.ToString());
				Assert("Citrix Channel Initialized With Latest Plugin", task.InitializeChannel(true));
				Assert(RemoteDesktopServicesInitializationTask.ChannelInitialized);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Second Citrix Channel Initialized Without Message", task.InitializeChannel(true));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				EnterpriseChannel.Instance?.Close();
				UnitTestUserNotification.Instance.ClearMessages();
				InitializationMessageHandler.RemoteInitializationMessage = null;
			}
		}

		[GuiTest]
		public void TestInitializeChannelStopRunningWhenUpgraderStartedOnClient()
		{
			var task = new RemoteDesktopServicesInitializationTaskForUpgradeTest();
			UnitTestUserNotification.Instance.ClearMessages();
			var nonZeroPtr = IntPtr.Add(IntPtr.Zero, 1);
			InitializationMessageHandler.InitializationCompleted.Set();

			var asyncResult = new Mock<IAsyncResult>();
			asyncResult
				.Setup(x => x.AsyncWaitHandle)
				.Returns(new ManualResetEvent(true));

			var mockStream = new Mock<Stream>();
			mockStream
				.Setup(x => x.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
				.Returns(asyncResult.Object);

			mockStream
				.Setup(x => x.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
				.Returns(asyncResult.Object);

			mockStream
				.Setup(x => x.EndRead(It.IsAny<IAsyncResult>()))
				.Returns(0);

			var mockApi = new Mock<IWtsApi>();
			mockApi
				.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(nonZeroPtr);
			mockApi
				.Setup(x => x.VirtualChannelGetStream(It.IsAny<IntPtr>()))
				.Returns(mockStream.Object);

			var mockRemoteFile = new Mock<IRemoteFile>();
			mockRemoteFile
				.Setup(x => x.Open())
				.Returns(true);

			var originalWtsApi = WtsApi.Instance;
			WtsApi.Instance = mockApi.Object;

			try
			{
				// Arrange
				using (new DisposableAction(
					() =>
					{
						EnvProxy.Instance.Registry.RemoteAppSendTestingMessageOnInitialize = false;
					},
					() =>
					{
						EnterpriseChannel.Instance.OnDisconnect();
						WtsApi.Instance = originalWtsApi;
						EnvProxy.Instance.Registry.RemoteAppSendTestingMessageOnInitialize = true;
					}))
				using (ObjectFactory.Substitute(mockRemoteFile.Object))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
						new[] {
							EnterpriseChannelMessageTypes.SaveFileDialog,
							EnterpriseChannelMessageTypes.OpenFile,
							EnterpriseChannelMessageTypes.OpenFileSupported
						}, task.GetExecutionResultCallbackVersionExposed().ToString());

					// Act
					Assert("InitializeChannel(...) should return false if upgrader starts successfully", !task.InitializeChannel(false));

					// Assert
					AssertEquals(0, EnterpriseChannel.Instance.ConnectedCount);
					AssertEquals(false, RemoteDesktopServicesInitializationTask.ChannelInitialized);
					AssertErrorsReportedCausedByWtsApiNotSetUpWellAreExpected3();
				}
			}
			finally
			{
				EnterpriseChannel.Instance?.Close();
				UnitTestUserNotification.Instance.ClearMessages();
				InitializationMessageHandler.RemoteInitializationMessage = null;
			}
		}

		[GuiTest]
		public void TestInitializeChannelKeepRunningWhenUpgraderFailedToOpenOnClient()
		{
			var task = new RemoteDesktopServicesInitializationTaskForUpgradeTest();
			UnitTestUserNotification.Instance.ClearMessages();
			var nonZeroPtr = IntPtr.Add(IntPtr.Zero, 1);
			InitializationMessageHandler.InitializationCompleted.Set();

			var asyncResult = new Mock<IAsyncResult>();
			asyncResult
				.Setup(x => x.AsyncWaitHandle)
				.Returns(new ManualResetEvent(true));

			var mockStream = new Mock<Stream>();
			mockStream
				.Setup(x => x.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
				.Returns(asyncResult.Object);

			mockStream
				.Setup(x => x.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
				.Returns(asyncResult.Object);

			mockStream
				.Setup(x => x.EndRead(It.IsAny<IAsyncResult>()))
				.Returns(0);

			var mockApi = new Mock<IWtsApi>();
			mockApi
				.Setup(x => x.VirtualChannelOpen(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(nonZeroPtr);
			mockApi
				.Setup(x => x.VirtualChannelGetStream(It.IsAny<IntPtr>()))
				.Returns(mockStream.Object);

			var mockRemoteFile = new Mock<IRemoteFile>();
			mockRemoteFile
				.Setup(x => x.Open())
				.Returns(false);

			var originalWtsApi = WtsApi.Instance;
			WtsApi.Instance = mockApi.Object;
			try
			{
				// Arrange
				using (new DisposableAction(
					() =>
					{
						EnvProxy.Instance.Registry.RemoteAppSendTestingMessageOnInitialize = false;
					},
					() =>
					{
						EnterpriseChannel.Instance.OnDisconnect();
						WtsApi.Instance = originalWtsApi;
						EnvProxy.Instance.Registry.RemoteAppSendTestingMessageOnInitialize = true;
					}))
				using (ObjectFactory.Substitute(mockRemoteFile.Object))
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					InitializationMessageHandler.RemoteInitializationMessage = new Enterprise.RemoteDesktopServices.MessageElements.InitializationMessage(
						new[] {
							EnterpriseChannelMessageTypes.SaveFileDialog,
							EnterpriseChannelMessageTypes.OpenFile,
							EnterpriseChannelMessageTypes.OpenFileSupported
						}, task.GetExecutionResultCallbackVersionExposed().ToString());

					// Act
					Assert("InitializeChannel(...) should return true if upgrader failed to open", task.InitializeChannel(false));

					// Assert
					AssertEquals(1, EnterpriseChannel.Instance.ConnectedCount);
					AssertEquals(true, RemoteDesktopServicesInitializationTask.ChannelInitialized);
					AssertErrorsReportedCausedByWtsApiNotSetUpWellAreExpected3();
				}
			}
			finally
			{
				EnterpriseChannel.Instance?.Close();
				UnitTestUserNotification.Instance.ClearMessages();
				InitializationMessageHandler.RemoteInitializationMessage = null;
			}
		}

		internal static void AssertBasicRemoteDesktopServicesInitialized(string message)
		{
			AssertNotNull(message, EnterpriseChannel.Instance);

			var expectedHandlers = new[] { EnterpriseChannelMessageTypes.ReturnCallback, EnterpriseChannelMessageTypes.InitializationNew, EnterpriseChannelMessageTypes.Initialization, EnterpriseChannelMessageTypes.EdiEntUrl, EnterpriseChannelMessageTypes.CheckDriveMapping };
			AssertContainsExactElementsInAnyOrder("Should register basic handlers for url authentication", expectedHandlers, MessageHandlers.RegisteredMessageTypes);
		}

		internal static void AssertAllRemoteDesktopServicesInitialized()
		{
			var expectedHandlers = new[]
			{
				EnterpriseChannelMessageTypes.ReturnCallback,
				EnterpriseChannelMessageTypes.Initialization,
				EnterpriseChannelMessageTypes.InitializationNew,
				EnterpriseChannelMessageTypes.EdiEntUrl,
				EnterpriseChannelMessageTypes.StartDrop,
				EnterpriseChannelMessageTypes.DragOver,
				EnterpriseChannelMessageTypes.DragDrop,
				EnterpriseChannelMessageTypes.DragDropLite,
				EnterpriseChannelMessageTypes.OpenFileChanged,
				EnterpriseChannelMessageTypes.SaveSentEmail,
				EnterpriseChannelMessageTypes.DragStatus,
				EnterpriseChannelMessageTypes.SessionSwitch,
				EnterpriseChannelMessageTypes.CheckDriveMapping,
				EnterpriseChannelMessageTypes.DragCallBack,
				EnterpriseChannelMessageTypes.MicrosoftOffice365Message
			};
			AssertContainsExactElementsInAnyOrder("Should register all handlers after login", expectedHandlers, MessageHandlers.RegisteredMessageTypes);
		}

		class RemoteDesktopServicesInitializationTaskForUpgradeTest : RemoteDesktopServicesInitializationTask
		{
			protected override Version GetServerVersion(bool isCitrix)
			{
				var baseVersion = base.GetServerVersion(isCitrix);
				return new Version(baseVersion.Major, baseVersion.Minor + 1);
			}

			public Version GetExecutionResultCallbackVersionExposed(bool isCitrix = false)
			{
				return GetExecutionResultCallbackVersion(isCitrix);
			}
		}

#if NETFRAMEWORK // Should be fixed in WI00669071: Remove usages of AppDomains
		protected override void SetUp()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			base.SetUp();
			appDomain = AppDomain.CreateDomain("RemoteDesktopServicesInitializationTaskTest");
			appDomain.DoCallBack(new AppDomainHelper(BaseSourcePath, Db.ServerName, Db.DatabaseName).Setup);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		protected override void TearDown()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			base.TearDown();
			appDomain.DoCallBack(() => { ExceptionReporterTestListener.Instance.EndTest(null, DateTime.Now); });
			int attempts = 0;
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			while (attempts < 5)
			{
				Exception lastException = null;

				GC.WaitForPendingFinalizers();

				var task = Task.Run(() =>
				{
					try
					{
						AppDomain.Unload(appDomain);
					}
					catch (AppDomainUnloadedException ex)
					{
						lastException = ex;
					}
					catch (CannotUnloadAppDomainException ex)
					{
						lastException = ex;
					}
					catch (OperationCanceledException ex)
					{
						lastException = ex;
					}
				}, cancellationToken);

				try
				{
					if (task.Wait(3000, cancellationToken))
					{
						if (lastException != null)
						{
							if (lastException.GetType() == typeof(AppDomainUnloadedException))
							{
								// if we are here means AppDomain is unloaded.
								break;
							}
							else if (lastException.GetType() == typeof(CannotUnloadAppDomainException))
							{
								// AppDomain can't be unloaded right now. Try again, but it might not be possible depending on what AppDomain is actually doing:
								// https://msdn.microsoft.com/en-us/library/system.appdomain.unload%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
								++attempts;
							}
							else
							{
								//somehow other exception? try again
								++attempts;
							}
						}
						else
						{
							//unloading successful - we're done
							break;
						}
					}
					else
					{
						//unloading timed out - try again
						cancellationTokenSource.Cancel();
						++attempts;
					}
				}
				catch (OperationCanceledException)
				{
					cancellationTokenSource.Cancel();
					++attempts;
				}
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}
#endif

		static void AssertErrorsReportedCausedByWtsApiNotSetUpWellAreExpected3()
		{
			AssertEquals(2, ErrorReporter.TotalErrorCount);
			AssertEquals(true, ErrorReporter.HasBeenReported("EnterpriseChannel-SendInfo ServerRDPVersion NotRegistered [FirstAttempOnInitializing]"));
			AssertEquals(true, ErrorReporter.HasBeenReported("EnterpriseChannel-SendInfo RemoteAppSettings NotRegistered [FirstAttempOnInitializing]"));

			// Cleanup
			ErrorReporter.Clear();
		}

#if NETFRAMEWORK // Should be fixed in WI00669071: Remove usages of AppDomains
		AppDomain appDomain;
#endif

		public override int DefaultErrorExitCode => ExitCodes.RemoteDesktopServicesInitializationTaskError;

		#region AppDomainHelper

		[Serializable]
		class AppDomainHelper
		{
			internal AppDomainHelper(string baseSourcePath, string server, string db)
			{
				this.baseSourcePath = baseSourcePath;
				this.server = server;
				this.db = db;
			}

			internal void Setup()
			{
				TestingState.Setup();
				BaseSourcePath = baseSourcePath;
				Db.InitializeDatabaseDetails(server, db);
				Globals.IsUserInteractive = true;
				new WinFormsEnvironmentProvider().Enable();
				Initialiser.InitialiseWinForms();
				TestingState.Setup();
				ExceptionReporterTestListener.Instance.StartTest(null, DateTime.Now);
			}

			readonly string baseSourcePath;
			readonly string server;
			readonly string db;
		}

		#endregion

	}
}
