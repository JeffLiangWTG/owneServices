//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTemplateReleaseGateRuleMappingValidation
//
//    This class should be used for overriding validation in AutoProcessTemplateReleaseGateRuleMappingValidation
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRuleMappingValidation : AutoProcessTemplateReleaseGateRuleMappingValidation
	{
		public ProcessTemplateReleaseGateRuleMappingValidation(AutoProcessTemplateReleaseGateRuleMapping parent) : base(parent)
		{
		}
	}
}

