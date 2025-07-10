using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TagMonitorServiceTask))]
	class TagMonitorServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<TagMonitorServiceTask>
	{
		public void TestTagRuleMonitor_LogsFighting()
		{
			var orgHeader = (IWorkflowProvider)Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, true);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Number one";

			var workflowToDeleteBeforeScan = BMSTestHelper.CreateWorkflow(jobHeader, "Number two");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			var addRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add AAA", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(addRule.Filter, "Completion Statement", "Number");

			var removeRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Remove AAA", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(removeRule.Filter, "Completion Statement", "Number");

			Factory.Save();

			var loggerForTagilator = new LoggerForTest();
			var addRunner = new DummyTagRuleRunner(loggerForTagilator, addRule);
			var removeRunner = new DummyTagRuleRunner(loggerForTagilator, removeRule);

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++) // Tag Rule Churn Detection Limit = 3
			{
				var workflowLoaded = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
				var workflowToDeleteBeforeScanLoaded = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
				AssertEquals(false, workflowLoaded.IsTagApplied(tagMagnitude));
				AssertEquals(false, workflowToDeleteBeforeScanLoaded.IsTagApplied(tagMagnitude));

				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				addRunner.Process();

				workflowLoaded = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
				workflowToDeleteBeforeScanLoaded = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
				AssertEquals(true, workflowLoaded.IsTagApplied(tagMagnitude));
				AssertEquals(true, workflowToDeleteBeforeScanLoaded.IsTagApplied(tagMagnitude));

				removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				removeRunner.Process();
			}

			AssertEquals("Precondition: changes should be logged", 3, loggerForTagilator.LogEntries.Count(x => x.StartsWith("Processed rule [Add AAA], rows modified [2]")));
			AssertEquals("Precondition: changes should be logged", 3, loggerForTagilator.LogEntries.Count(x => x.StartsWith("Processed rule [Remove AAA], rows modified [2]")));

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TAG");
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: changes should be logged in StmALog", 6, logs.Count(x => x.SL_Parent == workflow.PK));
			AssertEquals("Precondition: changes should be logged in StmALog", 6, logs.Count(x => x.SL_Parent == workflowToDeleteBeforeScan.PK));

			workflowToDeleteBeforeScan.Delete();
			Factory.Save();

			var serviceTask = GetNewServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var log = loggerForMonitor.ToString();
			AssertContains("Information|1 fighting tag rule incident(s) found:", log);
			AssertContains("Information|Incident 1", log);
			AssertContains("Information|The following tag rules were fighting:", log);
			AssertContains(" - Rule [Add AAA] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains(" - Rule [Remove AAA] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains("2 item(s) caused the tag fighting:", log);
			AssertContains("Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one", log);
			AssertContains("deleted in the interim between the tag fighting and monitoring", log);
			AssertContains("Information|2 fighting rule(s) have been deactivated", log);

			serviceTask = GetNewServiceTask();
			loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			log = loggerForMonitor.ToString();
			AssertContains("Information|1 fighting tag rule incident(s) found:", log);
			AssertContains("Information|Incident 1", log);
			AssertContains("Information|The following tag rules were fighting:", log);
			AssertContains(" - Rule [Add AAA] is already deactivated.", log);
			AssertContains(" - Rule [Remove AAA] is already deactivated.", log);
			AssertContains("2 item(s) caused the tag fighting:", log);
			AssertContains("Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one", log);
			AssertContains("deleted in the interim between the tag fighting and monitoring", log);
			AssertContains("Information|No fighting rules have been deactivated", log);
		}

		public void TestTagRuleMonitor_ReportsIfNoFightingRulesFound()
		{
			var serviceTask = GetNewServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			string expected = @"Debug|No fighting rules found";

			Assert(string.Format(CultureInfo.InvariantCulture, @"Service task log. Expected log:

{0}

But actual log was:

{1}", expected, loggerForMonitor.ToString()),
				string.Compare(expected, loggerForMonitor.ToString(), CultureInfo.InvariantCulture, CompareOptions.IgnoreSymbols) == 0);

			AssertEquals("No email should be created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestTagRuleMonitor_ProcessesComplexChainsOfRules()
		{
			var orgHeader = (IWorkflowProvider)Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, true);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "for chain 1";

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "for chain 2");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "TDF");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitudeAAA = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");
			var tagMagnitudeBBB = BMSTestHelper.CreateTagMagnitude(tagGroup, "BBB");
			var tagMagnitudeCCC = BMSTestHelper.CreateTagMagnitude(tagGroup, "CCC");

			// chains of rules:
			// 1 chain: A -> B -> C -> D -> A
			// 2 chain: A -> E -> A

			//the first rule in both chains of rules

			var ruleA = BMSTestHelper.CreateTagRule(tagMagnitudeAAA, "Rule A adds AAA (acts in both chains)", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(ruleA.Filter, "Completion Statement", "for");

			//other rules in the first chain

			var ruleB = BMSTestHelper.CreateTagRule(tagMagnitudeBBB, "Rule B adds BBB if a workflow is for the chain 1", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(ruleB.Filter, "Completion Statement", "for chain 1");

			var ruleC = BMSTestHelper.CreateTagRule(tagMagnitudeAAA, "Rule C removes AAA if a workflow is for the chain 1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(ruleC.Filter, "Completion Statement", "for chain 1");

			var ruleD = BMSTestHelper.CreateTagRule(tagMagnitudeBBB, "Rule D removes BBB if a workflow is for the chain 1", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(ruleD.Filter, "Completion Statement", "for chain 1");

			//the second chain of rules

			var ruleE = BMSTestHelper.CreateTagRule(tagMagnitudeAAA, "Rule E removes AAA if a workflow is for the chain 2", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(ruleE.Filter, "Completion Statement", "for chain 2");

			Factory.Save();

			var logger = new LoggerForTest();
			var runnerA = new DummyTagRuleRunner(logger, ruleA);
			var runnerB = new DummyTagRuleRunner(logger, ruleB);
			var runnerC = new DummyTagRuleRunner(logger, ruleC);
			var runnerD = new DummyTagRuleRunner(logger, ruleD);
			var runnerE = new DummyTagRuleRunner(logger, ruleE);

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++)
			{
				ruleA.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				runnerA.Process();

				ruleB.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				runnerB.Process();

				ruleC.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				runnerC.Process();

				ruleD.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				runnerD.Process();

				ruleE.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				runnerE.Process();
			}

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			workflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);

			AssertEquals("Precondition: changes should be logged", 3, logger.LogEntries.Count(x => x.StartsWith("Processed rule [Rule A adds AAA (acts in both chains)], rows modified [2]")));
			AssertEquals("Precondition: changes should be logged", 3, logger.LogEntries.Count(x => x.StartsWith("Processed rule [Rule B adds BBB if a workflow is for the chain 1], rows modified [1]")));
			AssertEquals("Precondition: changes should be logged", 3, logger.LogEntries.Count(x => x.StartsWith("Processed rule [Rule C removes AAA if a workflow is for the chain 1], rows modified [1]")));
			AssertEquals("Precondition: changes should be logged", 3, logger.LogEntries.Count(x => x.StartsWith("Processed rule [Rule D removes BBB if a workflow is for the chain 1], rows modified [1]")));
			AssertEquals("Precondition: changes should be logged", 3, logger.LogEntries.Count(x => x.StartsWith("Processed rule [Rule E removes AAA if a workflow is for the chain 2], rows modified [1]")));

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "TAG");
			var logs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: changes should be logged", 12, logs.Count(x => x.SL_Parent == workflow1.PK));
			AssertEquals("Precondition: changes should be logged", 6, logs.Count(x => x.SL_Parent == workflow2.PK));

			var serviceTask = GetNewServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var newFactory = Factory.CreateNewFactory();

			var ruleALoaded = newFactory.Load<TagRule>(ruleA.PK);
			var ruleBLoaded = newFactory.Load<TagRule>(ruleB.PK);
			var ruleCLoaded = newFactory.Load<TagRule>(ruleC.PK);
			var ruleDLoaded = newFactory.Load<TagRule>(ruleD.PK);
			var ruleELoaded = newFactory.Load<TagRule>(ruleE.PK);

			Assert("Rule A should now be deactivated", !ruleA.TGR_IsActive);
			Assert("Rule B should now be deactivated", !ruleB.TGR_IsActive);
			Assert("Rule C should now be deactivated", !ruleC.TGR_IsActive);
			Assert("Rule D should now be deactivated", !ruleD.TGR_IsActive);
			Assert("Rule E should now be deactivated", !ruleE.TGR_IsActive);

			var log = loggerForMonitor.ToString();
			AssertContains("Information|2 fighting tag rule incident(s) found:", log);
			AssertContains("Information|Incident 1", log);
			AssertContains("Information|The following tag rules were fighting:", log);
			AssertContains(" - Rule [Rule A adds AAA (acts in both chains)] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains(" - Rule [Rule B adds BBB if a workflow is for the chain 1] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains(" - Rule [Rule C removes AAA if a workflow is for the chain 1] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains(" - Rule [Rule D removes BBB if a workflow is for the chain 1] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains("1 item(s) caused the tag fighting:", log);
			AssertContains(" - Item 1: Code = for chain 1, Description = Organization (XVBQP68SIYXQ) - for chain 1", log);
			AssertContains("Information|Incident 2", log);
			AssertContains("Information|The following tag rules were fighting:", log);
			AssertContains(" - Rule [Rule A adds AAA (acts in both chains)] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains(" - Rule [Rule E removes AAA if a workflow is for the chain 2] is to be deactivated in order to prevent tag fighting.", log);
			AssertContains("1 item(s) caused the tag fighting:", log);
			AssertContains(" - Item 1: Code = for chain 2, Description = Organization (XVBQP68SIYXQ) - for chain 2", log);
			AssertContains("Information|5 fighting rule(s) have been deactivated", log);
		}

		public void TestTagRuleMonitor_DoesNotTreatSingleADDRuleAsFighting()
		{
			var orgHeader = (IWorkflowProvider)Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, true);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Number one";

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			var addRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add AAA", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(addRule.Filter, "Completion Statement", "Number");

			Factory.Save();

			var loggerForTagilator = new LoggerForTest();
			var addRunner = new DummyTagRuleRunner(loggerForTagilator, addRule);

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++)
			{
				var workflowLoaded = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
				AssertEquals(false, workflowLoaded.IsTagApplied(tagMagnitude));

				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				addRunner.Process();

				var newFactory = Factory.CreateNewFactory();
				workflowLoaded = newFactory.Load<ProcessHeader>(workflow.PK);
				AssertEquals(true, workflowLoaded.IsTagApplied(tagMagnitude));
				var tagLink = workflowLoaded.TagLinks.First(l => l.TGL_TGM_Magnitude == tagMagnitude.PK);
				tagLink.Delete();
				newFactory.Save();
			}

			AssertEquals("Precondition: changes should be logged", 3, loggerForTagilator.LogEntries.Count(x => x.StartsWith("Processed rule [Add AAA], rows modified")));

			using (TestConnection.TrackExecutedCommands())
			{
				var serviceTask = GetNewServiceTask();
				var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
				serviceTask.RunTask();

				var serviceTaskRunResultFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var query = new ZDBOnlyQuery(typeof(StmALog));
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "TAG");
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "RUL=");

				var tagALog = serviceTaskRunResultFactory.Load<StmALog>(query);

				var log = loggerForMonitor.ToString();

				CombineAssertions(() =>
				{
					AssertEquals("Tag rule applications should be logged in the database", 3, tagALog.Length);
					AssertEquals("Fighting tag rule SQL function should be run", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("dbo.GetFightingTagRules")));
					AssertEquals("Fighting tag rule should have been detected", 4, loggerForMonitor.Count);
					AssertContains("Information|1 fighting tag rule incident(s) found:", log);
					AssertContains("Information|Incident 1", log);
					AssertContains("Information|Suspicious tag rule activity:", log);
					AssertContains(" - Rule [Add AAA] triggered several times due to manual tagging/untagging and does not need to be deactivated.", log);
					AssertContains("1 item(s) caused the tag fighting:", log);
					AssertContains(" - Item 1: Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one", log);
					AssertContains("Information|No fighting rules have been deactivated.", log);
				});
			}
		}

		public void TestTagRuleMonitor_ReportsFirstNItemsOnly()
		{
			var orgHeader = (IWorkflowProvider)Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, true);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Number one";

			BMSTestHelper.CreateWorkflow(jobHeader, "Number two");
			BMSTestHelper.CreateWorkflow(jobHeader, "Number three");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			var addRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add AAA", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(addRule.Filter, "Completion Statement", "Number");

			var removeRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Remove AAA", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(removeRule.Filter, "Completion Statement", "Number");

			Factory.Save();

			var loggerForTagilator = new LoggerForTest();
			var addRunner = new DummyTagRuleRunner(loggerForTagilator, addRule);
			var removeRunner = new DummyTagRuleRunner(loggerForTagilator, removeRule);

			var printRunTime = ZDateTime.UtcNow.AddMinutes(-1);

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++) // Tag Rule Churn Detection Limit = 3
			{
				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = printRunTime;
				addRunner.Process();

				removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = printRunTime;
				removeRunner.Process();
			}

			var serviceTask = GetNewServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var log = loggerForMonitor.ToString();
			AssertContains("3 item(s) caused the tag fighting:", log);

			BMSTestHelper.CreateWorkflow(jobHeader, "Number four");
			BMSTestHelper.CreateWorkflow(jobHeader, "Number five");
			BMSTestHelper.CreateWorkflow(jobHeader, "Number six");
			BMSTestHelper.CreateWorkflow(jobHeader, "Number seven");

			Factory.Save();

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++) // Tag Rule Churn Detection Limit = 3
			{
				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = printRunTime;
				addRunner.Process();

				removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = printRunTime;
				removeRunner.Process();
			}

			serviceTask.RunTask();
			log = loggerForMonitor.ToString();
			AssertContains("item(s) caused the tag fighting (only first 3 items are shown):", log);
			AssertContains("........................", log);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, TagMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, TagMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", TagMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", TagMonitorServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region Setup

		protected override void SetUpCore()
		{
			base.SetUpCore();

			BMSTestHelper.EnableBMSInRegistry();

			BMSRegistry.Instance.TagRuleChurnDetectionDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			BMSRegistry.Instance.TagRuleChurnDetectionLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			BMSRegistry.Instance.TagRuleLogOutputLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			SetupStaffAndNotificationGroup();
		}

		public void SetupStaffAndNotificationGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());
		}

		#endregion

		#region Implementation

		protected override TagMonitorServiceTask GetNewServiceTask()
		{
			return new TagMonitorServiceTask_ForTest();
		}

		class TagMonitorServiceTask_ForTest : TagMonitorServiceTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
		}

		#endregion
	}
}
