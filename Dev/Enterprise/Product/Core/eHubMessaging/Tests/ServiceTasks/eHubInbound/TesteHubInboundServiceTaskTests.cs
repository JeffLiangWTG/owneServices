using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.eHubInbound
{
	[TestedType(typeof(TesteHubInboundServiceTask))]
	class TesteHubInboundServiceTaskTests : eHubServiceTaskWithAdaptorTest<TesteHubInboundServiceTask, InboundServiceTaskJob>
	{
		public void TestServerAddress()
		{
			var expectedServerAdress = "www.wisetechglobal.com";
			using (eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedServerAdress))
			{
				var adaptorFactory = new InboundAdaptorFactoryMock(expectedServerAdress);
				var mockServiceTask = new Mock<TesteHubInboundServiceTask>(adaptorFactory, new EHubCommunicationDiagnosterFactory()) { CallBase = true };
				SetupMock(mockServiceTask);
				InitialiseAndRunTaskSchedule(mockServiceTask.Object);
				AssertEquals("Wrong number of Adapters created", 1, adaptorFactory.Adapters.Length);
			}
		}

		public void TestProductionSystemDoesNotSatisfyRequirements()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.THI, ServiceTaskNames.TestEHubInboundMessages, "ESV", typeof(TesteHubInboundServiceTask)));
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);
			keyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				Assert(!serviceConfig.SatisfiesRequirements());
			}
		}

		public void TestNonProductionSystemSatisfiesRequirements()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.THI, ServiceTaskNames.TestEHubInboundMessages, "ESV", typeof(TesteHubInboundServiceTask)));
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);
			keyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Test);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				Assert(serviceConfig.SatisfiesRequirements());
			}
		}

		public void TestNudgedAttribute_NudgeURLConfigured_ReturnsTrue()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.THI, ServiceTaskNames.EHubInboundMessages, "ESV", typeof(TesteHubInboundServiceTask)));
			using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeNudgeURL\\ServiceTaskNudging"))
			{
				Assert(serviceConfig.IsConfiguredForNudging);
			}
		}
		public void TestNudgedAttribute_NudgeURLNotConfigured_ReturnsFalse()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.THI, ServiceTaskNames.EHubInboundMessages, "ESV", typeof(TesteHubInboundServiceTask)));
			using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				Assert(!serviceConfig.IsConfiguredForNudging);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
