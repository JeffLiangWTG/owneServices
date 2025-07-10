//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPrintBatchValidation
//
//    This class should be used for overriding validation in AutoClientPrintBatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientPrintBatchValidation : AutoClientPrintBatchValidation
	{
		public ClientPrintBatchValidation(AutoClientPrintBatch parent)
			: base(parent)
		{
		}
	}
}
