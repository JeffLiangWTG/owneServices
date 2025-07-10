//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoeRouterEdiEnterpriseCommunicationLookups
//
//    This class should be used for overriding collections in AutoeRouterEdiEnterpriseCommunicationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.eRouter.Business
{
	public class eRouterEdiEnterpriseCommunicationLookups : AutoeRouterEdiEnterpriseCommunicationLookups
	{
		public eRouterEdiEnterpriseCommunicationLookups(AutoeRouterEdiEnterpriseCommunication parent) : base(parent)
		{
		}
	}
}

