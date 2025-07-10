using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class CheckinTaskForPrecedingTasksValidationTestContext : CheckinTaskReviewValidationTestContext
	{
		public CheckinTaskForPrecedingTasksValidationTestContext(BusinessObjectFactory factory)
			: base(factory)
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "CDF", "CDU");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBC", "WKI");

			WorkItem.WKI_WorkItemType = WorkItemProcessTaskValidation.EnterpriseCode;
			WorkItem.WKI_WorkItemArea = WorkItemProcessTaskValidation.ModernizationCode;
		}

		public ProcessTask CreateCodingTask(IProcessHeader workflow, string taskType, int sequence)
			=> (ProcessTask)BMTestHelper.CreateTask(workflow, staffCode: AuthorStaffCode, taskType: taskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding", sequence: sequence);

		public ProcessTask CreateReviewTask(IProcessHeader workflow, int sequence)
			=> (ProcessTask)BMTestHelper.CreateTask(workflow, staffCode: ReviewerStaffCode, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "Code Review", sequence: sequence);
	}
}
