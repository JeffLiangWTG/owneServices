//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargePostingQueueValidation
//
//    This class should be used for overriding validation in AutoJobChargePostingQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobChargePostingQueueValidation : AutoJobChargePostingQueueValidation
	{
		public JobChargePostingQueueValidation(AutoJobChargePostingQueue parent) : base(parent)
		{
		}
	}
}