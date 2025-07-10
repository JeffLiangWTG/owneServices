using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleValidation : AutoProcessTemplateReleaseGroupRuleValidation
	{
		public ProcessTemplateReleaseGroupRuleValidation(AutoProcessTemplateReleaseGroupRule parent)
			: base(parent)
		{
		}

		new ProcessTemplateReleaseGroupRule Parent => (ProcessTemplateReleaseGroupRule)base.Parent;

		protected override void CheckPTR_Sequence()
		{
			base.CheckPTR_Sequence();

			if (Parent.Template != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PTR_SequenceInfo, Parent.Template.ReleaseGroupRules.Cast<BusinessObject>(), checkEmptyValues: true);
			}
		}

		protected override void CheckPTR_AreAllWorkflowCategoriesApplicable()
		{
			base.CheckPTR_AreAllWorkflowCategoriesApplicable();

			if (!Parent.PTR_AreAllWorkflowCategoriesApplicable && !Parent.Categories.Any())
			{
				Parent.PTR_AreAllWorkflowCategoriesApplicableInfo.AddError(Res.GetString("72bb768a-5cf2-447a-bf80-55f45c441f8b", "Please enter at least one applicable workflow category."));
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != ProcessTemplateReleaseGroupRuleSchema.Constants.PTR_P0_Template;
		}
	}
}
