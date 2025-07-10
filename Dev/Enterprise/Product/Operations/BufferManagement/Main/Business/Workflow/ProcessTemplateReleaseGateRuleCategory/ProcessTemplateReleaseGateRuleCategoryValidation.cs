//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTemplateReleaseGateRuleCategoryValidation
//
//    This class should be used for overriding validation in AutoProcessTemplateReleaseGateRuleCategoryValidation
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRuleCategoryValidation : AutoProcessTemplateReleaseGateRuleCategoryValidation
	{
		public ProcessTemplateReleaseGateRuleCategoryValidation(AutoProcessTemplateReleaseGateRuleCategory parent) : base(parent)
		{
		}
	}
}

