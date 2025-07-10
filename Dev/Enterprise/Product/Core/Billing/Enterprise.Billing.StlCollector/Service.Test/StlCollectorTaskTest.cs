using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDataRepo.Ent.Client;
using CargoWise.Types;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Billing.StlCollector.Service.Testing
{
	[TestedType(typeof(StlCollectorTask))]
	internal class StlCollectorTaskTest : ServiceTaskTestCase<StlCollectorTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new StlCollectorTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.HoursRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime - Default", ZDateTime.Empty, taskSchedule.Recurrence.CalcDailyStartTimeUtc);
		}

		public void TestNonProductionSystemSatisfiesRequirements()
		{
			var serviceconfig = new HostedServiceConfiguration(new HostedServiceAttribute(StlCollectorTask.ServiceTaskCode, "STL Collector Service", "SYS", typeof(StlCollectorTask)));

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				Assert(serviceconfig.SatisfiesRequirements());
			}
		}

		public void TestProductionSystemDoesNotRequireeHubTestingEnabled()
		{
			AssertSystemDoesNotRequireeHubTestingEnabled(DatabaseTypes.Codes.Production);
		}

		public void TestTestSystemDoesNotRequireeHubTestingEnabled()
		{
			AssertSystemDoesNotRequireeHubTestingEnabled(DatabaseTypes.Codes.Test);
		}

		public void TestTrainingSystemDoesNotRequireeHubTestingEnabled()
		{
			AssertSystemDoesNotRequireeHubTestingEnabled(DatabaseTypes.Codes.Training);
		}

		void AssertSystemDoesNotRequireeHubTestingEnabled(string databaseType)
		{
			using (RetrieverTestHelper.MockProductRegistration(databaseType, isInternalSystem: false))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var retrieverFactoryMock = new Mock<IStlRetrieverFactory>();
				retrieverFactoryMock.Setup(m => m.Create(It.IsAny<ILogger>())).Returns(new Mock<IStlRetriever>().Object);
				var testTask = new StlCollectorTask(retrieverFactoryMock.Object);
				var logger = InitialiseAndRunTaskSchedule(testTask);
				AssertEquals("Two log entries", 2, logger.Count);
				AssertEquals("Information|[STL Collection] - Start", logger[0]);
				AssertEquals("Information|[STL Collection] - End", logger[1]);
			}
		}

		public void TestInternalSystemRequireseHubTestingEnabled()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test, isInternalSystem: true))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testTask = new StlCollectorTask();
				var logger = InitialiseAndRunTaskSchedule(testTask);
				AssertEquals("Just the one log entry", 1, logger.Count);
				AssertEquals("Information|STL Collection is Disabled, as this is not a production system. Please turn on System->Testing->eHub Testing registry explicitly and leave System->Remote Database Repository-> Service Uri registry at it's default value if you want to submit STL collection data to test eHub.", logger[0]);
			}
		}

		public void TestInternalSystemRequiresDefaultReferenceDatabase()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Test, isInternalSystem: true))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RemoteDatabaseRegistry.Instance.SingleRefDatabaseName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyReferenceDatabase"))
			using (RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "MyReferenceDatabase.com"))
			{
				var testTask = new StlCollectorTask();
				var logger = InitialiseAndRunTaskSchedule(testTask);
				AssertEquals("Just the one log entry", 1, logger.Count);
				AssertEquals("Information|STL Collection is Disabled, as this is not a production system. Please turn on System->Testing->eHub Testing registry explicitly and leave System->Remote Database Repository-> Service Uri registry at it's default value if you want to submit STL collection data to test eHub.", logger[0]);
			}
		}

		public void TestRunTaskDisabled()
		{
			var configType = Factory.New<Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "STOPSTLUSS";
			configType.ZRT_Description = "Stop collecting and submitting usage data";
			configType.ZRT_LongDescription = "Stop collecting and submitting usage data";
			var config = Factory.New<Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = "STOPSTLUSS";
			config.ZRC_BitValue = true;
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			Factory.Save();

			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (RawDataRegistry.Instance.EHubTesting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testTask = new StlCollectorTask();
				var logger = InitialiseAndRunTaskSchedule(testTask);
				AssertEquals("Just the one log entry", 1, logger.Count);
				AssertEquals("Information|STL collection has been disabled through reference data.", logger[0]);
			}
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
