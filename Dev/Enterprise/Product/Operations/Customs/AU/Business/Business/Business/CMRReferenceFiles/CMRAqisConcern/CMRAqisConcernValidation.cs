//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisConcernValidation
//
//    This class should be used for overriding validation in AutoCMRAqisConcernValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisConcernValidation : AutoCMRAqisConcernValidation
	{
		public CMRAqisConcernValidation(AutoCMRAqisConcern parent)
			: base(parent)
		{
		}
	}
}
