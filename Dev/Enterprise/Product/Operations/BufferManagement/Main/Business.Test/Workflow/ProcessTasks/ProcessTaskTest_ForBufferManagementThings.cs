using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTaskTest_ForBufferManagementThings : BMSTestCaseWithFactory
	{
		#region logging

		public void TestStatusLogging_BufferManagement_Disabled()
		{
			BMSTestHelper.DisableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is disabled", false, BMSRegistryProvider.IsBufferManagementEnabled);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals("GIVEN no staff", true, task.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???",
				"|FRM=OPN|TO=CAN|CHM=OTH",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			task.P9_GS_NKAssignedStaffMember = "BAS";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals("GIVEN staff exists", "BAS", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???",
				"|FRM=CAN|TO=ASN|CHM=OTH|ASN=BAS",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);

			task.P9_GS_NKAssignedStaffMember = "RWH";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("GIVEN staff exists", "RWH", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???",
				"|FRM=ASN|TO=CLS|CHM=OTH|ASN=RWH",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
		}

		[TestDate(2014, 1, 31)]
		public void TestStatusLogging_BufferManagement_Enabled()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var task = config.Workflows.First().Tasks.First();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals("GIVEN no staff", true, task.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???|PEN=???|ZON=???",
				"|FRM=OPN|TO=CAN|CHM=OTH|PEN=0.08|ZON=3",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			task.P9_GS_NKAssignedStaffMember = "CCR";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals("GIVEN staff exists", "CCR", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???|PEN=???|ZON=???",
				"|FRM=CAN|TO=ASN|CHM=OTH|ASN=CCR|PEN=0.17|ZON=3",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
		}

		public void TestStatusLogging_BufferManagement_Enabled_NullProcessHeader()
		{
			BMSTestHelper.EnableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is enabled", true, BMSRegistryProvider.IsBufferManagementEnabled);

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertNull("GIVEN ProcessHeader = null", task.ProcessHeader);
			AssertEquals("GIVEN no staff", true, task.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals("GIVEN no staff, WHEN modify status", "|FRM=OPN|TO=CAN|CHM=OTH", task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
		}

		public void TestStatusLogging_BufferManagement_WorkflowInBucket()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "bucketWorkflow",
				currentComponent: config.Bucket,
				releaseDateTime: ZDateTime.Now,
				staffCode: "CCR",
				lowEstMinutes: 15,
				description: "bucketTask");

			Factory.Save();

			var task = workflow.Tasks.First();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals("GIVEN workflow in bucket (non buffer)", false, task.ProcessHeader.CurrentComponent.IsBuffer);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???|PEN=???|ZON=???",
				"|FRM=ASN|TO=CAN|CHM=OTH|ASN=CCR",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
		}

		[TestDate(2014, 1, 31)]
		public void TestStatusLogging_FetchHint()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSTestHelper.EnableBMSInRegistry();

			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var task = config.Workflows.First().Tasks.First();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task.P9_GS_NKAssignedStaffMember = string.Empty;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals("GIVEN no staff", true, task.P9_GS_NKAssignedStaffMember.IsEmpty);
			AssertEquals("WHEN modify status THEN logging should be in format |FRM=???|TO=???|CHM=???|ASN=???|PEN=???|ZON=???",
				"|FRM=OPN|TO=CAN|CHM=OTH|PEN=0.08|ZON=3",
				task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);

			Factory.DropHints();

			AssertEquals("GIVEN 0 fetchHint on ProcessHeader", 0, Factory.ActiveFetchHintsForTable(ProcessHeaderLinkSchema.Constants.TableName));

			task.P9_Description = "updating task";

			AssertEquals("GIVEN task status has not changed", false, task.P9_StatusInfo.HasChanges);
			AssertEquals("GIVEN task description is changed to trigger FetchForFactorySaveCore", true, task.P9_DescriptionInfo.HasChanges);

			Factory.Save();

			AssertEquals("WHEN saving THEN no ProcessHeader fetchHint should occurrs because task status has not changed", 0, Factory.ActiveFetchHintsForTable(ProcessHeaderLinkSchema.Constants.TableName));
		}

		#endregion

		#region Test duration based on status

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestDuration_OldAndNewStatusLogFormat()
		{
			var task = Factory.New<ProcessTaskForTest>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			task.Logs.AddNew(Events.StatusChange, "[ASN] to [WRK]"); // old format
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Logs.AddNew(Events.StatusChange, "[WRK] to [SUS]"); // old format
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Logs.AddNew(Events.StatusChange, "[SUS] to [WRK]"); // old format
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Logs.AddNew(Events.StatusChange, "[WRK] to [CLS]"); // old format

			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("[WRK] to [CLS]", task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
			AssertEquals("WHEN calculate duration, THEN should show correct duration", 2m, task.ActualDurationHours);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // execute ProcessTask.GetLastWorkedDuration

			Factory.Save();

			AssertEquals("|FRM=WRK|TO=CLS|CHM=OTH", task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
			AssertEquals("WHEN calculate duration, THEN should show correct duration", 3m, task.ActualDurationHours);
		}

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestDuration()
		{
			BMSTestHelper.DisableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is disabled", false, BMSRegistryProvider.IsBufferManagementEnabled);

			var expectedLog = "|FRM=WRK|TO=CLS|CHM=OTH";
			TestDuration_OnStatusChange(workflow: null, staffCode: "", expectedLog: expectedLog);
		}

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestDuration_NewStatusLogFormat()
		{
			BMSTestHelper.DisableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is disabled", false, BMSRegistryProvider.IsBufferManagementEnabled);

			TestDuration_OnStatusChange(workflow: null, staffCode: "", expectedLog: "|FRM=WRK|TO=CLS|CHM=OTH");
			TestDuration_OnStatusChange(workflow: null, staffCode: "BAS", expectedLog: "|FRM=WRK|TO=CLS|CHM=OTH|ASN=BAS");

			BMSTestHelper.EnableBMSInRegistry();
			AssertEquals("GIVEN BufferManagement is enabled", true, BMSRegistryProvider.IsBufferManagementEnabled);
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			TestDuration_OnStatusChange(workflow: config.Workflows.First(), staffCode: "", expectedLog: "|FRM=WRK|TO=CLS|CHM=OTH|PEN=0.05|ZON=3");
			TestDuration_OnStatusChange(workflow: config.Workflows.First(), staffCode: "BAS", expectedLog: "|FRM=WRK|TO=CLS|CHM=OTH|ASN=BAS|PEN=0.10|ZON=3");
		}

		void TestDuration_OnStatusChange(ProcessHeader workflow, string staffCode, string expectedLog)
		{
			var task = Factory.New<ProcessTaskForTest>();

			if (workflow != null)
			{
				task.P9_FH_ProcessHeader = workflow.PK;
				task.P9_ParentID = workflow.FH_ParentId;
				task.P9_ParentTableCode = workflow.FH_ParentTableCode;
			}

			if (!string.IsNullOrEmpty(staffCode))
			{
				task.P9_GS_NKAssignedStaffMember = staffCode;
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_ActualDuration = ZDateTime.Empty; // Clearing the duration now means, "Ignore previous logs" rather than "Lets recalculate the duration"
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // execute ProcessTask.GetLastWorkedDuration

			Factory.Save();

			AssertEquals(expectedLog, task.Logs?.MostRecentLogByEventTime(Events.StatusChange)?.SL_Reference);
			AssertEquals("WHEN calculate duration, THEN should show correct duration", 2m, task.ActualDurationHours);
		}

		#endregion

		public void TestGetParent_ProcessTaskDeleted()
		{
			var processTask = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();
			processTask.Delete();
			AssertNull(processTask.Parent);
		}

		#region Startable Task Filter

		#endregion

		[TestDate(2017, 09, 10, 12, 00, 00)]
		public void TestUniversalCopy_ShouldNotCopyRULTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			var ruleTag = task.AddTag(config.RuleTag).Link;
			task.AddTag(config.RedTag);

			((TagLink)ruleTag).AreOnSavingChecksDisabledForTesting = true;

			var template = CreateTemplate();
			var clone = CopyUniversally(task, template);

			AssertTagApplied(clone, config.RedTag);
			AssertTagNotApplied(clone, config.RuleTag);
		}

		public void TestUniversalCopy_TagsAreNotBeingCached()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			var ruleTag = task.AddTag(config.RuleTag).Link;
			task.AddTag(config.RedTag);

			((TagLink)ruleTag).AreOnSavingChecksDisabledForTesting = true;

			var template = CreateTemplate();
			var clone = CopyUniversally(task, template);

			clone.AddTag(config.PlatinumTag);
			clone.AddTag(config.PrincessCelestiaTag);

			clone = CopyUniversally(clone, template);

			AssertTagApplied(clone, config.RedTag);
			AssertTagNotApplied(clone, config.RuleTag);
			AssertTagApplied(clone, config.PlatinumTag);
			AssertTagApplied(clone, config.PrincessCelestiaTag);
		}

		static ProcessTask CopyUniversally(ProcessTask workflow, CopyTemplateTree copyTemplate)
		{
			return (ProcessTask)new BusinessObjectCopyManager().Copy(workflow, copyTemplate).Object;
		}

		static CopyTemplateTree CreateTemplate()
		{
			var copyTemplateTree = new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(ProcessTask), true));

			foreach (string propertyName in ProcessTaskProperties())
			{
				var property = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == propertyName);
				property.CopyMethod = CopyMethod.Copy;
			}

			var tagProperty = (CollectionCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is CollectionCopyTemplateNode && node.Name == "Tags");
			tagProperty.CopyMethod = CollectionCopyMethod.All;

			foreach (string propertyName in TagsProperties())
			{
				var property = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)tagProperty.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == propertyName);
				property.CopyMethod = CopyMethod.Copy;

				if (property.Name == TagLink.Schema.TGL_Sequence)
				{
					property.CopyMethod = CopyMethod.Macro;
					property.Value = "<NextSequenceNumber>";
				}
			}

			var magnitudeProperty = (RelatedEntityCopyTemplateNode)((EntityCopyTemplateNode)tagProperty.InnerNode).Nodes.Find(node => node is RelatedEntityCopyTemplateNode && node.Name == "Magnitude");
			magnitudeProperty.CopyMethod = RelatedEntityCopyMethod.LinkCopied;

			return copyTemplateTree;
		}

		static string[] ProcessTaskProperties()
		{
			return new string[]
				{
					ProcessTask.Schema.P9_CascadedEventsContext,
					ProcessTask.Schema.P9_Condition1,
					ProcessTask.Schema.P9_Condition2,
					ProcessTask.Schema.P9_Condition2Value,
					ProcessTask.Schema.P9_Description,
					ProcessTask.Schema.P9_Notes,
					ProcessTask.Schema.P9_RespondToCascadedEvents,
					ProcessTask.Schema.P9_Sequence
				};
		}

		static string[] TagsProperties()
		{
			return new string[]
			{
				TagLink.Schema.TGL_Sequence,
				TagLink.Schema.TGL_Description,
				TagLink.Schema.TGL_Magnitude,
				TagLink.Schema.TGL_RemovedTimeUtc
			};
		}

		public void TestAddTaskToDefaultWorkflow_ShouldActuallySaveWorkflowForeignKey()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var jobLevelWorkflow = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: true);
			var workflow = jobLevelWorkflow.ProcessHeaders.SingleOrDefault();
			AssertNotNull("A default workflow should have been created.", workflow);
			AssertEquals("Job Workflow", workflow.FH_CompletionStatement);

			var task = workflow.TaskCollection.AddNew();
			task.P9_Description = "Task";

			Factory.Save();

			var valueInRow = ((INeedRow)task).Row[ProcessTasksSchema.Constants.P9_FH_ProcessHeader];
			AssertEquals("The row should actually contain the PK of the default workflow, rather than relying on code to get it each time.", workflow.PK, valueInRow);

			AssertEquals("The workflow should be open since it has an open task in it.", WorkflowStatusList.Codes.Open, workflow.FH_Status);
		}

		public void TestP9_FC_CurrentComponent_NewTask()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Chilby", system: system);
			var component2 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Skook", system: system);
			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			Factory.Save();
			var jobLevelWorkflow = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: true);
			var workflow = jobLevelWorkflow.ProcessHeaders.SingleOrDefault();
			var task = workflow.TaskCollection.AddNew();
			task.P9_Description = "Foo bar";

			workflow.FH_FC_CurrentComponent = component1.PK;
			Factory.Save();

			AssertEquals("Precondition: Component is populated", true, workflow.FH_FC_CurrentComponent.IsValid);

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddToFilter(ProcessTasksSchema.PK, task.PK);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@component", workflow.FH_FC_CurrentComponent, ProcessHeaderSchema.FH_FC_CurrentComponent);
			query.AddFilterAndZSQLParameterCollection("P9_FC_CurrentComponent = @component", parameters);
			var tasks = Factory.Load<ProcessTask>(query);

			AssertEquals("task should have current component populated.", task, tasks.Single());
		}

		public void TestP9_FC_CurrentComponent_ChangedComponent()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Chilby", system: system);
			var component2 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Skook", system: system);
			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			Factory.Save();
			var jobLevelWorkflow = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: true);
			var workflow = jobLevelWorkflow.ProcessHeaders.SingleOrDefault();
			var task = workflow.TaskCollection.AddNew();
			task.P9_Description = "Foo bar";
			workflow.FH_FC_CurrentComponent = component1.PK;

			Factory.Save();

			AssertEquals("Precondition: Component is populated", true, workflow.FH_FC_CurrentComponent.IsValid);

			workflow.FH_FC_CurrentComponent = component2.PK; //Moving header to another component.

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddToFilter(ProcessTasksSchema.PK, task.PK);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@component", workflow.FH_FC_CurrentComponent, ProcessHeaderSchema.FH_FC_CurrentComponent);
			query.AddFilterAndZSQLParameterCollection("P9_FC_CurrentComponent = @component", parameters);
			var tasks = new BusinessObjectFactory().Load<ProcessTask>(query);
			AssertEquals("task should have current component populated.", task.PK, tasks.Single().PK);
		}

		public void TestP9_FC_CurrentComponent_TaskMovedBetweenWorkflows()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Chilby", system: system);
			var component2 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Skook", system: system);
			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			Factory.Save();
			var jobLevelWorkflow = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: true);
			var workflow1 = jobLevelWorkflow.ProcessHeaders.SingleOrDefault();
			var workflow2 = jobLevelWorkflow.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Im a different workflow aye";
			var task = workflow1.TaskCollection.AddNew();
			task.P9_Description = "Foo bar";

			workflow1.FH_FC_CurrentComponent = component1.PK;
			// Ensuring that workflow 2 is assigned to a different component to Workflow 1.
			workflow2.FH_FC_CurrentComponent = component2.PK;
			Factory.Save();

			AssertEquals("Precondition: Component is populated", true, workflow1.FH_FC_CurrentComponent.IsValid);

			task.P9_FH_ProcessHeader = workflow2.PK;

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddToFilter(ProcessTasksSchema.PK, task.PK);

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@component", workflow2.FH_FC_CurrentComponent, ProcessHeaderSchema.FH_FC_CurrentComponent);
			query.AddFilterAndZSQLParameterCollection("P9_FC_CurrentComponent = @component", parameters);
			var tasks = new BusinessObjectFactory().Load<ProcessTask>(query);
			AssertEquals("task should have current component populated.", task.PK, tasks.Single().PK);
		}
	}
}
