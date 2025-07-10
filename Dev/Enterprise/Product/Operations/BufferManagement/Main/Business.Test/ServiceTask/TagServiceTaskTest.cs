using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagServiceTask))]
	class TagServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<TagServiceTask>
	{
		#region CanRunInAnyBranch

		public void TestProcessTagRules_WithRuleThatHasNoSchedule_ShouldCreateSchedule_AndNotReportErrors()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			rule.Schedule.Delete();
			Factory.Save();

			StmScheduleTask GetScheduleInNewFactory()
			{
				var newFactory = new BusinessObjectFactory();
				var loadedRule = newFactory.Load<TagRule>(rule.PK);
				return loadedRule.GetScheduleFromDatabase_ForTest();
			}

			AssertNull("Precondition: There should be no schedule for this tag rule.", GetScheduleInNewFactory());

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			AssertNoExceptionThrown(() => serviceTask.RunTask());

			AssertNotNull("A schedule should have been created.", GetScheduleInNewFactory());
		}

		public void TestDisableRuleAndLogFailureOnRule_ShouldNotReportEnvironmentErrors()
		{
			BMSTestHelper.SetupEmailAndGroup(Factory);

			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "AAA");
			var rule = BMSTestHelper.CreateTagRule(tagMagnitude, "User rule", TagRuleActionTypeList.Codes.MaintainMagnitude);

			Factory.Save();

			tagDefinition.TGD_UsageScope = TagUsageScopeList.Codes.User; // this will cause the rule to be invalid and deactivate before its branch context has been set.
			Factory.Save();

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();

			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var loadedRule = newFactory.Load<TagRule>(rule.PK);
			AssertEquals("The rule should have been deactivated because its tag can't be used for tag rules.", false, loadedRule.TGR_IsActive);
		}

		public void TestInitialiseSecurityWhenRequiredByModule_ShouldNotReportEnvironmentErrors()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			Factory.Save();

			Env.Instance.ResetSecurityForTest();

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			AssertNoExceptionThrown(() => serviceTask.RunTask());
		}

		#endregion

		#region ConfigString

		[TestDate(2022, 1, 4)]
		public void TestRun_WhenConfigStringSpecifiesAll_ShouldRunAllRulesRegardlessOfSchedule()
		{
			var registryItem = BMSRegistry.Instance.TagRuleThrottling.Value;
			registryItem.IsThrottlingEnabled = false;
			BMSRegistry.Instance.TagRuleThrottling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);

			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			rule.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Factory.Save();

			var initialScheduledRunTime = rule.Schedule.S5_NextScheduledPrintRunTimeUtc;

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			serviceTask.RunTask();

			AssertRuleDidRun(serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);
			AssertEquals("The rule should be scheduled to run again in an hour.", initialScheduledRunTime.AddHours(1), rule.Schedule.S5_NextScheduledPrintRunTimeUtc);

			AdvanceOneMinuteAndRunServiceTask(serviceTask);
			AssertRuleDidNotRun("The next scheduled run time hasn't arrived yet, so the rule should be skipped under normal operation.", serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);

			AdvanceOneMinuteAndRunServiceTask(serviceTask, ":ALL");
			AssertRuleDidRun("The rule should run even though it's not schedule to yet since the command-line argument was used.", serviceTask);
			AssertRunningAllRulesMessage(serviceTask);

			AdvanceOneMinuteAndRunServiceTask(serviceTask, " : ALL");
			AssertRuleDidRun("The command line argument should still work with spaces.", serviceTask);
			AssertRunningAllRulesMessage(serviceTask);
		}

		[TestDate(2022, 1, 4)]
		public void TestRun_WhenConfigStringSpecifiesAll_ShouldRunAllRulesRegardlessOfThrottling()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			rule.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Factory.Save();

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			serviceTask.RunTask();

			AssertRuleDidRun(serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			rule.TGR_LastRunDurationInSeconds = 500;
			Factory.Save();

			AdvanceOneMinuteAndRunServiceTask(serviceTask);
			AssertRuleDidNotRun("The rule is scheduled but should be blocked by throttling.", serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);

			rule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			rule.TGR_LastRunDurationInSeconds = 500;
			Factory.Save();

			AdvanceOneMinuteAndRunServiceTask(serviceTask, ":ALL");
			AssertRuleDidRun("The rule should run even when it would be otherwise throttled since the command-line argument was used.", serviceTask);
			AssertRunningAllRulesMessage(serviceTask);
		}

		[TestDate(2022, 1, 4)]
		public void TestRun_WhenConfigStringPopulatedWithNonsense_ShouldRespectSchedulesAndThrottling()
		{
			var rule = BMSTestHelper.CreateTagRuleWithDefAndMag(Factory);
			rule.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Hourly;
			Factory.Save();

			var serviceTask = GetNewServiceTask();
			serviceTask.ServiceLogger = new BufferManagementLogger();
			serviceTask.RunTask();

			AssertRuleDidRun(serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);

			AdvanceOneMinuteAndRunServiceTask(serviceTask, ":lksdjflks;");
			AssertRuleDidNotRun("A nonsense command line argument was entered, which should have no effect on the service task.", serviceTask);
			AssertRunningFilteredRulesMessage(serviceTask);
		}

		static void AdvanceOneMinuteAndRunServiceTask(TagServiceTask serviceTask, string configString = null)
		{
			serviceTask.ServiceLogger = new BufferManagementLogger();
			TestDateAttribute.AddMinutes(1);
			((IServiceTaskConfigurationUser)serviceTask).ConfigString = configString;
			serviceTask.RunTask();
		}

		#region Assertions

		static void AssertRuleDidRun(TagServiceTask serviceTask)
		{
			AssertRuleDidRun(null, serviceTask);
		}

		static void AssertRuleDidRun(string message, TagServiceTask serviceTask)
		{
			AssertContains(message, RuleDidRunMessage, serviceTask.ServiceLogger.ToString());
		}

		static void AssertRuleDidNotRun(string message, TagServiceTask serviceTask)
		{
			AssertNotContains(message, RuleDidRunMessage, serviceTask.ServiceLogger.ToString(), ignoreCase: true);
		}

		static void AssertRunningFilteredRulesMessage(TagServiceTask serviceTask)
		{
			var log = serviceTask.ServiceLogger.ToString();
			AssertContains(RunningFilteredRulesMessage, log);
			AssertNotContains(RunningAllRulesMessage, log, ignoreCase: true);
		}

		static void AssertRunningAllRulesMessage(TagServiceTask serviceTask)
		{
			var log = serviceTask.ServiceLogger.ToString();
			AssertContains(RunningAllRulesMessage, log);
			AssertNotContains(RunningFilteredRulesMessage, log, ignoreCase: true);
		}

		const string RuleDidRunMessage = "Processed rule [The Tag Rule]";
		const string RunningFilteredRulesMessage = "Processing active rules allowed by schedule recurrence and frequency throttling. To run all active rules regardless of schedule and throttling, run the service task using the command 'TAG -configString:ALL' (without quotes).";
		const string RunningAllRulesMessage = "Processing all active rules regardless of schedule recurrence and frequency throttling.";

		#endregion

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, TagServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, TagServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", TagServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", TagServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region Implementation

		protected override void SetUpCore()
		{
			base.SetUpCore();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override TagServiceTask GetNewServiceTask()
		{
			return new TagServiceTask_ForTest();
		}

		class TagServiceTask_ForTest : TagServiceTask
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
