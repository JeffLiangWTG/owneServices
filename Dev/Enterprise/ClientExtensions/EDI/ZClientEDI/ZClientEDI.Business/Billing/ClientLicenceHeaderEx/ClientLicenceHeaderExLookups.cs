//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientLicenceHeaderExLookups
//
//    This class should be used for overriding collections in AutoClientLicenceHeaderExLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceHeaderExLookups : AutoClientLicenceHeaderExLookups
	{
		public ClientLicenceHeaderExLookups(AutoClientLicenceHeaderEx parent) : base(parent)
		{
		}
	}
}

