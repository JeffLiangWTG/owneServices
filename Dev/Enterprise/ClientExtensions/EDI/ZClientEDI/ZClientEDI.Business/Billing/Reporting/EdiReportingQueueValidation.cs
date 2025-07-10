//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiReportingQueueValidation
//
//    This class should be used for overriding validation in AutoEdiReportingQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiReportingQueueValidation : AutoEdiReportingQueueValidation
	{
		public EdiReportingQueueValidation(AutoEdiReportingQueue parent) : base(parent)
		{
		}
	}
}

