//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisProducerLookups
//
//    This class should be used for overriding collections in AutoCMRAqisProducerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisProducerLookups : AutoCMRAqisProducerLookups
	{
		public CMRAqisProducerLookups(AutoCMRAqisProducer parent)
			: base(parent)
		{
		}
	}
}
