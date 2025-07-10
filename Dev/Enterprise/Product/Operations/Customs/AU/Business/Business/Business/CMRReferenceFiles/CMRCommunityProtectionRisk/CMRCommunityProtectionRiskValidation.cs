//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCommunityProtectionRiskValidation
//
//    This class should be used for overriding validation in AutoCMRCommunityProtectionRiskValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionRiskValidation : AutoCMRCommunityProtectionRiskValidation
	{
		public CMRCommunityProtectionRiskValidation(AutoCMRCommunityProtectionRisk parent)
			: base(parent)
		{
		}
	}
}
