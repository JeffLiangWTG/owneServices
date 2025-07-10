using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(CapabilityFilter))]
	class CapabilityFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (CapabilityFilter)filterBizo["Capability Required on Any Task"];
		}

		#region Filtration

		public void TestIdenticalFilterTypeInSameOrCategory_DoesNotThrowException()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterA = (CapabilityFilter)stripARA.CurrentModuleFilter;
			capabilityFilterA.Property = ZGuid.BrettsGuid;
			capabilityFilterA.IsActive = true;
			capabilityFilterA.OrCategory = FilterOrCategory.Green;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterB = (CapabilityFilter)stripARB.CurrentModuleFilter;
			capabilityFilterB.Property = ZGuid.BrettsGuid;
			capabilityFilterB.IsActive = true;
			capabilityFilterB.OrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 2, filterBizo.FilterStrips.Count);
			AssertEquals("active", 2, filterBizo.ActiveModuleFilters.Count);

			var headers = Factory.Load<ProcessHeader>(filterBizo.Filter);
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var capabilityFilter = (CapabilityFilter)filterBizo["Capability Required on Any Task"];
			capabilityFilter.Property = ZGuid.BrettsGuid;
			capabilityFilter.IsActive = true;

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

			var capabilityFilter = (CapabilityFilter)filterBizo["Capability Required on Any Task"];
			capabilityFilter.Property = ZGuid.BrettsGuid;
			capabilityFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

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

			var capabilityFilter = (CapabilityFilter)filterBizo["Capability Required on Any Task"];
			capabilityFilter.Property = ZGuid.BrettsGuid;
			capabilityFilter.IsActive = true;

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

			AssertEquals(3, filterBizo.ActiveModuleFilters.Count);
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied_FourFiltersApplied_TwoBlue()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

			var capabilityFilter = (CapabilityFilter)filterBizo["Capability Required on Any Task"];
			capabilityFilter.Property = ZGuid.BrettsGuid;
			capabilityFilter.IsActive = true;

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var currentComponentFilter = (CurrentComponentFilter)filterBizo[ProcessHeader.ModuleFilterConstants.CurrentComponent];
			currentComponentFilter.Property = ZGuid.BrettsGuid;
			currentComponentFilter.IsActive = true;
			currentComponentFilter.OrCategory = FilterOrCategory.Blue;

			AssertEquals(4, filterBizo.ActiveModuleFilters.Count);
			AssertContains("UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertContains("UNION should still be applied", "UNION", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_JobFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterA = (CapabilityFilter)stripARA.CurrentModuleFilter;
			capabilityFilterA.Property = ZGuid.BrettsGuid;
			capabilityFilterA.IsActive = true;
			capabilityFilterA.GroupName = "A";
			capabilityFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetJobOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterB = (CapabilityFilter)stripARB.CurrentModuleFilter;
			capabilityFilterB.Property = ZGuid.BrettsGuid;
			capabilityFilterB.IsActive = true;
			capabilityFilterB.GroupName = "B";
			capabilityFilterB.GroupOrCategory = FilterOrCategory.Green;

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
			stripARA.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterA = (CapabilityFilter)stripARA.CurrentModuleFilter;
			capabilityFilterA.Property = ZGuid.BrettsGuid;
			capabilityFilterA.IsActive = true;
			capabilityFilterA.GroupName = "A";
			capabilityFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterB = (CapabilityFilter)stripARB.CurrentModuleFilter;
			capabilityFilterB.Property = ZGuid.BrettsGuid;
			capabilityFilterB.IsActive = true;
			capabilityFilterB.GroupName = "B";
			capabilityFilterB.GroupOrCategory = FilterOrCategory.Green;

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
			stripARA.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterA = (CapabilityFilter)stripARA.CurrentModuleFilter;
			capabilityFilterA.Property = ZGuid.BrettsGuid;
			capabilityFilterA.IsActive = true;
			capabilityFilterA.GroupName = "A";
			capabilityFilterA.GroupOrCategory = FilterOrCategory.Red;
			capabilityFilterA.OrCategory = FilterOrCategory.Yellow;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;
			jobOrWorkflowA.IsActive = true;

			var stripARAB = filterBizo.FilterStrips.AddNew();
			stripARAB.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterAB = (CapabilityFilter)stripARAB.CurrentModuleFilter;
			capabilityFilterAB.Property = ZGuid.BrettsGuid;
			capabilityFilterAB.IsActive = true;
			capabilityFilterAB.GroupName = "A";
			capabilityFilterAB.GroupOrCategory = FilterOrCategory.Red;
			capabilityFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = "Capability Required on Any Task";
			var capabilityFilterB = (CapabilityFilter)stripARB.CurrentModuleFilter;
			capabilityFilterB.Property = ZGuid.BrettsGuid;
			capabilityFilterB.IsActive = true;
			capabilityFilterB.GroupName = "B";
			capabilityFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;

			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 5, filterBizo.FilterStrips.Count);
			AssertEquals("active", 5, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(3, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		#endregion
	}
}
