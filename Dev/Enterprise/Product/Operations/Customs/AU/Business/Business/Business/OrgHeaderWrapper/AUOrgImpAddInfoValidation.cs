//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAUOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoAUOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgImpAddInfoValidation : AutoAUOrgImpAddInfoValidation
	{
		public AUOrgImpAddInfoValidation(AutoAUOrgImpAddInfo parent) : base(parent)
		{
		}
	}
}
