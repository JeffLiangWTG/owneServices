using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDateDefaultsFromList()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: "ORG");

			CombineAssertions(() =>
			{
				foreach (var workflowDescriptor in WorkflowDescriptors.Instance.Values)
				{
					template.P0_ProcessType = workflowDescriptor.Code;
					var workflow = template.ProcessHeaders.AddNew() as ProcessHeader;

					AssertContainsExactElementsInAnyOrder(
						$"Process-type = {workflowDescriptor.Code}",
						workflowDescriptor.EstimateDefaultedFromList,
						workflow.Lookups.DatesDefaultsFromList);
				}
			});
		}

		public void TestAddFilterDefaults_WithNullParentForSomeReason()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			AssertNoExceptionThrown(() => ProcessHeaderLookups.AddFilterDefaults(jobHeader, new ProcessHeaderCollection(Factory)));
		}

		[ExpectNoExceptions]
		public void TestShouldNotAccessComponentOnDeletedProcessHeader()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroup1 = BMSTestHelper.CreateReleaseGroup(system, group1);
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(system, group2);

			var buffer = BMSTestHelper.CreateBuffer(system);

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(job, buffer, "Another One Bites the Dust", ZDateTime.Now);

			workflow1.Delete();

			AssertEquals("Lookups should find all available release groups, regardless of system", 7, workflow1.Lookups.AllReleaseGroups.Count);
		}

		public void TestAllReleaseGroups_ForJob_ShouldIncludeGroupsInSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroup1 = system.ReleaseGroups.AddNew();
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			releaseGroup2.FSG_GG_Group = group2.PK;

			var job1 = Factory.New<OrgHeader>();
			var workflow1 = ProcessJobHeader.GetForParent(job1, Factory).ProcessHeaders[0];
			AssertEquals(system, workflow1.BMSystem);
			AssertCollectionContains(group1, workflow1.Lookups.AllReleaseGroups);
			AssertCollectionContains(group2, workflow1.Lookups.AllReleaseGroups);
		}

		public void TestAllReleaseGroups_ForTemplate_ShouldIncludeGroupsProcessTypeInSystem()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			BMSTestHelper.CreateReleaseGroup(system, group1);

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var workflow = (ProcessHeader)template.ProcessHeaders.AddNew();

			AssertEquals(null, workflow.ReleaseGroup);
			AssertEquals(true, workflow.FH_GG_ReleaseGroup.IsEmpty);
			AssertEquals("Lookups should find all available release groups, regardless of system", 7, workflow.Lookups.AllReleaseGroups.Count);
			AssertNotNull(workflow.BMSystem);

			AssertCollectionContains(group1, workflow.Lookups.AllReleaseGroups);
			AssertCollectionContains(group2, workflow.Lookups.AllReleaseGroups);
		}

		public void TestCompletionMilestones()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var header = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = header.ProcessHeaders.AddNew();

			AssertEquals(0, workflow.Lookups.CompletionMilestones.Count);

			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			var milestone2 = job.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			var milestone3 = job.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.ArrivalDocumentationReceivedCode;
			milestone3.P9_Description = "Mile";

			var actual = workflow.Lookups.CompletionMilestones
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			var expected = job.WorkflowItems.CompletionMilestoneCodeDescriptionPairList
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		public void TestCompletionMilestones_WhenTemplate()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			AssertEquals(0, workflow.Lookups.CompletionMilestones.Count);

			var milestone1 = template.WorkflowItems.Milestones.AddNew();
			var milestone2 = template.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			var milestone3 = template.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.ArrivalDocumentationReceivedCode;
			milestone3.P9_Description = "Mile";

			var actual = workflow.Lookups.CompletionMilestones
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			var expected = template.WorkflowItems.CompletionMilestoneCodeDescriptionPairList
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		public void TestHeadersWithDefaultFilters()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job1 = Factory.New<OrgHeader>();
			job1.OH_Code = "ORG1";
			var job1Header = ProcessJobHeader.GetForParent(job1, Factory);
			var header1A = job1Header.ProcessHeaders[0];
			var header1B = job1Header.ProcessHeaders.AddNew();
			var header1C = job1Header.ProcessHeaders.AddNew();

			var job2 = Factory.New<OrgHeader>();
			job2.OH_Code = "ORG2";
			var job2Header = ProcessJobHeader.GetForParent(job2, Factory);
			var header2A = job2Header.ProcessHeaders[0];
			var header2B = job2Header.ProcessHeaders.AddNew();
			var header2C = job2Header.ProcessHeaders.AddNew();

			AssertEquals(8, header1A.Lookups.HeadersWithDefaultFilters.Count);

			var aboc = header1A.Lookups.HeadersWithDefaultFilters as ActiveBusinessObjectCollection<ProcessHeader>;
			AssertNotNull(aboc);
			Assert(aboc.FilterBusinessObjectDefaults.ContainsDefaultFor(ProcessHeader.ModuleFilterConstants.JobCode + ":WorkflowTypeCode"));
			AssertEquals("ORG", aboc.FilterBusinessObjectDefaults[ProcessHeader.ModuleFilterConstants.JobCode + ":WorkflowTypeCode"].Value);
			Assert(aboc.FilterBusinessObjectDefaults.ContainsDefaultFor(ProcessHeader.ModuleFilterConstants.JobCode + ":Property"));
			AssertEquals("ORG1", aboc.FilterBusinessObjectDefaults[ProcessHeader.ModuleFilterConstants.JobCode + ":Property"].Value);
		}

		public void TestWorkflowCategories_ReturnsCategories_WhenWorkflowTypeUsed()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "TS1", "Test category");
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "TS2", "Test category");
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");

			var categoryCodes = workflow.Lookups.WorkflowCategories.OfType<WorkflowCategory>().Select(wfc => wfc.Code);

			AssertContainsExactElementsInAnyOrder(new[] { "TS1", "TS2" }, categoryCodes);
		}

		public void TestWorkflowCategories_ReturnsCategories_WhenProcessTypeUsed()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS1", "Test category");
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS2", "Test category");
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			var categoryCodes = workflow.Lookups.WorkflowCategories.OfType<WorkflowCategory>().Select(wfc => wfc.Code);

			AssertContainsExactElementsInAnyOrder(new[] { "TS1", "TS2" }, categoryCodes);
		}

		public void TestWorkflowCategories_ReturnsDefaultCategories_WhenNoCategoriesConfigured()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");

			var categoryCodes = workflow.Lookups.WorkflowCategories.OfType<WorkflowCategory>().Select(wfc => wfc.Code);

			AssertContainsExactElementsInAnyOrder(new[] { "UDF" }, categoryCodes);
		}

		public void TestWorkflowCategories_WhenUsedForDifferentWorkflowTypesInSameFactory_ShouldReturnDifferentValues()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS1", "Test category 1");
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, "TS2", "Test category 2");

			BMSTestHelper.CreateSystem(Factory, "ORG", "INQ");
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");

			AssertContainsExactElementsInAnyOrder("The lookups for both workflows should return the correct collection even when sharing a factory. SAD!", new[] { "TS1" }, workflow1.Lookups.WorkflowCategories.Cast<WorkflowCategory>().Select(x => x.Code));
			AssertContainsExactElementsInAnyOrder("The lookups for both workflows should return the correct collection even when sharing a factory. SAD!", new[] { "TS2" }, workflow2.Lookups.WorkflowCategories.Cast<WorkflowCategory>().Select(x => x.Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
