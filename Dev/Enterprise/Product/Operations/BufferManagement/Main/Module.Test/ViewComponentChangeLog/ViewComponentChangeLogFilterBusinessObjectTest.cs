using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ViewComponentChangeLogFilterBusinessObject))]
	public class ViewComponentChangeLogFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFromComponentFilter()
		{
			var workflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false), "Daniel's mum");
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.FromComponent];
			filter.IsActive = true;
			filter.Property = config.Buffer.PK;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestToComponentFilter()
		{
			var workflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false), "My spoon is too big");
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.ToComponent];
			filter.IsActive = true;
			filter.Property = anotherBucket.PK;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestWorkflowFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "I'm feeling fat");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "and sassy");

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.Workflow];
			filter.IsActive = true;
			filter.Property = workflow2.PK;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Bucket, anotherBucket, workflow2, TransferTypeList.Codes.ManualTransfer);
		}

		[TestDate(2015, 7, 14)]
		public void TestTransferTimeFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Silly hats only");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "I am the queen of France");

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			workflow2.FH_FC_CurrentComponent = anotherBucket.PK;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-1);

			var filter = (ModuleDateFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.TransferTime];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.Past;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Bucket, config.Buffer, workflow1, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestTransferTypeFilter()
		{
			FilterStripsTestHelper.AddFilterStrips(linkBetweenBufferAndBucket.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "PLZ TO HELP",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "OH MAN I AM NOT GOOD WITH COMPUTER");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "PLZ TO HELP");

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow1.FH_IsActive = false;

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();

			BMSTestCaseWithFactory.RunTransferRules(config.System);

			workflow1.Reload();
			workflow2.Reload();
			VisualBoardsTestCase.AssertSamePK(config.Buffer, workflow1.CurrentComponent);
			VisualBoardsTestCase.AssertSamePK(anotherBucket, workflow2.CurrentComponent);

			var filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.TransferType];
			filter.IsActive = true;
			filter.Property = TransferTypeList.Codes.SchematicTransfer;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow2, TransferTypeList.Codes.SchematicTransfer);
		}

		public void TestCCRStatusFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "IF YOU DON UPDUKE");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "U CAN HAZ LAF?");

			BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code);
			BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			workflow1.FH_FC_CurrentComponent = anotherBucket.PK;
			workflow2.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.CCRStatus];
			filter.IsActive = true;
			filter.Property = ConstraintStatusList.Codes.PreConstraint;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow1, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestWorkflowStatusFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "NO TU CAN H@Z SPOOKK");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "UPVOTEOH");

			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.WorkflowStatus];
			filter.IsActive = true;
			filter.Property = WorkflowStatusList.Codes.Open;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Bucket, config.Buffer, workflow1, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestUserFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Silly hats only");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "I am the queen of France");

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(config.CCR.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = true;

				workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

				Factory.Save();
			}

			var filter = (ModuleNkFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.User];
			filter.IsActive = true;
			filter.Property = config.CCR.GS_Code;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Bucket, config.Buffer, workflow2, TransferTypeList.Codes.ManualTransfer);
		}

		[TestDate(2015, 7, 14)]
		public void TestBufferPenetrationFilter()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Silly hats only");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "I am the queen of France");

			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-10);

			Factory.Save();

			workflow1.FH_FC_CurrentComponent = anotherBucket.PK;
			workflow2.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.BufferPenetration];
			filter.IsActive = true;
			filter.Property1 = 0.5m;
			filter.Property2 = 2m;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow2, TransferTypeList.Codes.ManualTransfer);
		}

		[TestDate(2015, 7, 14)]
		public void TestBufferZoneFilter()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Silly hats only");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "I am the queen of France");

			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-10);

			Factory.Save();

			workflow1.FH_FC_CurrentComponent = anotherBucket.PK;
			workflow2.FH_FC_CurrentComponent = anotherBucket.PK;

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.BufferZone];
			filter.IsActive = true;
			filter.Property1 = 1m;
			filter.Property2 = 2m;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);

			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, anotherBucket, workflow2, TransferTypeList.Codes.ManualTransfer);
		}

		public void TestIncludesTransferTimeByDefault()
		{
			var filterBizo = new ViewComponentChangeLogFilterBusinessObject();
			var filter = (ModuleDateFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.TransferTime];

			AssertEquals("Transfer Time", filter.Description);
			AssertEquals("This Week", filter.PropertySearch);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
		}

		[TestDate(2019, 1, 23)]
		public void TestDeferralReasonsFilter()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "I AM THE EGG MAN");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "I AM THE WALRUS");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "GOO GOO GA JOOB");

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;

			BMSTestHelper.Defer(workflow1, WorkflowDeferralReasonsList.Codes.PrioritiesChanged);
			BMSTestHelper.Defer(workflow2, WorkflowDeferralReasonsList.Codes.ResourcesOnLeave);
			BMSTestHelper.Defer(workflow3, WorkflowDeferralReasonsList.Codes.ResourcesOnLeave);

			Factory.Save();

			var filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.DeferralReason];
			filter.IsActive = true;
			filter.Property = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;

			var results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);
			AssertEquals(1, results.Length);
			AssertComponentChangeLog(results[0], config.Buffer, config.Bucket, workflow1, TransferTypeList.Codes.Defer);

			filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.DeferralReason];
			filter.IsActive = true;
			filter.Property = WorkflowDeferralReasonsList.Codes.ResourcesOnLeave;

			results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);
			AssertEquals(2, results.Length);
			AssertComponentChangeLog(results.Single(r => r.CCL_ParentId == workflow2.PK), config.Buffer, config.Bucket, workflow2, TransferTypeList.Codes.Defer);
			AssertComponentChangeLog(results.Single(r => r.CCL_ParentId == workflow3.PK), config.Buffer, config.Bucket, workflow3, TransferTypeList.Codes.Defer);

			filter = (ModuleTextFilter)filterBizo[ViewComponentChangeLog.ModuleFilterConstants.DeferralReason];
			filter.IsActive = true;
			filter.Property = WorkflowDeferralReasonsList.Codes.EstimatesExceeded;

			results = Factory.Load<ViewComponentChangeLog>(filterBizo.Filter);
			AssertEquals(0, results.Length);
		}

		#region Implementation

		static void AssertComponentChangeLog(ViewComponentChangeLog changeLog, BMComponent fromComponent, BMComponent toComponent, ProcessHeader workflow, string transferType)
		{
			VisualBoardsTestCase.AssertSamePK(fromComponent, changeLog.Factory.Load<BMComponent>(changeLog.CCL_FC_ComponentFrom));
			VisualBoardsTestCase.AssertSamePK(toComponent, changeLog.Factory.Load<BMComponent>(changeLog.CCL_FC_ComponentTo));
			VisualBoardsTestCase.AssertSamePK(workflow, changeLog.Factory.Load<ProcessHeader>(changeLog.CCL_ParentId));
			AssertEquals(transferType, changeLog.CCL_TransferType);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ViewComponentChangeLogFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			filterBizo = new ViewComponentChangeLogFilterBusinessObject();
			anotherBucket = BMSTestHelper.CreateBucket(config.System, "Zoidbucket");

			linkBetweenBufferAndBucket = BMSTestHelper.LinkComponents(config.Buffer, anotherBucket);
		}

		ViewComponentChangeLogFilterBusinessObject filterBizo;
		ConstrainedSchematicTestConfig config;
		BMComponent anotherBucket;
		BMComponentLink linkBetweenBufferAndBucket;

		#endregion
	}
}
