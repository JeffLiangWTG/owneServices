using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.ServiceModel;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Service.Reference;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Diagnostics;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.eHubMessaging.Tests.ServiceTasks;
using Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using SendStreamRequest = CargoWise.eHub.Common.Service.Reference.SendStreamRequest;

namespace Enterprise.eHubMessaging.Tests
{
	[TestedType(typeof(eAdaptorOutboundServiceTask))]
	class eAdaptorOutboundServiceTaskTests : eHubServiceTaskWithAdaptorTest<eAdaptorOutboundServiceTask, eAdaptorOutboundServiceTaskJob>
	{
		protected override void AssertErrorEmail(string containsMessage)
		{
			TestHelpers.AssertNoErrorEmail(); // eAdaptor should have no emails
		}

		protected override string EndpointNotFoundMessage => LogMessages.EAdaptorEndpointNotFoundMessage;

		public void TestEAdaptorOutboundServiceTask_CommunicationExceptions()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, "http://nooooooooooooooo.com/"))
			{
				var serviceTask = new Mock<eAdaptorOutboundServiceTask>() { CallBase = true } ;
				var notifier = new NotificationBuffer();
				var job = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
				job.Setup(m => m.Execute(It.IsAny<CancellationToken>())).Throws(new CommunicationException());
				job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
				serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
				serviceTask.Setup(m => m.Notifier).Returns(notifier);
				serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

				AssertNoExceptionThrown(() => serviceTask.Object.RunTask());
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertType<CommunicationException>(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestEAdaptorOutboundServiceTask_EndpointNotFoundException()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, "http://nooooooooooooooo.com/"))
			{
				var serviceTask = new Mock<eAdaptorOutboundServiceTask> { CallBase = true };
				var notifier = new NotificationBuffer();
				var job = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
				job.Setup(m => m.Execute(It.IsAny<CancellationToken>())).Throws(new EndpointNotFoundException("No endpoint is listening."));
				job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
				serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
				serviceTask.Setup(m => m.Notifier).Returns(notifier);
				serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

				serviceTask.Object.RunTask();

				AssertEquals(1, notifier.Events.Length);
				AssertEquals(NotificationType.Warning, notifier.Events[0].Type);
				AssertXMLContains(EndpointNotFoundMessage, notifier.AsString);
			}
		}

		public void TestEAdaptorOutboundServiceTask_InvalidServerAddress()
		{
			string uri = "https://ooooo.ca/eAdaptorOutbound/eAdaptorWebService.svc-do not use";
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, uri))
			{
				var serviceTask = new Mock<eAdaptorOutboundServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();
				var adaptorFactory = new OutboundAdaptorFactoryMock(uri);

				SetupMockEAdaptorOutboundServiceTask(notifier, serviceTask, adaptorFactory);
				serviceTask.Object.RunTask();

				AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
				AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.Events.Last().Type);
				AssertXMLContains("The eAdaptor Outbound Server Address is invalid", notifier.AsString);
			}
		}

		public void TestEAdaptorOutboundServiceTask_InvalidCommunicationPartyConfigServerAddress()
		{
			eAdaptorNextTestHelper.DoWithEAdaptorNextEnabled(() =>
			{
				string uri = "https://localhost/";
				var communicationParty = eAdaptorNextTestHelper.CreateEDICommunicationParty(Factory, "test", "bad_endpoint", EDICommunicationAuthModesList.Codes.BasicAuthentication);
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,
					Guid.Empty, uri))
				{
					var serviceTask = new Mock<eAdaptorOutboundServiceTask>() { CallBase = true };
					var notifier = new NotificationBuffer();
					var adaptorFactory = new OutboundAdaptorFactoryMock(uri);

					SetupMockEAdaptorOutboundServiceTask(notifier, serviceTask, adaptorFactory, communicationParty);
					serviceTask.Object.RunTask();

					AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
					AssertEquals(CargoWise.ComponentModel.NotificationType.Warning, notifier.Events.Last().Type);
					AssertXMLContains($"The eAdaptor end point of Communication Party Config: {communicationParty?.OutboundConfig?.Party?.ECP_Name} is invalid", notifier.AsString);
				}
			});
		}

	public new void TestCommunicationExceptionHandling()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://nooooooooooooooo.com/"))
			{
				var notifier = new Notifier();
				var service = new eAdaptorOutboundServiceTaskForTest { Notifier = notifier };
				var header = "A connection error occurred whilst connecting to http://nooooooooooooooo.com/.";
				notifier.ExpectedNotification =
		$@"{header}
Hint: This might be due to a fault on a switch or router.
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.Sockets.SocketException: The operation completed successfully";
				service.ExposeHandleCommunicationExceptionForTest(new SocketException());

				notifier.ExpectedNotification =
		$@"{header}
Hint: This might be due to a full HDD or incorrect permissions on the service task host.
System.ServiceModel.CommunicationException: Comm.Err
	System.IO.IOException: I/O error occurred.
	System.IO.PathTooLongException: Inner Exception1
	System.InvalidCastException: Inner Exception2";
				service.ExposeHandleCommunicationExceptionForTest(new IOException("I/O error occurred.",
					new PathTooLongException("Inner Exception1", new InvalidCastException("Inner Exception2"))));

				var mock = new Mock<HttpWebResponse>();
				notifier.ExpectedNotification =
		$@"{header}
HTTP Response: 502 - BadGateway
Hint: This might be due to a bad gateway between the service host and the server.
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.WebException: BadGateway";
				mock.SetupGet(c => c.StatusCode).Returns(HttpStatusCode.BadGateway);
				service.ExposeHandleCommunicationExceptionForTest(new WebException("BadGateway", null, WebExceptionStatus.ConnectFailure, mock.Object));

				notifier.ExpectedNotification =
		$@"{header}
HTTP Response: 403 - Forbidden
Hint: This might be due to the server restarting.
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.WebException: Forbidden";
				mock.SetupGet(c => c.StatusCode).Returns(HttpStatusCode.Forbidden);
				service.ExposeHandleCommunicationExceptionForTest(new WebException("Forbidden", null, WebExceptionStatus.ConnectFailure, mock.Object));

				notifier.ExpectedNotification =
		$@"{header}
HTTP Response: 401 - Unauthorized
Hint: The password specified by the registry (eServices > eAdaptor > Outbound eAdaptor Service URL) was not accepted by the server.
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.WebException: Unauthorized";
				mock.SetupGet(c => c.StatusCode).Returns(HttpStatusCode.Unauthorized);
				service.ExposeHandleCommunicationExceptionForTest(new WebException("Unauthorized", null, WebExceptionStatus.ConnectFailure, mock.Object));

				notifier.ExpectedNotification =
					$@"{header}
HTTP Response: 504 - GatewayTimeout
Hint: The server may be busy or there may be an issue with the network.
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.WebException: GatewayTimeout";
				mock.SetupGet(c => c.StatusCode).Returns(HttpStatusCode.GatewayTimeout);
				service.ExposeHandleCommunicationExceptionForTest(new WebException("GatewayTimeout", null, WebExceptionStatus.ConnectFailure, mock.Object));

				notifier.ExpectedNotification =
					$@"{header}
HTTP Response: 0 - 0
System.ServiceModel.CommunicationException: Comm.Err
	System.Net.WebException: Whut!?";
				mock.SetupGet(c => c.StatusCode).Returns(0);
				service.ExposeHandleCommunicationExceptionForTest(new WebException("Whut!?", null, WebExceptionStatus.ConnectFailure, mock.Object));
			}
		}

		public void TestAllowMultipleInstances()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				AssertEquals("AllowsMultipleInstances", true, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		public void TestServerAddress()
		{
			var expectedServerAddress = "https://localhost/";
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,
		Guid.Empty, expectedServerAddress))
			{
				var serviceTask = new Mock<eAdaptorOutboundServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();
				var adaptorFactory = new OutboundAdaptorFactoryMock(expectedServerAddress);

				SetupMockEAdaptorOutboundServiceTask(notifier, serviceTask, adaptorFactory);
				serviceTask.Object.RunTask();

				AssertEquals("Wrong number of Adapters created after first execute", 1, adaptorFactory.Adapters.Length);
			}
		}

		public void TestInvalidClientResponse()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://nooooooooooooooo.com/"))
			{
				var serviceTask = new Mock<eAdaptorOutboundServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();
				var adaptorFactory = new ErrorResponseOutboundAdaptorFactory();

				SetupMockEAdaptorOutboundServiceTask(notifier, serviceTask, adaptorFactory);

				AssertNoExceptionThrown(() => serviceTask.Object.RunTask());
				AssertContains("Error processing client response to outbound message", notifier.AsString);
			}
		}

		public void TestDiagnoseOutboundAdaptorConnectivityError()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty,Guid.Empty, "http://nooooooooooooooo.com/"))
			using (eAdaptorRegistry.Instance.OutboundAdaptorConnectivityTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://no.abcdef/"))
			{
				var notifier = new NotificationBuffer();
				ProcessOutboundMessageWithEndpointNotFoundException(notifier);

				AssertContains("Tried to connect to Service URL from registry", "Error connecting to [http://nooooooooooooooo.com/].", notifier.AsString);
				AssertContains("Tried to connect to Test Connectivity URL from registry", "Error connecting to [http://no.abcdef/].", notifier.AsString);
				AssertNotContains("Should not log diagnosis steps", "To Diagnose Further:", notifier.AsString);
			}
		}

		public void TestDiagnoseOutboundAdaptorConnectivityErrorValidServiceUrl()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/"))
			using (eAdaptorRegistry.Instance.OutboundAdaptorConnectivityTestUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://no.abcdef/"))
			{
				var notifier = new NotificationBuffer();
				ProcessOutboundMessageWithEndpointNotFoundException(notifier, returnSuccessCodeForLocalHost: true);

				AssertContains("Successfully connected to Service URL from registry", "Successfully connected to [http://localhost/].", notifier.AsString);
				AssertNotContains("Should not try to connect to Testing URL if Service URL is reachable", "Error connecting to [http://no.abcdef/].", notifier.AsString);
			}
		}

		void ProcessOutboundMessageWithEndpointNotFoundException(INotifications notifier, bool returnSuccessCodeForLocalHost = false)
		{
			var serviceTask = new Mock<eAdaptorOutboundServiceTask> { CallBase = true };
			var job = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			job.Setup(m => m.Execute(It.IsAny<CancellationToken>())).Throws(new EndpointNotFoundException("No endpoint is listening."));
			job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
			serviceTask.Setup(m => m.Notifier).Returns(notifier);
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

			if (returnSuccessCodeForLocalHost)
			{
				var url = "http://localhost/";
				var diagnosticsStep = new Mock<DiagnosticsStep>(url) { CallBase = true };
				diagnosticsStep.Setup(m => m.GetResponse(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpClient>())).Returns(new HttpResponseMessage(HttpStatusCode.OK));
				var diagnoster = new Mock<eAdaptorOutboundCommunicationDiagnoster>(url) { CallBase = true };
				diagnoster.Setup(m => m.CreateNewDiagnosticsStep(url)).Returns(diagnosticsStep.Object);
				serviceTask.Protected().Setup<IEHubCommunicationDiagnoster>("EHubCommunicationDiagnoster").Returns(diagnoster.Object);
			}

			serviceTask.Object.RunTask();
		}

		public void TestCompanySettingsManagerInitialisedOnce()
		{
			var service = new eAdaptorOutboundServiceTaskForTest();

			var settings1 = service.CompanySettingsManager;
			var settings2 = service.CompanySettingsManager;

			AssertEquals(settings1, settings2);
		}

		public void TestUnexpectedExceptionHandling()
		{
			TestExceptionHandling(new NullReferenceException("Unexpected Exception"), "Unexpected Exception", hasErrorReport: true);
		}

		[TestDate(2016, 06, 12)]
		public override void TestConfigurationErrorsExceptionHandling()
		{
			TestExceptionHandling(new ConfigurationErrorsException(configurationErrorMessage), configurationErrorMessage, hasErrorReport: true);
		}

		[TestDate(2016, 06, 12)]
		public override void TestJobExceptionDoesNotAffectOtherJobs()
		{
			JobExceptionDoesNotAffectOtherJobs(true);
		}

		[TestDate(2016, 06, 12)]
		public override void TestStop()
		{
			TestExceptionHandling(new OperationCanceledException(), "The operation was canceled.", hasErrorReport: true);
		}

		[TestDate(2016, 06, 12)]
		public override void TestArgumentExceptionWithoutSchemeMessageHandling()
		{
			TestExceptionHandling(new ArgumentException("Some other exception"), "Some other exception", hasErrorReport: true);
		}

		[SnailTest]
		public void TestRunWithProxy_eServices_EAM()
		{
			SetUpForProxyTest();
			var original = WebRequest.DefaultWebProxy;
			try
			{
				var eAdaptorServerAddress = "syd-wtst-1";
				var eHubServerAddress = eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://" + eAdaptorServerAddress + "/ehubgateway/ehubstreamedservice.svc");
				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.eAdaptorQueued;
				interchange.EI_TransportType = EDIInterchange.TransportType.eAdaptor;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_SessionGUID = interchange.PK;
				interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				interchange.EI_ApplicationCode = "XMS";
				interchange.EI_BodyText = new StreamReader(XmlMessageHelperTest.GetMesageStream("EVT")).ReadToEnd();
				var message = Factory.NewWithValidTestData<EDIMessage>();
				message.EM_EI = interchange.PK;
				message.EM_MessageType = "XMS";
				message.EM_MessageSubType = "EVT";

				var hostAddressesFromProxy = GetHostAddressesFromProxy("EAM");
				AssertEquals(string.Format("Url '{0}' was accessed the wrong number of times through the test proxy. There should be 2 from the initial and diagnostic request.", eAdaptorServerAddress),
					2, hostAddressesFromProxy[eAdaptorServerAddress]);
				AssertEquals(string.Format("Url '{0}' was accessed the wrong number of times through the test proxy. There should be 1 from the diagnostic test.", diagnosticTestURL),
					1, hostAddressesFromProxy[diagnosticTestURL]);
				Assert(string.Format("Url '{0}' was accessed the wrong number of times through the test proxy. There should be 0 from the diagnostic test.", eHubServerAddress),
					!hostAddressesFromProxy.TryGetValue(eHubServerAddress, out _));

				ErrorReporter.Clear();
			}
			finally
			{
				WebRequest.DefaultWebProxy = original;
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"eAdaptor Outbound",
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=TRX",
						EDIInterchangeSchema.Constants.EI_Status + "=AQU"),
				};
			}
		}

		protected override IAdaptorFactory CreateAdaptorFactory() => new eAdaptorFactory();

		void SetupMockEAdaptorOutboundServiceTask(NotificationBuffer notifier, Mock<eAdaptorOutboundServiceTask> serviceTask, AdaptorFactoryMock adaptorFactory, EDICommunicationParty communicationParty = null)
		{
			var company1 = ServiceTaskJobWithAdapterTests<ServiceTaskJobWithAdapter>.CreateCompanyWithBranch(Factory);
			var job = new Mock<eAdaptorOutboundServiceTaskJob>(serviceTask.Object, notifier, adaptorFactory, new ZQueryFactoryForTest(), new DynamicBusinessObjectCollectionFactoryForTest()) { CallBase = true };
			var interchange1 = OutboundEDIInterchangesServiceTaskJobTests<eAdaptorOutboundServiceTaskJob>.CreateTestInterchange(Factory, EDIInterchange.Status.eAdaptorQueued, company1, "RC1");
			OutboundEDIInterchangesServiceTaskJobTests<eAdaptorOutboundServiceTaskJob>.SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eAdaptorQueued);
			if (communicationParty != null)
			{
				interchange1.EI_ECC_CommunicationPartyConfig = communicationParty.OutboundConfig.PK;
			}
			Factory.Save();

			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
			serviceTask.Setup(m => m.Notifier).Returns(notifier);
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
		}

		class Notifier : INotifications
		{
			public void Add(INotification notification)
			{
				AssertEquals(ExpectedNotification, notification.Message);
			}
			public string ExpectedNotification { get; set; }
		}

		class eAdaptorOutboundServiceTaskForTest : eAdaptorOutboundServiceTask
		{
			public void ExposeHandleCommunicationExceptionForTest(Exception innerException)
			{
				HandleCommunicationException(new CommunicationException("Comm.Err", innerException));
			}
		}

		class ErrorResponseOutboundAdaptorFactory : AdaptorFactoryMock
		{
			public override IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
			{
				var configuration = new Mock<IServiceConfiguration>();
				var adapter = new Mock<eHubAdapter>(configuration.Object, "SenderID", "Password", true) { CallBase = true };

				var serviceMock = new Mock<eHubStreamedService>();
				serviceMock.Setup(service => service.SendStream(It.IsAny<SendStreamRequest>())).Callback((SendStreamRequest request) => throw new ArgumentException("I am an Exception mockup"));
				adapter.Protected().Setup<eHubStreamedService>("CreateService", configuration.Object, "SenderID", "Password").Returns(serviceMock.Object);

				return adapter.Object;
			}
		}
	}
}
