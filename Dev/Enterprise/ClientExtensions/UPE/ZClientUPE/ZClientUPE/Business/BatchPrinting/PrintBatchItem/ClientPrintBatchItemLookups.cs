//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientPrintBatchItemLookups
//
//    This class should be used for overriding collections in AutoClientPrintBatchItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.UPE.Business
{
	public class ClientPrintBatchItemLookups : AutoClientPrintBatchItemLookups
	{
		public ClientPrintBatchItemLookups(AutoClientPrintBatchItem parent)
			: base(parent)
		{
		}
	}
}
