//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEnRouteEventIncidentLookups
//
//    This class should be used for overriding collections in AutoEnRouteEventIncidentLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class EnRouteEventIncidentLookups : AutoEnRouteEventIncidentLookups
	{
		public EnRouteEventIncidentLookups(AutoEnRouteEventIncident parent) : base(parent)
		{
		}
	}
}
