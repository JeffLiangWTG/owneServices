//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPrintBatchItemValidation
//
//    This class should be used for overriding validation in AutoClientPrintBatchItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientPrintBatchItemValidation : AutoClientPrintBatchItemValidation
	{
		public ClientPrintBatchItemValidation(AutoClientPrintBatchItem parent)
			: base(parent)
		{
		}
	}
}
