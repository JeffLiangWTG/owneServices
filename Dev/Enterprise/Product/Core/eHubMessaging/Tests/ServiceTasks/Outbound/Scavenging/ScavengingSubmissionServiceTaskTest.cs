using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	[TestsSubclassesOf(typeof(ScavengingSubmissionServiceTask))]
	abstract class ScavengingSubmissionServiceTaskTest<TScavengingSubmissionServiceTask, TScavengingSubmissionJob, TOuboundItem, TLightweightOutboundItem> : eHubServiceTaskWithAdaptorTest<TScavengingSubmissionServiceTask, TScavengingSubmissionJob>
		where TScavengingSubmissionServiceTask : ScavengingSubmissionServiceTask
		where TOuboundItem : BusinessObject
		where TLightweightOutboundItem : LightweightOutboundItem<TOuboundItem>
		where TScavengingSubmissionJob : ScavengingSubmissionJob<TOuboundItem, TLightweightOutboundItem>
	{
		public void TestServerAddressProductionSystem()
		{
			AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.wisetechglobal.com", DatabaseTypes.Codes.Production);
		}

		public void TestServerAddressTestSystem()
		{
			AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.wisetechglobal.com", DatabaseTypes.Codes.Test);
		}

		public void TestServerAddressTrainingSystem()
		{
			AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.wisetechglobal.com", DatabaseTypes.Codes.Training);
		}

		public void TestServerAddressInternalSystem()
		{
			AssertServerAddress("www.wisetechglobal.com", "www.google.com", "www.google.com", DatabaseTypes.Codes.Test, isInternalSystem: true);
		}

		void AssertServerAddress(string productionAddress, string testAddress, string expectedServerAddress, string databaseType, bool isInternalSystem = false)
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(isInternalSystem);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);
			keyMock.Setup(m => m.DatabaseType).Returns(databaseType);
			keyMock.Setup(m => m.EnterpriseCode).Returns("BLA");
			keyMock.Setup(m => m.ServerCode).Returns("BLA");

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testAddress))
			using (eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, productionAddress))
			{
				CreateTestDataForServerAddress();

				var serviceTask = new Mock<TScavengingSubmissionServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();
				var adaptorFactory = new OutboundAdaptorFactoryMock(expectedServerAddress);
				var job = new Mock<TScavengingSubmissionJob>(serviceTask.Object, notifier, adaptorFactory) { CallBase = true };
				AdditionalServiceTaskJobSetup(job);

				serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
				serviceTask.Setup(m => m.Notifier).Returns(notifier);
				serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

				serviceTask.Object.RunTask();

				AssertEquals("Wrong number of Adapters created after first execute", 1, adaptorFactory.Adapters.Length);
			}
		}

		protected abstract void CreateTestDataForServerAddress();

		protected virtual void AdditionalServiceTaskJobSetup(Mock<TScavengingSubmissionJob> mockServiceTaskJob) { }
	}
}
