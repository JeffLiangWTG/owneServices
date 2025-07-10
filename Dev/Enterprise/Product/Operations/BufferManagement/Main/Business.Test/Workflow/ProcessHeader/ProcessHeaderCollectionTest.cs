using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderCollection))]
	class ProcessHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessHeaderCollection>
	{
		#region Misc

		public void TestFirstOpenWorkFlow()
		{
			var workitem = Factory.New<IWorkItem>() as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workitem, Factory);
			var headers = jobHeader.ProcessHeaders;
			headers.DeleteAll();
			AssertEquals("PreCondition", 0, headers.Count);

			var system = Factory.New<IBMSystem>();
			system.FS_Name = "Hey";
			var sequence = 1;
			for (var i = 1; i < 11; i++)
			{
				for (var j = 0; j < 11; j++)
				{
					if (j == 0 || i == 5)
					{
						var component = Factory.New<IBMComponent>();
						component.FC_FS_System = system.PK;
						component.FC_Name = j == 0 ? "Component Test " + i : "Component Test " + i + "." + j;

						var processHeader = jobHeader.ProcessHeaders.AddNew();
						processHeader.FH_FC_CurrentComponent = component.PK;
						processHeader.FH_CompletionStatement = j == 0 ? "Completion Statement " + i : "Completion Statement " + i + "." + j;
						if (j != 0)
						{
							processHeader.GetOrCreateLinkToParent(jobHeader.ProcessHeaders[4]); //Only "Completion Statement5" has child workflow
						}

						var task = workitem.WorkflowItems.Tasks.AddNew();
						task.P9_FH_ProcessHeader = processHeader.PK;
						task.P9_Status = (i == 5 && (j == 5 || j == 10)) || i == 10 ? ProcessTaskStatusCodeList.Codes.Open : ProcessTaskStatusCodeList.Codes.Closed;
						AssertEquals(sequence++, task.P9_Sequence);
						Factory.Save();
					}
				}
			}

			CombineAssertions("FirstOpenWorkFlow", () =>
			{
				AssertEquals(20, headers.Count);
				AssertEquals("Order by Sequence directly", "1,10,2,3,4,5,5.1,5.10,5.2,5.3,5.4,5.5,5.6,5.7,5.8,5.9,6,7,8,9", string.Join(",", headers.Select(x => x.Sequence).OrderBy(x => x).ToArray()));
				AssertEquals(4, headers.Count(x => x.IsOpen));
				AssertEquals("Open workflows", "Completion Statement 10,Completion Statement 5,Completion Statement 5.10,Completion Statement 5.5", string.Join(",", headers.Where(x => x.IsOpen).Select(x => x.FH_CompletionStatement).OrderBy(x => x).ToArray()));
				AssertEquals("Completion Statement 5", headers.FirstOpenWorkFlow.FH_CompletionStatement);
			});
		}

		public void TestExcludesChildWorkflowsNotInSameJob()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var workflowNotInSameJob = Factory.NewWithValidTestData<ProcessHeader>();
			workflowNotInSameJob.FH_FH_ParentHeader = jobHeader.PK;

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertCollectionContains(workflow1, jobHeader.ProcessHeaders);
			AssertCollectionContains(workflow2, jobHeader.ProcessHeaders);
		}

		public void TestNoResultsQueryCache()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.ProcessHeaders.AddNew();
			jobHeader.ProcessHeaders.AddNew();

			AssertEquals("No hits to DB before Save", 0, Factory.GetTableHitCount(ProcessHeader.Schema.TableName));

			Factory.Save();

			Factory.ReloadAll<ProcessHeader>();
			var processHeaders = jobHeader.ProcessHeaders;
			AssertEquals(3, processHeaders.Count);
			AssertEquals("One hit initial load", 1, Factory.GetTableHitCount(ProcessHeader.Schema.TableName));

			processHeaders.AdditionalFilter = ZQuery.NoResultQuery;
			var newFactory = new BusinessObjectFactory();
			((IActiveBusinessObjectCollection)processHeaders).SetFactory(newFactory);
			AssertEquals("Our AdditionalFilter should now stop the collection from loading anything", 0, processHeaders.Count);
			AssertEquals("No db hits when NoResultQuery", 0, newFactory.GetTableHitCount(ProcessHeader.Schema.TableName));

			processHeaders.AdditionalFilter = new ZQuery();
			AssertEquals("Now we should be able to load the headers", 3, processHeaders.Count);
			AssertEquals("One new db hit after second load", 1, newFactory.GetTableHitCount(ProcessHeader.Schema.TableName));
		}

		public void TestTemplateHeadersExcluded()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader1 = template1.GetJobHeader();
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_1 = jobHeader.ProcessHeaders.AddNew();
			var workflow1_2 = jobHeader.ProcessHeaders.AddNew();

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader2 = template2.GetJobHeader();
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2);

			var collection = new ProcessHeaderCollection(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1_1, workflow1_2, workflow2_1, workflow2_2, jobHeader, jobHeader2 }, collection);

			collection = new ProcessHeaderCollection(template1);
			AssertContainsExactElementsInAnyOrder(new[] { templateWorkflow1, templateJobHeader1 }, collection);

			collection = jobHeader.ProcessHeaders;
			AssertContainsExactElementsInAnyOrder(new[] { workflow1_1, workflow1_2 }, collection);
		}

		public void TestIgnoresCompletionStatements()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Ben Kenobi");
			var task1 = BMSTestHelper.CreateTask(workflow, "BEN", 7);

			var taskCollection = workflow.TaskCollection;

			AssertEquals(1, taskCollection.Count);

			var task2 = BMSTestHelper.CreateTask(workflow, "BEN", 7);

			AssertEquals(2, taskCollection.Count);

			task2.P9_Type = taskType.Code;

			Assert(task2.IsCompletionStatement);
			AssertEquals(1, taskCollection.Count);
		}

		#endregion

		#region Add

		public void TestAddNew_ShouldSetParentDetails()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var workflow = jobHeader.ProcessHeaders[0];
			AssertEquals(job.PK, workflow.FH_ParentId);
			AssertEquals(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(OrgHeaderSchema.Constants.TableName), workflow.FH_ParentTableCode);

			workflow = jobHeader.ProcessHeaders.AddNew();
			AssertEquals(job.PK, workflow.FH_ParentId);
			AssertEquals(ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(OrgHeaderSchema.Constants.TableName), workflow.FH_ParentTableCode);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = template.ProcessHeaders.AddNew();

			AssertEquals(ZString.Empty, templateWorkflow.FH_ParentTableCode);
			AssertEquals(ZGuid.Empty, templateWorkflow.FH_ParentId);
			AssertEquals(template.PK, templateWorkflow.FH_P0_Template);
		}

		public void TestAddNew_ShouldCascadeApprovedFlag()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.FH_IsApproved = true;

			var workflow = jobHeader.ProcessHeaders.AddNew();
			AssertEquals(true, workflow.FH_IsApproved);
		}

		public void TestAddNew_OnWorkflowTemplate_ShouldCascadeApprovedFlag()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			template.GetJobHeader().FH_IsApproved = true;

			var workflow = BMSTestHelper.CreateWorkflow(template, "One Workflow");
			AssertEquals(true, workflow.FH_IsApproved);
		}

		public void TestDeleteWorkflowTemplate_ShouldRemoveReferenceFromRelatedTasks()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var task = BMSTestHelper.CreateTask(template);
			AssertEquals(task.P9_FH_ProcessHeader, ZGuid.Empty);

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "One Workflow");
			AssertEquals(task.P9_FH_ProcessHeader, templateWorkflow.PK);

			((ICancelAddNew)template.ProcessHeaders).CancelNew(0);
			template.Workflows.Delete(templateWorkflow);

			using (((IBusinessObjectInternals)task).SuppressReportRowDeletedError())
			{
				AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);
			}
		}

		public void TestNonDeleteWorkflowTemplate_ShouldNotRemoveReferenceFromRelatedTasks()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var task = BMSTestHelper.CreateTask(template);
			AssertEquals(task.P9_FH_ProcessHeader, ZGuid.Empty);

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "One Workflow");
			AssertEquals(task.P9_FH_ProcessHeader, templateWorkflow.PK);

			((ICancelAddNew)template.ProcessHeaders).CancelNew(0);

			using (((IBusinessObjectInternals)task).SuppressReportRowDeletedError())
			{
				AssertEquals(task.P9_FH_ProcessHeader, templateWorkflow.PK);
			}
		}

		#region Release Group Determination For Added Workflows

		public void TestAddNew_WhenNotApplyingTemplate_ShouldTryToGetReleaseGroupFromOtherWorkflow()
		{
			var releaseGroup = BMSTestHelper.CreateGroup(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", releaseGroupPK: releaseGroup.PK);

			var collection = jobHeader.ProcessHeaders;

			var addedWorkflow = collection.AddNew();
			AssertEquals("Adding a workflow (when not applying a template) should attempt to set its release group based on other workflows already in the collection. SAD!", releaseGroup.PK, addedWorkflow.FH_GG_ReleaseGroup);

			BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			addedWorkflow = collection.AddNew();
			AssertEquals("Even though the registry has changed, we haven't told the collection that we're applying a template, so it should still attempt to set its release group based on other workflows already in the collection. SAD!",
				releaseGroup.PK, addedWorkflow.FH_GG_ReleaseGroup);
		}

		public void TestAddNew_WhenApplyingTemplate_AndRegistryItemEnabled_ShouldTryToGetReleaseGroupFromOtherWorkflow()
		{
			AssertEquals("The registry item should be enabled by default, so that customers don't see any change without changing it. SAD!", true, BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.Value);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", releaseGroupPK: releaseGroup.PK);

			var collection = jobHeader.ProcessHeaders;

			using (jobHeader.SetWorkflowTemplateApplicationMode())
			{
				var addedWorkflow = collection.AddNew();
				AssertEquals("Adding a workflow via template should attempt to set its release group based on other workflows already in the collection, because the registry told it to. SAD!", releaseGroup.PK, addedWorkflow.FH_GG_ReleaseGroup);
			}
		}

		public void TestAddNew_WhenApplyingTemplate_AndRegistryItemDisabled_ShouldNotTryToGetReleaseGroupFromOtherWorkflow()
		{
			BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", releaseGroupPK: releaseGroup.PK);

			var collection = jobHeader.ProcessHeaders;

			using (jobHeader.SetWorkflowTemplateApplicationMode())
			{
				var addedWorkflow = collection.AddNew();
				AssertEquals("Adding a workflow via template should NOT attempt to set its release group based on other workflows already in the collection, because the registry told it NOT to. SAD!", ZGuid.Empty, addedWorkflow.FH_GG_ReleaseGroup);
			}
		}

		public void TestAddNew_WhenReapplyingTemplate_AndRegistryItemDisabled_ShouldNotTryToGetReleaseGroupFromOtherWorkflow()
		{
			BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var genericRG = BMSTestHelper.CreateGroup(Factory, "GEN", "Generic RG");
			var specificRG = BMSTestHelper.CreateGroup(Factory, "SPE", "Specific RG");

			var genericTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var genericTemplateWorkflow = BMSTestHelper.CreateWorkflow(genericTemplate, "Generic Workflow", genericRG.PK);
			BMSTestHelper.CreateTaskForTemplate(genericTemplate, genericTemplateWorkflow);

			var specificTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", subType1: "AAA");
			var specificTemplateWorkflow = BMSTestHelper.CreateWorkflow(specificTemplate, "Specific Workflow");
			var specificTask = BMSTestHelper.CreateTaskForTemplate(specificTemplate, specificTemplateWorkflow);
			specificTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition; // specifying user-defined template condition will ensure this task will be added to the job after changing job's selection criteria even if there are other tasks created from another template
			specificTask.TemplateConditions.TemplateCondition2Value = "true";
			var specificRGRule = BMSTestHelper.CreateTemplateReleaseGroupRule(specificTemplate, "1");
			AssertEquals("Precondition", true, specificRGRule.PTR_AreAllWorkflowCategoriesApplicable);
			BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(specificRGRule, "1", specificRG);

			Factory.Save();

			var specificJob = Factory.New<IWorkItem>();
			specificJob.WKI_Summary = "Specific WI";
			specificJob.WKI_WorkItemType = "AAA";

			var genericJob = Factory.New<IWorkItem>();
			genericJob.WKI_Summary = "Generic WI";

			Factory.Save();

			var specificJobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)specificJob, Factory);
			AssertEquals("Precondition: should be just one workflow", 1, specificJobHeader.ProcessHeaders.Count);
			var specificWorkflow = specificJobHeader.ProcessHeaders.SingleOrDefault(h => h.FH_CompletionStatement == "Specific Workflow");
			AssertNotNull("Precondition: Specific", specificWorkflow);
			AssertEquals("Precondition: assigned to Specific RG", specificRG.PK, specificWorkflow.FH_GG_ReleaseGroup);

			var genericJobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)genericJob, Factory);
			AssertEquals("Precondition: should be just one workflow", 1, genericJobHeader.ProcessHeaders.Count);
			var genericWorkflow = genericJobHeader.ProcessHeaders.SingleOrDefault(h => h.FH_CompletionStatement == "Generic Workflow");
			AssertNotNull("Precondition: Generic", genericWorkflow);
			AssertEquals("Precondition: assigned to Generic RG", genericRG.PK, genericWorkflow.FH_GG_ReleaseGroup);

			genericJob.WKI_WorkItemType = "AAA";

			Factory.Save();

			AssertEquals("Should add a new workflow", 2, genericJobHeader.ProcessHeaders.Count);
			var newWorkflow = genericJobHeader.ProcessHeaders.SingleOrDefault(h => h.FH_CompletionStatement == "Specific Workflow");
			AssertNotNull("It should be taken from the specific template", newWorkflow);
			AssertEquals("Should be assigned to Specific RG", specificRG.PK, newWorkflow.FH_GG_ReleaseGroup);
		}

		#endregion

		#endregion

		#region Sort

		[TestDate(2020, 3, 1)]
		public void TestSortByActualHours_ShouldOrderNumerically()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3ChildWorkflow2");
			workflow3.GetOrCreateLinkToParent(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow3);

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4ChildWorkflow2");
			workflow4.GetOrCreateLinkToParent(workflow2);
			var task4 = BMSTestHelper.CreateTask(workflow4);

			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");
			var task5 = BMSTestHelper.CreateTask(workflow5);

			AssertEquals(5, jobHeader.ProcessHeaders.Count);

			task1.P9_ActualDuration = new ZDateTime(2020, 1, 1, 0, 0, 0);   //000:00
			task2.P9_ActualDuration = new ZDateTime(2020, 1, 1, 7, 1, 0);   //007:10
			task3.P9_ActualDuration = new ZDateTime(2020, 1, 1, 11, 16, 0); //011:16
			task4.P9_ActualDuration = new ZDateTime(2020, 1, 1, 0, 10, 0);  //000:10
			task5.P9_ActualDuration = new ZDateTime(2020, 1, 1, 0, 56, 0);  //000:56

			AssertEquals("Total Hours of Workflow 2 should be the sun of workflow2, 3 and 4",
				(ZDateTime)new TimeSpan(18, 27, 0), workflow2.TotalActualHoursIncludingChildrenDateTime);

			var propertyName = nameof(ProcessHeader.TotalActualHoursIncludingChildrenDateTime);

			jobHeader.ProcessHeaders.ApplySort(propertyName, ListSortDirection.Ascending);

			var expectyOrder = new[] { workflow1, workflow4, workflow5, workflow3, workflow2 };
			var result = jobHeader.ProcessHeaders.ToArray();

			AssertArrayEqualsByElements(expectyOrder, result);

			jobHeader.ProcessHeaders.ApplySort(propertyName, ListSortDirection.Descending);

			expectyOrder = expectyOrder.Reverse().ToArray();
			result = jobHeader.ProcessHeaders.ToArray();

			AssertArrayEqualsByElements(expectyOrder, result);
		}

		public void TestSortBySequence_ShouldOrderNumerically()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			ProcessHeader previousWorkflow = null;

			const int numberOfWorkflowGroups = 11;

			for (int i = 0; i < numberOfWorkflowGroups; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow" + i);
				var childWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow" + i + ".1");
				var childWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow" + i + ".2");
				var grandchildWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandchildWorkflow" + i + ".1.1");

				childWorkflow1.GetOrCreateLinkToParent(workflow);
				childWorkflow2.GetOrCreateLinkToParent(workflow);
				grandchildWorkflow.GetOrCreateLinkToParent(childWorkflow1);

				childWorkflow1.GetOrCreateDependencyLink(childWorkflow2);

				if (previousWorkflow != null)
				{
					previousWorkflow.GetOrCreateDependencyLink(workflow);
				}
				previousWorkflow = workflow;
			}

			AssertEquals(numberOfWorkflowGroups * 4, jobHeader.ProcessHeaders.Count);

			var property = typeof(ProcessHeader).GetProperty("Sequence");
			jobHeader.ProcessHeaders.ApplySort(property.Name, ListSortDirection.Ascending);

			CombineAssertions("Ascending sort", () =>
			{
				for (int i = 0; i < numberOfWorkflowGroups; i++)
				{
					var startIndex = i * 4;

					AssertEquals((i + 1).ToString(), jobHeader.ProcessHeaders[startIndex].Sequence);
					AssertEquals((i + 1).ToString() + ".1", jobHeader.ProcessHeaders[startIndex + 1].Sequence);
					AssertEquals((i + 1).ToString() + ".1.1", jobHeader.ProcessHeaders[startIndex + 2].Sequence);
					AssertEquals((i + 1).ToString() + ".2", jobHeader.ProcessHeaders[startIndex + 3].Sequence);
				}
			});

			jobHeader.ProcessHeaders.ApplySort(property.Name, ListSortDirection.Descending);
			CombineAssertions("Descending sort", () =>
			{
				for (int i = 0; i < numberOfWorkflowGroups; i++)
				{
					var startIndex = i * 4;
					var expectedTopLevelIndex = numberOfWorkflowGroups - i;

					AssertEquals(expectedTopLevelIndex.ToString() + ".2", jobHeader.ProcessHeaders[startIndex].Sequence);
					AssertEquals(expectedTopLevelIndex.ToString() + ".1.1", jobHeader.ProcessHeaders[startIndex + 1].Sequence);
					AssertEquals(expectedTopLevelIndex.ToString() + ".1", jobHeader.ProcessHeaders[startIndex + 2].Sequence);
					AssertEquals(expectedTopLevelIndex.ToString(), jobHeader.ProcessHeaders[startIndex + 3].Sequence);
				}
			});
		}

		public void TestClone_ShouldCreateCorrectlyTypedProcessHeaders()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var jobHeader = template.GetJobHeader();
			jobHeader.FH_CompletionStatement = "Snowbesity";

			var workflow = template.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "When you're unsure if someone is fat because they're wearing a winter coat";

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;

			var clonedTemplate = (ProcessTaskTemplate)template.Clone();
			clonedTemplate.P0_Name += " Clone";
			var clonedWorkflow = clonedTemplate.GetWorkflows().Single(x => x.FH_CompletionStatement == "When you're unsure if someone is fat because they're wearing a winter coat");
			AssertEquals("The cloned workflow should have been made a ProcessHeader by the type decider instead of ProcessJobHeader, and yet...", typeof(ProcessHeader), clonedWorkflow.GetType());

			clonedTemplate.Factory.Save();

			clonedWorkflow.FH_CompletionStatement += "!";
			clonedTemplate.Factory.Save();

			AssertEquals("When you're unsure if someone is fat because they're wearing a winter coat!", clonedWorkflow.FH_CompletionStatement);
			AssertEquals("When you're unsure if someone is fat because they're wearing a winter coat", workflow.FH_CompletionStatement);
		}

		public void TestSortBySequence_UnsequencedWorkflowsAreAlwaysLast_Ascending()
		{
			AssertWorkflowCollectionIsSorted(ListSortDirection.Ascending);
		}

		public void TestSortBySequence_UnsequencedWorkflowsAreAlwaysLast_Descending()
		{
			AssertWorkflowCollectionIsSorted(ListSortDirection.Descending);
		}

		void AssertWorkflowCollectionIsSorted(ListSortDirection sortInOrder)
		{
			var workflowCollection = new ProcessHeaderCollection(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			workflow1.GetOrCreateDependencyLink(workflow2);
			var workflow3 = Factory.New<ProcessHeader>();
			workflow3.FillWithValidTestData();
			workflow3.FH_CompletionStatement = "workflow3";

			AssertEquals("We should have succesfully made a workflow collection with 4 workflows, but instead...", 4, workflowCollection.Count);
			AssertEquals("We should have succesfully made a jobHeader with 2 workflows, but instead...", 2, jobHeader.ProcessHeaders.Count);
			AssertEquals("JobHeader should have a sequence of EMPTINESS", string.Empty, jobHeader.Sequence);
			AssertEquals("Workflow1 should have a sequence of 1", "1", workflow1.Sequence);
			AssertEquals("Workflow2 should have a sequence of 2", "2", workflow2.Sequence);
			AssertEquals("Workflow3 should have a sequence of EMPTINESS", string.Empty, workflow3.Sequence);

			var property = typeof(ProcessHeader).GetProperty("Sequence");
			workflowCollection.ApplySort(property.Name, sortInOrder);

			if (sortInOrder == ListSortDirection.Ascending)
			{
				CombineAssertions("Ascending sort", () =>
				{
					AssertEquals("Sequence of jobHeader", string.Empty, workflowCollection[0].Sequence);
					AssertEquals("Sequence of workflow1", "1", workflowCollection[1].Sequence);
					AssertEquals("Sequence of workflow2", "2", workflowCollection[2].Sequence);
					AssertEquals("Sequence of emptyWorkflow", string.Empty, workflowCollection[3].Sequence);

					AssertEquals("PK of jobHeader", jobHeader.PK, workflowCollection[0].PK);
					AssertEquals("PK of workflow1", workflow1.PK, workflowCollection[1].PK);
					AssertEquals("PK of workflow2", workflow2.PK, workflowCollection[2].PK);
					AssertEquals("PK of emptyWorkflow", workflow3.PK, workflowCollection[3].PK);
				});
			}
			else
			{
				CombineAssertions("Descending sort", () =>
				{
					AssertEquals("Sequence of workflow2", "2", workflowCollection[0].Sequence);
					AssertEquals("Sequence of workflow1", "1", workflowCollection[1].Sequence);
					AssertEquals("Sequence of jobHeader", string.Empty, workflowCollection[2].Sequence);
					AssertEquals("Sequence of emptyWorkflow", string.Empty, workflowCollection[3].Sequence);

					AssertEquals("PK of workflow2", workflow2.PK, workflowCollection[0].PK);
					AssertEquals("PK of workflow1", workflow1.PK, workflowCollection[1].PK);
					AssertEquals("PK of jobHeader", jobHeader.PK, workflowCollection[2].PK);
					AssertEquals("PK of emptyWorkflow", workflow3.PK, workflowCollection[3].PK);
				});
			}
		}

		#endregion

		#region Clone

		public void TestCloneWorkflowsAndLinks_ClonesTagLinks()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var tagConfig = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var workflow = BMSTestHelper.CreateWorkflow(template, "workflow");
			var ruleTagLink = BMSTestHelper.CreateTagLink(workflow, tagConfig.RuleTag);
			var tagLink = BMSTestHelper.CreateTagLink(workflow, tagConfig.RedTag);
			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			template.ProcessHeaders.CloneWorkflowsAndLinksForTemplates(targetTemplate.GetJobHeader(), targetTemplate.ProcessHeaders);
			var clonedWorkflow = targetTemplate.GetWorkflows().Single();

			AssertContainsExactElementsInAnyOrder(new[] { tagConfig.RuleTag, tagConfig.RedTag }, clonedWorkflow.TagLinks.Select(link => link.TagMagnitude));
			AssertCollectionNotContains(new[] { ruleTagLink.PK, tagLink.PK }, clonedWorkflow.TagLinks.Select(link => link.PK));
		}

		public void TestCloneWorkflowsAndLinks_ShouldCloneLinkFromExternalToInternal()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var externalTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var workflow = BMSTestHelper.CreateWorkflow(template, "Internal Workflow");
			var externalWorkflow = BMSTestHelper.CreateWorkflow(externalTemplate, "External Workflow");

			var link = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();
			link.FP_FH_HeaderFrom = externalWorkflow.PK;
			link.FP_FH_HeaderTo = workflow.PK;

			template.ProcessHeaders.CloneWorkflowsAndLinksForTemplates(targetTemplate.GetJobHeader(), targetTemplate.ProcessHeaders);

			AssertEquals("targetTemplate should have exactly 2 process headers after cloning (ProcessJobHeader & cloned Internal Workflow)", 2, targetTemplate.ProcessHeaders.Count);

			var clonedLink = targetTemplate.ProcessHeaderLinks[0];
			var clonedWorkflow = targetTemplate.GetWorkflows().Single();

			AssertEquals("HeaderFrom is external, thus should reference the same workflow between original and cloned template links", externalWorkflow, clonedLink.HeaderFrom);
			AssertNotEquals("HeaderTo is internal,  thus should reference different workflows between original and cloned template links", workflow, clonedLink.HeaderTo);
			AssertEquals(clonedWorkflow, clonedLink.HeaderTo);
		}

		public void TestCloneWorkflowsAndLinks_ShouldCloneLinkFromInternalToExternal()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var externalTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var workflow = BMSTestHelper.CreateWorkflow(template, "Internal Workflow");
			var externalWorkflow = BMSTestHelper.CreateWorkflow(externalTemplate, "External Workflow");

			var link = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();
			link.FP_FH_HeaderFrom = workflow.PK;
			link.FP_FH_HeaderTo = externalWorkflow.PK;

			template.ProcessHeaders.CloneWorkflowsAndLinksForTemplates(targetTemplate.GetJobHeader(), targetTemplate.ProcessHeaders);

			AssertEquals("targetTemplate should have exactly 2 process headers after cloning (ProcessJobHeader & cloned Internal Workflow)", 2, targetTemplate.ProcessHeaders.Count);

			var clonedLink = targetTemplate.ProcessHeaderLinks[0];
			var clonedWorkflow = targetTemplate.GetWorkflows().Single();

			AssertNotEquals("HeaderFrom is internal,  thus should reference different workflows between original and cloned template links", workflow, clonedLink.HeaderFrom);
			AssertEquals("HeaderTo is external, thus should reference the same workflow between original and cloned template links", externalWorkflow, clonedLink.HeaderTo);
			AssertEquals(clonedWorkflow, clonedLink.HeaderFrom);
		}

		public void TestClone_ShouldCloneCompletionMilestone()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "workflow2");
			var milestone1 = template.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "milestone1";
			var milestone2 = template.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "milestone2";

			MilestoneCompletionHelper.SetCompletionMilestone(workflow1, milestone1);
			MilestoneCompletionHelper.SetCompletionMilestone(workflow2, milestone2);

			var clonedTemplate = template.Clone() as ProcessTaskTemplate;
			var clonedWorkflows = clonedTemplate.GetWorkflows().ToArray();
			var clonedMilestones = clonedTemplate.WorkflowItems.Milestones.Cast<ProcessTask>().ToArray();

			AssertEquals(2, clonedWorkflows.Length);
			AssertEquals(2, clonedMilestones.Length);

			var clonedWorkflow1 = clonedWorkflows.FirstOrDefault(w => w.FH_CompletionStatement == workflow1.FH_CompletionStatement);
			var clonedWorkflow2 = clonedWorkflows.FirstOrDefault(w => w.FH_CompletionStatement == workflow2.FH_CompletionStatement);
			AssertNotNull(clonedWorkflow1);
			AssertNotNull(clonedWorkflow2);

			var clonedMilestone1 = clonedMilestones.FirstOrDefault(w => w.P9_Description == milestone1.P9_Description);
			var clonedMilestone2 = clonedMilestones.FirstOrDefault(w => w.P9_Description == milestone2.P9_Description);
			AssertNotNull(clonedMilestone1);
			AssertNotNull(clonedMilestone2);

			AssertEquals(clonedMilestone1.P9_MilestoneCompletionPivotKey, clonedWorkflow1.FH_MilestoneCompletionPivotKey);
			AssertEquals(clonedMilestone2.P9_MilestoneCompletionPivotKey, clonedWorkflow2.FH_MilestoneCompletionPivotKey);
		}

		#endregion

		#region Implementation

		protected override ProcessHeaderCollection GetCollectionToTest()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			return jobHeader.ProcessHeaders;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
		}

		#endregion
	}
}
