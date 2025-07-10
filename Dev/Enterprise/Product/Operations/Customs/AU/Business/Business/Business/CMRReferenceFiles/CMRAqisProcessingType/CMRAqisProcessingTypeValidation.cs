//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisProcessingTypeValidation
//
//    This class should be used for overriding validation in AutoCMRAqisProcessingTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisProcessingTypeValidation : AutoCMRAqisProcessingTypeValidation
	{
		public CMRAqisProcessingTypeValidation(AutoCMRAqisProcessingType parent)
			: base(parent)
		{
		}
	}
}
