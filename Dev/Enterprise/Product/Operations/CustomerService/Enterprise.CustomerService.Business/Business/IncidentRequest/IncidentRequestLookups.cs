//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentRequestLookups
//
//    This class should be used for overriding collections in AutoIncidentRequestLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CustomerService.Business
{
	public class IncidentRequestLookups : AutoIncidentRequestLookups
	{
		public IncidentRequestLookups(AutoIncidentRequest parent) : base(parent)
		{
		}
	}
}
