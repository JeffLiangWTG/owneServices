//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRRefundReasonValidation
//
//    This class should be used for overriding validation in AutoCMRRefundReasonValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRRefundReasonValidation : AutoCMRRefundReasonValidation
	{
		public CMRRefundReasonValidation(AutoCMRRefundReason parent)
			: base(parent)
		{
		}
	}
}
