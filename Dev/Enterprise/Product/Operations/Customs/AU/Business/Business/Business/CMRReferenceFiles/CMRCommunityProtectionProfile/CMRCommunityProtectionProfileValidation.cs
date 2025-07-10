//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRCommunityProtectionProfileValidation
//
//    This class should be used for overriding validation in AutoCMRCommunityProtectionProfileValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionProfileValidation : AutoCMRCommunityProtectionProfileValidation
	{
		public CMRCommunityProtectionProfileValidation(AutoCMRCommunityProtectionProfile parent)
			: base(parent)
		{
		}
	}
}
