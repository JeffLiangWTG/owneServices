//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIInterchangeLookups
//
//    This class should be used for overriding collections in AutoEDIInterchangeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business
{
	public class EDIInterchangeLookups : AutoEDIInterchangeLookups
	{
		public EDIInterchangeLookups(AutoEDIInterchange parent) : base(parent)
		{
		}

		public GlbStaffCollection GlbStaffs => Factory.GetCachedValue(nameof(EDIInterchangeLookups), () => new GlbStaffCollection(Factory));
	}
}
