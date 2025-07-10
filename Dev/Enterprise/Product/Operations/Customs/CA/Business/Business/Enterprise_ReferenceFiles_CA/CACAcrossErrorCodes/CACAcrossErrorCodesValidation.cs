//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACAcrossErrorCodesValidation
//
//    This class should be used for overriding validation in AutoCACAcrossErrorCodesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACAcrossErrorCodesValidation : AutoCACAcrossErrorCodesValidation
	{
		public CACAcrossErrorCodesValidation(AutoCACAcrossErrorCodes parent) : base(parent)
		{
		}
	}
}
