//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisEntityValidation
//
//    This class should be used for overriding validation in AutoCMRAqisEntityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisEntityValidation : AutoCMRAqisEntityValidation
	{
		public CMRAqisEntityValidation(AutoCMRAqisEntity parent)
			: base(parent)
		{
		}
	}
}
