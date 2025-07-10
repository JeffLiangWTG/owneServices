using System;
using System.Net;
using System.Text;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Microsoft.AspNet.SignalR.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class HubConnectionManagerTest : TestCase
	{
		public void TestPauseSignalRForceRestart()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, true, true, 3, 5, 20, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var logs = new StringBuilder();
				var hubConnectionManager = new HubConnectionManagerForStartTest(registryManager, null);
				hubConnectionManager.LogInformation += (_, logEvent) => logs.AppendLine(logEvent.Message);
				hubConnectionManager.ShouldPauseSignalRConnectionForAWhile_ForTest = true;
				hubConnectionManager.ForceStart_Exposed(webConfig1);

				var expectedMessage = @"Hub connection has been dropped and could not be reestablished. Please check your network connection & server.
This client will attempt to reconnect to the hub periodically.
Too many frequent connection attempts to SignalR Hub. Print Nudging will be suspended for 20 minutes from now on.
";
				AssertEquals(expectedMessage, logs.ToString());
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestShouldPauseSignalRConnectionForAWhile()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, true, true, 3, 5, 20, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			var hubConnectionManager = new HubConnectionManagerForStartTest(registryManager, null);
			var shouldPause = false;
			for (var i = 0; i < 4; i++)
			{
				shouldPause = hubConnectionManager.ShouldPauseSignalRConnectionForAWhile_Exposed(webConfig1);
			}
			AssertEquals("Should not pause SignalR", false, shouldPause);

			shouldPause = hubConnectionManager.ShouldPauseSignalRConnectionForAWhile_Exposed(webConfig1);
			AssertEquals("Should pause SignalR", true, shouldPause);
		}

		public void TestCredentialsWithProtectedPassword()
		{
			var protectedPwd = ProtectedDataHelper.Protect("pwd1");
			var webConfig1 = new WebClientConfiguration("url1", "user1", protectedPwd, 1, "machine1", true, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var hubConnectionManager = new HubConnectionManagerForTest(null, null);
			var hubConnection = hubConnectionManager.CreateHubConnection_ForTest(webConfig1);
			var password = ((NetworkCredential)hubConnection.Credentials).Password;
			CombineAssertions(() =>
			{
				AssertNotEquals("pwd1", protectedPwd);
				AssertEquals("pwd1", password);
			});
		}

		public void TestHubConnectionStartShouldHandleRedirectResponse()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", true, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			var controller = new HubClientController(null, null, null, null, null);
			const string ConfigName = "configForTest1";
			MethodDelegate retryAction = null;
			Exception outException = null;

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var mockResponseProcessor = new Mock<IErrorResponseWebRequestProcessor>();
				mockResponseProcessor.Setup(r => r.Process(It.IsAny<MethodDelegate>(), out retryAction, out outException, It.IsAny<bool>())).Verifiable();
				var hubConnectionManager = new HubConnectionManagerForStartTest(registryManager, controller, mockResponseProcessor.Object);

				AssertNoExceptionThrown(() => hubConnectionManager.Start(ConfigName));
				mockResponseProcessor.Verify();
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestStartWithLoopAllTransports()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", true, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			var controller = new HubClientController(null, null, null, null, null);
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var logs = new StringBuilder();
				var hubConnectionManager = new HubConnectionManagerForStartTest(registryManager, controller);
				hubConnectionManager.LogInformation += (_, logEvent) => logs.AppendLine(logEvent.Message);
				var expectedMessage = @"Connecting to hub.
Connecting with transport: AutoTransport
AutoTransport failed: Invalid URI: The format of the URI could not be determined.
Connecting with transport: PrintWebSocketTransport
PrintWebSocketTransport failed: Invalid URI: The format of the URI could not be determined.
Connecting with transport: ServerSentEventsTransport
ServerSentEventsTransport failed: Invalid URI: The format of the URI could not be determined.
Connecting with transport: LongPollingTransport
LongPollingTransport failed: Invalid URI: The format of the URI could not be determined.
";

				AssertExceptionThrown<ApplicationException>(() => hubConnectionManager.Start(ConfigName));
				AssertEquals(expectedMessage, logs.ToString());
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestHubConnectionTimeoutSetup()
		{
			const int ServerTimeout = 123;

			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, ServerTimeout, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				var hubConnection = hubConnectionManager.CreateHubConnection_ForTest(webConfig1);

				AssertEquals(ServerTimeout * 1000, (int)hubConnection.TransportConnectTimeout.TotalMilliseconds);
				AssertEquals(ServerTimeout * 1000, (int)hubConnection.DeadlockErrorTimeout.TotalMilliseconds);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestHubConnectionProxySetup()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", true, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				var hubConnection = hubConnectionManager.CreateHubConnection_ForTest(webConfig1);

				AssertNotNull("Should setup web proxy", hubConnection.Proxy);
				AssertNotEquals(WebRequest.DefaultWebProxy, hubConnection.Proxy);
				AssertEquals(typeof(WebProxy), hubConnection.Proxy.GetType());

				var webProxy = (WebProxy)hubConnection.Proxy;

				AssertEquals("proxy1", webProxy.Address.Host);
				AssertEquals(43, webProxy.Address.Port);

				var networkCredentials = webProxy.Credentials as NetworkCredential;

				AssertNotNull("Should have network credentials", networkCredentials);
				AssertEquals("proxyuser1", networkCredentials.UserName);
				AssertEquals("proxypwd1", networkCredentials.Password);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestHubConnectionNoProxySetup()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				var hubConnection = hubConnectionManager.CreateHubConnection_ForTest(webConfig1);

				AssertNull("Should not setup web proxy", hubConnection.Proxy);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestHubConnectionDefaultProxySetup()
		{
			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", true, "proxy1", 43, "proxyuser1", "proxypwd1", true, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false,
				0, 0, false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			const string ConfigName = "configForTest1";

			// Create test Registry Keys
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				var hubConnection = hubConnectionManager.CreateHubConnection_ForTest(webConfig1);

				AssertNotNull("Should setup web proxy", hubConnection.Proxy);
				AssertEquals("Should use default web proxy", WebRequest.DefaultWebProxy, hubConnection.Proxy);

				var networkCredentials = hubConnection.Proxy.Credentials as NetworkCredential;

				AssertNotNull("Should have network credentials", networkCredentials);
				AssertEquals("proxyuser1", networkCredentials.UserName);
				AssertEquals("proxypwd1", networkCredentials.Password);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestConnectionException_BaseInfo()
		{
			Exception exception = null;
			try
			{
				RaiseException();
				Fail("Should go to catch block with exception.");
			}
			catch (InvalidOperationException ex)
			{
				exception = ex;
			}

			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false, 0, 0,
				enableVerboseLogging: false, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			const string ConfigName = "configForTest1";

			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var lastLog = string.Empty;

				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				hubConnectionManager.LogErrorMessage += (_, logEvent) => lastLog = logEvent.Message;

				hubConnectionManager.HandleConnectionError_ForTest(exception, false);

				AssertEquals("SignalR connection error: Abc xyz", lastLog);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		public void TestConnectionException_VerboseInfo()
		{
			Exception exception = null;
			try
			{
				RaiseException();
				Fail("Should go to catch block with exception.");
			}
			catch (InvalidOperationException ex)
			{
				exception = ex;
			}

			var webConfig1 = new WebClientConfiguration("url1", "user1", "pwd1", 1, "machine1", false, "proxy1", 43, "proxyuser1", "proxypwd1", false, false,
				5000, false, 0, 0, 100, 123, false, false, 0, 0, 0, false, 0, 0,
				enableVerboseLogging: true, 0,
				ConnectionRegistryManagerTest.GetWebClientUpdateConfigurationForTest(), false, 100);
			const string ConfigName = "configForTest1";

			var registryManager = new ConnectionRegistryManagerTest.ConnectionRegistryManagerForTest();
			registryManager.LoadFromRegistry(ConfigName);
			registryManager.SaveRemotePrintingRegistryValues(webConfig1);
			try
			{
				var lastLog = string.Empty;

				var hubConnectionManager = new HubConnectionManagerForTest(registryManager, null);
				hubConnectionManager.LogErrorMessage += (_, logEvent) => lastLog = logEvent.Message;

				hubConnectionManager.HandleConnectionError_ForTest(exception, true);

				string expectedLog = $@"SignalR connection error: Abc xyz
{exception.GetType().FullName}
{exception.StackTrace}
";

				AssertEquals(expectedLog, lastLog);
			}
			finally
			{
				registryManager.DeleteFromRegistry(ConfigName);
			}
		}

		void RaiseException()
		{
			throw new InvalidOperationException("Abc xyz");
		}
	}

	class HubConnectionManagerForTest : HubConnectionManager
	{
		public HubConnectionManagerForTest(ConnectionRegistryManager connectionRegistryManager, HubClientController clientController)
			: base(connectionRegistryManager, clientController, new ErrorResponseWebRequestProcessor(new RemotePrintingService()))
		{
		}

		public void HandleConnectionError_ForTest(Exception ex, bool verbose) => HandleConnectionError(ex, verbose);

		public HubConnection CreateHubConnection_ForTest(WebClientConfiguration config) => CreateHubConnection(config);
	}

	class HubConnectionManagerForStartTest : HubConnectionManager
	{
		public HubConnectionManagerForStartTest(ConnectionRegistryManager connectionRegistryManager, HubClientController clientController)
			: this(connectionRegistryManager, clientController, new ErrorResponseWebRequestProcessor(new RemotePrintingService()))
		{
		}

		public HubConnectionManagerForStartTest(ConnectionRegistryManager connectionRegistryManager, HubClientController clientController, IErrorResponseWebRequestProcessor responseProcessor)
			: base(connectionRegistryManager, clientController, responseProcessor)
		{
		}

		protected override PrintHubConnection NewHubConnectionCore(string url) => new Mock<PrintHubConnection>("invalidUrl").Object;

		public void ForceStart_Exposed(WebClientConfiguration config) => ForceRestart(config);

		protected override void CreateTimerToPauseSignalRConnectionForAWhile(WebClientConfiguration config)
		{
			// Do nothing
		}

		protected override bool ShouldPauseSignalRConnectionForAWhile(WebClientConfiguration config) => ShouldPauseSignalRConnectionForAWhile_ForTest;

		public bool ShouldPauseSignalRConnectionForAWhile_ForTest { get; set; }

		public bool ShouldPauseSignalRConnectionForAWhile_Exposed(WebClientConfiguration config) => base.ShouldPauseSignalRConnectionForAWhile(config);
	}
}
