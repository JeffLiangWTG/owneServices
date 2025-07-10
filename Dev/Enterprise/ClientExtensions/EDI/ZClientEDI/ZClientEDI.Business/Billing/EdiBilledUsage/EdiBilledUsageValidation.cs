//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiBilledUsageValidation
//
//    This class should be used for overriding validation in AutoEdiBilledUsageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBilledUsageValidation : AutoEdiBilledUsageValidation
	{
		public EdiBilledUsageValidation(AutoEdiBilledUsage parent) : base(parent)
		{
		}
	}
}

