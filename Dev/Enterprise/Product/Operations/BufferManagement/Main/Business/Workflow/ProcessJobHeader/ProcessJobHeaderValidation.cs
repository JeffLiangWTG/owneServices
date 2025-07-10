
namespace Enterprise.BufferManagement.Business
{
	public class ProcessJobHeaderValidation : ProcessHeaderValidation
	{
		public ProcessJobHeaderValidation(ProcessJobHeader parent)
			: base(parent)
		{
		}

		protected override void CheckFH_GG_ReleaseGroup()
		{
			if (!Parent.FH_GG_ReleaseGroup.IsEmpty && Parent.Template != null && Parent.Template.P0_IsPartialTemplate)
			{
				Parent.FH_GG_ReleaseGroupInfo.AddError(Res.GetString("39ecce96-cadb-42d2-8c33-68874fbfc31b", "A Release Group cannot be specified on the Job-Level Workflow of a Partial Template."));
			}
		}

		protected override void CheckFH_FC_CurrentComponent()
		{
			// Never validate current component - it isn't specified at job level
		}

		protected override void CheckFH_CategoryCore()
		{
			if (Parent.FH_Category != BMConstants.JobLevelWorkflowCategoryCode)
			{
				Parent.FH_CategoryInfo.AddError(Res.GetString("b5cbdf0f-8850-4588-921d-c701ec419fce", "The category for job-level workflows must be of type {0}", BMConstants.JobLevelWorkflowCategoryCode));
			}
		}
	}
}
