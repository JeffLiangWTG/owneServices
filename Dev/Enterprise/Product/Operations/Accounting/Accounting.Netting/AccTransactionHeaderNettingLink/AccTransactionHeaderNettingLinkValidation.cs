//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderNettingLinkValidation
//
//    This class should be used for overriding validation in AutoAccTransactionHeaderNettingLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class AccTransactionHeaderNettingLinkValidation : AutoAccTransactionHeaderNettingLinkValidation
	{
		public AccTransactionHeaderNettingLinkValidation(AutoAccTransactionHeaderNettingLink parent) : base(parent)
		{
		}
	}
}
