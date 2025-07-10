using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module.Test
{
	abstract class ComponentFilterTestCase<T> : NonPersistentBusinessObjectTestCase where T : ComponentFilterBase
	{
		protected abstract string componentFilterDescription { get; }

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (T)filterBizo[componentFilterDescription];
		}

		protected abstract void SetComponent(ProcessHeader workflow, ZGuid value);

		#region Generated SQL Queries

		public void TestIdenticalFilterTypeInSameOrCategory_DoesNotThrowException()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripARA = filterBizo.FilterStrips.AddNew();
			stripARA.FilterDescription = componentFilterDescription;
			var componentFilterA = (T)stripARA.CurrentModuleFilter;
			componentFilterA.Property = ZGuid.BrettsGuid;
			componentFilterA.IsActive = true;
			componentFilterA.OrCategory = FilterOrCategory.Green;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = componentFilterDescription;
			var componentFilterB = (T)stripARB.CurrentModuleFilter;
			componentFilterB.Property = ZGuid.BrettsGuid;
			componentFilterB.IsActive = true;
			componentFilterB.OrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 2, filterBizo.FilterStrips.Count);
			AssertEquals("active", 2, filterBizo.ActiveModuleFilters.Count);

			var headers = Factory.Load<ProcessHeader>(filterBizo.Filter);
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.Property = ZGuid.BrettsGuid;
			componentFilter.IsActive = true;

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

			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.Property = ZGuid.BrettsGuid;
			componentFilter.IsActive = true;

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

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied_ExtraFilterApplied()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.Property = ZGuid.BrettsGuid;
			componentFilter.IsActive = true;
			componentFilter.OrCategory = FilterOrCategory.LemonChiffon;

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

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
			stripARA.FilterDescription = componentFilterDescription;
			var componentFilterA = (T)stripARA.CurrentModuleFilter;
			componentFilterA.Property = ZGuid.BrettsGuid;
			componentFilterA.IsActive = true;
			componentFilterA.GroupName = "A";
			componentFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetJobOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = componentFilterDescription;
			var componentFilterB = (T)stripARB.CurrentModuleFilter;
			componentFilterB.Property = ZGuid.BrettsGuid;
			componentFilterB.IsActive = true;
			componentFilterB.GroupName = "B";
			componentFilterB.GroupOrCategory = FilterOrCategory.Green;

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
			stripARA.FilterDescription = componentFilterDescription;
			var componentFilterA = (T)stripARA.CurrentModuleFilter;
			componentFilterA.Property = ZGuid.BrettsGuid;
			componentFilterA.IsActive = true;
			componentFilterA.GroupName = "A";
			componentFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = componentFilterDescription;
			var componentFilterB = (T)stripARB.CurrentModuleFilter;
			componentFilterB.Property = ZGuid.BrettsGuid;
			componentFilterB.IsActive = true;
			componentFilterB.GroupName = "B";
			componentFilterB.GroupOrCategory = FilterOrCategory.Green;

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
			stripARA.FilterDescription = componentFilterDescription;
			var componentFilterA = (T)stripARA.CurrentModuleFilter;
			componentFilterA.Property = ZGuid.BrettsGuid;
			componentFilterA.IsActive = true;
			componentFilterA.GroupName = "A";
			componentFilterA.GroupOrCategory = FilterOrCategory.Red;
			componentFilterA.OrCategory = FilterOrCategory.Yellow;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;
			jobOrWorkflowA.IsActive = true;

			var stripARAB = filterBizo.FilterStrips.AddNew();
			stripARAB.FilterDescription = componentFilterDescription;
			var componentFilterAB = (T)stripARAB.CurrentModuleFilter;
			componentFilterAB.Property = ZGuid.BrettsGuid;
			componentFilterAB.IsActive = true;
			componentFilterAB.GroupName = "A";
			componentFilterAB.GroupOrCategory = FilterOrCategory.Red;
			componentFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripARB = filterBizo.FilterStrips.AddNew();
			stripARB.FilterDescription = componentFilterDescription;
			var componentFilterB = (T)stripARB.CurrentModuleFilter;
			componentFilterB.Property = ZGuid.BrettsGuid;
			componentFilterB.IsActive = true;
			componentFilterB.GroupName = "B";
			componentFilterB.GroupOrCategory = FilterOrCategory.Green;

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

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_OrCategory_Simple()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = filterBizo.AddGuidFilterStrip(componentFilterDescription, ZGuid.BrettsGuid);
			var jobOrWorkflowFilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			jobOrWorkflowFilter.SetWorkflowOnly();

			var query = filterBizo.Filter.LiteralTextSqlFormatted;
			AssertEquals("The filters aren't in an OR category, so the optimisation should occur. SAD! " + query, 0, query.AllIndexesOf("UNION ALL").Count());

			componentFilter.OrCategory = FilterOrCategory.GhostWhite;
			jobOrWorkflowFilter.OrCategory = FilterOrCategory.GhostWhite;

			query = filterBizo.Filter.LiteralTextSqlFormatted;
			AssertEquals("The filters are now in an OR category, so the optimisation should not occur. SAD! " + query, 1, query.AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenMultipleJobOrWorkflowFiltersExist()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			filterBizo.AddGuidFilterStrip(componentFilterDescription, ZGuid.BrettsGuid);
			filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);

			var workflowOnlyFilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			workflowOnlyFilter.SetWorkflowOnly();

			var sql = filterBizo.Filter.LiteralTextSqlFormatted;
			AssertEquals(sql, 0, sql.AllIndexesOf("UNION ALL").Count());
		}

		#endregion

		public void TestComponentFilter_Exact()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Dr Zaius";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Dr Whom";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Doctoren");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Get your paws off me you dirty ape!");
			SetComponent(workflow1, component1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Hey! The Tardis!");
			SetComponent(workflow2, component2.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Won't you help me!");
			SetComponent(workflow3, component1.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Call him Mr Raider, call him Mr Wrong!");
			SetComponent(workflow4, component2.PK);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.IsActive = true;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			componentFilter.Property = component1.PK;

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

		public void TestComponentFilter_NotEqual()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Dr Zaius";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Dr Whom";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Doctoren");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Get your paws off me you dirty ape!");
			SetComponent(workflow1, component1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Hey! The Tardis!");
			SetComponent(workflow2, component2.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Won't you help me!");
			SetComponent(workflow3, component1.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Call him Mr Raider, call him Mr Wrong!");
			SetComponent(workflow4, component2.PK);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.IsActive = true;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.NotEqual;
			componentFilter.Property = component1.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(3, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader.FH_CompletionStatement, workflow2.FH_CompletionStatement, workflow4.FH_CompletionStatement }, processHeaderResult);

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetWorkflowOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(2, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { workflow2.FH_CompletionStatement, workflow4.FH_CompletionStatement }, processHeaderResult);
		}

		public void TestComponentFilters_WhenInOrCategory_ShouldStillConsiderJobLevelWorkflows()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job in Bucket 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job in Bucket 2");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow in Bucket 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow in Bucket 2");

			SetComponent(workflow1, bucket1.PK);
			SetComponent(workflow2, bucket2.PK);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter1 = (T)filterBizo.FilterStrips.AddNew(componentFilterDescription).CurrentModuleFilter;
			var filter2 = (T)filterBizo.FilterStrips.AddNew(componentFilterDescription).CurrentModuleFilter;
			filter1.Property = bucket1.PK;
			filter2.Property = bucket2.PK;
			filter1.OrCategory = FilterOrCategory.BlanchedAlmond;
			filter2.OrCategory = FilterOrCategory.BlanchedAlmond;

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			var message = "All ProcessHeaders should have been returned from the query, and yet... " + query.LiteralTextSqlFormatted;
			AssertContainsExactElementsInAnyOrder(message, new[] { "Job in Bucket 1", "Job in Bucket 2", "Workflow in Bucket 1", "Workflow in Bucket 2" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestComponentFilter_IsBlank()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component 1";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component 2";

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW1");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");
			SetComponent(workflow1, component1.PK);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow 2");
			SetComponent(workflow2, component2.PK);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW3");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Workflow 3");
			SetComponent(workflow3, ZGuid.Empty);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.IsActive = true;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1.FH_CompletionStatement, jobHeader2.FH_CompletionStatement, jobHeader3.FH_CompletionStatement, workflow3.FH_CompletionStatement }, processHeaderResult);

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetWorkflowOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertContainsExactElementsInAnyOrder(new[] { workflow3.FH_CompletionStatement }, processHeaderResult);
		}

		public void TestComponentFilter_IsNotBlank()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component 1";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component 2";

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW1");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");
			SetComponent(workflow1, component1.PK);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow 2");
			SetComponent(workflow2, component2.PK);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "JLW3");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Workflow 3");
			SetComponent(workflow3, ZGuid.Empty);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.IsActive = true;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsNotBlank;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1.FH_CompletionStatement, workflow1.FH_CompletionStatement, jobHeader2.FH_CompletionStatement, workflow2.FH_CompletionStatement }, processHeaderResult);

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetWorkflowOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement }, processHeaderResult);
		}

		#region Filters Match

		public void TestFilter_FiltersMatchOption()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Dr Zaius";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Dr Whom";
			var component3 = system.Components.AddNew();
			component3.FC_Name = "Doctor Beat";
			var component4 = system.Components.AddNew();
			component4.FC_Name = "Mr Vain";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Doctoren");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Get your paws off me you dirty ape!");
			SetComponent(workflow1, component1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Hey! The Tardis!");
			SetComponent(workflow2, component2.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Won't you help me!");
			SetComponent(workflow3, component3.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Call him Mr Raider, call him Mr Wrong!");
			SetComponent(workflow4, component4.PK);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo.FilterStrips.AddNew(componentFilterDescription).CurrentModuleFilter;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			componentFilter.SelectedFilters.AddTextFilterStrip("Name", "Dr");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(3, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader.FH_CompletionStatement, workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement }, processHeaderResult);

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(1, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader.FH_CompletionStatement }, processHeaderResult);

			jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetWorkflowOnly();

			Factory.Save();

			newFactory = new BusinessObjectFactory();
			processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals(2, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement }, processHeaderResult);
		}

		public void TestFilter_FiltersMatchOption_NoSubFilter()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Dr Zaius";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Dr Whom";
			var component3 = system.Components.AddNew();
			component3.FC_Name = "Doctor Beat";
			var component4 = system.Components.AddNew();
			component4.FC_Name = "Mr Vain";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Doctoren");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Get your paws off me you dirty ape!");
			SetComponent(workflow1, component1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Hey! The Tardis!");
			SetComponent(workflow2, component2.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Won't you help me!");
			SetComponent(workflow3, component3.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Call him Mr Raider, call him Mr Wrong!");
			SetComponent(workflow4, component4.PK);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var componentFilter = (T)filterBizo[componentFilterDescription];
			componentFilter.IsActive = true;
			componentFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.FiltersMatch;
			componentFilter.Property = ZGuid.BrettsGuid;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var processHeaderResult = newFactory.Load<ProcessHeader>(filterBizo.Filter).Select(x => x.FH_CompletionStatement);
			AssertEquals("All elements should be returned", 5, processHeaderResult.Count());
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader.FH_CompletionStatement, workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement, workflow3.FH_CompletionStatement, workflow4.FH_CompletionStatement }, processHeaderResult);
		}

		#endregion
	}
}
