//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTemplateReleaseGroupRuleCategoryLookups
//
//    This class should be used for overriding collections in AutoProcessTemplateReleaseGroupRuleCategoryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleCategoryLookups : AutoProcessTemplateReleaseGroupRuleCategoryLookups
	{
		public ProcessTemplateReleaseGroupRuleCategoryLookups(AutoProcessTemplateReleaseGroupRuleCategory parent)
			: base(parent)
		{
		}

		new ProcessTemplateReleaseGroupRuleCategory Parent
		{
			get { return (ProcessTemplateReleaseGroupRuleCategory)base.Parent; }
		}

		public CodeDescriptionPairList WorkflowCategories
		{
			get
			{
				return Factory.GetCachedValue("ProcessTemplateReleaseGroupRuleCategoryLookups.WorkflowCategories", () =>
				{
					var collection = BMSRegistry.Instance.WorkflowCategories.Value.GetCategoriesFromWorkflowCode(Parent?.Rule?.Template?.P0_ProcessType ?? string.Empty);
					var categories = (collection != null) ? collection.GetCodeDescriptionPairList() : new CodeDescriptionPairList();

					categories.AddPair(BMConstants.JobLevelWorkflowCategoryCode, ResString.GetMultilingualString("75dac886-ba4c-426e-8ba9-e952f6e12487", "Job-level workflow"));

					return categories;
				});
			}
		}
	}
}
