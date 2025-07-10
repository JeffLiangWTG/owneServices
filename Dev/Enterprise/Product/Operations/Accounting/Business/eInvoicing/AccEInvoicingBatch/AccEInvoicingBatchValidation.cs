//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccEInvoicingBatchValidation
//
//    This class should be used for overriding validation in AutoAccEInvoicingBatchValidation
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingBatchValidation : AutoAccEInvoicingBatchValidation
	{
		public AccEInvoicingBatchValidation(AutoAccEInvoicingBatch parent)
			: base(parent)
		{
		}
	}
}
