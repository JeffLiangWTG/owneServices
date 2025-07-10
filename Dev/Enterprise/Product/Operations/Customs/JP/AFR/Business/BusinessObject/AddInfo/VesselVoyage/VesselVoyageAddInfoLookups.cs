//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVesselVoyageAddInfoLookups
//
//    This class should be used for overriding collections in AutoVesselVoyageAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.JP.AFR.Business
{
	public class VesselVoyageAddInfoLookups : AutoVesselVoyageAddInfoLookups
	{
		public VesselVoyageAddInfoLookups(AutoVesselVoyageAddInfo parent) : base(parent)
		{
		}
	}
}
