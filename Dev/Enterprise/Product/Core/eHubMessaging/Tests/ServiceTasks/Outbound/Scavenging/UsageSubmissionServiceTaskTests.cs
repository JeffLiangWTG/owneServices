using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	[TestedType(typeof(UsageSubmissionServiceTask))]
	class UsageSubmissionServiceTaskTests : ScavengingSubmissionServiceTaskTest<UsageSubmissionServiceTask, BillingJob, StmUsageData, LightweightOutboundBillingItem>
	{
		public void TestHostedServiceAttributeParameters()
		{
			// Arrange
			var attributes = typeof(UsageSubmissionServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>();

			// Act
			var result = attributes.Single(attribute => attribute.Code == ServiceTaskCodes.USS);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("ESV", result.Category);
				AssertEquals(typeof(UsageSubmissionServiceTask), result.Type);
			});
		}

		public void TestRunTask_ProcessMultipleBatchesOfFailedMessages()
		{
			var numberOfRecordsWithNoCategory = BillingJob.BatchSize + 1;
			for (int i = 0; i < numberOfRecordsWithNoCategory; i++)
			{
				CreateBillingTransactionItem(BillingManager.NoCategory, "TST", false, "Some XML");
			}
			Factory.Save();

			var mock = new MockRepository(MockBehavior.Default);
			var serviceTask = new Mock<UsageSubmissionServiceTask>() { CallBase = true };
			var notifier = new NotificationBuffer();
			serviceTask.Setup(m => m.Notifier).Returns(notifier);
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);

			var cmdCountBefore = Db.Connection.ExecutedCommandCount;
			serviceTask.Object.RunTask();
			var cmdCountAfter = Db.Connection.ExecutedCommandCount;

			AssertEquals("NotFailedCategory [" + BillingManager.NoCategory + "]", ErrorReporter.LastKeyReported);
			AssertEquals("XML: [Some XML]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			var query = new ZQuery(StmUsageDataSchema.SUD_Fail, true);
			AssertEquals("Service Task failed to process multiple batches of failed records", numberOfRecordsWithNoCategory, Factory.GetDatabaseCount(BillingManager.StmUsageDataType, query));
			AssertLessThan("Too many DB Hits used when processing messages", cmdCountAfter, cmdCountBefore + 36);
		}

		public void TestRunTaskDisabled()
		{
			var configType = Factory.New<Integration.Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "STOPSTLUSS";
			configType.ZRT_Description = "Stop collecting and submitting usage data";
			configType.ZRT_LongDescription = "Stop collecting and submitting usage data";
			var config = Factory.New<Integration.Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = "STOPSTLUSS";
			config.ZRC_BitValue = true;
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			Factory.Save();

			var serviceTask = new Mock<UsageSubmissionServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);
			serviceTask.Object.ServiceLogger = new TestServiceLogger();
			serviceTask.Setup(m => m.Notifier).Returns(notifier.Object);
			notifier.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new Action<INotification>(notification =>
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					AssertEquals("Usage submission has been disabled through reference data.", notification.Message);
				}));
			serviceTask.Object.RunTask();
			serviceTask.Verify();
			notifier.VerifyAll();
		}

		public void TestNonProductionSystemSatisfiesRequirements()
		{
			var serviceConfig = new HostedServiceConfiguration(new HostedServiceAttribute(ServiceTaskCodes.USS, ServiceTaskNames.UsageSubmissionService, "ESV", typeof(UsageSubmissionServiceTask)));
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

		public void TestUsageServerAddress_HostedWithWiseCloud()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.SetIsInternalSystemForTest(false);
			AssertEquals(true, EnvProxy.IsHostedWithCargowise);
			TestRunTask_SubmitsToCorrectGatewayAddress(SystemDataRegistry.Instance.UsageBillingGateway.Value,false, EnvProxy.IsHostedWithCargowise);
			TestRunTask_SubmitsToCorrectGatewayAddress(SystemDataRegistry.Instance.UsageBillingTestGateway.Value, true, EnvProxy.IsHostedWithCargowise);
		}

		public void TestUsageServerAddressSystem_NotHostedWithWiseCloud()
		{
			EnvProxy.SetHostedLocationForTest(LicenceConstants.NotHostedWithCargoWise);
			EnvProxy.SetIsInternalSystemForTest(true);
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			TestRunTask_SubmitsToCorrectGatewayAddress(eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.Value, false, EnvProxy.IsHostedWithCargowise);
			TestRunTask_SubmitsToCorrectGatewayAddress(eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.Value, true, EnvProxy.IsHostedWithCargowise);
		}

		void TestRunTask_SubmitsToCorrectGatewayAddress(string expectedGateway, bool isInternalSystem, bool hostedWithWiseCloud)
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(isInternalSystem);
			productRegistrationMock.Setup(m => m.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				CreateTestDataForServerAddress();

				var serviceTask = new Mock<UsageSubmissionServiceTask>() { CallBase = true };
				var notifier = new NotificationBuffer();

				serviceTask.Setup(m => m.Notifier).Returns(notifier);
				serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
				serviceTask.Setup(m => m.IsWiseCloudHosted).Returns(hostedWithWiseCloud);

				serviceTask.Object.RunTask();

				AssertEquals(expectedGateway, serviceTask.Object.DefaultServerAddress);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmUsageDataSchema.Constants.TableName,
						"Usage Submission",
						StmUsageDataSchema.Constants.SUD_Category + "!=",
						StmUsageDataSchema.Constants.SUD_Code + "!=",
						StmUsageDataSchema.Constants.SUD_Fail + "=false",
						StmUsageDataSchema.Constants.SUD_Fixing + "=false"),
				};
			}
		}

		protected override void AssertErrorEmail(string containsMessage)
		{
			TestHelpers.AssertNoErrorEmail(); // Usage Submission should have no emails
		}

		protected override void CreateTestDataForServerAddress()
		{
			var encoding = new UTF8Encoding(false);
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Data = BillingDataEncryptor.Encrypt(encoding.GetBytes("TS0"));
			stmUsageData.SUD_Category = "TST";
			stmUsageData.SUD_Code = "TST";
			stmUsageData.SUD_SubmissionPriority = 4;
			stmUsageData.SUD_PostedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
		}

		protected override void TestExceptionHandling(Exception e, string logMessage, bool logExists = true, bool hasErrorReporter = false, bool shouldContainDiagnostics = false, bool shouldReportCommunicationExceptionsAsIssues = false, bool isProduction = false)
		{
			base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: true, isProduction: true);
			base.TestExceptionHandling(e, logMessage, logExists, hasErrorReporter, shouldContainDiagnostics, shouldReportCommunicationExceptionsAsIssues: false, isProduction: false);
		}

		protected override void SetOutageStartTime(UsageSubmissionServiceTask serviceTask, DateTime startTime)
		{
			serviceTask.OutageStartTime = startTime;
		}

		BusinessObject CreateBillingTransactionItem(string category, string priceItemCode, bool failed, string data)
		{
			var result = Factory.New(BillingManager.StmUsageDataType);
			result[StmUsageDataSchema.Constants.SUD_Schema] = BillingManager.CurrentSchemaVersion;
			result[StmUsageDataSchema.Constants.SUD_Category] = category;
			result[StmUsageDataSchema.Constants.SUD_Code] = priceItemCode;
			result[StmUsageDataSchema.Constants.SUD_Data] =
				BillingDataEncryptor.Encrypt(BillingManager.Encoding.GetBytes(data));
			result[StmUsageDataSchema.Constants.SUD_Fail] = failed;
			return result;
		}
	}
}
