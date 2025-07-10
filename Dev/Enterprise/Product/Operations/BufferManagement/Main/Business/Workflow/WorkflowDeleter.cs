using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Workflow
{
	class WorkflowDeleter : DeleteChecker
	{
		public override void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			base.BeforeSuccessfulDelete(businessObject);

			var jobHeader = GetJobHeader(businessObject);
			if (jobHeader != null)
			{
				jobHeader.Delete();
			}
		}

		public override DeleteDetails DeleteDetails(BusinessObject businessObject)
		{
			var jobHeader = GetJobHeader(businessObject);
			if (jobHeader != null && jobHeader.DirectlyApprovedShape != null)
			{
				return new DeleteDetails.Disallow(Res.GetString("b98d502f-aed7-4f52-a1e6-90914ec1d555", "This job is part of an approved project plan and cannot be deleted."));
			}

			return new DeleteDetails.Allow();
		}

		static ProcessJobHeader GetJobHeader(BusinessObject businessObject)
		{
			var workflowProvider = businessObject as IWorkflowProvider;
			return workflowProvider != null ? ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, businessObject.Factory) : null;
		}
	}
}
