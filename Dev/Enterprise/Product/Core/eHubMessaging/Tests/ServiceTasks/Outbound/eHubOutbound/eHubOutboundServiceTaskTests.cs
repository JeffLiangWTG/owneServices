using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.eHubOutbound
{
	[TestedType(typeof(eHubOutboundServiceTask))]
	class eHubOutboundServiceTaskTests : eHubServiceTaskWithAdaptorTest<eHubOutboundServiceTask, eHubOutboundServiceTaskJob>
	{
		public void TestSystemMessageAlwaysFirst()
		{
			var serviceTask = CreateMock();
			var jobs = serviceTask.Object.GetJobs().OfType<eHubOutboundServiceTaskJob>().ToList();

			Assert("System interchanges only", jobs[0].ProcessSystemInterchanges);
			Assert("Non-system interchanges only", !jobs[1].ProcessSystemInterchanges);
		}

		[TestDate(2016, 06, 12)]
		public new void TestCommunicationExceptionHandling()
		{
			var invalidOperationException = new InvalidOperationException(
				"There is already an open DataReader associated with this Command which must be closed first.");
			invalidOperationException.Source = "System.Data";
			TestExceptionHandling(new CommunicationException("Unable to execute command on the database", invalidOperationException), "", logExists: false);
			TestExceptionHandling(
				new CommunicationException("timeout", new WebException("The underlying connection was closed")),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new IOException("Unable to write data to the transport connection", new SocketException(10054))),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new IOException("Unable to read data from the transport connection: The connection was closed.")),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
			TestExceptionHandling(
				new CommunicationException("The socket connection was aborted", new SocketException(10054)),
				LogMessages.TimeoutMessage, shouldContainDiagnostics: true);
		}

		public void TestAllowMultipleInstances()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				AssertEquals("AllowsMultipleInstances", true, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		public void TestServerAddressProductioneHub()
		{
			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.wisetechglobal.com");
			}
		}

		public void TestServerAddressTesteHubOnProductionSystem()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);
			keyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			keyMock.Setup(m => m.EnterpriseCode).Returns("BLA");
			keyMock.Setup(m => m.ServerCode).Returns("BLA");

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.wisetechglobal.com");
			}
		}

		public void TestServerAddressTesteHub()
		{
			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.google.com");
			}
		}

		protected override void TestExceptionHandling(Exception e, string logMessage, bool logExists = true, bool hasErrorReporter = false, bool shouldContainDiagnostics = false, bool shouldReportCommunicationExceptionsAsIssues = false, bool isProduction = false)
		{
			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: true, isProduction: false);
			}

			using (eHubMessagingRegistry.Instance.eHubSendInterchangesToTestGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: false, isProduction: false);
				base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: true, isProduction: true);
			}
		}

		protected override void SetOutageStartTime(eHubOutboundServiceTask serviceTask, DateTime startTime)
		{
			serviceTask.OutageStartTime = startTime;
		}

		void AssertServerAddress(string productionAddress, string testAddress, string expectedServerAddress)
		{
			using (eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testAddress))
			using (eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, productionAddress))
			{
				var serviceTask = new Mock<eHubOutboundServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();
				var adaptorFactory = new OutboundAdaptorFactoryMock(expectedServerAddress);
				var company1 = ServiceTaskJobWithAdapterTests<ServiceTaskJobWithAdapter>.CreateCompanyWithBranch(Factory);
				var job = new Mock<eHubOutboundServiceTaskJob>(serviceTask.Object, notifier, adaptorFactory, new ZQueryFactoryForTest(), new DynamicBusinessObjectCollectionFactoryForTest()) { CallBase = true };
				var interchange1 = OutboundEDIInterchangesServiceTaskJobTests<eHubOutboundServiceTaskJob>.CreateTestInterchange(Factory, EDIInterchange.Status.eHubQueued, company1, "RC1");
				OutboundEDIInterchangesServiceTaskJobTests<eHubOutboundServiceTaskJob>.SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);
				Factory.Save();

				serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
				serviceTask.Setup(m => m.Notifier).Returns(notifier);
				serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

				serviceTask.Object.RunTask();

				AssertEquals("Wrong number of Adapters created after first execute", 1, adaptorFactory.Adapters.Length);
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
						ServiceTaskNames.EHubOutboundMessages,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=TRX",
						EDIInterchangeSchema.Constants.EI_Status + "=HQU"),
				};
			}
		}

		[SnailTest]
		public void TestRunWithProxy_eServices_EHO()
		{
			SetUpForProxyTest();
			var original = WebRequest.DefaultWebProxy;
			try
			{
				var eHubServerAddress = "test-2.net";
				eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eHubServerAddress);
				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.eHubQueued;
				interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_SessionGUID = interchange.PK;
				interchange.EI_ApplicationCode = "XMS";
				interchange.EI_BodyText = new StreamReader(XmlMessageHelperTest.GetMesageStream("EVT")).ReadToEnd();
				var message = Factory.NewWithValidTestData<EDIMessage>();
				message.EM_EI = interchange.PK;
				message.EM_MessageType = "XMS";
				message.EM_MessageSubType = "EVT";

				var hostAddressesFromProxy = GetHostAddressesFromProxy("EHO");
				AssertEquals(string.Format("Url '{0}' was accessed the wrong number of times through the test proxy. There should be 1 from the initial request, and 1 more from the diagnostic test.", eHubServerAddress),
					2, hostAddressesFromProxy[eHubServerAddress]);
				AssertEquals(string.Format("Url '{0}' was accessed the wrong number of times through the test proxy. There should be 1 from the diagnostic test.", diagnosticTestURL),
					1, hostAddressesFromProxy[diagnosticTestURL]);
				ErrorReporter.Clear();
			}
			finally
			{
				WebRequest.DefaultWebProxy = original;
			}
		}
	}
}
