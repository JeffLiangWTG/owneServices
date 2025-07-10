using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowFactoryTest : BMSTestCaseWithFactory
	{
		#region SQL

		public void TestGetMatchingWorkflows_BySql()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 120, estVariationFactor: 1);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 180, estVariationFactor: 1);

			Factory.Save();

			var workflows = WorkflowFactory.GetMatchingWorkflows("SELECT * FROM dbo.ProcessHeader");

			AssertEquals(4, workflows.Result.Count);

			AssertEquals(360, workflows.Result[jobHeader.PK].PlannedDurationMinutes);
			AssertEquals(60, workflows.Result[workflow1.PK].PlannedDurationMinutes);
			AssertEquals(120, workflows.Result[workflow2.PK].PlannedDurationMinutes);
			AssertEquals(180, workflows.Result[workflow3.PK].PlannedDurationMinutes);
		}

		public void TestGetMatchingWorkflows_BySql_WithFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 120, estVariationFactor: 1);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 180, estVariationFactor: 1);

			Factory.Save();

			var workflows = WorkflowFactory.GetMatchingWorkflows("SELECT * FROM dbo.ProcessHeader WHERE FH_FH_ParentHeader is not null");

			AssertEquals(3, workflows.Result.Count);

			AssertEquals(60, workflows.Result[workflow1.PK].PlannedDurationMinutes);
			AssertEquals(120, workflows.Result[workflow2.PK].PlannedDurationMinutes);
			AssertEquals(180, workflows.Result[workflow3.PK].PlannedDurationMinutes);
		}

		#endregion

		#region Acceptability Band

		[TestDate(2016, 4, 20)]
		public void TestGetMatchingWorkflows_ByAcceptabilityBand_SupersetNUP()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var nupRule = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Nope", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			FilterStripsTestHelper.AddFilterStrips(nupRule.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			FilterStripsTestHelper.AddFilterStrips(nupRule.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = "Today",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Look out!", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Look out!", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow2.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 19);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow3.FH_AgreedDeliveryDate = new CargoWise.Types.ZDateTime(2016, 4, 20);

			Factory.Save();

			var workflows = WorkflowFactory.GetMatchingWorkflows(nupRule, new AcceptabilityBandSqlBuilderParameters(nupRule) { ReleaseGroupPK = config.ReleaseGroup.PK });
			AssertEquals(1, workflows.Result.Count);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1.PK }, workflows.Result.Keys);
		}

		public void TestGetMatchingWorkflows_ByAcceptabilityBand_SupersetDUP()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var dupRule = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Nope", type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			var def1 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.PlannedDuration,
					FilterStripValueSetter = f => {
						var filter = (ModuleDurationFilter)f;
						filter.MinDurationMinutes = 60;
						filter.MaxDurationMinutes = 120;
						filter.Scope = "Between";
						filter.IsActive = true;
					},
				};
			var def2 =
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
					FilterStripValueSetter = f => { ((ModuleFlagsFilter)f).Property1 = true; }, // workflow
				};
			FilterStripsTestHelper.AddFilterStrips(dupRule.FilterRule, def1, def2);

			FilterStripsTestHelper.AddFilterStrips(dupRule.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Boutros!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Boutros!", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow1.FH_PlannedDurationInMinutes = 60;
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Boutros!", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow2.FH_PlannedDurationInMinutes = 180;
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 180);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Ghali!", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			workflow3.FH_PlannedDurationInMinutes = 60;
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var workflows = WorkflowFactory.GetMatchingWorkflows(dupRule, new AcceptabilityBandSqlBuilderParameters(dupRule) { ReleaseGroupPK = config.ReleaseGroup.PK });
			AssertEquals(1, workflows.Result.Count);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1.PK }, workflows.Result.Keys);
		}

		#endregion

		#region Implementation

		DisposableList disposables;

		protected override void SetUp()
		{
			base.SetUp();

			disposables = new DisposableList(new[] { BMSTestCaseWithFactory.DisableAsyncBehaviour() });
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		#endregion
	}

	class WorkflowFactoryNonTransactionedTest : NonTransactionedTestCase
	{
		#region SQL

		public void TestGetMatchingWorkflows_BySql()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "joob");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 120, estVariationFactor: 1);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 180, estVariationFactor: 1);

			Factory.Save();

			var workflows = WorkflowFactory.GetMatchingWorkflows("SELECT * FROM dbo.ProcessHeader");

			AssertEquals(4, workflows.Result.Count);

			AssertEquals(360, workflows.Result[jobHeader.PK].PlannedDurationMinutes);
			AssertEquals(60, workflows.Result[workflow1.PK].PlannedDurationMinutes);
			AssertEquals(120, workflows.Result[workflow2.PK].PlannedDurationMinutes);
			AssertEquals(180, workflows.Result[workflow3.PK].PlannedDurationMinutes);
		}

		#endregion
	}
}
