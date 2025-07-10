//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPermitRequirementValidation
//
//    This class should be used for overriding validation in AutoCMRPermitRequirementValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPermitRequirementValidation : AutoCMRPermitRequirementValidation
	{
		public CMRPermitRequirementValidation(AutoCMRPermitRequirement parent)
			: base(parent)
		{
		}
	}
}
