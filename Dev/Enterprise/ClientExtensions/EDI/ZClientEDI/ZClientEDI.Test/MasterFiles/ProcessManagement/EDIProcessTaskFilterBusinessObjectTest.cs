using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.ProcessManagement.Testing
{
	[TestedType(typeof(EDIProcessTaskFilterBusinessObject))]
	public class EDIProcessTaskFilterBusinessObjectTest : ProcessTaskFilterBusinessObjectTest
	{
		public void TestWorkItemTypeFilter()
		{
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem2.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			WorkItemProcessTask task1 = workItem1.WorkflowItems.AddNew();
			WorkItemProcessTask task2 = workItem2.WorkflowItems.AddNew();
			Factory.Save();
			EDIProcessTaskFilterBusinessObject filter = new EDIProcessTaskFilterBusinessObject();
			ModuleTextFilter workItemTypeFilter = (ModuleTextFilter)filter["Work Item Type"];
			ProcessTaskCollection taskCollection = new ProcessTaskCollection(Factory);
			workItemTypeFilter.IsActive = true;
			workItemTypeFilter.Property = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			taskCollection.Load(filter.Filter);
			AssertEquals(1, taskCollection.Count);
			AssertCollectionContains("should contain task from defect work item", task1, taskCollection);
			workItemTypeFilter.Property = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			taskCollection.Load(filter.Filter);
			AssertEquals(1, taskCollection.Count);
			AssertCollectionContains("should contain task form issue fix work item", task2, taskCollection);
		}

		public void TestTaskTypeFilter2()
		{
			CodeDescriptionPair code1 = new CodeDescriptionPair("XXX", "lololol");
			CodeDescriptionPair code2 = new CodeDescriptionPair("YYY", "sighwork");
			CategorisedWorkflowTaskTypesCollection taskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes workflowType1 = taskTypesCollection.AddNew();
			workflowType1.Code = "PRJ";
			WorkflowTaskType taskType1 = workflowType1.TaskTypes.AddNew();
			taskType1.Code = code1.Code;
			taskType1.Description = (NoResString)code1.Description;
			CategorisedWorkflowTaskTypes workflowType2 = taskTypesCollection.AddNew();
			workflowType2.Code = JobInvoicingConsumerTypes.WorkItem.Code;
			WorkflowTaskType taskType2 = workflowType2.TaskTypes.AddNew();
			taskType2.Code = code2.Code;
			taskType2.Description = (NoResString)code2.Description;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taskTypesCollection);
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask processTask1 = workItem1.WorkflowItems.AddNew();
			processTask1.P9_Type = code2.Code;
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask processTask2 = workItem2.WorkflowItems.AddNew();
			processTask2.P9_Type = "INV";
			Factory.Save();
			EDIProcessTaskFilterBusinessObject filter = new EDIProcessTaskFilterBusinessObject();
			ModuleTextFilter taskTypeFilter = (ModuleTextFilter)filter["Task Type"];
			ModuleTextFilter workflowTypeFilter = (ModuleTextFilter)filter["Workflow Type"];
			ProcessTaskCollection taskCollection = new ProcessTaskCollection(Factory);
			taskTypeFilter.IsActive = true;
			workflowTypeFilter.IsActive = false;
			AssertEquals("should contain task types for all workflow types", 2, taskTypeFilter.List.Count);
			taskTypeFilter.Property = code2.Code;
			taskCollection.Load(filter.Filter);
			AssertEquals(1, taskCollection.Count);
			AssertCollectionContains("should contain task of type \"YYY\"", processTask1, taskCollection);
			workflowTypeFilter.IsActive = true;
			workflowTypeFilter.Property = "PRJ";
			taskCollection.Load(filter.Filter);
			AssertEquals(0, taskCollection.Count);
			AssertEquals("should only contain task types for PRJ workflow type", 1, taskTypeFilter.List.Count);
		}

		public void TestNotReviewTaskTypeFilter()
		{
			var reviewTasks = new CodeDescriptionBoolCollection();
			reviewTasks.Add("RVW", (NoResString)"review");
			reviewTasks.Add("FRV", (NoResString)"functional review");
			EDIDataRegistry.Instance.ReviewTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reviewTasks);
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask processTask1 = workItem1.WorkflowItems.AddNew();
			processTask1.P9_Type = "INV";
			NewWorkItem workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask processTask2 = workItem2.WorkflowItems.AddNew();
			processTask2.P9_Type = "RVW";
			NewWorkItem workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask processTask3 = workItem2.WorkflowItems.AddNew();
			processTask3.P9_Type = "FRV";
			Factory.Save();
			EDIProcessTaskFilterBusinessObject filter = new EDIProcessTaskFilterBusinessObject();
			ModuleTextFilter taskTypeFilter = (ModuleTextFilter)filter["Task Type"];
			ProcessTaskCollection taskCollection = new ProcessTaskCollection(Factory);
			taskTypeFilter.IsActive = true;
			taskTypeFilter.Property = "NOR";
			taskCollection.Load(filter.Filter);
			AssertEquals(1, taskCollection.Count);
			AssertCollectionContains("should contain task of type \"INV\"", processTask1, taskCollection);
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDIProcessTaskFilterBusinessObject();
		}
		#endregion
	}
}
