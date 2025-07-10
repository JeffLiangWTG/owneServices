//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityTenantLookups
//
//    This class should be used for overriding collections in AutoEdiIdentityTenantLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IdentityTenant.Business
{
	public class EdiIdentityTenantLookups : AutoEdiIdentityTenantLookups
	{
		public EdiIdentityTenantLookups(AutoEdiIdentityTenant parent) : base(parent)
		{
		}
	}
}
