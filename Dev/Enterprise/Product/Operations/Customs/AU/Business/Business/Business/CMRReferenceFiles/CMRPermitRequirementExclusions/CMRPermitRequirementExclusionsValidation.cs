//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPermitRequirementExclusionsValidation
//
//    This class should be used for overriding validation in AutoCMRPermitRequirementExclusionsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPermitRequirementExclusionsValidation : AutoCMRPermitRequirementExclusionsValidation
	{
		public CMRPermitRequirementExclusionsValidation(AutoCMRPermitRequirementExclusions parent)
			: base(parent)
		{
		}
	}
}
