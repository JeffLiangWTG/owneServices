using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(AssignedResourceOnCurrentTaskFilter))]
	class AssignedResourceOnCurrentTaskTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
		}
	}

	class AssignedResourceOnCurrentTaskFilterTest : BMSTestCaseWithFactory
	{
		public void TestFilterContainsGetCurrentTasksInWorkflows_WhenQIEnabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var assignedResourceFilter = (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var query = filterBizo.Filter;
			CombineAssertions("The query should contain a reference to the GetCurrentTasksInWorkflows function when Quality Iterations is enabled.", () =>
			{
				AssertContains(BMGlobalConstants.CurrentTasksInWorkflowsSQLFunctionText + "()", query.LiteralTextADO);
				AssertNotContains(BMGlobalConstants.CurrentTasksInWorkflowsIgnoringIterationsSQLFunctionText + "()", query.LiteralTextADO);
			});
		}

		public void TestFilterContainsGetCurrentTasksInWorkflowsIgnoringIterations_WhenQIDisabled()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var assignedResourceFilter = (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var query = filterBizo.Filter;
			CombineAssertions("The query should contain a reference to the GetCurrentTasksInWorkflowsIgnoringIterations function when Quality Iterations is disabled.", () =>
			{
				AssertNotContains(BMGlobalConstants.CurrentTasksInWorkflowsSQLFunctionText + "()", query.LiteralTextADO);
				AssertContains(BMGlobalConstants.CurrentTasksInWorkflowsIgnoringIterationsSQLFunctionText + "()", query.LiteralTextADO);
			});
		}

		public void TestIdenticalFilterTypeInSameOrCategory_DoesNotThrowException()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceA = (AssignedResourceOnCurrentTaskFilter)stripARA.CurrentModuleFilter;
			assignedResourceA.Property = "AAA";
			assignedResourceA.IsActive = true;
			assignedResourceA.OrCategory = FilterOrCategory.Green;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceB = (AssignedResourceOnCurrentTaskFilter)stripARB.CurrentModuleFilter;
			assignedResourceB.Property = "BBB";
			assignedResourceB.IsActive = true;
			assignedResourceB.OrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 2, filterBizo.FilterStrips.Count);
			AssertEquals("active", 2, filterBizo.ActiveModuleFilters.Count);

			var query = filterBizo.Filter;
			AssertNoExceptionThrown(query.LiteralTextADO, () => Factory.Load<ProcessHeader>(query));
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var assignedResourceFilter = (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

			AssertEquals(2, filterBizo.ActiveModuleFilters.Count);
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var assignedResourceFilter = (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;

			AssertEquals(2, filterBizo.ActiveModuleFilters.Count);
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied_ExtraFilterApplied()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var assignedResourceFilter = (AssignedResourceOnCurrentTaskFilter)filterBizo["Resource Assigned To Current Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var capabilityFilter = (CapabilityFilter)filterBizo["Capability Required on Any Task"];
			capabilityFilter.Property = ZGuid.BrettsGuid;
			capabilityFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;

			AssertEquals(3, filterBizo.ActiveModuleFilters.Count);
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_JobFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterA = (AssignedResourceOnCurrentTaskFilter)stripARA.CurrentModuleFilter;
			assignedResourceFilterA.Property = "AAA";
			assignedResourceFilterA.IsActive = true;
			assignedResourceFilterA.GroupName = "A";
			assignedResourceFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetJobOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterB = (AssignedResourceOnCurrentTaskFilter)stripARB.CurrentModuleFilter;
			assignedResourceFilterB.Property = "BBB";
			assignedResourceFilterB.IsActive = true;
			assignedResourceFilterB.GroupName = "B";
			assignedResourceFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetWorkflowOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(1, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_WorkflowFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterA = (AssignedResourceOnCurrentTaskFilter)stripARA.CurrentModuleFilter;
			assignedResourceFilterA.Property = "AAA";
			assignedResourceFilterA.IsActive = true;
			assignedResourceFilterA.GroupName = "A";
			assignedResourceFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterB = (AssignedResourceOnCurrentTaskFilter)stripARB.CurrentModuleFilter;
			assignedResourceFilterB.Property = "BBB";
			assignedResourceFilterB.IsActive = true;
			assignedResourceFilterB.GroupName = "B";
			assignedResourceFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;

			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(1, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_OrCategory()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterA = (AssignedResourceOnCurrentTaskFilter)stripARA.CurrentModuleFilter;
			assignedResourceFilterA.Property = "AAA";
			assignedResourceFilterA.IsActive = true;
			assignedResourceFilterA.GroupName = "A";
			assignedResourceFilterA.GroupOrCategory = FilterOrCategory.Red;
			assignedResourceFilterA.OrCategory = FilterOrCategory.Yellow;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;
			jobOrWorkflowA.IsActive = true;

			var stripARAB = filterBizo.FilterStrips.AddNew();
			stripARAB.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterAB = (AssignedResourceOnCurrentTaskFilter)stripARA.CurrentModuleFilter;
			assignedResourceFilterAB.Property = "AAA";
			assignedResourceFilterAB.IsActive = true;
			assignedResourceFilterAB.GroupName = "A";
			assignedResourceFilterAB.GroupOrCategory = FilterOrCategory.Red;
			assignedResourceFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Current Task";
			var assignedResourceFilterB = (AssignedResourceOnCurrentTaskFilter)stripARB.CurrentModuleFilter;
			assignedResourceFilterB.Property = "BBB";
			assignedResourceFilterB.IsActive = true;
			assignedResourceFilterB.GroupName = "B";
			assignedResourceFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;

			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 5, filterBizo.FilterStrips.Count);
			AssertEquals("active", 5, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(2, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		public void TestWorkflow_WithOpenQualityIteration_WhichWouldOtherwiseMatch_ShouldNotMatch()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");
			var staff3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RAN", "The Rankin Family");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var mainWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Main Workflow");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("UDF", "INQ");

			var task1 = BMSTestHelper.CreateTask(mainWorkflow, "RIT", taskStatus: "ASN", sequence: 2);
			var task2 = BMSTestHelper.CreateTask(mainWorkflow, "ANN", taskStatus: "CLS", sequence: 3);
			var task3 = BMSTestHelper.CreateTask(mainWorkflow, "RAN", taskStatus: "ASN", sequence: 4);
			var iteration = CreateQualityIteration(task1, task2);
			var task4 = iteration.Tasks.Single(x => x.P9_GS_NKAssignedStaffMember == "RIT");
			var task5 = iteration.Tasks.Single(x => x.P9_GS_NKAssignedStaffMember == "ANN");
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			filter.Property = "RIT";
			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("There is an open task earlier than the CB task, so its workflow should be matched, and yet...", new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.Property = "RAN";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The assigned task comes after the open quality iteration, so its workflow shouldn't be matched, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			filter.Property = "ANN";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("There is a matching workflow in the quality iteration, so the QI should be matched, and yet...", new[] { "Main Workflow (Quality Iteration 1)", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(WorkflowStatusList.Codes.Closed, iteration.FH_Status);
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The QI is now closed, but the filter value is still looking for tasks assigned to a resource that doesn't have any open tasks. Therefore there should be no match, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			filter.Property = "RIT";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The QI is closed, and it's still the earliest task, so its workflow should still be matched, and yet...", new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.Property = "RAN";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("There's a matching task, but it's not the earliest, so its workflow shouldn't be matched, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Now it is the earliest task so its workflow should match, and yet...", new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned; // Reopen the QI
			Factory.Save();
			AssertEquals(WorkflowStatusList.Codes.Open, iteration.FH_Status);
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The QI was reopened, and the matching task is later in sequence than the task that caused the QI, so its workflow shouldn't be matched, and yet...", Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			var preTask = BMSTestHelper.CreateTask(mainWorkflow, "RAN", taskStatus: "ASN", sequence: 1);
			Factory.Save();
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			var jobFilter = (JobOrWorkflowFilter)strip.CurrentModuleFilter;
			jobFilter.Property1 = true;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Main Workflow" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestWorkflow_WithNonQualityIterationChildWorkflow_ShouldStillMatch()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var mainWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Main Workflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Child Workflow");
			BMSTestHelper.MakeChildOf(childWorkflow, mainWorkflow);

			var task1 = BMSTestHelper.CreateTask(mainWorkflow, "RIT", taskStatus: "ASN", sequence: 2);
			var task2 = BMSTestHelper.CreateTask(childWorkflow, "ANN", taskStatus: "ASN", sequence: 3);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			filter.Property = "RIT";
			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.Property = "ANN";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Child Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			task2.P9_Status = "CLS";
			Factory.Save();
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			filter.Property = "RIT";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Main Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			var jobFilter = (JobOrWorkflowFilter)strip.CurrentModuleFilter;
			jobFilter.Property1 = true;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Main Workflow" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestFiltersMatch()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Rita");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Anne");

			var task1 = BMSTestHelper.CreateTask(workflow1, "RIT", taskStatus: "ASN", sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, "ANN", taskStatus: "ASN", sequence: 2);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			AssertEquals("Filters match should be supported, so when the operator is set to filters match, it should not fail to set the value properly, and yet...", ModuleTextFilter.ComparisonConstants.FiltersMatch, filter.ComparisonOperator);

			filter.SelectedFilters.AddTextFilterStrip("Full Name", "Anne");

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(filterBizo.Filter.LiteralTextADO, new[] { "Workflow with Anne", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestComparisonOperators()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");
			var capability = BMSTestHelper.CreateCapability(Factory, "ALL", "All Users");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Rita");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Anne");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with unassigned task");

			var task1 = BMSTestHelper.CreateTask(workflow1, "RIT", taskStatus: "ASN", sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, "ANN", taskStatus: "ASN", sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow3, capability: capability, taskStatus: "ASN", sequence: 3);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			var testedComparisonOperators = new List<string> { ModuleTextFilter.ComparisonConstants.FiltersMatch, ModuleTextFilter.ComparisonConstants.Exact, string.Empty }; // These are tested in other tests.
			filter.ComparisonOperatorChanged += (_, x_) => testedComparisonOperators.Add(filter.ComparisonOperator);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = "RIT";

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(filterBizo.Filter.LiteralTextADO, new[] { "Workflow with Anne", "Workflow with unassigned task", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(filterBizo.Filter.LiteralTextADO, new[] { "Workflow with unassigned task", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(filterBizo.Filter.LiteralTextADO, new[] { "Workflow with Rita", "Workflow with Anne", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			using (Env.SetTemporaryUserContext(staff1.GS_LoginName, Guid.Empty, Guid.Empty))
			{
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
				results = Factory.Load<ProcessHeader>(filterBizo.Filter);
				AssertContainsExactElementsInAnyOrder(filterBizo.Filter.LiteralTextADO, new[] { "Workflow with Rita", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));
			}

			AssertContainsExactElementsInAnyOrder("All of the supported comparison operators should have been tested, and yet... If you've added a new supported operator, please test it here.", filter.AllowedComparisonOperators, testedComparisonOperators);
		}

		public void TestComparisonOperators_UnsupportedFilters_ResourceAssignedToCurrentTaskFilter()
		{
			BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");
			var capability = BMSTestHelper.CreateCapability(Factory, "ALL", "All Users");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Rita");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with Anne");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow with unassigned task");

			BMSTestHelper.CreateTask(workflow1, "RIT", taskStatus: "ASN", sequence: 1);
			BMSTestHelper.CreateTask(workflow2, "ANN", taskStatus: "ASN", sequence: 2);
			BMSTestHelper.CreateTask(workflow3, capability: capability, taskStatus: "ASN", sequence: 3);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			filter.ComparisonOperator = "?";

			ProcessHeader[] results = Array.Empty<ProcessHeader>();
			AssertNoExceptionThrown(() => results = Factory.Load<ProcessHeader>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "Job Inquiry is complete.", "Workflow with unassigned task" }, results.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = "hello";

			AssertNoExceptionThrown(() => results = Factory.Load<ProcessHeader>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "Job Inquiry is complete.", "Workflow with unassigned task" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestWorkflow_WithSameSequenceNoTasks_ButOneClosed()
		{
			BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIT", "Rita MacNeil");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ANN", "Anne Murray");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = BMSTestHelper.CreateTask(workflow, "RIT", taskStatus: "ASN", sequence: 2);
			var task2 = BMSTestHelper.CreateTask(workflow, "ANN", taskStatus: "ASN", sequence: 2);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask);
			var filter = (AssignedResourceOnCurrentTaskFilter)strip.CurrentModuleFilter;

			filter.Property = "RIT";
			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			filter.Property = "ANN";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));

			task2.P9_Status = "CLS";
			Factory.Save();
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			filter.Property = "RIT";
			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow", "Job Inquiry is complete." }, results.Select(x => x.FH_CompletionStatement));
		}
	}
}
