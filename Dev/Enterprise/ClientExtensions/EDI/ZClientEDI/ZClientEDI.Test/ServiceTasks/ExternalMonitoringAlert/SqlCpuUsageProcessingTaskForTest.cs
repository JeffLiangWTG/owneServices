using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Nest;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert.Testing
{
	[TestedType(typeof(SqlAlertProcessingTask))]
	[UseSqlBlockingObserver]
	[UseSnapshotProtection]
	class SqlCpuUsageProcessingTaskForTest : ServiceTaskTestCase<SqlAlertProcessingTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var dataReaderMock = CreateDataReaderMock();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object);
			var serviceTask = new SqlAlertProcessingTask(processor);
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;

			#region mock data
			var timeCollected = DateTime.Today.AddHours(3).AddMinutes(24);
			var system = AlertProcessorOfCpuUsageForTest.SystemMDM;
			var queryHash = "12345678901234567890";
			var occurances = 321;
			var cpuTime = int.MaxValue;
			var reads = 123456;
			var writes = 233333;
			var ruleId = AlertProcessorOfCpuUsageForTest.MDMSourceRuleId;
			var pk = Guid.NewGuid();
			var product = "CW1";
			var exeDate = DateTime.Today.AddDays(-1);

			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2);
				}

				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash,
					pk,
					timeCollected,
					occurances,
					reads,
					writes,
					cpuTime,
					product,
					exeDate,
					"19.11.11.111",
				});

				dataReaderMock.SetupSequence(dataTable);
			}
			#endregion

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => serviceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		protected override void TearDownCore()
		{
			AlertProcessingTaskTestHelper.TearDown();
			base.TearDownCore();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			AlertProcessingTaskTestHelper.SetUp();
		}

		Mock<IDataReader> CreateDataReaderMock()
		{
			var dataReaderMock = new Mock<IDataReader>();

			dataReaderMock.Setup(m => m.FieldCount).Returns(Columns.Count);
			for (var i = 0; i < Columns.Count; i++)
			{
				dataReaderMock.Setup(m => m.GetName(i)).Returns(Columns[i].Item1);
				dataReaderMock.Setup(m => m.GetFieldType(i)).Returns(Columns[i].Item2);
			}

			return dataReaderMock;
		}

		public void TestWorkItemCreation()
		{
			var dataReaderMock = CreateDataReaderMock();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object);
			var processingTask = new SqlAlertProcessingTask(processor);

			#region mock data

			var timeCollected = DateTime.Today.AddHours(3).AddMinutes(24);
			var system = AlertProcessorOfCpuUsageForTest.SystemMDM;
			var queryHash = "12345678901234567890";
			var occurances = 321;
			var cpuTime = int.MaxValue;
			var reads = 123456;
			var writes = 233333;
			var ruleId = AlertProcessorOfCpuUsageForTest.MDMSourceRuleId;
			var pk = Guid.NewGuid();
			var product = "CW1";
			var kibanaLink = "http://someuri.com?hash={0}";
			var exeDate = DateTime.Today.AddDays(-1);

			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2);
				}

				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash,
					pk,
					timeCollected,
					occurances,
					reads,
					writes,
					cpuTime,
					product,
					exeDate,
					"19.11.11.111",
				});

				dataReaderMock.SetupSequence(dataTable);
			}
			#endregion

			InitialiseTaskSchedule(processingTask);

			RunTaskSchedule(processingTask);

			var alertsCreated = processor.AlertsCreated.Where(x => x.MatchedRule.AlertName.Equals(SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, StringComparison.OrdinalIgnoreCase)).ToArray();
			AssertEquals("wrong number of alerts created", 1, alertsCreated.Length);
			var workItemNumber = alertsCreated.First().TargetKey;
			Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(workItemNumber));
			var workItem = GetWorkItem(workItemNumber);
			AssertNotEquals("workitem does not exist", null, workItem);
			AssertEquals(AlertProcessorOfCpuUsageForTest.ProductMDM, workItem.WKI_WorkItemType); // Product
			AssertEquals(AlertProcessorOfCpuUsageForTest.ProgramAreaMDM, workItem.WKI_WorkItemArea); // Product Area
			AssertEquals(AlertProcessorOfCpuUsageForTest.ModuleMDM, workItem.WKI_ActivityType); // Module
			AssertEquals(AlertProcessorOfCpuUsageForTest.ChangeTypePER, workItem.WKI_ActivitySubtype); // Change Type
			AssertEquals(AlertProcessorOfCpuUsageForTest.PriorityALP, workItem.WKI_Priority); // Priority

			AssertEndsWith("Summary must end with the query hash", queryHash, workItem.WKI_Summary);
			AssertEquals("Summary should have been truncated at max field length", 80, workItem.WKI_Summary.Length);

			var workItemDetails = workItem.WKI_Details.ToUTF8().ToString();

			AssertContains(new ZDateTime(timeCollected).ToString("u", CultureInfo.InvariantCulture), workItemDetails);
			AssertContains(system, workItemDetails);
			AssertContains(queryHash, workItemDetails);
			AssertContains(occurances.ToString("N0"), workItemDetails);
			AssertContains(cpuTime.ToString("N0"), workItemDetails);
			AssertContains(reads.ToString("N0"), workItemDetails);
			AssertContains(writes.ToString("N0"), workItemDetails);
			AssertContains(ruleId.ToString(), workItemDetails);
			AssertContains(product, workItemDetails);
			AssertContains(string.Format(kibanaLink, queryHash), workItemDetails);
		}

		public void TestForAlreadyFixed()
		{
			var dataReaderMock = CreateDataReaderMock();
			var releaseBuildContentMock = new Mock<IReleaseBuildContent>();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object, releaseBuildContentMock.Object);
			var processingTask = new SqlAlertProcessingTask(processor);

			#region mock data
			var queryHash2 = "12345678901234567899";
			var queryHash3 = "12345678901234567999";
			var queryHash4 = "12345678901234569999";
			var queryHash5 = "12345678901234599999";
			var queryHash6 = "12345678901234999999";
			var exeDate = DateTime.Today.AddDays(-1);
			var currentVersion = "19.11.11.111";

			var openedWI = Factory.NewWithValidTestData<NewWorkItem>();
			openedWI.WKI_Status = ProcessTaskStatusCodeList.Codes.Open;
			openedWI.WKI_Summary = "SQL CPU Usage - " + queryHash2;
			var openedTask = openedWI.WorkflowItems.Tasks.AddNew();
			openedTask.P9_Type = EDITaskTypes.TaskCheckin;
			openedTask.P9_Description = "ShelfName";
			openedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var openedWIGenPivot = Factory.New<GenPivot>();
			openedWIGenPivot.XX_Relation1ID = openedWI.PK;
			openedWIGenPivot.XX_Relation2ID = processor.CapabilityDOM.PK;
			openedWIGenPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			openedWIGenPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			openedWIGenPivot.XX_RelationType = "DBP";
			Factory.Save();
			releaseBuildContentMock.Setup(m => m.IsPatchedTo(It.Is<NewWorkItem>(w => w.PK == openedWI.PK), It.IsAny<Version>())).Returns(true);

			var closedWIPatched = Factory.NewWithValidTestData<NewWorkItem>();
			closedWIPatched.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			closedWIPatched.WKI_Summary = "SQL CPU Usage - " + queryHash3;
			var task = closedWIPatched.WorkflowItems.Tasks.AddNew();
			task.P9_Type = EDITaskTypes.TaskActiveCheckin;
			task.P9_Description = "ShelfName";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.P9_CompletedTime = exeDate;
			var closedWIPatchedGenPivot = Factory.New<GenPivot>();
			closedWIPatchedGenPivot.XX_Relation1ID = closedWIPatched.PK;
			closedWIPatchedGenPivot.XX_Relation2ID = processor.CapabilityDOM.PK;
			closedWIPatchedGenPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			closedWIPatchedGenPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			closedWIPatchedGenPivot.XX_RelationType = "DBP";
			Factory.Save();
			releaseBuildContentMock.Setup(m => m.IsPatchedTo(It.Is<NewWorkItem>(w => w.PK == closedWIPatched.PK), It.IsAny<Version>())).Returns(true);

			var closedWINotPatched = Factory.NewWithValidTestData<NewWorkItem>();
			closedWINotPatched.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			closedWINotPatched.WKI_Summary = "SQL CPU Usage - " + queryHash4;
			var closedTask = closedWINotPatched.WorkflowItems.Tasks.AddNew();
			closedTask.P9_Type = EDITaskTypes.TaskActiveCheckin;
			closedTask.P9_Description = "ShelfName";
			closedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			closedTask.P9_CompletedTime = exeDate.AddDays(-3);
			var closedWINotPatchedGenPivot = Factory.New<GenPivot>();
			closedWINotPatchedGenPivot.XX_Relation1ID = closedWINotPatched.PK;
			closedWINotPatchedGenPivot.XX_Relation2ID = processor.CapabilityDOM.PK;
			closedWINotPatchedGenPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			closedWINotPatchedGenPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			closedWINotPatchedGenPivot.XX_RelationType = "DBP";
			Factory.Save();
			releaseBuildContentMock.Setup(m => m.IsPatchedTo(It.Is<NewWorkItem>(w => w.PK == closedWINotPatched.PK), It.IsAny<Version>())).Returns(false);

			var canceledWINotPatchedWithPatchedPrerequisite = Factory.NewWithValidTestData<NewWorkItem>();
			canceledWINotPatchedWithPatchedPrerequisite.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			canceledWINotPatchedWithPatchedPrerequisite.WKI_Summary = "SQL CPU Usage - " + queryHash5;
			var canceledTask = canceledWINotPatchedWithPatchedPrerequisite.WorkflowItems.Tasks.AddNew();
			canceledTask.P9_Type = EDITaskTypes.TaskCheckin;
			canceledTask.P9_Description = "Canceled Task";
			canceledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			canceledTask.P9_CompletedTime = exeDate.AddDays(-3);
			var closedWINotPatchedWithPatchedPrerequisiteGenPivot = Factory.New<GenPivot>();
			closedWINotPatchedWithPatchedPrerequisiteGenPivot.XX_Relation1ID = canceledWINotPatchedWithPatchedPrerequisite.PK;
			closedWINotPatchedWithPatchedPrerequisiteGenPivot.XX_Relation2ID = processor.CapabilityDOM.PK;
			closedWINotPatchedWithPatchedPrerequisiteGenPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			closedWINotPatchedWithPatchedPrerequisiteGenPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			closedWINotPatchedWithPatchedPrerequisiteGenPivot.XX_RelationType = "DBP";
			ProcessJobHeader.GetForParent(closedWIPatched, Factory).MakePrerequisiteOf(ProcessJobHeader.GetForParent(canceledWINotPatchedWithPatchedPrerequisite, Factory));
			Factory.Save();
			releaseBuildContentMock.Setup(m => m.IsPatchedTo(It.Is<NewWorkItem>(w => w.PK == canceledWINotPatchedWithPatchedPrerequisite.PK), It.IsAny<Version>())).Returns(false);

			var canceledWINotPatchedWithNotPatchedPrerequisite = Factory.NewWithValidTestData<NewWorkItem>();
			canceledWINotPatchedWithNotPatchedPrerequisite.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			canceledWINotPatchedWithNotPatchedPrerequisite.WKI_Summary = "SQL CPU Usage - " + queryHash6;
			var canceledTask1 = canceledWINotPatchedWithNotPatchedPrerequisite.WorkflowItems.Tasks.AddNew();
			canceledTask1.P9_Type = EDITaskTypes.TaskCheckin;
			canceledTask1.P9_Description = "Canceled Task";
			canceledTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			canceledTask1.P9_CompletedTime = exeDate.AddDays(-3);
			var canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot = Factory.New<GenPivot>();
			canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot.XX_Relation1ID = canceledWINotPatchedWithNotPatchedPrerequisite.PK;
			canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot.XX_Relation2ID = processor.CapabilityDOM.PK;
			canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot.XX_Relation1TableCode = WorkItemSchema.Constants.Prefix;
			canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot.XX_Relation2TableCode = GlbCapabilitySchema.Constants.Prefix;
			canceledWINotPatchedWithNotPatchedPrerequisiteGenPivot.XX_RelationType = "DBP";
			ProcessJobHeader.GetForParent(closedWIPatched, Factory).MakePrerequisiteOf(ProcessJobHeader.GetForParent(canceledWINotPatchedWithNotPatchedPrerequisite, Factory));
			ProcessJobHeader.GetForParent(closedWINotPatched, Factory).MakePrerequisiteOf(ProcessJobHeader.GetForParent(canceledWINotPatchedWithNotPatchedPrerequisite, Factory));
			Factory.Save();
			releaseBuildContentMock.Setup(m => m.IsPatchedTo(It.Is<NewWorkItem>(w => w.PK == canceledWINotPatchedWithNotPatchedPrerequisite.PK), It.IsAny<Version>())).Returns(false);

			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2);
				}

				// The hash code matches an open WI. A WI should NOT be generated for this.
				dataTable.Rows.Add(new object[] {
					AlertProcessorOfCpuUsageForTest.SystemDOM,
					AlertProcessorOfCpuUsageForTest.DOMSourceRuleId,
					queryHash2,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate.AddDays(-2),
					currentVersion,
				});

				// The hash code matches a previous closed WI but WI is already patched to the release version. A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					AlertProcessorOfCpuUsageForTest.SystemDOM,
					AlertProcessorOfCpuUsageForTest.DOMSourceRuleId,
					queryHash3,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate.AddDays(-2),
					currentVersion,
				});

				// The hash code matches a previous closed WI and WI is not yet patched to the release version. A WI should NOT be generated for this.
				dataTable.Rows.Add(new object[] {
					AlertProcessorOfCpuUsageForTest.SystemDOM,
					AlertProcessorOfCpuUsageForTest.DOMSourceRuleId,
					queryHash4,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate.AddDays(-2),
					currentVersion,
				});

				// The hash code matches a previous closed WI and WI has no check-in task with a prerequisite WI patched to the release version. A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					AlertProcessorOfCpuUsageForTest.SystemDOM,
					AlertProcessorOfCpuUsageForTest.DOMSourceRuleId,
					queryHash5,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate.AddDays(-2),
					currentVersion,
				});

				// The hash code matches a previous closed WI and WI has no check-in task with at least one prerequisite WI not patched to the release version. A WI should NOT be generated for this.
				dataTable.Rows.Add(new object[] {
					AlertProcessorOfCpuUsageForTest.SystemDOM,
					AlertProcessorOfCpuUsageForTest.DOMSourceRuleId,
					queryHash6,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate.AddDays(-2),
					currentVersion,
				});
				dataReaderMock.SetupSequence(dataTable);
			}

			#endregion

			InitialiseTaskSchedule(processingTask);

			const int NumberOfRepeats = 3;
			var repeat = -1;
			while (++repeat < NumberOfRepeats)
			{
				RunTaskSchedule(processingTask);

				var alertsCreated = processor.AlertsCreated.Where(x => x.MatchedRule.AlertName.Equals(SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, StringComparison.OrdinalIgnoreCase)).ToArray();
				AssertEquals("wrong number of alerts created", 2, alertsCreated.Length);

				// Assert closedWIPatched
				var closedWIPatchedNumber = alertsCreated[0].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(closedWIPatchedNumber));
				var newWorkItem = GetWorkItem(closedWIPatchedNumber);
				AssertNotEquals("workitem does not exist", null, newWorkItem);
				AssertEndsWith("Summary must end with the query hash", queryHash3, newWorkItem.WKI_Summary);

				// Assert WINotPatchedWithPatchedPrerequisite
				var canceledWINotPatchedWithPatchedPrerequisiteNumber = alertsCreated[1].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(canceledWINotPatchedWithPatchedPrerequisiteNumber));
				var newWorkItem1 = GetWorkItem(canceledWINotPatchedWithPatchedPrerequisiteNumber);
				AssertNotEquals("workitem does not exist", null, newWorkItem1);
				AssertEndsWith("Summary must end with the query hash", queryHash5, newWorkItem1.WKI_Summary);
			}
		}

		public void TestForExeDateDoesNotExist()
		{
			var dataReaderMock = CreateDataReaderMock();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object);
			var processingTask = new SqlAlertProcessingTask(processor);

			#region mock data

			var timeCollected = DateTime.Today.AddHours(3).AddMinutes(24);
			var system = AlertProcessorOfCpuUsageForTest.SystemINT;
			var queryHash = "12345678901234567890";
			var queryHash2 = "12345678901234567899";
			var occurances = 321;
			var cpuTime = 999888.432;
			var reads = 123456;
			var writes = 233333;
			var ruleId = AlertProcessorOfCpuUsageForTest.INTSourceRuleId;
			var someGUID = Guid.NewGuid();
			var product = "CW1";
			var exeDate = DateTime.Today.AddDays(-1);
			var currentVersion = "19.11.11.111";

			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2).AllowDBNull = true;
				}

				// A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash,
					someGUID,
					timeCollected,
					occurances,
					reads,
					writes,
					cpuTime,
					product,
					exeDate,
					currentVersion,
				});

				// The hash code is the same as the previous record but all other values are different and ExeDate is null. A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash2,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					DBNull.Value,
					currentVersion,
				});

				dataReaderMock.SetupSequence(dataTable);
			}

			#endregion

			InitialiseTaskSchedule(processingTask);

			const int NumberOfRepeats = 3;
			var repeat = -1;
			while (++repeat < NumberOfRepeats)
			{
				RunTaskSchedule(processingTask);

				var alertsCreated = processor.AlertsCreated.Where(x => x.MatchedRule.AlertName.Equals(SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, StringComparison.OrdinalIgnoreCase)).ToArray();
				AssertEquals("wrong number of alerts created", 2, alertsCreated.Length);

				// Assert WI1
				var workItemNumber1 = alertsCreated[0].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(workItemNumber1));
				var workItem1 = GetWorkItem(workItemNumber1);
				AssertNotEquals("workitem does not exist", null, workItem1);
				AssertEndsWith("Summary must end with the query hash", queryHash, workItem1.WKI_Summary);

				if (repeat == 0)
				{
					workItem1.WKI_Details = new ZBlob();
				}
				else
				{
					AssertNullOrEmpty("Details should have been empty. This was to ensure a new WI wasn't created beyond the first process run", workItem1.WKI_Details.ToUTF8().ToString());
				}

				// Assert WI2
				var workItemNumber2 = alertsCreated[1].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(workItemNumber2));
				var workItem2 = GetWorkItem(workItemNumber2);
				AssertNotEquals("workitem does not exist", null, workItem2);
				AssertEndsWith("Summary must end with the query hash", queryHash2, workItem2.WKI_Summary);

				var workItemDetails = workItem2.WKI_Details.ToUTF8().ToString();
				AssertContains("Workitem2 should be created based on 3rd record", "Customer", workItemDetails);
			}
		}

		public void TestForDuplicates()
		{
			var dataReaderMock = CreateDataReaderMock();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object);
			var processingTask = new SqlAlertProcessingTask(processor);

			#region mock data

			var timeCollected = DateTime.Today.AddHours(3).AddMinutes(24);
			var system = AlertProcessorOfCpuUsageForTest.SystemINT;
			var queryHash = "12345678901234567890";
			var queryHash2 = "12345678901234567899";
			var occurances = 321;
			var cpuTime = 999888.432;
			var reads = 123456;
			var writes = 233333;
			var ruleId = AlertProcessorOfCpuUsageForTest.INTSourceRuleId;
			var someGUID = Guid.NewGuid();
			var product = "CW1";
			var exeDate = DateTime.Today.AddDays(-1);
			var currentVersion = "19.11.11.111";

			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2);
				}

				// A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash,
					someGUID,
					timeCollected,
					occurances,
					reads,
					writes,
					cpuTime,
					product,
					exeDate,
					currentVersion,
				});

				// The hash code differs from the previous record but all other values are the same. A WI should be generated for this.
				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash2,
					someGUID,
					timeCollected,
					occurances,
					reads,
					writes,
					cpuTime,
					product,
					exeDate,
					currentVersion,
				});

				// The hash code is the same as the previous record but all other values are different. A WI should NOT be generated for this.
				dataTable.Rows.Add(new object[] {
					system,
					ruleId,
					queryHash2,
					Guid.NewGuid(),
					DateTime.Today.AddHours(1).AddMinutes(42),
					5,
					10,
					100,
					1000,
					"Customer",
					exeDate,
					currentVersion,
				});

				dataReaderMock.SetupSequence(dataTable);
			}

			#endregion

			InitialiseTaskSchedule(processingTask);

			const int NumberOfRepeats = 3;
			var repeat = -1;
			while (++repeat < NumberOfRepeats)
			{
				RunTaskSchedule(processingTask);

				var alertsCreated = processor.AlertsCreated.Where(x => x.MatchedRule.AlertName.Equals(SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, StringComparison.OrdinalIgnoreCase)).ToArray();
				AssertEquals("wrong number of alerts created", 2, alertsCreated.Length);

				// Assert WI1
				var workItemNumber1 = alertsCreated[0].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(workItemNumber1));
				var workItem1 = GetWorkItem(workItemNumber1);
				AssertNotEquals("workitem does not exist", null, workItem1);
				AssertEndsWith("Summary must end with the query hash", queryHash, workItem1.WKI_Summary);

				if (repeat == 0)
				{
					workItem1.WKI_Details = new ZBlob();
				}
				else
				{
					AssertNullOrEmpty("Details should have been empty. This was to ensure a new WI wasn't created beyond the first process run", workItem1.WKI_Details.ToUTF8().ToString());
				}

				// Assert WI2
				var workItemNumber2 = alertsCreated[1].TargetKey;
				Assert("alert was missing the workitem number", !string.IsNullOrWhiteSpace(workItemNumber2));
				var workItem2 = GetWorkItem(workItemNumber2);
				AssertNotEquals("workitem does not exist", null, workItem2);
				AssertEndsWith("Summary must end with the query hash", queryHash2, workItem2.WKI_Summary);
			}
		}

		public void TestCpuUsageAlertsMaxTargets()
		{
			var maxTargets = AlertProcessingTaskTestHelper.MaxTargets();
			var dataReaderMock = CreateDataReaderMock();
			var processor = new AlertProcessorOfCpuUsageForTest(dataReaderMock.Object);
			var processingTask = new SqlAlertProcessingTask(processor);

			#region mock data
			var exeDate = DateTime.Today.AddDays(-1);
			var random = new Random();
			using (var dataTable = new DataTable("CpuUsageAlerts"))
			{
				dataTable.Locale = CultureInfo.InvariantCulture;
				for (var i = 0; i < Columns.Count; i++)
				{
					dataTable.Columns.Add(Columns[i].Item1, Columns[i].Item2);
				}

				for (var n = 0; n < maxTargets * 2; n++)
				{
					dataTable.Rows.Add(new object[] {
						AlertProcessorOfCpuUsageForTest.SystemINT,
						AlertProcessorOfCpuUsageForTest.INTSourceRuleId,
						Guid.NewGuid().ToString(),
						Guid.NewGuid(),
						DateTime.Today.AddHours(random.Next(1,12)).AddMinutes(random.Next(1,59)),
						random.Next(100, 32767),
						random.Next(100, 32767),
						random.Next(100, 32767),
						random.NextDouble() * 1e6,
						Guid.NewGuid(),
						exeDate,
						"19.11.11.111",
					});
				}

				dataReaderMock.SetupSequence(dataTable);
			}

			#endregion

			InitialiseTaskSchedule(processingTask);

			const int NumberOfRepeats = 3;
			var repeat = -1;
			while (++repeat < NumberOfRepeats)
			{
				RunTaskSchedule(processingTask);

				var cpuUsageAlerts = processor.AlertsCreated.ToArray();
				AssertLessThanOrEqualTo($"No more than max targets: {maxTargets}", maxTargets, processor.AlertsCreated.Count);
			}
		}

		public void TestGetAlertsWithESResponse()
		{
			var alertDefaults = new List<AlertDefault>() { new AlertDefault() { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = "1" } };
			var capabilityCount = new Dictionary<string, int>();

			var alertProviderWithNullESResponse = new SqlCpuUsageAlertProviderForESResponseTest(Db.Connection, alertDefaults, capabilityCount, SearchResponseType.Null);
			var alertProviderWithFullESResponse = new SqlCpuUsageAlertProviderForESResponseTest(Db.Connection, alertDefaults, capabilityCount, SearchResponseType.FullObject);
			var alertProviderWithPartialESResponse = new SqlCpuUsageAlertProviderForESResponseTest(Db.Connection, alertDefaults, capabilityCount, SearchResponseType.PartialObject);

			var capabilitiesNotFull = new Mock<IEnumerable<string>>();
			var processAlertAction = new Mock<Func<BaseAlert, bool>>();
			processAlertAction.Setup(x => x.Invoke(It.IsAny<BaseAlert>())).Returns(true);

			AssertNoExceptionThrown(() =>
			{
				alertProviderWithNullESResponse.GetAlertAndProcess(capabilitiesNotFull.Object, processAlertAction.Object);
				alertProviderWithFullESResponse.GetAlertAndProcess(capabilitiesNotFull.Object, processAlertAction.Object);
				alertProviderWithPartialESResponse.GetAlertAndProcess(capabilitiesNotFull.Object, processAlertAction.Object);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		WorkItem GetWorkItem(string workItemNumber)
		{
			var query = new ZQuery(WorkItemSchema.WKI_WorkItemNumber, workItemNumber);
			var workItem = Factory.LoadTop1<WorkItem>(query);
			return workItem;
		}

		readonly List<Tuple<string, Type>> Columns = new List<Tuple<string, Type>>
		{
			new Tuple<string, Type>("CUY_System", typeof(string)),
			new Tuple<string, Type>("RuleId", typeof(Guid)),
			new Tuple<string, Type>("CUY_QueryHash", typeof(string)),
			new Tuple<string, Type>("CUY_PK", typeof(Guid)),
			new Tuple<string, Type>("CUY_CreatedDate", typeof(DateTime)),
			new Tuple<string, Type>("CUY_Qty", typeof(long)),
			new Tuple<string, Type>("CUY_Reads", typeof(long)),
			new Tuple<string, Type>("CUY_Writes", typeof(long)),
			new Tuple<string, Type>("CUY_CPUSeconds", typeof(decimal)),
			new Tuple<string, Type>("CUY_Owner", typeof(string)),
			new Tuple<string, Type>("CUY_ExeDate", typeof(DateTime)),
			new Tuple<string, Type>("CUY_CurrentVersion", typeof(string)),
		};
	}

	#region CPU Usage Alerts

	class SqlCpuUsageAlertProviderForTest : SqlCpuUsageAlertProvider
	{
		public SqlCpuUsageAlertProviderForTest(DbConnection connection, IEnumerable<AlertDefault> defaults, IDataReader dataReader, IReleaseBuildContent releaseBuildContent = null)
			: base(new DummyLogger(), connection, defaults)
		{
			this.dataReader = dataReader;
			this.releaseBuildContent = releaseBuildContent;
			sqlCpuUsageExecutionPlanUri = "http://someuri.com?hash={0}";
		}

		public override void GetAlertAndProcess(IEnumerable<string> capabilitiesNotFull, Func<BaseAlert, bool> processAlertAction)
		{
			using (var alertReader = GetAlerts())
			{
				var canceled = false;
				while (alertReader.Read() && !canceled)
				{
					var alert = new SqlCpuUsageAlert();
					ReadAlert(alert, alertReader);
					canceled = processAlertAction(alert);
				}
			}
		}

		IDataReader GetAlerts()
		{
			return dataReader;
		}

		protected override IReleaseBuildContent GetReleaseBuildContent()
		{
			return releaseBuildContent ?? base.GetReleaseBuildContent();
		}

		void ReadAlert(BaseAlert alert, IDataReader alertReader)
		{
			var cpuUsageAlert = alert as SqlCpuUsageAlert;
			cpuUsageAlert.AlertName = Convert.ToString(alertReader["CUY_System"], CultureInfo.InvariantCulture);
			cpuUsageAlert.RuleId = (Guid)alertReader["RuleId"];
			cpuUsageAlert.Path = Convert.ToString(alertReader["CUY_QueryHash"], CultureInfo.InvariantCulture);
			cpuUsageAlert.TimeAdded = (DateTime)alertReader["CUY_CreatedDate"];
			cpuUsageAlert.Quantity = Convert.ToInt64(alertReader["CUY_Qty"], CultureInfo.InvariantCulture);
			cpuUsageAlert.Reads = Convert.ToInt64(alertReader["CUY_Reads"], CultureInfo.InvariantCulture);
			cpuUsageAlert.Writes = Convert.ToInt64(alertReader["CUY_Writes"], CultureInfo.InvariantCulture);
			cpuUsageAlert.CpuMilliSeconds = Convert.ToInt64(alertReader["CUY_CPUSeconds"], CultureInfo.InvariantCulture);
			cpuUsageAlert.Owner = Convert.ToString(alertReader["CUY_Owner"], CultureInfo.InvariantCulture);
			var exeDate = alertReader["CUY_ExeDate"];
			if (exeDate is not DBNull)
			{
				cpuUsageAlert.ExeDate = (DateTime)exeDate;
			}
			cpuUsageAlert.CurrentVersion = Convert.ToString(alertReader["CUY_CurrentVersion"]);
		}

		public override void UpdateAlert(BaseAlert alert, string targetNumber, string owner)
		{
		}

		readonly IDataReader dataReader;
		readonly IReleaseBuildContent releaseBuildContent;
	}

	class AlertProcessorOfCpuUsageForTest : AlertProcessor
	{
		readonly IDataReader dataReader;
		readonly IReleaseBuildContent releaseBuildContent;
		public AlertProcessorOfCpuUsageForTest(IDataReader dataReader, IReleaseBuildContent releaseBuildContent = null) : base(new DummyLogger())
		{
			this.dataReader = dataReader;
			this.releaseBuildContent = releaseBuildContent;
			var factory = new BusinessObjectFactory();
			CapabilityINT = factory.NewWithValidTestData<GlbCapability>();
			CapabilityINT.G4_Code = CapabilityINTCode;
			CapabilityDOM = factory.NewWithValidTestData<GlbCapability>();
			CapabilityDOM.G4_Code = CapabilityDOMCode;
			CapabilityMDM = factory.NewWithValidTestData<GlbCapability>();
			CapabilityMDM.G4_Code = CapabilityMDMCode;
			factory.Save();
		}
		public GlbCapability CapabilityINT { get; private set; }
		public GlbCapability CapabilityDOM { get; private set; }
		public GlbCapability CapabilityMDM { get; private set; }

		#region INT

		[ThreadSafe]
		static readonly Guid CapabilityINTPk = Guid.Parse("75a166ae-52f1-40ea-97c1-aa9e708b9596");
		[ThreadSafe]
		public static Guid INTSourceRuleId = Guid.Parse("f27bd91e-27a8-4dd2-9be1-3b4e8e910ecf");
		public const string CapabilityINTCode = "IAD";
		public const string ProductINT = "ENT";
		public const string ModuleINT = "FOR";
		public const string ProgramAreaINT = "INT";
		public const string SystemINT = "Forwarding";

		#endregion

		#region DOM

		[ThreadSafe]
		public static Guid DOMSourceRuleId = Guid.Parse("54650981-7310-4144-8c51-eb1efe76524a");
		public const string CapabilityDOMCode = "3PL";
		public const string ProductDOM = "ENT";
		public const string ModuleDOM = "WAR";
		public const string ProgramAreaDOM = "DOM";
		public const string SystemDOM = "Warehouse";

		#endregion

		#region MDM

		[ThreadSafe]
		public static Guid MDMSourceRuleId = Guid.Parse("87192934-f63e-4fc9-b1cc-42bbd420589d");
		public const string CapabilityMDMCode = "MDD";
		public const string ProductMDM = "ENT";
		public const string ModuleMDM = "CDS";
		public const string ProgramAreaMDM = "MDM";
		public const string SystemMDM = "Some Really Long System Name That Will Be Truncated";

		#endregion

		public const string ChangeTypePER = "PER";
		public const string PriorityALP = "ALP";
		public const string PriorityGP1 = "GP1";

		protected internal IList<AlertIncidentForTest> AlertsCreated { get; set; } = new List<AlertIncidentForTest>();

		protected internal override (AddAlertTargetResult result, string jobNumber) CreateTargetAndUpdateAlertAndSource(BaseAlert alert, AlertRule matchedRule, ZGuid capabilityId, Dictionary<string, (int, IEnumerable<string>, IEnumerable<ZString>, ZGuid)> capabilityJobsList)
		{
			var result = base.CreateTargetAndUpdateAlertAndSource(alert, matchedRule, capabilityId, capabilityJobsList);
			if (string.IsNullOrWhiteSpace(matchedRule.Criticality))
			{
				matchedRule.Criticality = "CR9";
			}
			if (result.result == AddAlertTargetResult.TargetAdded || result.result == AddAlertTargetResult.TaskAdded)
			{
				AlertsCreated.Add(new AlertIncidentForTest { AlertRuleId = alert.RuleId, TargetKey = result.jobNumber, SourceAlertId = alert.Id, SourceRuleId = matchedRule.PK, MatchedRule = matchedRule });
			}
			return result;
		}

		protected internal override BaseExternalAlertProvider BuildSqlCpuUsageAlertProvider(ILogger serviceLogger, DbConnection wiseGridReportingConnection, IEnumerable<AlertDefault> alertDefaults)
		{
			return new SqlCpuUsageAlertProviderForTest(wiseGridReportingConnection, alertDefaults, dataReader, this.releaseBuildContent);
		}

		protected internal override void AddAlertIncident(Guid sourceRuleId, Guid sourceAlertId, Guid alertRuleId, string incidentNumber)
		{
		}

		protected internal override DbConnection GetDbConnection(string connectionString)
		{
			return Db.NewAdminConnection();
		}

		protected internal override IEnumerable<AlertDefault> LoadAlertDefaults()
		{
			return new List<AlertDefault>
			{
				new AlertDefault { Name = FallbackCapabilityName, Value = CapabilityINTPk.ToString() },
				new AlertDefault { Name = ClientOrgIdName, Value = "02621F44-071D-42F1-9689-00D8377033A4" },
				new AlertDefault { Name = ClientCompanyIdName, Value = string.Empty },
				new AlertDefault { Name = ClientDbIdName, Value = string.Empty },
				new AlertDefault { Name = ClientContactIdName, Value = "4E990CC1-9536-4421-ACD8-BA61E471B845" },
				new AlertDefault { Name = DefaultCriticalityName, Value = "CR5" },
				new AlertDefault { Name = SqlCpuUsageAlertProvider.MaxiumNumberOfCpuUsageAlertPerDayPerSystemName, Value = Convert.ToString(AlertProcessingTaskTestHelper.MaxTargets()) }
			};
		}

		protected internal override IList<AlertRule> LoadAlertRules(AlertRuleType type)
		{
			return new List<AlertRule>
			{
				new AlertRule { PK = INTSourceRuleId, Capability = CapabilityINTCode, AlertName = SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, Path = SystemINT, Owner = "abc", Criticality = "CR3", Product = ProductINT, Module = ModuleINT, ProgramArea = ProgramAreaINT, Priority = PriorityALP, ChangeType = ChangeTypePER },
				new AlertRule { PK = DOMSourceRuleId, Capability = CapabilityDOMCode, AlertName = SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, Path = SystemDOM, Owner = "xyz", Product = ProductDOM, Module = ModuleDOM, ProgramArea = ProgramAreaDOM, Priority = PriorityALP, ChangeType = ChangeTypePER },
				new AlertRule { PK = MDMSourceRuleId, Capability = CapabilityMDMCode, AlertName = SqlCpuUsageAlertProvider.AlertNameExpensiveSQL, Path = SystemMDM, Owner = "def", Product = ProductMDM, Module = ModuleMDM, ProgramArea = ProgramAreaMDM, Priority = PriorityALP, ChangeType = ChangeTypePER }
			};
		}
	}

	#endregion

	#region CPU Usage Alerts Response

	class SqlCpuUsageAlertProviderForESResponseTest : SqlCpuUsageAlertProvider
	{
		public SqlCpuUsageAlertProviderForESResponseTest(DbConnection connection, IEnumerable<AlertDefault> defaults, Dictionary<string, int> capabilityCounts, SearchResponseType searchResponseType)
			: base(new DummyLogger(), connection, defaults)
		{
			this.searchResponseType = searchResponseType;
		}

		protected override IEnumerable<AggregateDictionary> GetResponse(IEnumerable<string> capabilitiesNotFull)
		{
			var emptyObject = new AggregateDictionary(new ReadOnlyDictionary<string, IAggregate>(new Dictionary<string, IAggregate>()));
			switch (searchResponseType)
			{
				case SearchResponseType.Null:
					yield return null;
					break;
				case SearchResponseType.PartialObject:
				case SearchResponseType.FullObject:
				default:
					yield return emptyObject;
					break;
			}
		}

		public override void UpdateAlert(BaseAlert alert, string targetNumber, string owner)
		{
		}

		readonly SearchResponseType searchResponseType;
	}

	enum SearchResponseType
	{
		FullObject,
		PartialObject,
		Null,
	}

	#endregion

	class AlertProcessingTaskTestHelper
	{
		public static void TearDown()
		{
			SetExternalMonitoringWiseGridReportingConnectionString(string.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1065:EncryptSqlConnection", Justification = "Baseline")]
		public static void SetUp()
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder { DataSource = Db.Connection.ServerName, InitialCatalog = Db.Connection.CurrentDatabase, UserID = "foo" };
			SetExternalMonitoringWiseGridReportingConnectionString(connectionStringBuilder.ConnectionString);
		}

		static void SetExternalMonitoringWiseGridReportingConnectionString(string connectionString)
		{
			EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, connectionString);
		}

		public static int MaxTargets()
		{
			return 3;
		}
	}

	class AlertIncidentForTest : AlertIncident
	{
		public AlertRule MatchedRule { get; set; }
	}
}
