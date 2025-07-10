//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBMReleaseSequenceLookups
//
//    This class should be used for overriding collections in AutoBMReleaseSequenceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.BufferManagement.Business
{
	public class BMReleaseSequenceLookups : AutoBMReleaseSequenceLookups
	{
		public BMReleaseSequenceLookups(AutoBMReleaseSequence parent) : base(parent)
		{
		}
	}
}
