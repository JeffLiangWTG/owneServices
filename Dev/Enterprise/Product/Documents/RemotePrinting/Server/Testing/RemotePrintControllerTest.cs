using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Principal;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.Testing
{
	[ThreadSafe]
	[HttpContextEnabledTest]
	public class RemotePrintControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRegisterClientForReconnecting()
		{
			var client = new Mock<IRemoteClient>();
			var clients = new Mock<IHubConnectionContext<IRemoteClient>>();
			clients.Setup(c => c.Client(It.IsAny<string>())).Returns(client.Object);
			var hub = new Mock<IHubContext<IRemoteClient>>();
			hub.Setup(h => h.Clients).Returns(clients.Object);
			var controller = new RemotePrintControllerForTest(hub.Object);
			controller.RegisterClientForReconnecting("TestConnectionId");

			client.Verify(c => c.RegisterClientForReconnecting(), Times.Once);
			clients.Verify(c => c.Client(It.IsAny<string>()), Times.Exactly(2));
			hub.Verify(c => c.Clients, Times.Exactly(2));
		}

		#region Test Register Clients

		[UseSnapshotProtection]
		public void TestRemoveRegisteredSignalRClientInfoByServerName()
		{
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintServerSchema.Constants.TableName);

			var printers = new string[] { "Queue1", "Queue2" };
			var controller = new RemotePrintControllerForTest3(GetMockHubContext());
			controller.WebServiceAddressForRegisterTest = "http://Jerrytest1.com";

			// Register first client
			controller.RegisterClient(new HubCallerContext(null, "JerryTestConnectionID1"), "JerryTestServer", printers, "1.0");
			AssertRegisterClient(controller, 2, "http://Jerrytest1.com", 1, "JerryTestConnectionID1");

			// Register second client
			controller.WebServiceAddressForRegisterTest = "http://Jerrytest2.com";
			controller.RegisterClient(new HubCallerContext(null, "JerryTestConnectionID2"), "JerryTestServer", printers, "1.0");
			AssertRegisterClient(controller, 2, "http://Jerrytest2.com", 1, "JerryTestConnectionID2");
		}

		[UseSnapshotProtection]
		public void TestUnregisterClientAndGetVersionNumberShouldRemovePrintQueuesWebPrintServerAddress()
		{
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintServerSchema.Constants.TableName);

			var printers = new string[] { "Queue1", "Queue2" };
			var controller = new RemotePrintControllerForTest3(GetMockHubContext());
			controller.WebServiceAddressForRegisterTest = "http://Jerrytest1.com";

			// Register client
			controller.RegisterClient(new HubCallerContext(null, "JerryTestConnectionID"), "JerryTestServer", printers, "1.0");
			AssertRegisterClient(controller, 2, "http://Jerrytest1.com", 1, "JerryTestConnectionID");

			// Unregister client
			controller.UnregisterClientAndGetVersionNumber(new HubCallerContext(null, "JerryTestConnectionID"));
			AssertRegisterClient(controller, 2, "", 0, "");
		}

		void AssertRegisterClient(RemotePrintController controller, int expectedQueuesCount, string expectedWebPrintServiceAddress, int expectedRegisteredClientCount, string expectedClientId)
		{
			var factory = new BusinessObjectFactory();
			var queues = factory.Load<StmPrintQueue>(new ZQuery());
			AssertEquals($"Should have {expectedQueuesCount} print queues", expectedQueuesCount, queues.Length);
			Assert($"StmPrintQueue SQ_WebPrintServiceAddress should be update to {expectedWebPrintServiceAddress}", !queues.Any(q => q.SQ_WebPrintServiceAddress != expectedWebPrintServiceAddress));

			var registerClients = controller.GetRegisteredClients();
			AssertEquals($"Should have {expectedRegisteredClientCount} registered client", expectedRegisteredClientCount, registerClients.Length);
			Assert($"Registered client id should be {expectedClientId}", !registerClients.Any(c => c.ClientId != expectedClientId));
		}

		#endregion

		public void TestGetSerialisablePrintJobJustOnlyLoadPRNPrintJobs()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = "ServiceAddress1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var printJob1 = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);

			var printJob2 = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			printJob2.SP_JobType = nameof(PrintType.PRS);
			Factory.Save();

			var hubMock = new Mock<IHubContext<IRemoteClient>>();
			var controller = new RemotePrintControllerForTest(hubMock.Object);

			var (job1, serviceAddress1) = controller.GetSerialisablePrintJob(printJob1.PK.ToGuid(), TestConnection);
			AssertEquals("Should load the jobs which job type is PRN", printJob1.PK, job1.JobPk);
			AssertEquals("ServiceAddress1", serviceAddress1);

			var (job2, serviceAddress2) = controller.GetSerialisablePrintJob(printJob2.PK.ToGuid(), TestConnection);
			AssertEquals("Cannot load the jobs which job type is PRS", null, job2);
			AssertEquals(null, serviceAddress2);
		}

		public void TestForwardNudgeToCorrectServer()
		{
			var printServer = Factory.New<StmPrintServer>();
			printServer.SPS_ServerName = "Server1";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";
			printQueue.SQ_WebPrintServiceAddress = "ServiceAddress1";
			printQueue.SQ_SPS_Server = printServer.PK;

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];
			var largePrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			largePrintJob.SP_CustomProperties = new byte[100_000];
			Factory.Save();

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var controller = new RemotePrintControllerForTest(GetMockHubContext(null));
			controller.ForceRequestPrintForTest = true;
			controller.Nudge("Server1", "Printer1", smallPrintJob.PK.ToGuid(), TestConnection);

			AssertEquals("Should send forward nudge to correct server", true, controller.NudgeForwarded);
			AssertEquals("Should not direct print if the server is incorrect", "QUE", smallPrintJob.SP_Status);
			AssertEquals("Should load service address", "ServiceAddress1", controller.ServiceAddressForTest);

			controller.Nudge("Server1", "Queue1", Guid.Empty, TestConnection);
			AssertEquals("Should send forward nudge to correct server", true, controller.NudgeForwarded);
			AssertEquals("Should not direct print if the server is incorrect", "QUE", smallPrintJob.SP_Status);
			AssertEquals("Should load service address", "ServiceAddress1", controller.ServiceAddressForTest);

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			controller.Nudge("Server1", "Queue1", smallPrintJob.PK.ToGuid(), TestConnection);
			AssertEquals("Should send forward nudge to correct server", true, controller.NudgeForwarded);
			AssertEquals("Should not direct print if the server is incorrect", "QUE", smallPrintJob.SP_Status);
			AssertEquals("Should load service address", "ServiceAddress1", controller.ServiceAddressForTest);
		}

		[UseSnapshotProtection]
		public void TestForwardNudgeToCorrectServerWithHttpServiceUrlWhenOccurAuthenticationExceptionWithInvalidCertificate()
		{
			try
			{
				ErrorReporter.Clear();
				WebRequestHelper.ThrowExceptionForTest.Value = new WebException("The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.", new AuthenticationException("The remote certificate is invalid according to the validation procedure."));

				var printServer = Factory.New<StmPrintServer>();
				printServer.SPS_ServerName = "Server1";

				var printQueue = Factory.New<StmPrintQueue>();
				printQueue.SQ_ServerName = "Server1";
				printQueue.SQ_QueueName = "Queue1";
				printQueue.SQ_DisplayName = "QueueDisplay1";
				printQueue.SQ_WebPrintServiceAddress = "https://10.20.30.50:443/WebPrint|host:jerry.hostname.com";
				printQueue.SQ_SPS_Server = printServer.PK;
				Factory.Save();

				var controller = new RemotePrintControllerForTest3(GetMockHubContext(null));
				controller.WebServiceAddressForRegisterTest = "https://10.20.30.51:443/WebPrint|host:test.com";
				controller.Nudge("Server1", "Queue1", Guid.NewGuid(), TestConnection);

				var lastWebRequest = WebRequestHelper.LastSentWebRequestForTest.Value;
				AssertEquals("Should use the correct host name.", "jerry.hostname.com", lastWebRequest.Host);
				AssertEquals("Should no error", string.Empty, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestNudgeLoadPrintJobIfWebPrintAllowDirectPrintPrintPushNotificationEnabled()
		{
			var loadPrintJobSql = "SP_PK = @PrintJobPK";
			var printJobPk = Guid.NewGuid();

			var controller = new RemotePrintControllerForTest(GetMockHubContext());
			using (TestConnection.TrackExecutedCommands())
			{
				DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				controller.Nudge("Server1", "Queue1", printJobPk, TestConnection);

				AssertNotContains("Should not load print job if WebPrintAllowDirectPrintPrintPushNotification is disabled", loadPrintJobSql, string.Join(System.Environment.NewLine, TestConnection.ExecutedCommands));

				DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				controller.Nudge("Server1", "Queue1", printJobPk, TestConnection);

				AssertContains("Should load print job if WebPrintAllowDirectPrintPrintPushNotification is enabled", loadPrintJobSql, string.Join(System.Environment.NewLine, TestConnection.ExecutedCommands));
			}
		}

		public void TestNudgeClient()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];

			var largePrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			largePrintJob.SP_CustomProperties = new byte[100_000];

			Factory.Save();

			var controller = new RemotePrintControllerForTest(GetMockHubContext());

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNudge(controller, "Server1", "Queue1", smallPrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: false, expectedNudgedClientId: "1");

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// Should call RequestPrint for small print job
			AssertNudge(controller, "Server1", "Queue1", smallPrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: true, expectedNudgedClientId: "1");
			// Should NOT call RequestPrint for large print job
			AssertNudge(controller, "Server1", "Queue1", largePrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: false, expectedNudgedClientId: "1");

			AssertNudge(controller, "Server1", "", smallPrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: true, expectedNudgedClientId: "1");
			AssertNudge(controller, "Server1", null, smallPrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: true, expectedNudgedClientId: "1");

			AssertNudge(controller, "Server1", "Queue1", Guid.Empty,
				expectedSearchedClientForPrinter: "Queue1", expectedRequestedPrint: false, expectedNudgedClientId: "1");
			AssertNudge(controller, "Server1", "Queue1", Guid.NewGuid(),
				expectedSearchedClientForPrinter: "Queue1", expectedRequestedPrint: false, expectedNudgedClientId: "1");

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNudge(controller, "Server1", "", Guid.Empty,
				expectedSearchedClientForPrinter: null, expectedRequestedPrint: false, expectedNudgedClientId: "2");
			AssertNudge(controller, "Server1", "", Guid.NewGuid(),
				expectedSearchedClientForPrinter: null, expectedRequestedPrint: false, expectedNudgedClientId: "2");
			AssertNudge(controller, "Server1", null, Guid.Empty,
				expectedSearchedClientForPrinter: null, expectedRequestedPrint: false, expectedNudgedClientId: "2");
			AssertNudge(controller, "Server1", null, Guid.NewGuid(),
				expectedSearchedClientForPrinter: null, expectedRequestedPrint: false, expectedNudgedClientId: "2");
		}

		void AssertNudge(RemotePrintControllerForTest controller, string serverName, string printerName, Guid jobPK,
			string expectedSearchedClientForPrinter, bool expectedRequestedPrint, string expectedNudgedClientId)
		{
			controller.ResetState();
			controller.Nudge(serverName, printerName, jobPK, TestConnection);

			AssertEquals(serverName, controller.SearchedClientForServer);
			AssertEquals(expectedSearchedClientForPrinter, controller.SearchedClientForPrinter);
			AssertEquals(expectedRequestedPrint, controller.RequestedPrint);
			AssertEquals(!expectedRequestedPrint, controller.Nudged);
			AssertEquals(expectedNudgedClientId, controller.NudgedClientId);
			AssertEquals(false, controller.NudgeForwarded);
		}

		public StmPrintJob GetNewStmPrintJob(ZGuid queuePk, ZGuid deliveryGroupPK)
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-3);
			printJob.SP_JobType = nameof(PrintType.PRN);
			printJob.SP_SQ = queuePk;
			printJob.SP_RetryAttempts = 0;
			printJob.SP_SB_DeliveryGroup = deliveryGroupPK;
			printJob.SP_Status = nameof(PrintJobStatus.QUE);
			return printJob;
		}

		public void TestRegisterSignalRExceptionHandler()
		{
			try
			{
				Assert("Should not be registered", !((IEnumerable)ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName)).OfType<SignalRExceptionHandler>().Any());

				var controller = new RemotePrintControllerForTest(null);

				Assert("Should be registered", ((IEnumerable)ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName)).OfType<SignalRExceptionHandler>().Any());
			}
			finally
			{
				SignalRExceptionHandler.UnregisterHandler();
			}

			Assert("Should not be registered", !((IEnumerable)ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName)).OfType<SignalRExceptionHandler>().Any());
		}

		public void TestNoDuplicateJobSent()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];
			smallPrintJob.SP_RetryAttempts = 2;

			Factory.Save();

			var controller = new RemotePrintControllerForTest(GetMockHubContext());

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNudge(controller, "Server1", "Queue1", smallPrintJob.PK.ToGuid(),
				expectedSearchedClientForPrinter: printQueue.SQ_QueueName, expectedRequestedPrint: false, expectedNudgedClientId: "1");
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestShouldNotAddPrinterForSupportUser()
		{
			var printers = new string[] { "print1", "print2", "print3" };

			var controller = new RemotePrintControllerForTest(GetMockHubContext());
			var context = new HubCallerContext(null, DbHelper.NewConnection().ObjectID.ToString());

			controller.RegisterClient(context, "server1", printers, "1.0");

			var signalRClientInfo = controller.GetRegisteredClients()[0];

			AssertNotNull(signalRClientInfo);
			AssertEquals("server1", signalRClientInfo.ServerName);
			AssertEquals(3, signalRClientInfo.PrintersList.Count);

			SetHttpContextUser("CWSupport-XYZ");
			controller.RegisterClient(context, "server1", printers, "1.0");

			signalRClientInfo = controller.GetRegisteredClients()[0];
			AssertNotNull(signalRClientInfo);
			AssertEquals("server1", signalRClientInfo.ServerName);
			AssertEquals(0, signalRClientInfo.PrintersList.Count);
		}

		void SetHttpContextUser(string userName)
		{
			HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(userName, "Enterprise.RemotePrinting.Server.Digest"), Array.Empty<string>());
		}

		public void TestMarkJobAsWorking()
		{
			var printServer = Factory.New<StmPrintServer>();
			printServer.SPS_ServerName = "SangoServer";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_SPS_Server = printServer.PK;
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];
			smallPrintJob.SP_Status = nameof(PrintJobStatus.QUE);

			// Case with RetryAttempts > 0
			smallPrintJob.SP_RetryAttempts = 2;

			Factory.Save();
			var controller = new RemotePrintControllerForTest(null);
			Assert(!controller.MarkJobAsWorking(smallPrintJob.PK.ToGuid(), TestConnection));
			var reloadedPrintJob = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJob>(smallPrintJob.PK);

			AssertEquals("Should not change for RetryAttempts > 0", nameof(PrintJobStatus.QUE), reloadedPrintJob.SP_Status);

			// Case with RetryAttempts = 0

			smallPrintJob.SP_RetryAttempts = 0;

			Factory.Save();
			Assert(controller.MarkJobAsWorking(smallPrintJob.PK.ToGuid(), TestConnection));

			reloadedPrintJob = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJob>(smallPrintJob.PK);

			AssertEquals("Should change for RetryAttempts = 0", "WRK", reloadedPrintJob.SP_Status);
		}

		public void TestMarkJobAsQueue()
		{
			var printServer = Factory.New<StmPrintServer>();
			printServer.SPS_ServerName = "SangoServer";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_SPS_Server = printServer.PK;
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];
			smallPrintJob.SP_Status = "WRK";

			smallPrintJob.SP_RetryAttempts = 0;

			Factory.Save();
			var controller = new RemotePrintControllerForTest(null);
			Assert(controller.MarkJobAsQueue(smallPrintJob.PK.ToGuid(), TestConnection));

			var reloadedPrintJob = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJob>(smallPrintJob.PK);

			AssertEquals("Job Status should be set to queue", "QUE", reloadedPrintJob.SP_Status);
		}

		public void TestNudgeExceptionThrown()
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_DisplayName = "QueueDisplay1";

			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var smallPrintJob = GetNewStmPrintJob(printQueue.PK, deliveryGroup.PK);
			smallPrintJob.SP_CustomProperties = new byte[100];
			smallPrintJob.SP_RetryAttempts = 0;
			Factory.Save();

			var controller = new RemotePrintControllerForTest2(GetMockHubContext());

			DocumentsDataRegistry.Instance.WebPrintAllowDirectPrintPrintPushNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			controller.ResetState();
			Assert("Should return false", !controller.Nudge("Server1", "Printer1", smallPrintJob.PK.ToGuid(), TestConnection));
			AssertContains("Nudge failed with the following exception: Don't judge each day by the harvest you reap but by the seeds that you plant.", ErrorReporter.LastMessageReported);

			var reloadedPrintJob = new BusinessObjectFactory { RefreshEnabled = false }.Load<StmPrintJob>(smallPrintJob.PK);

			AssertEquals("Job Status should be set back to QUE", "QUE", reloadedPrintJob.SP_Status);

			ErrorReporter.Clear();
		}

		IHubContext<IRemoteClient> GetMockHubContext()
		{
			var client = new Mock<IRemoteClient>();
			return GetMockHubContext(client.Object);
		}

		IHubContext<IRemoteClient> GetMockHubContext(IRemoteClient client)
		{
			var clients = new Mock<IHubConnectionContext<IRemoteClient>>();
			clients.Setup(c => c.Client(It.IsAny<string>())).Returns(client);
			var hubMock = new Mock<IHubContext<IRemoteClient>>();
			hubMock.Setup(h => h.Clients).Returns(clients.Object);

			return hubMock.Object;
		}
	}

	#region Class For Testing

	public class RemotePrintControllerForTest : RemotePrintController
	{
		public RemotePrintControllerForTest(IHubContext<IRemoteClient> hub)
			: base(hub)
		{
		}

		public string SearchedClientForServer { get; private set; }

		public string SearchedClientForPrinter { get; private set; }

		public bool Nudged { get; private set; }

		public bool RequestedPrint { get; private set; }

		public string NudgedClientId { get; private set; }

		public bool NudgeForwarded { get; private set; }

		public bool ForceRequestPrintForTest { get; set; }

		public string ServiceAddressForTest { get; private set; }

		public void ResetState()
		{
			SearchedClientForServer = string.Empty;
			SearchedClientForPrinter = string.Empty;
			Nudged = false;
			RequestedPrint = false;
			NudgedClientId = null;
		}

		protected override string FindClientForPrinter(string server, string printer)
		{
			SearchedClientForServer = server;
			SearchedClientForPrinter = printer;
			return "1";
		}

		protected override string FindClient(string server)
		{
			SearchedClientForServer = server;
			SearchedClientForPrinter = null;
			return "2";
		}

		protected override bool NudgeCore(string clientId)
		{
			Nudged = true;
			NudgedClientId = clientId;
			return false;
		}

		protected override bool ForwardNudgeToCorrectServer(string serviceAddress, string serverName, string queueName, Guid printJobPK, HubCallerContext context, DbConnection connection)
		{
			ServiceAddressForTest = serviceAddress;
			NudgeForwarded = true;
			return true;
		}

		protected override bool RequestPrintCore(string clientId, SerialisablePrintJob job)
		{
			if (ForceRequestPrintForTest)
			{
				return true;
			}

			RequestedPrint = true;
			NudgedClientId = clientId;
			return false;
		}

		protected override string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection)
		{
			return "HOST/SangoTheHost";
		}
	}

	public class RemotePrintControllerForTest2 : RemotePrintControllerForTest
	{
		public RemotePrintControllerForTest2(IHubContext<IRemoteClient> hub)
			: base(hub)
		{
		}

		protected override bool NudgeCore(string clientId)
		{
			throw new Exception("Don't judge each day by the harvest you reap but by the seeds that you plant.");
		}
	}

	public class RemotePrintControllerForTest3 : RemotePrintController
	{
		public RemotePrintControllerForTest3(IHubContext<IRemoteClient> hub)
			: base(hub)
		{
		}

		protected override string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection) => WebServiceAddressForRegisterTest;

		public string WebServiceAddressForRegisterTest { get; set; }
	}

	#endregion
}
