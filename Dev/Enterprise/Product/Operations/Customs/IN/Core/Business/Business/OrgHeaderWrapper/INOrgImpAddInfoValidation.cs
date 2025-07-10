//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoINOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoINOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.IN.Business
{
	public class INOrgImpAddInfoValidation : AutoINOrgImpAddInfoValidation
	{
		public INOrgImpAddInfoValidation(AutoINOrgImpAddInfo parent) : base(parent)
		{
		}
	}
}
