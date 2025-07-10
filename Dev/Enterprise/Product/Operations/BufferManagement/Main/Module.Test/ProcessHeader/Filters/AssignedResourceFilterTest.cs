using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(AssignedResourceFilter))]
	class AssignedResourceFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
		}

		#region Filtration

		public void TestAssignedResourceFilter_Exact()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ZAI", "Dr Zaius");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "WHO", "Dr Whom");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Doctoren");

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Get your paws off me you dirty ape!");
			var task11 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 30, description: "He can talk he can talk he can talk");
			var task12 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 30, description: "I can siiiiiiiiiiiiiiiiiing!");

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Hey! The Tardis!");
			var task21 = BMSTestHelper.CreateTask(workflow2, staff2.GS_Code, 30, description: "Posh posh posh");
			var task22 = BMSTestHelper.CreateTask(workflow2, staff2.GS_Code, 30, description: "Loadsamoney");

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "I hate every ape I see");
			var task31 = BMSTestHelper.CreateTask(workflow3, staff1.GS_Code, 30, description: "From chimpan-A to chimpanzee");
			var task32 = BMSTestHelper.CreateTask(workflow3, staff1.GS_Code, 30, description: "Oh you'll never make a monkey out of me!");

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Exterminate!");
			var task41 = BMSTestHelper.CreateTask(workflow4, staff2.GS_Code, 30, description: "We obey no one");
			var task42 = BMSTestHelper.CreateTask(workflow4, staff2.GS_Code, 30, description: "We are the superior beings");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var assignedResourceFilter = (AssignedResourceFilter)filterBizo[ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask];
			assignedResourceFilter.IsActive = true;
			assignedResourceFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			assignedResourceFilter.Property = staff1.GS_Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(3, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader.FH_CompletionStatement, workflow1.FH_CompletionStatement, workflow3.FH_CompletionStatement }, processHeaderResult);

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetWorkflowOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(2, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { workflow1.FH_CompletionStatement, workflow3.FH_CompletionStatement }, processHeaderResult);
		}

		public void TestAssignedResourceFilter_HasNoNotEqualOperator()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var assignedResourceFilter = (AssignedResourceFilter)filterBizo[ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask];
			AssertEquals(false, assignedResourceFilter.ComparisonOperator_List.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
		}

		public void TestIdenticalFilterTypeInSameOrCategory_DoesNotThrowException()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceA = (AssignedResourceFilter)stripARA.CurrentModuleFilter;
			assignedResourceA.Property = "AAA";
			assignedResourceA.IsActive = true;
			assignedResourceA.OrCategory = FilterOrCategory.Green;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceB = (AssignedResourceFilter)stripARB.CurrentModuleFilter;
			assignedResourceB.Property = "BBB";
			assignedResourceB.IsActive = true;
			assignedResourceB.OrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 2, filterBizo.FilterStrips.Count);
			AssertEquals("active", 2, filterBizo.ActiveModuleFilters.Count);

			var headers = Factory.Load<ProcessHeader>(filterBizo.Filter);
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
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

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
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

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
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
			stripARA.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterA = (AssignedResourceFilter)stripARA.CurrentModuleFilter;
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
			stripARB.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterB = (AssignedResourceFilter)stripARB.CurrentModuleFilter;
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
			stripARA.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterA = (AssignedResourceFilter)stripARA.CurrentModuleFilter;
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
			stripARB.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterB = (AssignedResourceFilter)stripARB.CurrentModuleFilter;
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
			stripARA.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterA = (AssignedResourceFilter)stripARA.CurrentModuleFilter;
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
			stripARAB.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterAB = (AssignedResourceFilter)stripARA.CurrentModuleFilter;
			assignedResourceFilterAB.Property = "AAA";
			assignedResourceFilterAB.IsActive = true;
			assignedResourceFilterAB.GroupName = "A";
			assignedResourceFilterAB.GroupOrCategory = FilterOrCategory.Red;
			assignedResourceFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Resource Assigned To Any Task";
			var assignedResourceFilterB = (AssignedResourceFilter)stripARB.CurrentModuleFilter;
			assignedResourceFilterB.Property = "BBB";
			assignedResourceFilterB.IsActive = true;
			assignedResourceFilterB.GroupName = "B";
			assignedResourceFilterB.GroupOrCategory = FilterOrCategory.Green;
			assignedResourceFilterB.OrCategory = FilterOrCategory.None;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;

			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;
			jobOrWorkflowB.OrCategory = FilterOrCategory.None;

			AssertEquals("strips", 5, filterBizo.FilterStrips.Count);
			AssertEquals("active", 5, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(2, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		#endregion
	}
}
