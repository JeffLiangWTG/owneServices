//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoProcessTemplateReleaseGateRuleValidation
//
//    This class should be used for overriding validation in AutoProcessTemplateReleaseGateRuleValidation
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRuleValidation : AutoProcessTemplateReleaseGateRuleValidation
	{
		public ProcessTemplateReleaseGateRuleValidation(AutoProcessTemplateReleaseGateRule parent) : base(parent)
		{
		}
	}
}

