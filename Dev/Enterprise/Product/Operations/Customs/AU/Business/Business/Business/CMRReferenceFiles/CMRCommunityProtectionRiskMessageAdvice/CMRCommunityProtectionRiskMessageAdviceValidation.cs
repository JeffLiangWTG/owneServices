//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCommunityProtectionRiskMessageAdviceValidation
//
//    This class should be used for overriding validation in AutoCMRCommunityProtectionRiskMessageAdviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionRiskMessageAdviceValidation : AutoCMRCommunityProtectionRiskMessageAdviceValidation
	{
		public CMRCommunityProtectionRiskMessageAdviceValidation(AutoCMRCommunityProtectionRiskMessageAdvice parent)
			: base(parent)
		{
		}
	}
}
