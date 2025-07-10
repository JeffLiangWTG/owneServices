//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPersonMergeQueueLookups
//
//    This class should be used for overriding collections in AutoEdiPersonMergeQueueLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiPersonMergeQueueLookups : AutoEdiPersonMergeQueueLookups
	{
		public EdiPersonMergeQueueLookups(AutoEdiPersonMergeQueue parent) : base(parent)
		{
		}
	}
}