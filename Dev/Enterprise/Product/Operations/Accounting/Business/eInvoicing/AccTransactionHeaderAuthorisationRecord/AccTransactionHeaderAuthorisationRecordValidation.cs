//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionHeaderAuthorisationRecordValidation
//
//    This class should be used for overriding validation in AutoAccTransactionHeaderAuthorisationRecordValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccTransactionHeaderAuthorisationRecordValidation : AutoAccTransactionHeaderAuthorisationRecordValidation
	{
		public AccTransactionHeaderAuthorisationRecordValidation(AutoAccTransactionHeaderAuthorisationRecord parent) : base(parent)
		{
		}
	}
}
