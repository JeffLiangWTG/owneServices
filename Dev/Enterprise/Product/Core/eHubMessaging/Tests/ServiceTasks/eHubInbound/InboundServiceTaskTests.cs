using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CargoWise.Common;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.eHubInbound
{
	[TestedType(typeof(InboundServiceTask))]
	class InboundServiceTaskTests : eHubServiceTaskWithAdaptorTest<InboundServiceTask, InboundServiceTaskJob>
	{
		public void TestRunTask_CorruptedInstallationException()
		{
			const string errorMessage = "Could not load type 'System.Runtime.Diagnostics.ITraceSourceStringProvider' from assembly 'System.ServiceModel.Internals, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'.";

			var serviceTaskJob = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			serviceTaskJob.Setup(m => m.Execute(It.IsAny<CancellationToken>())).Throws(new TypeLoadException(errorMessage));
			var mockServiceTask = CreateMock();
			mockServiceTask.Setup(s => s.GetJobs()).Returns(new[] { serviceTaskJob.Object });

			AssertExceptionThrown<TypeLoadException>(errorMessage, () => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
		}

		public void TestServerAddress()
		{
			var expectedServerAdress = "www.wisetechglobal.com";
			using (eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedServerAdress))
			{
				var adaptorFactory = new InboundAdaptorFactoryMock(expectedServerAdress);
				var mockServiceTask = new Mock<InboundServiceTask>(adaptorFactory, new EHubCommunicationDiagnosterFactory()) { CallBase = true };
				var actualCompanySettingsManager = CreateMockCompanySettingsManager();
				mockServiceTask.Setup(m => m.RunContinuously).Returns(false);
				mockServiceTask.Setup(m => m.IsCargoWiseDomain).Returns(true);
				mockServiceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
				mockServiceTask.Setup(m => m.CompanySettingsManager).Returns(actualCompanySettingsManager.Object);
				mockServiceTask.Setup(m => m.IsProduction).Returns(false);

				InitialiseAndRunTaskSchedule(mockServiceTask.Object);
				AssertEquals("Wrong number of Adapters created", 1, adaptorFactory.Adapters.Length);
			}
		}

		public void TestReceiveFromProdGatewayRequirement()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.EHI, ServiceTaskNames.EHubInboundMessages, "ESV", typeof(InboundServiceTask)));

			using (eHubMessagingRegistry.Instance.eHubEnableReceiveFromProductionGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(serviceConfig.SatisfiesRequirements());
			}

			using (eHubMessagingRegistry.Instance.eHubEnableReceiveFromProductionGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!serviceConfig.SatisfiesRequirements());
			}
		}

		public void TestNudgedAttribute_NudgeURLConfigured_ReturnsTrue()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.EHI, ServiceTaskNames.EHubInboundMessages, "ESV", typeof(InboundServiceTask)));
			using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeNudgeURL\\ServiceTaskNudging"))
			{
				Assert(serviceConfig.IsConfiguredForNudging);
			}
		}
		public void TestNudgedAttribute_NudgeURLNotConfigured_ReturnsFalse()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.EHI, ServiceTaskNames.EHubInboundMessages, "ESV", typeof(InboundServiceTask)));
			using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				Assert(!serviceConfig.IsConfiguredForNudging);
			}
		}

		protected override void TestExceptionHandling(Exception e, string logMessage, bool logExists = true, bool hasErrorReporter = false, bool shouldContainDiagnostics = false, bool shouldReportCommunicationExceptionsAsIssues = false, bool isProduction = false)
		{
			base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: true, isProduction);
		}

		protected override void SetOutageStartTime(InboundServiceTask serviceTask, DateTime startTime)
		{
			serviceTask.OutageStartTime = startTime;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[SnailTest]
		public void TestRunWithProxy_eServices_EHI()
		{
			SetUpForProxyTest();
			var original = WebRequest.DefaultWebProxy;
			try
			{
				var eHubServerAddress = "test-1.net";
				eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eHubServerAddress);
				var hostAddressesFromProxy = GetHostAddressesFromProxy("EHI");
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
