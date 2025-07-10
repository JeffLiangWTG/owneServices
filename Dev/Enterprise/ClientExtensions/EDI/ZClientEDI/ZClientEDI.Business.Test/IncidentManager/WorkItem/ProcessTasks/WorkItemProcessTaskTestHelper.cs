using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public static class WorkItemProcessTaskTestHelper
	{
		public static void AddRequiredAspects(ProcessTask task, params Guid[] aspectPKs)
		{
			foreach (var aspectPK in aspectPKs)
			{
				task.SkillsPivots.AddAspect(aspectPK);
			}
		}

		public static void SetupReviewTasksRegistry(params string[] reviewTaskTypes)
		{
			var reviewTasksList = new CodeDescriptionBoolCollection();
			foreach (var type in reviewTaskTypes)
			{
				reviewTasksList.Add(type);
			}
			EDIDataRegistry.Instance.ReviewTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reviewTasksList);
		}

		public static void SetupLearningTaskRegistry(string learningTaskTypeCode)
		{
			var collection = new CategorisedWorkflowTaskTypesCollection();
			var module = collection.AddNew();
			module.Code = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;

			var learningTaskType = module.TaskTypes.AddNew();
			learningTaskType.Code = learningTaskTypeCode;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			EDIDataRegistry.Instance.CompetencyLearningTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, learningTaskTypeCode);
		}
	}
}
