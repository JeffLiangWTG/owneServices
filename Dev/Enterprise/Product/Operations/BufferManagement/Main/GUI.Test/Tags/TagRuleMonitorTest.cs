using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagRuleMonitorTest : BMSTestCaseWithFactory
	{
		#region Prevent tag fighting

		public void TestTagRuleMonitor_ShouldPreventTagFighting()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger);
			RunTagRuleMonitor(logger);
			AssertTagFightingAroundWorkflow1HasBeenPrevented();
		}

		public void TestTagRuleMonitor_ShouldIgnoreTagFighting_IfThereAreManualChangesToSingleJob()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger, interimAction: MakeManualChangesToJob1);
			RunTagRuleMonitor(logger);
			AssertTagFightingHasBeenIgnored();
		}

		public void TestTagRuleMonitor_ShouldPreventTagFighting_IfManualChangesMadeToSomeOfRelatedJobs()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			MakeWorkflowTargetForFightingRules(workflow2);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger, interimAction: MakeManualChangesToJob1, affectedWorkflows: 2);
			RunTagRuleMonitor(logger);
			AssertTagFightingAroundWorkflow2HasBeenPrevented();
		}

		public void TestTagRuleMonitor_ShouldPreventTagFighting_IfManualChangesMadeToAllRelatedJobs()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			MakeWorkflowTargetForFightingRules(workflow2);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger, interimAction: MakeManualChangesToJob1AndJob2, affectedWorkflows: 2);
			RunTagRuleMonitor(logger);
			AssertTagFightingHasBeenIgnored();
		}

		public void TestTagRuleMonitor_ShouldPreventTagFighting_IfManualChangesToSingleJobAreMadeBeforeFighting()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger, precedingAction: MakeManualChangesToJob1);
			RunTagRuleMonitor(logger);
			AssertTagFightingAroundWorkflow1HasBeenPrevented();
		}

		public void TestTagRuleMonitor_ShouldPreventTagFighting_IfThereAreManualChangesToWorkflow()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger, interimAction: MakeManualChangesToWorkflow1);
			RunTagRuleMonitor(logger);
			AssertTagFightingAroundWorkflow1HasBeenPrevented();
		}

		public void TestTagRuleMonitor_ShouldTakeScanDepthIntoAccount()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			var logs = RunTagRuleRunnerAndAssertPreconditions(logger);

			var sqlTemplate = "SELECT Count(*) FROM dbo.GetFightingTagRules('{0}', '{1}', {2}, {3})";
			var times = logs.Where(x => x.SL_Parent == workflow1.PK && x.SL_SE_NKEvent == "TAG").OrderBy(x => x.SL_PostedTimeUtc).Select(x => x.SL_PostedTimeUtc);
			ZDateTime firstLogTime = times.First();
			ZDateTime thirdLogTime = times.Skip(2).First();

			//CASE A: ChurnDetectionLimit = 3, ChurnDetectionDepth covers all logs
			ZDateTime startTime = firstLogTime;
			ZDateTime endTime = ZDateTime.UtcNow;

			var sql = string.Format(sqlTemplate, startTime.SqlFormat, endTime.SqlFormat, 3, 5);
			var command = Db.Connection.Command(sql);
			var commandResult = command.ExecuteScalar();
			AssertNotNull(commandResult);
			AssertEquals("Case A - two tag rules should be fighting around one workflow", "1", commandResult.ToString());

			//CASE B: ChurnDetectionLimit = 3, ChurnDetectionDepth is reduced
			startTime = thirdLogTime;
			sql = string.Format(sqlTemplate, startTime.SqlFormat, endTime.SqlFormat, 3, 5);
			command = Db.Connection.Command(sql);
			commandResult = command.ExecuteScalar();
			AssertNotNull(commandResult);
			AssertEquals("Case B - should be no fighting rules detected because the time depth does not cover all the logs", "0", commandResult.ToString());

			//CASE C: ChurnDetectionLimit = 10, ChurnDetectionDepth covers all logs
			startTime = firstLogTime;
			sql = string.Format(sqlTemplate, startTime.SqlFormat, endTime.SqlFormat, 10, 5);
			command = Db.Connection.Command(sql);
			commandResult = command.ExecuteScalar();
			AssertNotNull(commandResult);
			AssertEquals("Case C - should be no fighting rules detected because 3 repetitions are not enough to make the decision that the rules are fighting", "0", commandResult.ToString());
		}

		#endregion

		public void TestTagRuleMonitor_ShouldNotDeactivateSystemRules_IfThereAreNonSystemRules()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			addRule.TGR_IsSystem = true;
			addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger);
			RunTagRuleMonitor(logger);

			var newFactory = Factory.CreateNewFactory();
			var addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			var removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding system rule should still be active", addRuleLoaded.TGR_IsActive);
			Assert("Removing rule should now be deactivated", !removeRuleLoaded.TGR_IsActive);

			AssertNotificationEmailHasBeenSent(new[] {
				"The following tag rules were fighting:",
				"- System rule [Add AAA] will stay active.",
				"- Rule [Remove AAA] is to be deactivated in order to prevent tag fighting.",
				"1 item(s) caused the tag fighting:",
				"Code = Look out!, Description = Organization (XVBQP68SIYXQ) - Look out!" });
			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunTagRuleMonitor(logger);

			newFactory = Factory.CreateNewFactory();
			addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding system rule should still be active", addRuleLoaded.TGR_IsActive);
			Assert("Removing rule should be already deactivated", !removeRuleLoaded.TGR_IsActive);

			AssertNoNotificationEmailsHaveBeenSent();
		}

		public void TestTagRuleMonitor_ShouldDeactivateSystemRules_IfThereAreNoOtherRules()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			addRule.TGR_IsSystem = true;
			removeRule.TGR_IsSystem = true;
			addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger);
			RunTagRuleMonitor(logger);

			var newFactory = Factory.CreateNewFactory();
			var addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			var removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding rule should now be deactivated", !addRuleLoaded.TGR_IsActive);
			Assert("Removing rule should now be deactivated", !removeRuleLoaded.TGR_IsActive);

			AssertNotificationEmailHasBeenSent(new[] {
				"The following tag rules were fighting:",
				"- System rule [Add AAA] is to be deactivated in order to prevent tag fighting.",
				"- System rule [Remove AAA] is to be deactivated in order to prevent tag fighting.",
				"1 item(s) caused the tag fighting:",
				"Code = Look out!, Description = Organization (XVBQP68SIYXQ) - Look out!" });
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertCollectionContains(ErrorReporter.LastKeyReported, new List<string>() { "The [Add AAA] system rule is fighting with other system rules or itself", "The [Remove AAA] system rule is fighting with other system rules or itself" });
			AssertEquals("Error reports should be sent", 2, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();

			RunTagRuleMonitor(logger);

			newFactory = Factory.CreateNewFactory();
			addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding system rule should be already deactivated", !addRuleLoaded.TGR_IsActive);
			Assert("Removing system rule should be already deactivated", !removeRuleLoaded.TGR_IsActive);

			AssertNoNotificationEmailsHaveBeenSent();
		}

		public void TestTagRuleMonitor_ShouldReportOnFigthingRules_WhichWereDeleted()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger);
			var addRulePk = addRule.PK;
			addRule.Delete();
			Factory.Save();

			RunTagRuleMonitor(logger);

			AssertNotificationEmailHasBeenSent(new[] {
				"The following tag rules were fighting:",
				$"- Rule [PK: {addRulePk}] was deleted in the interim between the tag fighting and scanning.",
				"- Rule [Remove AAA] is to be deactivated in order to prevent tag fighting.",
				"1 item(s) caused the tag fighting:",
				"Code = Look out!, Description = Organization (XVBQP68SIYXQ) - Look out!" });
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestTagRuleMonitor_WhenSelectingBranch_AndAllFightingRulesHaveAlreadyBeenDeleted_ShouldNotThrowException()
		{
			MakeWorkflowTargetForFightingRules(workflow1);
			Factory.Save();
			var logger = new LoggerForTest();
			RunTagRuleRunnerAndAssertPreconditions(logger);

			addRule.Delete();
			removeRule.Delete();
			Factory.Save();

			AssertNoExceptionThrown("When fighting is detected and the only rules involved are ones that have been deleted, there shouldn't be any exceptions thrown.", () => RunTagRuleMonitor(logger));
			AssertNoNotificationEmailsHaveBeenSent();
		}

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var logger = new TestServiceLogger();
			RunTagRuleMonitor(logger);

			AssertContains("The runner should run because Buffer Management or better is enabled.", "No fighting rules found.", logger.ToString());
			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var logger = new TestServiceLogger();
			RunTagRuleMonitor(logger);

			AssertContains("The runner should run because Buffer Management or better is enabled.", "No fighting rules found.", logger.ToString());
			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var logger = new TestServiceLogger();
			RunTagRuleMonitor(logger);

			AssertNotContains("The runner should not run because Buffer Management or better is not enabled.", "No fighting rules found.", logger.ToString());
			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var logger = new TestServiceLogger();
			RunTagRuleMonitor(logger);

			AssertNotContains("The runner should not run because Buffer Management or better is not enabled.", "No fighting rules found.", logger.ToString());
			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use this processor.", logger.ToString());
		}

		#endregion

		#region Implementation

		OrgHeader job1;
		OrgHeader job2;
		ProcessHeader workflow1;
		ProcessHeader workflow2;
		TagMagnitude tagMagnitude;
		TagRule addRule;
		TagRule removeRule;

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.TagRuleChurnDetectionDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			BMSRegistry.Instance.TagRuleChurnDetectionLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			BMSRegistry.Instance.TagRuleLogOutputLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			SetupStaffAndNotificationGroup();
			SetupTagsAndRules();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
		}

		void SetupStaffAndNotificationGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";

			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			Factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());
		}

		void SetupTagsAndRules()
		{
			job1 = Factory.NewWithValidTestData<OrgHeader>();
			job2 = Factory.NewWithValidTestData<OrgHeader>();
			workflow1 = ProcessJobHeader.GetForParent(job1, Factory).ProcessHeaders.AddNew();
			workflow2 = ProcessJobHeader.GetForParent(job2, Factory).ProcessHeaders.AddNew();

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			addRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add AAA", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(addRule.Filter, "Completion Statement", "Look out!");

			removeRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Remove AAA", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(removeRule.Filter, "Completion Statement", "Look out!");

			Factory.Save();
		}

		void MakeWorkflowTargetForFightingRules(ProcessHeader workflow)
		{
			workflow.FH_CompletionStatement = "Look out!";
		}

		StmALog[] RunTagRuleRunnerAndAssertPreconditions(LoggerForTest logger, Action precedingAction = null, Action interimAction = null, int affectedWorkflows = 1)
		{
			RunTagRuleRunnerForSetupRules(logger, precedingAction, interimAction);

			workflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			AssertTagNotApplied(workflow1, tagMagnitude);

			StmALog[] logs = null;
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: changes made by the [Add AAA] rule should be logged", BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value, logger.LogEntries.Count(x => x.StartsWith($"Processed rule [Add AAA], rows modified [{affectedWorkflows}]")));
				AssertEquals("Precondition: changes made by the [Remove AAA] rule should be logged", BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value, logger.LogEntries.Count(x => x.StartsWith($"Processed rule [Remove AAA], rows modified [{affectedWorkflows}]")));

				logs = GetStmALogs("TAG");
				AssertEquals("Precondition: changes should be logged into StmALog", BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value * 2, logs.Count(x => x.SL_Parent == workflow1.PK));
			});

			return logs;
		}

		StmALog[] GetStmALogs(string eventType)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType);
			var logs = Factory.CreateNewFactory().Load<StmALog>(query);
			return logs;
		}

		void RunTagRuleRunnerForSetupRules(LoggerForTest logger, Action precedingAction = null, Action interimAction = null)
		{
			precedingAction?.Invoke();

			var addRunner = new DummyTagRuleRunner(logger, addRule);
			var removeRunner = new DummyTagRuleRunner(logger, removeRule);

			addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			addRunner.Process();

			removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			removeRunner.Process();

			interimAction?.Invoke();

			for (int i = 1; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++)
			{
				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				addRunner.Process();

				removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				removeRunner.Process();
			}
		}

		void RunTagRuleMonitor(ILogger logger)
		{
			var ruleMonitor = new TagRuleMonitor(logger);
			ruleMonitor.Process();
		}

		void MakeManualChangesToJob1()
		{
			MakeManualChangesToJob(job1);
		}

		void MakeManualChangesToJob1AndJob2()
		{
			MakeManualChangesToJob(job1);
			MakeManualChangesToJob(job2);
		}

		void MakeManualChangesToJob(OrgHeader job)
		{
			job.OH_FullName = "Horns and Hooves Ltd.";
			Factory.Save();
			var logs = GetStmALogs("EDT");
			AssertEquals("Job changes should be logged into StmALog", 1, logs.Count(x => x.SL_Parent == job.PK));
		}

		void MakeManualChangesToWorkflow1()
		{
			MakeManualChangesToWorkflow(workflow1);
		}

		void MakeManualChangesToWorkflow(ProcessHeader workflow)
		{
			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			Factory.Save();
		}

		void AssertTagFightingAroundWorkflow1HasBeenPrevented()
		{
			AssertFightingRulesHaveBeenDeactivated();
			AssertNotificationEmailHasBeenSent(new[] {
				"The following tag rules were fighting:",
				"- Rule [Add AAA] is to be deactivated in order to prevent tag fighting.",
				"- Rule [Remove AAA] is to be deactivated in order to prevent tag fighting.",
				"1 item(s) caused the tag fighting:",
				"Code = Look out!, Description = Organization (XVBQP68SIYXQ) - Look out!" });
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		void AssertTagFightingAroundWorkflow2HasBeenPrevented()
		{
			AssertFightingRulesHaveBeenDeactivated();
			AssertNotificationEmailHasBeenSent(new[] {
				"The following tag rules were fighting:",
				"- Rule [Add AAA] is to be deactivated in order to prevent tag fighting.",
				"- Rule [Remove AAA] is to be deactivated in order to prevent tag fighting.",
				"1 item(s) caused the tag fighting:",
				"Code = Look out!, Description = Organization (H5ZX52PAMCOI) - Look out!" });
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		void AssertFightingRulesHaveBeenDeactivated()
		{
			var newFactory = Factory.CreateNewFactory();
			var addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			var removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding rule should now be deactivated", !addRuleLoaded.TGR_IsActive);
			Assert("Removing rule should now be deactivated", !removeRuleLoaded.TGR_IsActive);
		}

		void AssertTagFightingHasBeenIgnored()
		{
			var newFactory = Factory.CreateNewFactory();
			var addRuleLoaded = newFactory.Load<TagRule>(addRule.PK);
			var removeRuleLoaded = newFactory.Load<TagRule>(removeRule.PK);

			Assert("Adding rule should still be active", addRuleLoaded.TGR_IsActive);
			Assert("Removing rule should still be active", removeRuleLoaded.TGR_IsActive);

			AssertNoNotificationEmailsHaveBeenSent();
		}

		void AssertNotificationEmailHasBeenSent(string[] expectedBody)
		{
			var emailLinesWithNoItemNumbers = GetNotificationEmailAndAssertItHasBeenSent();
			AssertContainsExactElementsInAnyOrder(expectedBody, emailLinesWithNoItemNumbers);
		}

		IEnumerable<string> GetNotificationEmailAndAssertItHasBeenSent()
		{
			AssertEquals("One email per fighting group should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sendEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Tag rules fighting detected", sendEmail.Subject);

			AssertEquals(1, sendEmail.Recipients.Count);
			AssertEquals("frodo@bagend.com", sendEmail.Recipients[0].Email);

			var emailLinesWithNoItemNumbers = sendEmail.Body.Split('\n').Select(l => l.Trim()).Select(l => l.StartsWith("- Item ") ? l.Substring("- Item ".Length + 3) : l);
			return emailLinesWithNoItemNumbers;
		}

		void AssertNoNotificationEmailsHaveBeenSent()
		{
			AssertEquals("No notification emails should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion
	}
}
