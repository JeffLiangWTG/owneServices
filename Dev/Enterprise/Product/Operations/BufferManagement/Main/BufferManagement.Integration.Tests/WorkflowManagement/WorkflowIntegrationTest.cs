using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class WorkflowIntegrationTest : BMSTestCaseWithFactory
	{
		public void TestTransferRuleRunner_ForQuotedBookingWorkflow_CausesNoException()
		{
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2, sequence: 69);

			var qb = QuotedBooking.New(0, Factory);
			var jobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(qb, Factory, addDefaultProcessHeaderIfNone: false);
			BMSTestHelper.CreateWorkflow(jobHeader, "Hi", bucket1);

			Factory.Save();

			var runner = new DummyTransferRuleRunner(system, new DummyLogger());
			AssertNoExceptionThrown(() => runner.Process_ForTest());
		}

		public void TestSchematicTransferLoopMonitor_ForQuotedBookingWorkflow_CausesNoException()
		{
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "king.arthur@avalon.com";
			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);
			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var qb = QuotedBooking.New(0, Factory);
			var newJobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(qb, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(newJobHeader, "Hi");

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 3);
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3", sequence: 2);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
				});

			var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3);
			FilterStripsTestHelper.AddFilterStrips(link2_3.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
				});

			var link3_1 = BMSTestHelper.LinkComponents(bucket3, bucket1);
			FilterStripsTestHelper.AddFilterStrips(link3_1.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					ComparisonOperatorSetter = f => ((ModuleTextFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank
				});

			workflow.MoveToComponent(bucket1);
			Factory.Save();

			var logger = new DummyLogger();
			var runner = new DummyTransferRuleRunner(system, logger);
			for (int i = 0; i < BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value * 2; i++)
			{
				runner.Process_ForTest();
			}

			var serviceTask = new SchematicTransferLoopMonitorServiceTask_ForTest();
			serviceTask.ServiceLogger = logger;
			AssertNoExceptionThrown(() => serviceTask.RunTask());
		}

		public void TestTagMonitorServiceTask_ForShipmentWorkflow_CausesNoException()
		{
			BMSTestHelper.CreateServiceTask(Factory, TagMonitorServiceTask.Code, TagMonitorServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";
			var bmsNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			var consolWorkflow = BMSTestHelper.CreateWorkflowAndParents<ForwardingConsol>(Factory, "Consol Workflow");
			consolWorkflow.FH_Category = "CN";

			var consol = (CommonConsol)consolWorkflow.Parent;
			var shipment = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			shipment.FillWithValidTestData();

			var shipmentJobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(shipment, shipment.Factory, addDefaultProcessHeaderIfNone: false);
			var shipmentWorkflow = BMSTestHelper.CreateWorkflow(shipmentJobHeader, "Shipment Workflow");
			shipmentWorkflow.FH_Category = "SH";
			shipmentWorkflow.FH_CompletionStatement = "Look out!";

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			tagGroup.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagGroup, "AAA");

			var addRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Add AAA", TagRuleActionTypeList.Codes.AddTag);
			FilterStripsTestHelper.AddStartsWithFilter(addRule.Filter, "Completion Statement", "Look out!");

			var removeRule = BMSTestHelper.CreateTagRule(tagMagnitude, "Remove AAA", TagRuleActionTypeList.Codes.RemoveTag);
			FilterStripsTestHelper.AddStartsWithFilter(removeRule.Filter, "Completion Statement", "Look out!");

			Factory.Save();

			var logger = new LoggerForTest();
			var addRunner = new DummyTagRuleRunner(logger, addRule);
			var removeRunner = new DummyTagRuleRunner(logger, removeRule);

			addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			addRunner.Process();

			removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			removeRunner.Process();

			for (int i = 1; i < BMSRegistry.Instance.TagRuleChurnDetectionLimit.Value; i++)
			{
				addRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				addRunner.Process();

				removeRule.Schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
				removeRunner.Process();
			}

			var serviceTask = new TagMonitorServiceTask_ForTest();
			serviceTask.ServiceLogger = logger;
			AssertNoExceptionThrown(() => serviceTask.RunTask());
		}

		class SchematicTransferLoopMonitorServiceTask_ForTest : SchematicTransferLoopMonitorServiceTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
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
	}
}
