using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(TagMonitorServiceTask))]
	class TagMonitorServiceTaskTest : BMServiceTaskTestCase<TagMonitorServiceTask>
	{
		public void TestTagRuleMonitor_DetectsARMRuleFightingWithItself()
		{
			var orgHeader = (IWorkflowProvider)Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, true);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Number one";

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			var armRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add and Remove AAA", TagRuleActionTypeList.Codes.AddAndRemoveTag);

			using (var module = new BMFilterRuleModule())
			{
				var filters = module.FilterBusinessObject;

				var filterStrip1 = filters.FilterStrips.AddNew("Completion Statement");
				var filter1 = (ModuleTextFilter)filterStrip1.CurrentModuleFilter;
				filter1.Property = "Number";

				var filterStrip2 = filters.FilterStrips.AddNew("Tag Magnitude");
				var filter2 = (ModuleGuidAppliedToSubCollectionFilter)filterStrip2.CurrentModuleFilter;
				filter2.Property = tagMagnitude.PK;
				filter2.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;

				filters.WriteFilterStripsToXml(armRule.Filter, filters.FilterStrips, new EmptyLayoutsHelper());
			}

			Factory.Save();

			var loggerForTagilator = new LoggerForTest();
			var armRunner = new DummyTagRuleRunner(loggerForTagilator, armRule);

			for (int i = 0; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++)
			{
				armRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				armRunner.Process();
			}

			AssertEquals("Precondition: changes should be logged", 3, loggerForTagilator.LogEntries.Count(x => x.StartsWith("Processed rule [Add and Remove AAA], rows modified")));

			var serviceTask = new TagMonitorServiceTask();
			var loggerForMonitor = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var log = loggerForMonitor.ToString();
			AssertContains("Information|1 fighting tag rule incident(s) found:", log);
			AssertContains("Information|Incident 1", log);
			AssertContains("Information|Suspicious tag rule activity:", log);
			AssertContains(" - Rule [Add and Remove AAA] of the ARM type is probably fighting with itself and to be deactivated in order to prevent tag fighting.", log);
			AssertContains("1 item(s) caused the tag fighting:", log);
			AssertContains(" - Item 1: Code = Number one, Description = Organization (XVBQP68SIYXQ) - Number one", log);
			AssertContains("Information|1 fighting rule(s) have been deactivated", log);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Setup

		protected override void SetUpCore()
		{
			base.SetUpCore();
			disposables = new DisposableList(new[] { DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider() });

			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmALog");

			BMSTestHelper.EnableBMSInRegistry();

			BMSRegistry.Instance.TagRuleChurnDetectionDepth.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 120);
			BMSRegistry.Instance.TagRuleChurnDetectionLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			BMSRegistry.Instance.TagRuleLogOutputLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			SetupStaffAndNotificationGroup();
		}

		DisposableList disposables;

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

		#region TearDown

		protected override void TearDownCore()
		{
			base.TearDownCore();

			disposables.Dispose();
		}

		#endregion
	}
}
