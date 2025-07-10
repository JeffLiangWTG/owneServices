//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDocumentDeliveryLookups
//
//    This class should be used for overriding collections in AutoStmDocumentDeliveryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Business
{
	public class StmDocumentDeliveryLookups : AutoStmDocumentDeliveryLookups
	{
		public StmDocumentDeliveryLookups(AutoStmDocumentDelivery parent) : base(parent)
		{
		}
	}
}
