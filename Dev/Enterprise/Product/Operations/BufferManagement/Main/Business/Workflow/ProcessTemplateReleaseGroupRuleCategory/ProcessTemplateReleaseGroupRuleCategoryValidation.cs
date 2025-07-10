//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTemplateReleaseGroupRuleCategoryValidation
//
//    This class should be used for overriding validation in AutoProcessTemplateReleaseGroupRuleCategoryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.BufferManagement.Business
{
	using CargoWise.EntityFramework;

	public class ProcessTemplateReleaseGroupRuleCategoryValidation : AutoProcessTemplateReleaseGroupRuleCategoryValidation
	{
		public ProcessTemplateReleaseGroupRuleCategoryValidation(AutoProcessTemplateReleaseGroupRuleCategory parent)
			: base(parent)
		{
		}

		new ProcessTemplateReleaseGroupRuleCategory Parent
		{
			get { return (ProcessTemplateReleaseGroupRuleCategory)base.Parent; }
		}

		protected override void CheckPTC_Category()
		{
			base.CheckPTC_Category();
			MandatoryValidation.CheckEntered(Parent.PTC_CategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PTC_CategoryInfo);

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PTC_CategoryInfo, Parent.Rule.Categories);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePTC_Category();
		}
	}
}
