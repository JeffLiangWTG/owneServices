//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBLLFunctionAddInfoValidation
//
//    This class should be used for overriding validation in AutoBLLFunctionAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLFunctionAddInfoValidation : AutoBLLFunctionAddInfoValidation
	{
		public BLLFunctionAddInfoValidation(AutoBLLFunctionAddInfo parent) : base(parent)
		{
		}
	}
}
