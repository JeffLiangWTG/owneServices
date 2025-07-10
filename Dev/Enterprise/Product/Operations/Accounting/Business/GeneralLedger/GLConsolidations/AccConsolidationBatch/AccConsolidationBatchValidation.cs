//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccConsolidationBatchValidation
//
//    This class should be used for overriding validation in AutoAccConsolidationBatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationBatchValidation : AutoAccConsolidationBatchValidation
	{
		public AccConsolidationBatchValidation(AutoAccConsolidationBatch parent) : base(parent)
		{
		}
	}
}