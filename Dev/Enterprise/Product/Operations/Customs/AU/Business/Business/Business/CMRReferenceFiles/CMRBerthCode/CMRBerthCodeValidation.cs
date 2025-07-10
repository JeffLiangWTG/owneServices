//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRBerthCodeValidation
//
//    This class should be used for overriding validation in AutoCMRBerthCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRBerthCodeValidation : AutoCMRBerthCodeValidation
	{
		public CMRBerthCodeValidation(AutoCMRBerthCode parent)
			: base(parent)
		{
		}
	}
}
