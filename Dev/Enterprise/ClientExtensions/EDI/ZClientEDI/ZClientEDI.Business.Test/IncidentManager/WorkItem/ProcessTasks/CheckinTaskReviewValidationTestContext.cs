using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class CheckinTaskReviewValidationTestContext
	{
		public CheckinTaskReviewValidationTestContext(BusinessObjectFactory factory)
		{
			MasterFilesTestHelper.CreateStaff(factory, AuthorStaffCode, "Davey");
			MasterFilesTestHelper.CreateStaff(factory, ReviewerStaffCode, "Alfie");
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "COD", "CBC", "CHK", "CH0");

			BMTestHelper = ObjectFactory.Get<IBMTestHelper>();
			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			JobHeader = BMTestHelper.CreateJobHeader<NewWorkItem>(factory, addDefaultProcessHeaderIfNone: false);
			WorkItem = (NewWorkItem)JobHeader.Parent;
		}

		public NewWorkItem WorkItem { get; }

		public IBMTestHelper BMTestHelper { get; }

		public IProcessJobHeader JobHeader { get; }

		public IProcessHeader CreateWorkflow(string description)
			=> BMTestHelper.CreateWorkflow(JobHeader, description);

		public ProcessTask CreateCodingTask(IProcessHeader workflow)
			=> (ProcessTask)BMTestHelper.CreateTask(workflow, staffCode: AuthorStaffCode, taskType: "COD", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "Coding", sequence: 1);

		public ProcessTask CreateReviewTask(IProcessHeader workflow, int sequence = 2, string staffCode = null, string status = ProcessTaskStatusCodeList.Codes.Assigned)
			=> (ProcessTask)BMTestHelper.CreateTask(workflow, staffCode: staffCode ?? ReviewerStaffCode, taskType: "CBC", taskStatus: status, description: "Code Review", sequence: sequence);

		public ProcessTask CreateCheckinTask(IProcessHeader workflow, int sequence = 3)
		{
			var task = (ProcessTask)BMTestHelper.CreateTask(workflow, staffCode: "DE", taskType: "CHK", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "CHK", sequence: sequence);
			task.P9_NotesAsString = "http://example.com/pullrequest/123";
			return task;
		}

		public const string AuthorStaffCode = "DE";
		public const string ReviewerStaffCode = "AM";
	}
}
