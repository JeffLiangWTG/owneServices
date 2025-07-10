//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRLodgementQuestionLookups
//
//    This class should be used for overriding collections in AutoCMRLodgementQuestionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRLodgementQuestionLookups : AutoCMRLodgementQuestionLookups
	{
		public CMRLodgementQuestionLookups(AutoCMRLodgementQuestion parent)
			: base(parent)
		{
		}
	}
}
