//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientTelRimRegistrationLookups
//
//    This class should be used for overriding collections in AutoClientTelRimRegistrationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientTelRimRegistrationLookups : AutoClientTelRimRegistrationLookups
	{
		public ClientTelRimRegistrationLookups(AutoClientTelRimRegistration parent) : base(parent)
		{
		}
	}
}
